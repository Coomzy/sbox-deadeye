
using Sandbox;
using Sandbox.Citizen;
using System;
using System.Text.Json;
using static Sandbox.Gizmo;

public class StatAndLeaderboardTest : Component
{
	protected override void OnStart()
	{
		base.OnStart();

		GetStats();
		GetStatsAlt();
		GetLeaderboard();

		// For beating a level
		//Sandbox.Services.Stats.SetValue("level-original", 900);
	}

	[Button("GetStats")]
	async void GetStats()
	{
		var localStat = Sandbox.Services.Stats.LocalPlayer.Get("targets-eliminated");
		var globalStat = Sandbox.Services.Stats.Global.Get("targets-eliminated");

		var localRefreshTask = Sandbox.Services.Stats.LocalPlayer.Refresh();
		var globalRefreshTask = Sandbox.Services.Stats.Global.Refresh();

		await Task.WhenAll(localRefreshTask, globalRefreshTask);

		Log.Info($"targets-eliminated local {localStat.Value}, global {globalStat.Value}");
	}

	[Button("GetStatsAlt")]
	async void GetStatsAlt()
	{
		await GameTask.DelaySeconds(5.0f);

		var localStat = Sandbox.Services.Stats.LocalPlayer.Get("level-original");
		var globalStat = Sandbox.Services.Stats.Global.Get("level-original");

		var localRefreshTask = Sandbox.Services.Stats.LocalPlayer.Refresh();
		var globalRefreshTask = Sandbox.Services.Stats.Global.Refresh();

		await Task.WhenAll(localRefreshTask, globalRefreshTask);

		Log.Info($"level-original local {localStat.Value}, global {globalStat.Value}");
	}

	[Button("GetLeaderboard")]
	async void GetLeaderboard()
	{
		// get a board class for the board in my package called "decapitations"
		var board = Sandbox.Services.Leaderboards.Get("level-original");

		// return max 10 entries
		board.MaxEntries = 10;

		// compare against people in my country
		board.Group = "country";

		await board.Refresh();

		Log.Info($"Got board {board.DisplayName}, {board.Title}");
		Log.Info($"Board has {board.TotalEntries} total entries!");

		// print entries
		foreach (var e in board.Entries)
		{
			Log.Info($"[{e.Rank}] {e.DisplayName} - {e.Value} - Me: {e.Me}");
		}
	}
}
