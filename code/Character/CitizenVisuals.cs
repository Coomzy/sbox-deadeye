
using Sandbox;
using Sandbox.Citizen;
using System;
using System.Numerics;
using System.Security;
using System.Text.Json;
using System.Threading;
using static Sandbox.Citizen.CitizenAnimationHelper;
using static Sandbox.Clothing;
using static Sandbox.ClothingContainer;
using static Sandbox.Gizmo;

public class CitizenVisuals : Component, IRestartable, IShutdown
{
	[Group("Setup"), Property] public GameObject clothingHolder { get; set; }
	[Group("Setup"), Property] public GameObject weaponHolder { get; set; }
	[Group("Setup"), Property] public SkinnedModelRenderer bodyRenderer { get; set; }
	[Group("Setup"), Property] public ModelPhysics bodyPhysics { get; set; }
	[Group("Setup"), Property] public CitizenAnimationHelper animationHelper { get; set; }
	[Group("Setup"), Property] Target _target { get; set; }

	[Group("Random"), Property] string randomStart { get; set; }
	[Group("Clear"), Property] string clearStart { get; set; }

	[Group("Animation"), Property] public CitizenAnimationHelper.HoldTypes? holdTypeOverride { get; set; }
	[Group("Animation"), Property] public CitizenAnimationHelper.Hand handedness { get; set; } = CitizenAnimationHelper.Hand.Right;

	[Group("Character"), Range(0.5f, 1.5f), Property] public float characterHeight { get; set; } = 1.0f;
	[Group("Character"), Range(0.0f, 1.0f), Property] public float duckHeight { get; set; } = 0.0f;

	[Group("Body Groups"), Property] public bool head { get; set; } = true;
	[Group("Body Groups"), Property] public bool chest { get; set; } = false;
	[Group("Body Groups"), Property] public bool legs { get; set; } = false;
	[Group("Body Groups"), Property] public bool hands { get; set; } = true;
	[Group("Body Groups"), Property] public bool feet { get; set; } = false;

	[Group("Clothing - Hat"), Property, InlineEditor] public CitizenClothingInst hatClothing { get; set; }
	[Group("Clothing - Hair"), Property, InlineEditor] public CitizenClothingInst hairClothing { get; set; }
	[Group("Clothing - Facial"), Property, InlineEditor] public CitizenClothingInst facialClothing { get; set; }
	[Group("Clothing - Tops"), Property, InlineEditor] public CitizenClothingInst topsClothing { get; set; }
	[Group("Clothing - Gloves"), Property, InlineEditor] public CitizenClothingInst glovesClothing { get; set; }
	[Group("Clothing - Bottoms"), Property, InlineEditor] public CitizenClothingInst bottomsClothing { get; set; }
	[Group("Clothing - Footwear"), Property, InlineEditor] public CitizenClothingInst footwearClothing { get; set; }
	[Group("Clothing - Skin"), Property, InlineEditor] public CitizenClothingInst skinClothing { get; set; }

	[Group("Clothing"), Property] public List<Clothing> clothingList { get; set; }

	[Group("Weapon"), Property] public WeaponType weaponType { get; set; } = WeaponType.Pistol;

	[Group("Runtime"), Property] public GameObject weaponGameObject { get; set; }
	[Group("Runtime"), Property] public Weapon weapon { get; set; }
	[Group("Runtime"), Property] public Target target => _target ?? GameObject.Components.Get<Target>();

	CancellationTokenSource cancellationTokenSource;

	Vector3 startPos;
	Rotation startRot;

	protected override void OnAwake()
	{
		base.OnAwake();

		startPos = GameObject.Transform.Position;
		startRot = GameObject.Transform.Rotation;
		cancellationTokenSource = new CancellationTokenSource();

		// Need this while UpdateFromPrefab() is broken in edit time
		if (weapon == null && weaponGameObject != null)
		{
			weaponGameObject.BreakFromPrefab();
			weapon = weaponGameObject.Components.Get<Weapon>();
			bool shouldHaveWeapon = target != null ? target.isBadTarget : false;

			if (!shouldHaveWeapon)
			{
				weaponGameObject.Enabled = false;
			}
		}

		//RuntimeApply();
		//animationHelper.HoldType = HoldTypes.None;
	}

