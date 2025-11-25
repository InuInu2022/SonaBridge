using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

using SonaBridge.Core.Common;
using SonaBridge.Core.Common.Models;
using SonaBridge.Core.Rest.Models;
using SonaBridge.Core.Setting;

namespace SonaBridge.Core.Win;

public partial class WinTalkAutoService : ITalkAutoService
{
	public bool UseClassic { get; set; }

	public WinTalkAutoService()
	{
	}

	[Obsolete("Use StartAsync with parameters.")]
	public async Task StartAsync()
	{
		await InternalStartUpAsync().ConfigureAwait(false);
	}

	public async Task StartAsync(
		string userName,
		string password,
		int port = 32766,
		bool useClassic = false
	)
	{
		UseClassic = useClassic;
		if (UseClassic)
		{
			await InternalStartUpAsync().ConfigureAwait(false);
		}
		await InitAsync(userName, password, port)
			.ConfigureAwait(false);
	}

	/// <inheritdoc/>
	/// <seealso cref="SpeakAsync(string, string, CancellationToken?)"/>
	public async Task<bool> SpeakAsync(
		string text,
		CancellationToken? token = null)
	{
		if (!UseClassic)
		{
			return await Service.SpeakAsync(text, token)
				.ConfigureAwait(false);
		}
		else
		{
			await GetAppWindowAsync().ConfigureAwait(false);
			WinCommon.SaveMousePoint();
			WinCommon.MoveMouseCorner();
			await SetUtterance(text).ConfigureAwait(false);
			await PlayUtterance(token).ConfigureAwait(false);
			await WinCommon.RestoreMousePointAsync().ConfigureAwait(false);

			return true;
		}
	}

	public async Task<SpeakResult> SpeakAsync(
		string text,
		string analyzedText,
		CancellationToken? token = null)
	{
		if (!UseClassic)
		{
			return await Service.SpeakAsync(text, analyzedText, token)
				.ConfigureAwait(false);
		}
		else
		{
			throw new NotSupportedException();
		}
	}

	public async Task<string[]> GetAvailableCastsAsync()
	{
		if (!UseClassic)
		{
			return await Service.GetAvailableCastsAsync()
				.ConfigureAwait(false);
		}
		else
		{
			await GetAppWindowAsync().ConfigureAwait(false);
			WinCommon.SaveMousePoint();
			WinCommon.MoveMouseCorner();
			var voices = await GetVoiceNames().ConfigureAwait(false);
			await WinCommon.RestoreMousePointAsync().ConfigureAwait(false);
			return [.. voices];
		}

	}

	public async Task<string> GetCastAsync()
	{
		if (!UseClassic)
		{
			return await Service.GetCastAsync()
				.ConfigureAwait(false);
		}
		else
		{
			await Task.CompletedTask.ConfigureAwait(false);
			throw new NotSupportedException();
		}
	}

	public async ValueTask SetCastAsync(string castName)
	{
		if (!UseClassic)
		{
			await Service.SetCastAsync(castName)
				.ConfigureAwait(false);
		}
		else
		{
			await GetAppWindowAsync().ConfigureAwait(false);
			WinCommon.SaveMousePoint();
			WinCommon.MoveMouseCorner();
			var result = await SetVoiceAsync(castName).ConfigureAwait(false);
			await WinCommon.RestoreMousePointAsync().ConfigureAwait(false);
			if (!result)
			{
				throw new InvalidOperationException(
					$"{nameof(SetCastAsync)}({castName}): FlaUI operation error!"
				);
			}
		}
	}

