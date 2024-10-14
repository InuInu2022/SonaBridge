using FlaUI.Core.AutomationElements;
using FlaUI.Core.Tools;

namespace SonaBridge.Core.Win;

public static class AsyncRetry
{
	public static async ValueTask<T>
	WhileNull<T>(
		Func<T> checkMethod,
		TimeSpan? timeout = null,
		TimeSpan? interval = null,
		bool ignoreException = true
	)
		where T : AutomationElement?
	{
		var result = await Task
			.Run(() => Retry.WhileNull(
				checkMethod,
				timeout: timeout ?? TimeSpan.FromSeconds(3),
				interval: interval ?? TimeSpan.FromSeconds(0.1),
				ignoreException: ignoreException
			))
			.ConfigureAwait(false);

		if(!result.Success || result.Result is null) throw new InvalidOperationException("Failed to retry");

		return result.Result;
	}

	public static async ValueTask WaitUntilEnabledAsync(
		this AutomationElement elem,
		TimeSpan? timeout = null
	){
		await Task.Run(() => elem.WaitUntilEnabled(timeout))
			.ConfigureAwait(false);
	}

	public static async ValueTask WaitUntilClickableAsync(
		this AutomationElement elem,
		TimeSpan? timeout = null
	){
		await Task.Run(() => elem.WaitUntilClickable(timeout))
			.ConfigureAwait(false);
	}
}
