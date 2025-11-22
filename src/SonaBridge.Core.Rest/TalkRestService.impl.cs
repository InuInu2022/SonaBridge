using System.Linq;
using SonaBridge.Core.Rest.Extension;
using SonaBridge.Core.Rest.Internal.SpeechSyntheses;

namespace SonaBridge.Core.Rest;

public partial class TalkRestService
{
	async ValueTask<bool> TryUpdateLibraryAsync()
	{
		var result = await _client.Voices.GetAsync().ConfigureAwait(false);

		if (result is not { } data)
		{
			return false;
		}

		data.Items?.ForEach(static x =>
		{
			if (x.VoiceName is null)
				return;

			if (x.VoiceVersion is null)
				return;

			//VoiceByNameの更新
			AddOrUpdateVoiceByName(x);

			//VoiceByDisplayの更新
			AddOrUpdateVoiceByDisplay(x);
		});

		return true;
	}

	static void AddOrUpdateVoiceByName(
		Internal.Models.Voice_base_information vInfo)
	{
		if(vInfo.VoiceName is null) return;
		if(vInfo.VoiceVersion is null) return;

		var appendVoice = new VoiceData(
			VoiceName: new(vInfo.VoiceName),
			VoiceVersions: [new(vInfo.VoiceVersion)],
			Languages: vInfo.Languages is { } langs
				? new()
				{
						{ new VersionKey(vInfo.VoiceVersion),
						[.. langs.Select(ln => new LanguageKey(ln))] },
				}
				: [],
			DisplayNames: vInfo.DisplayName?.Where(v => v is { Name: not null, Language: not null })
				.ToList()
				.ToDictionary(v => new LanguageKey(v.Language!), v => v.Name!) ?? [],
			StyleNames: []
		);

		_ = VoiceByName.AddOrUpdate(
			new(vInfo.VoiceName),
			addValueFactory: _ => appendVoice,
			updateValueFactory: (_, oldValue) =>
			{
				var versions = vInfo.VoiceVersion is { } newVer
					? [.. oldValue.VoiceVersions, new VersionKey(newVer)]
					: oldValue.VoiceVersions;
				if (vInfo.Languages is not null)
				{
					oldValue.Languages.TryAdd(
						new(vInfo.VoiceVersion),
						[.. vInfo.Languages.Select(l => new LanguageKey(l))]
					);
				}
				return oldValue with
				{
					VoiceVersions = [.. versions.Distinct()],
				};
			}
		);
	}

	static void AddOrUpdateVoiceByDisplay(
		Internal.Models.Voice_base_information vInfo)
	{
		if (vInfo.DisplayName?.FirstOrDefault()?.Name is not { } name) return;
		if (vInfo.VoiceName is not { } vName) return;

		_ = VoiceByDisplay.AddOrUpdate(
			new(name),
			addValueFactory: _ => new(vName),
			updateValueFactory: (_, oldValue) => oldValue
		);
	}

	/// <summary>
	/// 音声ライブラリ表示名を取得します
	/// </summary>
	/// <param name="voiceNameKey">音声ライブラリ内部名キー</param>
	/// <param name="language">言語キー</param>
	/// <returns>音声ライブラリ表示名</returns>
	static string GetVoiceDisplayName(
		VoiceNameKey voiceNameKey,
		LanguageKey? language = null
	)
	{
		language ??= new("ja_JP");
		return language is not LanguageKey lang
			? string.Empty
			: VoiceByName.TryGetValue(voiceNameKey, out var voiceData)
			&& voiceData.DisplayNames.TryGetValue(lang, out var castName)
				? castName
				: string.Empty;
	}

	/// <summary>
	/// 喋りを実行する内部関数
	/// 音声は出力デバイスから再生されます
	/// </summary>
	/// <param name="text">合成対象テキスト</param>
	/// <param name="analyzedText">言語解析済みテキスト</param>
	/// <param name="token">キャンセルトークン</param>
	/// <seealso cref="SpeakAsync(string, CancellationToken?)"/>
	/// <seealso cref="SpeakAsync(string, string, CancellationToken?)"/>
	async Task<Internal.SpeechSyntheses.Item.WithUuGetResponse?> SpeakCoreAsync(
		string text,
		string analyzedText,
		CancellationToken? token
	) => await _client.SpeechSyntheses.PostAndWaitAsync(
		new()
		{
			Text = text,
			AnalyzedText = analyzedText,
			ForceEnqueue = true,
			//音声デバイスから
			Destination = SpeechSynthesesPostRequestBody_destination.Audio_device,
			VoiceName = LastCast.Name.ToString(),
			VoiceVersion = LastCast.Version.ToString(),
			Language = LastCast.Language.ToString(),
			GlobalParameters = LastCast.GlobalParameters.ToSsGp(),
		},
		TimeSpan.FromMinutes(5),
		ctx: token ?? CancellationToken.None
	);
}
