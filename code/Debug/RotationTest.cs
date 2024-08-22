
using System;

public class RotationTest : Component
{
	[Property] public Vector3 randomizedRotation { get; set; }

	[Property, ReadOnly] public Vector3 eulerAngles => Transform.World.Rotation.Angles().AsVector3();
	[Property, ReadOnly] public Vector3 forward => Transform.World.Forward.Normal;

	[Button("Random Rotaiton")]
	void RandomRotaiton()
	{
		float randomRange = 3.0f;
		float randomX = (Game.Random.NextSingle() * randomRange) - (randomRange / 2.0f);
		float randomY = (Game.Random.NextSingle() * randomRange) - (randomRange / 2.0f);
		float randomZ = (Game.Random.NextSingle() * randomRange) - (randomRange / 2.0f);

		float pitchDegrees = (float)(randomX * (180.0 / System.Math.PI));
		float yawDegrees = (float)(randomY * (180.0 / System.Math.PI));
		float rollDegrees = (float)(randomZ * (180.0 / System.Math.PI));

		//randomizedRotation = new Vector3(randomX, randomY, randomZ);
		//var angles = Transform.World.Rotation.Angles() + (new Angles(randomX, randomY, randomZ) / 360.0f);
		var angles = Transform.World.Rotation.Angles() + (new Angles(pitchDegrees, yawDegrees, rollDegrees) / 360.0f);
		randomizedRotation = new Vector3(randomX, randomY, randomZ);
		randomizedRotation = angles.AsVector3();
		randomizedRotation = new Vector3(1.0f, 0.1f, 0.1f).Normal;
		//var rotation = Rotation.From(randomizedRotation.EulerAngles);

		//Transform.Local.PointToLocal();

		randomizedRotation = Utils.GetRandomizedDirection(Transform.World.Forward, 10.0f);
	}

	protected override void DrawGizmos()
	{
		base.DrawGizmos();

		Gizmo.Transform = Transform.World;

		Gizmo.Draw.Color = Color.White;
		Gizmo.Draw.Line(Transform.Position, Transform.Position + Transform.World.Forward.Normal * 100.0f);

		Gizmo.Draw.Color = Color.Yellow;
		Gizmo.Draw.Line(Transform.Position, Transform.Position + randomizedRotation.Normal * 100.0f);
	}	
}
