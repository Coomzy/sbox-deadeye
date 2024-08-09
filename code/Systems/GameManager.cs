
using Sandbox;
using Sandbox.Citizen;
using System;
using System.Text.Json;
using static Sandbox.Gizmo;

public class GameManager : Component
{
	public static GameManager instance;

	protected override void OnAwake()
	{
		instance = this;

		base.OnAwake();
	}

	[Button("Test")]
	public void Test()
	{
		Log.Info($"GamePreferences.instance = {GamePreferences._instance}");
		Log.Info($"Before: GamePreferences.instance.useOneHandedMode = {GamePreferences.instance.useOneHandedMode}");
		Log.Info($"GamePreferences.instance = {GamePreferences._instance}");

		GamePreferences.instance.useOneHandedMode = true;
		Log.Info($"GamePreferences.instance = {GamePreferences._instance}");
		//GamePreferences.instance.Save();

		Log.Info($"After: GamePreferences.instance.useOneHandedMode = {GamePreferences.instance.useOneHandedMode}");
		Log.Info($"GamePreferences.instance = {GamePreferences._instance}");
	}
}
