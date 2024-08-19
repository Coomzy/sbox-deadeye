
public class Room : Component
{
	[Group("Generation"), Property, InlineEditor] public List<GeneratedTarget> generatedTargets { get; set; }

	[Group("Setup"), Property] public Spline walkToPath { get; set; }
	[Group("Setup"), Property] public GameObject playerPosOverride { get; set; }
	[Group("Setup"), Property] public List<Target> targets { get; set; }

	[Group("Config"), Property] public float reactTime { get; set; } = 5.0f;
	[Group("Config"), Property] public bool ignoreRandomize { get; set; } = false;

	[Group("Runtime"), Property] public int targetIndex { get; set; } = -1;
	public Target currentTarget => targets.ContainsIndex(targetIndex) ? targets[targetIndex] : null;
	public bool isFinalTarget => targetIndex == targets.Count - 1;

	public Vector3 walkToPos
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

	protected override void OnAwake()
	{
		base.OnAwake();

		targetIndex = -1;
	}

	public void Activate()
	{
		targetIndex = -1;

		foreach (var target in targets)
		{
			target.Activate();
		}
	}

	[Group("Buttons"), Button("Get Targets")]
	public void GetTargets()
	{
		targets = GameObject.Components.GetAll<Target>().ToList();
	}

	[Group("Buttons"), Button("Randomize Citizen Visuals")]
	void ForceRandomizeCitizenVisuals()
	{
		ignoreRandomize = false;
		RandomizeCitizenVisuals();
	}

	protected override void OnUpdate()
	{
		//MakeTargetsLookAtWalkPos();
	}

	[Group("Buttons"), Button("Make Targets Look At WalkPos")]
	void MakeTargetsLookAtWalkPos()
	{
		foreach (var target in targets)
		{
			if (target == null || !target.lookAtPlayer)
				continue;

			target.LookAtPlayerWalkPos();
		}
	}

	public void RandomizeCitizenVisuals()
	{
		if (ignoreRandomize)
		{
			return;
		}

		foreach (var target in targets)
		{
			target.citizenVisuals.RandomClothing();
		}
	}

	public override void Reset()
	{
		base.Reset();

		GetTargets();
	}
}
