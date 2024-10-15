
using System.Collections.ObjectModel;

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

	public Task<ReadOnlyDictionary<string, double>> GetGlobalParamsAsync()
	{
		throw new NotSupportedException();
	}

	public Task<ReadOnlyDictionary<string, double>> GetStylesAsync(string voiceName)
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

	public ValueTask SetGlobalParamsAsync(IDictionary<string, double> globalParams)
	{
		throw new NotSupportedException();
	}

	public ValueTask SetStylesAsync(string voiceName, IDictionary<string, double> styles)
	{
		throw new NotSupportedException();
	}

	public Task<bool> SpeakAsync(string text, CancellationToken? token = null)
	{
		throw new NotSupportedException();
	}
}
