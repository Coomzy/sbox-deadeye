
using Sandbox.Audio;
using Sandbox;

public class PlayerFootsteps : Component
{
	[Property] SkinnedModelRenderer Source { get; set; }

	protected override void OnEnabled()
	{
		if (Source is null)
			return;

		Source.OnFootstepEvent += OnEvent;
	}

	protected override void OnDisabled()
	{
		if (Source is null)
			return;

		Source.OnFootstepEvent -= OnEvent;
	}

	TimeSince timeSinceStep;

	void OnEvent(SceneModel.FootstepEvent e)
	{
		if (timeSinceStep < 0.2f)
			return;

		var tr = Scene.Trace
			.Ray(e.Transform.Position + Vector3.Up * 20, e.Transform.Position + Vector3.Up * -20)
			.Run();

		if (!tr.Hit)
			return;

		if (tr.Surface is null)
			return;

		timeSinceStep = 0;

		var sound = e.FootId == 0 ? tr.Surface.Sounds.FootLeft : tr.Surface.Sounds.FootRight;
		if (sound is null) return;

		var soundHandle = Sound.Play(sound, tr.HitPosition + tr.Normal * 5);
		soundHandle.Volume *= e.Volume;
		soundHandle.DistanceAttenuation = false;
		soundHandle.Occlusion = false;
		soundHandle.SpacialBlend = 0.0f;
		soundHandle.TargetMixer = Mixer.FindMixerByName("Game");
	}
}
