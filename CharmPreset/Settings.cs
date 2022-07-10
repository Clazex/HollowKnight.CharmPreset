namespace CharmPreset;

public sealed class LocalSettings {
	private int activePreset = 0;

	[JsonProperty(PropertyName = nameof(activePreset))]
	public int ActivePreset {
		get => activePreset;
		set => activePreset = value.Clamp(0, PresetController.PRESET_AMOUNT - 1);
	}

	private List<List<int>> presets = Enumerable.Repeat(() => new List<int>(), PresetController.PRESET_AMOUNT).Map(f => f()).ToList();

	[JsonProperty(PropertyName = nameof(presets))]
	public List<List<int>> Presets {
		get => presets;
		set {
			if (value.Count > PresetController.PRESET_AMOUNT) {
				value = value.GetRange(0, PresetController.PRESET_AMOUNT);
			}

			while (value.Count < PresetController.PRESET_AMOUNT) {
				value.Add(new());
			}

			presets = value;
		}
	}
}

public partial class CharmPreset : ILocalSettings<LocalSettings> {
	public static LocalSettings LocalSettings { get; private set; } = new();
	public void OnLoadLocal(LocalSettings s) => LocalSettings = s;
	public LocalSettings OnSaveLocal() => LocalSettings;
}