	public void PreRestart()
	{
		GameObject.Transform.Position = startPos;
		GameObject.Transform.Rotation = startRot;

		bodyPhysics.Enabled = false;
		bodyRenderer.UseAnimGraph = true;

		bodyRenderer.GameObject.Tags.Set("ragdoll", false);
		//bodyRenderer.GameObject.SetParent(null);
		bodyRenderer.Transform.ClearInterpolation();
		bodyRenderer.Transform.LocalPosition = Vector3.Zero;
		bodyRenderer.Transform.LocalRotation = Quaternion.Identity;

		animationHelper.HoldType = HoldTypes.None;
		animationHelper.Handedness = Hand.Right;
		animationHelper.DuckLevel = 0.0f;
	}

	public void PostRestart()
	{

	}

	public void PreShutdown()
	{
		this.Enabled = false;
	}

	public void PostShutdown()
	{

	}

	[Group("Body Groups"), Button("Apply")]
	public void Apply_BodyGroup()
	{
		bodyRenderer.SetBodyGroup("head", head ? 1 : 0);
		bodyRenderer.SetBodyGroup("chest", chest ? 1 : 0);
		bodyRenderer.SetBodyGroup("legs", legs ? 1 : 0);
		bodyRenderer.SetBodyGroup("hands", hands ? 1 : 0);
		bodyRenderer.SetBodyGroup("feet", feet ? 1 : 0);
	}

	public void RuntimeApply()
	{
		var holdType = GameSettings.instance.GetWeaponHoldType(weaponType);
		if (holdTypeOverride.HasValue)
		{
			holdType = holdTypeOverride.Value;
		}
		animationHelper.HoldType = holdType;
		animationHelper.Handedness = handedness;
		animationHelper.DuckLevel = duckHeight;

		bool shouldHaveWeapon = target != null ? target.isBadTarget : false;

		if (weaponGameObject != null)
		{
			//weaponGameObject.Enabled = shouldHaveWeapon;
		}
	}

	void Apply_Clothing()
	{
		//clothingHolder.Clear();
		var children = new List<GameObject>(clothingHolder.Children);
		foreach (var child in children)
		{
			child.Destroy();
		}

		foreach (var clothing in clothingList)
		{
			GameObject gameObject = GameObject.Scene.CreateObject(true);
			if (Game.IsEditor)
			{
				gameObject.Name = $"Clothing - {clothing.ResourceName}";
			}
			gameObject.SetParent(clothingHolder, false);

			var skinnedModelRenderer = gameObject.Components.Create<SkinnedModelRenderer>(true);

			skinnedModelRenderer.Model = Model.Load(clothing.Model);
			skinnedModelRenderer.UseAnimGraph = true;
			skinnedModelRenderer.BoneMergeTarget = bodyRenderer;
		}
	}

	void Apply_Weapon()
	{
		if (weaponGameObject != null)
		{
			weaponGameObject.Destroy();
		}

		var weaponPrefab = GameSettings.instance.GetWeaponPrefab(weaponType);
		if (weaponPrefab == null)
		{
			return;
		}
		weaponGameObject = GameObject.Scene.CreateObject(true);
		if (Game.IsEditor)
		{
			weaponGameObject.Name = weaponPrefab.ResourceName;
		}
		weaponGameObject.SetParent(weaponHolder, false);
		weaponGameObject.Transform.LocalPosition = Vector3.Zero;
		weaponGameObject.Transform.LocalRotation = Rotation.Identity;
		weaponGameObject.SetPrefabSource(weaponPrefab.ResourcePath);

		// HACK: This errors in edit time. https://github.com/Facepunch/sbox-issues/issues/6169
		if (!Scene.IsEditor)
		{
			weaponGameObject.UpdateFromPrefab();
		}

		weapon = weaponGameObject.Components.Get<Weapon>();
	}

