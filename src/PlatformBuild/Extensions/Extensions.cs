using System.Collections.Generic;

namespace PlatformBuild.Crap
{
	public static class Extensions
	{
		public static int Index<T>(this T[] src, T item)
		{
			for (int i = 0; i < src.Length; i++)
			{
				if (Equals(src[i], item)) return i;
			}
			return -1;
		}

        public static TValue Of<TKey,TValue>(this IDictionary<TKey, TValue> src, TKey key)
        {
	        return !src.ContainsKey(key) ? default(TValue) : src[key];
        }
	}
}