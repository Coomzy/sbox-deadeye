
using Sandbox;
using Sandbox.Citizen;
using Sandbox.Internal;
using Sandbox.Utils;
using Sandbox.Services;
using System;
using System.ComponentModel.Design;
using System.Text.Json;
using System.Transactions;
using Sandbox.UI.Menu.Helpers;

public enum MenuState
{
	Main,
	GameMode,
	LevelSelect,
	Stats,
	Leaderboards,
	HowToPlay,
	Settings,
	MarathonMode,
}

public enum SplineRotationType
{
	None = 0,
	Towards,
	SideStepLeft,
	SideStepRight,
}

public class MainMenu : Component
{
	public static MainMenu instance;

	public PanelComponent currentMenu { get; private set; }

	[Group("Screens"), Property] public MainMenuScreen mainMenuScreen { get; private set; }
	[Group("Screens"), Property] public GameModeScreen gameModeScreen { get; private set; }
	[Group("Screens"), Property] public MarathonModeScreen marathonModeScreen { get; private set; }
	[Group("Screens"), Property] public LevelSelectScreen levelSelectScreen { get; private set; }
	[Group("Screens"), Property] public SettingsScreen settingsScreen { get; private set; }
	[Group("Screens"), Property] public StatsScreen statsScreen { get; private set; }
	[Group("Screens"), Property] public LeaderboardsScreen leaderboardsScreen { get; private set; }
	[Group("Screens"), Property] public PassThruMenuScreen howToPlayScreen { get; private set; }

	[Group("Setup"), Property] public CameraComponent camera { get; private set; }
	[Group("Setup"), Property] public ModelRenderer GameTitleText { get; private set; }

	[Group("Virtual Area"), Property]
	public Spline TutorialSpline
	{
		get; private set;
	}
	[Group("Virtual Area"), Property]
	public Spline SideViewSpline
	{
		get; private set;
	}

	// VA Spline is short form of Virtual Area Spline
	[Group("Virtual Area"), Property, Rename("VA Spline"), ReadOnly]
	public Spline VASpline
	{
		get; private set;
	}

	[Group("Virtual Area"), Property, Range(0, 1), Rename("VA Active State"), ReadOnly]
	public float VAActiveState
	{
		get; private set;
	}

	[Group("Virtual Area"), Property, Range(0, 1), Rename("VA Next State")]
	public float VANextState
	{
		get; private set;
	}

	[Group("Virtual Area"), Property, Rename("VA Spline Move Speed")]
	public float VASplineMoveSpeed
	{
		get; private set;
	}

	[Group("Virtual Area"), Property, Rename("VA Spline Rotate Speed")]
	public float VASplineRotationSpeed
	{
		get; private set;
	}

	[Group("Virtual Area"), Property, Rename("VA Rotate Type"), ReadOnly]
	public SplineRotationType VARotationType
	{
		get; private set;
	}

	[Group("Runtime"), Property, ReadOnly]
	public MenuState menuState
	{
		get; private set;
	}

	public bool isRefreshingLeaderboards { get; private set; }
	public Leaderboards.Board globalBoard { get; private set; }
	public Leaderboards.Board friendsBoard { get; private set; }

	protected override void OnAwake()
	{
		base.OnAwake();

		instance = this;

		Mouse.Visible = true;
	}

	protected override void OnStart()
	{
		base.OnStart();

		currentMenu = mainMenuScreen;
		SetMenuState(MenuState.Main);

		// Regardless of whether enabled, we will toggle all of them during load.
		ToggleMenu(mainMenuScreen, true);
		ToggleMenu(gameModeScreen, false);
		ToggleMenu(levelSelectScreen, false);
		ToggleMenu(settingsScreen, false);
		ToggleMenu(howToPlayScreen, false);
		ToggleMenu(statsScreen, false);
		ToggleMenu(leaderboardsScreen, false);
		ToggleMenu(marathonModeScreen, false);

		GameStats.CheckMedalAchievements();
	}

	[Button("GetLeaderboard")]
	async void GetLeaderboard()
	{
		if (isRefreshingLeaderboards)
		{
			return;
		}
		isRefreshingLeaderboards = true;

		globalBoard = Sandbox.Services.Leaderboards.Get("level-original");
		friendsBoard = Sandbox.Services.Leaderboards.Get("level-original");

		globalBoard.MaxEntries = 10;
		friendsBoard.MaxEntries = 10;

		globalBoard.Group = "global";
		friendsBoard.Group = "friends";

		var globalRefreshTask = globalBoard.Refresh();
		var friendsRefreshTask = friendsBoard.Refresh();

		await Task.WhenAll(globalRefreshTask, friendsRefreshTask);
		isRefreshingLeaderboards = false;
	}

