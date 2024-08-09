
using Sandbox;
using Sandbox.Citizen;
using Sandbox.UI;
using static Sandbox.Gizmo;
using static Sandbox.PhysicsContact;
using static Sandbox.VertexLayout;

public enum PlayerState
{
	Idle,
	Walking,
	Deciding,
	Executing,
	Dead,
	KilledTooManyCivs,
	Won,
};

public class ExampleData
{
	[KeyProperty] public bool test { get; set; } = true;
	[KeyProperty] public bool test2 { get; set; } = true;
	[KeyProperty] public bool test3 { get; set; } = true;
	[KeyProperty] public string testString { get; set; } = "true";
	[KeyProperty] public float testFloat { get; set; } = 1.0f;
	[Property] public bool test4 { get; set; } = true;
}

public class Player : Component
{
	public static Player instance;

	[Property] public ExampleData exampleData { get; set; }

	[Group("Setup"), Property] public SkinnedModelRenderer bodyRenderer { get; private set; }
	[Group("Setup"), Property] public ModelPhysics bodyPhysics { get; private set; }
	[Group("Setup"), Property] public CitizenAnimationHelper thirdPersonAnimationHelper { get; private set; }
	[Group("Setup"), Property] public Dictionary<string, string> propertyDict { get; private set; }
	[Group("Setup"), KeyProperty] public Dictionary<string, string> keyPropertyDict { get; private set; }

	[Group("Runtime"), Property] public PlayerState state { get; private set; } = PlayerState.Idle;
	[Group("Runtime"), Property] public Target target { get; private set; }
	[Group("Runtime"), Property] public List<Target> targets { get; private set; } = new List<Target>();
	[Group("Runtime"), Property] public Weapon weapon { get; private set; }
	[Group("Runtime"), Property] public int civiliansKilled { get; private set; }

	public TimeSince timeSinceStartedDecisionMaking { get; private set; }

	protected override void OnAwake()
	{
		instance = this;

		base.OnAwake();

		LoadClothing();

		Log.Info($"Before: GamePreferences.instance.useOneHandedMode = {GamePreferences.instance.useOneHandedMode}");

		GamePreferences.instance.useOneHandedMode = true;
		//GamePreferences.instance.Save();

		Log.Info($"After: GamePreferences.instance.useOneHandedMode = {GamePreferences.instance.useOneHandedMode}");

		thirdPersonAnimationHelper.MoveStyle = CitizenAnimationHelper.MoveStyles.Walk;
		thirdPersonAnimationHelper.HoldType = CitizenAnimationHelper.HoldTypes.Pistol;
		thirdPersonAnimationHelper.Handedness = GamePreferences.instance.useOneHandedMode ? CitizenAnimationHelper.Hand.Right : CitizenAnimationHelper.Hand.Both;
	}

	protected override void OnStart()
	{
		base.OnStart();

		GoToNextRoom();
	}

	void LoadClothing()
	{
		var avatarJson = Connection.Local.GetUserData("avatar");
		var clothing = new ClothingContainer();
		clothing.Deserialize(avatarJson);
		clothing.Apply(bodyRenderer);
	}

	void GoToNextRoom()
	{
		RoomManager.instance.roomIndex++;
		SetState(PlayerState.Walking);
	}

	void WalkingStart()
	{
		
	}

	void WalkingUpdate()
	{
		if (RoomManager.instance.currentRoom == null)
		{
			return;
		}

		var currentPos = Transform.Position;
		var targetPos = RoomManager.instance.currentRoom.targetPos;
		var newPos = MoveTowards(Transform.Position, targetPos, PlayerSettings.instance.walkSpeed * Time.Delta);
		Transform.Position = newPos;

		var moveDelta = Vector3.Direction(currentPos, targetPos);
		
		Transform.Rotation = Rotation.Slerp(Transform.Rotation, Rotation.From(moveDelta.EulerAngles), PlayerSettings.instance.faceMovementSpeed * Time.Delta);

		thirdPersonAnimationHelper.WithWishVelocity(moveDelta * 100.0f);
		thirdPersonAnimationHelper.WithVelocity(moveDelta * 100.0f);

		if (Vector3.DistanceBetween(Transform.Position, targetPos) < 0.01f)
		{
			SetState(PlayerState.Deciding);
		}
	}

