
using Sandbox;
using Sandbox.Citizen;
using System;
using System.Numerics;
using System.Text.Json;
using System.Transactions;
using static Sandbox.Gizmo;

public class Target : Component
{
	[Group("Setup"), Property] public SkinnedModelRenderer bodyRenderer { get; set; }
	[Group("Setup"), Property] public ModelPhysics bodyPhysics { get; set; }
	[Group("Setup"), Property] public CitizenAnimationHelper animationHelper { get; set; }
	[Group("Setup"), Property] public CitizenVisuals citizenVisuals { get; set; }
	[Group("Setup"), Property] public HighlightOutline highlightOutline { get; set; }

	[Group("Config"), Property] public bool isBadTarget { get; set; } = true;
	[Group("Config"), Property] public bool lookAtPlayer { get; set; } = true;

	[Group("Runtime"), Property] public bool isDead { get; set; } = false;

	protected override void OnStart()
	{
		base.OnStart();

		highlightOutline.Color = isBadTarget ? GameSettings.instance.badHighlightColour : GameSettings.instance.goodHighlightColour;
		highlightOutline.ObscuredColor = highlightOutline.Color;
		highlightOutline.Enabled = false;
	}

	public void Activate()
	{
		citizenVisuals.RuntimeApply();
	}

	public void Select()
	{
		highlightOutline.Enabled = true;
	}

	public void Deselect()
	{
		highlightOutline.Enabled = false;
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

		var dirToWalkPos = Vector3.Direction(GameObject.Transform.Position, walkPos).WithZ(0.0f);
		Transform.Rotation = Rotation.From(dirToWalkPos.Normal.EulerAngles);
	}

	[Button("Die")]
	public void Die()
	{
		isDead = true;
		citizenVisuals.Die();
	}
}
