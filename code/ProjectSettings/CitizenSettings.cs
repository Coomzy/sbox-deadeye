using Sandbox.Citizen;
using Sandbox.Internal;
using System.IO;
using System;
using Sandbox;
using static Sandbox.Clothing;
using static Sandbox.Gizmo;

public enum TargetType
{
	Any,
	Good,
	Bad
}

public enum TintMode
{
	None,
	Allow,
	Override
}

public class CitizenClothingInst
{
	[Group("Clothing"), Property] public Clothing clothing { get; set; }
	[Group("Tinting"), Property] public Color? tintSelection { get; set; }
}

public class CitizenClothing
{
	[Group("Clothing"), Property] public Clothing clothing { get; set; }
	[Group("Target"), Property] public TargetType targetType { get; set; }
	[Group("Randomizer"), Property, Range(0.0f, 1.0f)] public float chanceForGoodGuy { get; set; } = 1.0f;
	[Group("Randomizer"), Property, Range(0.0f, 1.0f)] public float chanceForBadGuy { get; set; } = 1.0f;
	[Group("Tinting"), Property] public TintMode tintMode { get; set; } = TintMode.Allow;
	[Group("Tinting"), Property] public List<Color> tintSelection { get; set; } = new List<Color>();
}

public class CitizenClothingCategory
{
	[Group("Randomizer"), Property, Range(0.0f, 1.0f)] public float chanceOf { get; set; } = 1.0f;
	[Group("Clothing"), Property, InlineEditor] public List<CitizenClothing> clothing { get; set; } = new List<CitizenClothing>();
	[Group("Tinting"), Property] public List<Color> tintSelection { get; set; } = new List<Color>();
}

[GameResource("Citizen Settings", "citizen", "Citizen Settings")]
public class CitizenSettings : GameResourceSingleton<CitizenSettings>
{
	[Group("Clothing - Hat"), Property, InlineEditor] public CitizenClothingCategory hat { get; set; }
	[Group("Clothing - Hair"), Property, InlineEditor] public CitizenClothingCategory hair { get; set; }
	[Group("Clothing - Facial"), Property, InlineEditor] public CitizenClothingCategory facial { get; set; }
	[Group("Clothing - Tops"), Property, InlineEditor] public CitizenClothingCategory tops { get; set; }
	[Group("Clothing - Gloves"), Property, InlineEditor] public CitizenClothingCategory gloves { get; set; }
	[Group("Clothing - Bottoms"), Property, InlineEditor] public CitizenClothingCategory bottoms { get; set; }
	[Group("Clothing - Footwear"), Property, InlineEditor] public CitizenClothingCategory footwear { get; set; }
	[Group("Clothing - Skin"), Property, InlineEditor] public CitizenClothingCategory skin { get; set; }

	[Button("GetAll")]
	public void GetAll()
	{
		var allClothing = ResourceLibrary.GetAll<Clothing>();

		foreach (var clothing in allClothing)
		{
			var category = ClothingCategoryToInternalCategory(clothing.Category);

			var inst = new CitizenClothing();
			inst.clothing = clothing;
			inst.tintMode = clothing.AllowTintSelect ? TintMode.Allow : TintMode.None;
			category.clothing.Add(inst);
		}
	}

	public CitizenClothingInst GetRandomClothingForCategory(ClothingCategory clothingCategory, bool isBadGuy, List<CitizenClothingInst> compatibilityCheck = null)
	{
		var category = ClothingCategoryToInternalCategory(clothingCategory);
		var inst = new CitizenClothingInst();
		float randomChance = Random.Shared.Float(1);

		if (randomChance > category.chanceOf)
		{
			return null;
		}

		float randomChancePerType = Random.Shared.Float(1);

		var validClothing = new List<CitizenClothing>();
		foreach (var clothing in category.clothing)
		{
			float randomChanceThreshold = isBadGuy ? clothing.chanceForBadGuy : clothing.chanceForGoodGuy;
			if (randomChancePerType > randomChanceThreshold)
			{
				continue;
			}

			if (clothing.clothing.SubCategory == "Full Outfits")
			{
				continue;
			}

			if (compatibilityCheck != null && compatibilityCheck.Count > 0)
			{
				bool incompatible = false;
				foreach (var compatabilityClothing in compatibilityCheck)
				{
					if (compatabilityClothing?.clothing == null)
						continue;

					if (!compatabilityClothing.clothing.CanBeWornWith(clothing.clothing))
					{
						incompatible = true;
						break;
					}
				}

				if (incompatible)
					continue;
			}

			validClothing.Add(clothing);
		}

		if (validClothing == null || validClothing.Count <= 0)
		{
			return null;
		}

		var randomCitizenClothing = validClothing.Random();
		inst.clothing = randomCitizenClothing.clothing;

		var tints = new List<Color>();
		if (randomCitizenClothing.tintSelection != null)
		{
			tints.AddRange(randomCitizenClothing.tintSelection);
		}

		if (randomCitizenClothing.tintMode == TintMode.Allow)
		{
			if (category.tintSelection != null)
			{
				tints.AddRange(category.tintSelection);
			}
		}
		if (randomCitizenClothing.tintMode != TintMode.None)
		{
			if (tints.Count > 0)
			{
				inst.tintSelection = tints.Random();
			}
		}

		return inst;
	}

	public List<CitizenClothingInst> GetRandomClothingFull(bool isBadGuy)
	{
		var clothing = new List<CitizenClothingInst>();
		foreach (ClothingCategory clothingCategory in Enum.GetValues(typeof(ClothingCategory)))
		{
			if (clothingCategory == ClothingCategory.None || clothingCategory == ClothingCategory.Skin)
			{
				continue;
			}
			var inst = GetRandomClothingForCategory(clothingCategory, isBadGuy);
			if (inst.clothing.SubCategory == "Full Outfits")
			{
				continue;
			}
			clothing.Add(inst);
		}
		return clothing;
	}

	CitizenClothingCategory ClothingCategoryToInternalCategory(ClothingCategory clothingCategory)
	{
		switch (clothingCategory)
		{
			case ClothingCategory.Hat:
				return hat;
			case ClothingCategory.Hair:
				return hair;
			case ClothingCategory.Facial:
				return facial;
			case ClothingCategory.Tops:
				return tops;
			case ClothingCategory.Gloves:
				return gloves;
			case ClothingCategory.Bottoms:
				return bottoms;
			case ClothingCategory.Footwear:
				return footwear;
			case ClothingCategory.Skin:
				return skin;
		}

		Log.Error($"ClothingCategoryToInternalCategory() known ClothingCategory: {clothingCategory}");
		return null;
	}
}
