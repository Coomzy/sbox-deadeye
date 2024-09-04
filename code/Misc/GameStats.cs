
using Sandbox;
using Sandbox.Citizen;
using System;
using System.Text.Json;
using static Sandbox.Gizmo;

public static class GameStats
{
	public const string TARGETS_ELIMINATED = "targets-eliminated";
	public const string CIVILIANS_KILLED = "civilians-killed";
	public const string FAILURE_TOO_MANY_CIVS_KILLED = "failure-too-many-civs-killed";
	public const string WON = "won";
	public const string DIED = "died";
	public const string KILLED_TONY_LAZUTO = "killed-tony-lazuto";
	public const string LOWEST_MEDAL = "lowest-medal";
	//public const string COMBINED_TIME = "combined-time";
	public const string COMBINED_TIME = "combined-time2";
	public const string LEVEL_LOADS = "level-loads";
	public const string LEVEL_EXITS = "level-exits";
	public const string ATTEMPTS = "attempts";
	public const string RESTARTS = "restarts";
	public const string LEVEL_FURTHEST_PLAYED = "level-far-played";
	public const string LEVEL_FURTHEST_BEAT = "level-far-beat";
	public const string MARATHON_MODE = "marathon-mode";

	public static void Increment(string stat, double incrementAmount = 1)
	{
		// Once the game is published, we'll stop editor stats
		if (Game.IsEditor)
		{
			//return;
		}
		Sandbox.Services.Stats.Increment(stat, incrementAmount);
	}

	public static void Set(string stat, double amount)
	{
		// Once the game is published, we'll stop editor stats
		if (Game.IsEditor)
		{
			//return;
		}
		Sandbox.Services.Stats.SetValue(stat, amount);
	}

	[ConCmd("stats_reset")]
	public static void Reset()
	{
		Sandbox.Services.Stats.SetValue(TARGETS_ELIMINATED, 0);
		Sandbox.Services.Stats.SetValue(CIVILIANS_KILLED, 0);
		Sandbox.Services.Stats.SetValue(FAILURE_TOO_MANY_CIVS_KILLED, 0);
		Sandbox.Services.Stats.SetValue(WON, 0);
		Sandbox.Services.Stats.SetValue(DIED, 0);
		Sandbox.Services.Stats.SetValue(LOWEST_MEDAL, 0);
		Sandbox.Services.Stats.SetValue(COMBINED_TIME, 99999);

		foreach (var level in GameSettings.instance.topDownLevels)
		{
			if (level == null || !level.isLeaderboardLevel)
				continue;

			Sandbox.Services.Stats.SetValue(level.statName, level.slowestTime);
		}
	}

	[ConCmd("stats_fix")]
	public static void FixStats()
	{
		//Sandbox.Services.Stats.SetValue(LOWEST_MEDAL, 0);

		float slowestTime = 0.0f;
		foreach (var level in GameSettings.instance.topDownLevels)
		{
			Log.Info($"FixStats() level = {level}");
			if (level == null || !level.isLeaderboardLevel)
				continue;

			slowestTime += level.slowestTime;
		}

		Log.Info($"FixStats() slowestTime = {slowestTime}");
		Sandbox.Services.Stats.SetValue(COMBINED_TIME, slowestTime);
	}
}