	[Category("Random"), Button("Random Clothing")]
	public void RandomClothing()
	{
		var children = new List<GameObject>(clothingHolder.Children);
		foreach (var child in children)
		{
			child.Destroy();
		}

		hatClothing = null;
		hairClothing = null;
		facialClothing = null;
		topsClothing = null;
		glovesClothing = null;
		bottomsClothing = null;
		footwearClothing = null;

		hatClothing = CitizenSettings.instance.GetRandomClothingForCategory(ClothingCategory.Hat, target.isBadTarget, GetAllClothingInsts());
		hairClothing = CitizenSettings.instance.GetRandomClothingForCategory(ClothingCategory.Hair, target.isBadTarget, GetAllClothingInsts());
		facialClothing = CitizenSettings.instance.GetRandomClothingForCategory(ClothingCategory.Facial, target.isBadTarget, GetAllClothingInsts());
		topsClothing = CitizenSettings.instance.GetRandomClothingForCategory(ClothingCategory.Tops, target.isBadTarget, GetAllClothingInsts());
		glovesClothing = CitizenSettings.instance.GetRandomClothingForCategory(ClothingCategory.Gloves, target.isBadTarget, GetAllClothingInsts());
		bottomsClothing = CitizenSettings.instance.GetRandomClothingForCategory(ClothingCategory.Bottoms, target.isBadTarget, GetAllClothingInsts());
		footwearClothing = CitizenSettings.instance.GetRandomClothingForCategory(ClothingCategory.Footwear, target.isBadTarget, GetAllClothingInsts());
		//skinClothing = CitizenSettings.instance.GetRandomClothingForCateogry(ClothingCategory.Skin, target.isBadTarget);

		//var clothingInsts = CitizenSettings.instance.GetRandomClothingFull(target.isBadTarget);
		var clothingInsts = GetAllClothingInsts();

		foreach (var clothingInst in clothingInsts)
		{
			if (clothingInst?.clothing == null)
				continue;

			ApplyClothingInst(clothingInst);
		}

		UpdateBodyGroups();
		//Apply_BodyGroup();

		//bodyRenderer.SetBodyGroup("head", (bodyGroups & Clothing.BodyGroups.Head) == Clothing.BodyGroups.Head ? 1 : 0);
		//bodyRenderer.SetBodyGroup("chest", (bodyGroups & Clothing.BodyGroups.Chest) == Clothing.BodyGroups.Chest ? 1 : 0);
		//bodyRenderer.SetBodyGroup("legs", (bodyGroups & Clothing.BodyGroups.Legs) == Clothing.BodyGroups.Legs ? 1 : 0);
		//bodyRenderer.SetBodyGroup("hands", (bodyGroups & Clothing.BodyGroups.Hands) == Clothing.BodyGroups.Hands ? 1 : 0);
		//bodyRenderer.SetBodyGroup("feet", (bodyGroups & Clothing.BodyGroups.Feet) == Clothing.BodyGroups.Feet ? 1 : 0);
	}

	public List<CitizenClothingInst> GetAllClothingInsts()
	{
		var clothingInsts = new List<CitizenClothingInst>();

		if (hatClothing != null) clothingInsts.Add(hatClothing);
		if (hairClothing != null) clothingInsts.Add(hairClothing);
		if (facialClothing != null) clothingInsts.Add(facialClothing);
		if (topsClothing != null) clothingInsts.Add(topsClothing);
		if (glovesClothing != null) clothingInsts.Add(glovesClothing);
		if (bottomsClothing != null) clothingInsts.Add(bottomsClothing);
		if (footwearClothing != null) clothingInsts.Add(footwearClothing);
		if (skinClothing != null) clothingInsts.Add(skinClothing);

		return clothingInsts;
	}

	[Category("Character"), Button("Random Height")]
	public void RandomHeight()
	{
		characterHeight = CitizenSettings.instance.GetRandomCharacterHeight();
	}

	[Category("Character"), Button("Random Duck")]
	public void RandomDuckHeight()
	{
		duckHeight = CitizenSettings.instance.GetRandomCharacterDuckHeight(target.isBadTarget);
	}

