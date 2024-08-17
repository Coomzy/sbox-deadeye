
using Sandbox;
using Sandbox.Citizen;
using System;
using System.Text.Json;
using System.Threading.Tasks;
using Sandbox.Services;
using static Sandbox.Gizmo;

public enum LeaderboardGroup
{
	Global,
	Country,
	Friends
}

public static class GameLeaderboards
{
	public const string LOWEST_MEDAL = "lowest-medal-lb";
	public const string COMBINED_TIME = "combined-time-lb";

	public static void SetLeaderboardLevelTime(LevelData levelData, float levelTime)
	{
		SetLeaderboard(levelData.ResourceName, levelTime);
	}

	public static void SetLeaderboard(string leaderboardName, double value)
	{
		if (Game.IsEditor)
		{
			return;
		}

		if (string.IsNullOrEmpty(leaderboardName))
		{
			Log.Error($"SetLeaderboard() leaderboardName null");
			return;
		}

		bool allowSubmit = !Game.IsEditor;
		if (!allowSubmit)
		{
			Log.Info($"Not submitting leaderboard '{leaderboardName}' for time '{value}'");
			return;
		}

		Sandbox.Services.Stats.SetValue(leaderboardName, value);
		Log.Info($"Submitting leaderboard '{leaderboardName}' for time '{value}'");
	}

	public static async Task<Leaderboards.Board> GetLeaderboard(string leaderboardName, LeaderboardGroup group, int maxEntries = 10)
	{
		var board = Sandbox.Services.Leaderboards.Get(leaderboardName);

		board.MaxEntries = maxEntries;
		board.Group = GetLeaderboardGroup(group);

		await board.Refresh();

		return board;
	}

	public static string GetLeaderboardGroup(LeaderboardGroup group)
	{
		switch (group)
		{
			case LeaderboardGroup.Global:
				return "global";
			case LeaderboardGroup.Country:
				return "country";
			case LeaderboardGroup.Friends:
				return "friends";
		}
		return null;
	}
}
