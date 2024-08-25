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
	[Group("Setup"), Property] public AfterActionReportScreen afterActionReportScreen { get; private set; }
	[Group("Setup"), Property] public DiedScreen diedScreen { get; private set; }
	[Group("Setup"), Property] public FailedTooManyCivsKilledScreen failedTooManyCivsKilledScreen { get; private set; }
	[Group("Setup"), Property] public WonScreen wonScreen { get; private set; }

	[Group("Setup"), Property] public LeaderboardsScreen leaderboardsScreen { get; private set; }
	[Group("Setup"), Property] public LevelLeaderboards levelLeaderboards { get; private set; }

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
		//if (timeAddedWidget != null) timeAddedWidget.Enabled = false;
		if (reactTimeBarWidget != null) reactTimeBarWidget.Enabled = false;
		//if (timeAddedWidget != null) timeAddedWidget.Enabled = false;
		//if (currentTimeWidget != null) currentTimeWidget.Enabled = false;
		//if (leaderboardsScreen != null) leaderboardsScreen.Enabled = false;
		//if (diedScreen != null) diedScreen.Enabled = true;
		if (afterActionReportScreen != null) afterActionReportScreen.Enabled = true;
	}

	public void FailedTooManyCivsKilled()
	{
		//if (currentTimeWidget != null) currentTimeWidget.Enabled = false;
		//if (timeAddedWidget != null) timeAddedWidget.Enabled = false;
		if (reactTimeBarWidget != null) reactTimeBarWidget.Enabled = false;
		//if (failedTooManyCivsKilledScreen != null) failedTooManyCivsKilledScreen.Enabled = true;
		if (afterActionReportScreen != null) afterActionReportScreen.Enabled = true;
	}

	public void Won()
	{
		//if (currentTimeWidget != null) currentTimeWidget.Enabled = false;
		//if (timeAddedWidget != null) timeAddedWidget.Enabled = false;
		if (reactTimeBarWidget != null) reactTimeBarWidget.Enabled = false;
		//if (wonScreen != null) wonScreen.Enabled = true;
		if (afterActionReportScreen != null) afterActionReportScreen.Enabled = true;
	}

	public void CivilianKilled()
	{
		if (timeAddedWidget != null) 
		{
			timeAddedWidget.Enabled = true;
			timeAddedWidget.AddTimePlus5s();
		}
	}

	public void OpenLevelLeaderboard()
	{
		if (levelLeaderboards == null)
		{
			Log.Warning($"UI Manager no levelLeaderboards");
		}

		if (leaderboardsScreen == null)
		{
			Log.Warning($"UI Manager no leaderboardsScreen");
			return;
		}

		if (leaderboardsScreen != null)
		{			
			leaderboardsScreen.SetupLevelLeaderboard(LevelData.active);
		}

		if (currentTimeWidget != null) currentTimeWidget.Enabled = false;
		if (timeAddedWidget != null) timeAddedWidget.Enabled = false;
		if (afterActionReportScreen != null) afterActionReportScreen.Enabled = false;
	}

	public async void CloseLeaderboard()
	{
		await Task.FrameEnd();

		if (leaderboardsScreen != null)
		{
			leaderboardsScreen.Enabled = false;
		}

		if (currentTimeWidget != null) currentTimeWidget.Enabled = true;
		//if (timeAddedWidget != null) timeAddedWidget.Enabled = true;
		if (afterActionReportScreen != null) afterActionReportScreen.Enabled = true;
	}

	public static string FormatTime(double time)
	{
		return FormatTime((float)time);
	}

	public static string FormatTime(float time, bool useSign = false)
	{
		float seconds = time % 60;
		
		string sign = seconds < 0 ? "-" : "+";

		return (useSign ? sign : "") + string.Format("{0:00.000}", MathF.Abs(seconds));
	}
}