	public async Task<bool> OutputWaveToFileAsync(string text, string path)
	{
		if (!UseClassic)
		{
			return await Service.OutputWaveToFileAsync(text, path)
				.ConfigureAwait(false);
		}
		else
		{
			await GetAppWindowAsync().ConfigureAwait(false);
			WinCommon.SaveMousePoint();
			WinCommon.MoveMouseCorner();
			await SetUtterance(text).ConfigureAwait(false);

			Debug.WriteLine($"output wav file: {path}");

			try
			{
				await SaveWavAsync(path).ConfigureAwait(false);
			}
			catch (System.Exception e)
			{
				Console.Error.WriteLine(e.Message);
				await WinCommon.RestoreMousePointAsync().ConfigureAwait(false);
				return false;
			}
			finally
			{
				await WinCommon.RestoreMousePointAsync().ConfigureAwait(false);
			}

			return true;
		}

	}

	public async Task<ReadOnlyDictionary<string, double>> GetGlobalParamsAsync()
	{
		if (!UseClassic)
		{
			return await Service.GetGlobalParamsAsync()
				.ConfigureAwait(false);
		}
		else
		{
			await GetAppWindowAsync().ConfigureAwait(false);
			return await GetCurrentGlobalParamAsync()
				.ConfigureAwait(false);
		}
	}

	public async ValueTask SetGlobalParamsAsync(IDictionary<string, double> globalParams)
	{
		if (!UseClassic)
		{
			await Service.SetGlobalParamsAsync(globalParams)
				.ConfigureAwait(false);
		}
		else
		{
			await GetAppWindowAsync().ConfigureAwait(false);
			await SetCurrentGlobalParamsAsync(globalParams).ConfigureAwait(false);
		}
	}

	public async Task<ReadOnlyDictionary<string, double>>
	GetStylesAsync(string voiceName)
	{
		if (!UseClassic)
		{
			return await Service.GetStylesAsync(voiceName)
				.ConfigureAwait(false);
		}
		else
		{
			await GetAppWindowAsync().ConfigureAwait(false);
			WinCommon.SaveMousePoint();
			WinCommon.MoveMouseCorner();
			var result = await GetCurrentStylesAsync(voiceName).ConfigureAwait(false);
			await WinCommon.RestoreMousePointAsync().ConfigureAwait(false);
			return result;
		}
	}

	public async ValueTask SetStylesAsync(string voiceName, IDictionary<string, double> styles)
	{
		if (!UseClassic)
		{
			await Service.SetStylesAsync(voiceName, styles)
				.ConfigureAwait(false);
		}
		else
		{
			await GetAppWindowAsync().ConfigureAwait(false);
			WinCommon.SaveMousePoint();
			WinCommon.MoveMouseCorner();
			await SetCurrentStylesAsync(voiceName, styles).ConfigureAwait(false);
			await WinCommon.RestoreMousePointAsync().ConfigureAwait(false);
		}

	}

	public async ValueTask<IReadOnlyList<string>> GetPresetsAsync(string voiceName)
	{
		if (!UseClassic)
		{
			var presets = await TalkPresetService.LoadAsync()
				.ConfigureAwait(false);

			if (presets is null)
			{
				return [];
			}
			else
			{
				return [.. presets!
					.Where(p => string.Equals(
						p.Speaker,
						voiceName,
						StringComparison.OrdinalIgnoreCase))
					.Select(p => p.Name),
				];
			}
		}
		else
		{
			await GetAppWindowAsync().ConfigureAwait(false);
			await SetCastAsync(voiceName).ConfigureAwait(false);
			return await GetCurrentPresets().ConfigureAwait(false);
		}
	}

	public async ValueTask SetPresetsAsync(string voiceName, string presetName)
	{
		if (!UseClassic)
		{

		}
		else
		{
			await GetAppWindowAsync().ConfigureAwait(false);
			await SetCastAsync(voiceName).ConfigureAwait(false);
			await SetCurrentPreset(presetName).ConfigureAwait(false);
		}
	}

	public async Task<ReadOnlyCollection<PhonemeData>> GetPhonemesAsync(string text)
	{
		return !UseClassic
			? await Service.GetPhonemesAsync(text)
				.ConfigureAwait(false)
			: throw new NotSupportedException();
	}
}
