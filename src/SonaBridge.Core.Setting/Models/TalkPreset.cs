using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace SonaBridge.Core.Setting.Models;

public partial record TalkPreset
{
	[JsonPropertyName("speaker")]
	public required string Speaker { get; set; }

	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	[JsonPropertyName("name")]
	public required string Name { get; set; }

	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	[JsonPropertyName("speed_ratio")]
	[Display(Name = "Speed")]
	public decimal? SpeedRatio { get; set; }

	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	[JsonPropertyName("c0_shift")]
	[Display(Name = "Volume")]
	public decimal? C0Shift { get; set; }

	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	[JsonPropertyName("log_f0_shift")]
	[Display(Name = "Pitch")]
	public decimal? LogF0Shift { get; set; }

	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	[JsonPropertyName("alpha_shift")]
	[Display(Name = "Alpha")]
	public decimal? AlphaShift { get; set; }

	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	[JsonPropertyName("log_f0_scale")]
	[Display(Name = "Into.")]
	public decimal? LogF0Scale { get; set; }

	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	[JsonPropertyName("husky_shift")]
	[Display(Name = "Hus.")]
	public decimal? HuskyShift { get; set; }

	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	[JsonPropertyName("style")]
	public TalkStyle[]? Style { get; set; }
}
