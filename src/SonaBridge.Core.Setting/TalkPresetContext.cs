using System.Text.Json.Serialization;

using SonaBridge.Core.Setting.Models;

namespace SonaBridge.Core.Setting;

[JsonSerializable(typeof(TalkPreset[]))]
[JsonSourceGenerationOptions(WriteIndented = true)]
public partial class TalkPresetContext : JsonSerializerContext;
