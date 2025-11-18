using Microsoft.Extensions.Logging;

namespace SonaBridge.Core.Rest;

public partial class TalkRestService
{
	[LoggerMessage(
		Level = LogLevel.Warning,
		Message = "Cast not found: {CastName}")]
	partial void LogCastNotFound(string castName);

	//_logger.LogError($"Failed to get available casts.{ex.Message}");
	[LoggerMessage(
		Level = LogLevel.Error,
		Message = "Failed to get available casts: {Message}")]
	partial void LogFailedToGetAvailableCasts(string message);
}
