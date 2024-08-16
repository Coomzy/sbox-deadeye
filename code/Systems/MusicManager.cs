
using Sandbox;
using Sandbox.Audio;
using Sandbox.Citizen;
using Sandbox.Services;
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using static Sandbox.Gizmo;

public class MusicManager : Component, IHotloadManaged
{
	public static MusicManager instance { get; private set; }

	public static bool currentTrackIsMainMenu { get; private set; }
	public static Task currentMusicTask { get; private set; } = null;
	public static Task fadingMusicTask { get; private set; } = null;
	public static CancellationTokenSource currentCancellationToken { get; private set; }

	int menuMusicIndex { get; set; }
	int gameMusicIndex { get; set; }

	protected override void OnAwake()
	{
		instance = this;

		base.OnAwake();

		GameObject.Flags = GameObjectFlags.DontDestroyOnLoad;

		var insts = Scene.GetAllComponents<MusicManager>();

		Log.Info($"MusicManager::OnAwake() MusicManager inst count: {insts.Count()}");

		GamePreferences.instance.ApplyVolumesToMixers();

		menuMusicIndex = Game.Random.Next(0, MusicSettings.instance.menuMusic.Count);
		gameMusicIndex = Game.Random.Next(0, MusicSettings.instance.gameMusic.Count);

		if (Game.ActiveScene.Title == GameSettings.instance.menuLevel.scene.Title)
		{
			PlayMenuMusicTrack();
		}
		else
		{
			PlayGameMusicTrack();
		}
	}

	[Button("Start Menu Music")]
	void PlayMenuMusicTrack()
	{
		var soundData = MusicSettings.instance.menuMusic[0];
		StartMusicTask(soundData, PlayMenuMusicTrack);
		currentTrackIsMainMenu = true;
	}

	[Button("Start Game Music")]
	void PlayGameMusicTrack()
	{
		var soundData = MusicSettings.instance.gameMusic[0];
		StartMusicTask(soundData, PlayGameMusicTrack);
		currentTrackIsMainMenu = false;
	}

	void StartMusicTask(SoundData soundData, Action nextMusicCallback)
	{
		currentCancellationToken?.Cancel();
		currentCancellationToken = new CancellationTokenSource();

		var task = PlayMusic(soundData, nextMusicCallback, currentCancellationToken.Token);

		if (currentMusicTask != null)
		{
			fadingMusicTask = currentMusicTask;
		}
		currentMusicTask = task;
	}

	async Task PlayMusic(SoundData soundData, Action nextMusicCallback, CancellationToken cancellationToken)
	{		
		var soundHandle = Sound.Play(soundData.soundEvent);
		soundHandle.TargetMixer = Mixer.FindMixerByName("Music");
		var mixer = Mixer.FindMixerByName("Music");		

		TimeSince timeSinceSoundStarted = 0.0f;
		TimeUntil fadeInTime = MusicSettings.instance.crossFadeTime / 2.0f;

		while (!fadeInTime && !cancellationToken.IsCancellationRequested)
		{
			soundHandle.Volume = fadeInTime.Fraction * MusicSettings.instance.musicVolume;
			await Task.Frame();
		}

		float crossFadeTarget = soundData.length - (MusicSettings.instance.crossFadeTime / 2.0f);

		while (timeSinceSoundStarted < crossFadeTarget && !cancellationToken.IsCancellationRequested)
		{
			await Task.Frame();
		}

		TimeUntil fadeOutTime = MusicSettings.instance.crossFadeTime / 2.0f;

		if (!cancellationToken.IsCancellationRequested)
		{
			nextMusicCallback?.Invoke();
		}

		while (!fadeOutTime)
		{
			soundHandle.Volume = (1.0f - fadeOutTime.Fraction) * MusicSettings.instance.musicVolume;
			await Task.Frame();
		}
	}

	public void OnLevelLoaded()
	{
		Log.Info($"MusicManager::OnLevelLoaded() GameObject: {GameObject}");

		bool isMainMenu = Game.ActiveScene.Title == GameSettings.instance.menuLevel.scene.Title;

		if (isMainMenu == currentTrackIsMainMenu)
		{
			return;
		}

		// TODO: Stop it restarting on same type
		if (Game.ActiveScene.Title == GameSettings.instance.menuLevel.scene.Title)
		{
			PlayMenuMusicTrack();
		}
		else
		{
			PlayGameMusicTrack();
		}
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();

		if (Input.Pressed("ToggleMusic"))
		{
			GamePreferences.instance.ToggleMusic();
		}
	}

	protected override void OnDestroy()
	{
		//Log.Info($"MusicManager::OnDestroy() GameObject: {GameObject}");
		instance = null;
		base.OnDestroy();
	}

	void IHotloadManaged.Created(IReadOnlyDictionary<string, object> state)
	{		
		instance = null;
	}

	void IHotloadManaged.Destroyed(Dictionary<string, object> state)
	{
		instance = null;
	}
}

public class MusicManagerSystem : GameObjectSystem
{
	public MusicManagerSystem(Scene scene) : base(scene)
	{
		// TODO: Report bug about objects begin created in here having null GameObjects
		/*if (MusicManager.instance == null)
		{
			//var musicManagerGO = scene.CreateObject(true);
			var musicManagerGO = new GameObject(true, "Music Manager");
			musicManagerGO.Components.Create<MusicManager>();
			Log.Info($"MusicManager.instance was null! Creating {musicManagerGO}");
		}*/

		Listen(Stage.SceneLoaded, -1, OnLevelLoaded, "OnLevelLoaded");
	}

	void OnLevelLoaded()
	{
		if (!Game.IsPlaying)
			return;

		if (MusicManager.instance == null)
		{
			var musicManagerGO = Game.ActiveScene.CreateObject(true);
			if (Game.IsEditor)
			{
				musicManagerGO.Name = "Music Manager";
			}
			musicManagerGO.Components.Create<MusicManager>();
		}

		if (MusicManager.instance == null)
			return;

		MusicManager.instance.OnLevelLoaded();
	}
}