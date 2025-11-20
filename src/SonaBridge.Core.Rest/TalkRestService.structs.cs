using System.ComponentModel.DataAnnotations;

namespace SonaBridge.Core.Rest;

public partial class TalkRestService
{
	/// <summary>
	/// 音声ライブラリ情報
	/// </summary>
	/// <param name="Name"></param>
	/// <param name="Version"></param>
	/// <param name="Language"></param>
	readonly record struct CastData(
		VoiceNameKey Name,
		string Version,
		LanguageKey Language
	);

	/// <summary>
	/// 音声ライブラリデータ
	/// </summary>
	/// <param name="VoiceName">音声ライブラリ内部名</param>
	/// <param name="VoiceVersions">音声ライブラリバージョン一覧
	/// (e.g. ["1.0.0", "1.1.0"])</param>
	/// <param name="Languages">音声ライブラリが対応している言語一覧
	/// キー: バージョン、値: 言語コード一覧(e.g. ["ja_JP", "en_US"])</param>
	/// <param name="DisplayNames">音声ライブラリ表示名一覧
	/// キー: 言語コード、値: 表示名(e.g. "田中傘")
	/// </param>
	/// <seealso cref="VoiceByName"/>
	readonly record struct VoiceData(
		VoiceNameKey VoiceName,
		string[] VoiceVersions,
		Dictionary<string, LanguageKey[]> Languages,
		Dictionary<LanguageKey, string> DisplayNames
	);

	/// <summary>
	/// 音声ライブラリ表示名(e.g. "田中傘")をキーとする変換キャッシュテーブルのキー
	/// </summary>
	/// <param name="VoiceDisplay">音声ライブラリ表示名(e.g. "田中傘")</param>
	///  <seealso cref="VoiceByDisplay"/>
	readonly record struct VoiceDisplayKey(string VoiceDisplay)
	{
		public override string ToString() => VoiceDisplay;
	}
	/// <summary>
	/// 音声ライブラリ内部名(e.g. "tanaka-san_ja_JP")をキーとする変換キャッシュテーブルのキー
	/// </summary>
	/// <param name="VoiceName">音声ライブラリ内部名(e.g. "tanaka-san_ja_JP")</param>
	/// <seealso cref="VoiceByName"/>
	readonly record struct VoiceNameKey(
		[RegularExpression("^[a-zA-Z0-9_-]{7,}$")]
		string VoiceName
	)
	{
		public override string ToString() => VoiceName;
	}

	readonly record struct LanguageKey(
		[RegularExpression("""^[a-zA-Z]{2,3}([-_][a-zA-Z]{2,8}){1,2}$""")]
		string Language
	)
	{
		public override string ToString() => Language;

		public static bool operator ==(LanguageKey left, string? right) =>
			string.Equals(left.Language, right, StringComparison.Ordinal);

		public static bool operator !=(LanguageKey left, string? right) =>
			!string.Equals(left.Language, right, StringComparison.Ordinal);

		public static bool operator ==(string? left, LanguageKey right) =>
			string.Equals(left, right.Language, StringComparison.Ordinal);

		public static bool operator !=(string? left, LanguageKey right) =>
			!string.Equals(left, right.Language, StringComparison.Ordinal);
	}
}
