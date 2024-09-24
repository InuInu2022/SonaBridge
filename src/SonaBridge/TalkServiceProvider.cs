using Jab;

using SonaBridge.Core.Common;

namespace SonaBridge;

[ServiceProvider]
#if WINDOWS
[Singleton(typeof(ITalkAutoService), typeof(SonaBridge.Core.Win.WinTalkAutoService))]
#elif MACOS
[Singleton(typeof(ITalkAutoService), typeof(SonaBridge.Core.Mac.MacTalkAutoService))]
#endif
public partial class TalkServiceProvider { }
