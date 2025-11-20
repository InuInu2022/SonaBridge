using System.ComponentModel.DataAnnotations;

namespace SonaBridge.Core.Rest.Models;

public readonly record struct
GlobalParameters(
	[Range(-1, 1)]
	double? Alp = 0,
	[Range(-20, 20)]
	double? Huskiness = 0,
	[Range(0, 2)]
	double? Intonation = 1,
	[Range(-600, 600)]
	double? Pitch = 0,
	[Range(0.2, 5.0)]
	double? Speed = 1,
	IList<double?>? StyleWeights = null,
	[Range(-8, 8)]
	double? Volume = 0,
	IDictionary<string, object>? AdditionalData = null
);
