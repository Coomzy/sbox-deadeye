
using Sandbox;
using Sandbox.Citizen;
using System;
using System.Text.Json;
using System.Threading;
using static Sandbox.Gizmo;
using static Sandbox.Services.Stats;

public class StatAndLeaderboardTest : Component
{
	CancellationTokenSource cancellationTokenSource { get; set; } = null;

	protected override void OnStart()
	{
		base.OnStart();

		GetStats();
		GetStatsAlt();
		GetLeaderboard();

		// For beating a level
		//Sandbox.Services.Stats.SetValue("level-original", 900);
	}

	[Button("Set Stats")]
	void SetStats()
	{
		//Sandbox.Services.Stats.SetValue("level-original", 1);
		Sandbox.Services.Stats.Increment("level-original", -double.MaxValue);
		Log.Info($"Set Stats!");
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
		if (cancellationTokenSource != null)
		{
			cancellationTokenSource.Cancel();
		}
		cancellationTokenSource = new CancellationTokenSource();

		var board = await GameLeaderboards.GetLeaderboard(GameLeaderboards.COMBINED_TIME, LeaderboardGroup.Global, 10, cancellationTokenSource.Token);
		//var board = await GameLeaderboards.GetLeaderboard("level-original", LeaderboardGroup.Friends, 10);

		Log.Info($"Board: {board.DisplayName}, Group: {board.Title}, entries: {board.TotalEntries}");

		foreach (var e in board.Entries)
		{
			Log.Info($"[{e.Rank}] {e.DisplayName} - {e.Value} - Me: {e.Me}");
		}
	}
}
