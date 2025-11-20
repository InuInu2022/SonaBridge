using Xunit.Abstractions;
using SonaBridge.Core.Rest;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using System.Diagnostics.CodeAnalysis;

namespace CoreRestTest;

[System.Diagnostics.CodeAnalysis.SuppressMessage(
	"Usage",
	"SMA0040:Missing Using Statement",
	Justification = "<保留中>"
)]
public class RestTest(RestServiceFixture fixture, ITestOutputHelper output)

	: IClassFixture<RestServiceFixture>
{
	public async Task StartTest()
	{
	}

	[Theory]
	[InlineData("これはテストです")]
	[InlineData("テストなのです")]
	[InlineData("あめんぼ赤いなあいうえお")]

	public async Task SpeakTest(string text)
	{
		Assert.NotNull(fixture.Service);
		var result = await fixture.Service.SpeakAsync(text);

		Assert.True(result);

		var result2 = await fixture.Service.SpeakAsync(text, "");
		output.WriteLine($"Analyzed Text: {result2}");
	}

	[Fact]
	public async Task AvailableCast()
	{
		Assert.NotNull(fixture.Service);
		var result = await fixture.Service.GetAvailableCastsAsync();

		Assert.NotNull(result);
		Assert.NotEmpty(result);

		output.WriteLine(
			$"Available Casts: {string.Join(", ", result)}"
		);
	}

	[Theory]
	[InlineData("田中傘")]
	[InlineData("さとうささら")]
	[InlineData("佐藤筅", false)]
	public async Task SetCast(string name, bool expectSuccess = true)
	{
		Assert.NotNull(fixture.Service);
		await fixture.Service.SetCastAsync(name);

		var result = await fixture.Service.GetCastAsync();

		if (expectSuccess)
		{
			Assert.Equal(name, result);
		}
		else
		{
			Assert.NotEqual(name, result);
		}
	}

	[Theory]
	[InlineData("これはテストです", "output_test1.wav")]
	[InlineData("テストなのです", "output_test2.wav")]
	[InlineData("あめんぼ赤いなあいうえお", "output_test3.wav")]
	public async Task SaveFile(string text, string fileName)
	{
		Assert.NotNull(fixture.Service);
		var folder = Path.Combine(Path.GetTempPath(), fileName);
		output.WriteLine($"Output Path: {folder}");

		var result = await fixture.Service
			.OutputWaveToFileAsync(text, folder);

		Assert.True(result);
		Assert.True(File.Exists(folder));
	}

	[Theory]
	[InlineData("さとうささら", 4)]
	[InlineData("田中傘", 5)]
	[InlineData("梵そよぎ", 8)]
	[InlineData("トモ", 1)]
	public async Task GetStyle(string castName, int count)
	{
		Assert.NotNull(fixture.Service);
		var result = await fixture.Service.GetStylesAsync(castName);

		Assert.NotEmpty(result);
		Assert.Equal(count, result.Count);
		Assert.All(result.Values, value => Assert.InRange(value, 0.0, 1.0));
	}
}

[SuppressMessage("Design", "MA0048:File name must match type name", Justification = "<保留中>")]
public class RestServiceFixture : IDisposable, IAsyncLifetime
{
	bool _disposedValue;

	public string UserName { get; }
	public string Password { get; }
	public TalkRestService? Service { get; private set; }

	public RestServiceFixture()
	{
		var builder = new ConfigurationBuilder().AddUserSecrets<InternalTest>();
		var config = builder.Build();

		UserName =
			config["Api:Username"]
			?? throw new InvalidOperationException("Api:Username is not configured.");
		Password =
			config["Api:Password"]
			?? throw new InvalidOperationException("Api:Password is not configured.");
	}

	public async Task InitializeAsync()
	{
		Service = await TalkRestService
			.StartAsync(UserName, Password, updateLibrary:true)
			.ConfigureAwait(false);
	}

	protected virtual void Dispose(bool disposing)
	{
		if (!_disposedValue)
		{
			if (disposing)
			{
				// マネージド状態を破棄します (マネージド オブジェクト)
				Service?.Dispose();
			}

			// TODO: アンマネージド リソース (アンマネージド オブジェクト) を解放し、ファイナライザーをオーバーライドします
			// TODO: 大きなフィールドを null に設定します
			_disposedValue = true;
		}
	}

	// // TODO: 'Dispose(bool disposing)' にアンマネージド リソースを解放するコードが含まれる場合にのみ、ファイナライザーをオーバーライドします
	// ~RestServiceFixture()
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

	public Task DisposeAsync()
	{
		Dispose();
		return Task.CompletedTask;
	}
}