	[Category("Random"), Button("Random Hat")]
	void RandomHatClothing()
	{
		DestroyClothing(hatClothing);
		hatClothing = CitizenSettings.instance.GetRandomClothingForCategory(ClothingCategory.Hat, target.isBadTarget, GetAllClothingInsts());
		ApplyClothingInst(hatClothing);
		UpdateBodyGroups();
	}

	[Category("Random"), Button("Random Hair")]
	void RandomHairClothing()
	{
		DestroyClothing(hairClothing);
		hairClothing = CitizenSettings.instance.GetRandomClothingForCategory(ClothingCategory.Hair, target.isBadTarget, GetAllClothingInsts());
		ApplyClothingInst(hairClothing);
		UpdateBodyGroups();
	}

	[Category("Random"), Button("Random Facial")]
	void RandomFacialClothing()
	{
		DestroyClothing(facialClothing);
		facialClothing = CitizenSettings.instance.GetRandomClothingForCategory(ClothingCategory.Facial, target.isBadTarget, GetAllClothingInsts());
		ApplyClothingInst(facialClothing);
		UpdateBodyGroups();
	}

	[Category("Random"), Button("Random Tops")]
	void RandomTopsClothing()
	{
		DestroyClothing(topsClothing);
		topsClothing = CitizenSettings.instance.GetRandomClothingForCategory(ClothingCategory.Tops, target.isBadTarget, GetAllClothingInsts());
		ApplyClothingInst(topsClothing);
		UpdateBodyGroups();
	}

	[Category("Random"), Button("Random Gloves")]
	void RandomGlovesClothing()
	{
		DestroyClothing(glovesClothing);
		glovesClothing = CitizenSettings.instance.GetRandomClothingForCategory(ClothingCategory.Gloves, target.isBadTarget, GetAllClothingInsts());
		ApplyClothingInst(glovesClothing);
		UpdateBodyGroups();
	}

	[Category("Random"), Button("Random Bottoms")]
	void RandomBottomsClothing()
	{
		DestroyClothing(bottomsClothing);
		bottomsClothing = CitizenSettings.instance.GetRandomClothingForCategory(ClothingCategory.Bottoms, target.isBadTarget, GetAllClothingInsts());
		ApplyClothingInst(bottomsClothing);
		UpdateBodyGroups();
	}

	[Category("Random"), Button("Random Footwear")]
	void RandomFootwearClothing()
	{
		DestroyClothing(footwearClothing);
		footwearClothing = CitizenSettings.instance.GetRandomClothingForCategory(ClothingCategory.Footwear, target.isBadTarget, GetAllClothingInsts());
		ApplyClothingInst(footwearClothing);
		UpdateBodyGroups();
	}

	[Category("Random"), Button("Random Skin")]
	void RandomSkinClothing()
	{
		DestroyClothing(skinClothing);
		skinClothing = CitizenSettings.instance.GetRandomClothingForCategory(ClothingCategory.Skin, target.isBadTarget, GetAllClothingInsts());
		ApplyClothingInst(skinClothing);
		UpdateBodyGroups();
	}

	[Category("Clear"), Button("Clear All")]
	void ClearAllClothing()
	{
		var children = new List<GameObject>(clothingHolder.Children);
		foreach (var child in children)
		{
			child.Destroy();
		}

		hatClothing = null;
		hairClothing = null;
		facialClothing = null;
		topsClothing = null;
		glovesClothing = null;
		bottomsClothing = null;
		footwearClothing = null;

		UpdateBodyGroups();
	}

	[Category("Clear"), Button("Clear Hat")]
	void ClearHatClothing()
	{
		DestroyClothing(hatClothing);
		hatClothing = null;
		UpdateBodyGroups();
	}

	[Category("Clear"), Button("Clear Hair")]
	void ClearHairClothing()
	{
		DestroyClothing(hairClothing);
		hairClothing = null;
		UpdateBodyGroups();
	}

	[Category("Clear"), Button("Clear Facial")]
	void ClearFacialClothing()
	{
		DestroyClothing(facialClothing);
		facialClothing = null;
		UpdateBodyGroups();
	}

