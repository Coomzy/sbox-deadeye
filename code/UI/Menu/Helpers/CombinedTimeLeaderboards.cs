
using Sandbox.Services;
using Sandbox.Utility;
using System.Threading;
using System.Threading.Tasks;
using static Sandbox.Services.Stats;

public struct CombinedTimeLeaderboardEntry
{
	[Property] public long rank { get; set; }
	[Property] public string displayName { get; set; }
	[Property] public double combinedTimeRaw { get; set; }
	[Property] public string combinedTime => UIManager.FormatTime(combinedTimeRaw);
	[Property] public MedalType medalType { get; set; }
	[Property] public bool isMe { get; set; }
}

public class CombinedTimeLeaderboards : Component
{
	[Property] public List<CombinedTimeLeaderboardEntry> entries { get; set; } = new List<CombinedTimeLeaderboardEntry>();

	[Property, ReadOnly] public bool isRefreshing => cancellationTokenSource != null;
	CancellationTokenSource cancellationTokenSource { get; set; } = null;

	public async void GetLeaderboard()
	{
		entries.Clear();

		if (cancellationTokenSource != null)
		{
			cancellationTokenSource.Cancel();
		}
		cancellationTokenSource = new CancellationTokenSource();

		var board = await GameLeaderboards.GetLeaderboard(GameLeaderboards.COMBINED_TIME, LeaderboardGroup.Friends, cancellationTokenSource.Token);
		
		/*List<PlayerStats> leaderboardPlayerStats = new List<PlayerStats>();
		foreach (var entry in board.Entries)
		{
			//string log = $"[{entry.Rank}] {entry.DisplayName} - {entry.Value} - Me: {entry.Me}";
			//Log.Info(log);

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
		}*/

		//await Stats.Global.Refresh();

		for (int i = 0; i < board.Entries.Length; i++)
		{
			var boardEntry = board.Entries[i];
			//var playerStats = leaderboardPlayerStats[i];
			//var lowestMedalStat = playerStats.Get(GameStats.LOWEST_MEDAL);

			var leaderboardEntry = new CombinedTimeLeaderboardEntry();

			leaderboardEntry.rank = boardEntry.Rank;
			leaderboardEntry.displayName = boardEntry.DisplayName;
			leaderboardEntry.combinedTimeRaw = boardEntry.Value;
			leaderboardEntry.isMe = boardEntry.Me;
			//leaderboardEntry.medalType = (MedalType)(int)lowestMedalStat.Value;

			entries.Add(leaderboardEntry);

			//string log = $"[{boardEntry.Rank}] {boardEntry.DisplayName} - combined time: {boardEntry.Value} - Me: {boardEntry.Me}";
			//string log = $"[{boardEntry.Rank}] {boardEntry.DisplayName} - combined time: {boardEntry.Value} - lowestMedalStat.Value: {lowestMedalStat.Value} - Me: {boardEntry.Me}";
			//Log.Info(log);
		}

		cancellationTokenSource = null;
	}
}
