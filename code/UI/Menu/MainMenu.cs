
using Sandbox;
using Sandbox.Citizen;
using Sandbox.Services;
using System;
using System.Text.Json;
using static Sandbox.Gizmo;


public enum MenuState
{
	Main,
	Play,
	Settings,
	Stats	
}

public class MainMenu : Component
{
	public static MainMenu instance;

	[Group("Screens"), Property] public MainMenuScreen mainMenuScreen { get; private set; }
	[Group("Screens"), Property] public LevelSelectScreen levelSelectScreen { get; private set; }
	[Group("Screens"), Property] public SettingsScreen settingsScreen { get; private set; }
	[Group("Screens"), Property] public StatsScreen statsScreen { get; private set; }

	[Group("Runtime"), Property] public MenuState menuState { get; private set; }

	public bool isRefreshingLeaderboards { get; private set; }
	public Leaderboards.Board globalBoard { get; private set; }
	public Leaderboards.Board friendsBoard { get; private set; }

	protected override void OnAwake()
	{
		instance = this;

		base.OnAwake();

		SetMenuState(MenuState.Main);
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
		menuState = state;

		mainMenuScreen.Enabled = state == MenuState.Main;
		levelSelectScreen.Enabled = state == MenuState.Play;
		settingsScreen.Enabled = state == MenuState.Settings;
		statsScreen.Enabled = state == MenuState.Stats;

		if (state == MenuState.Play)
		{
			GetLeaderboard();
		}
	}
}
