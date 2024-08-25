
using Sandbox.Citizen;

public class LevelDataSetter : Component
{
	[Group("Setup"), Property] public LevelData levelData { get; set; }

	protected override void OnAwake()
	{
		base.OnAwake();

		LevelData.active = levelData;
	}
}
