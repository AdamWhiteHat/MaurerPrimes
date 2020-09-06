using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AlgorithmLibrary
{
	public class AggregateTimer
	{
		private object totalTimeLockObject;
		public TimeSpan TotalTime { get; private set; }

		public AggregateTimer()
		{
			totalTimeLockObject = new object();
			TotalTime = TimeSpan.Zero;
		}

		public TimerToken StartTimer()
		{
			return new TimerToken(this);
		}

		private void AddTime(TimeSpan time)
		{
			lock (totalTimeLockObject)
			{
				TotalTime = TotalTime.Add(time);
			}
		}

		public class TimerToken : IDisposable
		{
			private AggregateTimer parent;
			private DateTime startTime;

			internal TimerToken(AggregateTimer parentTimer)
			{
				parent = parentTimer;
				startTime = DateTime.UtcNow;
			}

			public void Dispose()
			{
				parent.AddTime(DateTime.UtcNow.Subtract(startTime));
				parent = null;
			}
		}

		public static string FormatTimeSpan(TimeSpan timeSpan)
		{
			if (timeSpan.TotalMilliseconds < 1)
			{
				return "0.0";
			}
			if (timeSpan.TotalMilliseconds < 1000)
			{
				return string.Concat(timeSpan.Milliseconds.ToString().PadLeft(3), "ms ");
			}

			StringBuilder result = new StringBuilder();
			if (timeSpan.Days > 0) { result.Append(timeSpan.Days.ToString().PadLeft(2)).Append("d "); }
			if (timeSpan.Hours > 0) { result.Append(timeSpan.Hours.ToString().PadLeft(2)).Append("h "); }
			if (timeSpan.Minutes > 0) { result.Append(timeSpan.Minutes.ToString().PadLeft(2)).Append("m "); }
			if (timeSpan.Seconds > 0)
			{
				result.Append(timeSpan.Seconds.ToString().PadLeft(2));
				if (timeSpan.Milliseconds > 0)
				{
					result.AppendFormat(".{0}", timeSpan.Milliseconds.ToString().PadLeft(3, '0').Substring(0, 1));
				}
				result.Append("s");
			}

			return result.ToString();
		}
	}
}
