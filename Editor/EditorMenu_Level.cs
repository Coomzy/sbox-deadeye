
using Sandbox;
using static Editor.EditorUtility;

public static class Level
{
	[Menu("Editor", "Level/Make All Props Static")]
	public static void MakeAllPropsStatic()
	{
		if (SceneEditorSession.Active?.Scene == null)
		{
			Log.Error("You can't run a scene based tool, without an active scene!");
			return;
		}

		Scene scene = SceneEditorSession.Active.Scene;
		bool foundAnyStaticProps = false;
		using (scene.Push())
		{
			var props = scene.GetAllComponents<Prop>();

			foreach (var prop in props)
			{
				if (prop.IsStatic)
					continue;

				prop.IsStatic = true;
				foundAnyStaticProps = true;

				Log.Warning($"{scene} prop '{prop.GameObject}' was not static. Making static");
			}
		}

		if (!foundAnyStaticProps)
		{
			Log.Info("No non static props were found");
		}
		else
		{
			SceneEditorSession.Active.Save(false);	
		}
	}
}