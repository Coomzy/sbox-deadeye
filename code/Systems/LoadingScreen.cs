using System;
using System.Reflection;

public class LoadingScreen : Component
{
	public static GameResource nextLevel = null;

	protected override void OnStart()
	{
		base.OnStart();

		if (nextLevel == null)
		{
			nextLevel = null;
			Game.ActiveScene.LoadFromFile("scenes/menu.scene");
			return;
		}

		var nextLevelToLoad = nextLevel;
		nextLevel = null;
		Game.ActiveScene.Load(nextLevelToLoad);
	}

	public static void SwitchLevel(GameResource nextLevelToLoad)
	{
		nextLevel = nextLevelToLoad;
		Game.ActiveScene.LoadFromFile("scenes/loadingscreen.scene");
	}

	public static void ReloadLevel()
	{
		nextLevel = Game.ActiveScene.Source;
		Game.ActiveScene.LoadFromFile("scenes/loadingscreen.scene");
	}

	public static void SwitchToMenu()
	{
		nextLevel = null;
		Game.ActiveScene.LoadFromFile("scenes/loadingscreen.scene");
	}
}