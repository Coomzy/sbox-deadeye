using Sandbox.Citizen;
using Sandbox.Internal;
using System.IO;
using System;
using Sandbox;

public enum WallType
{
	None,
	Wall,
	Window,
	Door,
}


public enum Direction
{
	None,
	North,
	East,
	South,
	West
}

[GameResource("Room Settings", "rs", "Room Settings")]
public class RoomSettings : GameResourceSingleton<RoomSettings>
{
	[Group("Prefabs"), Property, InlineEditor] public List<PrefabFile> floors { get; set; } = new List<PrefabFile>();
	[Group("Prefabs"), Property, InlineEditor] public List<PrefabFile> doors { get; set; } = new List<PrefabFile>();
	[Group("Prefabs"), Property, InlineEditor] public List<PrefabFile> walls { get; set; } = new List<PrefabFile>();
	[Group("Prefabs"), Property, InlineEditor] public List<PrefabFile> windows { get; set; } = new List<PrefabFile>();
	[Group("Prefabs"), Property, InlineEditor] public List<PrefabFile> balcony { get; set; } = new List<PrefabFile>();
	[Group("Prefabs"), Property, InlineEditor] public List<PrefabFile> steps { get; set; } = new List<PrefabFile>();

	[Group("Prefabs"), Property] public PrefabFile roomVisualGenerator { get; set; }
}
