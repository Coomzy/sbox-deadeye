using Sandbox;
using Sandbox.Citizen;
using System;
using System.Text.Json;
using static Sandbox.Gizmo;

public class UIManager : Component
{
	public static UIManager instance;

	[Property] public CurrentTimeWidget currentTimeWidget { get; private set; }
	[Property] public ReactTimeBarWidget reactTimeBarWidget { get; private set; }
	[Property] public DiedScreen diedScreen { get; private set; }
	[Property] public FailedTooManyCivsKilledScreen failedTooManyCivsKilledScreen { get; private set; }
	[Property] public WonScreen wonScreen { get; private set; }

	protected override void OnAwake()
	{
		instance = this;

		base.OnAwake();
	}

	public void Died()
	{
		//currentTimeWidget.Enabled = false;
		reactTimeBarWidget.Enabled = false;
		diedScreen.Enabled = true;
	}

	public void FailedTooManyCivsKilled()
	{
		//currentTimeWidget.Enabled = false;
		reactTimeBarWidget.Enabled = false;
		failedTooManyCivsKilledScreen.Enabled = true;
	}

	public void Won()
	{
		currentTimeWidget.Enabled = false;
		reactTimeBarWidget.Enabled = false;
		wonScreen.Enabled = true;
	}
}
