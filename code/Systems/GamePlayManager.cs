
using Sandbox;
using Sandbox.Citizen;
using Sandbox.Services;
using System;
using System.Diagnostics;
using System.Text.Json;
using static Sandbox.Gizmo;

public enum FailReason
{
	None,
	KilledTooManyCivs,
	Died
}

public class GamePlayManager : Component
{
	public static GamePlayManager instance;

	[Category("Runtime"), Property] public bool isPlayingLevel { get; private set; } = true;
	[Category("Runtime"), Property] public float endLevelTime { get; private set; }
	[Group("Runtime"), Property] public int civiliansKilled { get; set; }

	TimeSince timeSinceLevelStart { get; set; }

	public MedalType currentMedal => LevelData.active != null ? LevelData.active.TimeToMedalType(levelTime) : MedalType.None;

	public float levelTime
	{ 
		get
		{
			float time = timeSinceLevelStart;
			if (!isPlayingLevel)
			{
				time = endLevelTime;
			}
			time += civiliansKilled * GameSettings.instance.civilianKilledTimePenalty;
			return time;
		}
	}

	protected override void OnAwake()
	{
		instance = this;

		base.OnAwake();

		timeSinceLevelStart = 0.0f;
	}

	public void FailLevel(FailReason failReason)
	{
		switch (failReason)
		{
			case FailReason.KilledTooManyCivs:
				Stats.Increment(Stats.FAILURE_TOO_MANY_CIVS_KILLED);
				break;
			case FailReason.Died:
				Stats.Increment(Stats.DIED);
				break;
		}

		isPlayingLevel = false;
		endLevelTime = timeSinceLevelStart;
	}

	public void WonLevel()
	{
		Stats.Increment(Stats.WON);

		isPlayingLevel = false;
		endLevelTime = timeSinceLevelStart;

		string levelName = LevelData.active.ResourceName;
		bool isNewPersonalBest = true;

		if (GameSave.instance.levelNameToBestTime.TryGetValue(levelName, out float savedBestTime))
		{
			if (levelTime >= savedBestTime)
			{
				isNewPersonalBest = false;
			}
		}

		Log.Info($"WonLevel() levelTime: {levelTime}, savedBestTime: {savedBestTime}, isNewPersonalBest: {isNewPersonalBest}");

		if (isNewPersonalBest)
		{
			GameSave.instance.levelNameToBestTime[LevelData.active.ResourceName] = levelTime;
			GameSave.instance.Save();

			string leaderboardName = LevelData.active.leaderboardName;
			if (!string.IsNullOrEmpty(leaderboardName))
			{
				bool allowSubmit = !Game.IsEditor;
				if (allowSubmit)
				{
					Sandbox.Services.Stats.SetValue(leaderboardName, levelTime);
					Log.Info($"Submitting leaderboard '{leaderboardName}' for time '{levelTime}'");
				}
				else
				{
					Log.Info($"Not submitting leaderboard '{leaderboardName}' for time '{levelTime}'");
				}
			}
			else
			{
				Log.Error($"WonLevel() leaderboardName null for {levelName}");
			}
		}

		if (LevelData.active == null)
		{
			Log.Warning($"No active level data for scene {Game.ActiveScene}");
			return;
		}

		if (BotModePreferences.instance.IsInBotMode(PlayerBotMode.FastestTime))
		{
			LevelData.active.fastestTime = endLevelTime;
		}
		else if (BotModePreferences.instance.IsInBotMode(PlayerBotMode.SlowestTime))
		{
			LevelData.active.slowestTime = endLevelTime;
		}
	}

	[Button("Test")]
	public void Test()
	{
		Log.Info($"GameSave.instance.temp was '{GameSave.instance.temp}'");

		GameSave.instance.temp += 1.0f;
		GameSave.instance.Save();

		FileSystem.Data.WriteJson("GameSaveAlt.json", GameSave.instance);
	}
}
