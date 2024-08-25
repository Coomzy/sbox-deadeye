
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
	public const string LOWEST_MEDAL = "lowest-medal";
	public const string COMBINED_TIME = "combined-time";

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
}
