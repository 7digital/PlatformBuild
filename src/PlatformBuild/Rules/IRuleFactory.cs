using System.Collections.Generic;
using System.IO;

namespace PlatformBuild.Rules
{
	public interface IRuleFactory
	{
		IModules GetModules();
        IPatterns GetRulePatterns();
		IEnumerable<FilePath> GetPathsToDelete();
	}
}