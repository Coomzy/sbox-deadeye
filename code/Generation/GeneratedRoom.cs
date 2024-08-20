
using Editor;
using Sandbox.Citizen;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using static Sandbox.PhysicsContact;

public class GeneratedTarget
{
	[KeyProperty] public bool isBadGuy { get; set; } = true;
	[KeyProperty] public Vector3 localPos { get; set; } = new Vector3(0, 0, 0);
}

public class GeneratedRoom
{
	[Group("Room"), KeyProperty] public Vector3 relativePos { get; set; } = new Vector3(0, 0, 0);
	[Group("Room"), KeyProperty] public List<Vector4> targetsRaw { get; set; } = new List<Vector4>();
	[JsonIgnore, Hide] public List<GeneratedTarget> targets => GetTargets(targetsRaw);

	public static List<GeneratedTarget> GetTargets(List<Vector4> rawTargets)
	{
		List<GeneratedTarget> tempTargets = new List<GeneratedTarget>();
		foreach (var target in rawTargets)
		{
			var generatedTarget = new GeneratedTarget();
			generatedTarget.isBadGuy = target.w == 0 ? false : true;
			generatedTarget.localPos = new Vector3(target.x, target.y, target.z);
			tempTargets.Add(generatedTarget);
		}
		return tempTargets;
	}
}