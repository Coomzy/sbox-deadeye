
using Sandbox.Citizen;

public class Weapon : Component
{
	[Group("Setup"), Property] public Rigidbody rigidbody { get; set; }
	[Group("Setup"), Property] public LegacyParticleSystem muzzleFlashVFX { get; set; }

	[Button("Shoot")]
	public void Shoot()
	{
		//Particles.Create();
		//muzzleFlashVFX.
	}

	[Button("Drop")]
	public void Drop()
	{
		GameObject.SetParent(null, true);
		rigidbody.Enabled = true;
	}

	public override void Reset()
	{ 
		base.Reset();

		rigidbody = GameObject.Components.GetInDescendantsOrSelf<Rigidbody>(true);
	}

}
