namespace SonaBridge.Core.Common;

/// <summary>
/// 音素データの単位オブジェクト。
/// </summary>
[Serializable]
public record struct PhonemeData
{
	/// <summary>
	/// 音素を取得します。
	/// </summary>
	public string Phoneme { get; set; }

	/// <summary>
	/// 開始時間を取得します。単位は秒。
	/// </summary>
	public double StartTime { get; set; }
	/// <summary>
	/// 終了時間を取得します。単位は秒。
	/// </summary>
	public double EndTime { get; set; }

	/// <summary>
	/// コンストラクタ
	/// </summary>
	/// <param name="startTime">開始時間（秒）</param>
	/// <param name="endTime">終了時間（秒）</param>
	/// <param name="phoneme">音素</param>
	public PhonemeData(double startTime, double endTime, string phoneme)
	{
		StartTime = startTime;
		EndTime = endTime;
		Phoneme = phoneme;
	}
}