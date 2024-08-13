using Sandbox;
using Sandbox.Citizen;
using System;
using System.Text.Json;
using static Sandbox.Gizmo;

public class UIManager : Component
{
	public static UIManager instance;

	[Group("Setup"), Property] public CurrentTimeWidget currentTimeWidget { get; private set; }
	[Group("Setup"), Property] public ReactTimeBarWidget reactTimeBarWidget { get; private set; }
	[Group("Setup"), Property] public DiedScreen diedScreen { get; private set; }
	[Group("Setup"), Property] public FailedTooManyCivsKilledScreen failedTooManyCivsKilledScreen { get; private set; }
	[Group("Setup"), Property] public WonScreen wonScreen { get; private set; }

	[Group("Setup"), Property] public BlackFadeWidget blackFadeWidget { get; private set; }

	[Group("Runtime"), Property] public float blackFadeAlpha { get; set; } = 1.0f;

	protected override void OnAwake()
	{
		instance = this;

		base.OnAwake();
	}

	public void Died()
	{
		//if (currentTimeWidget != null) //currentTimeWidget.Enabled = false;
		if (reactTimeBarWidget != null) reactTimeBarWidget.Enabled = false;
		if (diedScreen != null) diedScreen.Enabled = true;
	}

	public void FailedTooManyCivsKilled()
	{
		//if (currentTimeWidget != null) currentTimeWidget.Enabled = false;
		if (reactTimeBarWidget != null) reactTimeBarWidget.Enabled = false;
		if (failedTooManyCivsKilledScreen != null) failedTooManyCivsKilledScreen.Enabled = true;
	}

	public void Won()
	{
		if (currentTimeWidget != null) currentTimeWidget.Enabled = false;
		if (reactTimeBarWidget != null) reactTimeBarWidget.Enabled = false;
		if (wonScreen != null) wonScreen.Enabled = true;
	}
}
