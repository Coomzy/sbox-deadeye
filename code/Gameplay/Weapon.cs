
using Sandbox.Audio;
using Sandbox;
using Sandbox.Citizen;

public class Weapon : Component
{
	[Group("Setup"), Property] public Rigidbody rigidbody { get; set; }
	[Group("Setup"), Property] public GameObject muzzleFlashHolder { get; set; }
	[Group("Setup"), Property] public PrefabFile smokePFX { get; set; }
	[Group("Setup"), Property] public LegacyParticleSystem muzzleFlashVFX { get; set; }
	[Group("Setup"), Property] public LineRenderer bulletTracerLineRenderer { get; set; }
	[Group("Setup"), Property] public Light muzzleFlashLight { get; set; }
	[Group("Setup"), Property] public PrefabFile bloodSplatPFX { get; set; }
	[Group("Setup"), Property] public ParticleEffect shellEjectPFX { get; set; }
	[Group("Setup"), Property] public ParticleConeEmitter shellEjectEmitter { get; set; }

	protected override void OnAwake()
	{
		base.OnAwake();

		muzzleFlashVFX.Enabled = false;
		bulletTracerLineRenderer.Enabled = false;
		muzzleFlashLight.Enabled = false;
		shellEjectEmitter.Enabled = false;
	}

	[Button("Shoot")]
	public void Shoot(Vector3 hitPosition)
	{
		//Particles.Create();
		//muzzleFlashVFX.
		var particles = muzzleFlashVFX.Particles;
		var soundHandle = Sound.Play("weapon.pistol", GameObject.Transform.Position);
		soundHandle.TargetMixer = Mixer.FindMixerByName("Game");

		var smokeGO = Scene.CreateObject();
		smokeGO.SetPrefabSource(smokePFX.ResourcePath);
		smokeGO.UpdateFromPrefab();
		smokeGO.Transform.Position = muzzleFlashHolder.Transform.Position;
		smokeGO.Transform.Rotation = muzzleFlashHolder.Transform.Rotation;


		var bloodSplatGO = Scene.CreateObject();
		bloodSplatGO.SetPrefabSource(bloodSplatPFX.ResourcePath);
		bloodSplatGO.UpdateFromPrefab();
		bloodSplatGO.Transform.Position = hitPosition;
		bloodSplatGO.Transform.Rotation = (-GameObject.Transform.Rotation.Forward).EulerAngles.ToRotation();

		/*shellEjectEmitter.Enabled = true;
		shellEjectEmitter.Emit(shellEjectPFX);
		shellEjectEmitter.Enabled = false;*/

		BulletTracer(hitPosition);
		MuzzleFlashLight();
	}

	async void BulletTracer(Vector3 hitPosition)
	{
		List<Vector3> points = new List<Vector3>();
		points.Add(muzzleFlashHolder.Transform.Position + (muzzleFlashHolder.Transform.Rotation.Forward * 25.0f));
		points.Add(muzzleFlashHolder.Transform.Position + (muzzleFlashHolder.Transform.Rotation.Forward * 125.0f));
		bulletTracerLineRenderer.VectorPoints = points;
		bulletTracerLineRenderer.Enabled = true;
		await Task.Delay(20);
		bulletTracerLineRenderer.Enabled = false;
	}

	async void MuzzleFlashLight()
	{
		muzzleFlashLight.Enabled = true;
		await Task.Delay(1);
		muzzleFlashLight.Enabled = false;
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
