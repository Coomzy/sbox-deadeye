using System;
using System.Runtime.InteropServices.JavaScript;

public static class GameInputs
{
	[ConCmd("setbinds_to_meta_strat")]
	public static void setbinds_to_meta_strat()
	{
		IGameInstance.Current.SetBind("Shoot", "leftarrow");
		IGameInstance.Current.SetBind("Shoot_Alt", "rightarrow");

		IGameInstance.Current.SetBind("Spare", "z");
		IGameInstance.Current.SetBind("Spare_Alt", "c");

		IGameInstance.Current.SaveBinds();

		Log.Info($"Shoot is bound to '{GetBind("Shoot")}'");
		Log.Info($"Shoot_Alt is bound to '{GetBind("Shoot_Alt")}'");
		Log.Info($"Spare is bound to '{GetBind("Spare")}'");
		Log.Info($"Spare_Alt is bound to '{GetBind("Spare_Alt")}'");
	}

	public static string GetBind(string bind)
	{
		return IGameInstance.Current.GetBind(bind, out bool isDefault, out bool isCommon);
	}
}
