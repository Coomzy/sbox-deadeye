
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
	[Group("Runtime"), Property] public float decidingTime { get; set; }
	[Group("Runtime"), Property] public int civiliansKilled { get; set; }
	[Group("Runtime"), Property] public bool isNewPersonalBest { get; set; }
	[Group("Runtime"), Property] public float? previousBestTime { get; set; }
	[Group("Runtime"), Property] public TimeUntil countDownFinish { get; set; }
	[Group("Runtime"), Property] public bool hasFinishedCountDown => countDownFinish;

	public static float marathonModeDecidingTime { get; set; }
	public static bool isInMarathonMode { get; set; } = false;
	
	public MedalType currentMedal => LevelData.active != null ? LevelData.active.TimeToMedalType(levelTime) : MedalType.None;

	public float levelTime
	{ 
		get
		{
			float time = decidingTime;
			// Might consider rapidly adding the time rather than instant
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

		countDownFinish = 3f;
		Restart_Internal();
	}

	protected override void OnStart()
	{
		base.OnStart();

		if (isInMarathonMode)
		{
			var firstLevel = GameSettings.instance.topDownLevels[0];
			bool isFirstLevel = firstLevel == LevelData.active;
			if (isFirstLevel)
			{
				marathonModeDecidingTime = 0.0f;
			}
		}

		if (GlobalHighlight.instance == null)
		{
			if (Game.IsEditor)
			{
				Log.Warning("Could not find a GlobalHighlight!!");
			}
		}

		GameStats.Increment(GameStats.LEVEL_LOADS);
		GameStats.Increment(GameStats.ATTEMPTS);

		if (GameSettings.instance != null && GameSettings.instance.topDownLevels != null && GameSave.instance != null)
		{
			for (int i = 0; i < GameSettings.instance.topDownLevels.Count; i++)
			{
				var level = GameSettings.instance.topDownLevels[i];
				if (level == null)
					continue;

				if (level != LevelData.active)
					continue;

				int levelIndex = i + 1;

				if (GameSave.instance.furthestLevelPlayed >= levelIndex)
				{
					continue;
				}

				int diff = levelIndex - GameSave.instance.furthestLevelPlayed;
				GameSave.instance.furthestLevelPlayed = levelIndex;
				GameSave.instance.Save();
				GameStats.Increment(GameStats.LEVEL_FURTHEST_PLAYED, diff);
				break;
			}
		}
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();

		CheckForExitLevelInput();

		if (countDownFinish)
		{
			CheckForReloadLevelInput();
		}

		//Log.Warning($"Execute Commands Stage: {executeCommandsStage}, loop: {loop}");
	}

	void CheckForExitLevelInput()
	{
		if (Input.EscapePressed)
		{
			Input.EscapePressed = false;
			ExitLevel();
			return;
		}

		if (!Input.Pressed("Quit"))
		{
			return;
		}

		if (UIManager.instance?.leaderboardsScreen != null && UIManager.instance.leaderboardsScreen.Enabled)
			return;

		ExitLevel();
	}

	void CheckForReloadLevelInput()
	{
		bool pressed = Input.Pressed("Restart");
		if (Input.Pressed("Restart_Alt"))
		{
			pressed = true;
		}
		if (!pressed)
			return;

		if (UIManager.instance?.leaderboardsScreen != null && UIManager.instance.leaderboardsScreen.Enabled)
			return;

		Restart();
		//LoadingScreen.ReloadLevel();
	}

	void Restart_Internal()
	{
		isPlayingLevel = true;
		decidingTime = 0.0f;
		civiliansKilled = 0;
		isNewPersonalBest = false;
		previousBestTime = null;

		var firstLevel = GameSettings.instance.topDownLevels[0];
		bool isFirstLevel = firstLevel == LevelData.active;
		if (isFirstLevel)
		{
			marathonModeDecidingTime = 0.0f;
		}
	}

	public void Restart()
	{
		GameStats.Increment(GameStats.ATTEMPTS);
		GameStats.Increment(GameStats.RESTARTS);

		if (RestartMarathonMode())
		{
			return;
		}
		Restart_Internal();

		var restartables = Scene.GetAllComponents<IRestartable>();
		foreach (var restartable in restartables)
		{
			if (restartable == null)
				continue;

			restartable.PreRestart();
		}
		foreach (var restartable in restartables)
		{
			if (restartable == null)
				continue;

			restartable.PostRestart();
		}
	}

	public bool RestartMarathonMode()
	{
		if (!isInMarathonMode)
		{
			return false;
		}

		var firstLevel = GameSettings.instance.topDownLevels[0];
		bool isFirstLevel = firstLevel == LevelData.active;
		if (isFirstLevel)
			return false;

		marathonModeDecidingTime = 0.0f;
		var shutdowns = Scene.GetAllComponents<IShutdown>();
		foreach (var shutdown in shutdowns)
		{
			if (shutdown == null)
				continue;

			shutdown.PreShutdown();
		}
		foreach (var shutdown in shutdowns)
		{
			if (shutdown == null)
				continue;

			shutdown.PostShutdown();
		}
		LoadingScreen.SwitchLevel(firstLevel.scene);

		return true;
	}

	public void ExitLevel()
	{
		GameStats.Increment(GameStats.LEVEL_EXITS);

		marathonModeDecidingTime = 0.0f;
		var shutdowns = Scene.GetAllComponents<IShutdown>();
		foreach (var shutdown in shutdowns)
		{
			if (shutdown == null)
				continue;

			shutdown.PreShutdown();
		}
		foreach (var shutdown in shutdowns)
		{
			if (shutdown == null)
				continue;

			shutdown.PostShutdown();
		}

		LoadingScreen.SwitchToMenu();
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
	}

	public void WonLevel()
	{
		GameStats.Increment(GameStats.WON);

		isPlayingLevel = false;

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
				LevelData.active.fastestTime = levelTime;
			}
			else if (BotModePreferences.instance.IsInBotMode(PlayerBotMode.SlowestTime))
			{
				LevelData.active.slowestTime = levelTime;
			}

			previousBestTime = hasBeatLevel ? previousBestCache : null;
			isNewPersonalBest = hasBeatLevel ? previousBestTime <= levelTime : true;
			return;
		}

		isNewPersonalBest = LevelData.active.SetBestTime(levelTime);

		previousBestTime = hasBeatLevel ? previousBestCache : null;

		if (Game.IsEditor)
		{
			if (hasBeatLevel)
			{
				Log.Info($"WonLevel() levelTime: {levelTime}, previousBestCache: {previousBestCache}, isNewPersonalBest: {isNewPersonalBest}");
			}
			else
			{
				Log.Info($"WonLevel() for first time! levelTime: {levelTime}, isNewPersonalBest: {isNewPersonalBest}");
			}
		}

		if (isNewPersonalBest)
		{
			GameLeaderboards.SetLeaderboardLevelTime(LevelData.active, levelTime);

			//var bestMedalType = GameSettings.instance.GetLowestMedalType();
			//GameLeaderboards.SetLeaderboard(GameStats.LOWEST_MEDAL, (int)bestMedalType);
		}

		GameStats.CheckMedalAchievements();

		float combinedTime = 0.0f;
		bool hasBeatAllLevels = true;
		foreach (var level in GameSettings.instance.topDownLevels)
		{
			if (!level.HasCompletedLevel())
			{
				hasBeatAllLevels = false;
				break;
			}
			combinedTime += level.GetBestTime();
		}

		if (hasBeatAllLevels)
		{
			GameStats.Set(GameStats.COMBINED_TIME, combinedTime);
		}

		if (GameSettings.instance != null && GameSettings.instance.topDownLevels != null && GameSave.instance != null)
		{
			for (int i = 0; i < GameSettings.instance.topDownLevels.Count; i++)
			{
				var level = GameSettings.instance.topDownLevels[i];
				if (level == null)
					continue;

				if (level != LevelData.active)
					continue;

				int levelIndex = i + 1;

				if (GameSave.instance.furthestLevelComplete >= levelIndex)
				{
					continue;
				}

				int diff = levelIndex - GameSave.instance.furthestLevelComplete;
				GameSave.instance.furthestLevelComplete = levelIndex;
				GameSave.instance.Save();
				GameStats.Increment(GameStats.LEVEL_FURTHEST_BEAT, diff);
				break;
			}
		}

		if (isInMarathonMode)
		{
			var lastLevel = GameSettings.instance.topDownLevels[GameSettings.instance.topDownLevels.Count-1];
			bool isLastLevel = lastLevel == LevelData.active;
			if (isLastLevel)
			{
				GameLeaderboards.SetLeaderboard(GameStats.MARATHON_MODE, marathonModeDecidingTime);
			}
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