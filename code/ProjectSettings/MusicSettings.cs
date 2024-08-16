using Sandbox.Citizen;
using Sandbox.Internal;
using System.IO;
using System;
using Sandbox;

public struct SoundData
{
	public SoundEvent soundEvent { get; set; }
	public float length { get; set; }
}

[GameResource("Music Settings", "ms", "Music Settings")]
public class MusicSettings : GameResourceSingleton<MusicSettings>
{
	[Group("Config"), Property] public float crossFadeTime { get; set; } = 3.0f;
	[Group("Config"), Property] public float musicVolume { get; set; } = 0.5f;

	[Group("Menu"), Property, InlineEditor] public List<SoundData> menuMusic { get; set; } = new List<SoundData>();
	[Group("Game"), Property, InlineEditor] public List<SoundData> gameMusic { get; set; } = new List<SoundData>();
}
