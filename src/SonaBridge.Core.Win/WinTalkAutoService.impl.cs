using System.Diagnostics;
using System.Globalization;

using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;
using FlaUI.Core.Input;
using FlaUI.Core.Tools;
using FlaUI.Core.WindowsAPI;
using FlaUI.UIA3;

using SonaBridge.Core.Common;

namespace SonaBridge.Core.Win;

public partial class WinTalkAutoService : ITalkAutoService
{
	private readonly string _pathToVsTalk = """C:\Program Files\Techno-Speech\VoiSona Talk""";
	private Application? _app;
	private Window? _win;
	private AutomationElement? _table;
	private int _uPos = -1;
	private int _lenPos = -1;
	private AutomationElement? _row;
	private static IReadOnlyList<string>? VoiceNames { get; set; }

	internal Window? TopWindow { get => _win; }

	internal async ValueTask GetAppWindowAsync(string? pathToExe = null)
	{
		_app ??= await GetApp(pathToExe).ConfigureAwait(false);
		using var automation = new UIA3Automation();
		_win ??= _app?.GetAllTopLevelWindows(automation)[0];
	}

	internal async ValueTask PlayUtterance(
		CancellationToken? ctx = default
	)
	{
		var row = GetRow();

		var col = GetLengthPosition();
		var box = GetTextBox(col);
		var t = box?.Text is null or [] ? "5.0" : box.Text;
		var sec = double.Parse(t, CultureInfo.InvariantCulture);
		var tp = TimeSpan.FromSeconds(sec+0.5);

		var result = await Task.Run(()=>
			Retry.WhileNull(() =>
				row?
					.FindAllDescendants(f => f
						.ByControlType(ControlType.Button)
						.And(f.ByName("UtteranceListPanelDrawableButton"))
					)
					.FirstOrDefault(
						e => e.Patterns.GridItem.Pattern.Column == 0
					)
					.AsButton(),
			TimeSpan.FromSeconds(30),
			TimeSpan.FromMilliseconds(300))
		).ConfigureAwait(false);

		//playBtn?.FocusNative();
		//playBtn?.SetForeground();
		result.Result?.Invoke();

		await Task.Delay(tp, ctx ?? default)
			.ConfigureAwait(false);
		System.Diagnostics.Debug.WriteLine($"len: {tp}, {t}");
	}

	internal async ValueTask SetUtterance(string text = "")
	{
		var col = GetUtterancePosition();
		var edit = GetTextBox(col);

		if (edit is null) return;
		edit.FocusNative();
		await Task
			.Run(() => edit.WaitUntilEnabled(TimeSpan.FromSeconds(10)))
			.ConfigureAwait(false);
		edit.Text = text;
		//edit.Enter(text);
		Keyboard.Press(VirtualKeyShort.RETURN);

		await Task.Run(() =>
			GetUtteranceEneble()
				.WaitUntilEnabled(TimeSpan.FromSeconds(10))
		).ConfigureAwait(false);
		//await Task.Delay(100).ConfigureAwait(false);
		//Keyboard.Press(VirtualKeyShort.RETURN);

		//wait
		//await Task.Delay(300).ConfigureAwait(false);
	}

	internal async ValueTask<IReadOnlyList<string>> GetVoiceNames()
	{
		if(VoiceNames?.Any() == true){ return [.. VoiceNames]; }
		ComboBox? cb = GetVoiceCombo();

		if (cb is null) return [];

		if (!cb.ExpandCollapseState.Equals(ExpandCollapseState.Expanded))
		{
			cb.Expand();
		}

		using var automation = new UIA3Automation();
		var result = await GetVoiceListAsync().ConfigureAwait(false);
		foreach (var item in result)
		{
			Console.WriteLine(item.Name);
			System.Diagnostics.Debug.WriteLine(item.Name);
		}
		var voiceNames = result.Select(v => v.Name).ToList();
		VoiceNames = [..voiceNames];
		return VoiceNames ?? [];
	}

	internal async ValueTask<bool> SetVoiceAsync(string voiceName)
	{
		ComboBox? cb = GetVoiceCombo();

		if (cb is null) return false;

		if (!cb.ExpandCollapseState.Equals(ExpandCollapseState.Expanded))
		{
			cb.Expand();
		}

		using var automation = new UIA3Automation();
		var result = await GetVoiceListAsync().ConfigureAwait(false);
		var voice = result
			.FirstOrDefault(v => string.Equals(v.Name, voiceName, StringComparison.Ordinal))
			.AsMenuItem()
			;
		if (voice is null) return false;

		voice.Focus();
		voice.Expand();
		await SetVoiceInnerAsync(automation).ConfigureAwait(false);

		//wait
		await Task.Run(()=>
			Retry.WhileTrue(() =>
			{
				//TODO: 正確な合成待ちを追加する
				var isOffScr = _win?.AsWindow().IsOffscreen ?? true;

				return isOffScr && !_win!.IsAvailable && !_win!.IsEnabled;
			},
			TimeSpan.FromSeconds(30),
			TimeSpan.FromMilliseconds(100))
		).ConfigureAwait(false);
		await Task.Delay(100).ConfigureAwait(false);	//安全策

		return true;
	}

