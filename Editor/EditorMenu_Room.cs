
public static class EditorMenu_Room
{
	[Menu("Editor", "Room/Get All")]
	public static void OpenMyMenu()
	{
		var roomSettings = RoomSettings.instance;
		roomSettings.floors = ResourceLibrary.GetAll<PrefabFile>("prefabs/environment/floor/").ToList();
		roomSettings.doors = ResourceLibrary.GetAll<PrefabFile>("prefabs/environment/doors/").ToList();
		roomSettings.walls = ResourceLibrary.GetAll<PrefabFile>("prefabs/environment/walls/").ToList();
		roomSettings.wallsHalf = ResourceLibrary.GetAll<PrefabFile>("prefabs/environment/walls_half/").ToList();
		roomSettings.windows = ResourceLibrary.GetAll<PrefabFile>("prefabs/environment/windows/").ToList();
		roomSettings.balcony = ResourceLibrary.GetAll<PrefabFile>("prefabs/environment/balcony/").ToList();
		roomSettings.steps = ResourceLibrary.GetAll<PrefabFile>("prefabs/environment/steps/").ToList();

		roomSettings.floorsOutside = ResourceLibrary.GetAll<PrefabFile>("prefabs/environment/outside/floor/").ToList();
		roomSettings.doorsOutside = ResourceLibrary.GetAll<PrefabFile>("prefabs/environment/outside/doors/").ToList();
		roomSettings.wallsOutside = ResourceLibrary.GetAll<PrefabFile>("prefabs/environment/outside/walls/").ToList();
		roomSettings.wallsHalfOutside = ResourceLibrary.GetAll<PrefabFile>("prefabs/environment/outside/walls_half/").ToList();
		roomSettings.windowsOutside = ResourceLibrary.GetAll<PrefabFile>("prefabs/environment/outside/windows/").ToList();
		roomSettings.balconyOutside = ResourceLibrary.GetAll<PrefabFile>("prefabs/environment/outside/balcony/").ToList();
		roomSettings.stepsOutside = ResourceLibrary.GetAll<PrefabFile>("prefabs/environment/outside/steps/").ToList();

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