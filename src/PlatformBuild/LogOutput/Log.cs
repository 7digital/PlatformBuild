using System;

namespace PlatformBuild.LogOutput
{
	public static class Log
	{
        static int level = 0;

        // 0 -> errors only
        // 1 -> status
        // 2 -> info
        // 3 -> verbose

        static readonly object _writeLock = new Object();

        public static void SetLevel(string l)
        {
           level = stringLevel(l);
        }
        static int stringLevel(string l)
        {
            switch (l.ToLowerInvariant())
            {
                case "status": return 1;
				case "info": return 2;
				case "verbose": return 3;
				case "error": return 0;

				default: return 1;
            }
        }

		public static void Verbose(string message)
        {
            if (level < 3) return;
			lock (_writeLock)
			{
                Console.ForegroundColor = ConsoleColor.Gray;
				Console.WriteLine(message);
                Console.ResetColor();
			}
        }

		public static void Status(string message)
        {
            if (level < 1) return;
			lock (_writeLock)
			{
                Console.ForegroundColor = ConsoleColor.DarkCyan;
				Console.WriteLine(message);
                Console.ResetColor();
			}
        }
        
		public static void Info(string message)
        {
            if (level < 2) return;
			lock (_writeLock)
			{
                Console.ForegroundColor = ConsoleColor.White;
				Console.WriteLine(message);
                Console.ResetColor();
			}
        }
		
		public static void Error(string message)
        {
			lock (_writeLock)
			{
                Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine(message);
                Console.ResetColor();
			}
        }

	}
}
