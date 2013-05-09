using System.Collections.Generic;
using System.IO;
using System.Threading;
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
		IPatterns _patterns;

		[SetUp]
		public void builder()
		{
			_filesystem = Substitute.For<IFileSystem>();
			_filesystem.GetPlatformRoot().Returns(The.Root);

			_filesystem.Exists(null).ReturnsForAnyArgs(true);

			_modules = Substitute.For<IModules>();
			_modules.Paths.Returns(new[] { "group1/proj1", "group1/proj2", "group2/proj3" });
			_modules.Repos.Returns(new[] { "p1Repo", "p2Repo", "p3Repo" });
			_locks = new List<AutoResetEvent> { new AutoResetEvent(true), new AutoResetEvent(true), new AutoResetEvent(true) };
			_modules.CreateAndSetLocks().Returns(_locks);

			_patterns = new Patterns
			{
				DependencyPattern = "*.dll",
				DependencyPath = (FilePath)"lib",
				Masters = new FilePath[] { }
			};

			_git = Substitute.For<IGit>();
			_depMgr = Substitute.For<IDependencyManager>();
			_ruleFac = Substitute.For<IRuleFactory>();
			_ruleFac.GetModules().Returns(_modules);
			_ruleFac.GetRulePatterns().Returns(_patterns);

			_buildCmd = Substitute.For<IBuildCmd>();

			_subject = new Builder(_filesystem, _git, _depMgr, _ruleFac, _buildCmd);
			_subject.Prepare();
			_subject.GetDependenciesAndBuild();
		}

		[Test]
		public void copies_dependencies_to_project()
		{
			_depMgr.Received().CopyBuildResultsTo(The.Root.Navigate((FilePath)"group1/proj1/lib"));
			_depMgr.Received().CopyBuildResultsTo(The.Root.Navigate((FilePath)"group1/proj2/lib"));
			_depMgr.Received().CopyBuildResultsTo(The.Root.Navigate((FilePath)"group2/proj3/lib"));
		}

		[Test]
		public void builds_all_projects()
		{
			_buildCmd.Received().Build(The.Root, The.Root.Navigate((FilePath)"group1/proj1"));
			_buildCmd.Received().Build(The.Root, The.Root.Navigate((FilePath)"group1/proj2"));
			_buildCmd.Received().Build(The.Root, The.Root.Navigate((FilePath)"group2/proj3"));
		}

		[Test]
		public void distributes_new_binaries_for_all_built_projects()
		{
			_depMgr.Received().UpdateAvailableDependencies(The.Root.Navigate((FilePath)"group1/proj1/src"));
			_depMgr.Received().UpdateAvailableDependencies(The.Root.Navigate((FilePath)"group1/proj2/src"));
			_depMgr.Received().UpdateAvailableDependencies(The.Root.Navigate((FilePath)"group2/proj3/src"));
		}
	}
}
