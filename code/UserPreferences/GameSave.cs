
using Sandbox.Citizen;
using Sandbox.Internal;
using System.IO;
using System;
using Sandbox;

public class GameSave : UserPreferences<GameSave>
{
	//public float temp { get; set; } = 1.0f;
	public Dictionary<string, float> levelNameToBestTime { get; set; } = new Dictionary<string, float>(StringComparer.OrdinalIgnoreCase);

	public int furthestLevelPlayed { get; set; } = 0;
	public int furthestLevelComplete { get; set; } = 0;
}
