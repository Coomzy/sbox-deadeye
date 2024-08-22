
using Sandbox.Citizen;

public class PlayerCamera_TD : Component
{
	public static PlayerCamera_TD instance;

	[Group("Setup"), Property] public CameraComponent camera { get; set; }

	[Group("Config"), Property] public float topDownOffset { get; set; } = 700.0f;

	[Group("Config"), Property] public float targetHeight { get; set; } = 433.6f; // The height you want the camera to cover in world units.

	protected override void OnAwake()
	{
		instance = this;

		base.OnAwake();

		GameObject.Transform.Rotation = new Angles(90.0f, 0.0f, 0.0f);
	}

	protected override void OnUpdate()
	{
		Vector3 cameraPos = Player_TD.instance.Transform.Position;
		cameraPos.z += topDownOffset;
		//GameObject.Transform.Position = cameraPos;
		
		var currentPosition = GameObject.Transform.Position;
		var newPosition = currentPosition.LerpTo(cameraPos, RealTime.Delta * PlayerSettings.instance.cameraLerpSpeed);
		GameObject.Transform.Position = newPosition;

		CalculateAndSetFOV();
	}

	void CalculateAndSetFOV()
	{
		// Calculate half of the target height
		float halfHeight = targetHeight / 2.0f;

		// Calculate the angle in radians for the vertical FOV
		float verticalTheta = (float)System.Math.Atan(halfHeight / topDownOffset);

		// Convert the angle to degrees and double it to get the full vertical FOV
		float verticalFOV = verticalTheta.RadianToDegree() * 2.0f;

		// Calculate the horizontal FOV based on the vertical FOV and the aspect ratio
		float horizontalFOV = (float)(2.0f * System.Math.Atan(System.Math.Tan(verticalTheta) * Screen.Aspect) * 57.29578f);

		// Set the camera's field of view to the calculated horizontal FOV
		camera.FieldOfView = horizontalFOV;
	}
}