	public static Vector3 MoveTowards(Vector3 current, Vector3 target, float maxDistanceDelta)
	{
		Vector3 direction = target - current;

		float distance = direction.Length;

		if (distance <= maxDistanceDelta)
		{

			return target;
		}
		else
		{

			Vector3 scaledDirection = direction.Normal * maxDistanceDelta;

			Vector3 newPosition = current + scaledDirection;

			return newPosition;
		}
	}

	void DecidingStart()
	{
		timeSinceStartedDecisionMaking = 0;
		targets.Clear();

		thirdPersonAnimationHelper.WithWishVelocity(Vector3.Zero);
		thirdPersonAnimationHelper.WithVelocity(Vector3.Zero);

		RoomManager.instance.currentRoom.Activate();
		NextTarget();
	}

	void NextTarget()
	{
		if (RoomManager.instance.currentRoom.currentTarget != null)
		{
			RoomManager.instance.currentRoom.currentTarget.Deselect();
		}
		RoomManager.instance.currentRoom.targetIndex++;

		if (!PlayerSettings.instance.firstPersonMode)
		{
			var directionToTarget = Vector3.Direction(Transform.Position, RoomManager.instance.currentRoom.currentTarget.Transform.Position);
			directionToTarget.z = 0;
			GameObject.Transform.Rotation = directionToTarget.Normal.EulerAngles.ToRotation();
		}

		RoomManager.instance.currentRoom.currentTarget.Select();
	}

	void DecidingUpdate()
	{
		if (PlayerSettings.instance.firstPersonMode)
		{
			var directionToTarget = Vector3.Direction(Transform.Position, RoomManager.instance.currentRoom.currentTarget.Transform.Position);
			directionToTarget.z = 0;
			var desiredRotation = directionToTarget.Normal.EulerAngles.ToRotation();
			GameObject.Transform.Rotation = Rotation.Slerp(GameObject.Transform.Rotation, desiredRotation, 10.0f * Time.Delta);
		}

		if (timeSinceStartedDecisionMaking >= RoomManager.instance.currentRoom.reactTime)
		{
			bool anyBadGuys = false;
			foreach (var target in RoomManager.instance.currentRoom.targets)
			{
				if (target.isBadTarget)
				{
					anyBadGuys = true;
					break;
				}
			}

			if (anyBadGuys)
			{
				RoomManager.instance.currentRoom.currentTarget.Deselect();
				SetState(PlayerState.Dead);
			}
			else
			{
				RoomManager.instance.currentRoom.currentTarget.Deselect();
				RoomManager.instance.roomIndex++;
				SetState(PlayerState.Walking);
			}

			return;
		}

		bool madeChoice = false;
		if (ShootKeyIsDown())
		{
			madeChoice = true;
			targets.Add(RoomManager.instance.currentRoom.currentTarget);
		}
		if (SpareKeyIsDown())
		{
			madeChoice = true;
		}

		if (madeChoice)
		{
			if (RoomManager.instance.currentRoom.isFinalTarget)
			{
				RoomManager.instance.currentRoom.currentTarget.Deselect();
				SetState(PlayerState.Executing);
			}
			else
			{
				NextTarget();
			}
		}
	}

	bool ShootKeyIsDown()
	{
		if (Input.Pressed("attack1"))
			return true;

		return false;
	}

	bool SpareKeyIsDown()
	{
		if (Input.Pressed("attack2"))
			return true;

		return false;
	}

	void ExecutingStart()
	{
		ExecuteCommands();
	}

