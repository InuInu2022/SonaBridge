using System.Diagnostics;

using FlaUI.Core.AutomationElements;

using FluentAssertions;

using SonaBridge.Core.Common;
using SonaBridge.Core.Win;

using Xunit.Abstractions;

namespace CoreTest;

public class WinAutoServiceTests : IClassFixture<ServiceFixture>, IDisposable, IAsyncLifetime
{
	private readonly ServiceFixture _fixture;
	private readonly WinTalkAutoService _service;
	private readonly ITestOutputHelper _output;
	private bool _disposedValue;

	private readonly Stopwatch _sw;

	public WinAutoServiceTests(
		ServiceFixture fixture,
		ITestOutputHelper output
	)
	{
		_fixture = fixture;
		_service = new WinTalkAutoService();
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
	public async Task GetAppAsync()
	{
		_sw.Start();
		await _service.GetAppWindowAsync();
	}

	[Fact]
	public async Task PlayAsync()
	{
		//var service = new WinTalkAutoService();
		await _service.GetAppWindowAsync();
		_sw.Start();
		await WinTalkAutoService.PlayUtterance();
	}

	[Theory]
	[InlineData(3)]
	public async Task GetIndexAsync(int index)
	{
		//var service = new WinTalkAutoService();
		await _service.GetAppWindowAsync();
		_sw.Start();
		var result = WinTalkAutoService.GetUtterancePosition();

		Assert.Equal(index, result);
	}
	[Theory]
	[InlineData(6)]
	public async Task GetLenIndex(int index)
	{
		//var service = new WinTalkAutoService();
		await _service.GetAppWindowAsync();
		var result = WinTalkAutoService.GetLengthPosition();

		Assert.Equal(index, result);
	}

	[Theory]
	[InlineData("テスト")]
	public async Task SetText(string text)
	{
		//var _service.= new WinTalkAutoService();
		await _service.GetAppWindowAsync();
		_sw.Start();
		await WinTalkAutoService.SetUtterance(text);
	}

	[Theory]
	[InlineData("こんにちは")]
	[InlineData("ありがとうございます。")]
	[InlineData("本日は晴天なり。")]
	[InlineData("あめんぼ甘いな、あいうえお。")]
	public async Task PlayTextAsync(string text)
	{
		//var _service.= new WinTalkAutoService();
		await _service.GetAppWindowAsync();
		await WinTalkAutoService.SetUtterance(text);
		await WinTalkAutoService.PlayUtterance();

		await Task.Delay(1000);
	}

	[Fact]
	public async void GetVoices()
	{
		//var _service.= new WinTalkAutoService();
		await _service.GetAppWindowAsync();
		var items = await WinTalkAutoService.GetVoiceNames();

		items.ToList().ForEach(v => _output.WriteLine($"voice: {v}") );

		items.Should().NotBeEmpty();
	}

	[Theory]
	[InlineData("Suzuki Tsudumi", true)]
	[InlineData("Sato Sasara", true)]
	[InlineData("Tamaki", true)]
	[InlineData("Takahashi", true)]
	//[InlineData("No Name Voice", false)]
	public async void SetVoice(string name, bool expected)
	{
		//var _service.= new WinTalkAutoService();
		await _service.GetAppWindowAsync();
		var result = await WinTalkAutoService.SetVoiceAsync(name);

		result.Should().Be(expected);
		await WinTalkAutoService.PlayUtterance();
	}

	[Fact]
	public async void SetCastAsyncThrowsInvalidOperationExceptionWhenCastNameNotFound()
	{
		//var _service.= new WinTalkAutoService();
		await _service.GetAppWindowAsync();

		var ex = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
		{
			await _service.SetCastAsync("NonExistentCastName")
			.ConfigureAwait(true);
		});

		Assert.Equal("FlaUI operation error!", ex.Message);
	}

