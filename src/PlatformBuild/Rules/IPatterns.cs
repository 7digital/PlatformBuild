using System.IO;

namespace PlatformBuild.Rules
{
	public interface IPatterns
	{
		FilePath[] Masters { get; }
		FilePath DependencyPath { get; }
		string DependencyPattern { get; }
	}
}
