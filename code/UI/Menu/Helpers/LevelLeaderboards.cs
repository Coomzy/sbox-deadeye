
using Sandbox.Services;
using Sandbox.Utility;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using static Sandbox.Services.Stats;

public class LevelLeaderboards : Component
{
	public Leaderboards.Board board { get; set; } = null;
	[Property, ReadOnly] public bool isRefreshing => cancellationTokenSource != null;
	CancellationTokenSource cancellationTokenSource { get; set; } = null;

	public async void GetLevelLeaderboard(LevelData level, LeaderboardGroup group)
	{
		if (level == null)
		{
			Log.Info($"GetLevelLeaderboard() Failed! level was null");
			return;
		}

		board = null;

		if (cancellationTokenSource != null)
		{
			cancellationTokenSource.Cancel();
		}
		cancellationTokenSource = new CancellationTokenSource();
		board = await GameLeaderboards.GetLeaderboard(level.leaderboardName, group, cancellationTokenSource.Token);

		cancellationTokenSource = null;
	}

	[Button("Force Stop")]
	public void ForceStop()
	{
		if (cancellationTokenSource != null)
		{
			cancellationTokenSource.Cancel();
		}
		cancellationTokenSource = null;
	}

	protected override void OnDisabled()
	{
		ForceStop();
		base.OnDisabled();
	}
}
