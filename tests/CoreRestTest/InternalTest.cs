using Microsoft.Kiota.Abstractions.Authentication;
using Microsoft.Kiota.Http.HttpClientLibrary;
using Microsoft.Extensions.Configuration;
using SonaBridge.Core.Rest;
using SonaBridge.Core.Rest.Internal;
using System.Diagnostics;
using System.Threading.Tasks;
using Xunit.Abstractions;
using System.Text;
using System.Net.Http.Headers;

namespace CoreRestTest;

[System.Diagnostics.CodeAnalysis.SuppressMessage(
	"Usage",
	"SMA0024:Enum to String",
	Justification = "<保留中>"
)]
public class InternalTest(ServiceFixture fixture, ITestOutputHelper output) : IClassFixture<ServiceFixture>
{
	readonly ITestOutputHelper _output = output;
	readonly ServiceFixture _fixture = fixture;

	[Fact]
	public async Task Voices()
	{
		// まず手動でHTTP接続を確認
		//await TestApiEndpoint(username, password);


		_output.WriteLine($"Adapter BaseUrl: {_fixture.Adapter.BaseUrl}");
		// Create the API client
		var client = new RawTalkApi(_fixture.Adapter);

		var result = await client.Voices.GetAsync();

	}

	private async Task TestApiEndpoint(string username, string password)
	{
		using var httpClient = new HttpClient { BaseAddress = new Uri("http://localhost:32766") };
		var authToken = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{username}:{password}"));
		httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
			"Basic",
			authToken
		);

		try
		{
			var response = await httpClient.GetAsync("/api/talk/v1/voices");
			_output.WriteLine($"Manual HTTP test - Status: {response.StatusCode}");
			var content = await response.Content.ReadAsStringAsync();
			_output.WriteLine($"Response: {content}");

			Assert.True(
				response.IsSuccessStatusCode,
				$"Expected success but got {response.StatusCode}"
			);
		}
		catch (Exception ex)
		{
			_output.WriteLine($"Manual HTTP test failed: {ex.Message}");
		}
	}
}

public class ServiceFixture : IDisposable
{
	bool _disposedValue;

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
	}

	public required string UserName { get; init; }
	public required string Password { get; init; }
	public BasicAuthenticationProvider AuthProvider { get; }
	public HttpClientRequestAdapter Adapter { get; }

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