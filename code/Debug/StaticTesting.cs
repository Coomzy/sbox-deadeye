
using Sandbox;
using Sandbox.Citizen;
using System;
using System.Diagnostics.Metrics;
using System.Text.Json;
using static Sandbox.Gizmo;

public class StaticTesting : Component
{
	public bool useOneHandedMode = true;

	public static int counter { get; set; } = 0;

	protected override void OnStart()
	{
		base.OnStart();

		Test();
	}

	[Button("Test Log")]
	public void IncrementCounter()
	{
		Log.Info($"StaticTesting::IncrementCounter() counter = {counter}");
	}

	[Button("Test Log")]
	public void Test_log()
	{
		//Log.Info($"Before: GamePreferences.instance.useOneHandedMode = {GamePreferences.instance.useOneHandedMode}");
	}

	[Button("Test")]
	public void Test()
	{
		//Log.Info($"GamePreferences.instance = {GamePreferences._instance}");
		//Log.Info($"Before: GamePreferences.instance.useOneHandedMode = {GamePreferences.instance.useOneHandedMode}");
		//Log.Info($"GamePreferences.instance = {GamePreferences._instance}");

		//var inst = GamePreferences.instance;
		//inst.useOneHandedMode = useOneHandedMode;
		//Log.Info($"Before: GamePreferences.instance.useOneHandedMode = {GamePreferences.instance.useOneHandedMode}");
		//Log.Info($"inst.instance.useOneHandedMode = {inst.useOneHandedMode}");
		//Log.Info($"GamePreferences.instance = {GamePreferences._instance}");
		//inst.Save();

		//FileSystem.Data.WriteJson("Test.json", inst);

		//Log.Info($"After: GamePreferences.instance.useOneHandedMode = {GamePreferences.instance.useOneHandedMode}");
		//Log.Info($"GamePreferences.instance = {GamePreferences._instance}");
	}
}
