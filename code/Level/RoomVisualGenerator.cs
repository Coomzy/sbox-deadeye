
using Microsoft.VisualBasic;

public class RoomVisualGenerator : Component
{
	[Group("Setup"), Property] public Room roomReference { get; set; }

	[Group("Setup - Children"), Property] public GameObject floor { get ; set; }

	[Group("Setup - Children"), Property] public GameObject northWall { get ; set; }
	[Group("Setup - Children"), Property] public GameObject northBalcony { get ; set; }
	[Group("Setup - Children"), Property] public GameObject northSteps { get ; set; }

	[Group("Setup - Children"), Property] public GameObject eastWall { get; set; }
	[Group("Setup - Children"), Property] public GameObject eastBalcony { get; set; }
	[Group("Setup - Children"), Property] public GameObject eastSteps { get; set; }

	[Group("Setup - Children"), Property] public GameObject southWall { get; set; }
	[Group("Setup - Children"), Property] public GameObject southBalcony { get; set; }
	[Group("Setup - Children"), Property] public GameObject southSteps { get; set; }

	[Group("Setup - Children"), Property] public GameObject westWall { get; set; }
	[Group("Setup - Children"), Property] public GameObject westBalcony { get; set; }
	[Group("Setup - Children"), Property] public GameObject westSteps { get; set; }

	[Group("Config"), Property] public RoomType roomType { get; set; } = RoomType.Inside;

	[Group("Config - Walls"), Property] public WallType northWallType { get; set; } = WallType.Wall;
	[Group("Config - Walls"), Property] public WallType eastWallType { get; set; } = WallType.Door;
	[Group("Config - Walls"), Property] public WallType southWallType { get; set; } = WallType.Wall;
	[Group("Config - Walls"), Property] public WallType westWallType { get; set; } = WallType.Door;

	[Group("Config - Balcony"), Property] public bool hasNorthBalcony { get; set; } = false;
	[Group("Config - Balcony"), Property] public bool hasEastBalcony { get; set; } = false;
	[Group("Config - Balcony"), Property] public bool hasSouthBalcony { get; set; } = false;
	[Group("Config - Balcony"), Property] public bool hasWestBalcony { get; set; } = false;

	[Group("Random"), Button("Random By Config")]
	public void RandomByConfig()
	{
		SetPrefab(floor, RoomSettings.instance.GetRandomFloor(roomType));
		SetPrefab(northWall, northWallType);
		SetPrefab(eastWall, eastWallType);
		SetPrefab(southWall, southWallType);
		SetPrefab(westWall, westWallType);

		SetPrefab(northBalcony, hasNorthBalcony ? RoomSettings.instance.GetRandomBalcony(roomType) : null);
		SetPrefab(northSteps, hasNorthBalcony ? RoomSettings.instance.GetRandomSteps(roomType) : null);

		SetPrefab(eastBalcony, hasEastBalcony ? RoomSettings.instance.GetRandomBalcony(roomType) : null);
		SetPrefab(eastSteps, hasEastBalcony ? RoomSettings.instance.GetRandomSteps(roomType) : null);

		SetPrefab(southBalcony, hasSouthBalcony ? RoomSettings.instance.GetRandomBalcony(roomType) : null);
		SetPrefab(southSteps, hasSouthBalcony ? RoomSettings.instance.GetRandomSteps(roomType) : null);

		SetPrefab(westBalcony, hasWestBalcony ? RoomSettings.instance.GetRandomBalcony(roomType) : null);
		SetPrefab(westSteps, hasWestBalcony ? RoomSettings.instance.GetRandomSteps(roomType) : null);
	}

	[Group("Random - Floor"), Button("Floor")] public void rf() => SetPrefab(floor, RoomSettings.instance.GetRandomFloor(roomType));

	[Group("Random - North"), Button("Door")] public void rnd() => SetPrefab(northWall, WallType.Door);
	[Group("Random - North"), Button("Wall")] public void rnw() => SetPrefab(northWall, WallType.Wall);
	[Group("Random - North"), Button("Wall Half")] public void rnwh() => SetPrefab(northWall, WallType.WallHalf);
	[Group("Random - North"), Button("Window")] public void rnwd() => SetPrefab(northWall, WallType.Window);


	[Group("Random - East"), Button("Door")] public void rsd() => SetPrefab(eastWall, WallType.Door);
	[Group("Random - East"), Button("Wall")] public void rsw() => SetPrefab(eastWall, WallType.Wall);
	[Group("Random - East"), Button("Wall Half")] public void rswh() => SetPrefab(eastWall, WallType.WallHalf);
	[Group("Random - East"), Button("Window")] public void rswd() => SetPrefab(eastWall, WallType.Window);

	[Group("Random - EaSouthst"), Button("Door")] public void red() => SetPrefab(southWall, WallType.Door);
	[Group("Random - South"), Button("Wall")] public void rew() => SetPrefab(southWall, WallType.Wall);
	[Group("Random - South"), Button("Wall Half")] public void rewh() => SetPrefab(southWall, WallType.WallHalf);
	[Group("Random - South"), Button("Window")] public void rewd() => SetPrefab(southWall, WallType.Window);

	[Group("Random - West"), Button("Door")] public void rwd() => SetPrefab(westWall, WallType.Door);
	[Group("Random - West"), Button("Wall")] public void rww() => SetPrefab(westWall, WallType.Wall);
	[Group("Random - West"), Button("Wall Half")] public void rwwh() => SetPrefab(westWall, WallType.WallHalf);
	[Group("Random - West"), Button("Window")] public void rwwd() => SetPrefab(westWall, WallType.Window);


	public void SetPrefab(GameObject prefabInst, WallType wallType)
	{
		SetPrefab(prefabInst, RoomSettings.instance.GetRandomWall(roomType, wallType));
	}

	public void SetPrefab(GameObject prefabInst, List<PrefabFile> prefabFiles)
	{
		//Game.ActiveScene = GameObject.Scene;
		var prefabFile = prefabFiles != null ? prefabFiles.Random() : null;
		string prefabFilePath = prefabFile != null ? prefabFile.ResourcePath : "";
		prefabInst.Enabled = string.IsNullOrEmpty(prefabFilePath) ? false : true;
		prefabInst.SetPrefabSource(prefabFilePath);

		if (string.IsNullOrEmpty(prefabFilePath))
		{
			prefabInst.BreakFromPrefab();
		}
		else
		{
			prefabInst.UpdateFromPrefab();
		}
	}

	public override void Reset()
	{
		base.Reset();

		roomReference = GameObject.Components.GetInAncestorsOrSelf<Room>();

		floor = GameObject.Children.Find((go) => go.Name == "floor");

		northWall = GameObject.Children.Find((go) => go.Name == "north wall");
		northBalcony = GameObject.Children.Find((go) => go.Name == "north balcony");
		northSteps = GameObject.Children.Find((go) => go.Name == "north steps");

		eastWall = GameObject.Children.Find((go) => go.Name == "east wall");
		eastBalcony = GameObject.Children.Find((go) => go.Name == "east balcony");
		eastSteps = GameObject.Children.Find((go) => go.Name == "east steps");

		southWall = GameObject.Children.Find((go) => go.Name == "south wall");
		southBalcony = GameObject.Children.Find((go) => go.Name == "south balcony");
		southSteps = GameObject.Children.Find((go) => go.Name == "south steps");

		westWall = GameObject.Children.Find((go) => go.Name == "west wall");
		westBalcony = GameObject.Children.Find((go) => go.Name == "west balcony");
		westSteps = GameObject.Children.Find((go) => go.Name == "west steps");
	}
}
