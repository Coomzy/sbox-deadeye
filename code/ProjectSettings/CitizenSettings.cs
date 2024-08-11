using Sandbox.Citizen;
using Sandbox.Internal;
using System.IO;
using System;
using Sandbox;
using static Sandbox.Clothing;

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

public class CitizenClothing
{
	[Group("Clothing"), Property] public Clothing clothing { get; set; }
	[Group("Target"), Property] public TargetType targetType { get; set; }
	[Group("Randomizer"), Property, Range(0.0f, 1.0f)] public float chanceForGoodGuy { get; set; } = 1.0f;
	[Group("Randomizer"), Property, Range(0.0f, 1.0f)] public float chanceForBadGuy { get; set; } = 1.0f;
	[Group("Tinting"), Property] public TintMode tintMode { get; set; } = TintMode.Allow;
	[Group("Tinting"), Property] public List<Gradient> tintSelection { get; set; }
}

public class CitizenClothingCategory
{
	[Group("Clothing"), Property, InlineEditor] public List<CitizenClothing> clothing { get; set; } = new List<CitizenClothing>();
	[Group("Tinting"), Property] public List<Gradient> tintSelection { get; set; }
}

[GameResource("Citizen Settings", "citizen", "Citizen Settings")]
public class CitizenSettings : GameResourceSingleton<CitizenSettings>
{
	[Group("Randomizer"), Property, Range(0.0f, 1.0f)] public float chanceOfHat { get; set; } = 0.3f;
	[Group("Randomizer"), Property, Range(0.0f, 1.0f)] public float chanceOfHair { get; set; } = 0.9f;

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
