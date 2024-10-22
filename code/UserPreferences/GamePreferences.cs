
using Sandbox.Citizen;
using Sandbox.Internal;
using System.IO;
using System;
using Sandbox;
using Sandbox.Audio;

public class GamePreferences : UserPreferences<GamePreferences>
{
	public bool restartLevelOnFail { get; set; } = false;
	public bool restartLevelOnCivKill { get; set; } = false;
	public bool useOriginalClothing { get; set; } = false;

	public float gameVolume { get; set; } = 1.0f;
	public float musicVolume { get; set; } = 0.35f;
	public float uiVolume { get; set; } = 0.8f;

	public bool muteMusic { get; set; }

	public void ApplyVolumesToMixers()
	{
		var mixerGame = Mixer.FindMixerByName("Game");
		var mixerMusic = Mixer.FindMixerByName("Soundtrack");
		var mixerUI = Mixer.FindMixerByName("UI");

		//Log.Info($"mixerGame: {mixerGame}, mixerMusic: {mixerMusic}, mixerUI: {mixerUI}");
		if (mixerGame != null)
		{
			mixerGame.Volume = gameVolume;
		}
		if (mixerMusic != null)
		{
			mixerMusic.Volume = muteMusic ? 0.0f : musicVolume;
		}
		if (mixerUI != null)
		{
			mixerUI.Volume = uiVolume;
		}
	}

	public void UseOriginalClothing(bool isUsing)
	{
		useOriginalClothing = isUsing;
		Save();
		if (MainMenuCharacter.instance != null)
		{
			MainMenuCharacter.instance.Refresh();
		}
	}

	public void MuteMusic(bool mute)
	{
		muteMusic = mute;
		ApplyVolumesToMixers();
		Save();
	}

	public void ToggleMusic()
	{
		muteMusic = !muteMusic;
		ApplyVolumesToMixers();
		Save();
	}
}
