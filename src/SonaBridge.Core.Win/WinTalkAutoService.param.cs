using System.Collections.ObjectModel;

using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;
using FlaUI.Core.Tools;
using FlaUI.UIA3;

namespace SonaBridge.Core.Win;

//グローバルパラメータパネルから取得する処理関連
public partial class WinTalkAutoService
{
	static ToggleButton? gPanelToggle;
	static Button? styleBarButton;
	static Slider[]? gParamSliders;
	static readonly Dictionary<string, Slider[]> _styleSliderCache = [];
	static readonly Dictionary<string, IReadOnlyList<string>> _styleNamesCache = [];
	static string? lastVoiceName;
	private static ToggleButton? _globalPanelButton;
	private static Button? _styleButton;
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
					static () => GetGlobalPanelButton(),
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
		await OpenGlobalParamsPanelAsync().ConfigureAwait(false);
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
		//TODO: tune
		var sliders = await GetGlobalParamSliders()
			.ConfigureAwait(false);
		foreach (var item in globalParams)
		{
			if(!gParam.TryGetValue(item.Key, out var value))
			{
				await Console.Error
					.WriteLineAsync($"[{nameof(SetCurrentGlobalParamsAsync)}] Key {item.Key} not found")
					.ConfigureAwait(false);
				continue;
			}
			var p = FindGParamSlider(sliders, value);
			if (p is null) continue;
			p.Value = Math.Max(value.min, Math.Min(item.Value, value.max));
			await p.WaitUntilEnabledAsync()
				.ConfigureAwait(false);
		}
	}

	internal async ValueTask SetStyleBarTabEnabledAsync()
	{
		if(styleBarButton is null)
		{
			styleBarButton = await GetStyleBarButton().ConfigureAwait(false);
		}
		if (styleBarButton is null) return;
		await styleBarButton!.WaitUntilClickableAsync().ConfigureAwait(false);
		styleBarButton?.Invoke();
		await styleBarButton!.WaitUntilEnabledAsync().ConfigureAwait(false);
	}

	async ValueTask<Button?> GetStyleBarButton()
	{
		await GetAppWindowAsync().ConfigureAwait(false);
		var result = await Task
			.Run(() => Retry.WhileNull(
				static () => GetStyleButton(),
				timeout: TimeSpan.FromSeconds(3),
				interval: TimeSpan.FromSeconds(0.1),
				ignoreException: true
			))
			.ConfigureAwait(false);
		if (!result.Success) return default;
		return result.Result;
	}

	static Button? GetStyleButton()
	{
		if (_styleButton is not null) return _styleButton;
		_styleButton = _win?
			.FindAllDescendants(f =>
				f.ByName("Bar")
				.And(f.ByControlType(ControlType.Button))
			)
			.FirstOrDefault()
			.AsButton();
		return _styleButton;
	}

	internal async ValueTask<Slider[]> GetStyleSlidersAsync(
		string voiceName
	)
	{
		if(_styleSliderCache.TryGetValue(voiceName, out var sliders))
		{
			return sliders;
		}

		await GetAppWindowAsync().ConfigureAwait(false);
		await OpenGlobalParamsPanelAsync().ConfigureAwait(false);
		await SetStyleBarTabEnabledAsync().ConfigureAwait(false);
		lastVoiceName = voiceName;
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
						.Where(s => s.SmallChange == 0.01 && s.Minimum == 0 && s.Maximum == 1)
						.ToArray()
						;
					return sliders is [] ? null : sliders;
				},
				timeout: TimeSpan.FromSeconds(3),
				interval: TimeSpan.FromSeconds(0.05),
				ignoreException: true
			))
			.ConfigureAwait(false);
		if (!result.Success) return [];
		var foundSliders = result.Result ?? [];
		_styleSliderCache.Add(voiceName, foundSliders);
		return foundSliders;
	}

	// 0.75 sec.
	internal async ValueTask<ReadOnlyDictionary<string, double>>
	GetCurrentStylesAsync(string voiceName)
	{
		var sliders = await GetStyleSlidersAsync(voiceName)
			.ConfigureAwait(false);
		var names = await GetCurrentStyleNamesAsync(voiceName)
			.ConfigureAwait(false);
		var dic = sliders
			.Zip(names, (slider, name) => (name, slider.Value))
			.ToDictionary(x => x.name, x => x.Value, StringComparer.Ordinal);
		return new(dic);
	}

	//time: 0.7588927 sec.
	internal async ValueTask<IReadOnlyList<string>>
	GetCurrentStyleNamesAsync(string voiceName)
	{
		if(_styleNamesCache.TryGetValue(voiceName, out var names)){
			return names;
		}
		styleBarButton ??= await GetStyleBarButton().ConfigureAwait(false);
		var group = styleBarButton?.Parent;
		var walker = _automation.TreeWalkerFactory.GetRawViewWalker();
		walker.GetNextSibling(group);

		//TODO: 通常loop化
		AutomationElement? lastElem = group;
		await Task
			.Run(() => Retry.WhileFalse(
				() =>
				{
					var elem = walker.GetNextSibling(lastElem);
					var isText = elem.ControlType == ControlType.Text;
					lastElem = elem;
					return isText;
				},
				timeout: TimeSpan.FromSeconds(1),
				interval: TimeSpan.FromSeconds(0.001),
				ignoreException: true
			))
			.ConfigureAwait(false);

		AutomationElement? lastText = lastElem.AsTextBox();
		List<TextBox> textBoxes = [lastText.AsTextBox()];
		//TODO: 通常loop化
		await Task
			.Run(() => Retry.WhileTrue(
				() =>
				{
					var elem = walker.GetNextSibling(lastText);
					var isText = elem.ControlType == ControlType.Text;
					lastText = elem;
					if(isText) {
						textBoxes.Add(elem.AsTextBox());
					}
					return isText;
				},
				timeout: TimeSpan.FromSeconds(1),
				interval: TimeSpan.FromSeconds(0.001),
				ignoreException: true
			))
			.ConfigureAwait(false);
		var foundNames = textBoxes.ConvertAll(t => t.Text);
		_styleNamesCache.Add(voiceName, foundNames);
		return foundNames;
	}

	// 1.4 sec.
	internal async ValueTask
	SetCurrentStylesAsync(string voiceName, IDictionary<string, double> styles)
	{
		SetFocusFirstRow();
		var sliders = await GetStyleSlidersAsync(voiceName)
			.ConfigureAwait(false);
		var names = await GetCurrentStyleNamesAsync(voiceName)
			.ConfigureAwait(false);
		foreach (var item in styles)
		{
			if(!names.Contains(item.Key, StringComparer.Ordinal))
			{
				await Console.Error
					.WriteLineAsync($"[{nameof(SetCurrentStylesAsync)}] Key {item.Key} not found")
					.ConfigureAwait(false);
				continue;
			}
			var index = names.ToList().IndexOf(item.Key);
			var p = sliders[index];
			if (p is null) continue;
			p.Value = Math.Max(0.0, Math.Min(item.Value, 1.0));
			await p.WaitUntilEnabledAsync()
				.ConfigureAwait(false);
		}
	}

	static ToggleButton? GetGlobalPanelButton()
	{
		if (_globalPanelButton is not null) return _globalPanelButton;
		_globalPanelButton = _win?
			.FindAllDescendants(f =>
				f.ByName("global parameters")
				.And(f.ByControlType(ControlType.Button))
			)
			.FirstOrDefault()
			.AsToggleButton();
		return _globalPanelButton;
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
