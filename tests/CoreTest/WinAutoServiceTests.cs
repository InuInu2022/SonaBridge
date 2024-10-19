using FlaUI.Core.AutomationElements;

using FluentAssertions;

using SonaBridge.Core.Common;
using SonaBridge.Core.Win;

using Xunit.Abstractions;

namespace CoreTest;

public class WinAutoServiceTests : IClassFixture<ServiceFixture>
{
	private readonly ServiceFixture _fixture;
	private readonly WinTalkAutoService _service;
	private readonly ITestOutputHelper _output;

	public WinAutoServiceTests(
		ServiceFixture fixture,
		ITestOutputHelper output
	)
	{
		_fixture = fixture;
		_service = new WinTalkAutoService();
		_output = output;
	}



	[Fact]
	public async Task GetAppAsync()
	{
		//var service = new WinTalkAutoService();
		await _service.GetAppWindowAsync();
	}

	[Fact]
	public async Task PlayAsync()
	{
		//var service = new WinTalkAutoService();
		await _service.GetAppWindowAsync();
		await _service.PlayUtterance();
	}

	[Theory]
	[InlineData(3)]
	public async Task GetIndexAsync(int index)
	{
		//var service = new WinTalkAutoService();
		await _service.GetAppWindowAsync();
		var result = _service.GetUtterancePosition();

		Assert.Equal(index, result);
	}
	[Theory]
	[InlineData(6)]
	public async Task GetLenIndex(int index)
	{
		//var service = new WinTalkAutoService();
		await _service.GetAppWindowAsync();
		var result = _service.GetLengthPosition();

		Assert.Equal(index, result);
	}

	[Theory]
	[InlineData("テスト")]
	public async Task SetText(string text)
	{
		//var _service.= new WinTalkAutoService();
		await _service.GetAppWindowAsync();
		await _service.SetUtterance(text);
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
		await _service.SetUtterance(text);
		await _service.PlayUtterance();

		await Task.Delay(1000);
	}

	[Fact]
	public async void GetVoices()
	{
		//var _service.= new WinTalkAutoService();
		await _service.GetAppWindowAsync();
		var items = await _service.GetVoiceNames();

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
		var result = await _service.SetVoiceAsync(name);

		result.Should().Be(expected);
		await _service.PlayUtterance();
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
		var sw = System.Diagnostics.Stopwatch.StartNew();

		await _service.OpenGlobalParamsPanelAsync();

		sw.Stop();
		_output.WriteLine($"1 toggle gparam. time: {sw.Elapsed.TotalSeconds} sec.");
		sw.Restart();

		await _service.OpenGlobalParamsPanelAsync();

		sw.Stop();
		_output.WriteLine($"2 toggle gparam. time: {sw.Elapsed.TotalSeconds} sec.");
	}

	[Fact]
	public async void GetGlobalParamSliders()
	{
		//var _service.= new WinTalkAutoService();
		var sw = System.Diagnostics.Stopwatch.StartNew();

		var sliders = await _service.GetGlobalParamSliders();

		sw.Stop();
		foreach(var s in sliders)
		{
			var s2 = s.AsSlider();
			_output.WriteLine($"Slider: {s2.Value}, max:{s2.Maximum} min:{s2.Minimum}");
		}
		_output.WriteLine($"get sliders. time: {sw.Elapsed.TotalSeconds} sec.");

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
		var sw = System.Diagnostics.Stopwatch.StartNew();

		await _service.SetCurrentGlobalParamsAsync(
			new Dictionary<string,double>(StringComparer.Ordinal){
				{key, value},
			});

		var values = await _service.GetCurrentGlobalParamAsync();
		sw.Stop();
		_output.WriteLine($"SetGlobalParamSingle time: {sw.Elapsed.TotalSeconds} sec.");
		values.TryGetValue(key, out var found)
			.Should().Be(hasKey);
		var isSame = Math.Abs(found - value) < 0.01;
		isSame.Should().Be(expect);
	}

	[Theory]
	[InlineData("Tanaka San")]

	public async void GetStyleSliders(string voice)
	{
		//var _service.= new WinTalkAutoService();
		var sw = System.Diagnostics.Stopwatch.StartNew();

		var sliders = await _service.GetStyleSlidersAsync(voice);

		sw.Stop();
		foreach(var s in sliders)
		{
			var s2 = s.AsSlider();
			_output.WriteLine($"Slider: {s2.Value}, max:{s2.Maximum} min:{s2.Minimum}");
		}
		_output.WriteLine($"get sliders. time: {sw.Elapsed.TotalSeconds} sec.");

		sw.Restart();
		var names = await _service.GetCurrentStyleNamesAsync();
		sw.Stop();
		foreach (var item in names)
		{
			_output.WriteLine($"Style name: {item} ");
		}
		_output.WriteLine($"get style names time: {sw.Elapsed.TotalSeconds} sec.");

		sw.Restart();
		var values = await _service.GetCurrentStylesAsync(voice);
		sw.Stop();
		foreach (var item in values)
		{
			_output.WriteLine($"Value: {item.Key}, {item.Value:F2} ");
		}
		_output.WriteLine($"get style time: {sw.Elapsed.TotalSeconds} sec.");

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
}
