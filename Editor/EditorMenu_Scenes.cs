using System;
using System.Reflection;

public static class EditorMenu_Scenes
{
	// HACK: I don't know how to clear the menu and don't know a better event to use, this will do for now
	public static bool hasInited = false;

	[Event("refresh")]
	public static void RegisterTopDownLevels()
	{
		if (hasInited)
			return;
		hasInited = true;

		//Log.Info("Editor Event 'refresh' called, doing RegisterTopDownLevels()");
		FieldInfo targetsFieldInfo = typeof(MenuAttribute).GetField("Targets", BindingFlags.NonPublic | BindingFlags.Static);
		if (targetsFieldInfo == null)
		{
			return;
		}

		var targetsValue = targetsFieldInfo.GetValue(null) as Dictionary<string, MenuBar>;
		if (targetsValue == null)
		{
			return;
		}

		if (!targetsValue.TryGetValue("Editor", out MenuBar editorMenuBar))
		{
			return;
		}

		if (GameSettings.instance == null)
		{
			return;
		}

		/*var scenePaths = editorMenuBar.GetPathTo("Scenes/");

		if (scenePaths != null && scenePaths.Count > 0)
		{
			editorMenuBar.RemovePath("Scenes/");
		}*/
		//editorMenuBar.RemovePath("Scenes");

		if (GameSettings.instance.menuLevel != null)
		{
			//editorMenuBar.RemovePath("Scenes/Menu");
			editorMenuBar.AddOption("Scenes/Menu", null, () => EditorScene.LoadFromScene(GameSettings.instance.menuLevel.scene));
		}

		if (GameSettings.instance.topDownLevels == null)
		{
			return;
		}

		List<string> addedScenes = new List<string>();
		string optionPrefix = $"Scenes/Levels/";

		foreach (var level in GameSettings.instance.topDownLevels)
		{
			if (level.scene == null)
				continue;

			string optionPath = optionPrefix + level.scene.ResourceName;

			if (addedScenes.Contains(optionPath))
				continue;

			//editorMenuBar.RemovePath(optionPath);
			editorMenuBar.AddOption(optionPath, null, () => EditorScene.LoadFromScene(level.scene));
			addedScenes.Add(optionPath);
		}
	}
}