using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;
using FlaUI.Core.Tools;

namespace SonaBridge.Core.Win;

public partial class WinTalkAutoService
{
	/// <summary>
	/// VoiSona Talkのmenu展開したモーダルウィンドウ内の選択肢<see cref="MenuItem"/>を取得する
	/// </summary>
	/// <returns></returns>
	static async ValueTask<AutomationElement[]> GetModalMenuItems()
	{
		var result = await Task
			.Run(() => Retry.WhileNull(
				() => {
					var ms = _automation
					.GetDesktop()
					.FindAllChildren()
					.FirstOrDefault(w =>
						string.Equals(w.Name, "VoiSona Talk Editor", StringComparison.Ordinal)
						&& w.AsWindow().IsModal)?
					.FindAllDescendants(f =>
						f.ByControlType(ControlType.MenuItem)
						.And(f.ByFrameworkId("JUCE"))
					);
					return ms is [] or null ? null : ms;
				},
				timeout: TimeSpan.FromSeconds(3),
				interval: TimeSpan.FromSeconds(0.1),
				ignoreException: true
			))
			.ConfigureAwait(false);
		return !result.Success
			? throw new InvalidOperationException("Failed to get menu item")
			: result.Result ?? [];
	}

	static async ValueTask InvokeModalMenuItemAsync(
		Func<AutomationElement, bool> predicate
	)
	{
		var menus = await GetModalMenuItems().ConfigureAwait(false);
		var eMenu = menus?.FirstOrDefault(predicate);
		eMenu.AsMenuItem().Invoke();
		//await WinCommon.WaitUntilInputIsProcessedAsync().ConfigureAwait(false);
	}
}
