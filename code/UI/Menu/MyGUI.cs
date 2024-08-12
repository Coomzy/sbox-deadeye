using Sandbox;
using Sandbox.UI;

public class MyGUI : RootPanel
{
	public override void DrawBackground(ref RenderState state)
	{
		// Obviously, don't declare this here every frame. But you get the idea.
		System.Span<Vertex> vertices = new Vertex[] {
			new Vertex( new Vector3( 100, 100 ), new Vector4( 0, 0, 0, 0 ), new Color32(255, 0, 0 ) ),
			new Vertex( new Vector3( 300, 100 ), new Vector4( 0, 0, 0, 0 ), new Color32(0, 255, 0 ) ),
			new Vertex( new Vector3( 300, 300 ), new Vector4( 0, 0, 0, 0 ), new Color32(0, 0, 255 ) ),
		};

		var attribs = new RenderAttributes();
		attribs.Set("Texture", Texture.White);

		Graphics.Draw(vertices, 3, Material.UI.Basic, attribs, Graphics.PrimitiveType.Triangles);
	}
}