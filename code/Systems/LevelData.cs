using Sandbox;
using Sandbox.Citizen;
using System;
using System.Runtime.CompilerServices;
using System.Text.Json;
using static Sandbox.Gizmo;

[GameResource("Level Data", "ld", "Level Data")]
public class LevelData : GameResource
{
	static Dictionary<string, LevelData> sceneNameToLevelData { get; set; } = new Dictionary<string, LevelData>();

	[Category("Times"), Property] public float silverTime = 60.0f;
	[Category("Times"), Property] public float goldTime = 55.0f;
	[Category("Times"), Property] public float onyxTime = 50.0f;

	[Category("Times"), Property] public float slowestTime = 180.0f;
	[Category("Times"), Property] public float fastestTime = 60.0f;

	protected override void PostLoad()
	{
		base.PostLoad();

		sceneNameToLevelData[this.ResourceName] = this;

		Log.Info($"PostLoad()");
	}

	protected override void PostReload()
	{
		base.PostReload();

		sceneNameToLevelData[this.ResourceName] = this;

		Log.Info($"PostReload()");
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

public static class LevelDataExtension
{
	//[]
	public static void ABC()
	{
		
	}

	public static LevelData GetSceneLevelData(this Scene scene)
	{
		return LevelData.GetSceneLevelData(scene);
	}
}
