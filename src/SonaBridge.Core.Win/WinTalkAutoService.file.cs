using System;

using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;
using FlaUI.Core.Input;
using FlaUI.Core.Tools;
using FlaUI.Core.WindowsAPI;

namespace SonaBridge.Core.Win;

public partial class WinTalkAutoService
{
	internal async ValueTask SaveWavAsync(
		string fullPathWavFile,
		string exportMenuItemName = "エクスポート",
		string wavExportMenuName = "WAV"
	)
	{
		await GetAppWindowAsync().ConfigureAwait(false);
		if (_win is null) throw new InvalidOperationException("window is null");
		_win.SetForeground();
		//_win.Focus();

		SetFocusFirstRow(true);

		/*
		var menus = await GetModalMenuItems().ConfigureAwait(false);
		var eMenu = menus?.FirstOrDefault(m =>
			string.Equals(m.Name, exportMenuItemName, StringComparison.Ordinal));
		eMenu.AsMenuItem().Invoke();
		await WinCommon.WaitUntilInputIsProcessedAsync().ConfigureAwait(false);
		*/
		await InvokeModalMenuItemAsync(m =>
			string.Equals(m.Name, exportMenuItemName, StringComparison.Ordinal))
			.ConfigureAwait(false);

		/*
		menus = await GetModalMenuItems().ConfigureAwait(false);
		var menu = menus?
			.FirstOrDefault(m => m.Name.Contains(wavExportMenuName, StringComparison.InvariantCultureIgnoreCase));
		menu.AsMenuItem().Invoke();
		await WinCommon.WaitUntilInputIsProcessedAsync().ConfigureAwait(false);
		*/
		await InvokeModalMenuItemAsync(m =>
			m.Name.Contains(wavExportMenuName, StringComparison.InvariantCultureIgnoreCase))
			.ConfigureAwait(false);

		await WinCommon.SaveWavFileAsync(
			_win,
			fullPathWavFile
		).ConfigureAwait(false);
		//await WinCommon.WaitUntilInputIsProcessedAsync().ConfigureAwait(false);

		await Task.Run(
			()=> _win.WaitUntilClickable(TimeSpan.FromSeconds(10))
		).ConfigureAwait(false);
	}
}
