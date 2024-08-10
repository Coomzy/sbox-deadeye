
using Sandbox;
using Sandbox.Citizen;
using Sandbox.Services;
using System;
using System.ComponentModel.Design;
using System.Text.Json;
using static Sandbox.Gizmo;


public enum MenuState
{
	Main,
	GameMode,
	LevelSelect,
	Settings,
	Stats	
}

public class MainMenu : Component
{
	public static MainMenu instance;

	private PanelComponent _currentMenu = null;
	public PanelComponent currentMenu
	{
		get => _currentMenu;
	}

	[Group("Screens"), Property] public MainMenuScreen mainMenuScreen { get; private set; }
	[Group("Screens"), Property] public GameModeScreen gameModeScreen { get; private set; }
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
		_currentMenu = mainMenuScreen;

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

	protected override void OnEnabled()
	{
		base.OnEnabled();

		_currentMenu = mainMenuScreen;
	}

	public void SetMenuState(MenuState state)
	{ 
		menuState = state;
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
		}

		if (selected == null)
		{
			Log.Warning("Selected is null using MenuState " + state.ToString());
		}

		_currentMenu.Enabled = false;
		selected.Enabled = true;
		_currentMenu = selected;

		/*mainMenuScreen.Enabled = state == MenuState.Main;
		levelSelectScreen.Enabled = state == MenuState.Play;
		settingsScreen.Enabled = state == MenuState.Settings;
		statsScreen.Enabled = state == MenuState.Stats;

		if (state == MenuState.Play)
		{
			
		}*/
	}
}
