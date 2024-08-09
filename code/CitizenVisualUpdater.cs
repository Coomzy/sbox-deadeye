
using Sandbox;
using Sandbox.Citizen;
using System;
using System.Text.Json;
using static Sandbox.Gizmo;

public class CitizenVisualUpdater : Component
{
	[Group("Setup"), Property] public CitizenVisuals citizenVisuals { get; set; }

	[Group("Config"), Property] public bool isBadTarget { get; set; } = true;

	protected override void OnStart()
	{
		base.OnStart();

		UpdateAnimation();
	}

	void UpdateAnimation()
	{
		citizenVisuals.Apply(true);
		//LookAtPlayer();
	}

	void LookAtPlayer()
	{
		// Make this look at the player later
		var camera = Scene.GetAllComponents<CameraComponent>().Where(x => x.IsMainCamera).FirstOrDefault();
		var dirToCamera = Vector3.Direction(Transform.Position, camera.Transform.Position);
		dirToCamera.z = 0.0f;
		dirToCamera = dirToCamera.Normal;
		Transform.Rotation = Rotation.From(dirToCamera.EulerAngles);
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();

		UpdateAnimation();
	}

	[Button("Die")]
	public void Die()
	{
		citizenVisuals.Die();
	}
}
