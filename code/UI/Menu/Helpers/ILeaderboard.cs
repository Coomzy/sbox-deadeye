using Sandbox.Services;

namespace Sandbox.UI.Menu.Helpers
{
	public interface ILeaderboard
	{
		public Leaderboards.Board board { get; set; }
		public LevelData context { get; set; }
		public bool isRefreshing { get; }

		public void SetContext(LevelData data);
		public void Cancel();
		public void GetLeaderboard(LeaderboardGroup group);
	}
}
