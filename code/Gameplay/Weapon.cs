
using Sandbox.Audio;
using Sandbox;
using Sandbox.Citizen;
using System.Threading;

public class Weapon : Component, IRestartable, IShutdown
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
	CancellationTokenSource cancellationTokenSource { get; set; }
	TimeSince timeSinceLastShot {  get; set; }
	GameObject originalParent { get; set; }

	Vector3 startPos;
	Rotation startRot;
	Vector3 startRigidBodyPos;
	Rotation startRigidBodyRot;

	protected override void OnAwake()
	{
		base.OnAwake();

		originalParent = GameObject.Parent;
		startPos = GameObject.Transform.LocalPosition;
		startRot = GameObject.Transform.LocalRotation;
		startRigidBodyPos = rigidbody.GameObject.Transform.LocalPosition;
		startRigidBodyRot = rigidbody.GameObject.Transform.LocalRotation;
		cancellationTokenSource = new CancellationTokenSource();
		PreRestart();
	}

	public void PreRestart()
	{
		muzzleFlashVFX.Enabled = false;
		bulletTracerLineRenderer.Enabled = false;
		muzzleFlashLight.Enabled = false;
		shellEjectEmitter.Enabled = false;

		rigidbody.Enabled = false;
		GameObject.SetParent(originalParent, true);
		GameObject.Transform.LocalPosition = startPos;
		GameObject.Transform.LocalRotation = startRot;
		rigidbody.GameObject.Transform.LocalPosition = startRigidBodyPos;
		rigidbody.GameObject.Transform.LocalRotation = startRigidBodyRot;
	}

	public void PostRestart()
	{

	}

	public void PreShutdown()
	{
		this.Enabled = false;
	}

	public void PostShutdown()
	{

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

		//BulletTracer(hitPosition);
		//MuzzleFlashLight();

		timeSinceLastShot = 0;
		List<Vector3> points = new List<Vector3>();
		points.Add(muzzleFlashHolder.Transform.Position + (muzzleFlashHolder.Transform.Rotation.Forward * 25.0f));
		points.Add(muzzleFlashHolder.Transform.Position + (muzzleFlashHolder.Transform.Rotation.Forward * 125.0f));
		bulletTracerLineRenderer.VectorPoints = points;
		bulletTracerLineRenderer.Enabled = true;
		muzzleFlashLight.Enabled = true;
	}

	async void BulletTracer(Vector3 hitPosition)
	{
		List<Vector3> points = new List<Vector3>();
		points.Add(muzzleFlashHolder.Transform.Position + (muzzleFlashHolder.Transform.Rotation.Forward * 25.0f));
		points.Add(muzzleFlashHolder.Transform.Position + (muzzleFlashHolder.Transform.Rotation.Forward * 125.0f));
		bulletTracerLineRenderer.VectorPoints = points;
		bulletTracerLineRenderer.Enabled = true;
		//await Task.Delay(20);
		await GameTask.Delay(20, cancellationTokenSource.Token);
		if (cancellationTokenSource != null && cancellationTokenSource.IsCancellationRequested)
		{
			return;
		}
		bulletTracerLineRenderer.Enabled = false;
	}

	async void MuzzleFlashLight()
	{
		muzzleFlashLight.Enabled = true;
		//await Task.Delay(1);
		await GameTask.Delay(1, cancellationTokenSource.Token);
		if (cancellationTokenSource != null && cancellationTokenSource.IsCancellationRequested)
		{
			return;
		}
		muzzleFlashLight.Enabled = false;
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();

		if (bulletTracerLineRenderer.Enabled && timeSinceLastShot > 0.02f)
		{
			bulletTracerLineRenderer.Enabled = false;
		}
		if (muzzleFlashLight.Enabled && timeSinceLastShot > 0.001f)
		{
			muzzleFlashLight.Enabled = false;
		}
	}

	[Button("Drop")]
	public void Drop(Vector3 force)
	{
		GameObject.SetParent(null, true);
		rigidbody.Enabled = true;
		rigidbody.ApplyImpulse(force);
		var weaponRandomTorque = Game.Random.Float(1500.0f, 3500.0f);
		rigidbody.ApplyTorque(Game.Random.Rotation().Forward * weaponRandomTorque);
	}

	protected override void OnDestroy()
	{
		if (cancellationTokenSource != null)
		{
			cancellationTokenSource.Cancel();
		}

		base.OnDestroy();
	}

	public override void Reset()
	{ 
		base.Reset();

		rigidbody = GameObject.Components.GetInDescendantsOrSelf<Rigidbody>(true);
	}
}
