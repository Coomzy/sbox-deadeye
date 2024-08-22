
using Sandbox;
using Sandbox.Citizen;

public static partial class Utils
{
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

	public static T Random<T>(this IEnumerable<T> list)
	{
		if (list == null || list.Count() < 1)
			return default(T);

		var index = System.Random.Shared.Next(list.Count());
		return list.ElementAt(index);
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

	public static Vector3 GetRandomizedDirection(Vector3 originalDirection, float maxAngle)
	{
		float pitch = Game.Random.Float(-maxAngle, maxAngle);
		float yaw = Game.Random.Float(-maxAngle, maxAngle);
		float roll = Game.Random.Float(-maxAngle, maxAngle);

		Rotation randomRotation = Rotation.From(pitch, yaw, roll);
		Vector3 randomizedDirection = randomRotation * originalDirection;

		return randomizedDirection.Normal;
	}

	public static float RandomRange(this Vector2 inst)
	{
		return Game.Random.Float(inst.x, inst.y);
	}
}
