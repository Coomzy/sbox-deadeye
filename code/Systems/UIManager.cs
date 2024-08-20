using Sandbox;
using Sandbox.Citizen;
using System;
using System.Text.Json;
using static Sandbox.Gizmo;

public class UIManager : Component
{
	public static UIManager instance;

	[Group("Setup"), Property] public CurrentTimeWidget currentTimeWidget { get; private set; }
	[Group("Setup"), Property] public TimeAddedWidget timeAddedWidget { get; private set; }
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
		if (timeAddedWidget != null) timeAddedWidget.Enabled = false;
		if (diedScreen != null) diedScreen.Enabled = true;
	}

	public void FailedTooManyCivsKilled()
	{
		//if (currentTimeWidget != null) currentTimeWidget.Enabled = false;
		if (reactTimeBarWidget != null) reactTimeBarWidget.Enabled = false;
		if (timeAddedWidget != null) timeAddedWidget.Enabled = false;
		if (failedTooManyCivsKilledScreen != null) failedTooManyCivsKilledScreen.Enabled = true;
	}

	public void Won()
	{
		if (currentTimeWidget != null) currentTimeWidget.Enabled = false;
		if (reactTimeBarWidget != null) reactTimeBarWidget.Enabled = false;
		if (timeAddedWidget != null) timeAddedWidget.Enabled = false;
		if (wonScreen != null) wonScreen.Enabled = true;
	}

	public static string FormatTime(double time)
	{
		return FormatTime((float)time);
	}

	public static string FormatTime(float time, bool useSign = false)
	{
		int minutes = (int)(time / 60);
		float seconds = time % 60;
		
		string sign = minutes < 0 || seconds < 0 ? "-" : "+";

		return (useSign ? sign : "") + string.Format("{0:00}:{1:00.000}", MathF.Abs(minutes), MathF.Abs(seconds));
	}
}
