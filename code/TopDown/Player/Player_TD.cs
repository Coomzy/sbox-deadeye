
using Sandbox;
using Sandbox.Audio;
using Sandbox.Citizen;
using Sandbox.UI;
using System.Diagnostics;
using static Sandbox.Gizmo;
using static Sandbox.PhysicsContact;
using static Sandbox.VertexLayout;

public enum PlayerState_TD
{
	Idle,
	Walking,
	Deciding,
	Executing,
	Dead,
	KilledTooManyCivs,
	Won,
};


public class Player_TD : Component
{
	public static Player_TD instance;

	[Group("Setup"), Property] public SkinnedModelRenderer bodyRenderer { get; private set; }
	[Group("Setup"), Property] public ModelPhysics bodyPhysics { get; private set; }
	[Group("Setup"), Property] public CitizenAnimationHelper thirdPersonAnimationHelper { get; private set; }
	[Group("Setup"), Property] public GameObject weaponHolder { get; set; }

	[Group("Config"), Property] public WeaponType weaponType { get; set; } = WeaponType.Pistol;

	[Group("Runtime"), Property, ReadOnly] public PlayerState_TD state { get; private set; } = PlayerState_TD.Idle;
	[Group("Runtime"), Property, ReadOnly] public Target target { get; private set; }
	[Group("Runtime"), Property] public List<Target> targets { get; private set; } = new List<Target>();

	[Group("Runtime"), Property] public GameObject weaponGameObject { get; private set; }
	[Group("Runtime"), Property] public Weapon weapon { get; private set; }

	public RealTimeSince timeSinceStartedWalking2 { get; private set; }
	public TimeSince timeSinceStartedWalking { get; private set; }
	public TimeSince timeSinceStartedDecisionMaking { get; private set; }

	protected override void OnAwake()
	{
		instance = this;

		base.OnAwake();

		Mouse.Visible = true;

		LoadClothing();

		thirdPersonAnimationHelper.MoveStyle = CitizenAnimationHelper.MoveStyles.Walk;
		thirdPersonAnimationHelper.HoldType = CitizenAnimationHelper.HoldTypes.Pistol;
		thirdPersonAnimationHelper.Handedness = CitizenAnimationHelper.Hand.Right;
	}

	protected override void OnStart()
	{
		base.OnStart();

		Apply_Weapon();
		GoToNextRoom();
	}

	void Apply_Weapon()
	{
		if (weaponGameObject != null)
		{
			weaponGameObject.Destroy();
		}

		var weaponType = WeaponType.Pistol;
		var weaponPrefab = GameSettings.instance.GetWeaponPrefab(WeaponType.Pistol);
		if (weaponPrefab == null)
		{
			Log.Error($"Player_TD::GetWeapon() weaponType = {weaponType} failed to get Prefab!");
			return;
		}
		weaponGameObject = GameObject.Scene.CreateObject(true);
		weaponGameObject.Name = weaponPrefab.ResourceName;
		weaponGameObject.SetParent(weaponHolder, false);
		weaponGameObject.Transform.LocalPosition = Vector3.Zero;
		weaponGameObject.Transform.LocalRotation = Rotation.Identity;
		weaponGameObject.SetPrefabSource(weaponPrefab.ResourcePath);

		// HACK: This errors in edit time. https://github.com/Facepunch/sbox-issues/issues/6169
		if (!Scene.IsEditor)
		{
			weaponGameObject.UpdateFromPrefab();
		}

		weapon = weaponGameObject.Components.Get<Weapon>();
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
		SetState(PlayerState_TD.Walking);
	}

	void WalkingStart()
	{
		thirdPersonAnimationHelper.Handedness = CitizenAnimationHelper.Hand.Right;
		timeSinceStartedWalking = 0;
	}

