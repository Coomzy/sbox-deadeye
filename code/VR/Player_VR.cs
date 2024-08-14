
using Sandbox;
using Sandbox.Citizen;
using Sandbox.UI;
using static Sandbox.Gizmo;
using static Sandbox.PhysicsContact;
using static Sandbox.VertexLayout;

public enum PlayerState_VR
{
	Idle,
	Walking,
	Executing,
	Dead,
	KilledTooManyCivs,
	Won,
};

public class Player_VR : Component
{
	public static Player_VR instance;

	[Group("Runtime"), Property] public PlayerState_VR state { get; private set; } = PlayerState_VR.Idle;
	[Group("Runtime"), Property] public int civiliansKilled { get; private set; }

	public TimeSince timeSinceStartedDecisionMaking { get; private set; }

	protected override void OnAwake()
	{
		instance = this;

		base.OnAwake();
		Mouse.Visible = false;
	}

	protected override void OnStart()
	{
		base.OnStart();

		GoToNextRoom();
	}

	void GoToNextRoom()
	{
		RoomManager.instance.roomIndex++;
		SetState(PlayerState_VR.Walking);
	}

	async void WalkingStart()
	{
		TimeUntil fadeIn = 1.5f;

		// Fade To Black
		/*UIManager.instance.blackFadeWidget.Enabled = true;
		while (UIManager.instance.blackFadeAlpha < 1.0f)
		{
			UIManager.instance.blackFadeAlpha = fadeIn.Fraction;
			await Task.Frame();
		}
		UIManager.instance.blackFadeAlpha = 1.0f; */

		await GameTask.DelaySeconds(0.25f);

		TimeUntil walkTime = 5.0f;

		while (!walkTime)
		{
			Sound.Play("footstep-concrete", Transform.Position);
			await GameTask.DelaySeconds(0.5f);
		}
		Sound.Play("footstep-concrete", Transform.Position);

		await GameTask.DelaySeconds(0.25f);

		SetState(PlayerState_VR.Executing);
	}

	/*async*/ void ExecutingStart()
	{
		//await GameTask.DelaySeconds(3.5f);

		// Fade From Black
		/*TimeUntil fadeOut = 0.25f;

		do
		{
			UIManager.instance.blackFadeAlpha = 1.0f - fadeOut.Fraction;
			await Task.Frame();
		}
		while (UIManager.instance.blackFadeAlpha > 0.0f);

		//UIManager.instance.blackFadeAlpha = 0.0f;
		UIManager.instance.blackFadeWidget.Enabled = false;*/

		timeSinceStartedDecisionMaking = 0;
	}

	void ExecutingUpdate()
	{
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
				SetState(PlayerState_VR.Dead);
			}
			else
			{
				RoomManager.instance.currentRoom.currentTarget.Deselect();
				RoomManager.instance.roomIndex++;
				SetState(PlayerState_VR.Walking);
			}

			return;
		}

		// TODO: Move crosshair and find target

		if (ShootKeyIsDown())
		{
			/*if (target.isBadTarget)
			{
				Stats.Increment(Stats.TARGETS_ELIMINATED);
			}
			else
			{
				civiliansKilled++;
				Stats.Increment(Stats.CIVILIANS_KILLED);
			}

			target.Die();*/

			if (civiliansKilled >= LevelData.active.allowedCivilianCasualties)
			{
				SetState(PlayerState_VR.KilledTooManyCivs);
			}
		}

		bool allBadGuysDead = true;
		foreach (var target in RoomManager.instance.currentRoom.targets)
		{
			if (!target.isBadTarget)
				continue;

			if (target.isDead)
				continue;

			allBadGuysDead = false;
			break;
		}

		if (allBadGuysDead)
		{
			if (RoomManager.instance.isFinalRoom)
			{
				SetState(PlayerState_VR.Won);
			}
			else
			{
				RoomManager.instance.roomIndex++;
				SetState(PlayerState_VR.Walking);
			}
		}
	}

	bool ShootKeyIsDown()
	{
		if (Input.Pressed("Shoot_VR"))
			return true;

		return false;
	}

	async void DeadStart()
	{
		GameStats.Increment(GameStats.DIED);

		//UIManager.instance.blackFadeAlpha = 1.0f;

		await Task.DelaySeconds(1.5f);

		Game.ActiveScene.Load(Game.ActiveScene.Source);
	}

	async void KilledTooManyCivsStart()
	{
		GameStats.Increment(GameStats.FAILURE_TOO_MANY_CIVS_KILLED);

		//UIManager.instance.blackFadeAlpha = 1.0f;

		await Task.DelaySeconds(1.5f);

		Game.ActiveScene.Load(Game.ActiveScene.Source);
	}

	async void WonStart()
	{
		GameStats.Increment(GameStats.WON);

		await Task.DelaySeconds(1.5f);

		//UIManager.instance.Won();
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
		if (!Input.VR.LeftHand.ButtonB.IsPressed)
			return;

		Game.ActiveScene.LoadFromFile("scenes/menu.scene");
	}

	void CheckForReloadLevelInput()
	{
		if (!Input.VR.RightHand.ButtonB.IsPressed)
			return;

		Game.ActiveScene.Load(Game.ActiveScene.Source);
	}

	void StateMachineUpdate()
	{
		switch (state)
		{
			case PlayerState_VR.Executing:
				ExecutingUpdate();
				break;
		}
	}

	void SetState(PlayerState_VR newState)
	{
		state = newState;

		switch (state)
		{
			case PlayerState_VR.Walking:
				WalkingStart();
				break;
			case PlayerState_VR.Executing:
				ExecutingStart();
				break;
			case PlayerState_VR.Dead:
				DeadStart();
				break;
			case PlayerState_VR.KilledTooManyCivs:
				KilledTooManyCivsStart();
				break;
			case PlayerState_VR.Won:
				WonStart();
				break;
		}
	}
}