	[Theory]
	[InlineData("Koharu Rikka","やれないとコホがあるこはる。")]
	[InlineData("Sato Sasara","なかなかやりたいと重く事五とやるぜきの喉のバランスボールが取れないと言うか。")]
	[InlineData("Takahashi", "感というか、私は優先順位をつけるというのか本王に苦手で、")]
	[InlineData("Tanaka San", "管理に湯として、大した事が出来た無いのが申し訳ないよねらとぉってるのですが。")]
	[InlineData("Tamaki", "イウしキーを呑んとるのですが。")]
	[InlineData("Suzuki Tsudumi", "そうやなす。")]
	public async void SetVoiceAndTextAsync(
		string castName,
		string text
	)
	{
		//var _service.= new WinTalkAutoService();
		await _service.SetCastAsync(castName);
		await _service.SpeakAsync(text);
		//await _service.GetAppWindowAsync();
		//var result = await _service.SetVoiceAsync(castName);
		//await _service.SetUtterance(text);
		//await _service.PlayUtterance();
	}

	[Theory]
	[InlineData("Koharu Rikka","やれないとコホがあるこはる。")]
	[InlineData("Sato Sasara","なかなかやりたいと重く事五とやるぜきの喉のバランスボールが取れないと言うか。")]
	[InlineData("Takahashi", "感というか、私は優先順位をつけるというのか本王に苦手で、")]
	[InlineData("Tanaka San", "管理に湯として、大した事が出来た無いのが申し訳ないよねらとぉってるのですが。")]
	[InlineData("Tamaki", "イウしキーを呑んとるのですが。")]
	[InlineData("Suzuki Tsudumi", "そうやなす。")]
	public async void OutputWavAsync(
		string castName,
		string text
	)
	{
		//var _service.= new WinTalkAutoService();

		var sw = System.Diagnostics.Stopwatch.StartNew();
		await _service.SetCastAsync(castName);
		var path = Path.Combine(
			Path.GetTempPath(),
			$"{castName}_{text}"
		);
		sw.Stop();
		_output.WriteLine($"set time: {sw.Elapsed.TotalSeconds} sec., {castName},{text}");
		sw.Restart();
		var result = await _service.OutputWaveToFileAsync(text, path);
		sw.Stop();

		_output.WriteLine($"output time: {sw.Elapsed.TotalSeconds} sec., {castName},{text}");

		result.Should().BeTrue();
		Path.Exists($"{path}").Should().BeTrue();
	}

	[Theory]
	[InlineData("tmp")]
	[InlineData("abc")]
	public async void FixExtension(string ext)
	{
		//var _service.= new WinTalkAutoService();
		var path = Path.Combine(
			Path.GetTempPath(),
			$"{Path.GetRandomFileName()}.{ext}"
		);
		await using var fs = File.Create($"{path}.wav");
		fs.Close();
		var sw = System.Diagnostics.Stopwatch.StartNew();
		await WinTalkAutoService.FixExtensionAsync(path);
		sw.Stop();
		_output.WriteLine($"fix ext. time: {sw.Elapsed.TotalSeconds} sec.");
		Path.Exists(path).Should().BeTrue();
	}

	[Fact]
	public async void OpenGlobalParamsPanel()
	{
		//var _service.= new WinTalkAutoService();
		//var sw = System.Diagnostics.Stopwatch.StartNew();
		_sw.Start();

		await _service.OpenGlobalParamsPanelAsync();

		_sw.Stop();
		_output.WriteLine($"1 toggle gparam. time: {_sw.Elapsed.TotalMilliseconds} ms.");
		_sw.Restart();

		await _service.OpenGlobalParamsPanelAsync();

		_sw.Stop();
		_output.WriteLine($"2 toggle gparam. time: {_sw.Elapsed.TotalMilliseconds} sec.");
	}

	[Fact]
	public async void GetGlobalParamSliders()
	{
		//var _service.= new WinTalkAutoService();
		//var sw = System.Diagnostics.Stopwatch.StartNew();
		_sw.Start();

		var sliders = await _service.GetGlobalParamSliders();

		_sw.Stop();
		foreach(var s in sliders)
		{
			var s2 = s.AsSlider();
			_output.WriteLine($"Slider: {s2.Value}, max:{s2.Maximum} min:{s2.Minimum}");
		}
		_output.WriteLine($"get sliders. time: {_sw.Elapsed.TotalMilliseconds} ms.");

		var values = await _service.GetCurrentGlobalParamAsync();
		foreach (var item in values)
		{
			_output.WriteLine($"Value: {item.Key}, {item.Value:F2} ");
		}
	}