	async void ExecuteCommands()
	{
		foreach (var target in targets)
		{
			await GameTask.DelaySeconds(PlayerSettings.instance.delayPerExecute);

			var directionToTarget = Vector3.Direction(Transform.Position, target.Transform.Position);
			directionToTarget.z = 0;
			GameObject.Transform.Rotation = directionToTarget.Normal.EulerAngles.ToRotation();

			if (target.isBadTarget)
			{
				Sandbox.Services.Stats.Increment("targets-eliminated", 1);
			}
			else
			{
				civiliansKilled++;
				Sandbox.Services.Stats.Increment("civilians-killed", 1);
			}

			target.Die();
		}

		await GameTask.DelaySeconds(PlayerSettings.instance.delayAfterExecute);

		bool anyTargetsLeft = false;
		foreach (var target in RoomManager.instance.currentRoom.targets)
		{
			if (!target.isBadTarget)
				continue;

			if (target.isDead)
				continue;

			anyTargetsLeft = true;
		}

		if (anyTargetsLeft)
		{
			SetState(PlayerState.Dead);
		}
		// TODO: Make this based on a level setting or at least a game setting
		else if (civiliansKilled > 3)
		{
			SetState(PlayerState.KilledTooManyCivs);
		}
		else if (RoomManager.instance.isFinalRoom)
		{
			SetState(PlayerState.Won);
		}
		else
		{
			RoomManager.instance.roomIndex++;
			SetState(PlayerState.Walking);
		}
	}

	async void DeadStart()
	{
		Sandbox.Services.Stats.Increment("died", 1);

		DeathAnimate();

		while (!bodyPhysics.Enabled)
		{
			await Task.Frame();
		}

		await Task.DelaySeconds(1.5f);

		UIManager.instance.Died();
	}

	async void DeathAnimate()
	{
		var currentTarget = RoomManager.instance.currentRoom.currentTarget;

		var hitBoxes = new List<HitboxSet.Box>();
		foreach (var hitbox in bodyRenderer.Model.HitboxSet.All)
		{
			//HB_pelvis
			//HB_spine_0
			//HB_spine_1
			//HB_spine_2
			//HB_neck_0
			//HB_head
			//HB_clavicle_R
			//HB_arm_upper_R
			//HB_arm_lower_R
			//HB_hand_R
			//HB_clavicle_L
			//HB_arm_upper_L
			//HB_arm_lower_L
			//HB_hand_L
			//HB_leg_upper_R
			//HB_leg_lower_R
			//HB_ankle_R
			//HB_leg_upper_L
			//HB_leg_lower_L
			//HB_ankle_L
			if (hitbox.Name == "HB_head" ||
				hitbox.Name == "HB_neck_0" ||
				hitbox.Name == "HB_arm_upper_R" ||
				hitbox.Name == "HB_arm_upper_L" ||
				hitbox.Name == "HB_arm_lower_R" ||
				hitbox.Name == "HB_arm_lower_L" ||
				hitbox.Name == "HB_neck_0" ||
				hitbox.Name == "HB_spine_2") //||
				//hitbox.Name == "HB_clavicle_R" ||
				//hitbox.Name == "HB_clavicle_L")
			{
				continue;
			}
			if (hitbox.Name == "HB_leg_upper_R" ||
				hitbox.Name == "HB_leg_upper_L" ||
				hitbox.Name == "HB_leg_lower_R" ||
				hitbox.Name == "HB_leg_lower_L" ||
				hitbox.Name == "HB_ankle_R" ||
				hitbox.Name == "HB_ankle_L" ||
				hitbox.Name == "HB_hand_R" ||
				hitbox.Name == "HB_hand_L" ||
				//hitbox.Name == "HB_arm_lower_R" ||
				//hitbox.Name == "HB_arm_lower_L" ||
				hitbox.Name == "HB_pelvis" ||
				hitbox.Name == "HB_spine_0" ||
				hitbox.Name == "HB_spine_1")
			{
				continue;
			}
			hitBoxes.Add(hitbox);
			Log.Info($"hitbox = {hitbox.Name}");
		}

		thirdPersonAnimationHelper.HoldType = CitizenAnimationHelper.HoldTypes.None;

		TimeSince shootTime = 0;
		Vector3 force = Vector3.Zero;
		Vector3 hitPosition = Vector3.Zero;
		while (true)
		{
			//var damageInfo = new DamageInfo(100.0f, currentTarget.GameObject, currentTarget.citizenVisuals?.weaponGameObject);
			var randomIndex = System.Random.Shared.Next(hitBoxes.Count);
			Log.Info($"random hitbox = {hitBoxes[randomIndex].Name}");
			var boneIndex = hitBoxes[randomIndex].Bone.Index;			
			//Gizmo.Draw.LineSphere(hitBoxes[randomIndex].Bone.LocalTransform.PointToWorld(hitBoxes[randomIndex].RandomPointInside), 1.0f);
			var damageScale = 10.0f;
			force = new Vector3(-Transform.Rotation.Forward * 25.0f);
			hitPosition = hitBoxes[randomIndex].RandomPointInside;
			ProceduralHitReaction(boneIndex, damageScale, force);
			//thirdPersonAnimationHelper.ProceduralHitReaction(damageInfo, 10, force);

			if (shootTime < 1.5f)
			{
				await GameTask.DelaySeconds(0.3f);
			}
			else
			{
				break;
			}
		}

		foreach (var target in RoomManager.instance.currentRoom.targets)
		{
			if (!target.isBadTarget)
			{
				continue;
			}

			if (target?.citizenVisuals?.weapon == null)
			{
				continue;
			}

			target.citizenVisuals.weapon.Shoot();
		}

		bodyPhysics.Enabled = true;
		bodyRenderer.UseAnimGraph = false;

		bodyRenderer.GameObject.Tags.Set("ragdoll", true);
		bodyRenderer.GameObject.SetParent(null);

		foreach (var body in bodyPhysics.PhysicsGroup.Bodies)
		{
			body.ApplyImpulseAt(hitPosition, force);
		}

		if (weapon != null)
		{
			weapon.Drop();
		}
	}

