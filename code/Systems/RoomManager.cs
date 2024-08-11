
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

	[Button("Apply All Citizen Visuals")]
	void ApplyAllCitizenVisuals()
	{
		var all = Scene.GetAllComponents<CitizenVisuals>();
		foreach(var inst in all)
		{ 
			inst.Apply(true);
		}
	}

	[Button("Randomize Citizen Visuals")]
	void RandomizeCitizenVisuals()
	{
		foreach (var room in rooms)
		{
			var roomName = room.GameObject.Name;
			if (roomName == "Room - 1" ||
				roomName == "Room - 4" ||
				roomName == "Room - 15")
				continue;

			foreach(var target in room.targets)
			{
				target.citizenVisuals.RandomClothing();
			}
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
