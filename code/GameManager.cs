
using Sandbox;
using Sandbox.Citizen;
using System;
using System.Text.Json;
using static Sandbox.Gizmo;

public class GameManager : Component
{
	public static GameManager instance;

	protected override void OnAwake()
	{
		instance = this;

		base.OnAwake();
	}
}
