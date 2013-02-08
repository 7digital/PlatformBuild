using System.IO;

namespace PlatformBuild.DependencyManagement
{
	public interface IDependencyManager
	{
		void CopyBuildResultsTo(FilePath dest);
		void UpdateAvailableDependencies(FilePath srcPath);
		void ReadMasters(FilePath rootPath, FilePath[] masters);
	}
}