	void ProceduralHitReaction(int boneIndex = 0, float damageScale = 1.0f, Vector3 force = default)
	{
		var tx = thirdPersonAnimationHelper.Target.GetBoneObject(boneIndex);

		var localToBone = tx.Transform.Local.Position;
		if (localToBone == Vector3.Zero) localToBone = Vector3.One;

		thirdPersonAnimationHelper.Target.Set("hit", true);
		thirdPersonAnimationHelper.Target.Set("hit_bone", boneIndex);
		thirdPersonAnimationHelper.Target.Set("hit_offset", localToBone);
		thirdPersonAnimationHelper.Target.Set("hit_direction", force.Normal);
		thirdPersonAnimationHelper.Target.Set("hit_strength", (force.Length / 1000.0f) * damageScale);
	}

	async void WonStart()
	{
		Sandbox.Services.Stats.Increment("won", 1);

		await Task.DelaySeconds(1.5f);

		UIManager.instance.Won();
	}

	async void KilledTooManyCivsStart()
	{
		Sandbox.Services.Stats.Increment("failure-too-many-civs-killed", 1);

		await Task.DelaySeconds(1.5f);

		UIManager.instance.FailedTooManyCivsKilled();
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();

		CheckForExitLevelInput();
		CheckForReloadLevelInput();
		StateMachineUpdate();
	}

	void CheckForExitLevelInput()
	{
		if (!Input.EscapePressed)
			return;
		Input.EscapePressed = false;

		Game.ActiveScene.LoadFromFile("scenes/menu.scene");
	}

	void CheckForReloadLevelInput()
	{
		if (!Input.Pressed("reload"))
			return;

		Game.ActiveScene.Load(Game.ActiveScene.Source);
	}

	void StateMachineUpdate()
	{
		switch (state)
		{
			case PlayerState.Walking:
				WalkingUpdate();
				break;
			case PlayerState.Deciding:
				DecidingUpdate();
				break;
		}
	}

	void SetState(PlayerState newState)
	{
		state = newState;

		switch (state)
		{
			case PlayerState.Walking:
				WalkingStart();
				break;
			case PlayerState.Deciding:
				DecidingStart();
				break;
			case PlayerState.Executing:
				ExecutingStart();
				break;
			case PlayerState.Dead:
				DeadStart();
				break;
			case PlayerState.KilledTooManyCivs:
				KilledTooManyCivsStart();
				break;
			case PlayerState.Won:
				WonStart();
				break;
		}
	}
}
