
using Sandbox.Citizen;
using static Sandbox.PhysicsContact;

public class RoomManager : Component
{
	public static RoomManager instance;

	[Group("Setup"), Property] public List<Room> rooms { get; set; }

	public int roomIndex { get; set; }
	public Room currentRoom => rooms.ContainsIndex(roomIndex) ? rooms[roomIndex] : null;
	public bool isFinalRoom => roomIndex == rooms.Count - 1;

	protected override void OnAwake()
	{
		instance = this;

		base.OnAwake();
	}

	[Button("Get Simulated Time")]
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

	[Button("Randomize All Citizen Height And Duck")]
	void ApplyRandomHeights()
	{
		var all = Scene.GetAllComponents<CitizenVisuals>();
		foreach (var inst in all)
		{
			inst.RandomHeight();
			inst.RandomDuckHeight();
		}
	}

	[Button("Randomize Citizen Visuals")]
	void RandomizeCitizenVisuals()
	{
		foreach (var room in rooms)
		{
			room.RandomizeCitizenVisuals();
		}
	}

	[Button("Get Rooms")]
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
