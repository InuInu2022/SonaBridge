using System.Collections.ObjectModel;

namespace SonaBridge.Core.Common;

//VoiSona Talk固有
public partial interface ITalkAutoService : IAutoService
{
	/// <summary>
	/// 非同期で起動
	/// </summary>
	/// <returns></returns>
	Task StartAsync();

	/// <summary>
	/// 現在のキャストのグローバルパラメータ一覧を取得します。
	/// </summary>
	/// <returns></returns>
	Task<ReadOnlyDictionary<string, double>> GetGlobalParamsAsync();

	/// <summary>
	/// 現在のキャストにグローバルパラメータを設定します。
	/// </summary>
	/// <param name="globalParams">パラメータ名と値。値はUIと同じものを渡してください。範囲外の値は丸められます。</param>
	/// <returns></returns>
	ValueTask SetGlobalParamsAsync(IDictionary<string, double> globalParams);

	/// <summary>
	/// 指定したキャスト（ボイスライブラリ）のスタイル一覧を取得します。
	/// </summary>
	/// <param name="voiceName">ボイスライブラリ名</param>
	/// <returns></returns>
	Task<ReadOnlyDictionary<string, double>> GetStylesAsync(string voiceName);

	/// <summary>
	/// 指定したキャスト（ボイスライブラリ）にスタイルを設定します。
	/// </summary>
	/// <param name="voiceName">ボイスライブラリ名</param>
	/// <param name="styles">スタイル名と値。値はUIと同じものを渡してください。範囲外の値は丸められます。</param>
	ValueTask SetStylesAsync(string voiceName, IDictionary<string, double> styles);
}
