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
	private static Application? _app;
	private static Window? _win;
	private static AutomationElement? _table;
	private static int _uPos = -1;
	private static int _lenPos = -1;
	private static AutomationElement? _row;
	private static string? _lastVoiceName;
	private bool _disposedValue;
	private static ComboBox? _voiceCombo;
	private static CheckBox? _utteranceCheckBox;
	private static AutomationElement[]? _textBoxes;
	private static readonly UIA3Automation _automation = new();

	private static IReadOnlyList<string>? VoiceNames { get; set; }

	internal static Window? TopWindow { get => _win; }

	internal async ValueTask GetAppWindowAsync(string? pathToExe = null)
	{
		_app ??= await GetApp(pathToExe).ConfigureAwait(false);
		_win ??= _app?.GetAllTopLevelWindows(_automation)[0];
	}

	internal static async ValueTask PlayUtterance(
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
			TimeSpan.FromSeconds(3),
			TimeSpan.FromMilliseconds(100))
		).ConfigureAwait(false);

		//playBtn?.FocusNative();
		//playBtn?.SetForeground();
		result.Result?.Invoke();

		await Task.Delay(tp, ctx ?? default)
			.ConfigureAwait(false);
		System.Diagnostics.Debug.WriteLine($"len: {tp}, {t}");
	}

	internal static async ValueTask SetUtterance(string text = "")
	{
		_win?.SetForeground();
		await _win.WaitUntilEnabledAsync().ConfigureAwait(false);
		var col = GetUtterancePosition();
		var edit = GetTextBox(col);

		if (edit is null) return;
		edit.FocusNative();
		await edit.WaitUntilEnabledAsync(TimeSpan.FromSeconds(5))
			.ConfigureAwait(false);
		edit.Text = text;
		//edit.Enter(text);
		Keyboard.Press(VirtualKeyShort.RETURN);

		var checkbox = await Task.Run(()=>{
			var result = Retry
				.WhileNull(
					() => GetUtteranceEnable(),
					TimeSpan.FromSeconds(3),
					TimeSpan.FromMilliseconds(100),
					ignoreException:true
				);
			return result.Result;
		}).ConfigureAwait(false);
		await checkbox
			.WaitUntilEnabledAsync(TimeSpan.FromSeconds(5))
			.ConfigureAwait(false);
	}

	internal static async ValueTask<IReadOnlyList<string>> GetVoiceNames()
	{
		if(VoiceNames?.Any() == true){ return [.. VoiceNames]; }
		ComboBox? cb = GetVoiceCombo();

		if (cb is null) return [];

		if (!cb.ExpandCollapseState.Equals(ExpandCollapseState.Expanded))
		{
			cb.Expand();
		}

		var result = await GetVoiceListAsync().ConfigureAwait(false);
		foreach (var item in result)
		{
			Console.WriteLine(item.Name);
			System.Diagnostics.Debug.WriteLine(item.Name);
		}
		var voiceNames = result.Select(v => v.Name).ToList();
		VoiceNames = [..voiceNames];

		if (!cb.ExpandCollapseState.Equals(ExpandCollapseState.Collapsed))
		{
			cb.Click();
		}
		await cb.WaitUntilClickableAsync().ConfigureAwait(false);

		return VoiceNames ?? [];
	}

	internal static async ValueTask<bool> SetVoiceAsync(string voiceName)
	{
		if (string.Equals(_lastVoiceName, voiceName, StringComparison.Ordinal)) { return true; }

		ComboBox? cb = GetVoiceCombo();

		if (cb is null) return false;

		if (!cb.ExpandCollapseState.Equals(ExpandCollapseState.Expanded))
		{
			cb.Expand();
		}

		var result = await GetVoiceListAsync().ConfigureAwait(false);
		var voice = result
			.FirstOrDefault(v => string.Equals(v.Name, voiceName, StringComparison.Ordinal))
			.AsMenuItem()
			;
		if (voice is null) return false;

		voice.Focus();
		voice.Expand();
		await SetVoiceInnerAsync(_automation).ConfigureAwait(false);

		//wait
		await Task.Run(()=>
			Retry.WhileTrue(() =>
			{
				cb.WaitUntilClickable();
				var isOffScr = _win?.AsWindow().IsOffscreen ?? true;

				return isOffScr && !_win!.IsAvailable && !_win!.IsEnabled;
			},
			TimeSpan.FromSeconds(10),
			TimeSpan.FromMilliseconds(100))
		).ConfigureAwait(false);
		await Task.Delay(50).ConfigureAwait(false);	//安全策

		Console.WriteLine($"last:{_lastVoiceName}, set:{voiceName}");
		_lastVoiceName = voiceName;

		return true;
	}

	internal static void SetFocusFirstRow(bool isWithRightClick = false)
	{
		var row = GetRow();
		row.AsGridRow().Focus();
		if (isWithRightClick)
		{
			row.WaitUntilClickable();
			row.AsGridRow().RightClick();
		}
	}

	static async Task<RetryResult<AutomationElement[]?>> SetVoiceInnerAsync(UIA3Automation automation){
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
				timeout: TimeSpan.FromSeconds(10),
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

	static ComboBox? GetVoiceCombo()
	{
		if (_voiceCombo is not null) return _voiceCombo;
		_voiceCombo = _win?.FindFirstDescendant(
				f => f.ByControlType(ControlType.ComboBox)
				.And(f.ByHelpText("ボイスを選択"))
			)
			.AsComboBox();
		return _voiceCombo;
	}

	static TextBox? GetTextBox(int col)
	{
		if (_textBoxes is null)
		{
			var row = GetRow();
			_textBoxes = row?.FindAllDescendants(
				f => f.ByControlType(ControlType.Edit)
			);
		}
		return _textBoxes?
			.FirstOrDefault(x => x.AsGridCell().Patterns.GridItem.Pattern.Column == col)
			.AsTextBox();
	}

	static CheckBox? GetUtteranceEnable()
	{
		if (_utteranceCheckBox is not null) return _utteranceCheckBox;
		var row = GetRow();
		_utteranceCheckBox = row?.FindFirstDescendant(
			f => f.ByControlType(ControlType.CheckBox)
		).AsCheckBox();
		return _utteranceCheckBox;
	}

	internal static int GetUtterancePosition(string headerName = "文")
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
	internal static int GetLengthPosition(string headerName = "長さ")
	{
		if(_lenPos >= 0){return _lenPos;}
		_lenPos = GetPosition(headerName);
		return _lenPos;
	}

	static int GetPosition(string headerName)
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

	static AutomationElement? GetRow()
	{
		if(_row is not null){return _row;}
		var table = GetTable();
		_row = table?
			.FindFirstChild(f => f.ByName("Row 1"));
		return _row;
	}

	static AutomationElement? GetTable()
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

	protected virtual void Dispose(bool disposing)
	{
		if (!_disposedValue)
		{
			if (disposing)
			{
				_automation.Dispose();
			}

			_disposedValue = true;
		}
	}

	public void Dispose()
	{
		// このコードを変更しないでください。クリーンアップ コードを 'Dispose(bool disposing)' メソッドに記述します
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}
}
