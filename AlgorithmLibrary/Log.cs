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
		private static DepthCounter depth; // Use a class instead of mutating a static variable

		static Log()
		{
			loggingEnabled = false;
			filename = "Methods.log.txt";
			depth = new DepthCounter();
		}

		public static void SetLoggingPreference(bool enabled)
		{
			// Yes, I'm modifying a static member. 
			// This is okay as long as you understand that
			// ALL threads can change and are effected by this variable
			loggingEnabled = enabled;
		}

		public static void MethodEnter(string methodName, params object[] args)
		{
			if (loggingEnabled)
			{
				Message("{0}({1})", methodName, string.Join(", ", args));
				Message("{");				
			}
			// If we turn logging off for a while, then back on
			// we still want the depth to be correct when you turn it back on.
			// Otherwise it doesn't line up at the end.
			depth.Increase(); 
		}

		public static void MethodLeave()
		{
			depth.Decrease();
			if (loggingEnabled)
			{
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

		// As stated above, 
		// its poor form to keep track of state by modifying a static member.
		// An alternative would be to have the static member hold an instance of a class.
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
