using System.IO;

namespace PlatformBuild.Rules
{
	public class Patterns : IPatterns
	{
		public FilePath[] Masters { get; set; }
		public FilePath DependencyPath { get; set; }
		public string DependencyPattern { get; set; }
		public string BuildPattern { get; set; }
		public string BuildCmd { get; set; }
		public CopyPath[] CopyPaths { get; set; }
	}
}