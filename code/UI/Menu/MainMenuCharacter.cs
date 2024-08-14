
using Sandbox;
using Sandbox.Citizen;
using Sandbox.Services;
using System;
using System.ComponentModel.Design;
using System.Text.Json;
using static Sandbox.Gizmo;

public class MainMenuCharacter : Component
{
	[Group("Setup"), Property] public SkinnedModelRenderer bodyRenderer { get; private set; }
	[Group("Setup"), Property] public CitizenAnimationHelper thirdPersonAnimationHelper { get; private set; }
	[Group("Setup"), Property] public Weapon weapon { get; private set; }

	protected override void OnAwake()
	{
		base.OnAwake();

		Mouse.Visible = true;

		LoadClothing();

		thirdPersonAnimationHelper.MoveStyle = CitizenAnimationHelper.MoveStyles.Walk;
		thirdPersonAnimationHelper.HoldType = CitizenAnimationHelper.HoldTypes.Pistol;
		thirdPersonAnimationHelper.Handedness = CitizenAnimationHelper.Hand.Both;
	}

	void LoadClothing()
	{
		var avatarJson = Connection.Local.GetUserData("avatar");
		var clothing = new ClothingContainer();
		clothing.Deserialize(avatarJson);
		clothing.Apply(bodyRenderer);
	}
}
