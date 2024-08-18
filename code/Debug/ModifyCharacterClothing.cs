
using Sandbox;
using Sandbox.Citizen;
using Sandbox.Services;
using System;
using System.ComponentModel.Design;
using System.Text.Json;
using static Sandbox.ClothingContainer;
using static Sandbox.Gizmo;

public class ModifyCharacterClothing : Component
{
	[Group("Setup"), Property] public SkinnedModelRenderer bodyRenderer { get; private set; }
	[Group("Setup"), Property] public CitizenAnimationHelper thirdPersonAnimationHelper { get; private set; }
	[Group("Setup"), Property] public Weapon weapon { get; private set; }

	protected override void OnAwake()
	{
		base.OnAwake();

		LoadClothing();

		thirdPersonAnimationHelper.MoveStyle = CitizenAnimationHelper.MoveStyles.Walk;
		thirdPersonAnimationHelper.HoldType = CitizenAnimationHelper.HoldTypes.Pistol;
		thirdPersonAnimationHelper.Handedness = CitizenAnimationHelper.Hand.Both;
	}

	void LoadClothing()
	{
		var avatarJson = Connection.Local.GetUserData("avatar");
		var clothingContainer = new ClothingContainer();
		clothingContainer.Deserialize(avatarJson);

		var originalClothing = new List<ClothingEntry>(clothingContainer.Clothing);
		foreach (var clothingItem in originalClothing)
		{
			if (clothingItem.Clothing.Category == Clothing.ClothingCategory.Hair)
			{
				continue;
			}
			if (clothingItem.Clothing.Category == Clothing.ClothingCategory.Facial)
			{
				continue;
			}
			if (CitizenSettings.instance.whitelistCowboyClothing.Contains(clothingItem.Clothing))
			{
				continue;
			}

			clothingContainer.Toggle(clothingItem.Clothing);
		}

		foreach (var clothing in CitizenSettings.instance.cowboyClothing)
		{
			if (clothingContainer.Has(clothing))
			{
				continue;
			}

			clothingContainer.Toggle(clothing);
		}

		clothingContainer.Apply(bodyRenderer);
	}
}
