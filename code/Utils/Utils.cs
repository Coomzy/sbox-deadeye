
using Sandbox.Citizen;

public static class Utils
{
	public static bool isEditTime => Game.IsEditor && Game.ActiveScene == null;
}
