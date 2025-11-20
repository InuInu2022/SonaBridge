using System.Linq;

namespace SonaBridge.Core.Rest;

public partial class TalkRestService
{
	async ValueTask<bool> TryUpdateLibraryAsync()
	{
		var result = await _client
			.Voices
			.GetAsync()
			.ConfigureAwait(false);

		if (result is not { } data)
		{
			return false;
		}

		data.Items?.ForEach(static x =>
		{
			if (x.VoiceName is null)
				return;

			if (x.VoiceVersion is null)
				return;

			var appendVoice = new VoiceData(
				VoiceName: new(x.VoiceName),
				VoiceVersions: [x.VoiceVersion],

				Languages: x.Languages is { } langs
					? new(StringComparer.Ordinal) {
						{ x.VoiceVersion, [.. langs.Select(ln => new LanguageKey(ln))] },
					}
					: [],

				DisplayNames: x.DisplayName?
					.Where(v => v is { Name: not null, Language: not null })
					.ToList()
					.ToDictionary(
						v => new LanguageKey(v.Language!),
						v => v.Name!)
					?? []
			);

			_ = VoiceByName.AddOrUpdate(
				new(x.VoiceName),
				addValueFactory: _ => appendVoice,
				updateValueFactory: (_, oldValue) =>
				{
					var versions = x.VoiceVersion is { } newVer
						? [.. oldValue.VoiceVersions, newVer]
						: oldValue.VoiceVersions;
					if (x.Languages is not null)
					{
						oldValue.Languages
							.TryAdd(x.VoiceVersion, [.. x.Languages.Select(l => new LanguageKey(l))]);
					}
					return oldValue with
					{
						VoiceVersions = [.. versions.Distinct(StringComparer.Ordinal)],
					};
				});

			if (x.DisplayName?.FirstOrDefault()?.Name is not { } name) return;
			_ = VoiceByDisplay.AddOrUpdate(
				new(name),
				addValueFactory: _ => new(x.VoiceName),
				updateValueFactory: (_, oldValue) => oldValue
			);
		});

		return true;
	}
}
