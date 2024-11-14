using System.Diagnostics;

using FluentAssertions;
using SonaBridge;
using SonaBridge.Core.Common;
using Xunit.Abstractions;

namespace CoreTest;

public class ServiceProviderTests : IClassFixture<ServiceFixture>, IAsyncLifetime
{
	private readonly ServiceFixture _fixture;
	private readonly ITalkAutoService _service;
	private readonly ITestOutputHelper _output;
	private readonly Stopwatch _sw;

	public ServiceProviderTests(ServiceFixture fixture, ITestOutputHelper output)
	{
		_fixture = fixture;
		_service = _fixture.Service;
		_output = output;
		_sw = new Stopwatch();
	}

	public Task InitializeAsync()
	{
		return Task.CompletedTask;
	}

	public Task DisposeAsync()
	{
		_sw.Stop();
		_output.WriteLine($"------ {_sw.ElapsedMilliseconds} ms. ------");
		_sw.Reset();
		return Task.CompletedTask;
	}

	[Fact]
	public async Task CallTest()
	{
		_sw.Restart();

		var _ = await _service.SpeakAsync("サービスから呼び出しています。");
	}

	[Fact]
	public async Task GetAvailableCastsAsync()
	{
		_sw.Restart();

		var casts = await _service.GetAvailableCastsAsync();
		casts.Should().NotBeEmpty();
		_output.WriteLine($"casts: {string.Join(",\n", casts)}");
	}

	[Fact]
	public async Task SetCastAsync()
	{
		_sw.Restart();
		long a;

		await _service.SetCastAsync("Sato Sasara");
		_output.WriteLine($"- {_sw.ElapsedMilliseconds} ms: 1");
		a = _sw.ElapsedMilliseconds;
		await _service.SetCastAsync("Takahashi");
		_output.WriteLine($"- {_sw.ElapsedMilliseconds - a} ms: 2");
		a = _sw.ElapsedMilliseconds;
		await _service.SetCastAsync("Tamaki");
		_output.WriteLine($"- {_sw.ElapsedMilliseconds - a} ms: 3");
	}
	[Fact]
	public async Task SetCastSingleAsync()
	{
		await _service.SetCastAsync("Tanaka San");
		_sw.Restart();
		await _service.SetCastAsync("Sato Sasara");
	}

	[Fact]
	public async Task OutputWaveToFileAsync()
	{
		_sw.Restart();

		var path = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
		var result = await _service
			.OutputWaveToFileAsync("こんにちは", path);
		result.Should().BeTrue();
	}

	[Theory]
	[InlineData("Takahashi")]
	[InlineData("Sato Sasara")]
	[InlineData("Tanaka San")]
	public async Task Presets(string voice)
	{
		_sw.Restart();
		var result = await _service.GetPresetsAsync(voice);
		_sw.Stop();
		_output.WriteLine($"{voice}\n - { string.Join(',', result) }");
		_output.WriteLine($"{_sw.ElapsedMilliseconds} ms");
		_sw.Restart();
		await _service.SetPresetsAsync(voice, result[0]);
		_sw.Stop();
		_output.WriteLine($"{_sw.ElapsedMilliseconds} ms");
	}


}
