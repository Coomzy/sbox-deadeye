
public class Room : Component, IRestartable, IShutdown
{
	[Group("Generation"), Property, InlineEditor] public List<Vector4> generatedTargetsRaw { get; set; } = new List<Vector4>();

	[Group("Setup"), Property] public Spline walkToPath { get; set; }
	[Group("Setup"), Property] public GameObject playerPosOverride { get; set; }
	[Group("Setup"), Property] public GameObject targetsHolder { get; set; }
	[Group("Setup"), Property] public List<Target> targets { get; set; } = new List<Target>();
	[Group("Setup"), Property] public List<Light> lights { get; set; } = new List<Light>();

	[Group("Config"), Property] public float reactTime { get; set; } = 5.0f;
	[Group("Config"), Property] public bool ignoreRandomize { get; set; } = false;

	[Group("Runtime"), Property] public int targetIndex { get; set; } = -1;
	public Target currentTarget => targets.ContainsIndex(targetIndex) ? targets[targetIndex] : null;
	public bool isFinalTarget => targetIndex == targets.Count - 1;

	[Group("Config"), Property] public RoomVisualGenerator roomVisualGenerator { get; set; }

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

	public void PreRestart()
	{
		targetIndex = -1;
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

	public void SetRoomObjectsActive(bool active)
	{
		foreach (var target in targets)
		{
			foreach (var smr in target.allRenderers)
			{
				smr.Enabled = active;
				smr.UseAnimGraph = active;
			}
		}

		foreach (var light in lights)
		{
			light.Enabled = active;
		}
	}

	public void Activate()
	{
		targetIndex = -1;

		foreach (var target in targets)
		{
			target.Activate();
		}
	}

	[Group("Buttons"), Button("Get Lights")]
	public void GetLights()
	{
		lights = GameObject.Components.GetAll<Light>(FindMode.EnabledInSelfAndDescendants).ToList();
	}

	[Group("Generation"), Button("Generation Targets")]
	public void Generate()
	{
		if (targetsHolder == null)
		{
			targetsHolder = Scene.CreateObject();
			targetsHolder.Name = "Targets";
			targetsHolder.SetParent(GameObject);
			targetsHolder.Transform.LocalPosition = Vector3.Zero;
		}

		var children = new List<GameObject>(targetsHolder.Children);
		foreach (var child in children)
		{
			child.Destroy();
		}

		if (targets == null)
		{
			targets = new List<Target>();
		}
		targets.Clear();

		var generatedTargetsConverted = GeneratedRoom.GetTargets(generatedTargetsRaw);
		foreach (var generatedTarget in generatedTargetsConverted)
		{
			var newTarget = GameObject.Scene.CreateObject(true);
			newTarget.Name = $"Target - {(generatedTarget.isBadGuy ? "Enemy" : "Civilian")}";
			newTarget.SetParent(targetsHolder, false);
			newTarget.Transform.LocalPosition = generatedTarget.localPos;
			newTarget.Transform.LocalRotation = Rotation.Identity;
			newTarget.SetPrefabSource(GameSettings.instance.targetPrefab.ResourcePath);

			Game.ActiveScene = GameObject.Scene;
			if (Game.ActiveScene != null)
			{
				newTarget.UpdateFromPrefab();
				newTarget.BreakFromPrefab();
			}
			Game.ActiveScene = null;

			var target = newTarget.Components.Get<Target>();
			if (target != null)
			{
				target.isBadTarget = generatedTarget.isBadGuy;
				target.citizenVisuals.weaponType = generatedTarget.isBadGuy ? WeaponType.Pistol : WeaponType.None;
				target.citizenVisuals.weaponGameObject.Enabled = generatedTarget.isBadGuy ? true : false;
				target.citizenVisuals.RandomClothing();
				target.LookAtPoint(GameObject.Transform.Position);
				targets.Add(target);
			}
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

	/*protected override void DrawGizmos()
	{
		base.DrawGizmos();

		Gizmo.Transform = Game.ActiveScene.Transform.World;

		Vector3 pos = walkToPos;

		foreach (var item in lights)
		{
			Gizmo.Draw.Line(pos, item.Transform.Position);
		}
	}*/
}
