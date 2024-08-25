using Sandbox.Citizen;
using Sandbox.Internal;
using System.IO;
using System;
using Sandbox;

public enum RoomType
{
	Inside,
	Outside,
	None
}

public enum WallType
{
	None,
	Wall,
	Window,
	Door,
	WallHalf,
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
	[Group("Prefabs"), Property] public PrefabFile roomVisualGenerator { get; set; }

	[Group("Prefabs - Inside"), Property, InlineEditor] public List<PrefabFile> floors { get; set; } = new List<PrefabFile>();
	[Group("Prefabs - Inside"), Property, InlineEditor] public List<PrefabFile> doors { get; set; } = new List<PrefabFile>();
	[Group("Prefabs - Inside"), Property, InlineEditor] public List<PrefabFile> walls { get; set; } = new List<PrefabFile>();
	[Group("Prefabs - Inside"), Property, InlineEditor] public List<PrefabFile> wallsHalf { get; set; } = new List<PrefabFile>();
	[Group("Prefabs - Inside"), Property, InlineEditor] public List<PrefabFile> windows { get; set; } = new List<PrefabFile>();
	[Group("Prefabs - Inside"), Property, InlineEditor] public List<PrefabFile> balcony { get; set; } = new List<PrefabFile>();
	[Group("Prefabs - Inside"), Property, InlineEditor] public List<PrefabFile> steps { get; set; } = new List<PrefabFile>();

	[Group("Prefabs - Outside"), Property, InlineEditor] public List<PrefabFile> floorsOutside { get; set; } = new List<PrefabFile>();
	[Group("Prefabs - Outside"), Property, InlineEditor] public List<PrefabFile> doorsOutside { get; set; } = new List<PrefabFile>();
	[Group("Prefabs - Outside"), Property, InlineEditor] public List<PrefabFile> wallsOutside { get; set; } = new List<PrefabFile>();
	[Group("Prefabs - Outside"), Property, InlineEditor] public List<PrefabFile> wallsHalfOutside { get; set; } = new List<PrefabFile>();
	[Group("Prefabs - Outside"), Property, InlineEditor] public List<PrefabFile> windowsOutside { get; set; } = new List<PrefabFile>();
	[Group("Prefabs - Outside"), Property, InlineEditor] public List<PrefabFile> balconyOutside { get; set; } = new List<PrefabFile>();
	[Group("Prefabs - Outside"), Property, InlineEditor] public List<PrefabFile> stepsOutside { get; set; } = new List<PrefabFile>();

	public List<PrefabFile> GetRandomFloor(RoomType roomType)
	{
		if (roomType == RoomType.None)
			return null;

		return (roomType == RoomType.Outside) ? floorsOutside : floors;
	}

	public List<PrefabFile> GetRandomWall(RoomType roomType, WallType wallType)
	{
		if (roomType == RoomType.None || wallType == WallType.None)
			return null;

		switch (wallType)
		{
			case WallType.Door:
				return (roomType == RoomType.Outside) ? doorsOutside : doors;
			case WallType.Wall:
				return (roomType == RoomType.Outside) ? wallsOutside : walls;
			case WallType.WallHalf:
				return (roomType == RoomType.Outside) ? wallsHalfOutside : wallsHalf;
			case WallType.Window:
				return (roomType == RoomType.Outside) ? windowsOutside : windows;
		}

		return null;
	}

	public List<PrefabFile> GetRandomBalcony(RoomType roomType)
	{
		if (roomType == RoomType.None)
			return null;

		return (roomType == RoomType.Outside) ? balconyOutside : balcony;
	}

	public List<PrefabFile> GetRandomSteps(RoomType roomType)
	{
		if (roomType == RoomType.None)
			return null;

		return (roomType == RoomType.Outside) ? stepsOutside : steps;
	}
}
