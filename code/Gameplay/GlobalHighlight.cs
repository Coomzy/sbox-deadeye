using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class GlobalHighlight : Component, IRestartable
{
	public static GlobalHighlight instance;

	[Group("Setup"), Property] public HighlightOutline highlightOutline { get; set; }

	public void PreRestart()
	{
		highlightOutline.Color = Color.Transparent;
		highlightOutline.ObscuredColor = Color.Transparent;
	}

	public void PostRestart()
	{

	}

	protected override void OnAwake()
	{
		instance = this;

		base.OnAwake();
	}

	protected override void OnDestroy()
	{
		if (instance == this)
		{
			instance = null;
		}

		base.OnDestroy();
	}
}
