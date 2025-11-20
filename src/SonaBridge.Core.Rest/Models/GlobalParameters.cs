using System.ComponentModel.DataAnnotations;

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
