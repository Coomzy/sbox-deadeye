using Sandbox.Citizen;
using System.Text.Json.Serialization;
using static Sandbox.PhysicsContact;

public class RoomManager : Component, IRestartable, IShutdown
{
	public static RoomManager instance;

	[Group("Generation"), Order(25), Property, InlineEditor] public List<GeneratedRoom> generatedRooms { get; set; } = new List<GeneratedRoom>();

	[Group("Setup"), Property] public List<Room> rooms { get; set; }

	[Group("Runtime"), Property, JsonIgnore] public int roomIndex { get; private set; }
	[Group("Runtime"), Property, JsonIgnore] public Room currentRoom => rooms.ContainsIndex(roomIndex) ? rooms[roomIndex] : null;
	[Group("Runtime"), Property, JsonIgnore] public bool isFinalRoom => roomIndex == rooms.Count - 1;

	protected override void OnAwake()
	{
		instance = this;

		base.OnAwake();

		roomIndex = 0;
	}


	[Button("UpdateRoomDataForPerf")]
	void UpdateRoomDataForPerf()
	{
		//var r = GameObject.Components.GetAll<Room>();
		//foreach (var room in r)
		//{
		//	RoomVisualGenerator rvg = room.Components.Get<RoomVisualGenerator>(FindMode.EverythingInDescendants);
		//	if (rvg != null)
		//	{
		//		room.roomVisualGenerator = rvg;
		//	}
		//}

		foreach (var room in rooms)
		{
			room.GetLights();
		}
	}

	//protected override void OnStart()
	//{
	//	base.OnStart();
	//
	//	TestPerf();
	//}
	//
	//void TestPerf()
	//{
	//	//for (int room = 0; room < rooms.Count; room++)
	//	//{
	//	//	for (int target = 0; target < rooms[room].targets.Count; target++)
	//	//	{
	//	//		Target tgt = rooms[room].targets[target];
	//	//		//tgt.bodyRenderer.Enabled = false;
	//	//
	//	//		//List<CitizenClothingInst> insts = rooms[room].targets[target].citizenVisuals.GetAllClothingInsts();
	//	//		List<SkinnedModelRenderer> list = tgt.Components.GetAll<SkinnedModelRenderer>(FindMode.EverythingInDescendants).ToList();
	//	//		foreach (var item in list)
	//	//		{
	//	//			item.Enabled = false;
	//	//			//item.UseAnimGraph = false;
	//	//		}
	//	//		//
	//	//		//tgt.bodyPhysics.Enabled = false;
	//	//		//
	//	//		//tgt.bodyRenderer.UseAnimGraph = false;
	//	//
	//	//		//tgt.highlightOutline.Enabled = false;
	//	//	}
	//	//
	//	//	//if (rooms[room].roomVisualGenerator != null)
	//	//	//{
	//	//	//	rooms[room].roomVisualGenerator.GameObject.Enabled = false;
	//	//	//}
	//	//}
	//
	//	Benchmark();
	//}
	//
	//async void Benchmark()
	//{
	//	TimeUntil t = 5.0f;
	//
	//	int frameCount = 0;
	//	float timeTotal = 0.0f;
	//	while (!t)
	//	{
	//		timeTotal += Time.Delta;
	//		frameCount++;
	//		await Task.Frame();
	//	}
	//
	//	float avg = timeTotal / (float)frameCount;
	//	float avgMS = avg * 1000.0f;
	//	Log.Info($"AVG FRAME TIME: {avgMS:F0}ms");
	//}

	public void PreRestart()
	{
		roomIndex = 0;
	}

	public void PostRestart()
	{

	}

	public void PreShutdown()
	{
		this.Enabled = false;
	}

	public void PostShutdown()
	{

	}

	public void NextRoom()
	{
		roomIndex++;
		DirtyLiveRooms();
	}

