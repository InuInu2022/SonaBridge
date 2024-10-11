using System.Drawing;

using FlaUI.Core.Input;

namespace SonaBridge.Core.Win;

public sealed partial class WinCommon
{
	static Point? originalPosition;

	public static void SaveMousePoint()
	{
		originalPosition = Mouse.Position;
	}

	public static async ValueTask RestoreMousePointAsync(
		int wait = 100
	)
	{
		//Mouse.MoveTo(originalPosition ?? new(0,0));
		Mouse.Position = originalPosition ?? new(0,0);
		await Task.Delay(wait).ConfigureAwait(false);
	}
}
