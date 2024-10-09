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

		await InvokeModalMenuItemAsync(m =>
			string.Equals(m.Name, exportMenuItemName, StringComparison.Ordinal))
			.ConfigureAwait(false);

		await InvokeModalMenuItemAsync(m =>
			m.Name.Contains(wavExportMenuName, StringComparison.InvariantCultureIgnoreCase))
			.ConfigureAwait(false);

		await WinCommon.SaveWavFileAsync(
			_win,
			fullPathWavFile
		).ConfigureAwait(false);

		// ".wav"以外の拡張子を与えられたら出力ファイルの".wav"を消す
		await FixExtensionAsync(fullPathWavFile).ConfigureAwait(false);

		await Task.Run(
			() => _win.WaitUntilClickable(TimeSpan.FromSeconds(10))
		).ConfigureAwait(false);
	}

	internal static async ValueTask FixExtensionAsync(string fullPathWavFile)
	{
		if (fullPathWavFile.EndsWith(".wav", StringComparison.OrdinalIgnoreCase)) return;

		await Task.Run(() =>
		{
			var chk = $"{fullPathWavFile}.wav";

			var r1 = Retry.WhileFalse(
				() => File.Exists(chk),
				TimeSpan.FromSeconds(10),
				TimeSpan.FromSeconds(0.1),
				ignoreException:true
			);

			if (!r1.Success) throw new FileNotFoundException(chk);

			var newName = Path.GetFileNameWithoutExtension(chk);
			var newfull = Path.Combine(Path.GetDirectoryName(chk!)!, newName);
			var r2 = Retry.WhileException(
				()=>{

					File.Copy(
						chk,
						newfull,
						overwrite: true
					);
				},
				TimeSpan.FromSeconds(10),
				TimeSpan.FromSeconds(0.1)
			);

			if(!r2.Success) throw new FileNotFoundException(newfull);
		}).ConfigureAwait(false);
	}
}