	[Category("Clear"), Button("Clear Tops")]
	void ClearTopsClothing()
	{
		DestroyClothing(topsClothing);
		topsClothing = null;
		UpdateBodyGroups();
	}

	[Category("Clear"), Button("Clear Gloves")]
	void ClearGlovesClothing()
	{
		DestroyClothing(glovesClothing);
		glovesClothing = null;
		UpdateBodyGroups();
	}

	[Category("Clear"), Button("Clear Bottoms")]
	void ClearBottomsClothing()
	{
		DestroyClothing(bottomsClothing);
		bottomsClothing = null;
		UpdateBodyGroups();
	}

	[Category("Clear"), Button("Clear Footwear")]
	void ClearFootwearClothing()
	{
		DestroyClothing(footwearClothing);
		footwearClothing = null;
		UpdateBodyGroups();
	}

	[Category("Clear"), Button("Clear Skin")]
	void ClearSkinClothing()
	{
		DestroyClothing(skinClothing);
		skinClothing = null;
		UpdateBodyGroups();
	}


	void DestroyClothing(CitizenClothingInst clothingInst)
	{
		if (clothingInst?.clothing != null)
		{
			var children = new List<GameObject>(clothingHolder.Children);
			foreach (var child in children)
			{
				var name = $"Clothing - {clothingInst.clothing.ResourceName}";
				if (child.Name != name)
				{
					continue;
				}
				child.Destroy();
			}
		}
	}

	public void UpdateBodyGroups(bool apply = true)
	{
		bool foundHeadBodyGroup = false;
		Clothing.BodyGroups bodyGroups = Clothing.BodyGroups.Head;

		var clothingInsts = GetAllClothingInsts();

		foreach (var clothingInst in clothingInsts)
		{
			if (clothingInst?.clothing == null)
				continue;

			bodyGroups |= clothingInst.clothing.HideBody;

			bool hasHeadBodyGroup = (clothingInst.clothing.HideBody & Clothing.BodyGroups.Head) == Clothing.BodyGroups.Head;
			if (hasHeadBodyGroup)
			{
				foundHeadBodyGroup = true;
			}
		}

		if (!foundHeadBodyGroup)
		{
			bodyGroups &= ~Clothing.BodyGroups.Head;
		}

		head = (bodyGroups & Clothing.BodyGroups.Head) == Clothing.BodyGroups.Head;
		chest = (bodyGroups & Clothing.BodyGroups.Chest) == Clothing.BodyGroups.Chest;
		legs = (bodyGroups & Clothing.BodyGroups.Legs) == Clothing.BodyGroups.Legs;
		hands = (bodyGroups & Clothing.BodyGroups.Hands) == Clothing.BodyGroups.Hands;
		feet = (bodyGroups & Clothing.BodyGroups.Feet) == Clothing.BodyGroups.Feet;

		if (apply)
		{
			Apply_BodyGroup();
		}
	}

	public void ApplyClothingInst(CitizenClothingInst clothingInst)
	{
		if (clothingInst?.clothing == null)
		{
			return;
		}

		GameObject gameObject = GameObject.Scene.CreateObject(true);
		if (Game.IsEditor)
		{
			gameObject.Name = $"Clothing - {clothingInst.clothing.ResourceName}";
		}
		gameObject.SetParent(clothingHolder, false);

		var skinnedModelRenderer = gameObject.Components.Create<SkinnedModelRenderer>(true);

		skinnedModelRenderer.Model = Model.Load(clothingInst.clothing.Model);
		skinnedModelRenderer.UseAnimGraph = true;
		skinnedModelRenderer.BoneMergeTarget = bodyRenderer;

		if (clothingInst.tintSelection.HasValue)
		{
			skinnedModelRenderer.Tint = clothingInst.tintSelection.Value;
		}

		//bodyRenderer.SetBodyGroup("head", (bodyGroups & Clothing.BodyGroups.Head) == Clothing.BodyGroups.Head ? 1 : 0);
		//bodyRenderer.SetBodyGroup("chest", (bodyGroups & Clothing.BodyGroups.Chest) == Clothing.BodyGroups.Chest ? 1 : 0);
		//bodyRenderer.SetBodyGroup("legs", (bodyGroups & Clothing.BodyGroups.Legs) == Clothing.BodyGroups.Legs ? 1 : 0);
		//bodyRenderer.SetBodyGroup("hands", (bodyGroups & Clothing.BodyGroups.Hands) == Clothing.BodyGroups.Hands ? 1 : 0);
		//bodyRenderer.SetBodyGroup("feet", (bodyGroups & Clothing.BodyGroups.Feet) == Clothing.BodyGroups.Feet ? 1 : 0);
	}

