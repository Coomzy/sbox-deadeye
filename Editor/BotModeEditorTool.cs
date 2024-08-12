public class BotModeBar : ToolbarGroup
{
	BotModePreferences _inst;
	public BotModePreferences inst
	{
		get => _inst ?? (_inst = BotModePreferences.instance);
		set => _inst = value;
	}

	[Event("tools.headerbar.build", Priority = 150)]
	public static void OnBuildHeaderToolbar(HeadBarEvent e)
	{
		e.RightCenter.Add(new BotModeBar(null));
		e.RightCenter.AddSpacingCell(8);
	}

	public BotModeBar(Widget parent) : base(parent, "Bot Mode", null)
	{
		ToolTip = "Bot Mode";
	}

	public override void Build()
	{
		base.Build();

		AddToggleButton("Off", "close", () => inst.mode == PlayerBotMode.None, (v) => { if (v) { SetBotMode(PlayerBotMode.None); } });
		AddToggleButton("Slowest", "play_arrow", () => inst.mode == PlayerBotMode.SlowestTime, (v) => { if (v) { SetBotMode(PlayerBotMode.SlowestTime); } });
		AddToggleButton("Fastest", "fast_forward", () => inst.mode == PlayerBotMode.FastestTime, (v) => { if (v) { SetBotMode(PlayerBotMode.FastestTime); } });
	}

	public void SetBotMode(PlayerBotMode mode)
	{
		inst.mode = mode;
		inst.Save();
	}
}