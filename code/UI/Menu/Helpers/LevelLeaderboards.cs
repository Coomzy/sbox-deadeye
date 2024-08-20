
using Sandbox.Services;
using Sandbox.UI.Menu.Helpers;
using Sandbox.Utility;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using static Sandbox.Services.Stats;

public class LevelLeaderboards : Component, ILeaderboard
{
	public Leaderboards.Board board { get; set; } = null;

	[Property, ReadOnly] public bool isRefreshing => cancellationTokenSource != null;
	CancellationTokenSource cancellationTokenSource { get; set; } = null;

	public LevelData context { get; set; }

	public void SetContext(LevelData data)
	{
		context = data; 
	}

	[Button("Cancel")]
	public void Cancel()
	{
		cancellationTokenSource?.Cancel();
		cancellationTokenSource = null;
	}

	public async void GetLeaderboard(LeaderboardGroup group)
	{
		if (context == null)
		{
			Log.Info($"GetLevelLeaderboard() Failed! LevelData context was null");
			return;
		}

		board = null;

		Cancel();

		cancellationTokenSource = new CancellationTokenSource();
		board = await GameLeaderboards.GetLeaderboard(context.leaderboardName, group, cancellationTokenSource.Token);

		cancellationTokenSource = null;
	}

	protected override void OnDisabled()
	{
		Cancel();
		base.OnDisabled();
	}
}
