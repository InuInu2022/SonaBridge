using System.Collections.ObjectModel;

using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;
using FlaUI.Core.Tools;

namespace SonaBridge.Core.Win;

//グローバルパラメータパネルから取得する処理関連
public partial class WinTalkAutoService
{
	static ToggleButton? gPanelToggle;
	static Slider[]? gParamSliders;
	static readonly Dictionary<string,(double max, double min, double, double)> gParam = new(StringComparer.Ordinal)
	{
		{"Speed", (5.0, 0.2, 0.048, 0.048)},
		{"Volume", (8.0, -8.0, 0.16, 0.16)},
		{"Pitch", (600, -600, 12, 12)},
		{"Alpha", (1.0, -1.0, 0.02, 0.02)},
		{"Into.", (2.0, 0.0, 0.02, 0.02)},
		{"Hus.", (20, -20, 0.4, 0.4)},
	};

	internal async ValueTask OpenGlobalParamsPanelAsync()
	{
		//TODO:tree cache
		if(gPanelToggle is null)
		{
			await GetAppWindowAsync().ConfigureAwait(false);
			var result = await Task
				.Run(() => Retry.WhileNull(
					() => _win?
						.FindAllDescendants(f =>
							f.ByName("global parameters")
							.And(f.ByControlType(ControlType.Button))
						)
						.FirstOrDefault()
						.AsToggleButton(),
					timeout: TimeSpan.FromSeconds(3),
					interval: TimeSpan.FromSeconds(0.1),
					ignoreException: true
				))
				.ConfigureAwait(false);
			if (!result.Success) return;
			gPanelToggle = result.Result;
		}
		if (gPanelToggle is null) return;
		if (gPanelToggle.IsToggled is true) return;
		gPanelToggle.Toggle();
	}

	internal async ValueTask<Slider[]> GetGlobalParamSliders()
	{
		if(gParamSliders is not null && gParamSliders.Length > 0)
		{
			return gParamSliders;
		}
		await GetAppWindowAsync().ConfigureAwait(false);
		var result = await Task
			.Run(() => Retry.WhileNull(
				() =>
				{
					var sliders = _win?
						.FindAllDescendants(f =>
							f.ByFrameworkId("JUCE")
							.And(f.ByControlType(ControlType.Slider))
						)
						.Select(e => e.AsSlider())
						.ToArray()
						;
					return sliders is [] ? null : sliders;
				},
				timeout: TimeSpan.FromSeconds(3),
				interval: TimeSpan.FromSeconds(0.1),
				ignoreException: true
			))
			.ConfigureAwait(false);
		if (!result.Success) return [];
		gParamSliders = result.Result ?? [];
		return gParamSliders;
	}

	internal async ValueTask<ReadOnlyDictionary<string, double>> GetCurrentGlobalParamAsync()
	{
		var sliders = await GetGlobalParamSliders()
			.ConfigureAwait(false);
		var d = new Dictionary<string, double>(StringComparer.Ordinal)
		{
			{"Speed", FindValue(sliders, gParam["Speed"])},
			{"Volume", FindValue(sliders, gParam["Volume"])},
			{"Pitch", FindValue(sliders, gParam["Pitch"])},
			{"Alpha", FindValue(sliders, gParam["Alpha"])},
			{"Into.", FindValue(sliders, gParam["Into."])},
			{"Hus.", FindValue(sliders, gParam["Hus."])},
		};
		return new(d);

		static double FindValue(
			Slider[] sliders,
			(
				double max,
				double min,
				double smallChange,
				double largeChange
			) search
		) => FindGParamSlider(sliders, search)?.Value ?? 0.0;
	}

	internal async ValueTask SetCurrentGlobalParamsAsync(
		IDictionary<string, double> globalParams
	)
	{
		var sliders = await GetGlobalParamSliders()
			.ConfigureAwait(false);
		foreach (var item in globalParams)
		{
			if(!gParam.TryGetValue(item.Key, out var value))
			{
				Console.Error.WriteLine($"[{nameof(SetCurrentGlobalParamsAsync)}] Key {item.Key} not found");
				continue;
			}
			var p = FindGParamSlider(sliders, value);
			if (p is null) continue;
			p.Value = Math.Max(value.min, Math.Min(item.Value, value.max));
			await p.WaitUntilEnabledAsync()
				.ConfigureAwait(false);
		}
	}

	static Slider? FindGParamSlider(Slider[] sliders, (double max, double min, double smallChange, double largeChange) search)
	{
		return sliders
			.FirstOrDefault(s =>
				s.Maximum == search.max
				&& s.Minimum == search.min
				&& s.SmallChange == search.smallChange
				&& s.LargeChange == search.largeChange
			)
			;
	}
}
