using FlaUI.Core.Tools;

namespace SonaBridge.Core.Win;

public partial class WinTalkAutoService
{
	internal async ValueTask SaveWavAsync(
		string fullPathWavFile,
		string exportMenuItemName = "エクスポート",
		string wavExportMenuName = "WAV"
	)
	{
		var sw = System.Diagnostics.Stopwatch.StartNew();
		await GetAppWindowAsync().ConfigureAwait(false);
		if (_win is null) throw new InvalidOperationException("window is null");
		_win.SetForeground();
		//_win.Focus();

		SetFocusFirstRow(true);

		await InvokeModalMenuItemAsync(m =>
			string.Equals(m.Name, exportMenuItemName, StringComparison.Ordinal))
			.ConfigureAwait(false);

		await InvokeModalMenuItemAsync(m =>
#if NETSTANDARD2_0
			m.Name.Contains(wavExportMenuName))
#else
			m.Name.Contains(wavExportMenuName, StringComparison.InvariantCultureIgnoreCase))
#endif
			.ConfigureAwait(false);

		sw.Stop();
		Console.WriteLine($"export setting time : {sw.Elapsed.TotalSeconds}");
		sw.Restart();

		await WinCommon.SaveWavFileAsync(
			_win,
			fullPathWavFile
		).ConfigureAwait(false);

		sw.Stop();
		Console.WriteLine($"save time : {sw.Elapsed.TotalSeconds}");
		sw.Restart();

		// ".wav"以外の拡張子を与えられたら出力ファイルの".wav"を消す
		await FixExtensionAsync(fullPathWavFile).ConfigureAwait(false);

		await _win.WaitUntilClickableAsync(TimeSpan.FromSeconds(10))
			.ConfigureAwait(false);

		sw.Stop();
		Console.WriteLine($"wait until time : {sw.Elapsed.TotalSeconds}");
		//sw.Restart();
	}

	internal static async ValueTask FixExtensionAsync(string fullPathWavFile)
	{
		if (fullPathWavFile.EndsWith(".wav", StringComparison.OrdinalIgnoreCase)) return;

		await Task.Run(() =>
		{
			var chk = $"{fullPathWavFile}.wav";

			var r1 = Retry.WhileFalse(
				() => File.Exists(chk),
				TimeSpan.FromSeconds(5),
				TimeSpan.FromSeconds(0.1),
				ignoreException:true
			);

			if (!r1.Success) throw new FileNotFoundException(chk);

			var newName = Path.GetFileNameWithoutExtension(chk);
			var newfull = Path.Combine(Path.GetDirectoryName(chk!)!, newName);
			var r2 = Retry.WhileException(
				()=>{
#if NETSTANDARD2_0
					File.Delete(newfull);
					File.Move(
						chk,
						newfull
					);
#else
					File.Move(
						chk,
						newfull,
						overwrite: true
					);
#endif
				},
				TimeSpan.FromSeconds(5),
				TimeSpan.FromSeconds(0.1)
			);

			if(!r2.Success) throw new FileNotFoundException(newfull);
		}).ConfigureAwait(false);
	}
}
