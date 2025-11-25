using System.Diagnostics;
using System.Text.Json;
using SonaBridge.Core.Setting.Models;

namespace SonaBridge.Core.Setting;

public static class TalkPresetService
{
	static string PresetPath => Path.Combine(
		Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
		"Techno-Speech",
		"VoiSona Talk",
		"Talker",
		"preset.json"
	);

	public static async Task<TalkPreset[]?> LoadAsync()
	{
		try
		{
			var utf8Bytes = await File
				.ReadAllBytesAsync(PresetPath)
				.ConfigureAwait(false);
			return JsonSerializer.Deserialize(
				utf8Bytes,
				TalkPresetContext.Default.TalkPresetArray
			);
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Failed to load talk presets. {ex.Message}");
			return default;
		}
	}
}
