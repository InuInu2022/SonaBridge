using System.Diagnostics.CodeAnalysis;

using SonaBridge.Core.Rest.Internal.Models;
using SonaBridge.Core.Rest.Models;

namespace SonaBridge.Core.Rest.Extension;

[SuppressMessage("Naming", "CA1708", Justification = "https://github.com/dotnet/sdk/issues/51716")]

public static class GlobalParametersExtensions
{

	extension(GlobalParameters gParams)
	{
		public Speech_synthesis_global_parameters ToSsGp()
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