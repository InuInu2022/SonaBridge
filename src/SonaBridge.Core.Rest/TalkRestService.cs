using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Kiota.Abstractions.Authentication;
using Microsoft.Kiota.Abstractions.Extensions;
using Microsoft.Kiota.Http.HttpClientLibrary;

using SonaBridge.Core.Common;
using SonaBridge.Core.Rest.Internal;
using SonaBridge.Core.Rest.Internal.SpeechSyntheses;
using SonaBridge.Core.Rest.Internal.Voices;
using SonaBridge.Core.Rest.Models;

using static SonaBridge.Core.Rest.Extension.GlobalParametersExtensions;
using static SonaBridge.Core.Rest.Extension.SpeakResultExtensions;
using static SonaBridge.Core.Rest.Extension.WaitExtension;

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

	/// <summary>
	/// 直近に使用した音声ライブラリの情報キャッシュ
	/// ボイスライブラリ毎にデータを持つ
	/// </summary>
	static ConcurrentDictionary<VoiceNameKey, CastData> LastCasts { get; set; } = [];

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

		AuthProvider = new(user, password);
		Adapter = new(AuthProvider)
		{
			BaseUrl = $"""http://localhost:{port}/api/talk/v1""",
		};
		LastLanguage = new(language);

		UpdateLastCast(new CastData(
			new("tanaka-san_ja_JP"),
			new("2.0.1"),
			LastLanguage,
			new()
		));

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

	/// <summary>
	/// TalkRestServiceのインスタンスを生成し、音声ライブラリ情報を取得します
	/// </summary>
	/// <param name="user">API認証用ユーザー名</param>
	/// <param name="password">API認証用パスワード</param>
	/// <param name="port">SonaBridge TalkのREST APIポート番号</param>
	/// <param name="language">使用する言語コード(e.g. "ja_JP")</param>
	/// <param name="updateLibrary">音声ライブラリ情報を最新の状態に更新するかどうか</param>
	/// <returns>初期化されたTalkRestServiceのインスタンス</returns>
	public static async Task<TalkRestService> StartAsync(
		string user,
		[DataType(DataType.Password)]
		string password,
		[Range(1, 65535)]
		int port = 32766,
		[RegularExpression(
			"""^[a-zA-Z]{2,3}([-_][a-zA-Z]{2,8})+$""",
			ErrorMessage = "形式が正しくありません。"
		)]
		string language = "ja_JP",
		bool updateLibrary = false
	)
	{
		var instance = new TalkRestService(user, password, port, language);

		//ライブラリ情報取得更新
		if (updateLibrary && !await instance.TryUpdateLibraryAsync())
		{
			instance.LogWarning("Failed to update voice library!");
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
		var name = GetVoiceDisplayName(LastCast.Name, LastLanguage);
		return Task.FromResult(name);
	}

	public Task<ReadOnlyDictionary<string, double>> GetGlobalParamsAsync()
	{
		var dict = LastCast.GlobalParameters.ToDictionary();
		return Task.FromResult(dict);
	}

	[Obsolete("REST APIではサポートされていません。")]
	public ValueTask<IReadOnlyList<string>> GetPresetsAsync(string voiceName)
	{
		throw new NotSupportedException("REST APIではサポートされていません。");
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "MA0002:IEqualityComparer<string> or IComparer<string> is missing", Justification = "<保留中>")]
	public async Task<ReadOnlyDictionary<string, double>> GetStylesAsync(string voiceName)
	{
		if (!VoiceByDisplay.TryGetValue(new(voiceName), out var voice)
		|| !VoiceByName.TryGetValue(voice, out var voiceData))
		{
			LogCastNotFound(voiceName);
			return new Dictionary<string, double>().AsReadOnly();
		}

		var result = await GetDefaultStylesCoreAsync(
			voiceData.VoiceName,
			voiceData.VoiceVersions.FirstOrDefault()
		);

		if (voiceData.StyleNames is null or { Count: 0 }
		|| !voiceData.StyleNames.ContainsKey(voiceData.VoiceVersions.FirstOrDefault()))
		{
			voiceData.StyleNames?.AddOrReplace(
				voiceData.VoiceVersions.FirstOrDefault(),
				[.. result?.StyleNames ?? []]
			);
		}


		var hasStyle = LastCast.GlobalParameters.StyleWeights?
			.SequenceEqual(result?.DefaultStyleWeights ?? []) == false;

		if (!hasStyle)
		{
			UpdateLastCast(LastCast with
			{
				GlobalParameters = new(StyleWeights: result?.DefaultStyleWeights ?? []),
			});
		}

		var weights = hasStyle
			? LastCast.GlobalParameters.StyleWeights!
			: result?.DefaultStyleWeights ?? [];
		var names = result?.StyleNames ?? [];
		return names
			.Zip(weights, (k, v) => (k, v: v ?? 0.0))
			.ToDictionary(x => x.k, x => x.v)
			.AsReadOnly();
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
					VoiceVersion = LastCast.Version.ToString(),
					Language = LastCast.Language.ToString(),
					OutputFilePath = path,
					CanOverwriteFile = true,
					GlobalParameters = LastCast.GlobalParameters.ToSsGp(),
				},
				TimeSpan.FromMinutes(5),
				ctx: CancellationToken.None
			);

			if (result is null){
				return false;
			}
		}
		catch (Exception ex)
		{
			LogException(ex.Message); //TODO:better logging
			return false;
		}

		return true;
	}

	public async ValueTask SetCastAsync(string castName)
	{
		if (VoiceByDisplay.TryGetValue(new(castName), out var id)
		&& VoiceByName.TryGetValue(id, out var cast))
		{
			if (LastCasts.TryGetValue(
				cast.VoiceName, out var existingCast))
			{
				UpdateLastCast(existingCast);
			}
			else
			{
				//キャッシュに無いなら初期Weight取得
				var result = await GetDefaultStyleWeightsAsync(
					cast.VoiceName,
					cast.VoiceVersions.FirstOrDefault()
				);
				var newCast = new CastData
				(
					cast.VoiceName,
					cast.VoiceVersions.FirstOrDefault(),
					LastLanguage,
					new(
						StyleWeights: result
					)
				);
				UpdateLastCast(newCast);
			}
		}
		else
		{
			LogCastNotFound(castName);
		}
	}


	public async ValueTask SetGlobalParamsAsync(IDictionary<string, double> globalParams)
	{
		//throw new NotImplementedException();

		var lastStyles = LastCast.GlobalParameters.StyleWeights
			?? await GetDefaultStyleWeightsAsync(
				LastCast.Name,
				LastCast.Version
			);

		_ = globalParams.TryGetValue("ALP", out var alpha);

		var param = new GlobalParameters(
			Alp: alpha,
			StyleWeights: lastStyles
		);

		UpdateLastCast(LastCast with
		{
			GlobalParameters = param,
		});

	}

	[Obsolete("REST APIではサポートされていません。")]
	public ValueTask SetPresetsAsync(string voiceName, string presetName)
	{
		throw new NotSupportedException("REST APIではサポートされていません。");
	}

	public ValueTask SetStylesAsync(string voiceName, IDictionary<string, double> styles)
	{
		if (
			VoiceByDisplay.TryGetValue(new(voiceName), out var id)
			&& VoiceByName.TryGetValue(id, out var cast)
		)
		{
			var x = styles
				.OrderBy(kv => kv.Key, StringComparer.Ordinal)
				.Select(static kv => kv.Value);
			//todo: 既存設定を上書きするようにする
			//VoiceDataにスタイルの名称順が記録されているのでそれに合わせてweightsを設定する
			//impl.csの方に共通関数化しておくべきかも
			UpdateLastCast(LastCast with
			{
				GlobalParameters = LastCast.GlobalParameters with
				{
					StyleWeights = [.. x],
				},
			});
		}
		else
		{
			LogCastNotFound(voiceName);
		}
		return ValueTask.CompletedTask;
	}

	public async Task<bool> SpeakAsync(string text, CancellationToken? token = null)
	{
		try
		{
			var result = await SpeakCoreAsync(text, string.Empty, token);
		}
		catch (Exception ex)
		{
			LogException(ex.Message);	//TODO:better logging
			return false;
		}
		return true;
	}


	/// <summary>
	/// 調整したパラメータで喋らせる
	/// 調整可能：読み、アクセント
	/// </summary>
	/// <param name="text"></param>
	/// <param name="analyzedText"></param>
	/// <param name="token"></param>
	/// <returns></returns>
	public async Task<SpeakResult> SpeakAsync(
		string text,
		string analyzedText,
		CancellationToken? token = null)
	{
		try
		{
			var result = await SpeakCoreAsync(text, analyzedText, token);
			return result?.ToSpeakResult(analyzedText) ?? default;
		}
		catch (Exception ex)
		{
			LogException(ex.Message); //TODO:better logging
			throw;
		}
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
			LastCasts.Clear();
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
