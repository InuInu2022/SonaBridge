using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace SonaBridge.Core.Rest.Models;

[Obfuscation(Exclude = true, ApplyToMembers = true)]

public enum Destination
{
	AudioDevice,
	File,
	[SuppressMessage("Usage", "SMA0027:Unusual Enum Definition", Justification = "<保留中>")]
	Unknown = 999,
}