	void DirtyLiveRooms()
	{
		if (rooms == null)
		{
			return;
		}

		int cap = rooms.Count;
		int minActiveRoom = System.Math.Clamp(roomIndex - 2, 0, cap);
		int maxActiveRoom = System.Math.Clamp(roomIndex + 1, 0, cap);

		//Log.Info($"roomIndex: {roomIndex}    min:{minActiveRoom}    max:{maxActiveRoom}");

		for (int i = 0; i < rooms.Count; i++)
		{
			if (rooms[i] == null)
			{
				continue;
			}

			bool active = (i >= minActiveRoom) && (i <= maxActiveRoom);
			rooms[i].SetRoomObjectsActive(active);

			//Log.Info($"[{i}]: active: {active}");
		}
	}

	[Button("Generate Rooms")]
	void GenerateRooms()
	{
		if (GameObject.Transform.Position == Vector3.Zero)
		{
			GameObject.Transform.Position = Vector3.Forward;	
		}

		var children = new List<GameObject>(GameObject.Children);
		foreach (var child in children)
		{
			child.Destroy();
		}

		rooms.Clear();

		Vector3 relativePos = Vector3.Zero;
		for (int i = 0; i < generatedRooms.Count; i++)
		{
			var generatedRoom = generatedRooms[i];

			relativePos += generatedRoom.relativePos;

			// Room
			var roomGO = Scene.CreateObject();
			roomGO.Name = $"Room - {i}";
			roomGO.SetParent(GameObject);
			roomGO.Transform.LocalPosition = relativePos;

			var room = roomGO.Components.Create<Room>();
			room.generatedTargetsRaw = generatedRoom.targetsRaw;

			// Spline
			var splineGO = Scene.CreateObject();
			splineGO.Name = $"Spline";
			splineGO.SetParent(roomGO);
			splineGO.Transform.LocalPosition = new Vector3(0, 0, 5);

			var spline = splineGO.Components.Create<Spline>();

			var splineStart = Scene.CreateObject();
			splineStart.Name = "Point";
			splineStart.SetParent(splineGO);
			splineStart.Transform.LocalPosition = -generatedRoom.relativePos;

			var splineCurve = Scene.CreateObject();
			splineCurve.Name = "Curve";
			splineCurve.SetParent(splineGO);
			splineCurve.Transform.LocalPosition = Vector3.Lerp(-generatedRoom.relativePos, Vector3.Zero, 0.5f);

			var splineEnd = Scene.CreateObject();
			splineEnd.Name = "Point";
			splineEnd.SetParent(splineGO);
			splineEnd.Transform.LocalPosition = Vector3.Zero;

			// Targets
			var targetsGO = Scene.CreateObject();
			targetsGO.Name = "Targets";
			targetsGO.SetParent(roomGO);
			targetsGO.Transform.LocalPosition = new Vector3(0,0,5);

			// First room is empty by default
			if (i != 0)
			{
				// Room Visual Generator
				var roomVisualGeneratorGO = Scene.CreateObject();
				roomVisualGeneratorGO.Name = $"Room Visual Generator";
				roomVisualGeneratorGO.SetParent(roomGO);
				roomVisualGeneratorGO.Transform.LocalPosition = Vector3.Zero;

				roomVisualGeneratorGO.SetPrefabSource(RoomSettings.instance.roomVisualGenerator.ResourcePath);

				//Game.ActiveScene = GameObject.Scene;
				if (Game.ActiveScene != null)
				{
					roomVisualGeneratorGO.UpdateFromPrefab();
					roomVisualGeneratorGO.BreakFromPrefab();
				}
				//Game.ActiveScene = null;

				var roomVisualGenerator = splineGO.Components.Get<RoomVisualGenerator>();
				if (roomVisualGenerator != null)
				{
					roomVisualGenerator.RandomByConfig();
				}
			}

			room.walkToPath = spline;
			room.targetsHolder = targetsGO;

			rooms.Add(room);
			room.Generate();
		}

		for (int i = 1; i < rooms.Count; i++)
		{
			var room = rooms[i];
			var roomVisualGenerator = room.Components.GetInDescendantsOrSelf<RoomVisualGenerator>();

			if (roomVisualGenerator == null)
			{
				Log.Error($"room '{room}' is missing RoomVisualGenerator");
				continue;
			}

			var prevRoom = rooms[i - 1];
			var prevRoomDir = DirectionFromPoints(room.Transform.Position, prevRoom.Transform.Position);

			var nextRoom = (i < rooms.Count - 1) ? rooms[i + 1] : null;
			var nextRoomDir = (nextRoom != null) ? DirectionFromPoints(room.Transform.Position, nextRoom.Transform.Position) : Direction.None;

			roomVisualGenerator.northWallType = (prevRoomDir == Direction.North || nextRoomDir == Direction.North) ? WallType.Door : WallType.Wall;
			roomVisualGenerator.eastWallType = (prevRoomDir == Direction.East || nextRoomDir == Direction.East) ? WallType.Door : WallType.Wall;
			roomVisualGenerator.southWallType = (prevRoomDir == Direction.South || nextRoomDir == Direction.South) ? WallType.Door : WallType.Wall;
			roomVisualGenerator.westWallType = (prevRoomDir == Direction.West || nextRoomDir == Direction.West) ? WallType.Door : WallType.Wall;

			roomVisualGenerator.hasNorthBalcony = (i == 1 && prevRoomDir == Direction.North);
			roomVisualGenerator.hasEastBalcony = (i == 1 && prevRoomDir == Direction.East);
			roomVisualGenerator.hasSouthBalcony = (i == 1 && prevRoomDir == Direction.South);
			roomVisualGenerator.hasWestBalcony = (i == 1 && prevRoomDir == Direction.West);

			roomVisualGenerator.RandomByConfig();
		}
	}

