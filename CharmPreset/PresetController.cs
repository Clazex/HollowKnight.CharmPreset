using UnityEngine.Assertions;

namespace CharmPreset;

internal sealed class PresetController : MonoBehaviour {
	public const int PRESET_AMOUNT = 3;

	private static List<PresetController> instances = new();

	private int num;

	private static readonly Lazy<Sprite[]> sprites = new(() => {
		Sprite[] res = Resources.FindObjectsOfTypeAll<Sprite>();
		return new Sprite[] {
			res.First(i => i.name == "Inv_0013_charm1"),
			res.First(i => i.name == "Inv_0012_charm2"),
			res.First(i => i.name == "Inv_0011_charm3")
		};
	});

	internal static void Init(GameObject parent, FadeGroup group) {
		Sprite[] sprites = PresetController.sprites.Value;
		Assert.AreEqual(sprites.Length, PRESET_AMOUNT, $"Sprite amount does not equals to PRESET_AMOUNT ({PRESET_AMOUNT})");
		instances = sprites.Select((s, i) => CreatePreset(i, parent, group, s)).ToList();
		instances[CharmPreset.LocalSettings.ActivePreset].gameObject.SetActive(true);
	}

	private static PresetController CreatePreset(int num, GameObject parent, FadeGroup group, Sprite sprite) {
		GameObject go = GameObjectUtil.New("Preset " + num, parent, typeof(PresetController), typeof(SpriteRenderer));
		go.SetActive(false);
		go.transform.localPosition = Vector3.zero;
		go.layer = (int) PhysLayers.UI;

		PresetController pc = go.GetComponent<PresetController>();
		pc.num = num;

		SpriteRenderer sr = go.GetComponent<SpriteRenderer>();
		group.spriteRenderers = group.spriteRenderers.Append(sr).ToArray();
		sr.sprite = sprite;
		sr.color = new Color(1, 1, 1, 0);
		sr.sortingLayerID = 629535577;
		sr.sortingLayerName = "HUD";
		sr.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
		sr.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;

		return pc;
	}

	internal void Load() {
		LocalSettings settings = CharmPreset.LocalSettings;

		settings.Presets[settings.ActivePreset] = Ref.PD.GetVariable<List<int>>("equippedCharms").ToList();
		instances[settings.ActivePreset].gameObject.SetActive(false);

		settings.ActivePreset = num;
		gameObject.SetActive(true);

		CharmUtil.UnequipAllCharms();
		_ = CharmUtil.EquipCharms(settings.Presets[num].ToArray());
		CharmUtil.UpdateCharm();
	}

	internal static void LoadLast() {
		int toLoad = (CharmPreset.LocalSettings.ActivePreset - 1) switch {
			int i when i < 0 => PRESET_AMOUNT - 1,
			int i => i
		};

		instances[toLoad].Load();
	}

	internal static void LoadNext() {
		int toLoad = (CharmPreset.LocalSettings.ActivePreset + 1) switch {
			int i when i >= PRESET_AMOUNT => 0,
			int i => i
		};

		instances[toLoad].Load();
	}
}
