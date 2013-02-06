namespace PlatformBuild
{
	public static class ArrayExtensions
	{
		public static int Index<T>(this T[] src, T item)
		{
			for (int i = 0; i < src.Length; i++)
			{
				if (Equals(src[i], item)) return i;
			}
			return -1;
		}
	}
}