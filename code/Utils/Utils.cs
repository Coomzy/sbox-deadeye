
using Sandbox.Citizen;

public static partial class Utils
{
	public static bool isEditTime => Game.IsEditor && Game.ActiveScene == null;

	public static GameObject BreakPrefab(this GameObject inst)
	{
		inst.BreakFromPrefab();
		return inst;
	}

	public static T Random<T>(this List<T> list)
	{
		var index = System.Random.Shared.Next(list.Count);
		return list[index];
	}

	public static void Shuffle<T>(this IList<T> list)
	{
		int n = list.Count;
		while (n > 1)
		{
			n--;
			int k = System.Random.Shared.Next(n + 1);
			T value = list[k];
			list[k] = list[n];
			list[n] = value;
		}
	}

	public static Vector3 MoveTowards(Vector3 current, Vector3 target, float maxDistanceDelta)
	{
		Vector3 direction = target - current;

		float distance = direction.Length;

		if (distance <= maxDistanceDelta)
		{
			return target;
		}
		else
		{

			Vector3 scaledDirection = direction.Normal * maxDistanceDelta;

			Vector3 newPosition = current + scaledDirection;

			return newPosition;
		}
	}

	public static float MoveTowards(float current, float target, float maxDelta)
	{
		if (System.Math.Abs(target - current) <= maxDelta)
		{
			return target;
		}

		return current + System.Math.Sign(target - current) * maxDelta;
	}
}
