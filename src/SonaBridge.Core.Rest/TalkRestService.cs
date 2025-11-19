using System.Collections.Concurrent;
using System.Collections.ObjectModel;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Kiota.Abstractions.Authentication;
using Microsoft.Kiota.Http.HttpClientLibrary;
using SonaBridge.Core.Common;
using SonaBridge.Core.Rest.Internal;
using SonaBridge.Core.Rest.Internal.SpeechSyntheses;
using SonaBridge.Core.Rest.Internal.Voices;
using static SonaBridge.Core.Rest.Extension.WaitExtension;

namespace SonaBridge.Core.Rest;

using CastData = (string Name, string Version, string Language);

[System.Diagnostics.CodeAnalysis.SuppressMessage(
	"Usage",
	"MA0004:Use Task.ConfigureAwait",
	Justification = "<保留中>"
)]
public partial class TalkRestService : ITalkAutoService, IRestAutoService
{
	bool _disposedValue;

	BasicAuthenticationProvider AuthProvider { get; set; }
	HttpClientRequestAdapter Adapter { get; set; }
	RawTalkApi Client { get; set; }
	string Language { get; set; }

	ConcurrentDictionary<string, VoiceData> VoiceByName { get; set; } = [];
	ConcurrentDictionary<string, string> VoiceByDisplay { get; set; } = [];

	CastData LastCast { get; set; }

	readonly ILogger<TalkRestService> _logger;

	readonly record struct VoiceData(
		string VoiceName,
		string[] VoiceVersions,
		Dictionary<string, string[]> Languages,
		Dictionary<string, string> DisplayNames
	);

	public TalkRestService(
		string user,
		string password,
		int port = 32766,
		string language = "ja_JP",
		ILogger<TalkRestService>? logger = null
	)
	{
		ArgumentException.ThrowIfNullOrEmpty(user);
		ArgumentException.ThrowIfNullOrEmpty(password);
		ArgumentException.ThrowIfNullOrEmpty(language);

		AuthProvider = new BasicAuthenticationProvider(user, password);
		Adapter = new HttpClientRequestAdapter(AuthProvider)
		{
			BaseUrl = $"http://localhost:{port}/api/talk/v1",
		};
		Language = language;

		LastCast = ("tanaka-san_ja_JP", "2.0.1", language);

		Client = new RawTalkApi(Adapter);
		_logger = logger ?? NullLogger<TalkRestService>.Instance;
	}

	/// <seealso cref="StartAsync(string, string, int, string, bool)"/>
	[Obsolete($"use {nameof(StartAsync)} method.")]
	public async Task StartAsync()
	{
		throw new NotSupportedException();
	}

	public static async Task<TalkRestService> StartAsync(
		string user,
		string password,
		int port = 32766,
		string language = "ja_JP",
		bool updateLibrary = false
	)
	{
		var instance = new TalkRestService(user, password, port, language);

		//TODO: ライブラリ情報取得更新
		if (updateLibrary && !await instance.TryUpdateLibraryAsync())
		{
			instance._logger.LogWarning("Failed to update voice library.");
		}

		return instance;
	}

	/// <summary>
	/// <inheritdoc/>
	/// <see cref="Language"/>の設定変更で取得できる名称が異なります
	/// </summary>

	public async Task<string[]> GetAvailableCastsAsync()
	{
		VoicesGetResponse? result;
		try
		{
			result = await Client.Voices.GetAsync();
		}
		catch (Exception ex)
		{
			LogFailedToGetAvailableCasts(ex.Message);
			return [];
		}
		if (result is null)
			return [];

		return result
			.Items?
			.Select(v => v.DisplayName?
				.Find(x => string.Equals(
					x.Language,
					Language,
					StringComparison.Ordinal))?
				.Name
			)
			.Where(name => name is not null and not "")
			.Distinct(StringComparer.Ordinal)
			.OfType<string>()
			.ToArray() ?? [];
	}

	public Task<string> GetCastAsync()
	{
		return Task.FromResult(LastCast.Name);
	}

	public Task<ReadOnlyDictionary<string, double>> GetGlobalParamsAsync()
	{
		throw new NotImplementedException();
	}

	[Obsolete("REST APIではサポートされていません。")]
	public ValueTask<IReadOnlyList<string>> GetPresetsAsync(string voiceName)
	{
		throw new NotImplementedException();
	}

	public Task<ReadOnlyDictionary<string, double>> GetStylesAsync(string voiceName)
	{
		throw new NotImplementedException();
	}

	public async Task<bool> OutputWaveToFileAsync(string text, string path)
	{
		try
		{
			var result = await Client.SpeechSyntheses.PostAndWaitAsync(
				new()
				{
					Text = text,
					ForceEnqueue = true,
					Destination = SpeechSynthesesPostRequestBody_destination.File,
					VoiceName = LastCast.Name,
					VoiceVersion = LastCast.Version,
					Language = LastCast.Language,
					OutputFilePath = path,
				},
				TimeSpan.FromMinutes(5),
				ctx: CancellationToken.None
			);
		}
		catch (Exception ex)
		{
			_logger.LogWarning("Exception: {Message}", ex.Message);
			return false;
		}
		return true;
	}

	public ValueTask SetCastAsync(string castName)
	{
		if (VoiceByDisplay.TryGetValue(castName, out var id)
		&& VoiceByName.TryGetValue(id, out var cast))
		{
			LastCast = (
				castName,
				cast.VoiceVersions.FirstOrDefault() ?? "2.0.0",
				Language
			);
		}
		else
		{
			LogCastNotFound(castName);
		}
		return ValueTask.CompletedTask;
	}

	public ValueTask SetGlobalParamsAsync(IDictionary<string, double> globalParams)
	{
		throw new NotImplementedException();
	}

	public ValueTask SetPresetsAsync(string voiceName, string presetName)
	{
		throw new NotImplementedException();
	}

	public ValueTask SetStylesAsync(string voiceName, IDictionary<string, double> styles)
	{
		throw new NotImplementedException();
	}

	public async Task<bool> SpeakAsync(string text, CancellationToken? token = null)
	{
		try
		{
			var result = await Client.SpeechSyntheses.PostAndWaitAsync(
				new()
				{
					Text = text,
					ForceEnqueue = true,
					Destination = SpeechSynthesesPostRequestBody_destination.Audio_device,
					VoiceName = LastCast.Name,
					VoiceVersion = LastCast.Version,
					Language = LastCast.Language,
				},
				TimeSpan.FromMinutes(5),
				ctx: token ?? CancellationToken.None
			);
		}
		catch (System.Exception ex)
		{
			_logger.LogWarning("Exception: {Message}", ex.Message);
			return false;
		}
		return true;
	}

	protected virtual void Dispose(bool disposing)
	{
		if (!_disposedValue)
		{
			if (disposing)
			{
				//マネージド状態を破棄します (マネージド オブジェクト)
				Adapter?.Dispose();
			}

			// TODO: アンマネージド リソース (アンマネージド オブジェクト) を解放し、ファイナライザーをオーバーライドします
			// TODO: 大きなフィールドを null に設定します
			_disposedValue = true;
		}
	}

	// // TODO: 'Dispose(bool disposing)' にアンマネージド リソースを解放するコードが含まれる場合にのみ、ファイナライザーをオーバーライドします
	// ~TalkRestService()
	// {
	//     // このコードを変更しないでください。クリーンアップ コードを 'Dispose(bool disposing)' メソッドに記述します
	//     Dispose(disposing: false);
	// }

	public void Dispose()
	{
		// このコードを変更しないでください。クリーンアップ コードを 'Dispose(bool disposing)' メソッドに記述します
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}
}
