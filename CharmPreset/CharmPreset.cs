namespace CharmPreset;

public sealed partial class CharmPreset : Mod {
	public static CharmPreset? Instance { get; private set; }
	public static CharmPreset UnsafeInstance => Instance!;

	private static readonly Lazy<string> version = AssemblyUtil
#if DEBUG
		.GetMyDefaultVersionWithHash();
#else
		.GetMyDefaultVersion();
#endif

	public override string GetVersion() => version.Value;

	public override void Initialize() {
		if (Instance != null) {
			LogWarn("Attempting to initialize multiple times, operation rejected");
			return;
		}

		Instance = this;
	}
}
