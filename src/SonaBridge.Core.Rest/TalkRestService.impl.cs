namespace SonaBridge.Core.Rest;

public partial class TalkRestService
{
	async ValueTask<bool> TryUpdateLibraryAsync()
	{
		var result = await Client
			.Voices
			.GetAsync()
			.ConfigureAwait(false);

		if (result is not { } data)
		{
			return false;
		}

		data.Items?.ForEach(x =>
		{
			if (x.VoiceName is null)
				return;

			if (x.VoiceVersion is null)
				return;

			var appendVoice = new VoiceData(
				VoiceName: x.VoiceName,
				VoiceVersions: [x.VoiceVersion],

				Languages: x.Languages is { } langs
					? new(StringComparer.Ordinal) {
						{ x.VoiceVersion, [.. langs] } }
					: [],

				DisplayNames: x.DisplayName?
					.Where(v => v is { Name: not null, Language: not null })?
					.ToDictionary(
						v => v.Language!,
						v => v.Name!,
						StringComparer.Ordinal)
					?? []
			);

			_ = VoiceByName.AddOrUpdate(
				x.VoiceName,
				addValueFactory: _ => appendVoice,
				updateValueFactory: (_, oldValue) =>
				{
					var versions = x.VoiceVersion is { } newVer
						? [.. oldValue.VoiceVersions, newVer]
						: oldValue.VoiceVersions;
					if (x.Languages is not null)
					{
						oldValue.Languages
							.TryAdd(x.VoiceVersion, [.. x.Languages]);
					}
					return oldValue with
					{
						VoiceVersions = [.. versions.Distinct(StringComparer.Ordinal)],
					};
				});

			if (x.DisplayName?.FirstOrDefault()?.Name is not { } name) return;
			_ = VoiceByDisplay.AddOrUpdate(
				name,
				addValueFactory: _ => x.VoiceName,
				updateValueFactory: (_, oldValue) => oldValue
			);
		});

		return true;
	}
}
