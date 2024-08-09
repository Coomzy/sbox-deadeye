
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
