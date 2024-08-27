
using Sandbox;
using Sandbox.Citizen;
using System;
using System.Numerics;
using System.Text.Json;
using System.Transactions;
using static Sandbox.Gizmo;

public class Target : Component, IRestartable, IShutdown
{
	[Group("Setup"), Property] public SkinnedModelRenderer bodyRenderer { get; set; }
	[Group("Setup"), Property] public ModelPhysics bodyPhysics { get; set; }
	[Group("Setup"), Property] public CitizenAnimationHelper animationHelper { get; set; }
	[Group("Setup"), Property] public CitizenVisuals citizenVisuals { get; set; }
	[Group("Setup"), Property] public HighlightOutline highlightOutline { get; set; }

	[Group("Config"), Property] public bool isBadTarget { get; set; } = true;
	[Group("Config"), Property] public bool lookAtPlayer { get; set; } = true;
	[Group("Config"), Property] public bool isTonyLazuto { get; set; } = false;

	[Group("Runtime"), Property] public bool isDead { get; set; } = false;

	GameObject originalParent;

	public List<SkinnedModelRenderer> allRenderers { get; set; }

	protected override void OnAwake()
	{
		base.OnAwake();
		
		highlightOutline.Enabled = false;
		//highlightOutline.Color = Color.Transparent;
		//highlightOutline.ObscuredColor = highlightOutline.Color;

		originalParent = GameObject.Parent;

		allRenderers = Components.GetAll<SkinnedModelRenderer>(FindMode.EverythingInDescendants).ToList();
	}

	protected override void OnStart()
	{
		base.OnStart();

		//highlightOutline.Color = isBadTarget ? GameSettings.instance.badHighlightColour : GameSettings.instance.goodHighlightColour;
		//highlightOutline.ObscuredColor = highlightOutline.Color;
		//highlightOutline.Enabled = false;
	}

	public void PreRestart()
	{
		isDead = false;

		GameObject.SetParent(originalParent);
		highlightOutline.Color = Color.Transparent;
		highlightOutline.ObscuredColor = Color.Transparent;
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

	public Vector3 GetHeadPos()
	{
		var headBone = bodyRenderer.GetBoneObject("head");
		if (headBone != null)
		{
			return headBone.Transform.Position;
		}
		return Transform.Position;
	}

	public void Activate()
	{
		citizenVisuals.RuntimeApply();
	}

	public void Select()
	{
		if (GlobalHighlight.instance != null)
		{
			GameObject.SetParent(GlobalHighlight.instance.GameObject);
			GlobalHighlight.instance.highlightOutline.Color = isBadTarget ? GameSettings.instance.badHighlightColour : GameSettings.instance.goodHighlightColour;
			GlobalHighlight.instance.highlightOutline.ObscuredColor = highlightOutline.Color;
		}

		//highlightOutline.Color = isBadTarget ? GameSettings.instance.badHighlightColour : GameSettings.instance.goodHighlightColour;
		//highlightOutline.ObscuredColor = highlightOutline.Color;
		//highlightOutline.Enabled = true;
	}

	public void Deselect()
	{
		//highlightOutline.Enabled = false;
		//highlightOutline.Color = Color.Transparent;
		//highlightOutline.ObscuredColor = highlightOutline.Color;

		if (GlobalHighlight.instance != null)
		{
			GameObject.SetParent(originalParent);
			GlobalHighlight.instance.highlightOutline.Color = Color.Transparent;
			GlobalHighlight.instance.highlightOutline.ObscuredColor = Color.Transparent;
		}
	}

	[Button("LookAtPlayer")]
	public void LookAtPlayerWalkPos()
	{
		var walkPos = Vector3.Zero;
		if (Player_TD.instance?.Transform != null)
		{
			walkPos = Player_TD.instance.Transform.Position;
		}

		if (Game.IsPlaying)
		{
			var room = Components.GetInAncestorsOrSelf<Room>(true);
			if (room != null)
			{
				walkPos = room.walkToPos;
			}
		}

		LookAtPoint(walkPos);
	}

	public void LookAtPoint(Vector3 point)
	{
		var dirToWalkPos = Vector3.Direction(GameObject.Transform.Position, point).WithZ(0.0f);
		Transform.Rotation = Rotation.From(dirToWalkPos.Normal.EulerAngles);
	}

	[Button("Die")]
	public void Die(Vector3 force)
	{
		isDead = true;
		citizenVisuals.Die(force);

		if (isTonyLazuto)
		{
			GameStats.Increment(GameStats.KILLED_TONY_LAZUTO);
		}
	}
}
