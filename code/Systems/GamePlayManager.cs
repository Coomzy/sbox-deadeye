
using Sandbox;
using Sandbox.Citizen;
using Sandbox.Services;
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
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
	[Group("Runtime"), Property] public bool isNewPersonalBest { get; set; }
	[Group("Runtime"), Property] public float? previousBestTime { get; set; }

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

	public float levelTimeDiff
	{
		get
		{
			if (previousBestTime == null)
			{
				return 0.0f;
			}
			return levelTime - previousBestTime.Value;
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
				GameStats.Increment(GameStats.FAILURE_TOO_MANY_CIVS_KILLED);
				break;
			case FailReason.Died:
				GameStats.Increment(GameStats.DIED);
				break;
		}

		isPlayingLevel = false;
		endLevelTime = timeSinceLevelStart;
	}

	public void WonLevel()
	{
		GameStats.Increment(GameStats.WON);

		isPlayingLevel = false;
		endLevelTime = timeSinceLevelStart;

		string levelName = LevelData.active.ResourceName;

		if (LevelData.active == null)
		{
			Log.Error($"No active level data for scene {Game.ActiveScene}");
			return;
		}

		bool hasBeatLevel = LevelData.active.HasCompletedLevel();
		float previousBestCache = LevelData.active.GetBestTime();

		if (BotModePreferences.instance.IsInAnyBotMode())
		{
			if (BotModePreferences.instance.IsInBotMode(PlayerBotMode.FastestTime))
			{
				LevelData.active.fastestTime = endLevelTime;
			}
			else if (BotModePreferences.instance.IsInBotMode(PlayerBotMode.SlowestTime))
			{
				LevelData.active.slowestTime = endLevelTime;
			}

			isNewPersonalBest = hasBeatLevel ? previousBestTime <= levelTime : true;
			previousBestTime = hasBeatLevel ? previousBestCache : null;
			return;
		}

		isNewPersonalBest = LevelData.active.SetBestTime(levelTime);

		previousBestTime = previousBestCache;

		if (hasBeatLevel)
		{
			Log.Info($"WonLevel() levelTime: {levelTime}, previousBestCache: {previousBestCache}, isNewPersonalBest: {isNewPersonalBest}");
		}
		else
		{
			Log.Info($"WonLevel() for first time! levelTime: {levelTime}, isNewPersonalBest: {isNewPersonalBest}");
		}

		if (isNewPersonalBest)
		{
			GameLeaderboards.SetLeaderboardLevelTime(LevelData.active, levelTime);

			var bestMedalType = GameSettings.instance.GetLowestMedalType();
			GameLeaderboards.SetLeaderboard(GameStats.LOWEST_MEDAL, (int)bestMedalType);
		}
	}

	[Button("Test")]
	public void Test()
	{
		Log.Info($"Game.IsPlaying: {Game.IsPlaying}");
		//Log.Info($"MusicManagerSystem::OnLevelLoaded() Game.IsPlaying: {Game.IsPlaying}, MusicManager.instance == null {MusicManager.instance == null}, Game.ActiveScene == null {Game.ActiveScene == null}");
		//Log.Info($"GameSave.instance.temp was '{GameSave.instance.temp}'");

		//GameSave.instance.temp += 1.0f;
		//GameSave.instance.Save();

		//FileSystem.Data.WriteJson("GameSaveAlt.json", GameSave.instance);
	}
}