﻿using System.Diagnostics;

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
		throw new NotSupportedException();
		//return "";
	}

	public async ValueTask SetCastAsync(string castName)
	{
		await GetAppWindowAsync().ConfigureAwait(false);
		var result = await SetVoiceAsync(castName).ConfigureAwait(false);
		if(!result){
			throw new InvalidOperationException("FlaUI operation error!");
		}
	}

	public async Task<bool> OutputWaveToFileAsync(string text, string path)
	{
		await GetAppWindowAsync().ConfigureAwait(false);
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
			return false;
		}

		return true;
	}
}
