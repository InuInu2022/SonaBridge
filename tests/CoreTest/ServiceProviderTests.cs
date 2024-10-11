using FluentAssertions;
using SonaBridge;
using SonaBridge.Core.Common;
using Xunit.Abstractions;

namespace CoreTest;

public class ServiceProviderTests : IClassFixture<ServiceFixture>
{
	private readonly ServiceFixture _fixture;
	private readonly ITalkAutoService _service;
	private readonly ITestOutputHelper _output;

	public ServiceProviderTests(ServiceFixture fixture, ITestOutputHelper output)
	{
		_fixture = fixture;
		_service = _fixture.Service;
		_output = output;
	}

	[Fact]
	public async void CallTest()
	{
		var _ = await _service.SpeakAsync("サービスから呼び出しています。");
	}

	[Fact]
	public async void GetAvailableCastsAsync()
	{
		var casts = await _service.GetAvailableCastsAsync();
		casts.Should().NotBeEmpty();
		_output.WriteLine($"casts: {string.Join(",\n", casts)}");
	}

	[Fact]
	public async void SetCastAsync()
	{
		await _service.SetCastAsync("Sato Sasara");
		await _service.SetCastAsync("Takahashi");
		await _service.SetCastAsync("Tamaki");
	}

	[Fact]
	public async void OutputWaveToFileAsync()
	{
		var path = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
		var result = await _service
			.OutputWaveToFileAsync("こんにちは", path);
		result.Should().BeTrue();
	}
}
