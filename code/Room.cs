
using Sandbox.Citizen;

public class Room : Component
{
	[Group("Setup"), Property] public GameObject hiddenBlocker { get; set; }
	[Group("Setup"), Property] public GameObject playerPosOverride { get; set; }
	[Group("Setup"), Property] public List<Target> targets { get; set; }

	[Group("Config"), Property] public float reactTime { get; set; } = 5.0f;

	[Group("Runtime"), Property] public int targetIndex { get; set; } = -1;
	public Target currentTarget => targets.ContainsIndex(targetIndex) ? targets[targetIndex] : null;
	public bool isFinalTarget => targetIndex == targets.Count - 1;

	public Vector3 targetPos
	{
		get
		{
			if (playerPosOverride != null)
			{
				return playerPosOverride.Transform.Position;
			}
			return GameObject.Transform.Position;
		}
	}

	public void Activate()
	{
		targetIndex = -1;
		hiddenBlocker.Enabled = false;

		foreach (var target in targets)
		{
			target.Activate();
		}
	}

	[Button("Get Targets")]
	public void GetTargets()
	{
		targets = GameObject.Components.GetAll<Target>().ToList();
	}

	public override void Reset()
	{
		base.Reset();

		GetTargets();
	}
}
