using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnostics.Windows.Configs;
using BenchmarkDotNet.Engines;

using FlaUI.Core.AutomationElements;

using SonaBridge;
using SonaBridge.Core.Common;
using SonaBridge.Core.Win;

namespace CoreBench;

[SimpleJob(RunStrategy.ColdStart, iterationCount: 5)]
[MemoryDiagnoser]
//[TailCallDiagnoser]
//[ConcurrencyVisualizerProfiler]
//[NativeMemoryProfiler]
[ThreadingDiagnoser]
[ExceptionDiagnoser]
[DisplayColumn("Allocated")]
public class Benchmarks : IDisposable
{
	private TalkServiceProvider? provider;
	private bool _disposedValue;

	private ITalkAutoService? service { get; set; }

	private Random? _random;

	public IReadOnlyList<string> VoiceNames
	{ get; set; } = [
		"Tanaka San",
		"Sato Sasara",
		"Suzuki Tsudumi",
		"Takahashi",
		"Futaba Minato",
		"Koharu Rikka",
		"Tsurumaki Maki",
		"Tamaki",
		"Hanakuma Chifuyu",
	];

	public string RandomVoiceName => VoiceNames
		.OrderBy(x => _random!.Next())
		.First();

	[GlobalSetup]
    public void Setup()
    {
		provider = new TalkServiceProvider();
		service = provider.GetService<ITalkAutoService>();
		_random = new Random();
    }

	[GlobalCleanup]
    public void Cleanup()
    {
		service?.Dispose();
    }

	#region Standard

	[Benchmark]
	[BenchmarkCategory("Standard")]
	public async Task SpeakSingleAsync()
	{
		await service!.SpeakAsync("あ").ConfigureAwait(false);
	}

	[Benchmark]
	[BenchmarkCategory("Standard")]
	public async Task SpeakRandomVoiceAsync()
	{
		await service!.SetCastAsync(RandomVoiceName)
			.ConfigureAwait(false);
		await service!.SpeakAsync("あ")
			.ConfigureAwait(false);
	}

	[Benchmark]
	[BenchmarkCategory("Standard")]
	public async Task GetAvailableCastsAsync()
	{
		await service!.GetAvailableCastsAsync()
			.ConfigureAwait(false);
	}

	[Benchmark]
	[BenchmarkCategory("Standard")]
	public async Task SetCastAsync()
	{
		await service!.SetCastAsync(RandomVoiceName)
			.ConfigureAwait(false);
	}

	[Benchmark]
	[BenchmarkCategory("Standard")]
	public async Task OutputWaveToFileSingleAsync()
	{
		await service!
			.OutputWaveToFileAsync(
				"あ",
				Path.Combine(Path.GetTempPath(),Path.GetRandomFileName())
			)
			.ConfigureAwait(false);
	}

	[Benchmark]
	[BenchmarkCategory("Standard")]
	public async Task OutputWaveToFileRandomAsync()
	{
		await service!.SetCastAsync(RandomVoiceName)
			.ConfigureAwait(false);
		await service!
			.OutputWaveToFileAsync(
				"あ",
				Path.Combine(Path.GetTempPath(),Path.GetRandomFileName())
			)
			.ConfigureAwait(false);
	}

	[Benchmark]
	[BenchmarkCategory("Standard")]
	public async Task GetGlobalParamsAsync()
	{
		await service!
			.GetGlobalParamsAsync()
			.ConfigureAwait(false);
	}

	[Benchmark]
	[BenchmarkCategory("Standard")]
	public async Task SetGlobalParamsAsync()
	{
		await service!
			.SetGlobalParamsAsync(new Dictionary<string , double>(StringComparer.Ordinal){
				{"Pitch",1.0}
			})
			.ConfigureAwait(false);
	}

	[Benchmark]
	[BenchmarkCategory("Standard")]
	public async Task GetStylesAsync()
	{
		await service!
			.GetStylesAsync(RandomVoiceName)
			.ConfigureAwait(false);
	}

	[Benchmark]
	[BenchmarkCategory("Standard")]
	public async Task SetStylesAsync()
	{
		await service!
			.SetStylesAsync(
				RandomVoiceName,
				new Dictionary<string, double>(StringComparer.Ordinal){
					{"Normal",0.5}
				}
			)
			.ConfigureAwait(false);
	}

	#endregion Standard

	#region SetCast

	[Benchmark(Baseline = true)]
	[BenchmarkCategory("SetCast")]
	public async Task SetCastCacheNone()
	{
		if (service is not WinTalkAutoService wService) return;
		await wService.GetAppWindowAsync()
			.ConfigureAwait(false);
		WinCommon.SaveMousePoint();
		var result = await WinTalkAutoService
			.SetVoiceAsync(RandomVoiceName)
			.ConfigureAwait(false);
		await WinCommon.RestoreMousePointAsync()
			.ConfigureAwait(false);
		if(!result){
			throw new InvalidOperationException("FlaUI operation error!");
		}
	}

	[Benchmark]
	[BenchmarkCategory("SetCast")]
	public async Task SetCastCacheWith()
	{
		if (service is not WinTalkAutoService wService) return;
		await wService.GetAppWindowAsync()
			.ConfigureAwait(false);
		WinCommon.SaveMousePoint();

		var result = await WinTalkAutoService
			.SetVoiceAsync2(RandomVoiceName)
			.ConfigureAwait(false);

		await WinCommon.RestoreMousePointAsync()
			.ConfigureAwait(false);
		if(!result)
		{
			throw new InvalidOperationException("FlaUI operation error!");
		}
	}

	#endregion SetCast


	///////////////////////////////////////////////////////////////////////////////////

	protected virtual void Dispose(bool disposing)
	{
		if (!_disposedValue)
		{
			if (disposing)
			{
				service?.Dispose();
			}

			_disposedValue = true;
		}
	}

	[SuppressMessage("Design", "MA0055:Do not use finalizer", Justification = "<保留中>")]
	~Benchmarks()
	{
		Dispose(disposing: false);
	}

	public void Dispose()
	{
		// このコードを変更しないでください。クリーンアップ コードを 'Dispose(bool disposing)' メソッドに記述します
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}
}
