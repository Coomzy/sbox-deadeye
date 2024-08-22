using Sandbox.Citizen;
using Sandbox.Internal;
using System.IO;
using System;
using Sandbox;

[GameResource("Player Settings", "ps", "Player Settings")]
public class PlayerSettings : GameResourceSingleton<PlayerSettings>
{
	[Group("Movement"), Property] public float walkSpeed { get; set; } = 200.0f;
	[Group("Movement"), Property] public float faceMovementSpeed { get; set; } = 5.0f;

	[Group("Executing"), Property] public float delayBeforeExecute { get; set; } = 0.2375f;
	[Group("Executing"), Property] public float delayPerExecute { get; set; } = 0.2375f;
	[Group("Executing"), Property] public float delayAfterExecute { get; set; } = 0.2375f;

	[Group("Executing"), Property] public float shootForceRandomAngle { get; set; } = 25.0f;
	[Group("Executing"), Property] public Vector2 shootForceRange { get; set; } = new Vector2(350.0f, 1000.0f);

}
