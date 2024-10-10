using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;
using FlaUI.Core.Input;
using FlaUI.Core.Tools;

namespace SonaBridge.Core.Win;

public sealed class WinCommon
{
	internal static async ValueTask SaveWavFileAsync(
		Window window,
		string pathAndFileName,
		string saveDialogTitle = "WAVを出力",
		string overrideDialogTitle = "保存の確認"
	)
	{
		//modal dialog
		var saveDialog = await GetWin32DialogAsync(window, saveDialogTitle)
			.ConfigureAwait(false);
		if (saveDialog is null) return;

		//file name
		var fileNameBox = saveDialog
			.FindFirstDescendant(cf => cf.ByAutomationId("1001"))
			.AsTextBox();
		if (fileNameBox is null) return;
		fileNameBox.Text = pathAndFileName;

		var saveButton = saveDialog
			.FindFirstDescendant(cf =>
				cf.ByAutomationId("1")
					.And(cf.ByControlType(ControlType.Button))
			)
			.AsButton()
			;
		saveButton?.Invoke();
		//await WaitUntilInputIsProcessedAsync().ConfigureAwait(false);

		//override check
		var overwriteDialog = await GetWin32DialogAsync(
			window, overrideDialogTitle, 1
		).ConfigureAwait(false);
		var overwriteButton = overwriteDialog?
			.FindFirstDescendant(cf => cf.ByAutomationId("CommandButton_6")
				.And(cf.ByControlType(ControlType.Button)))
			.AsButton();
		overwriteButton?.Invoke();
		//await WaitUntilInputIsProcessedAsync().ConfigureAwait(false);
	}

	static async Task<Window?> GetWin32DialogAsync(
		Window window,
		string titleContainText,
		int timeout = 5
	)
	{
		var result = await Task
			.Run(() =>
				Retry.WhileNull(() =>
				{
					return window
						.ModalWindows
						.FirstOrDefault(w => w.Title.Contains(titleContainText, StringComparison.Ordinal));
				},
				TimeSpan.FromSeconds(timeout),
				TimeSpan.FromMilliseconds(100))
			).ConfigureAwait(false);
		return result.Success ? result.Result : null;
	}

	public static async Task WaitUntilInputIsProcessedAsync(int timeoutSec = 30)
	{
		await Task.Run(()=>Wait.UntilInputIsProcessed(TimeSpan.FromSeconds(timeoutSec)))
			.ConfigureAwait(false);
	}
}
