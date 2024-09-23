namespace SonaBridge.Core.Common;

public interface ITalkAutoService : IAutoService
{
	Task<bool> SpeakAsync(
		string text,
		CancellationToken? token = default
	);
}
