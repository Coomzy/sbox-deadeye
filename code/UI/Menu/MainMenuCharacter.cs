
using Sandbox;
using Sandbox.Citizen;
using Sandbox.Services;
using System;
using System.ComponentModel.Design;
using System.Text.Json;
using static Sandbox.ClothingContainer;
using static Sandbox.Gizmo;

public class MainMenuCharacter : Component
{
	public static MainMenuCharacter instance { get; private set; }

	[Group("Setup"), Property] public SkinnedModelRenderer bodyRenderer { get; private set; }
	[Group("Setup"), Property] public CitizenAnimationHelper thirdPersonAnimationHelper { get; private set; }
	[Group("Setup"), Property] public Weapon weapon { get; private set; }

	protected override void OnAwake()
	{
		instance = this;

		base.OnAwake();

		LoadClothing();

		thirdPersonAnimationHelper.MoveStyle = CitizenAnimationHelper.MoveStyles.Walk;
		thirdPersonAnimationHelper.HoldType = CitizenAnimationHelper.HoldTypes.Pistol;
		thirdPersonAnimationHelper.Handedness = CitizenAnimationHelper.Hand.Both;
	}

	void LoadClothing()
	{
		var clothingContainer = CitizenSettings.instance.GetPlayerClothingContainer();
		clothingContainer.Apply(bodyRenderer);
	}

	public void Refresh()
	{
		LoadClothing();
	}
}
