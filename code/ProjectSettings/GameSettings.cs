
using Sandbox.Citizen;
using Sandbox.Internal;
using System.IO;
using System;
using Sandbox;
using static Sandbox.Connection;

public enum MedalType
{
	None,
	Bronze,
	Silver,
	Gold,
	Onyx
}

public enum WeaponType
{
	None,
	Pistol,
	Shotgun,
	Rifle
}

[GameResource("Game Settings", "gs", "Game Settings")]
public class GameSettings : GameResourceSingleton<GameSettings>
{
	[Group("Weapons"), Property] public PrefabFile pistolPrefab { get; set; } = ResourceLibrary.Get<PrefabFile>("prefabs/weapons/weapon - pistol.prefab");

	[Group("Highlight"), Property] public Color badHighlightColour { get; set; } = Color.Red;
	[Group("Highlight"), Property] public Color goodHighlightColour { get; set; } = Color.White;

	[Group("Medals"), Property] public Color noneColour { get; set; } = Color.Transparent;
	[Group("Medals"), Property] public Color bronzeColour { get; set; } = new Color(0xFF3C5E8C);
	[Group("Medals"), Property] public Color silverColour { get; set; } = new Color(0xFFC0C0C0);
	[Group("Medals"), Property] public Color goldColour { get; set; } = new Color(0xFF00D7FF);
	[Group("Medals"), Property] public Color onyxColour { get; set; } = new Color(0xFF393835);

	public PrefabFile GetWeaponPrefab(WeaponType weaponType)
	{
		switch (weaponType)
		{
			case WeaponType.Pistol:
				return pistolPrefab;
		}

		return null;
	}

	public CitizenAnimationHelper.HoldTypes GetWeaponHoldType(WeaponType weaponType)
	{
		switch (weaponType)
		{
			case WeaponType.Pistol:
				return CitizenAnimationHelper.HoldTypes.Pistol;
			case WeaponType.Shotgun:
				return CitizenAnimationHelper.HoldTypes.Shotgun;
			case WeaponType.Rifle:
				return CitizenAnimationHelper.HoldTypes.Rifle;
		}

		return CitizenAnimationHelper.HoldTypes.None;
	}

	public Color GetMedalColour(MedalType medalType)
	{
		switch (medalType)
		{
			case MedalType.Bronze:
				return bronzeColour;
			case MedalType.Silver:
				return silverColour;
			case MedalType.Gold:
				return goldColour;
			case MedalType.Onyx:
				return onyxColour;
		}

		return noneColour;
	}
}
