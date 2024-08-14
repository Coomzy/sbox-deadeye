
public class GizmoTest : Component
{
	protected override void DrawGizmos()
	{
		base.DrawGizmos();

		if (Gizmo.IsSelected)
		{
			Gizmo.Draw.Color = Color.Yellow;
		}

		if (Gizmo.IsChildSelected)
		{
			Gizmo.Draw.Color = Color.Red;
		}

		Gizmo.Draw.LineSphere(Vector3.Zero, 5.0f);
	}	
}
