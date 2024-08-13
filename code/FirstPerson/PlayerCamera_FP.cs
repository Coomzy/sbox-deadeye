
using Sandbox.Citizen;

public class PlayerCamera_FP : Component
{
	public static PlayerCamera_FP instance;

	[Group("Setup"), Property] public CameraComponent camera { get; set; }

	[Group("Config"), Property] public float topDownOffset { get; set; } = 88.0f;

	protected override void OnAwake()
	{
		instance = this;

		base.OnAwake();
	}

	protected override void OnUpdate()
	{
		Vector3 cameraPos = Player_FP.instance.Transform.Position;
		cameraPos.z += topDownOffset;
		GameObject.Transform.Position = cameraPos;

		GameObject.Transform.Rotation = Player_FP.instance.Transform.Rotation;
	}
}
