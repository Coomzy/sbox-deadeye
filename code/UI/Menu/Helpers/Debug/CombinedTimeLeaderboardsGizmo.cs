
using Sandbox.Services;
using Sandbox.Utility;
using System.Threading;
using System.Threading.Tasks;
using static Sandbox.Services.Stats;

public class CombinedTimeLeaderboardsGizmo : Component
{
	[Property] public CombinedTimeLeaderboards combinedTimeLeaderboards { get; set; }

	protected override void OnStart()
	{
		base.OnStart();

		GetLeaderboard();
	}

	[Button("Get Leaderboard")]
	public void GetLeaderboard()
	{
		if (combinedTimeLeaderboards == null)
			return;

		combinedTimeLeaderboards.GetLeaderboard();
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

		if (combinedTimeLeaderboards == null)
		{
			Vector2 pos = new Vector2(initalX, initalY);
			Gizmo.Draw.ScreenText("CombinedTimeLeaderboards Null!", pos);
			return;
		}

		for (int i = 0; i < combinedTimeLeaderboards.entries.Count; i++)
		{
			float x = initalX;
			float y = initalY + (size * i) + (offset * i);
			Vector2 pos = new Vector2(x, y);

			var entry = combinedTimeLeaderboards.entries[i];
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

	public override void Reset()
	{
		base.Reset();

		combinedTimeLeaderboards = Components.Get<CombinedTimeLeaderboards>();
	}
}
