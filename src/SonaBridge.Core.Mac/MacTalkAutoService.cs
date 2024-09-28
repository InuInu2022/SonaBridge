
using SonaBridge.Core.Common;

namespace SonaBridge.Core.Mac;

public class MacTalkAutoService : ITalkAutoService
{
	public Task<string[]> GetAvailableCastsAsync()
	{
		throw new NotSupportedException();
	}

	public Task<bool> SpeakAsync(string text, CancellationToken? token = null)
	{
		throw new NotSupportedException();
	}
}
