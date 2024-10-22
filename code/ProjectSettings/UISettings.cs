using Sandbox.Citizen;
using Sandbox.Internal;
using System.IO;
using System;
using Sandbox;


[GameResource("UI Settings", "uis", "UI Settings")]
public class UISettings : GameResourceSingleton<UISettings>
{
	[Group("Config"), Property] public float goFadeOutTime { get; set; } = 1.0f;
	[Group("Config"), Property] public Curve countdownCurve { get; set; }

	[Group("Medals"), Property] public Texture civAliveImageTex = Texture.Load(FileSystem.Mounted, "textures/civ_alive.png");
	[Group("Medals"), Property] public Texture civDeadImageTex = Texture.Load(FileSystem.Mounted, "textures/civ_dead.png");
}