	public void Die(Vector3 force)
	{
		bodyPhysics.Enabled = true;
		bodyRenderer.UseAnimGraph = false;

		bodyRenderer.GameObject.Tags.Set("ragdoll", true);
		//bodyRenderer.GameObject.SetParent(null);
		bodyRenderer.Transform.ClearInterpolation();

		PhysicsBody hitPhysBody = null;

		//GroupName: pelvis, GroupIndex: 0
		//GroupName: spine_0, GroupIndex: 1
		//GroupName: spine_2, GroupIndex: 2
		//GroupName: head, GroupIndex: 3
		//GroupName: arm_upper_R, GroupIndex: 4
		//GroupName: arm_lower_R, GroupIndex: 5
		//GroupName: hand_R, GroupIndex: 6
		//GroupName: arm_upper_L, GroupIndex: 7
		//GroupName: arm_lower_L, GroupIndex: 8
		//GroupName: hand_L, GroupIndex: 9
		//GroupName: leg_upper_R, GroupIndex: 10
		//GroupName: leg_lower_R, GroupIndex: 11
		//GroupName: ankle_R, GroupIndex: 12
		//GroupName: leg_upper_L, GroupIndex: 13
		//GroupName: leg_lower_L, GroupIndex: 14
		//GroupName: ankle_L, GroupIndex: 15

		var pickedBodyGroupIndex = -1;
		var rndIndex = Game.Random.Int(0, 3);

		switch (rndIndex)
		{
			// head
			case 0:
				pickedBodyGroupIndex = 3;
				break;
			// spine_2
			case 1:
				pickedBodyGroupIndex = 2;
				break;
			// arm_upper_R
			case 2:
				pickedBodyGroupIndex = 4;
				break;
			// arm_upper_L
			case 3:
				pickedBodyGroupIndex = 7;
				break;
		}

		foreach (var body in bodyPhysics.PhysicsGroup.Bodies)
		{
			if (pickedBodyGroupIndex != body.GroupIndex)
				continue;

			hitPhysBody = body;
			break;
		}

		if (hitPhysBody != null)
		{
			hitPhysBody.ApplyImpulse(force * 10.0f);
		}

		//var randomBody = bodyPhysics.PhysicsGroup.Bodies.Random();
		/*foreach (var body in bodyPhysics.PhysicsGroup.Bodies)
		{
			//body.ApplyImpulseAt(hitPosition, force);
			//body.ApplyImpulse(force);
		}*/

		if (weaponGameObject == null)
		{
			return;
		}

		if (weapon == null)
		{
			weapon = weaponGameObject.Components.Get<Weapon>();
		}
		if (weapon != null)
		{
			var weaponForce = force + Utils.GetRandomizedDirection(force, 35.0f);
			weapon.Drop(weaponForce * 0.10f);
		}

		//DisableRagdoll();
	}

	async void DisableRagdoll()
	{
		//await Task.DelayRealtimeSeconds(5.0f);
		await GameTask.DelayRealtimeSeconds(5.0f, cancellationTokenSource.Token);
		if (cancellationTokenSource != null && cancellationTokenSource.IsCancellationRequested)
		{
			return;
		}
		bodyPhysics.PhysicsGroup.Sleeping = true;
	}

	protected override void OnDestroy()
	{
		if (cancellationTokenSource != null)
		{
			cancellationTokenSource.Cancel();
		}

		base.OnDestroy();
	}
}
