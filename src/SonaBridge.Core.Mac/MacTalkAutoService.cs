
using System.Collections.ObjectModel;

using SonaBridge.Core.Common;

namespace SonaBridge.Core.Mac;

public class MacTalkAutoService : ITalkAutoService
{
	private bool _disposedValue;

	public Task StartAsync()
	{
		throw new NotSupportedException();
	}

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

	public ValueTask<IReadOnlyList<string>> GetPresetsAsync(string voiceName)
	{
		throw new NotSupportedException();
	}

	public ValueTask SetPresetsAsync(string voiceName, string presetName)
	{
		throw new NotSupportedException();
	}

	protected virtual void Dispose(bool disposing)
	{
		if (!_disposedValue)
		{
			if (disposing)
			{
				// TODO: マネージド状態を破棄します (マネージド オブジェクト)
			}

			// TODO: アンマネージド リソース (アンマネージド オブジェクト) を解放し、ファイナライザーをオーバーライドします
			// TODO: 大きなフィールドを null に設定します
			_disposedValue = true;
		}
	}

	// // TODO: 'Dispose(bool disposing)' にアンマネージド リソースを解放するコードが含まれる場合にのみ、ファイナライザーをオーバーライドします
	// ~MacTalkAutoService()
	// {
	//     // このコードを変更しないでください。クリーンアップ コードを 'Dispose(bool disposing)' メソッドに記述します
	//     Dispose(disposing: false);
	// }

	public void Dispose()
	{
		// このコードを変更しないでください。クリーンアップ コードを 'Dispose(bool disposing)' メソッドに記述します
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}
}
