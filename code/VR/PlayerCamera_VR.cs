
using Sandbox.Citizen;

public class PlayerCamera_VR : Component
{
	public static PlayerCamera_VR instance;

	[Group("Setup"), Property] public CameraComponent camera { get; set; }

	protected override void OnAwake()
	{
		instance = this;

		base.OnAwake();
	}
}
