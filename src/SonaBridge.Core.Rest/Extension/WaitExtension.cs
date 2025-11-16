using System.Diagnostics;

using Microsoft.Kiota.Abstractions;
using Microsoft.Extensions.Logging;

using SonaBridge.Core.Rest.Internal.SpeechSyntheses;
using SonaBridge.Core.Rest.Internal.SpeechSyntheses.Item;

namespace SonaBridge.Core.Rest.Extension;

public static partial class WaitExtension
{
	[LoggerMessage(
		Level = LogLevel.Warning,
		Message = "音声合成リクエストが失敗しました（UUIDが取得できませんでした）")]
	static partial void LogSynthesisRequestFailed(ILogger logger);

	[LoggerMessage(
		Level = LogLevel.Warning,
		Message = "音声合成リクエストがキャンセルされました")]
	static partial void LogSynthesisRequestCancelled(ILogger logger);

	[LoggerMessage(
		Level = LogLevel.Warning,
		Message = "音声合成待機がタイムアウトしました（{Timeout}）")]
	static partial void LogSynthesisRequestTimeout(ILogger logger, TimeSpan timeout);

	[LoggerMessage(
		Level = LogLevel.Critical,
		Message = "音声合成が失敗しました（{message}）")]
	static partial void LogSynthesisFailed(ILogger logger, string message);

	[LoggerMessage(
		Level = LogLevel.Information,
		Message = "音声合成が完了しました: {Uuid}")]
	static partial void LogSynthesisCompleted(ILogger logger, Guid uuid);

	extension(SpeechSynthesesRequestBuilder builder)
	{
		/// <summary>
		/// 音声合成の完了を待機します
		/// 完了しなかった場合、nullを返します
		/// </summary>
		/// <param name="body">必須パラメータが未記入の場合、失敗します。</param>
		/// <param name="progress">合成の進捗率(0-100)</param>
		/// <param name="ctx"></param>
		/// <returns><inheritdoc cref="WithUuGetResponse"/>完了しなかった場合、nullを返します</returns>
		public async Task<WithUuGetResponse?> PostAndWaitAsync(
			SpeechSynthesesPostRequestBody body,
			TimeSpan timeout = default,
			IProgress<int>? progress = default,
			ILogger? logger = default,
			CancellationToken ctx = default
		)
		{
			if (timeout == default) timeout = TimeSpan.FromMinutes(1);

			const int interval = 100;
			using var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(interval));
			using var cts = CancellationTokenSource
				.CreateLinkedTokenSource(ctx);
			cts.CancelAfter(timeout);

			try
			{
				//リクエストをpost
				var posted = await builder
					.PostAsync(body, null, ctx)
					.ConfigureAwait(false);
				if (posted?.Uuid is not { } uuid)
				{
					//失敗
					if (logger is not null) {
						LogSynthesisRequestFailed(logger);
					}
					return null;
				}

				//まつ
				while (await timer
					.WaitForNextTickAsync(ctx)
					.ConfigureAwait(false)
				)
				{
					WithUuGetResponse? info = default;
					try
					{
						info = await builder[uuid]
							.GetAsync(cancellationToken: ctx)
							.ConfigureAwait(false);
					}
					catch (ApiException apiEx)
						when (apiEx.ResponseStatusCode == 404)
					{
						// 登録直後の404は許容
						continue;
					}
					catch (ApiException apiEx)
					{
						if (logger is not null)
							LogSynthesisFailed(logger, apiEx.Message);
						return null;
					}

					progress?.Report(info?.ProgressPercentage ?? 0);

					if (info?.ProgressPercentage is >= 100)
					{
						if (logger is not null)
							LogSynthesisCompleted(logger, uuid);
						return info;
					}
				}
			}
			catch (OperationCanceledException)
			{
				// キャンセルされた
				if (ctx.IsCancellationRequested && logger is not null)
				{
					LogSynthesisRequestCancelled(logger);
				}
				else if (logger is not null)
				{
					LogSynthesisRequestTimeout(logger, timeout);
				}
				return null;
			}

			return null;
		}
	}
}
