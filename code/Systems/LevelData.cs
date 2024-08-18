using Sandbox;
using Sandbox.Citizen;
using Sandbox.Internal;
using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.Json;
using static Sandbox.Gizmo;

public enum GameMode
{
	TopDown,
	//FirstPerson,
	VR
}

[GameResource("Level Data", "ld", "Level Data")]
public class LevelData : GameResource
{
	static Dictionary<string, LevelData> sceneNameToLevelData { get; set; } = new Dictionary<string, LevelData>(StringComparer.OrdinalIgnoreCase);
	public static LevelData active { get; private set; }

	[Category("Setup"), Property] public SceneFile scene { get; set; }
	[Category("Setup"), Property] public GameMode gameMode { get; set; } = GameMode.TopDown;

	[Category("Civilians"), Property] public int allowedCivilianCasualties = 3;

	[Category("Times"), Property] public float silverTime = 60.0f;
	[Category("Times"), Property] public float goldTime = 55.0f;
	[Category("Times"), Property] public float onyxTime = 50.0f;

	[Category("Times"), Property] public float slowestTime = 180.0f;
	[Category("Times"), Property] public float fastestTime = 60.0f;
	[Category("Times"), Property] public float simulatedTime = 80.0f;

	[Category("Metadata"), Property]
	public string friendlyLevelName { get; set; }

	[Category("Metadata"), Property, TextArea]
	public string friendlyLevelDescription { get; set; }

	[Category("Metadata"), Property]
	public string artworkImageData { get; set; }

	[Category("Leaderboard"), Property] public bool isLeaderboardLevel { get; set; } = true;
	[Category("Leaderboard"), Property] public string statNameOverride { get; set; }

	[Category("Leaderboard"), Property, ReadOnly]
	public string statName
	{
		get
		{
			if (!isLeaderboardLevel)
				return null;

			if (statNameOverride != null)
			{
				return statNameOverride;
			}

			return $"{this.ResourceName}".ToLower();
		}
	}

	[Category("Leaderboard"), Property, ReadOnly]
	public string leaderboardName
	{
		get
		{
			if (!isLeaderboardLevel)
				return null;

			return $"{statName}-lb".ToLower();
		}
	}

	public static void ClearRegister()
	{
		sceneNameToLevelData = new Dictionary<string, LevelData>(StringComparer.OrdinalIgnoreCase);
	}

	public static void SetActiveLevelData()
	{
		active = Game.ActiveScene.GetSceneLevelData();
	}

	public void Register()
	{
		if (scene == null)
		{
			Log.Info($"Register() scene was null for LevelData '{ResourceName}'");
			return;
		}

		sceneNameToLevelData[scene.Title] = this;
	}

	public MedalType TimeToMedalType(float time)
	{
		if (time <= onyxTime)
		{
			return MedalType.Onyx;
		}
		if (time <= goldTime)
		{
			return MedalType.Gold;
		}
		if (time <= silverTime)
		{
			return MedalType.Silver;
		}
		return MedalType.Bronze;
	}

	public float MedalTypeToTime(MedalType medalType)
	{
		switch (medalType)
		{
			case MedalType.Onyx:
				return onyxTime;
			case MedalType.Gold:
				return goldTime;
			case MedalType.Silver:
				return silverTime;
			case MedalType.Bronze:
				return 999.0f;
		}

		return 9999.0f;
	}

	public bool HasCompletedLevel()
	{
		if (GameSave.instance == null)
		{
			return true;
		}

		if (GameSave.instance.levelNameToBestTime.TryGetValue(ResourceName, out float savedBestTime))
		{
			return true;
		}

		return false;
	}

	public float GetBestTime()
	{
		if (GameSave.instance == null)
		{
			return float.MaxValue;
		}

		if (GameSave.instance.levelNameToBestTime.TryGetValue(ResourceName, out float savedBestTime))
		{
			return savedBestTime;
		}

		return float.MaxValue;
	}

	public bool SetBestTime(float newTime)
	{
		if (GameSave.instance.levelNameToBestTime.TryGetValue(ResourceName, out float savedBestTime))
		{
			if (newTime >= savedBestTime)
			{
				return false;
			}
		}

		GameSave.instance.levelNameToBestTime[ResourceName] = newTime;
		GameSave.instance.Save();
		return true;
	}

	public MedalType GetBestTimeMedal()
	{
		if (!HasCompletedLevel())
			return MedalType.None;

		return TimeToMedalType(GetBestTime());
	}

	public bool HasBestMedalType()
	{
		if (!HasCompletedLevel())
		{
			return false;
		}

		var bestMedal = GetBestTimeMedal();

		if (bestMedal == MedalType.Onyx)
			return true;

		return false;
	}

	public MedalType GetNextMedalType()
	{
		if (!HasCompletedLevel())
		{
			return MedalType.Bronze;
		}

		var bestMedal = GetBestTimeMedal();

		if (bestMedal == MedalType.Onyx)
			return MedalType.Onyx;

		bestMedal++;

		return bestMedal;
	}

	public float GetNextMedalTime()
	{
		if (HasBestMedalType())
			return 0.0f;

		var nextMedalType = GetNextMedalType();
		float nextTime = MedalTypeToTime(nextMedalType);

		return nextTime;
	}

	public bool HasGotMedal(MedalType medalType)
	{
		var bestMedalType = GetBestTimeMedal();

		return medalType <= bestMedalType;
	}

	public static LevelData GetSceneLevelData(Scene scene)
	{
		if (!Game.IsPlaying)
		{
			if (scene?.Source?.ResourcePath == null)
			{
				Log.Error($"scene?.Source?.ResourcePath was null!");
				return null;
			}
			var path = scene.Source.ResourcePath.Replace(".scene", ".ld");

			if (ResourceLibrary.TryGet(path, out LevelData data))
			{
				//Log.Info($"Loaded levelData at path: {path}");
				return data;
			}
			return null;
		}

		if (sceneNameToLevelData.TryGetValue(scene.Title, out var levelData))
		{
			return levelData;
		}

		return null;
	}
}

public class LevelDataSystem : GameObjectSystem
{
	public LevelDataSystem(Scene scene) : base(scene)
	{
		LevelData.ClearRegister();

		var allLevelDatas = ResourceLibrary.GetAll<LevelData>();

		foreach (var levelData in allLevelDatas)
		{
			levelData.Register();
		}
		
		Listen(Stage.SceneLoaded, -1, SetActiveSceneData, "SetActiveSceneData");
	}

	void SetActiveSceneData()
	{
		LevelData.SetActiveLevelData();
	}
}

public static class LevelDataExtension
{
	public static LevelData GetSceneLevelData(this Scene scene)
	{
		return LevelData.GetSceneLevelData(scene);
	}
}