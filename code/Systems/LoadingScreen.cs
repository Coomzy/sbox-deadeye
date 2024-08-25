using System;
using System.Reflection;

public class LoadingScreen : Component
{
	public static bool isLoading { get; set; }
	public static GameResource nextLevel = null;

	protected override void OnStart()
	{
		base.OnStart();

		if (nextLevel == null)
		{
			nextLevel = null;
			isLoading = false;
			Game.ActiveScene.LoadFromFile("scenes/menu.scene");
			return;
		}

		var nextLevelToLoad = nextLevel;
		nextLevel = null;
		isLoading = false;
		Game.ActiveScene.Load(nextLevelToLoad);
	}

	public static void SwitchLevel(GameResource nextLevelToLoad)
	{
		isLoading = true;
		nextLevel = nextLevelToLoad;
		Game.ActiveScene.LoadFromFile("scenes/loadingscreen.scene");
	}

	public static void ReloadLevel()
	{
		isLoading = true;
		nextLevel = Game.ActiveScene.Source;
		Game.ActiveScene.LoadFromFile("scenes/loadingscreen.scene");
	}

	public static void SwitchToMenu()
	{
		isLoading = true;
		nextLevel = null;
		Game.ActiveScene.LoadFromFile("scenes/loadingscreen.scene");
	}
}