using System.Text.Json.Serialization;

namespace SonaBridge.Core.Setting.Models;

public partial record TalkStyle
{
	[JsonPropertyName("name")]
	public required string Name { get; set; }

	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	[JsonPropertyName("weight")]
	public decimal? Weight { get; set; }
}
