
public static class EditorMenu_Room
{
	[Menu("Editor", "Room/Get All")]
	public static void OpenMyMenu()
	{
		var roomSettings = RoomSettings.instance;
		roomSettings.floors = ResourceLibrary.GetAll<PrefabFile>("prefabs/environment/floor/").ToList();
		roomSettings.doors = ResourceLibrary.GetAll<PrefabFile>("prefabs/environment/doors/").ToList();
		roomSettings.walls = ResourceLibrary.GetAll<PrefabFile>("prefabs/environment/walls/").ToList();
		roomSettings.windows = ResourceLibrary.GetAll<PrefabFile>("prefabs/environment/windows/").ToList();
		roomSettings.balcony = ResourceLibrary.GetAll<PrefabFile>("prefabs/environment/balcony/").ToList();
		roomSettings.steps = ResourceLibrary.GetAll<PrefabFile>("prefabs/environment/steps/").ToList();

		var sceneAsset = AssetSystem.CreateResource(RoomSettings.fileExtension, RoomSettings.fullFilePath);
		if (sceneAsset != null)
		{
			sceneAsset.SaveToDisk(roomSettings);
		}
		else
		{
			Log.Error($"Failed to save RoomSettings, please do it manually! Also let Cal know it fucked up");
		}
	}
}