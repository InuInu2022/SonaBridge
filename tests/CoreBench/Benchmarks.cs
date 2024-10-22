using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnostics.Windows.Configs;
using BenchmarkDotNet.Engines;

using SonaBridge;
using SonaBridge.Core.Common;

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

	//[Params("短い文章", "あめんぼ甘いな、あいうえお。これは長い文章です。")]
	//public string SpeakText { get; set; } = "";

	[Benchmark]
	public async Task SpeakSingleAsync()
	{
		await service!.SpeakAsync("あ").ConfigureAwait(false);
	}

	[Benchmark]
	public async Task SpeakRandomVoiceAsync()
	{
		await service!.SetCastAsync(RandomVoiceName)
			.ConfigureAwait(false);
		await service!.SpeakAsync("あ")
			.ConfigureAwait(false);
	}

	[Benchmark]
	public async Task GetAvailableCastsAsync()
	{
		await service!.GetAvailableCastsAsync()
			.ConfigureAwait(false);
	}

	[Benchmark]
	public async Task SetCastAsync()
	{
		await service!.SetCastAsync(RandomVoiceName)
			.ConfigureAwait(false);
	}

	[Benchmark]
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
	public async Task GetGlobalParamsAsync()
	{
		await service!
			.GetGlobalParamsAsync()
			.ConfigureAwait(false);
	}

	[Benchmark]
	public async Task SetGlobalParamsAsync()
	{
		await service!
			.SetGlobalParamsAsync(new Dictionary<string , double>(){
				{"Pitch",1.0}
			})
			.ConfigureAwait(false);
	}

	[Benchmark]
	public async Task GetStylesAsync()
	{
		await service!
			.GetStylesAsync(RandomVoiceName)
			.ConfigureAwait(false);
	}

	[Benchmark]
	public async Task SetStylesAsync()
	{
		await service!
			.SetStylesAsync(
				RandomVoiceName,
				new Dictionary<string, double>(){
					{"Normal",0.5}
				}
			)
			.ConfigureAwait(false);
	}


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
