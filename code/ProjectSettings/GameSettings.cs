
using Sandbox.Citizen;
using Sandbox.Internal;
using System.IO;
using System;
using Sandbox;

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
	[Group("Weapons"), Property] public PrefabFile pistolPrefab { get; set; }

	[Group("Highlight"), Property] public Color badHighlightColour { get; set; } = Color.Red;
	[Group("Highlight"), Property] public Color goodHighlightColour { get; set; } = Color.White;

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
}
