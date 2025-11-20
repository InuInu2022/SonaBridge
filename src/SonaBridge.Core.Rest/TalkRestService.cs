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
using SonaBridge.Core.Rest.Models;

using static SonaBridge.Core.Rest.Extension.WaitExtension;
using static SonaBridge.Core.Rest.Extension.SpeakResultExtensions;

namespace SonaBridge.Core.Rest;
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

	LanguageKey LastLanguage { get; set; }

	/// <summary>
	/// 音声ライブラリ内部名(e.g. "tanaka-san_ja_JP")をキーとする変換キャッシュテーブル
	/// </summary>
	static ConcurrentDictionary<VoiceNameKey, VoiceData> VoiceByName { get; set; } = [];
	/// <summary>
	/// 音声ライブラリ表示名(e.g. "田中傘")をキーとする変換キャッシュテーブル
	/// </summary>
	static ConcurrentDictionary<VoiceDisplayKey, VoiceNameKey> VoiceByDisplay { get; set; } = [];
	CastData LastCast { get; set; }

	readonly ILogger<TalkRestService> _logger;
	readonly RawTalkApi _client;



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
			BaseUrl = $"""http://localhost:{port}/api/talk/v1""",
		};
		LastLanguage = new(language);

		LastCast = new(new("tanaka-san_ja_JP"), "2.0.1", LastLanguage);

		_client = new RawTalkApi(Adapter);
		_logger = logger ?? NullLogger<TalkRestService>.Instance;
	}

	/// <summary>
	/// <inheritdoc/>
	/// </summary>
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

		//ライブラリ情報取得更新
		if (updateLibrary && !await instance.TryUpdateLibraryAsync())
		{
			instance._logger.LogWarning("Failed to update voice library.");
		}

		return instance;
	}

	/// <summary>
	/// <inheritdoc/>
	/// <see cref="LastLanguage"/>の設定変更で取得できる名称が異なります
	/// </summary>

	public async Task<string[]> GetAvailableCastsAsync()
	{
		VoicesGetResponse? result;
		try
		{
			result = await _client.Voices.GetAsync();
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
				.Find(x => x.Language == LastLanguage)?
				.Name
			)
			.Where(name => name is not null and not "")
			.Distinct(StringComparer.Ordinal)
			.OfType<string>()
			.ToArray() ?? [];
	}

	public Task<string> GetCastAsync()
	{
		return Task.FromResult(LastCast.Name.ToString());
	}

	public Task<ReadOnlyDictionary<string, double>> GetGlobalParamsAsync()
	{
		throw new NotImplementedException();
	}

	[Obsolete("REST APIではサポートされていません。")]
	public ValueTask<IReadOnlyList<string>> GetPresetsAsync(string voiceName)
	{
		throw new NotSupportedException("REST APIではサポートされていません。");
	}

	public Task<ReadOnlyDictionary<string, double>> GetStylesAsync(string voiceName)
	{
		throw new NotImplementedException();
	}

	public async Task<bool> OutputWaveToFileAsync(string text, string path)
	{
		try
		{
			var result = await _client.SpeechSyntheses.PostAndWaitAsync(
				new()
				{
					Text = text,
					ForceEnqueue = true,
					Destination = SpeechSynthesesPostRequestBody_destination.File,
					VoiceName = LastCast.Name.ToString(),
					VoiceVersion = LastCast.Version,
					Language = LastCast.Language.ToString(),
					OutputFilePath = path,
				},
				TimeSpan.FromMinutes(5),
				ctx: CancellationToken.None
			);
		}
		catch (Exception ex)
		{
			LogException(ex.Message); //TODO:better logging
			return false;
		}
		return true;
	}

	public ValueTask SetCastAsync(string castName)
	{
		if (VoiceByDisplay.TryGetValue(new(castName), out var id)
		&& VoiceByName.TryGetValue(id, out var cast))
		{
			LastCast = new(
				cast.VoiceName,
				cast.VoiceVersions.FirstOrDefault() ?? "2.0.0",
				LastLanguage
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

	[Obsolete("REST APIではサポートされていません。")]
	public ValueTask SetPresetsAsync(string voiceName, string presetName)
	{
		throw new NotSupportedException("REST APIではサポートされていません。");
	}

	public ValueTask SetStylesAsync(string voiceName, IDictionary<string, double> styles)
	{
		throw new NotImplementedException();
	}

	public async Task<bool> SpeakAsync(string text, CancellationToken? token = null)
	{
		try
		{
			var result = await ProcessSpeechSynthesisAsync(text, string.Empty, token);
		}
		catch (Exception ex)
		{
			LogException(ex.Message);	//TODO:better logging
			return false;
		}
		return true;
	}



	public async Task<SpeakResult> SpeakAsync(
		string text,
		string analyzedText,
		CancellationToken? token = null)
	{
		try
		{
			var result = await ProcessSpeechSynthesisAsync(text, analyzedText, token);
			return result?.ToSpeakResult(analyzedText) ?? default;
		}
		catch (Exception ex)
		{
			LogException(ex.Message); //TODO:better logging
			throw;
		}
	}

	async Task<Internal.SpeechSyntheses.Item.WithUuGetResponse?> ProcessSpeechSynthesisAsync(string text, string analyzedText, CancellationToken? token)
	{
		return await _client.SpeechSyntheses.PostAndWaitAsync(
			new()
			{
				Text = text,
				AnalyzedText = analyzedText,
				ForceEnqueue = true,
				Destination = SpeechSynthesesPostRequestBody_destination.Audio_device,
				VoiceName = LastCast.Name.ToString(),
				VoiceVersion = LastCast.Version,
				Language = LastCast.Language.ToString(),
			},
			TimeSpan.FromMinutes(5),
			ctx: token ?? CancellationToken.None
		);
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
			VoiceByDisplay.Clear();
			VoiceByName.Clear();
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
