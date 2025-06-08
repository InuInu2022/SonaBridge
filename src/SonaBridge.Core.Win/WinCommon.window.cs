using System.Drawing;
using System.Runtime.InteropServices;

using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;
using FlaUI.Core.Input;

namespace SonaBridge.Core.Win;

// ウィンドウ操作
public sealed partial class WinCommon
{
	static Point? originalWinPosition;
	static Size originalWinSize = new(800, 600);
	static double windowScaleFactor = 1.0;

	/// <summary>
	/// マウスカーソル誤動作防止の為、自動操作対象ウィンドウを画面外に移動する
	/// </summary>
	/// <param name="window"></param>
	/// <returns></returns>
	public static async ValueTask MoveWindowOutOfScreenAsync(Window window)
	{
		if (window is null) return;

		windowScaleFactor = GetScaleFactor(window);
		var rect = window.BoundingRectangle;

		// ウィンドウの位置を保存
		originalWinPosition = new(
			(int)rect.Left,
			(int)rect.Top
		);
		originalWinSize = new Size(
			(int)rect.Width,
			(int)rect.Height
		);

		window.Patterns.Window.Pattern
			.SetWindowVisualState(WindowVisualState.Normal);

		// ウィンドウを画面外に移動
		window.Move(4000, 4000);
		// サイズがおかしくならないように調整
		if (window.Patterns.Transform.IsSupported)
		{
			window.Patterns.Transform.Pattern
				.Resize(
					originalWinSize.Width / windowScaleFactor,
					originalWinSize.Height / windowScaleFactor
				);
		}

		await Task.Delay(100).ConfigureAwait(false);
	}

	/// <summary>
	/// 自動操作対象ウィンドウの位置を元に戻す
	/// </summary>
	/// <param name="window"></param>
	/// <returns></returns>
	public static async ValueTask RestoreWindowPositionAsync(Window window)
	{
		if (window is null || originalWinPosition is null) return;
		windowScaleFactor = GetScaleFactor(window);

		// ウィンドウを元の位置に戻す（そのまま復元）
		window.Move(
			originalWinPosition.Value.X,    // そのまま復元
			originalWinPosition.Value.Y     // そのまま復元
		);
		// サイズも元に戻す
		if (window.Patterns.Transform.IsSupported)
		{
			window.Patterns.Transform.Pattern.Resize(
				originalWinSize.Width / windowScaleFactor,
				originalWinSize.Height / windowScaleFactor
			);
		}

		await Task.Delay(100).ConfigureAwait(false);
	}

	[DllImport("User32.dll")]
	static extern int GetDpiForWindow(IntPtr hWnd);

	static double GetScaleFactor(Window window)
	{
		if (window.Properties.NativeWindowHandle?.Value == null)
			return 1.0; // デフォルトのスケールファクター

		var hwnd = window.Properties.NativeWindowHandle.Value;
		var dpi = GetDpiForWindow(hwnd);
		return dpi / 96.0;
	}
}
