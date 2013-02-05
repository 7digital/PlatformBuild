using PlatformBuild.FileSystem;
using PlatformBuild.GitVCS;

namespace PlatformBuild
{
	public class Builder
	{
		readonly IFileSystem _files;
		readonly IGit _git;

		public Builder(IFileSystem files, IGit git)
		{
			_files = files;
			_git = git;
		}

		public void Build()
		{
			var baseDir = _git.ParentRepo(_files.GetExeDirectory());
			//using (var repo = new Repository(
		}
	}
}