	internal void SetFocusFirstRow(bool isWithRightClick = false)
	{
		var row = GetRow();
		row.AsGridRow().Focus();
		if (isWithRightClick)
		{
			row.WaitUntilClickable();
			row.AsGridRow().RightClick();
		}
	}

	async Task<RetryResult<AutomationElement[]?>> SetVoiceInnerAsync(UIA3Automation automation){
		var modals = await Task
			.Run(() => Retry.WhileNull(
				() =>
				{
					var customs = automation
						.GetDesktop()
						.FindAllChildren()
						.FirstOrDefault(w =>
							string.Equals(w.Name, "VoiSona Talk Editor", StringComparison.Ordinal)
							&& w.AsWindow().IsModal)?
						.FindAllDescendants(
							f => f.ByFrameworkId("JUCE")
							.And(f.ByControlType(ControlType.Custom))
						);
					return customs?.Length == 0 ? null : customs;
				},
				timeout: TimeSpan.FromSeconds(30),
				interval: TimeSpan.FromSeconds(0.1),
				ignoreException: true
			))
			.ConfigureAwait(false);

		modals.Result?[0].Click();

		return modals;
	}

	static async Task<AutomationElement[]>
	GetVoiceListAsync()
	{
		return await GetModalMenuItems().ConfigureAwait(false);
	}

	ComboBox? GetVoiceCombo()
	{
		return _win?.FindFirstDescendant(
				f => f.ByControlType(ControlType.ComboBox)
				.And(f.ByHelpText("ボイスを選択"))
			)
			.AsComboBox();
	}

	TextBox? GetTextBox(int col)
	{
		var row = GetRow();
		var children = row?.FindAllDescendants(
			f => f.ByControlType(ControlType.Edit)
		);
		return children?
			.FirstOrDefault(x => x.AsGridCell().Patterns.GridItem.Pattern.Column == col)
			.AsTextBox();
	}

	CheckBox? GetUtteranceEneble()
	{
		var row = GetRow();
		var target = row?.FindFirstDescendant(
			f => f.ByControlType(ControlType.CheckBox)
		).AsCheckBox();
		return target;
	}

	internal int GetUtterancePosition(string headerName = "文")
	{
		if(_uPos >= 0){return _uPos;}

		//header行の「文」列の位置から調べる
		_uPos = GetPosition(headerName);
		return _uPos;
	}

	/// <summary>
	/// 「長さ」列の位置を取得
	/// </summary>
	/// <param name="headerName">note: 別言語UIの場合は表記を変える必要がある</param>
	/// <returns></returns>
	internal int GetLengthPosition(string headerName = "長さ")
	{
		if(_lenPos >= 0){return _lenPos;}
		_lenPos = GetPosition(headerName);
		return _lenPos;
	}

	int GetPosition(string headerName)
	{
		var table = GetTable();
		var hRow = table?
			.FindFirstChild(f => f.ByControlType(ControlType.Header))
			.AsGridHeader();
		var children = hRow?
			.FindAllChildren(f => f.ByControlType(ControlType.Header));
		return children?
			.Select((v, i) => (v, i))
			.FirstOrDefault(x => string
				.Equals(x.v.Name, headerName, StringComparison.Ordinal))
			.i ?? -1;
	}

	AutomationElement? GetRow()
	{
		if(_row is not null){return _row;}
		var table = GetTable();
		_row = table?
			.FindFirstChild(f => f.ByName("Row 1"));
		return _row;
	}

	AutomationElement? GetTable()
	{
		if (_table is not null) return _table;

		_table = _win?
			.FindFirstChild(f => f
				.ByControlType(ControlType.Table))
			?? default;
		return _table;
	}

	async ValueTask<Application?> GetApp(
		string? pathToExe
	)
	{
		var path = pathToExe is [] or null
			? _pathToVsTalk : pathToExe;
		var psi = new ProcessStartInfo()
		{
			FileName = path,
		};

		var result = await Task
			.Run(() => Retry.WhileNull(
				() => Application.AttachOrLaunch(psi),
				timeout: TimeSpan.FromSeconds(30),
				interval: TimeSpan.FromSeconds(1),
				ignoreException: true
			))
			.ConfigureAwait(false);
		return result.Success ? result.Result : default;
	}


}
