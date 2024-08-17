
using Sandbox.Services;
using Sandbox.Utility;
using System.Threading.Tasks;
using static Sandbox.Services.Stats;

public struct GlobalLeaderboardEntry
{
	[Property] public long rank { get; set; }
	[Property] public string displayName { get; set; }
	[Property] public double combinedTimeRaw { get; set; }
	[Property] public string combinedTime => FormatTime((float)combinedTimeRaw);
	[Property] public MedalType medalType { get; set; }
	[Property] public bool isMe { get; set; }

	public string FormatTime(float time)
	{
		int minutes = (int)(time / 60);
		float seconds = time % 60;

		return string.Format("{0:00}:{1:00.000}", minutes, seconds);
	}
}

public class GlobalLeaderboards : Component
{
	[Property] public List<GlobalLeaderboardEntry> globalLeaderboards { get; set; } = new List<GlobalLeaderboardEntry>();

	[Property, ReadOnly] public bool isRefreshing { get; set; } = false;

	protected override void OnStart()
	{
		base.OnStart();

		GetGlobalLeaderboard();
	}

	[Button("Set Stats")]
	public void SetStats()
	{
		Sandbox.Services.Stats.SetValue(GameStats.COMBINED_TIME, 9000);
		Sandbox.Services.Stats.SetValue(GameStats.LOWEST_MEDAL, 1);
	}

	[Button("Get Stat")]
	public void GetStat()
	{
		var statName = GameStats.LOWEST_MEDAL;
		statName = GameStats.TARGETS_ELIMINATED;
		//GetStats(statName);
		GetAltStats(statName);
	}

	public async void GetStats(string statName)
	{
		var localStat = Sandbox.Services.Stats.LocalPlayer.Get(statName);
		var globalStat = Sandbox.Services.Stats.Global.Get(statName);

		var localRefreshTask = Sandbox.Services.Stats.LocalPlayer.Refresh();
		var globalRefreshTask = Sandbox.Services.Stats.Global.Refresh();

		await Task.WhenAll(localRefreshTask, globalRefreshTask);

		Log.Info($"{statName} local: {localStat.Value}, global: {globalStat.Value}");
	}

	public void GetAltStats(string statName)
	{
		var s = Sandbox.Services.Stats.LocalPlayer;
		s.Refresh().ContinueWith(x => 
		{
			foreach (var stat in s)
			{
				Log.Info($"Stat: name: {stat.Name}, title: {stat.Title} unit: {stat.Unit}, value: {stat.Value}");
			}
		}
		);
		/*var localStat =  .Get(statName);
		var globalStat = Sandbox.Services.Stats.Global.Get(statName);

		var localRefreshTask = Sandbox.Services.Stats.LocalPlayer.Refresh();
		var globalRefreshTask = Sandbox.Services.Stats.Global.Refresh();

		await Task.WhenAll(localRefreshTask, globalRefreshTask);

		Log.Info($"{statName} local: {localStat.Value}, global: {globalStat.Value}");*/
	}

	[Button("Get Global Leaderboard")]
	public void GetGlobalLeaderboard()
	{
		if (isRefreshing)
			return;

		GetLeaderboard();
	}

	async void GetLeaderboard()
	{
		if (isRefreshing)
			return;

		globalLeaderboards.Clear();
		logs.Clear();
		isRefreshing = true;
		logs.Add("Refreshing");

		var board = await GameLeaderboards.GetLeaderboard(GameLeaderboards.COMBINED_TIME, LeaderboardGroup.Friends);
		
		List<PlayerStats> leaderboardPlayerStats = new List<PlayerStats>();
		foreach (var entry in board.Entries)
		{
			//string log = $"[{entry.Rank}] {entry.DisplayName} - {entry.Value} - Me: {entry.Me}";
			//Log.Info(log);
			//logs.Add(log);

			var lowestMedalStat = Sandbox.Services.Stats.GetPlayerStats(GameStats.LOWEST_MEDAL, entry.SteamId);
			leaderboardPlayerStats.Add(lowestMedalStat);
		}

		// Wait for all stats to refresh
		foreach (var stat in leaderboardPlayerStats)
		{
			while (stat.IsRefreshing)
			{
				await Task.Frame();
			}
		}

		//await Stats.Global.Refresh();

		logs.Clear();
		for (int i = 0; i < board.Entries.Length; i++)
		{
			var boardEntry = board.Entries[i];
			var playerStats = leaderboardPlayerStats[i];
			var lowestMedalStat = playerStats.Get(GameStats.LOWEST_MEDAL);

			var leaderboardEntry = new GlobalLeaderboardEntry();

			leaderboardEntry.rank = boardEntry.Rank;
			leaderboardEntry.displayName = boardEntry.DisplayName;
			leaderboardEntry.combinedTimeRaw = boardEntry.Value;
			leaderboardEntry.isMe = boardEntry.Me;
			leaderboardEntry.medalType = (MedalType)(int)lowestMedalStat.Value;

			globalLeaderboards.Add(leaderboardEntry);

			string log = $"[{boardEntry.Rank}] {boardEntry.DisplayName} - combined time: {boardEntry.Value} - lowestMedalStat.Value: {lowestMedalStat.Value} - Me: {boardEntry.Me}";
			Log.Info(log);
			//logs.Add(log);
		}

		isRefreshing = false;
	}

	protected override void DrawGizmos()
	{
		base.DrawGizmos();

		DrawLeaderboards();
	}

	void DrawLeaderboards()
	{
		float size = 12.0f;
		float offset = 5.0f;
		float initalX = 10.0f;
		float initalY = 10;

		for (int i = 0; i < globalLeaderboards.Count; i++)
		{
			float x = initalX;
			float y = initalY + (size * i) + (offset * i);
			Vector2 pos = new Vector2(x, y);

			var entry = globalLeaderboards[i];
			string log = $"[{entry.rank}] {entry.displayName} - Combined Time: {entry.combinedTime} - Lowest Medal: {entry.medalType} - Me: {entry.isMe}";
			Gizmo.Draw.ScreenText(log, pos);
		}
	}


	[Property] public List<string> logs { get; set; } = new List<string>();
	void DrawLogs()
	{
		float size = 12.0f;
		float offset = 5.0f;
		float initalX = 10.0f;
		float initalY = 10;

		for (int i = 0; i < logs.Count; i++)
		{
			float x = initalX;
			float y = initalY + (size * i) + (offset * i);
			Vector2 pos = new Vector2(x, y);
			Gizmo.Draw.ScreenText(logs[i], pos);
		}
	}
}
