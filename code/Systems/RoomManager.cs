
using Sandbox.Citizen;
using System.Text.Json.Serialization;
using static Sandbox.PhysicsContact;

public class RoomManager : Component
{
	public static RoomManager instance;

	[Group("Generation"), Order(25), Property, InlineEditor] public List<GeneratedRoom> generatedRooms { get; set; } = new List<GeneratedRoom>();

	[Group("Setup"), Property] public List<Room> rooms { get; set; }

	[Group("Runtime"), Property, JsonIgnore] public int roomIndex { get; set; }
	[Group("Runtime"), Property, JsonIgnore] public Room currentRoom => rooms.ContainsIndex(roomIndex) ? rooms[roomIndex] : null;
	[Group("Runtime"), Property, JsonIgnore] public bool isFinalRoom => roomIndex == rooms.Count - 1;

	protected override void OnAwake()
	{
		instance = this;

		base.OnAwake();

		roomIndex = 0;
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
			splineGO.Transform.LocalPosition = Vector3.Zero;

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
			targetsGO.Transform.LocalPosition = Vector3.Zero;

			room.walkToPath = spline;
			room.targetsHolder = targetsGO;

			rooms.Add(room);
			room.Generate();
		}
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
}
