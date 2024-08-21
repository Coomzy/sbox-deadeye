
using Editor;
using Sandbox.Citizen;
using System.Collections.Generic;
using System.ComponentModel;
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
	[Group("Room"), Description("X: Angle Y: Distance W: IsBadGuy (if not 0)"), KeyProperty] public List<Vector4> targetsRaw { get; set; } = new List<Vector4>();
	[JsonIgnore, Hide] public List<GeneratedTarget> targets => GetTargets(targetsRaw);

	public static List<GeneratedTarget> GetTargets(List<Vector4> rawTargets)
	{
		List<GeneratedTarget> tempTargets = new List<GeneratedTarget>();
		foreach (var target in rawTargets)
		{
			var generatedTarget = new GeneratedTarget();
			generatedTarget.isBadGuy = target.w == 0 ? false : true;
			

			var rotation = Rotation.FromYaw(target.x);
			var direction = rotation.Forward.Normal * target.y;
			var localPos = direction;
			//var localPos = new Vector3(target.x, target.y, target.z);
			generatedTarget.localPos = localPos;

			tempTargets.Add(generatedTarget);
		}
		return tempTargets;
	}
}