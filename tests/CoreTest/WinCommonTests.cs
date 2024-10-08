using System.Diagnostics;

using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;
using FlaUI.Core.Input;
using FlaUI.Core.WindowsAPI;

using FluentAssertions;

using SharpHook;
using SharpHook.Native;

using SonaBridge;
using SonaBridge.Core.Common;
using SonaBridge.Core.Win;

using Xunit.Abstractions;

namespace CoreTest;

public class WinCommonTests(ITestOutputHelper output)
{
	private readonly ITestOutputHelper _output = output;

	[Theory]
	[InlineData("保存テスト音声")]
	[InlineData("これは音声保存のテストです")]
	public async void WinSaveWavAsync(string text)
	{
		var service = new WinTalkAutoService();
		await service.GetAppWindowAsync();
		await service.SetUtterance(text);

		var path = Path.Combine(
			Path.GetTempPath(),
			Path.ChangeExtension(Path.GetRandomFileName(),"wav")
		);

		_output.WriteLine($"temp wav file: {path}");

		await service.SaveWavAsync(path)
			.ConfigureAwait(true);



		File.Exists(path).Should().BeTrue();

		//*
		Process.Start(new ProcessStartInfo()
		{
			FileName = Path.GetDirectoryName(path),
			UseShellExecute = true,
		});
		//*/
	}

	[Fact]
	public async void SendKeyTest()
	{
		var service = new WinTalkAutoService();
		await service.GetAppWindowAsync();
		await service.SetUtterance("あいうえお");
		service.SetFocusFirstRow(true);
		var win = service.TopWindow;
		win?.SetForeground();

		var menus = GetMenuItems(win);
		foreach (var item in menus ?? [])
		{
			_output.WriteLine($"items: {item}");
		}
		var eMenu = menus?.FirstOrDefault(m =>
			string.Equals(m.Name, "エクスポート", StringComparison.Ordinal));
		eMenu.AsMenuItem().Invoke();

		Wait.UntilInputIsProcessed();
		menus = GetMenuItems(win);
		foreach (var item in menus ?? [])
		{
			_output.WriteLine($"items: {item}");
		}
		var menu = menus?
			.FirstOrDefault(m => m.Name.Contains("WAV", StringComparison.InvariantCultureIgnoreCase));
		menu.AsMenuItem().Invoke();


		Wait.UntilInputIsProcessed();


		static AutomationElement[]? GetMenuItems(Window? win)
		{
			using var automation = new FlaUI.UIA3.UIA3Automation();
			return automation
				.GetDesktop()
				.FindAllChildren()
				.FirstOrDefault(w =>
					string.Equals(w.Name, "VoiSona Talk Editor", StringComparison.Ordinal)
					&& w.AsWindow().IsModal)?
				.FindAllDescendants(f =>
					f.ByControlType(ControlType.MenuItem)
					.And(f.ByFrameworkId("JUCE"))
				);
		}
	}
}
