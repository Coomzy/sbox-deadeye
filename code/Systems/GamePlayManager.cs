
using Sandbox;
using Sandbox.Citizen;
using System;
using System.Diagnostics;
using System.Text.Json;
using static Sandbox.Gizmo;

public enum PlayerBotMode
{
	None,
	SlowestTime,
	FastestTime
};

public class GamePlayManager : Component
{
	public static GamePlayManager instance;

	[Group("Config"), Property] public PlayerBotMode botMode { get; private set; }

	[Category("Runtime"), Property] public bool isPlayingLevel { get; private set; } = true;
	[Category("Runtime"), Property] public float endLevelTime { get; private set; }
	[Category("Runtime"), Property] public LevelData currentLevelData { get; private set; }

	TimeSince timeSinceLevelStart { get; set; }

	public MedalType currentMedal => currentLevelData != null ? currentLevelData.TimeToMedalType(levelTime) : MedalType.None;

	public float levelTime
	{ 
		get
		{
			if (isPlayingLevel)
			{
				return timeSinceLevelStart;
			}
			return endLevelTime;
		}
	}

	protected override void OnAwake()
	{
		instance = this;

		base.OnAwake();

		timeSinceLevelStart = 0.0f;		
	}

	protected override void OnStart()
	{
		base.OnStart();
		
		// TODO: Make this work
		//currentLevelData = GameObject.Scene.GetSceneLevelData();
	}

	public void EndLevel()
	{
		endLevelTime = timeSinceLevelStart;
		isPlayingLevel = false;


		if (botMode == PlayerBotMode.FastestTime)
		{
			currentLevelData.fastestTime = endLevelTime;
		}
		else if (botMode == PlayerBotMode.SlowestTime)
		{
			currentLevelData.slowestTime = endLevelTime;
		}
	}

	[Button("Test")]
	public void Test()
	{		
		var path = GameObject.Scene.Source.ResourcePath.Replace(".scene", ".ld");

		Log.Info($"levelData: {GameObject.Scene.GetSceneLevelData()}");
	}
}
