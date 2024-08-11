
using Sandbox;
using Sandbox.Citizen;
using System;
using System.Text.Json;
using static Sandbox.Gizmo;

public static class Stats
{
	public const string TARGETS_ELIMINATED = "targets-eliminated";
	public const string CIVILIANS_KILLED = "civilians-killed";
	public const string FAILURE_TOO_MANY_CIVS_KILLED = "failure-too-many-civs-killed";
	public const string WON = "won";
	public const string DIED = "died";

	public static void Increment(string stat, double incrementAmount = 1)
	{
		// Once the game is published, we'll stop editor stats
		if (Game.IsEditor)
		{
			//return;
		}
		Sandbox.Services.Stats.Increment(stat, incrementAmount);
	}

	public static void Reset()
	{
		Sandbox.Services.Stats.SetValue(TARGETS_ELIMINATED, 0);
		Sandbox.Services.Stats.SetValue(CIVILIANS_KILLED, 0);
		Sandbox.Services.Stats.SetValue(FAILURE_TOO_MANY_CIVS_KILLED, 0);
		Sandbox.Services.Stats.SetValue(WON, 0);
		Sandbox.Services.Stats.SetValue(DIED, 0);
	}
}
