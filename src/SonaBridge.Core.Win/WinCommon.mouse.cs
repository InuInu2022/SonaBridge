using System.Drawing;

using FlaUI.Core.Input;

namespace SonaBridge.Core.Win;

public sealed partial class WinCommon
{
	static Point? originalMousePosition;

	public static void SaveMousePoint()
	{
		originalMousePosition = Mouse.Position;
	}

	public static async ValueTask RestoreMousePointAsync(
		int wait = 100
	)
	{
		//Mouse.MoveTo(originalPosition ?? new(0,0));
		Mouse.Position = originalMousePosition ?? new(0, 0);
		await Task.Delay(wait).ConfigureAwait(false);
	}

	public static void MoveMouseCorner()
	{
		Mouse.Position = new(0, 0);
	}
}