	public void SetMenuState(MenuState state)
	{
		PanelComponent selected = null;

		switch (state)
		{
			case MenuState.Main:
				selected = mainMenuScreen;
				break;
			case MenuState.GameMode:
				selected = gameModeScreen;
				break;
			case MenuState.LevelSelect:
				selected = levelSelectScreen;
				break;
			case MenuState.Settings:
				selected = settingsScreen;
				break;
			case MenuState.Stats:
				selected = statsScreen;
				GetLeaderboard();
				break;
			case MenuState.Leaderboards:
				selected = leaderboardsScreen;
				break;
			case MenuState.HowToPlay:
				selected = howToPlayScreen;
				break;
			case MenuState.MarathonMode:
				selected = marathonModeScreen;
				break;
		}

		/*if (GameTitleText != null) GameTitleText.RenderType = (
			state == MenuState.Main ?
			ModelRenderer.ShadowRenderType.On :
			ModelRenderer.ShadowRenderType.ShadowsOnly
		);*/

		// Move us to the correct place to skip to the next menu transition.
		//CancelSplineAnimation();

		// Update Virtual Area Spline
		switch (state)
		{
			case MenuState.Main:
			case MenuState.Leaderboards:
				// Reset back, no spline to set here.
				VANextState = 0.0f;
				break;
			case MenuState.HowToPlay:
				VARotationType = SplineRotationType.Towards;
				VASplineMoveSpeed = 1f;
				VASpline = TutorialSpline;
				VANextState = 1.0f;
				break;
			case MenuState.GameMode:
			case MenuState.LevelSelect:
			case MenuState.Stats:
			case MenuState.Settings:
			case MenuState.MarathonMode:
				VARotationType = SplineRotationType.None;
				VASplineMoveSpeed = 2f;
				VASpline = SideViewSpline;
				VANextState = 1.0f;
				break;
		}

		menuState = state;

		if (selected == null)
		{
			Log.Warning("Selected is null using MenuState " + state.ToString());
			return;
		}

		if (currentMenu != null) ToggleMenu(currentMenu, false);
		ToggleMenu(selected, true);
		currentMenu = selected;
	}

	async protected void ToggleMenu(PanelComponent Component, bool Visibility)
	{
		if (Component == null) return;

		// Component.Enabled = Visibility;

		// HACK: This is a fix so that if two screens share a bind it doesn't press the button for both lol
		await Task.FrameEnd();

		Component.Enabled = true;
		Component.SetClass("visible", Visibility);
		Component.SetClass("invisible", !Visibility);
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();
		UpdateCameraViewSpline();
	}

	void CancelSplineAnimation()
	{
		if (MathX.AlmostEqual(VAActiveState, VANextState, 0.1f))
		{
			return;
		}

		Log.Warning(String.Format("Cancelling spline animation {0:00.00} -> {1:00.00}", VAActiveState, VANextState));

		var VAMoveSpeed = VASplineMoveSpeed;
		var VARotSpeed = VASplineRotationSpeed;

		// We will appear instantaneously....
		VASplineMoveSpeed = 500f;
		VASplineRotationSpeed = 500f;
		VAActiveState = VANextState;

		UpdateCameraViewSpline();

		VASplineMoveSpeed = VASplineMoveSpeed;
		VASplineRotationSpeed = VASplineRotationSpeed;
	}

	void UpdateCameraViewSpline()
	{
		if (VASpline == null) return;
		if (MathX.AlmostEqual(VAActiveState, VANextState)) return;

		VAActiveState = MathX.Clamp(Utils.MoveTowards(VAActiveState, VANextState, VASplineMoveSpeed * Time.Delta), 0.0f, 0.99f);

		float time = VAActiveState * VASpline.CalculateTotalSplineLength();

		var cameraPoint = VASpline.GetPointAlongSplineAtTime(time);
		camera.Transform.Position = cameraPoint;

		// The direction is always what's behind us, so step slightly back in time.
		// The precision around 0.1-0.025f is best, otherwise the precision causes us to ripple
		var moveDelta = Vector3.Direction(VASpline.GetPointAlongSplineAtTime(time - 0.1f), cameraPoint);
		moveDelta.z = 0.0f;

		if (moveDelta.Length <= 0.01f)
			return;

		Rotation rotation = Rotation.From(moveDelta.Normal.EulerAngles);

		switch (VARotationType)
		{
			case SplineRotationType.Towards:
				rotation = Rotation.From(moveDelta.Normal.EulerAngles);
				break;
			case SplineRotationType.SideStepLeft:
				rotation = Rotation.FromToRotation(moveDelta.Normal, Vector3.Left.Normal);
				break;
			case SplineRotationType.SideStepRight:
				rotation = Rotation.FromToRotation(moveDelta.Normal, Vector3.Right.Normal);
				break;
		}

		if (VARotationType != SplineRotationType.None)
		{
			camera.Transform.Rotation = Rotation.Slerp(
				camera.Transform.Rotation,
				rotation,
				// Rotation.From(moveDelta.Normal.EulerAngles),
				VASplineRotationSpeed * Time.Delta
			);
		}
	}
}
