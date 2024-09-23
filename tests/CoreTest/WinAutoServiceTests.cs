using System;
using System.Diagnostics;
using System.Threading.Tasks;
using SonaBridge.Core.Common;
using SonaBridge.Core.Win;
using Xunit;

namespace CoreTest;

public class WinAutoServiceTests
{
	[Fact]
	public async Task GetAppAsync()
	{
		var service = new WinAutoService();
		await service.GetAppWindowAsync();
	}

	[Fact]
	public async Task PlayAsync()
	{
		var service = new WinAutoService();
		await service.GetAppWindowAsync();
		await service.PlayUtterance();
	}

	[Theory]
	[InlineData(3)]
	public async Task GetIndexAsync(int index)
	{
		var service = new WinAutoService();
		await service.GetAppWindowAsync();
		var result = service.GetUtterancePosition();

		Assert.Equal(index, result);
	}
	[Theory]
	[InlineData(6)]
	public async Task GetLenIndex(int index)
	{
		var service = new WinAutoService();
		await service.GetAppWindowAsync();
		var result = service.GetLengthPosition();

		Assert.Equal(index, result);
	}

	[Theory]
	[InlineData("テスト")]
	public async Task SetText(string text)
	{
		var service = new WinAutoService();
		await service.GetAppWindowAsync();
		await service.SetUtterance(text);
	}

	[Theory]
	[InlineData("こんにちは")]
	[InlineData("どうも。")]
	[InlineData("本日は晴天なり。")]
	[InlineData("あめんぼ甘いな、あいうえお。")]
	public async Task PlayTextAsync(string text)
	{
		var service = new WinAutoService();
		await service.GetAppWindowAsync();
		await service.SetUtterance(text);
		await service.PlayUtterance();

		await Task.Delay(1000);
	}
}
