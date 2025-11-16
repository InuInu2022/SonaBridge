using Microsoft.Kiota.Abstractions.Authentication;
using Microsoft.Kiota.Http.HttpClientLibrary;
using Microsoft.Extensions.Configuration;
using SonaBridge.Core.Rest.Internal;

namespace CoreRestTest;

public class ServiceFixture : IDisposable
{
	bool _disposedValue;

	[System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "SMA0040:Missing Using Statement", Justification = "<保留中>")]
	public ServiceFixture()
	{
		var builder = new ConfigurationBuilder().AddUserSecrets<InternalTest>();
		var config = builder.Build();

		UserName = config["Api:Username"] ?? throw new InvalidOperationException("Api:Username is not configured.");
		Password = config["Api:Password"] ?? throw new InvalidOperationException("Api:Password is not configured.");

		AuthProvider = new BasicAuthenticationProvider(UserName, Password);
		Adapter = new HttpClientRequestAdapter(AuthProvider)
		{
			BaseUrl = "http://localhost:32766/api/talk/v1",
		};

		Client = new RawTalkApi(Adapter);
	}

	public required string UserName { get; init; }
	public required string Password { get; init; }
	public BasicAuthenticationProvider AuthProvider { get; }
	public HttpClientRequestAdapter Adapter { get; }
	public RawTalkApi Client { get; }

	protected virtual void Dispose(bool disposing)
	{
		if (!_disposedValue)
		{
			if (disposing)
			{
				// マネージド状態を破棄します (マネージド オブジェクト)
				Adapter?.Dispose();
			}

			// TODO: アンマネージド リソース (アンマネージド オブジェクト) を解放し、ファイナライザーをオーバーライドします
			// TODO: 大きなフィールドを null に設定します
			_disposedValue = true;
		}
	}

	// // TODO: 'Dispose(bool disposing)' にアンマネージド リソースを解放するコードが含まれる場合にのみ、ファイナライザーをオーバーライドします
	// ~ServiceFixture()
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