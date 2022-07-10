using InvokeMethod = Osmi.FsmActions.InvokeMethod;

namespace CharmPreset;

public partial class CharmPreset {
	private static readonly Vector3 presetPosition = new(0.6f, 1.4f, -3.33f);

	private void BuildGUI() {
		Log("Building Charm Preset GUI");


		GameObject charmsPane = CharmUtil.CharmsPane;
		PlayMakerFSM charmsFsm = charmsPane.LocateMyFSM("UI Charms");
		FadeGroup group = charmsPane.GetComponent<FadeGroup>();

		GameObject presetGroup = GameObjectUtil.New("Presets", charmsPane, typeof(BoxCollider2D), typeof(ConstrainPosition));
		presetGroup.layer = (int) PhysLayers.UI;
		presetGroup.transform.position = presetPosition;
		presetGroup.GetComponent<BoxCollider2D>().size = new(1.3f, 1.3f);

		// Y position is sometines shifted by +100 unit for unknown reason after
		// entering save, so simply contrain it here until a better solution is found
		ConstrainPosition cp = presetGroup.GetComponent<ConstrainPosition>();
		cp.constrainY = true;
		cp.yMin = 1.4f;
		cp.yMax = 1.4f;


		PresetController.Init(presetGroup, group);


		FsmState stateToPreset = charmsFsm.AddState("To Preset");
		stateToPreset.Actions = new FsmStateAction[] {
			charmsFsm.GetAction<SetFsmString>("Notch?", 3),
			charmsFsm.GetAction<SetFsmString>("Notch?", 4),
			charmsFsm.GetAction<SetFsmInt>("Notch?", 5),
			charmsFsm.GetAction<SetFsmInt>("Notch?", 6),
			charmsFsm.GetAction<SendEventByName>("Notch?", 7),
			charmsFsm.GetAction<ActivateGameObject>("Con Action Down", 0),
			new SetFsmGameObject() {
				gameObject = new FsmOwnerDefault(),
				fsmName = "Update Cursor",
				variableName = "Item",
				setValue = presetGroup
			},
			charmsFsm.GetAction<SendEventByName>("Get Selected", 2),
			charmsFsm.GetAction<SendEventByName>("Get Selected", 3)
		}.Map(CloneUtil.CreateMemberwiseClone<FsmStateAction>).ToArray();
		charmsFsm.ChangeTransition("Up", "TO TOP", stateToPreset.Name);
		charmsFsm.ChangeTransition("Idle Collection", "UI RS UP", stateToPreset.Name);
		charmsFsm.ChangeTransition("Idle Equipped", "UI DOWN", stateToPreset.Name);
		charmsFsm.ChangeTransition("Idle Equipped", "UI RS DOWN", stateToPreset.Name);

		FsmState stateIdle = charmsFsm.AddState("Idle Preset");
		stateIdle.Actions = charmsFsm.GetState("Idle Equipped").Actions
			.Map(CloneUtil.CreateMemberwiseClone<FsmStateAction>).ToArray();
		stateToPreset.AddTransition(FsmEvent.Finished.Name, stateIdle.Name);
		stateIdle.AddTransition("UI UP", "To Equipment");
		stateIdle.AddTransition("UI RS UP", "To Equipment");
		stateIdle.AddTransition("UI DOWN", "To Bot");
		stateIdle.AddTransition("UI RS DOWN", "To Bot");


		FsmState stateBenchReminder = charmsFsm.CopyState("Bench Reminder", "Bench Reminder Preset");
		stateBenchReminder.ChangeTransition(FsmEvent.Finished.Name, stateIdle.Name);

		FsmState stateBoundReminder = charmsFsm.CopyState("Bound Reminder", "Bound Reminder Preset");
		stateBoundReminder.ChangeTransition(FsmEvent.Finished.Name, stateIdle.Name);

		PlayerDataBoolTest benchTest = charmsFsm.GetAction<PlayerDataBoolTest>("Deactivate UI", 2);
		GGCheckBoundCharms boundTest = charmsFsm.GetAction<GGCheckBoundCharms>("Deactivate UI", 3);
		AudioPlayerOneShotSingle audioPlay = charmsFsm.GetAction<AudioPlayerOneShotSingle>("Tween Up", 1);


		FsmState stateLoadLast = charmsFsm.AddState("Preset Load Last");
		stateLoadLast.Actions = new FsmStateAction[] {
			boundTest,
			benchTest,
			audioPlay,
			new InvokeMethod(PresetController.LoadLast)
		}.Map(CloneUtil.CreateMemberwiseClone<FsmStateAction>).ToArray();
		stateIdle.AddTransition("UI LEFT", stateLoadLast.Name);
		stateLoadLast.AddTransition("NOT BENCH", stateBenchReminder.Name);
		stateLoadLast.AddTransition("CHARM BOUND", stateBoundReminder.Name);
		stateLoadLast.AddTransition(FsmEvent.Finished.Name, stateIdle.Name);

		FsmState stateLoadNext = charmsFsm.AddState("Preset Load Next");
		stateLoadNext.Actions = new FsmStateAction[] {
			boundTest,
			benchTest,
			audioPlay,
			new InvokeMethod(PresetController.LoadNext)
		}.Map(CloneUtil.CreateMemberwiseClone<FsmStateAction>).ToArray();
		stateIdle.AddTransition("UI RIGHT", stateLoadNext.Name);
		stateIdle.AddTransition("UI CONFIRM", stateLoadNext.Name);
		stateLoadNext.AddTransition("NOT BENCH", stateBenchReminder.Name);
		stateLoadNext.AddTransition("CHARM BOUND", stateBoundReminder.Name);
		stateLoadNext.AddTransition(FsmEvent.Finished.Name, stateIdle.Name);
	}
}
