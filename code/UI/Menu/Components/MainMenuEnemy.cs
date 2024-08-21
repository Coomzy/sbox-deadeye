
using Sandbox;
using Sandbox.Citizen;
using Sandbox.Services;
using System;
using System.ComponentModel.Design;
using System.Text.Json;
using static Sandbox.Gizmo;

public class MainMenuEnemy : Component
{
	[Group("Setup"), Property] public SkinnedModelRenderer bodyRenderer { get; private set; }
	[Group("Setup"), Property] public CitizenAnimationHelper thirdPersonAnimationHelper { get; private set; }
	[Group("Setup"), Property] public CitizenVisuals citizenVisuals { get; private set; }
	[Group("Setup"), Property] public Weapon weapon { get; private set; }
	[Group("Setup"), Property] public bool isBadGuy { get; private set; }

	protected override void OnAwake()
	{
		base.OnAwake();

		thirdPersonAnimationHelper.MoveStyle = CitizenAnimationHelper.MoveStyles.Walk;
		thirdPersonAnimationHelper.HoldType = isBadGuy ? CitizenAnimationHelper.HoldTypes.Pistol : CitizenAnimationHelper.HoldTypes.None;
		thirdPersonAnimationHelper.Handedness = isBadGuy ? CitizenAnimationHelper.Hand.Right : CitizenAnimationHelper.Hand.Right;
	}
}
