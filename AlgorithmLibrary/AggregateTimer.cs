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
	}


}
