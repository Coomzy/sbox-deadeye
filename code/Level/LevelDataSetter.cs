
using Sandbox.Citizen;

public class LevelDataSetter : Component
{
	[Group("Setup"), Property] public LevelData levelData { get; set; }

	protected override void OnAwake()
	{
		base.OnAwake();

		LevelData.active = levelData;

		// Set the level select to our current level
		if (GameSettings.instance != null && GameSettings.instance.topDownLevels != null)
		{
			for (int i = 0; i < GameSettings.instance.topDownLevels.Count; i++)
			{
				var level = GameSettings.instance.topDownLevels[i];
				if (level == null)
					continue;

				if (level != levelData)
					continue;

				LevelSelectScreen.currentIndex = i;
				break;
			}
		}
	}

	[Button]
	public void Test()
	{
		Sandbox.Services.Stats.SetValue("set-value-only", 1);
	}

	[Button]
	public void GetAlt()
	{
		var localStat = Sandbox.Services.Stats.LocalPlayer["set-value-only"];
		Log.Info($"Value: {localStat.Value}, FirstValue: {localStat.FirstValue}, LastValue: {localStat.LastValue}, Min: {localStat.Min}, Max: {localStat.Max}, Avg: {localStat.Avg}, Sum: {localStat.Sum}");
	}

	[Button]
	public void LogSetValueOnly()
	{
		//LogStat("set-value-only");
		LogStat(GameStats.TARGETS_ELIMINATED);
	}

	public void LogStat(string stat)
	{
		var localStat = Sandbox.Services.Stats.LocalPlayer[stat];
		Log.Info($"Stat Name: {localStat.Name}");
		Log.Info($"Value: {localStat.Value}");
		Log.Info($"FirstValue: {localStat.FirstValue}");
		Log.Info($"LastValue: {localStat.LastValue}");
		Log.Info($"Min: {localStat.Min}");
		Log.Info($"Max: {localStat.Max}");
		Log.Info($"Avg: {localStat.Avg}");
		Log.Info($"Sum: {localStat.Sum}");
	}
}
