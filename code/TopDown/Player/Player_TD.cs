
using Sandbox;
using Sandbox.Audio;
using Sandbox.Citizen;
using Sandbox.UI;
using System;
using System.Diagnostics;
using System.Numerics;
using System.Threading;
using static Sandbox.Gizmo;
using static Sandbox.PhysicsContact;
using static Sandbox.VertexLayout;
using static Sandbox.VoxResource;

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


public class Player_TD : Component, IRestartable, IShutdown
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

	public TimeSince timeSinceStartedWalking { get; private set; }
	public RealTimeSince timeSinceStartedDecisionMaking { get; private set; }
	CancellationTokenSource cancellationTokenSource;

	bool queueShoot;
	bool queueSpare;

	Vector3 startPos;
	Rotation startRot;

	protected override void OnAwake()
	{
		instance = this;

		base.OnAwake();

		Mouse.Visible = true;
		startPos = GameObject.Transform.Position;
		startRot = GameObject.Transform.Rotation;
		state = PlayerState_TD.Idle;

		var clothingContainer = CitizenSettings.instance.GetPlayerClothingContainer();
		clothingContainer.Apply(bodyRenderer);

		thirdPersonAnimationHelper.MoveStyle = CitizenAnimationHelper.MoveStyles.Walk;
		thirdPersonAnimationHelper.HoldType = CitizenAnimationHelper.HoldTypes.Pistol;
		thirdPersonAnimationHelper.Handedness = CitizenAnimationHelper.Hand.Right;

		cancellationTokenSource = new CancellationTokenSource();
	}

	protected override void OnStart()
	{
		Apply_Weapon();
	}

	public void PreRestart()
	{
		state = PlayerState_TD.Idle;
		GameObject.Transform.Position = startPos;
		GameObject.Transform.Rotation = startRot;

		target = null;
		targets.Clear();

		bodyPhysics.Enabled = false;
		bodyRenderer.UseAnimGraph = true;

		bodyRenderer.GameObject.Tags.Set("ragdoll", false);
		//bodyRenderer.GameObject.SetParent(null);
		bodyRenderer.Transform.ClearInterpolation();
		bodyRenderer.Transform.LocalPosition = Vector3.Zero;
		bodyRenderer.Transform.LocalRotation = Quaternion.Identity;

		thirdPersonAnimationHelper.MoveStyle = CitizenAnimationHelper.MoveStyles.Walk;
		thirdPersonAnimationHelper.HoldType = CitizenAnimationHelper.HoldTypes.Pistol;
		thirdPersonAnimationHelper.Handedness = CitizenAnimationHelper.Hand.Right;
		thirdPersonAnimationHelper.LookAtEnabled = false;
		thirdPersonAnimationHelper.WithLook(GameObject.Transform.Rotation.Forward);
	}

	public void PostRestart()
	{
		GoToNextRoom();
	}

	public void PreShutdown()
	{
		this.Enabled = false;
	}

	public void PostShutdown()
	{

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

	void GoToNextRoom()
	{
		if (FUCKING_STOP_YOU_CUNT())
			return;

		RoomManager.instance.NextRoom();
		SetState(PlayerState_TD.Walking);
	}

	void IdleUpdate()
	{
		if (GamePlayManager.instance == null)
		{
			return;
		}

		if (!GamePlayManager.instance.hasFinishedCountDown)
		{
			return;
		}

		GoToNextRoom();
	}

	void WalkingStart()
	{
		if (FUCKING_STOP_YOU_CUNT())
			return;

		Game.ActiveScene.TimeScale = 1.0f;
		thirdPersonAnimationHelper.Handedness = CitizenAnimationHelper.Hand.Right;
		timeSinceStartedWalking = 0;

		if (RoomManager.instance?.currentRoom?.walkToPath == null)
		{
			if (RoomManager.instance?.currentRoom != null)
			{
				Log.Error($"{RoomManager.instance?.currentRoom.GameObject.Name} does not have a walkToPath!");
			}
			else
			{
				Log.Error($"RoomManager.instance.currentRoom is null, everything is fucked");
			}
		}
	}

	void WalkingUpdate()
	{
		if (FUCKING_STOP_YOU_CUNT())
			return;

		if (RoomManager.instance == null)
			return;

		if (RoomManager.instance.currentRoom == null)
			return;

		if (RoomManager.instance?.currentRoom?.GameObject == null)
			return;

		if (RoomManager.instance?.currentRoom?.GameObject?.Transform == null)
			return;

		if (RoomManager.instance?.currentRoom?.walkToPath == null)
			return;

		// FUCK YOU SPLINES
		/*var currentPos = Transform.Position;
		var walkToPath = RoomManager.instance.currentRoom.walkToPath;
		float totalSplineLength = walkToPath.GetTotalSplineTime();
		float timeLeftAlongSpline = totalSplineLength - timeSinceStartedWalking;
		Vector3 endOfSplinePoint = walkToPath.endOfSplinePoint;
		var moveToPos = walkToPath.GetPointAlongSplineAtTime(timeSinceStartedWalking);

		// Yikes, this is really shit but no time to fix :/
		if (timeLeftAlongSpline < 0.015f)
		{
			moveToPos = Utils.MoveTowards(Transform.Position, endOfSplinePoint, PlayerSettings.instance.walkSpeed * Time.Delta);
		}*/

		var currentPos = Transform.Position;
		var moveToPoint = RoomManager.instance.currentRoom.walkToPos;
		var nextPos = Utils.MoveTowards(Transform.Position, moveToPoint, PlayerSettings.instance.walkSpeed * Time.Delta);

		Transform.Position = nextPos;

		var moveDelta = Vector3.Direction(currentPos, nextPos);
		
		Transform.Rotation = Rotation.Slerp(Transform.Rotation, Rotation.From(moveDelta.EulerAngles), PlayerSettings.instance.faceMovementSpeed * Time.Delta);

		thirdPersonAnimationHelper.WithWishVelocity(moveDelta * 100.0f);
		thirdPersonAnimationHelper.WithVelocity(moveDelta * 100.0f);

		if (Vector3.DistanceBetween(Transform.Position, moveToPoint) < 0.01f)
		{
			if (RoomManager.instance.currentRoom.targets == null || RoomManager.instance.currentRoom.targets.Count < 1)
			{
				RoomManager.instance.NextRoom();
				SetState(PlayerState_TD.Walking);
			}
			else
			{
				SetState(PlayerState_TD.Deciding);
			}
		}
	}

	void DecidingStart()
	{
		if (FUCKING_STOP_YOU_CUNT())
			return;

		thirdPersonAnimationHelper.IsWeaponLowered = true;
		thirdPersonAnimationHelper.Handedness = CitizenAnimationHelper.Hand.Right;
		thirdPersonAnimationHelper.WithWishVelocity(Vector3.Zero);
		thirdPersonAnimationHelper.WithVelocity(Vector3.Zero);

		timeSinceStartedDecisionMaking = 0;
		targets.Clear();

		//Game.ActiveScene.TimeScale = 0.25f;

		RoomManager.instance.currentRoom.Activate();
		NextTarget();

		if (RoomManager.instance.currentRoom.targets?.Count > 0)
		{
			var soundHandle = Sound.Play("encounterstart");
			soundHandle.TargetMixer = Mixer.FindMixerByName("UI");
		}
	}

	void NextTarget()
	{
		if (FUCKING_STOP_YOU_CUNT())
			return;

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
		if (FUCKING_STOP_YOU_CUNT())
			return;

		GamePlayManager.instance.decidingTime += Time.Delta;

		if (RoomManager.instance?.currentRoom == null)
		{
			return;
		}

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
				RoomManager.instance.NextRoom();
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
		if (FUCKING_STOP_YOU_CUNT())
			return false;

		bool input1 = Input.Pressed("Shoot");
		bool input2 = Input.Pressed("Shoot_Alt");

		if (input1 && input2)
		{
			queueShoot = true;
		}

		if (input1 || input2)
		{
			return true;
		}

		if (queueShoot)
		{
			queueShoot = false;
			return true;
		}

		return false;
	}


	bool SpareKeyIsDown()
	{
		if (FUCKING_STOP_YOU_CUNT())
			return false;

		bool input1 = Input.Pressed("Spare");
		bool input2 = Input.Pressed("Spare_Alt");

		if (input1 && input2)
		{
			queueSpare = true;
		}

		if (input1 || input2)
		{
			return true;
		}

		if (queueSpare)
		{
			queueSpare = false;
			return true;
		}

		return false;
	}

	enum ExecuteSubState
	{
		InitalDelay,
		Shooting,
		PostDelay
	}

	ExecuteSubState executeSubState = ExecuteSubState.InitalDelay;
	float executeError = 0.0f;

	void ExecutingStart()
	{
		if (FUCKING_STOP_YOU_CUNT())
			return;

		//Game.ActiveScene.TimeScale = 1.0f;
		executeSubState = ExecuteSubState.InitalDelay;
		killedAnyCivs2 = false;
		initDelay = 0;
		executeError = MathX.Clamp(MusicManager.timeTillBeat, 0.0001f, MusicManager.TIME_BASE - 0.0001f);
		initDelay = MusicManager.TIME_BASE - executeError;
		//ExecuteCommands();
	}

	void ExecutingUpdate()
	{
		if (FUCKING_STOP_YOU_CUNT())
			return;

		switch (executeSubState)
		{
			case ExecuteSubState.InitalDelay:
				Executing_InitalDelay();
				return;
			case ExecuteSubState.Shooting:
				Executing_Shooting();
				return;
			case ExecuteSubState.PostDelay:
				Executing_PostDelay();
				return;
		}
	}

	TimeUntil initDelay;
	void Executing_InitalDelay()
	{
		if (FUCKING_STOP_YOU_CUNT())
			return;

		if (!initDelay)
		{
			return;
		}

		executeSubState = ExecuteSubState.Shooting;
		killedAnyCivs2 = false;
		isWaitingForShoot = false;
		shootingTarget = null;
		shootingTargetIndex = 0;
		TimeUntil timeUntilShot = MusicManager.TIME_BASE;
	}

	bool killedAnyCivs2 = false;
	bool isWaitingForShoot = false;
	TimeUntil timeUntilShoot;
	int shootingTargetIndex = -1;
	Target shootingTarget = null;
	void Executing_Shooting()
	{
		if (FUCKING_STOP_YOU_CUNT())
			return;

		if (targets == null || !targets.ContainsIndex(shootingTargetIndex))
		{
			postDelay = executeError;
			executeSubState = ExecuteSubState.PostDelay;
			return;
		}

		if (!isWaitingForShoot)
		{
			shootingTarget = targets[shootingTargetIndex];
			var directionToTarget = Vector3.Direction(Transform.Position, shootingTarget.Transform.Position);
			directionToTarget.z = 0;
			GameObject.Transform.Rotation = directionToTarget.Normal.EulerAngles.ToRotation();
			isWaitingForShoot = true;
			//shootingTargetIndex = 0;
			timeUntilShoot = MusicManager.TIME_BASE;
			return;
		}

		if (!timeUntilShoot)
		{
			return;
		}

		shootingTarget = targets[shootingTargetIndex];
		shootingTargetIndex++;
		isWaitingForShoot = false;
		timeUntilShoot = MusicManager.TIME_BASE;

		if (shootingTarget.isBadTarget)
		{
			GameStats.Increment(GameStats.TARGETS_ELIMINATED);
		}
		else
		{
			killedAnyCivs2 = true;
			GamePlayManager.instance.civiliansKilled++;
			GameStats.Increment(GameStats.CIVILIANS_KILLED);
			UIManager.instance.CivilianKilled();
		}

		Vector3 force = weapon.Transform.World.Forward;
		force = Utils.GetRandomizedDirection(force, PlayerSettings.instance.shootForceRandomAngle) * PlayerSettings.instance.shootForceRange.RandomRange();

		shootingTarget.Die(force);
		weapon.Shoot(shootingTarget.Transform.Position);

		var bloodDecalGO = Scene.CreateObject();
		bloodDecalGO.SetPrefabSource(GameSettings.instance.bloodDecalPrefab.ResourcePath);
		bloodDecalGO.UpdateFromPrefab();
		bloodDecalGO.Transform.Position = shootingTarget.GetHeadPos();
		Vector3 bloodSplatDir = weapon.Transform.Rotation.Forward;

		float bloodSplatRandomRange = 10.0f;
		bloodSplatDir = Utils.GetRandomizedDirection(bloodSplatDir, bloodSplatRandomRange);
		bloodDecalGO.Transform.Rotation = Rotation.From(bloodSplatDir.EulerAngles);
	}

	TimeUntil postDelay;
	void Executing_PostDelay()
	{
		if (FUCKING_STOP_YOU_CUNT())
			return;

		if (!postDelay)
		{
			return;
		}

		if (killedAnyCivs2 && GamePreferences.instance.restartLevelOnCivKill)
		{
			GamePlayManager.instance.Restart();
			return;
		}

		//Game.ActiveScene.TimeScale = 1.0f;

		bool anyTargetsLeft = false;

		if (RoomManager.instance?.currentRoom?.targets != null)
		{
			foreach (var target in RoomManager.instance.currentRoom.targets)
			{
				if (!target.isBadTarget)
					continue;

				if (target.isDead)
					continue;

				anyTargetsLeft = true;
			}
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
			RoomManager.instance.NextRoom();
			SetState(PlayerState_TD.Walking);
		}
	}

	// Async can get really fucky sometime :/
	async void ExecuteCommands()
	{
		float error = MusicManager.timeTillBeat;

		error = MathX.Clamp(error, 0.0001f, MusicManager.TIME_BASE - 0.0001f);

		//await Task.DelaySeconds(MusicManager.TIME_BASE - error);
		await GameTask.DelaySeconds(MusicManager.TIME_BASE - error, cancellationTokenSource.Token);
		if (cancellationTokenSource != null && cancellationTokenSource.IsCancellationRequested)
		{
			return;
		}
		bool killedAnyCivs = false;

		foreach (var target in targets)
		{
			TimeUntil timeUntilShot = MusicManager.TIME_BASE;
			var directionToTarget = Vector3.Direction(Transform.Position, target.Transform.Position);
			directionToTarget.z = 0;
			GameObject.Transform.Rotation = directionToTarget.Normal.EulerAngles.ToRotation();
			//thirdPersonAnimationHelper.MoveRotationSpeed = 10000.0f;

			//await Task.DelaySeconds(MusicManager.TIME_BASE);
			await GameTask.DelaySeconds(MusicManager.TIME_BASE, cancellationTokenSource.Token);
			/*while (!timeUntilShot)
			{
				await Task.Frame();
			}*/

			if (cancellationTokenSource != null && cancellationTokenSource.IsCancellationRequested)
			{
				return;
			}

			//Game.ActiveScene.TimeScale = 0.0f;

			if (target.isBadTarget)
			{
				GameStats.Increment(GameStats.TARGETS_ELIMINATED);
			}
			else
			{
				killedAnyCivs = true;
				GamePlayManager.instance.civiliansKilled++;
				GameStats.Increment(GameStats.CIVILIANS_KILLED);
				UIManager.instance.CivilianKilled();
			}

			Vector3 force = weapon.Transform.World.Forward;
			force = Utils.GetRandomizedDirection(force, PlayerSettings.instance.shootForceRandomAngle) * PlayerSettings.instance.shootForceRange.RandomRange();

			target.Die(force);
			weapon.Shoot(target.Transform.Position);

			var bloodDecalGO = Scene.CreateObject();
			bloodDecalGO.SetPrefabSource(GameSettings.instance.bloodDecalPrefab.ResourcePath);
			bloodDecalGO.UpdateFromPrefab();
			bloodDecalGO.Transform.Position = target.GetHeadPos();
			Vector3 bloodSplatDir = weapon.Transform.Rotation.Forward;

			float bloodSplatRandomRange = 10.0f;
			bloodSplatDir = Utils.GetRandomizedDirection(bloodSplatDir, bloodSplatRandomRange);
			bloodDecalGO.Transform.Rotation = Rotation.From(bloodSplatDir.EulerAngles);
		}

		//await Task.DelaySeconds(MusicManager.TIME_BASE + error);
		await GameTask.DelaySeconds(MusicManager.TIME_BASE + error, cancellationTokenSource.Token);
		if (cancellationTokenSource != null && cancellationTokenSource.IsCancellationRequested)
		{
			return;
		}

		if (killedAnyCivs && GamePreferences.instance.restartLevelOnCivKill)
		{
			GamePlayManager.instance.Restart();
			return;
		}

		//Game.ActiveScene.TimeScale = 1.0f;

		bool anyTargetsLeft = false;

		if (RoomManager.instance?.currentRoom?.targets != null)
		{
			foreach (var target in RoomManager.instance.currentRoom.targets)
			{
				if (!target.isBadTarget)
					continue;

				if (target.isDead)
					continue;

				anyTargetsLeft = true;
			}
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
			RoomManager.instance.NextRoom();
			SetState(PlayerState_TD.Walking);
		}
	}

	/*async*/ void DeadStart()
	{
		hasShownDiedScreen = false;
		//Game.ActiveScene.TimeScale = 1.0f;
		GamePlayManager.instance.FailLevel(FailReason.Died);
		diedScreenDelay = 1.75f;

		DeathAnimeNoAsyncStart();
		/*DeathAnimate();

		while (!bodyPhysics.Enabled)
		{
			//await Task.Frame();
			await GameTask.Delay(1, cancellationTokenSource.Token);
			if (cancellationTokenSource != null && cancellationTokenSource.IsCancellationRequested)
			{
				return;
			}
		}

		//await Task.DelaySeconds(1.5f);
		await GameTask.DelaySeconds(1.5f, cancellationTokenSource.Token);
		if (cancellationTokenSource != null && cancellationTokenSource.IsCancellationRequested)
		{
			return;
		}

		if (GamePreferences.instance.restartLevelOnFail)
		{
			GamePlayManager.instance.Restart();
			return;
		}

		UIManager.instance.Died();*/
	}


	TimeUntil diedScreenDelay;
	bool hasShownDiedScreen = false;
	void DeadUpdate()
	{
		if (FUCKING_STOP_YOU_CUNT())
			return;

		DeathAnimateNoAsync();

		if (isDeathAnimating)
		{
			return;
		}

		if (!hasShownDiedScreen && diedScreenDelay)
		{
			if (GamePreferences.instance.restartLevelOnFail)
			{
				GamePlayManager.instance.Restart();
				return;
			}

			UIManager.instance.Died();
			hasShownDiedScreen = true;
		}
	}

	List<Target> shooters2 = new List<Target>();
	int shooterIndex2 = 0;
	List<HitboxSet.Box> hitBoxes2 = new List<HitboxSet.Box>();
	bool isDeathAnimating = false;
	void DeathAnimeNoAsyncStart()
	{
		if (FUCKING_STOP_YOU_CUNT())
			return;

		if (shooters2 == null)
		{
			shooters2 = new List<Target>();
		}
		else
		{
			shooters2.Clear();
		}

		shooterIndex2 = 0;
		var currentTarget = RoomManager.instance.currentRoom.currentTarget;

		foreach (var target in RoomManager.instance.currentRoom.targets)
		{
			if (!target.isBadTarget)
				continue;

			if (target.isDead)
				continue;

			if (target?.citizenVisuals?.weapon == null)
				continue;

			shooters2.Add(target);
		}

		shooters2.Shuffle();

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
			hitBoxes2.Add(hitbox);
			//Log.Info($"hitbox = {hitbox.Name}");
		}

		thirdPersonAnimationHelper.HoldType = CitizenAnimationHelper.HoldTypes.None;
		isDeathAnimating = true;
		deathAnimatingTime = 1.5f;
	}

	TimeUntil nextShotDelay;
	TimeUntil deathAnimatingTime;
	void DeathAnimateNoAsync()
	{
		if (FUCKING_STOP_YOU_CUNT())
			return;

		if (!isDeathAnimating)
		{
			return;
		}

		if (shooters2 == null || !shooters2.ContainsIndex(shooterIndex2))
		{
			isDeathAnimating = false;
			DeathAnimeNoAsyncEnd();
			return;
		}

		if (!nextShotDelay)
		{
			return;
		}

		nextShotDelay = 0.3f;
		//isDeathAnimating = false;

		TimeSince shootTime = 0;

		var shooter = shooters2[shooterIndex2];

		if (shooter?.citizenVisuals?.weapon == null)
		{
			shooterIndex2++;
			if (shooterIndex2 >= shooters2.Count)
			{
				shooterIndex2 = 0;
				shooters2.Shuffle();
			}
			return;
		}

		//var damageInfo = new DamageInfo(100.0f, currentTarget.GameObject, currentTarget.citizenVisuals?.weaponGameObject);
		var randomIndex = System.Random.Shared.Next(hitBoxes2.Count);
		//Log.Info($"random hitbox = {hitBoxes[randomIndex].Name}");
		var boneIndex = (hitBoxes2.ContainsIndex(randomIndex) && hitBoxes2[randomIndex]?.Bone != null) ? hitBoxes2[randomIndex].Bone.Index : 3;
		//Gizmo.Draw.LineSphere(hitBoxes[randomIndex].Bone.LocalTransform.PointToWorld(hitBoxes[randomIndex].RandomPointInside), 1.0f);
		var damageScale = 10.0f;
		//force = new Vector3(-Transform.Rotation.Forward * 25.0f);
		force2 = new Vector3(shooter.citizenVisuals.weapon.Transform.Rotation.Forward * 25.0f);
		hitPosition2 = hitBoxes2[randomIndex].RandomPointInside;
		ProceduralHitReaction(boneIndex, damageScale, force2);
		//thirdPersonAnimationHelper.ProceduralHitReaction(damageInfo, 10, force);

		var hitBoneGO = thirdPersonAnimationHelper.Target.GetBoneObject(boneIndex);
		shooter.citizenVisuals.weapon.Shoot(hitBoneGO.Transform.Position);

		shooterIndex2++;
		if (shooterIndex2 >= shooters2.Count)
		{
			shooterIndex2 = 0;
			shooters2.Shuffle();
		}

		if (deathAnimatingTime)
		{
			isDeathAnimating = false;
			DeathAnimeNoAsyncEnd();
			return;
		}
	}

	Vector3 hitPosition2;
	Vector3 force2;
	void DeathAnimeNoAsyncEnd()
	{
		if (FUCKING_STOP_YOU_CUNT())
			return;

		diedScreenDelay = 0.75f;
		bodyPhysics.Enabled = true;
		bodyRenderer.UseAnimGraph = false;

		bodyRenderer.GameObject.Tags.Set("ragdoll", true);
		//bodyRenderer.GameObject.SetParent(null);

		foreach (var body in bodyPhysics.PhysicsGroup.Bodies)
		{
			body.ApplyImpulseAt(hitPosition2, force2);
		}

		if (weapon != null)
		{
			weapon.Drop(force2);
		}
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

			if (shooter?.citizenVisuals?.weapon == null)
			{
				continue;
			}

			//var damageInfo = new DamageInfo(100.0f, currentTarget.GameObject, currentTarget.citizenVisuals?.weaponGameObject);
			var randomIndex = System.Random.Shared.Next(hitBoxes.Count);
			//Log.Info($"random hitbox = {hitBoxes[randomIndex].Name}");
			var boneIndex = (hitBoxes.ContainsIndex(randomIndex) && hitBoxes[randomIndex].Bone != null) ? hitBoxes[randomIndex].Bone.Index : 3;			
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
				//await Task.DelaySeconds(0.3f);
				await GameTask.DelaySeconds(0.3f, cancellationTokenSource.Token);
				if (cancellationTokenSource != null && cancellationTokenSource.IsCancellationRequested)
				{
					return;
				}
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
		//bodyRenderer.GameObject.SetParent(null);

		foreach (var body in bodyPhysics.PhysicsGroup.Bodies)
		{
			body.ApplyImpulseAt(hitPosition, force);
		}

		if (weapon != null)
		{
			weapon.Drop(force);
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

	/*async*/ void KilledTooManyCivsStart()
	{
		if (FUCKING_STOP_YOU_CUNT())
			return;

		//Game.ActiveScene.TimeScale = 1.0f;
		thirdPersonAnimationHelper.HoldType = CitizenAnimationHelper.HoldTypes.None;

		//LowerHead();

		GamePlayManager.instance.FailLevel(FailReason.KilledTooManyCivs);

		hasShownCivsKilledScreen = false;
		hasDroppedWeapon = false;
		lowerHeadTime = 0.25f;
		hasDroppedWeaponDelay = 0.15f;
		civsKilledScreenDelay = 1.5f;

		//await Task.DelaySeconds(0.15f);
		/*await GameTask.DelaySeconds(0.15f, cancellationTokenSource.Token);
		if (cancellationTokenSource != null && cancellationTokenSource.IsCancellationRequested)
		{
			return;
		}

		Vector3 force = Transform.Rotation.Forward;
		weapon.Drop(force);

		//await Task.DelaySeconds(1.5f);
		await GameTask.DelaySeconds(1.5f, cancellationTokenSource.Token);
		if (cancellationTokenSource != null && cancellationTokenSource.IsCancellationRequested)
		{
			return;
		}

		if (GamePreferences.instance.restartLevelOnFail)
		{
			GamePlayManager.instance.Restart();
			return;
		}

		UIManager.instance.FailedTooManyCivsKilled();*/
	}

	TimeUntil civsKilledScreenDelay;
	TimeUntil hasDroppedWeaponDelay;
	TimeUntil lowerHeadTime;
	bool hasShownCivsKilledScreen = false;
	bool hasDroppedWeapon = false;
	void KilledTooManyCivsUpdate()
	{
		if (FUCKING_STOP_YOU_CUNT())
			return;

		LowerHeadNoAsync();

		if (!hasDroppedWeapon && hasDroppedWeaponDelay)
		{
			hasDroppedWeapon = true;
			Vector3 force = Transform.Rotation.Forward;
			weapon.Drop(force);
		}

		if (!hasShownCivsKilledScreen && civsKilledScreenDelay)
		{
			if (GamePreferences.instance.restartLevelOnFail)
			{
				GamePlayManager.instance.Restart();
				return;
			}

			UIManager.instance.FailedTooManyCivsKilled();
			hasShownCivsKilledScreen = true;
		}
	}

	void LowerHeadNoAsync()
	{
		if (FUCKING_STOP_YOU_CUNT())
			return;

		thirdPersonAnimationHelper.LookAtEnabled = true;

		Vector3 lookAtPos = GameObject.Transform.Position;
		lookAtPos += GameObject.Transform.Rotation.Forward * 10.0f;
		var headBone = bodyRenderer.GetBoneObject("head");
		if (headBone == null)
		{
			return;
		}
		Vector3 headPos = headBone.Transform.Position;
		Vector3 dirToFloor = Vector3.Direction(headPos, lookAtPos).Normal;
		Vector3 currentHeadForward = GameObject.Transform.Rotation.Forward;

		var lerp = Vector3.Lerp(currentHeadForward, dirToFloor, lowerHeadTime.Fraction);
		thirdPersonAnimationHelper.WithLook(lerp);
		if (cancellationTokenSource != null && cancellationTokenSource.IsCancellationRequested)
		{
			return;
		}

		thirdPersonAnimationHelper.WithLook(dirToFloor);
	}

	async void LowerHead()
	{
		thirdPersonAnimationHelper.LookAtEnabled = true;

		Vector3 lookAtPos = GameObject.Transform.Position;
		lookAtPos += GameObject.Transform.Rotation.Forward * 10.0f;
		var headBone = bodyRenderer.GetBoneObject("head");
		if (headBone == null)
		{
			return;
		}
		Vector3 headPos = headBone.Transform.Position;
		Vector3 dirToFloor = Vector3.Direction(headPos, lookAtPos).Normal;
		Vector3 currentHeadForward = GameObject.Transform.Rotation.Forward;

		TimeUntil lowerHeadTime = 0.25f;

		while (!lowerHeadTime)
		{
			var lerp = Vector3.Lerp(currentHeadForward, dirToFloor, lowerHeadTime.Fraction);
			thirdPersonAnimationHelper.WithLook(lerp);
			//await Task.Frame();
			await GameTask.Delay(1, cancellationTokenSource.Token);
			if (cancellationTokenSource != null && cancellationTokenSource.IsCancellationRequested)
			{
				return;
			}
		}

		thirdPersonAnimationHelper.WithLook(dirToFloor);
	}

	/*async*/ void WonStart()
	{
		if (FUCKING_STOP_YOU_CUNT())
			return;

		hasShownWonScreen = false;
		//Game.ActiveScene.TimeScale = 1.0f;
		GamePlayManager.instance.WonLevel();

		wonScreenDelay = 1.5f;

		//await Task.DelaySeconds(1.5f);
		/*await GameTask.DelaySeconds(1.5f, cancellationTokenSource.Token);
		if (cancellationTokenSource != null && cancellationTokenSource.IsCancellationRequested)
		{
			return;
		}

		UIManager.instance.Won();*/
	}

	TimeUntil wonScreenDelay;
	bool hasShownWonScreen = false;
	void WonUpdate()
	{
		if (FUCKING_STOP_YOU_CUNT())
			return;

		if (hasShownWonScreen || !wonScreenDelay)
		{
			return;
		}
		UIManager.instance.Won();
		hasShownWonScreen = true;
	}

	protected override void OnUpdate()
	{
		if (FUCKING_STOP_YOU_CUNT())
			return;

		base.OnUpdate();

		StateMachineUpdate();

		//Log.Warning($"Execute Commands Stage: {executeCommandsStage}, loop: {loop}");
	}

	void StateMachineUpdate()
	{
		switch (state)
		{
			case PlayerState_TD.Idle:
				IdleUpdate();
				break;
			case PlayerState_TD.Walking:
				WalkingUpdate();
				break;
			case PlayerState_TD.Deciding:
				DecidingUpdate();
				break;
			case PlayerState_TD.Executing:
				ExecutingUpdate();
				break;
			case PlayerState_TD.Dead:
				DeadUpdate();
				break;
			case PlayerState_TD.KilledTooManyCivs:
				KilledTooManyCivsUpdate();
				break;
			case PlayerState_TD.Won:
				WonUpdate();
				break;
		}
	}

	void SetState(PlayerState_TD newState)
	{
		if (FUCKING_STOP_YOU_CUNT())
			return;

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

	protected override void OnDestroy()
	{
		if (instance == this)
		{
			instance = null;
		}

		if (cancellationTokenSource != null)
		{
			cancellationTokenSource.Cancel();
		}

		base.OnDestroy();
	}

	public bool FUCKING_STOP_YOU_CUNT()
	{
		if (LoadingScreen.isLoading)
			return false;

		if (GameObject == null || !GameObject.IsValid)
			return false;

		if (weapon == null || !weapon.IsValid)
			return true;

		if (RoomManager.instance == null || !RoomManager.instance.IsValid)
			return true;

		if (GamePlayManager.instance == null || !GamePlayManager.instance.IsValid)
			return true;

		if (CitizenSettings.instance == null)
			return true;

		if (GameSettings.instance == null)
			return true;

		return false;
	}
}
