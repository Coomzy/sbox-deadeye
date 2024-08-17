using System;
using System.Reflection;

public static class EditorMenu_Scenes
{
	[Event("refresh")]
	public static void RegisterTopDownLevels()
	{
		Log.Info("Editor Event 'refresh' called, doing RegisterTopDownLevels()");
		Type menuAttributeType = typeof(MenuAttribute);
		FieldInfo targetsFieldInfo = menuAttributeType.GetField("Targets", BindingFlags.NonPublic | BindingFlags.Static);

		if (targetsFieldInfo == null)
		{
			Log.Info("Field 'Targets' not found.");
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

		var scenePaths = editorMenuBar.GetPathTo("Scenes/");

		if (scenePaths != null && scenePaths.Count > 0)
		{
			editorMenuBar.RemovePath("Scenes/");
		}
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