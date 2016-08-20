using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AlgorithmLibrary
{
	public class ThreadedAlgorithmWorker
	{		
		
		public object Argument { get; private set; }
		public BigInteger Result { get; private set; }
		public TimeSpan RuntimeTimer { get; private set; }
		public Func<CancellationToken, object, BigInteger> DoWorkFunc;
		public event RunWorkerCompletedEventHandler WorkerComplete;
		public int CompositeSearchDepth { get; set; } = 4;

		
		private DateTime startTime;
		private Algorithm algorithm;
		private BackgroundWorker bgWorker;
		private CancellationToken cancelToken;
		private bool LoggingEnabled;

		public ThreadedAlgorithmWorker(int compositeSearchDepth, bool loggingEnabled = false)
		{
			RuntimeTimer = TimeSpan.Zero;
			CompositeSearchDepth = compositeSearchDepth;
			bgWorker = new BackgroundWorker();
			bgWorker.DoWork += bgWorker_DoWork;
			bgWorker.RunWorkerCompleted += bgWorker_RunWorkerCompleted;
			bgWorker.WorkerSupportsCancellation = true;
			this.LoggingEnabled = loggingEnabled;
		}

		public void StartWorker(CancellationToken cancellationToken, object argument)
		{
			if (cancellationToken != null)
			{
				cancelToken = cancellationToken;
			}
			Argument = argument;

			startTime = new DateTime();
			startTime = DateTime.Now;
			bgWorker.RunWorkerAsync(Argument);
		}

		private void OnWorkerComplete(object result, Exception error, bool cancelled)
		{
			RunWorkerCompletedEventHandler handler = WorkerComplete;
			if (handler != null)
			{
				RunWorkerCompletedEventArgs args = new RunWorkerCompletedEventArgs(result, error, cancelled);
				handler(this, args);
			}
		}

		public BigInteger DoWork_FindPrime(CancellationToken token, object argument)
		{
			algorithm = new Algorithm(token, LoggingEnabled);
			return algorithm.ProvablePrime((int)argument, CompositeSearchDepth);
		}

		private void bgWorker_DoWork(object sender, DoWorkEventArgs e)
		{
			Result = DoWorkFunc.Invoke(cancelToken, e.Argument);

			if (cancelToken.IsCancellationRequested)
			{
				e.Cancel = true;
			}
			else
			{
				e.Result = Result;
			}
		}

		private void bgWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			if (startTime != DateTime.MinValue)
			{
				RuntimeTimer = DateTime.Now.Subtract(startTime);
			}

			if (cancelToken.IsCancellationRequested)
			{
				OnWorkerComplete(null, null, true);
			}
			else
			{
				OnWorkerComplete(e.Result, e.Error, e.Cancelled);
			}
			if (algorithm != null)
			{
				algorithm.Dispose();
			}
		}

		public static string FormatTimeSpan(TimeSpan timeSpan)
		{
			if (timeSpan.TotalMilliseconds < 1)
			{
				return "0.0";
			}

			StringBuilder result = new StringBuilder();
			if (timeSpan.Days > 0) result.Append(timeSpan.Days.ToString().PadLeft(2)).Append("d ");
			if (timeSpan.Hours > 0) result.Append(timeSpan.Hours.ToString().PadLeft(2)).Append("h ");
			if (timeSpan.Minutes > 0) result.Append(timeSpan.Minutes.ToString().PadLeft(2)).Append("m ");
			if (timeSpan.Seconds > 0)
			{
				result.Append(timeSpan.Seconds.ToString().PadLeft(2));
				if (timeSpan.Milliseconds > 0)
				{
					result.AppendFormat(".{0}", timeSpan.Milliseconds.ToString().PadLeft(3, '0').Substring(0, 1));
				}
				result.Append("s");
			}

			if (timeSpan.TotalMilliseconds < 1000) result.Append(timeSpan.Milliseconds.ToString().PadLeft(3)).Append("ms");
			return result.ToString();
		}
	}
}