	void WalkingUpdate()
	{
		if (RoomManager.instance?.currentRoom?.GameObject?.Transform == null)
		{
			return;
		}

		var currentPos = Transform.Position;
		// BUG: WHY DOES THIS ERROR ON SCENE RELOAD!?
		//var targetPos = RoomManager.instance.currentRoom.walkToPos;
		//var newPos = Utils.MoveTowards(Transform.Position, targetPos, PlayerSettings.instance.walkSpeed * Time.Delta);
		//Transform.Position = newPos;

		var walkToPath = RoomManager.instance.currentRoom.walkToPath;
		float totalSplineLength = walkToPath.GetTotalSplineTime();
		float timeLeftAlongSpline = totalSplineLength - timeSinceStartedWalking;
		Vector3 endOfSplinePoint = walkToPath.endOfSplinePoint;
		var moveToPos = walkToPath.GetPointAlongSplineAtTime(timeSinceStartedWalking);

		// Yikes, this is really shit but no time to fix :/
		if (timeLeftAlongSpline < 0.015f)
		{
			moveToPos = Utils.MoveTowards(Transform.Position, endOfSplinePoint, PlayerSettings.instance.walkSpeed * Time.Delta);
		}

		Transform.Position = moveToPos;

		var moveDelta = Vector3.Direction(currentPos, moveToPos);
		
		Transform.Rotation = Rotation.Slerp(Transform.Rotation, Rotation.From(moveDelta.EulerAngles), PlayerSettings.instance.faceMovementSpeed * Time.Delta);

		thirdPersonAnimationHelper.WithWishVelocity(moveDelta * 100.0f);
		thirdPersonAnimationHelper.WithVelocity(moveDelta * 100.0f);

		if (Vector3.DistanceBetween(Transform.Position, endOfSplinePoint) < 0.01f)
		{
			SetState(PlayerState_TD.Deciding);
		}
	}

	void DecidingStart()
	{
		thirdPersonAnimationHelper.IsWeaponLowered = true;
		thirdPersonAnimationHelper.Handedness = CitizenAnimationHelper.Hand.Both;

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

		var directionToTarget = Vector3.Direction(Transform.Position, RoomManager.instance.currentRoom.currentTarget.Transform.Position);
		directionToTarget.z = 0;
		GameObject.Transform.Rotation = directionToTarget.Normal.EulerAngles.ToRotation();

		RoomManager.instance.currentRoom.currentTarget.Select();
	}

	void DecidingUpdate()
	{
		if (timeSinceStartedDecisionMaking >= RoomManager.instance.currentRoom.reactTime)
		{
			if (BotModePreferences.instance.IsInBotMode(PlayerBotMode.SlowestTime))
			{
				if (RoomManager.instance.currentRoom.currentTarget.isBadTarget)
				{
					targets.Add(RoomManager.instance.currentRoom.currentTarget);
				};
				RoomManager.instance.currentRoom.currentTarget.Deselect();
				SetState(PlayerState_TD.Executing);
				return;
			}

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
				SetState(PlayerState_TD.Dead);
			}
			else
			{
				RoomManager.instance.currentRoom.currentTarget.Deselect();
				RoomManager.instance.roomIndex++;
				SetState(PlayerState_TD.Walking);
			}

			return;
		}

		bool madeChoice = false;
		if (ShootKeyIsDown())
		{
			madeChoice = true;
			targets.Add(RoomManager.instance.currentRoom.currentTarget);
			var soundHandle = Sound.Play("target.confirmed");
			soundHandle.TargetMixer = Mixer.FindMixerByName("Game");
		}
		if (SpareKeyIsDown())
		{
			madeChoice = true;
			var soundHandle = Sound.Play("target.passed");
			soundHandle.TargetMixer = Mixer.FindMixerByName("Game");
		}

		if (BotModePreferences.instance.IsInBotMode(PlayerBotMode.FastestTime))
		{
			madeChoice = true;
			if (RoomManager.instance.currentRoom.currentTarget.isBadTarget)
			{
				targets.Add(RoomManager.instance.currentRoom.currentTarget);
			}
		}

