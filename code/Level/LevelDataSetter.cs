
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
}
