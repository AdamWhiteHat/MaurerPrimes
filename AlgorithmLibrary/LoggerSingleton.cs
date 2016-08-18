using System;
using System.IO;

namespace AlgorithmLibrary
{
	public static class LoggerSingleton
	{
		private static string _filename;
		static LoggerSingleton() { _filename = "Methods.log.txt"; }

		public static void Log(string message)
		{			
			File.AppendAllText(_filename, message + Environment.NewLine);
		}

		public static void Log(string format, params object[] args)
		{
			if (args == null)
			{
				if (string.IsNullOrWhiteSpace(format))
				{
					Log(string.Empty);
				}
				else
				{
					Log(format);
				}
			}
			else
			{
				Log(string.Format(format, args));
			}
		}
	}
}
