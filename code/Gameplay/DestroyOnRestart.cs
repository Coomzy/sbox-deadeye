
using Sandbox.Citizen;

public class DestroyOnRestart : Component, IRestartable, IShutdown
{
	public void PreRestart()
	{
		GameObject.Destroy();
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
}
