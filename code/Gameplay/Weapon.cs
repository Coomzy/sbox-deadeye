
using Sandbox.Citizen;

public class Weapon : Component
{
	[Group("Setup"), Property] public Rigidbody rigidbody { get; set; }
	[Group("Setup"), Property] public GameObject muzzleFlashHolder { get; set; }
	[Group("Setup"), Property] public PrefabFile smokePFX { get; set; }
	[Group("Setup"), Property] public LegacyParticleSystem muzzleFlashVFX { get; set; }

	[Button("Shoot")]
	public void Shoot()
	{
		//Particles.Create();
		//muzzleFlashVFX.
		var particles = muzzleFlashVFX.Particles;
		var handle = Sound.Play("weapon.pistol", GameObject.Transform.Position);

		var smokeGO = Scene.CreateObject();
		smokeGO.SetPrefabSource(smokePFX.ResourcePath);
		smokeGO.UpdateFromPrefab();
		smokeGO.Transform.Position = GameObject.Transform.Position;
		smokeGO.Transform.Rotation = GameObject.Transform.Rotation;
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
