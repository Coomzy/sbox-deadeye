
using Sandbox.Citizen;

public interface IShutdown
{
	void PreShutdown();
	void PostShutdown();
}
