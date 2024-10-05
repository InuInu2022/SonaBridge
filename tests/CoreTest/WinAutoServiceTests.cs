using FluentAssertions;

using SonaBridge.Core.Win;

using Xunit.Abstractions;

namespace CoreTest;

public class WinAutoServiceTests(ITestOutputHelper output)
{
	private readonly ITestOutputHelper _output = output;

	[Fact]
	public async Task GetAppAsync()
	{
		var service = new WinTalkAutoService();
		await service.GetAppWindowAsync();
	}

	[Fact]
	public async Task PlayAsync()
	{
		var service = new WinTalkAutoService();
		await service.GetAppWindowAsync();
		await service.PlayUtterance();
	}

	[Theory]
	[InlineData(3)]
	public async Task GetIndexAsync(int index)
	{
		var service = new WinTalkAutoService();
		await service.GetAppWindowAsync();
		var result = service.GetUtterancePosition();

		Assert.Equal(index, result);
	}
	[Theory]
	[InlineData(6)]
	public async Task GetLenIndex(int index)
	{
		var service = new WinTalkAutoService();
		await service.GetAppWindowAsync();
		var result = service.GetLengthPosition();

		Assert.Equal(index, result);
	}

	[Theory]
	[InlineData("テスト")]
	public async Task SetText(string text)
	{
		var service = new WinTalkAutoService();
		await service.GetAppWindowAsync();
		await service.SetUtterance(text);
	}

	[Theory]
	[InlineData("こんにちは")]
	[InlineData("ありがとうございます。")]
	[InlineData("本日は晴天なり。")]
	[InlineData("あめんぼ甘いな、あいうえお。")]
	public async Task PlayTextAsync(string text)
	{
		var service = new WinTalkAutoService();
		await service.GetAppWindowAsync();
		await service.SetUtterance(text);
		await service.PlayUtterance();

		await Task.Delay(1000);
	}

	[Fact]
	public async void GetVoices()
	{
		var service = new WinTalkAutoService();
		await service.GetAppWindowAsync();
		var items = await service.GetVoiceNames();

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
		var service = new WinTalkAutoService();
		await service.GetAppWindowAsync();
		var result = await service.SetVoiceAsync(name);

		result.Should().Be(expected);
		await service.PlayUtterance();
	}

	[Fact]
	public async void SetCastAsyncThrowsInvalidOperationExceptionWhenCastNameNotFound()
	{
		var service = new WinTalkAutoService();
		await service.GetAppWindowAsync();

		var ex = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
		{
			await service.SetCastAsync("NonExistentCastName")
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
		var service = new WinTalkAutoService();
		await service.SetCastAsync(castName);
		await service.SpeakAsync(text);
		//await service.GetAppWindowAsync();
		//var result = await service.SetVoiceAsync(castName);
		//await service.SetUtterance(text);
		//await service.PlayUtterance();
	}
}
