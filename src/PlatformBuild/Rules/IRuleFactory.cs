namespace PlatformBuild.Rules
{
	public interface IRuleFactory
	{
		IModules GetModules();
        IPatterns GetRulePatterns();
	}
}