using System.Collections.ObjectModel;

namespace SonaBridge.Core.Common;

//VoiSona Talk固有
public partial interface ITalkAutoService : IAutoService
{
	/// <summary>
	/// 現在のキャストのグローバルパラメータ一覧を取得します。
	/// </summary>
	/// <returns></returns>
	Task<ReadOnlyDictionary<string, double>> GetGlobalParamsAsync();

	/// <summary>
	/// 現在のキャストにグローバルパラメータを設定します。
	/// </summary>
	/// <param name="styles">パラメータ名と値。値はUIと同じものを渡してください。範囲外の値は丸められます。</param>
	/// <returns></returns>
	Task SetGlobalParamsAsync(IDictionary<string, double> styles);
}
