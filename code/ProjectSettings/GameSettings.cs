
using Sandbox.Citizen;
using Sandbox.Internal;
using System.IO;
using System;
using Sandbox;
using static Sandbox.Connection;

public enum MedalType
{
	None = 0,
	Bronze = 1,
	Silver = 2,
	Gold = 3,
	Onyx = 4
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
	[Group("Levels"), Property] public LevelData menuLevel { get; set; }
	[Group("Levels"), Property] public List<LevelData> topDownLevels { get; set; } = new List<LevelData>();

	[Group("Weapons"), Property] public PrefabFile pistolPrefab { get; set; } = ResourceLibrary.Get<PrefabFile>("prefabs/weapons/weapon - pistol.prefab");
	[Group("Weapons"), Property] public PrefabFile bloodDecalPrefab { get; set; } = ResourceLibrary.Get<PrefabFile>("prefabs/weapons/blood decal.prefab");

	[Group("Targets"), Property] public PrefabFile targetPrefab { get; set; } = ResourceLibrary.Get<PrefabFile>("prefabs/target/target_v2.prefab");

	[Group("Highlight"), Property] public Color badHighlightColour { get; set; } = Color.Red;
	[Group("Highlight"), Property] public Color goodHighlightColour { get; set; } = Color.White;

	[Group("Medals"), Property, ImageAssetPath] public string noneImage { get; set; } = "";
	[Group("Medals"), Property, ImageAssetPath] public string bronzeImage { get; set; } = "textures/medals/medal_bronze.psd";
	[Group("Medals"), Property, ImageAssetPath] public string silverImage { get; set; } = "textures/medals/medal_silver.psd";
	[Group("Medals"), Property, ImageAssetPath] public string goldImage { get; set; } = "textures/medals/medal_gold.psd";
	[Group("Medals"), Property, ImageAssetPath] public string onyxImage { get; set; } = "textures/medals/medal_platinum.psd";
	[Group("Medals"), Property, ImageAssetPath] public string lockedImage { get; set; } = "textures/medals/medal_locked.psd";

	[Group("Medals"), Property] public Texture bronzeImageTex { get; set; } // = Texture.Load(FileSystem.Mounted, "textures/medals/medal_bronze.psd");
	[Group("Medals"), Property] public Texture silverImageTex { get; set; } //= Texture.Load(FileSystem.Mounted, "textures/medals/medal_silver.psd");
	[Group("Medals"), Property] public Texture goldImageTex { get; set; } //= Texture.Load(FileSystem.Mounted, "textures/medals/medal_gold.psd");
	[Group("Medals"), Property] public Texture onyxImageTex { get; set; } //= Texture.Load(FileSystem.Mounted, "textures/medals/medal_platinum.psd");

	[Group("Time"), Property] public float civilianKilledTimePenalty { get; set; } = 5.0f;
	[Group("Time"), Property] public float simulatedReactTime { get; set; } = 0.25f;

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

	public string GetMedalImage(MedalType medalType)
	{
		switch (medalType)
		{
			case MedalType.Bronze:
				return bronzeImage;
			case MedalType.Silver:
				return silverImage;
			case MedalType.Gold:
				return goldImage;
			case MedalType.Onyx:
				return onyxImage;
		}

		return noneImage;
	}

	public MedalType GetLowestMedalType()
	{
		var lowestMedalType = MedalType.Onyx;
		foreach (var level in topDownLevels)
		{
			if (level.HasCompletedLevel())
			{
				lowestMedalType = MedalType.None;
				break;
			}

			var levelMedalType = level.GetBestMedal();
			if (levelMedalType >= lowestMedalType)
				continue;

			lowestMedalType = levelMedalType;
		}
		return lowestMedalType;
	}
}
