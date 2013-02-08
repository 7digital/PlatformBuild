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
	public class Builder_Pulling
	{
		
		Builder _subject;
		IFileSystem _filesystem;
		IGit _git;
		IDependencyManager _depMgr;
		IRuleFactory _ruleFac;
		IModules _modules;
		IList<AutoResetEvent> _locks;

		[SetUp]
		public void builder()
		{
			_filesystem = Substitute.For<IFileSystem>();
			_filesystem.GetPlatformRoot().Returns(The.Root);

            _filesystem.Exists(null).ReturnsForAnyArgs(c=> (c.Args()[0] as FilePath != (FilePath)"group1/proj2"));

			_modules = Substitute.For<IModules>();
			_modules.Paths.Returns(new[] {"group1/proj1", "group1/proj2", "group2/proj3"});
			_modules.Repos.Returns(new[] {"p1Repo", "p2Repo", "p3Repo"});
			_locks = new List<AutoResetEvent> { new AutoResetEvent(false), new AutoResetEvent(false), new AutoResetEvent(false) };
			_modules.CreateAndSetLocks().Returns(_locks);

			_git = Substitute.For<IGit>();
			_depMgr = Substitute.For<IDependencyManager>();
			_ruleFac = Substitute.For<IRuleFactory>();
			_ruleFac.GetModules().Returns(_modules);

			_subject = new Builder(_filesystem, _git, _depMgr, _ruleFac, null);
			_subject.Prepare();
			_subject.PullRepos();
		}

		[Test]
		public void resets_all_lib_folders()
		{
			_git.Received(1).Reset(The.Root.Navigate((FilePath)"group1/proj1/lib"));
			_git.Received(1).Reset(The.Root.Navigate((FilePath)"group1/proj2/lib"));
			_git.Received(1).Reset(The.Root.Navigate((FilePath)"group2/proj3/lib"));
		}

		[Test]
		public void pulls_changes ()
		{
			_git.Received(1).PullCurrentBranch(The.Root.Navigate((FilePath)"group1/proj1"));
			_git.Received(1).PullCurrentBranch(The.Root.Navigate((FilePath)"group1/proj2"));
			_git.Received(1).PullCurrentBranch(The.Root.Navigate((FilePath)"group2/proj3"));
		}

		[Test]
		public void sets_all_wait_events()
		{
			Assert.That(_locks[0].WaitOne(10), Is.True);
			Assert.That(_locks[1].WaitOne(10), Is.True);
			Assert.That(_locks[2].WaitOne(10), Is.True);
		}
	}
}
