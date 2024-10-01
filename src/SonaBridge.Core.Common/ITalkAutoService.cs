namespace SonaBridge.Core.Common;

public interface ITalkAutoService : IAutoService
{
	/// <summary>
	/// 指定したセリフの再生を開始します。
	/// </summary>
	/// <param name="text"></param>
	/// <param name="token"></param>
	/// <returns></returns>
	Task<bool> SpeakAsync(
		string text,
		CancellationToken? token = default
	);

	/// <summary>
	/// 利用可能なキャスト名 <c>string[]</c> を取得します。
	/// </summary>
	/// <remarks>
	/// FluentCeVIOWrapper互換（<see cref="FluentCeVIO.GetAvailableCastsAsync()" />）
	/// </remarks>
	/// <returns></returns>
	Task<string[]> GetAvailableCastsAsync();

	/// <summary>
	/// 現在のキャスト(話者)を取得します。
	/// </summary>
	/// <returns>ボイスライブラリ名</returns>
	/// <seealso cref="SetCastAsync(string)"/>
	Task<string> GetCastAsync();

	/// <summary>
	/// キャスト(話者)を設定します。
	/// </summary>
	/// <param name="castName">キャスト名。利用可能なキャスト名は<see cref="GetAvailableCastsAsync"/>で取得できます。</param>
	/// <returns></returns>
	/// <see cref="GetCastAsync"/>
	System.Threading.Tasks.ValueTask SetCastAsync(string castName);
}
