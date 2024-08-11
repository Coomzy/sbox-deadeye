
using Sandbox.Citizen;

public class PlayerCamera_TD : Component
{
	public static PlayerCamera_TD instance;

	[Group("Setup"), Property] public CameraComponent camera { get; set; }

	[Group("Config"), Property] public float topDownOffset { get; set; } = 700.0f;

	protected override void OnAwake()
	{
		instance = this;

		base.OnAwake();
	}

	protected override void OnUpdate()
	{
		Vector3 cameraPos = Player_TD.instance.Transform.Position;
		cameraPos.z += topDownOffset;
		GameObject.Transform.Position = cameraPos;
	}
}
