
using Sandbox;
using Sandbox.Citizen;
using System;
using System.Text.Json;
using System.Transactions;
using static Sandbox.Gizmo;

public class Target : Component
{
	[Group("Setup"), Property] public SkinnedModelRenderer bodyRenderer { get; set; }
	[Group("Setup"), Property] public ModelPhysics bodyPhysics { get; set; }
	[Group("Setup"), Property] public CitizenAnimationHelper animationHelper { get; set; }
	[Group("Setup"), Property] public CitizenVisuals citizenVisuals { get; set; }
	[Group("Setup"), Property] public HighlightOutline highlightOutline { get; set; }

	[Group("Config"), Property] public bool isBadTarget { get; set; } = true;

	[Group("Runtime"), Property] public bool isDead { get; set; } = false;

	protected override void OnStart()
	{
		base.OnStart();

		UpdateAnimation();

		highlightOutline.Color = isBadTarget ? GameSettings.instance.badHighlightColour : GameSettings.instance.goodHighlightColour;
		highlightOutline.ObscuredColor = highlightOutline.Color;
		highlightOutline.Enabled = false;
	}

	public void Activate()
	{
		LookAtPlayer();
	}

	public void Select()
	{
		highlightOutline.Enabled = true;
	}

	public void Deselect()
	{
		highlightOutline.Enabled = false;
	}

	void UpdateAnimation()
	{
		citizenVisuals.Apply();

		//animationHelper.AimEyesWeight = 1.0f;
		//animationHelper.AimHeadWeight = 1.0f;
		//animationHelper.AimBodyWeight = 1.0f;

		var camera = Scene.GetAllComponents<CameraComponent>().Where(x => x.IsMainCamera).FirstOrDefault();
		var dirToCamera = Vector3.Direction(animationHelper.EyeSource.Transform.Position, camera.Transform.Position);
		//Gizmo.Draw.LineSphere(animationHelper.EyeSource.Transform.Position, 10.0f);
		//Gizmo.Draw.LineSphere(camera.Transform.Position, 10.0f);
		//Gizmo.Draw.Line(animationHelper.EyeSource.Transform.Position, Camera.Position);
		//dirToCamera.z = 0.0f;
		//dirToCamera = dirToCamera.Normal;
		//animationHelper.AimAngle = Rotation.From(dirToCamera.EulerAngles);
		var aimAngles = new Angles(150.0f, 0.0f, 0.0f);
		//animationHelper.AimAngle = Rotation.From(dirToCamera.EulerAngles);
		animationHelper.AimAngle = aimAngles.ToRotation();
		//animationHelper.
		//animationHelper.LookAt = camera.GameObject;
		//animationHelper.LookAt = null;
		//animationHelper.LookAtEnabled = true;
		LookAtPlayer();
	}

	void LookAtPlayer()
	{
		var dirToCamera = Vector3.Direction(Transform.Position, Player.instance.Transform.Position);
		dirToCamera.z = 0.0f;
		dirToCamera = dirToCamera.Normal;
		Transform.Rotation = Rotation.From(dirToCamera.EulerAngles);
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();

		//UpdateAnimation();
	}

	[Button("Die")]
	public void Die()
	{
		isDead = true;
		citizenVisuals.Die();
	}
}
