namespace PlatformBuild.Rules
{
	public interface IRuleFactory
	{
		string DependencyPattern { get; }
		IModules GetModules();
	}
}