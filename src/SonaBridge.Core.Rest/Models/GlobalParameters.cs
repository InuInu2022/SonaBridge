using System.ComponentModel.DataAnnotations;

using SonaBridge.Core.Rest.Internal.Models;

namespace SonaBridge.Core.Rest.Models;

public readonly record struct
GlobalParameters(
	[Range(-1, 1)]
	double? Alp,
	[Range(-20, 20)]
	double? Huskiness,
	[Range(0, 2)]
	double? Intonation,
	[Range(-600, 600)]
	double? Pitch,
	[Range(0.2, 5.0)]
	double? Speed,
	IList<double?>? StyleWeights,
	[Range(-8, 8)]
	double? Volume,
	IDictionary<string, object>? AdditionalData
);

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