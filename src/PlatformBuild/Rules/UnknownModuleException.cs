using System;

namespace PlatformBuild.Rules
{
	public class UnknownModuleException : Exception
	{
		public UnknownModuleException(string s):base(s)
		{
		}
	}
}