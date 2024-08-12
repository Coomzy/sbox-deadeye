
using Sandbox.Citizen;
using Sandbox.Internal;
using System.IO;
using System;
using Sandbox;
using System.Text.Json.Serialization;

public enum PlayerBotMode
{
	None,
	SlowestTime,
	FastestTime
};

public class BotModePreferences : UserPreferences<BotModePreferences>
{
	public PlayerBotMode mode { get; set; }

	public bool IsInBotMode(PlayerBotMode checkingMode)
	{
		if (!Game.IsEditor)
		{
			return false;
		}

		return checkingMode == mode;
	}
}
