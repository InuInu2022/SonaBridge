using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading.Tasks;
using SonaBridge.Core.Rest;
using SonaBridge.Core.Rest.Internal;
using SonaBridge.Core.Rest.Internal.Models;
using SonaBridge.Core.Rest.Internal.SpeechSyntheses;

using Xunit.Abstractions;
using static SonaBridge.Core.Rest.Extension.WaitExtension;

namespace CoreRestTest;

[System.Diagnostics.CodeAnalysis.SuppressMessage(
	"Usage",
	"SMA0024:Enum to String",
	Justification = "<保留中>"
)]
[System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "SMA0040:Missing Using Statement", Justification = "<保留中>")]
public class InternalTest(ServiceFixture fixture, ITestOutputHelper output)
	: IClassFixture<ServiceFixture>
{
	readonly ITestOutputHelper _output = output;
	readonly ServiceFixture _fixture = fixture;

	readonly JsonSerializerOptions jsonOption = new()
	{
		WriteIndented = true,
		Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
	};

	[Fact]
	public async Task Voices()
	{
		_output.WriteLine($"Adapter BaseUrl: {_fixture.Adapter.BaseUrl}");

		var result = await _fixture.Client.Voices.GetAsync();
	}

	[Fact]
	public async Task Languages()
	{
		var result = await _fixture.Client.Languages.GetAsync();
		Assert.NotNull(result);

		_output.WriteLine(
			$"Languages: {string.Join(", ", result?.Items?.Select(i => i.Language) ?? [])}"
		);
	}

	[Fact]
	public async Task SpeechSyntheses()
	{
		var body = new SpeechSynthesesPostRequestBody
		{
			Text = "これはテストです",
			ForceEnqueue = true,
			Destination = SpeechSynthesesPostRequestBody_destination.Audio_device,
			VoiceName = "tanaka-san_ja_JP",
			VoiceVersion = "2.0.0",
			Language = "ja_JP",
		};

		try
		{
			var posted = await _fixture.Client.SpeechSyntheses.PostAsync(body);
			Assert.NotNull(posted);

			_output.WriteLine(
				$"Posted result: {JsonSerializer.Serialize(posted, jsonOption)}"
			);

			Assert.NotNull(posted.Uuid);

			if (posted.Uuid is not { } uuid) return;

			var info = await _fixture.Client.SpeechSyntheses[uuid].GetAsync();
			do
			{
				info = await _fixture.Client.SpeechSyntheses[uuid].GetAsync();
				_output.WriteLine($"Progress: {info?.ProgressPercentage}");
				await Task.Delay(10);
			}
			while (info is not null && info.ProgressPercentage < 100);

		}
		catch (Content_created400Error ex400)
		{
			_output.WriteLine($"400 Error Details:");
			_output.WriteLine($"Error Type: {ex400.GetType().Name}");
			_output.WriteLine($"Message: {ex400.Message}");

			var props = ex400
				.GetType()
				.GetProperties(
					System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance
				);

			foreach (var prop in props)
			{
				try
				{
					var value = prop.GetValue(ex400);
					_output.WriteLine($"{prop.Name}: {value}");
				}
				catch (Exception propEx)
				{
					_output.WriteLine($"{prop.Name}: <Could not retrieve value: {propEx.Message}>");
				}
			}

			throw;
		}
		catch (Microsoft.Kiota.Abstractions.ApiException apiEx)
		{
			_output.WriteLine($"API Exception:");
			_output.WriteLine($"Status Code: {apiEx.ResponseStatusCode}");
			_output.WriteLine($"Message: {apiEx.Message}");
			_output.WriteLine(
				$"Headers: {string.Join(", ", apiEx.ResponseHeaders?.Select(h => $"{h.Key}={string.Join(",", h.Value)}") ?? [])}"
			);

			throw;
		}
		catch (Exception ex)
		{
			_output.WriteLine($"Unexpected Exception:");
			_output.WriteLine($"Type: {ex.GetType().Name}");
			_output.WriteLine($"Message: {ex.Message}");
			_output.WriteLine($"Stack Trace: {ex.StackTrace}");

			throw;
		}

		var result = await _fixture.Client.SpeechSyntheses.GetAsync();
		Assert.NotNull(result);

		_output.WriteLine(
			$"SpeechSyntheses result: {JsonSerializer.Serialize(result, jsonOption)}"
		);
	}

	[Fact]
	public async Task SpeechSynthesesWithWait()
	{
		var result = await _fixture.Client.SpeechSyntheses.PostAndWaitAsync(
			new()
			{
				Text = "これはテストです",
				ForceEnqueue = true,
				Destination = SpeechSynthesesPostRequestBody_destination.Audio_device,
				VoiceName = "tanaka-san_ja_JP",
				VoiceVersion = "2.0.0",
				Language = "ja_JP",
			}
		);
		Assert.NotNull(result);
	}
}
