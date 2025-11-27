using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;

using SonaBridge.Core.Rest.Internal.Models;
using SonaBridge.Core.Rest.Models;

namespace SonaBridge.Core.Rest.Extension;

[SuppressMessage("Naming", "CA1708", Justification = "https://github.com/dotnet/sdk/issues/51716")]

public static class GlobalParametersExtensions
{

	extension(GlobalParameters gParams)
	{
		public Speech_synthesis_global_parameters
		ToSsGp()
		{
			return new Speech_synthesis_global_parameters
			{
				Alp = gParams.Alp,
				Huskiness = gParams.Huskiness,
				Intonation = gParams.Intonation,
				Pitch = gParams.Pitch,
				Speed = gParams.Speed,
				StyleWeights = gParams.StyleWeights?.ToList(),
				Volume = gParams.Volume,
				AdditionalData = gParams.AdditionalData,
			};
		}

		public ReadOnlyDictionary<string, double>
		ToDictionary()
		{
			var dict = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);

			if (gParams.Alp.HasValue)
			{
				dict["Alpha"] = gParams.Alp.Value;
			}
			if (gParams.Huskiness.HasValue)
			{
				dict["Hus."] = gParams.Huskiness.Value;
			}
			if (gParams.Intonation.HasValue)
			{
				dict["Into."] = gParams.Intonation.Value;
			}
			if (gParams.Pitch.HasValue)
			{
				dict["Pitch"] = gParams.Pitch.Value;
			}
			if (gParams.Speed.HasValue)
			{
				dict["Speed"] = gParams.Speed.Value;
			}
			if (gParams.Volume.HasValue)
			{
				dict["Volume"] = gParams.Volume.Value;
			}

			return new ReadOnlyDictionary<string, double>(dict);
		}
	}
	extension(Speech_synthesis_global_parameters ssParams)
	{
		/// <summary>
		/// Convert to GlobalParameters
		/// </summary>
		/// <returns></returns>
		public GlobalParameters ToGlobalParameters()
		{
			return new GlobalParameters(
				ssParams.Alp,
				ssParams.Huskiness,
				ssParams.Intonation,
				ssParams.Pitch,
				ssParams.Speed,
				ssParams.StyleWeights,
				ssParams.Volume,
				ssParams.AdditionalData
			);
		}
	}
}