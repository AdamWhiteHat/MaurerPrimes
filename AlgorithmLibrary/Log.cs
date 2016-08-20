using System;
using System.IO;
using System.Linq;

namespace AlgorithmLibrary
{
	public sealed class Log
	{
		//private static int recursionDepthCount;
		private static bool loggingEnabled;
		private static string filename;
		private static DepthCounter depth;

		static Log()
		{
			loggingEnabled = false;
			filename = "Methods.log.txt";
			depth = new DepthCounter();
		}

		public static void SetLoggingPreference(bool enabled)
		{
			loggingEnabled = enabled;
		}

		public static void MethodEnter(string methodName, params object[] args)
		{
			if (loggingEnabled)
			{
				Message("{0}({1})", methodName, string.Join(", ", args));
				Message("{");
				depth.Increase();
			}
		}

		public static void MethodLeave()
		{
			if (loggingEnabled)
			{
				depth.Decrease();
				Message("}");
			}
		}

		public static void Message(string format, params object[] args)
		{
			if (loggingEnabled)
			{
				if (args == null)
				{
					if (string.IsNullOrWhiteSpace(format))
					{
						Message(string.Empty);
					}
					else
					{
						Message(format);
					}
				}
				else
				{
					Message(string.Format(format, args));
				}
			}
		}

		public static void Message(string message)
		{
			if (loggingEnabled)
			{
				string toLog = message.Replace("\n", "\n" + depth.GetPadding());
				File.AppendAllText(filename, depth.GetPadding() + toLog + Environment.NewLine);
			}
		}

		private class DepthCounter
		{
			private int _depth;

			public DepthCounter()
			{
				_depth = 0;
			}

			public void Increase()
			{
				_depth++;
			}

			public void Decrease()
			{
				_depth--;
			}

			public string GetPadding()
			{
				return (_depth < 1) ? "" : new string(Enumerable.Repeat('\t', _depth).ToArray());
			}
		}
	}
}
