using System.Diagnostics.CodeAnalysis;
using Microsoft.Kiota.Abstractions.Authentication;
using Microsoft.Kiota.Http.HttpClientLibrary;
using SonaBridge.Core.Rest;

namespace SonaBridge.Core.Win;

public partial class WinTalkAutoService
{
	BasicAuthenticationProvider? AuthProvider { get; set; }
	HttpClientRequestAdapter? Adapter { get; set; }
	TalkRestService? _service;
	public TalkRestService Service
	{
		get
		{
			return _service
				?? throw new InvalidOperationException(
					"Service is not initialized. Call InitAsync first."
				);
		}
	}

	[MemberNotNull(nameof(AuthProvider))]
	[MemberNotNull(nameof(Adapter))]
	[MemberNotNull(nameof(_service))]
	[SuppressMessage("Usage", "CS8774", Justification = "<保留中>")]
	internal async ValueTask InitAsync(string userName, string password, int port = 32766)
	{
		AuthProvider = new BasicAuthenticationProvider(userName, password);
		Adapter = new HttpClientRequestAdapter(AuthProvider)
		{
			BaseUrl = $"http://localhost:{port}/api/talk/v1",
		};

		#pragma warning disable CS8774
		_service = await TalkRestService
			.StartAsync(userName, password, updateLibrary: true)
			.ConfigureAwait(false);
		#pragma warning restore CS8774

		if (Service is null)
			throw new InvalidOperationException("Failed to initialize TalkRestService.");
	}
}