	Direction DirectionFromPoints(Vector3 from, Vector3 to)
	{
		var directionToNextRoom = Vector3.Direction(from, to);
		float northDotProd = Vector3.Dot(Vector3.Forward, directionToNextRoom.Normal);
		float eastDotProd = Vector3.Dot(Vector3.Right, directionToNextRoom.Normal);

		bool upDownIsBetter = System.Math.Abs(northDotProd) > System.Math.Abs(eastDotProd);

		if (upDownIsBetter)
		{
			return (northDotProd < 0.0f) ? Direction.South : Direction.North;
		}

		return (eastDotProd < 0.0f) ? Direction.West : Direction.East;
	}

	[Group("Times"), Button("Get Simulated Time")]
	void GetSimulatedTime()
	{
		var levelData = Scene.GetSceneLevelData();
		float simulatedTime = levelData.fastestTime;
		float simulatedReactTime = GameSettings.instance.simulatedReactTime;

		foreach (var room in rooms)
		{
			foreach (var target in room.targets)
			{
				simulatedTime += simulatedReactTime;
			}
		}

		levelData.simulatedTime = simulatedTime;
		Log.Info($"simulatedTime: {simulatedTime}");
	}

	//[Button("Apply All Citizen Visuals")]
	void ApplyAllCitizenVisuals()
	{
		var all = Scene.GetAllComponents<CitizenVisuals>();
		foreach(var inst in all)
		{ 
			//inst.Apply(true);
		}
	}

	[Group("Randomize"), Button("Randomize All Citizen Height And Duck")]
	void ApplyRandomHeights()
	{
		var all = Scene.GetAllComponents<CitizenVisuals>();
		foreach (var inst in all)
		{
			inst.RandomHeight();
			inst.RandomDuckHeight();
		}
	}

	[Group("Randomize"), Button("Randomize Citizen Visuals")]
	void RandomizeCitizenVisuals()
	{
		foreach (var room in rooms)
		{
			room.RandomizeCitizenVisuals();
		}
	}

	[Group("Getters"), Button("Get Rooms")]
	void GetRooms()
	{
		rooms = GameObject.Components.GetAll<Room>().ToList();

		foreach (var room in rooms)
		{
			room.GetTargets();
		}
	}

	public override void Reset()
	{
		base.Reset();

		GetRooms();
	}

	protected override void OnDestroy()
	{
		if (instance == this)
		{
			instance = null;
		}

		base.OnDestroy();
	}
}