	[Theory]
	[InlineData("Hoge", 1.0, false, false)]
	[InlineData("Speed", 1.0)]
	[InlineData("Speed", 0.2)]
	[InlineData("Speed", 5.0)]
	[InlineData("Speed", 100.0, true, false)]
	[InlineData("Speed", 0.0, true, false)]
	[InlineData("Volume", 1.0)]
	[InlineData("Volume", 8.0)]
	[InlineData("Volume", -8.0)]
	[InlineData("Pitch", 1.0)]
	[InlineData("Pitch", 600.0)]
	[InlineData("Pitch", -600.0)]
	[InlineData("Alpha", 1.0)]
	[InlineData("Into.", 1.0)]
	[InlineData("Hus.", 1.0)]
	public async void SetGlobalParamSingle(
		string key,
		double value,
		bool hasKey = true,
		bool expect = true)
	{
		//var _service.= new WinTalkAutoService();
		//var sw = System.Diagnostics.Stopwatch.StartNew();
		_sw.Start();

		await _service.SetCurrentGlobalParamsAsync(
			new Dictionary<string,double>(StringComparer.Ordinal){
				{key, value},
			});
		_sw.Stop();
		var values = await _service.GetCurrentGlobalParamAsync();

		_output.WriteLine($"SetGlobalParamSingle[{key}] time: {_sw.Elapsed.TotalMilliseconds} ms.");
		values.TryGetValue(key, out var found)
			.Should().Be(hasKey);
		var isSame = Math.Abs(found - value) < 0.01;
		isSame.Should().Be(expect);
	}

	[Theory]
	[InlineData("Tanaka San")]
	[InlineData("Takahashi")]
	public async void GetStyleSliders(string voice)
	{
		//var _service.= new WinTalkAutoService();
		//var sw = System.Diagnostics.Stopwatch.StartNew();
		await _service.SetCastAsync(voice);
		_sw.Start();

		var sliders = await _service.GetStyleSlidersAsync(voice);

		_sw.Stop();
		foreach(var s in sliders)
		{
			var s2 = s.AsSlider();
			_output.WriteLine($"Slider: {s2.Value}, max:{s2.Maximum} min:{s2.Minimum}");
		}
		_output.WriteLine($"get sliders. time: {_sw.Elapsed.TotalMilliseconds} ms.");

		_sw.Restart();
		var names = await _service.GetCurrentStyleNamesAsync(voice);
		_sw.Stop();
		foreach (var item in names)
		{
			_output.WriteLine($"Style name: {item} ");
		}
		_output.WriteLine($"get style names time: {_sw.Elapsed.TotalMilliseconds} ms.");

		_sw.Restart();
		var values = await _service.GetCurrentStylesAsync(voice);
		_sw.Stop();
		foreach (var item in values)
		{
			_output.WriteLine($"Value: {item.Key}, {item.Value:F2} ");
		}
		_output.WriteLine($"get style time: {_sw.Elapsed.TotalMilliseconds} ms.");

	}

	[Theory]
	[InlineData("Tanaka San","Hoge", 1.0, false, false)]
	[InlineData("Tanaka San","Normal", 1.0)]
	[InlineData("Tanaka San","Normal", 0.0)]
	[InlineData("Tanaka San","Normal", 0.5)]
	[InlineData("Futaba Minato","Child", 0.23)]
	[InlineData("Futaba Minato","Shy", 0.78)]
	public async void SetStyleSingle(
		string voice,
		string key,
		double value,
		bool hasKey = true,
		bool expect = true)
	{
		await _service.SetCastAsync(voice);
		var sw = System.Diagnostics.Stopwatch.StartNew();

		await _service.SetCurrentStylesAsync(
			voice,
			new Dictionary<string,double>(StringComparer.Ordinal){
				{key, value},
			});

		var values = await _service.GetCurrentStylesAsync(voice);
		sw.Stop();
		_output.WriteLine($"SetStyleSingle time: {sw.Elapsed.TotalSeconds} sec.");
		values.TryGetValue(key, out var found)
			.Should().Be(hasKey);
		var isSame = Math.Abs(found - value) < 0.01;
		isSame.Should().Be(expect);
	}

	protected virtual void Dispose(bool disposing)
	{
		if (!_disposedValue)
		{
			if (disposing)
			{
				_service.Dispose();
			}
			_disposedValue = true;
		}
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}


}
