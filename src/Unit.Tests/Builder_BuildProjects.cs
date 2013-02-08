using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NSubstitute;
using NUnit.Framework;
using PlatformBuild;
using PlatformBuild.CmdLineProxies;
using PlatformBuild.DependencyManagement;
using PlatformBuild.FileSystem;
using PlatformBuild.Rules;

namespace Unit.Tests
{
	[TestFixture]
	public class Builder_BuildProjects
	{
		
		Builder _subject;
		IFileSystem _filesystem;
		IGit _git;
		IDependencyManager _depMgr;
		IRuleFactory _ruleFac;
		IModules _modules;
		IList<AutoResetEvent> _locks;
		IBuildCmd _buildCmd;

		[SetUp]
		public void builder()
		{
			_filesystem = Substitute.For<IFileSystem>();
			_filesystem.GetPlatformRoot().Returns(The.Root);

			var skipBuild = The.Root.Navigate((FilePath)"group1/proj2/build");
            _filesystem.Exists(null).ReturnsForAnyArgs(c=> c.Args()[0] as FilePath != skipBuild);

			_modules = Substitute.For<IModules>();
			_modules.Paths.Returns(new[] {"group1/proj1", "group1/proj2", "group2/proj3"});
			_modules.Repos.Returns(new[] {"p1Repo", "p2Repo", "p3Repo"});
			_locks = new List<AutoResetEvent> { new AutoResetEvent(true), new AutoResetEvent(true), new AutoResetEvent(true) };
			_modules.CreateAndSetLocks().Returns(_locks);

			_git = Substitute.For<IGit>();
			_depMgr = Substitute.For<IDependencyManager>();
			_ruleFac = Substitute.For<IRuleFactory>();
			_ruleFac.GetModules().Returns(_modules);

			_buildCmd = Substitute.For<IBuildCmd>();

			_subject = new Builder(_filesystem, _git, _depMgr, _ruleFac, _buildCmd);
			_subject.Prepare();
			_subject.GetDependenciesAndBuild();
		}

		[Test]
		public void copies_dependencies_to_project ()
		{
			_depMgr.Received().CopyBuildResultsTo(The.Root.Navigate((FilePath)"group1/proj1/lib"));
			_depMgr.Received().CopyBuildResultsTo(The.Root.Navigate((FilePath)"group1/proj2/lib"));
			_depMgr.Received().CopyBuildResultsTo(The.Root.Navigate((FilePath)"group2/proj3/lib"));
		}

		[Test]
		public void builds_all_projects_with_a_build_folder ()
		{
			_buildCmd.Received().Build(The.Root.Navigate((FilePath)"group1/proj1/build"));
			_buildCmd.Received().Build(The.Root.Navigate((FilePath)"group2/proj3/build"));
			_buildCmd.DidNotReceive().Build(The.Root.Navigate((FilePath)"group1/proj2/build"));
		}

		[Test]
		public void distributes_new_binaries_for_all_built_projects ()
		{
			_depMgr.Received().UpdateAvailableDependencies(The.Root.Navigate((FilePath)"group1/proj1/src"));
			_depMgr.Received().UpdateAvailableDependencies(The.Root.Navigate((FilePath)"group2/proj3/src"));
			_depMgr.DidNotReceive().UpdateAvailableDependencies(The.Root.Navigate((FilePath)"group1/proj2/src"));
		}
	}
}
