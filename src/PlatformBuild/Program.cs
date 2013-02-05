namespace PlatformBuild
{
	public class Program
	{
		static void Main()
		{
			new Builder(new RealFileSystem()).Build();
		}
	}
}
