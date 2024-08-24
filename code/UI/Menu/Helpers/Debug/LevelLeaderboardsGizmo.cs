
using Sandbox.Services;
using Sandbox.Utility;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using static Sandbox.Services.Stats;

public class LevelLeaderboardsGizmo : Component
{
	[Property] public LevelLeaderboards levelLeaderboards { get; private set; }
	[Property] public LeaderboardGroup leaderboardGroup { get; set; } = LeaderboardGroup.Global;
	[Property] public LevelData testLevelData { get; set; }

	protected override void OnStart()
	{
		base.OnStart();

		GetLevelLeaderboard();
	}

	[Button("Get Level Leaderboard")]
	public void GetLevelLeaderboard()
	{
		if (levelLeaderboards == null)
			return;

		levelLeaderboards.SetContext(testLevelData);
		levelLeaderboards.GetLeaderboard(leaderboardGroup);
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
		float initalX = 500.0f;
		float initalY = 10;

		if (levelLeaderboards == null)
		{
			Vector2 pos = new Vector2(initalX, initalY);
			Gizmo.Draw.ScreenText("Level Leaderboards Null!", pos);
			return;
		}

		if (levelLeaderboards.isRefreshing)
		{
			Vector2 pos = new Vector2(initalX, initalY);
			Gizmo.Draw.ScreenText("Refreshing", pos);
			return;
		}

		if (levelLeaderboards.board?.Entries == null)
		{
			Vector2 pos = new Vector2(initalX, initalY);
			Gizmo.Draw.ScreenText("No Entries", pos);
			return;
		}

		for (int i = 0; i < levelLeaderboards.board.Entries.Length; i++)
		{
			float x = initalX;
			float y = initalY + (size * i) + (offset * i);
			Vector2 pos = new Vector2(x, y);

			var entry = levelLeaderboards.board.Entries[i];
			string log = $"[{entry.Rank}] {entry.DisplayName} - Level Time: {UIManager.FormatTime(entry.Value)} - Me: {entry.SteamId == levelLeaderboards.board.TargetSteamId}";
			Gizmo.Draw.ScreenText(log, pos);
		}
	}

	public override void Reset()
	{
		base.Reset();

		levelLeaderboards = Components.Get<LevelLeaderboards>();
	}
}
