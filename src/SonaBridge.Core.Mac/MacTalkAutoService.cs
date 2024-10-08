
using SonaBridge.Core.Common;

namespace SonaBridge.Core.Mac;

public class MacTalkAutoService : ITalkAutoService
{
	public Task<string[]> GetAvailableCastsAsync()
	{
		throw new NotSupportedException();
	}

	public Task<string> GetCastAsync()
	{
		throw new NotSupportedException();
	}

	public Task<bool> OutputWaveToFileAsync(string text, string path)
	{
		throw new NotSupportedException();
	}

	public ValueTask SetCastAsync(string castName)
	{
		throw new NotSupportedException();
	}

	public Task<bool> SpeakAsync(string text, CancellationToken? token = null)
	{
		throw new NotSupportedException();
	}
}
