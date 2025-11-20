using SonaBridge.Core.Rest.Internal.Models;
using SonaBridge.Core.Rest.Models;

namespace SonaBridge.Core.Rest.Extension;

public static class GlobalParametersExtensions
{
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