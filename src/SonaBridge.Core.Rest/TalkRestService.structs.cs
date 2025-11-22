using System.ComponentModel.DataAnnotations;
using System.Runtime.InteropServices;

using SonaBridge.Core.Rest.Models;

namespace SonaBridge.Core.Rest;

public partial class TalkRestService
{
	/// <summary>
	/// 音声ライブラリ情報
	/// </summary>
	/// <param name="Name"></param>
	/// <param name="Version"></param>
	/// <param name="Language"></param>
	/// <param name="GlobalParameters"></param>
	[StructLayout(LayoutKind.Auto)]
	readonly record struct CastData(
		VoiceNameKey Name,
		VersionKey Version,
		LanguageKey Language,
		GlobalParameters GlobalParameters
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
	/// <param name="StyleNames">音声スタイル名一覧
	/// キー: バージョン、値: スタイル表示名(e.g. "元気な")の一覧</param>
	/// <seealso cref="VoiceByName"/>
	readonly record struct VoiceData(
		VoiceNameKey VoiceName,
		VersionKey[] VoiceVersions,
		Dictionary<VersionKey, LanguageKey[]> Languages,
		Dictionary<LanguageKey, string> DisplayNames,
		Dictionary<VersionKey, string[]>? StyleNames
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

	readonly record struct VersionKey(string VersionString)
	{
		public override string ToString() => VersionString;
		public Version ToVersion() =>
			Version.TryParse(VersionString, out var version)
				? version : new Version(0, 0);
	};
}
