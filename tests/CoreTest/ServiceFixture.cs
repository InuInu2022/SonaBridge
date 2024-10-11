using SonaBridge;
using SonaBridge.Core.Common;

namespace CoreTest;

public class ServiceFixture : IDisposable
{
	TalkServiceProvider provider {get;}
	public ITalkAutoService Service { get; }
	public ServiceFixture()
	{
		provider = new TalkServiceProvider();
		Service = provider.GetService<ITalkAutoService>();
	}

	public void Dispose()
	{
		provider.Dispose();
	 	GC.SuppressFinalize(this);
	}
}