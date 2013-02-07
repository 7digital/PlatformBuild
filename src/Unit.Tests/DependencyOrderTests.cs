using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using PlatformBuild;

namespace Unit.Tests
{
	[TestFixture]
	public class DependencyOrderTests
	{
		Modules _subject;

		[SetUp]
		public void setup()
		{
			_subject = new Modules(null, null);
			_subject.Repos = new []{"g_4", "g_5", "g_1", "g_3", "g_2", "g_6"};
			_subject.Paths = new []{
				"four",//0
				"five",//1
				"one", //2
				"three",//3
				"two",//4
				"six"//5
			};
			_subject.Deps = new[]{
				new List<int>{4,3},
				new List<int>{0,3,2},
				new List<int>{},
				new List<int>{4,2},
				new List<int>{},
				new List<int>{2,1,0}
			};
		}

		[Test]
		public void sorts_paths_in_order()
		{
			_subject.SortInDependencyOrder();
			Assert.That(_subject.Paths, Is.EquivalentTo(new[]{"one", "two", "three", "four", "five", "six"}));
		}
		[Test]
		public void sorts_repos_in_order()
		{
			_subject.SortInDependencyOrder();
			Assert.That(_subject.Repos, Is.EquivalentTo(new[]{"g_1", "g_2", "g_3", "g_4", "g_5", "g_6"}));
		}
	}
}
