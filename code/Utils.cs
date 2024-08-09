
using Sandbox.Citizen;

public static class Utils
{
	public static bool ContainsIndex<T>(this List<T> list, int index)
	{
		if (index < 0)
			return false;

		if (index >= list.Count)
			return false;

		return true;
	}
}
