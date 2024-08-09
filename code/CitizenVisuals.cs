
using Sandbox;
using Sandbox.Citizen;
using System;
using System.Text.Json;
using static Sandbox.ClothingContainer;
using static Sandbox.Gizmo;

public class CitizenVisuals : Component, Component.ExecuteInEditor
{
	[Group("Setup"), Property] public GameObject clothingHolder { get; set; }
	[Group("Setup"), Property] public GameObject weaponHolder { get; set; }
	[Group("Setup"), Property] public SkinnedModelRenderer bodyRenderer { get; set; }
	[Group("Setup"), Property] public ModelPhysics bodyPhysics { get; set; }
	[Group("Setup"), Property] public CitizenAnimationHelper animationHelper { get; set; }

	[Group("Body Groups"), Property] public bool head { get; set; } = true;
	[Group("Body Groups"), Property] public bool chest { get; set; } = false;
	[Group("Body Groups"), Property] public bool legs { get; set; } = false;
	[Group("Body Groups"), Property] public bool hands { get; set; } = true;
	[Group("Body Groups"), Property] public bool feet { get; set; } = false;

	[Group("Animation"), Property] public CitizenAnimationHelper.HoldTypes? holdTypeOverride { get; set; }
	[Group("Animation"), Property] public CitizenAnimationHelper.Hand handedness { get; set; } = CitizenAnimationHelper.Hand.Right;

	[Group("Clothing"), Property] public List<Clothing> clothingList { get; set; }

	[Group("Character"), Range(0.5f, 1.5f), Property] public float characterHeight { get; set; } = 1.0f;

	[Group("Weapon"), Property] public WeaponType weaponType { get; set; } = WeaponType.Pistol;
	[Group("Weapon"), Property] public GameObject weaponGameObject { get; set; }
	[Group("Weapon"), Property] public Weapon weapon { get; set; }

	protected override void OnAwake()
	{
		base.OnAwake();

		if (weapon == null && weaponGameObject != null)
		{
			weaponGameObject.BreakFromPrefab();
			weapon = weaponGameObject.Components.Get<Weapon>();
		}
	}

	public void Apply(bool fullUpdate = false)
	{
		bodyRenderer.SetBodyGroup("head", head ? 0 : 1);
		bodyRenderer.SetBodyGroup("chest", chest ? 0 : 1);
		bodyRenderer.SetBodyGroup("legs", legs ? 0 : 1);
		bodyRenderer.SetBodyGroup("hands", hands ? 0 : 1);
		bodyRenderer.SetBodyGroup("feet", feet ? 0 : 1);

		var holdType = GameSettings.instance.GetWeaponHoldType(weaponType);
		if (holdTypeOverride.HasValue)
		{
			holdType = holdTypeOverride.Value;
		}
		animationHelper.HoldType = holdType;
		animationHelper.Handedness = handedness;
		animationHelper.Height = characterHeight;
		//animationHelper.DuckLevel = duckLevel;

		if (fullUpdate)
		{
			Apply_Clothing();
			Apply_Weapon();
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
			gameObject.Name = $"Clothing - {clothing.ResourceName}";
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
		weaponGameObject.Name = weaponPrefab.ResourceName;
		weaponGameObject.SetParent(weaponHolder, false);
		weaponGameObject.Transform.LocalPosition = Vector3.Zero;
		weaponGameObject.Transform.LocalRotation = Rotation.Identity;
		weaponGameObject.SetPrefabSource(weaponPrefab.ResourcePath);
		weaponGameObject.UpdateFromPrefab();

		weapon = weaponGameObject.Components.Get<Weapon>();
	}

	[Button("Apply")]
	void Apply_Editor()
	{
		Apply(true);
		//Apply_Clothing();
		//Apply_Weapon();

		Log.Info($"Application.IsEditor = {Application.IsEditor}");
		Log.Info($"Game.IsEditor = {Game.IsEditor}");
		Log.Info($"Game.InGame = {Game.InGame}");
		Log.Info($"Application.IsStandalone = {Application.IsStandalone}");
	}

	public void Die()
	{
		bodyPhysics.Enabled = true;
		bodyRenderer.UseAnimGraph = false;

		bodyRenderer.GameObject.Tags.Set("ragdoll", true);
		bodyRenderer.GameObject.SetParent(null);
		bodyRenderer.Transform.ClearInterpolation();

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
			weapon.Drop();
		}
	}
}
