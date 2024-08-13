
using Sandbox;
using Sandbox.Citizen;
using Sandbox.UI;
using static Sandbox.Gizmo;
using static Sandbox.PhysicsContact;
using static Sandbox.VertexLayout;

public enum PlayerState_FP
{
	Idle,
	Walking,
	Executing,
	Dead,
	KilledTooManyCivs,
	Won,
};

public class Player_FP : Component
{
	public static Player_FP instance;

	[Group("Runtime"), Property] public PlayerState_FP state { get; private set; } = PlayerState_FP.Idle;
	[Group("Runtime"), Property] public int civiliansKilled { get; private set; }

	public TimeSince timeSinceStartedDecisionMaking { get; private set; }

	protected override void OnAwake()
	{
		instance = this;

		base.OnAwake();
	}

	protected override void OnStart()
	{
		base.OnStart();

		GoToNextRoom();
		Mouse.CursorType = "crosshair";
	}

	void GoToNextRoom()
	{
		Mouse.Visible = false;
		RoomManager.instance.roomIndex++;
		SetState(PlayerState_FP.Walking);
	}

	async void WalkingStart()
	{
		TimeUntil fadeIn = 1.5f;

		UIManager.instance.blackFadeWidget.Enabled = true;
		while (UIManager.instance.blackFadeAlpha < 1.0f)
		{
			UIManager.instance.blackFadeAlpha = fadeIn.Fraction;
			await Task.Frame();
		}
		UIManager.instance.blackFadeAlpha = 1.0f;

		await GameTask.DelaySeconds(0.25f);

		TimeUntil walkTime = 5.0f;

		while (!walkTime)
		{
			Sound.Play("footstep-concrete", Transform.Position);
			await GameTask.DelaySeconds(0.5f);
		}
		Sound.Play("footstep-concrete", Transform.Position);

		await GameTask.DelaySeconds(0.25f);

		SetState(PlayerState_FP.Executing);
	}

	async void ExecutingStart()
	{
		//await GameTask.DelaySeconds(3.5f);
		Mouse.Visible = true;

		TimeUntil fadeOut = 0.25f;

		do
		{
			UIManager.instance.blackFadeAlpha = 1.0f - fadeOut.Fraction;
			await Task.Frame();
		}
		while (UIManager.instance.blackFadeAlpha > 0.0f);

		//UIManager.instance.blackFadeAlpha = 0.0f;
		UIManager.instance.blackFadeWidget.Enabled = false;
		Mouse.Visible = true;

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
				SetState(PlayerState_FP.Dead);
			}
			else
			{
				RoomManager.instance.currentRoom.currentTarget.Deselect();
				RoomManager.instance.roomIndex++;
				SetState(PlayerState_FP.Walking);
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
				SetState(PlayerState_FP.KilledTooManyCivs);
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
				SetState(PlayerState_FP.Won);
			}
			else
			{
				RoomManager.instance.roomIndex++;
				SetState(PlayerState_FP.Walking);
			}
		}
	}

	bool ShootKeyIsDown()
	{
		if (Input.Pressed("Shoot_FP"))
			return true;

		return false;
	}

	async void DeadStart()
	{
		Stats.Increment(Stats.DIED);

		UIManager.instance.blackFadeAlpha = 1.0f;

		await Task.DelaySeconds(1.5f);

		Game.ActiveScene.Load(Game.ActiveScene.Source);
	}

	async void KilledTooManyCivsStart()
	{
		Stats.Increment(Stats.FAILURE_TOO_MANY_CIVS_KILLED);

		UIManager.instance.blackFadeAlpha = 1.0f;

		await Task.DelaySeconds(1.5f);

		Game.ActiveScene.Load(Game.ActiveScene.Source);
	}

	async void WonStart()
	{
		Stats.Increment(Stats.WON);

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
		if (!Input.EscapePressed)
			return;
		Input.EscapePressed = false;

		Game.ActiveScene.LoadFromFile("scenes/menu.scene");
	}

	void CheckForReloadLevelInput()
	{
		if (!Input.Pressed("restart"))
			return;

		Game.ActiveScene.Load(Game.ActiveScene.Source);
	}

	void StateMachineUpdate()
	{
		switch (state)
		{
			case PlayerState_FP.Executing:
				ExecutingUpdate();
				break;
		}
	}

	void SetState(PlayerState_FP newState)
	{
		state = newState;

		switch (state)
		{
			case PlayerState_FP.Walking:
				WalkingStart();
				break;
			case PlayerState_FP.Executing:
				ExecutingStart();
				break;
			case PlayerState_FP.Dead:
				DeadStart();
				break;
			case PlayerState_FP.KilledTooManyCivs:
				KilledTooManyCivsStart();
				break;
			case PlayerState_FP.Won:
				WonStart();
				break;
		}
	}
}
