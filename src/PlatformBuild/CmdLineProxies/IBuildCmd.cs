using System.IO;

namespace PlatformBuild
{
	public interface IBuildCmd
	{
		int Build(FilePath buildPath);
	}
}