using System.Diagnostics.CodeAnalysis;
using System.Xml.Linq;

using SonaBridge.Core.Rest.Internal.Models;
using SonaBridge.Core.Rest.Internal.SpeechSyntheses.Item;
using SonaBridge.Core.Rest.Models;

namespace SonaBridge.Core.Rest.Extension;

using SsBaseInfo = Speech_synthesis_base_information;

[SuppressMessage("Naming", "CA1708", Justification = "https://github.com/dotnet/sdk/issues/51716")]
public static class
SpeakResultExtensions
{
	extension(SpeakResult sResult)
	{
		public T Convert<T>()
			where T : SsBaseInfo
		{
			var result = new SsBaseInfo
			{
				Language = sResult.Language,
				OutputFilePath = sResult.OutputFilePath,
				ProgressPercentage = null,
				State = null,
				Text = sResult.Text,
				Uuid = Guid.NewGuid(),
			};

			return (T)result;
		}
	}

	extension(WithUuGetResponse result)
	{
		public SpeakResult ToSpeakResult(string analyzedText = "")
		{
			var str = analyzedText is ""
				? result?.AnalyzedText ?? string.Empty
				: analyzedText;

			return new(
				AnalyzedText: XDocument.Parse(str),
				Destination: result?.Destination?.ToPublic(),
				Duration: result?.Duration,
				GlobalParams: result?.GlobalParameters?.ToGlobalParameters(),
				Language: result?.Language,
				OutputFilePath: result?.OutputFilePath,
				Uuid: result?.Uuid,
				PhonemeDurations: result?.PhonemeDurations,
				Phonemes: result?.Phonemes,
				Text: result?.Text,
				VoiceName: result?.VoiceName,
				VoiceVersion: result?.VoiceVersion,
				AdditionalData: result?.AdditionalData
			);
		}
	}
}