using SonaBridge;
using SonaBridge.Core.Common;

namespace CoreTest;

public class ServiceProviderTests
{
	[Fact]
	public async void CallTest()
	{
		var provider = new TalkServiceProvider();
		var service = provider.GetService<ITalkAutoService>();

		var _ = await service
			.SpeakAsync("サービスから呼び出しています。");
	}
}
