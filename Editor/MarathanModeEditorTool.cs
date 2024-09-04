public class MarathanModeBar : ToolbarGroup
{
	[Event("tools.headerbar.build", Priority = 150)]
	public static void OnBuildHeaderToolbar(HeadBarEvent e)
	{
		e.RightCenter.Add(new MarathanModeBar(null));
		e.RightCenter.AddSpacingCell(8);
	}

	public MarathanModeBar(Widget parent) : base(parent, "Marathon Mode", null)
	{
		ToolTip = "Marathon Mode";
	}

	public override void Build()
	{
		base.Build();

		AddToggleButton("Off", "close", () => !GamePlayManager.isInMarathonMode, (v) => { GamePlayManager.isInMarathonMode = false; });
		AddToggleButton("On", "arrow_right", () => GamePlayManager.isInMarathonMode, (v) => { GamePlayManager.isInMarathonMode = true; });
	}
}