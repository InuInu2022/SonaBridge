using System.Collections.ObjectModel;
using System.Diagnostics;

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
		WinCommon.SaveMousePoint();
		await SetUtterance(text).ConfigureAwait(false);
		await PlayUtterance(token).ConfigureAwait(false);
		await WinCommon.RestoreMousePointAsync().ConfigureAwait(false);

		return true;
	}

	public async Task<string[]> GetAvailableCastsAsync()
	{
		await GetAppWindowAsync().ConfigureAwait(false);
		WinCommon.SaveMousePoint();
		var voices = await GetVoiceNames().ConfigureAwait(false);
		await WinCommon.RestoreMousePointAsync().ConfigureAwait(false);
		return [.. voices];
	}

	public async Task<string> GetCastAsync()
	{
		await Task.CompletedTask.ConfigureAwait(false);
		throw new NotSupportedException();
		//return "";
	}

	public async ValueTask SetCastAsync(string castName)
	{
		await GetAppWindowAsync().ConfigureAwait(false);
		WinCommon.SaveMousePoint();
		var result = await SetVoiceAsync(castName).ConfigureAwait(false);
		await WinCommon.RestoreMousePointAsync().ConfigureAwait(false);
		if(!result){
			throw new InvalidOperationException("FlaUI operation error!");
		}
	}

	public async Task<bool> OutputWaveToFileAsync(string text, string path)
	{
		await GetAppWindowAsync().ConfigureAwait(false);
		WinCommon.SaveMousePoint();
		await SetUtterance(text).ConfigureAwait(false);

		Debug.WriteLine($"output wav file: {path}");

		try
		{
			await SaveWavAsync(path)
				.ConfigureAwait(false);
		}
		catch (System.Exception e)
		{
			Console.Error.WriteLine(e.Message);
			await WinCommon.RestoreMousePointAsync().ConfigureAwait(false);
			return false;
		}
		await WinCommon.RestoreMousePointAsync().ConfigureAwait(false);

		return true;
	}

	public async Task<ReadOnlyDictionary<string, double>> GetGlobalParamsAsync()
	{
		await GetAppWindowAsync().ConfigureAwait(false);
		return await GetCurrentGlobalParamAsync()
			.ConfigureAwait(false);
	}

	public async ValueTask SetGlobalParamsAsync(IDictionary<string, double> globalParams)
	{
		await GetAppWindowAsync().ConfigureAwait(false);
		await SetCurrentGlobalParamsAsync(globalParams)
			.ConfigureAwait(false);
	}

	public async Task<ReadOnlyDictionary<string, double>>
	GetStylesAsync(string voiceName)
	{
		await GetAppWindowAsync().ConfigureAwait(false);
		WinCommon.SaveMousePoint();
		var result = await GetCurrentStylesAsync(voiceName).ConfigureAwait(false);
		await WinCommon.RestoreMousePointAsync().ConfigureAwait(false);
		return result;
	}

	public async ValueTask SetStylesAsync(string voiceName, IDictionary<string, double> styles)
	{
		await GetAppWindowAsync().ConfigureAwait(false);
		WinCommon.SaveMousePoint();
		await SetCurrentStylesAsync(voiceName, styles).ConfigureAwait(false);
		await WinCommon.RestoreMousePointAsync().ConfigureAwait(false);
	}
}
