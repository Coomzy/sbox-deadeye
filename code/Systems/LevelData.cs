using Sandbox;
using Sandbox.Citizen;
using Sandbox.Internal;
using System;
using System.Runtime.CompilerServices;
using System.Text.Json;
using static Sandbox.Gizmo;

[GameResource("Level Data", "ld", "Level Data")]
public class LevelData : GameResource
{
	static Dictionary<string, LevelData> sceneNameToLevelData { get; set; } = new Dictionary<string, LevelData>(StringComparer.OrdinalIgnoreCase);
	public static LevelData active { get; private set; }

	[Category("Civilians"), Property] public int allowedCivilianCasualties = 3;

	[Category("Times"), Property] public float silverTime = 60.0f;
	[Category("Times"), Property] public float goldTime = 55.0f;
	[Category("Times"), Property] public float onyxTime = 50.0f;

	[Category("Times"), Property] public float slowestTime = 180.0f;
	[Category("Times"), Property] public float fastestTime = 60.0f;
	[Category("Times"), Property] public float simulatedTime = 80.0f;

	[Category("Leaderboard"), Property] public bool isLeaderboardLevel { get; set; } = true;
	[Category("Leaderboard"), Property] 
	public string leaderboardName
	{
		get
		{
			if (isLeaderboardLevel)
				return null;

			return $"level-{this.ResourceName.ToLower()}";
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
		sceneNameToLevelData[this.ResourceName] = this;
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

	public static LevelData GetSceneLevelData(Scene scene)
	{
		if (Utils.isEditTime)
		{
			if (scene?.Source?.ResourcePath == null)
			{
				Log.Error($"scene?.Source?.ResourcePath was null!");
				return null;
			}
			var path = scene.Source.ResourcePath.Replace(".scene", ".ld");

			if (ResourceLibrary.TryGet(path, out LevelData data))
			{
				return data;
			}
			return null;
		}

		if (sceneNameToLevelData.TryGetValue(scene.Name, out var levelData))
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