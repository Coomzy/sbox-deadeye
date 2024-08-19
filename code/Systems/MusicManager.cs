
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

	public const string CUT_BASS = "cut_bass";
	public const string CUT_DRUMS = "cut_drums";
	public const string CUT_GUITAR = "cut_guitar";
	public const string CUT_INSTRUMENTS = "cut_instruments";

	public SoundHandle mixBass;
	public SoundHandle mixDrums;
	public SoundHandle mixGuitar;
	public SoundHandle mixInstruments;

	public float tgtVolBass;
	public float tgtVolDrums;
	public float tgtVolGuitar;
	public float tgtVolInstruments;

	public float currentVolBass;
	public float currentVolDrums;
	public float currentVolGuitar;
	public float currentVolInstruments;

	float moveRate = 1.0f;
	public static float timeTillBeat;

	protected override void OnAwake()
	{
		base.OnAwake();

		instance = this;

		GameObject.Flags = GameObjectFlags.DontDestroyOnLoad;
	}

	protected override void OnStart()
	{
		base.OnStart();

		GamePreferences.instance.ApplyVolumesToMixers();

		MusicStart();
	}

	async void MusicStart()
	{
		Sound.Preload(CUT_BASS);
		Sound.Preload(CUT_DRUMS);
		Sound.Preload(CUT_GUITAR);
		Sound.Preload(CUT_INSTRUMENTS);

		Sound.Preload("weapon.pistol");

		await Task.DelaySeconds(1.5f);

		MusicLoop();

		mixBass.Volume = 0.0f;
		mixDrums.Volume = 0.0f;
		mixGuitar.Volume = 0.0f;
		mixInstruments.Volume = 0.0f;
	}

	async void MusicLoop()
	{
		mixBass = Sound.Play(CUT_BASS);
		mixDrums = Sound.Play(CUT_DRUMS);
		mixGuitar = Sound.Play(CUT_GUITAR);
		mixInstruments = Sound.Play(CUT_INSTRUMENTS);

		mixBass.TargetMixer = Mixer.FindMixerByName("Music");
		mixDrums.TargetMixer = Mixer.FindMixerByName("Music");
		mixGuitar.TargetMixer = Mixer.FindMixerByName("Music");
		mixInstruments.TargetMixer = Mixer.FindMixerByName("Music");


		float timeBase = (0.241375f * 2.0f) + -0.00775f;

		timeTillBeat = timeBase + 0.1f + -0.008f;

		await Task.Frame();

		while (mixBass.IsPlaying)
		{
			timeTillBeat -= Time.Delta;

			if (timeTillBeat < 0.0f)
			{
				float timeOver = timeTillBeat;
				timeTillBeat = timeBase - timeOver;

				//var soundHandle = Sound.Play("weapon.pistol");
				//soundHandle.Volume = 0.4f;
				//soundHandle.TargetMixer = Mixer.FindMixerByName("Game");
			}

			await Task.Frame();
		}

		MusicLoop();
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();

		if (Input.Pressed("ToggleMusic"))
		{
			GamePreferences.instance.ToggleMusic();
		}

		if (mixBass == null || mixDrums == null || mixGuitar == null || mixInstruments == null)
		{
			return;
		}

		tgtVolGuitar = 1.0f;

		if (Game.ActiveScene.Title == GameSettings.instance.menuLevel.scene.Title)
		{
			tgtVolBass = 0.0f;
			tgtVolDrums = 0.0f;
			tgtVolInstruments = 0.0f;
		}
		else
		{
			if (RoomManager.instance?.rooms == null)
			{
				return;
			}

			if (RoomManager.instance.rooms.Count <= 0)
			{
				return;
			}

			tgtVolBass = 1.0f;

			float total = RoomManager.instance.rooms.Count;
			float current = RoomManager.instance.roomIndex;

			float pct = current / total;

			tgtVolDrums = (pct > 0.33f) ? 1.0f : 0.0f;
			tgtVolInstruments = (pct > 0.66f) ? 1.0f : 0.0f;
		}

		currentVolBass = Utils.MoveTowards(currentVolBass, tgtVolBass, moveRate * Time.Delta);
		currentVolDrums = Utils.MoveTowards(currentVolDrums, tgtVolDrums, moveRate * Time.Delta);
		currentVolGuitar = Utils.MoveTowards(currentVolGuitar, tgtVolGuitar, moveRate * Time.Delta);
		currentVolInstruments = Utils.MoveTowards(currentVolInstruments, tgtVolInstruments, moveRate * Time.Delta);

		mixBass.Volume = currentVolBass;
		mixDrums.Volume = currentVolDrums;
		mixGuitar.Volume = currentVolGuitar;
		mixInstruments.Volume = currentVolInstruments;
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
		if (Game.IsEditor && !Game.IsPlaying)
		{
			Log.Info("IS EDITOR AND IS NOT PLAYING");
			return;
		}

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
	}
}