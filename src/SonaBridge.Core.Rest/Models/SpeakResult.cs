using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Xml.Linq;

namespace SonaBridge.Core.Rest.Models;

/// <summary>
/// Speak Result Data
/// </summary>
/// <param name="AnalyzedText">tsml</param>
/// <param name="Destination">出力先</param>
/// <param name="Duration">合成音声の長さ（秒）</param
/// ><param name="GlobalParams">グローバルパラメータ</param>
/// <param name="Language">Language (e.g. "ja_JP")</param>
/// <param name="OutputFilePath">出力ファイルパス</param>
/// <param name="Uuid">一意な識別子</param>
/// <param name="PhonemeDurations">音素ごとの長さ（秒）</param>
/// <param name="Phonemes">音素列</param>
/// <param name="Text">合成対象テキスト</param>
/// <param name="VoiceName">音声名</param>
/// <param name="VoiceVersion">音声バージョン</param>
/// <param name="AdditionalData">追加データ</param>
public readonly record struct
SpeakResult(
	XDocument AnalyzedText,
	[EnumDataType(typeof(Destination))]
	Destination? Destination,
	[Range(0.0, double.MaxValue)]
	double? Duration,
	GlobalParameters? GlobalParams,
	string? Language,
	string? OutputFilePath,
	Guid? Uuid,
	IList<double?>? PhonemeDurations,
	IList<string>? Phonemes,
	[MaxLength(500)]
	[UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "MaxLength on string is trim-safe")]
	string? Text,
	string? VoiceName,
	string? VoiceVersion,
	IDictionary<string, object>? AdditionalData
);