using SonaBridge.Core.Common;

namespace SonaBridge.Core.Win;

public partial class WinTalkAutoService : ITalkAutoService
{
	public WinTalkAutoService()
	{
	}

	/// <inheritdoc/>
	public async Task<bool> SpeakAsync(
		string text,
		CancellationToken? token = null)
	{
		await GetAppWindowAsync().ConfigureAwait(false);
		await SetUtterance(text).ConfigureAwait(false);
		await PlayUtterance(token).ConfigureAwait(false);

		return true;
	}

	public async Task<string[]> GetAvailableCastsAsync()
	{
		await GetAppWindowAsync().ConfigureAwait(false);
		var voices = await GetVoiceNames().ConfigureAwait(false);
		return [.. voices];
	}

	public async Task<string> GetCastAsync()
	{
		await Task.CompletedTask.ConfigureAwait(false);
		return "";
	}

	public async ValueTask SetCastAsync(string castName)
	{
		await Task.CompletedTask.ConfigureAwait(false);
	}
}