		if (BotModePreferences.instance.IsInBotMode(PlayerBotMode.SlowestTime))
		{
			if (!RoomManager.instance.currentRoom.isFinalTarget)
			{
				madeChoice = true;
				if (RoomManager.instance.currentRoom.currentTarget.isBadTarget)
				{
					targets.Add(RoomManager.instance.currentRoom.currentTarget);
				}
			}
		}

		if (madeChoice)
		{
			if (RoomManager.instance.currentRoom.isFinalTarget)
			{
				RoomManager.instance.currentRoom.currentTarget.Deselect();
				SetState(PlayerState_TD.Executing);
			}
			else
			{
				NextTarget();
			}
		}
	}

	bool ShootKeyIsDown()
	{
		bool pressed = Input.Pressed("Shoot");
		if (Input.Pressed("Shoot_Alt"))
		{
			pressed = true;
		}
		if (!pressed)
			return false;

		return true;
	}

	bool SpareKeyIsDown()
	{
		bool pressed = Input.Pressed("Spare");
		if (Input.Pressed("Spare_Alt"))
		{
			pressed = true;
		}
		if (!pressed)
			return false;

		return true;
	}

	void ExecutingStart()
	{
		ExecuteCommands();
	}

	async void ExecuteCommands()
	{
		await GameTask.DelaySeconds(PlayerSettings.instance.delayBeforeExecute);

		foreach (var target in targets)
		{			

			var directionToTarget = Vector3.Direction(Transform.Position, target.Transform.Position);
			directionToTarget.z = 0;
			GameObject.Transform.Rotation = directionToTarget.Normal.EulerAngles.ToRotation();
			//thirdPersonAnimationHelper.MoveRotationSpeed = 10000.0f;

			await GameTask.DelaySeconds(PlayerSettings.instance.delayPerExecute);

			//Game.ActiveScene.TimeScale = 0.0f;

			if (target.isBadTarget)
			{
				GameStats.Increment(GameStats.TARGETS_ELIMINATED);
			}
			else
			{
				GamePlayManager.instance.civiliansKilled++;
				GameStats.Increment(GameStats.CIVILIANS_KILLED);
			}

			target.Die();
			weapon.Shoot(target.Transform.Position);
		}

		await GameTask.DelaySeconds(PlayerSettings.instance.delayAfterExecute);

		Game.ActiveScene.TimeScale = 1.0f;

		bool anyTargetsLeft = false;
		foreach (var target in RoomManager.instance.currentRoom.targets)
		{
			if (!target.isBadTarget)
				continue;

			if (target.isDead)
				continue;

			anyTargetsLeft = true;
		}

		int civilianKillLimit = 3;

		if (LevelData.active != null)
		{
			civilianKillLimit = LevelData.active.allowedCivilianCasualties;
		}
		else
		{
			Log.Error($"Failed to get LevelData.active!");
		}

		if (anyTargetsLeft)
		{
			SetState(PlayerState_TD.Dead);
		}
		else if (GamePlayManager.instance.civiliansKilled >= civilianKillLimit)
		{
			SetState(PlayerState_TD.KilledTooManyCivs);
		}
		else if (RoomManager.instance.isFinalRoom)
		{
			SetState(PlayerState_TD.Won);
		}
		else
		{
			RoomManager.instance.roomIndex++;
			SetState(PlayerState_TD.Walking);
		}
	}

	async void DeadStart()
	{
		GamePlayManager.instance.FailLevel(FailReason.Died);

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

		List<Target> shooters = new List<Target>();

		foreach (var target in RoomManager.instance.currentRoom.targets)
		{
			if (!target.isBadTarget)
				continue;

			if (target.isDead)
				continue;

			if (target?.citizenVisuals?.weapon == null)
				continue;

			shooters.Add(target);
		}

		int shooterIndex = 0;
		shooters.Shuffle();

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
			//Log.Info($"hitbox = {hitbox.Name}");
		}

		thirdPersonAnimationHelper.HoldType = CitizenAnimationHelper.HoldTypes.None;

		TimeSince shootTime = 0;
		Vector3 force = Vector3.Zero;
		Vector3 hitPosition = Vector3.Zero;
		while (true)
		{
			var shooter = shooters[shooterIndex];

			//var damageInfo = new DamageInfo(100.0f, currentTarget.GameObject, currentTarget.citizenVisuals?.weaponGameObject);
			var randomIndex = System.Random.Shared.Next(hitBoxes.Count);
			//Log.Info($"random hitbox = {hitBoxes[randomIndex].Name}");
			var boneIndex = hitBoxes[randomIndex].Bone.Index;			
			//Gizmo.Draw.LineSphere(hitBoxes[randomIndex].Bone.LocalTransform.PointToWorld(hitBoxes[randomIndex].RandomPointInside), 1.0f);
			var damageScale = 10.0f;
			//force = new Vector3(-Transform.Rotation.Forward * 25.0f);
			force = new Vector3(shooter.citizenVisuals.weapon.Transform.Rotation.Forward * 25.0f);
			hitPosition = hitBoxes[randomIndex].RandomPointInside;
			ProceduralHitReaction(boneIndex, damageScale, force);
			//thirdPersonAnimationHelper.ProceduralHitReaction(damageInfo, 10, force);

			var hitBoneGO = thirdPersonAnimationHelper.Target.GetBoneObject(boneIndex);
			shooter.citizenVisuals.weapon.Shoot(hitBoneGO.Transform.Position);

			shooterIndex++;
			if (shooterIndex >= shooters.Count)
			{
				shooterIndex = 0;
				shooters.Shuffle();
			}

			if (shootTime < 1.5f)
			{
				await GameTask.DelaySeconds(0.3f);
			}
			else
			{
				break;
			}
		}

		/*foreach (var target in RoomManager.instance.currentRoom.targets)
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
		}*/

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

	async void KilledTooManyCivsStart()
	{
		thirdPersonAnimationHelper.HoldType = CitizenAnimationHelper.HoldTypes.None;

		GamePlayManager.instance.FailLevel(FailReason.KilledTooManyCivs);

		await Task.DelaySeconds(0.15f);

		weapon.Drop();

		await Task.DelaySeconds(1.5f);

		UIManager.instance.FailedTooManyCivsKilled();
	}

	async void WonStart()
	{
		GamePlayManager.instance.WonLevel();

		await Task.DelaySeconds(1.5f);

		UIManager.instance.Won();
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
		bool pressed = Input.Pressed("Quit");
		if (Input.EscapePressed)
		{
			Input.EscapePressed = false;
			pressed = true;
		}

		if (!pressed)
			return;

		Game.ActiveScene.LoadFromFile("scenes/menu.scene");
	}

	void CheckForReloadLevelInput()
	{
		bool pressed = Input.Pressed("Restart");
		if (Input.Pressed("Restart_Alt"))
		{
			pressed = true;
		}
		if (!pressed)
			return;

		Game.ActiveScene.Load(Game.ActiveScene.Source);
	}

	void StateMachineUpdate()
	{
		switch (state)
		{
			case PlayerState_TD.Walking:
				WalkingUpdate();
				break;
			case PlayerState_TD.Deciding:
				DecidingUpdate();
				break;
		}
	}

	void SetState(PlayerState_TD newState)
	{
		state = newState;

		switch (state)
		{
			case PlayerState_TD.Walking:
				WalkingStart();
				break;
			case PlayerState_TD.Deciding:
				DecidingStart();
				break;
			case PlayerState_TD.Executing:
				ExecutingStart();
				break;
			case PlayerState_TD.Dead:
				DeadStart();
				break;
			case PlayerState_TD.KilledTooManyCivs:
				KilledTooManyCivsStart();
				break;
			case PlayerState_TD.Won:
				WonStart();
				break;
		}
	}
}
