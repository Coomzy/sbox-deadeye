
using Sandbox.Citizen;
using System.Security.Authentication.ExtendedProtection;

public class LifeTime : Component
{
	[Property] public float? time { get; set; }
	TimeSince lifeTimeSet;

	protected override void OnStart()
	{
		base.OnStart();

		lifeTimeSet = 0.0f;
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();

		if (!time.HasValue)
			return;

		if (lifeTimeSet < time)
			return;

		GameObject.Destroy();
	}
}
