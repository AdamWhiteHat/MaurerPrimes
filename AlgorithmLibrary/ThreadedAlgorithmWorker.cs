using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


namespace AlgorithmLibrary.MaurerPrimes
{
	public class ThreadedAlgorithmWorker
	{
		public TimeSpan RunTime { get; private set; }
		public event RunWorkerCompletedEventHandler WorkerComplete;
		public bool LoggingEnabled { get; set; }

		private int bits;
		private DateTime startTime;
		private Algorithm algorithm;
		private BackgroundWorker bgWorker;
		private CancellationToken cancelToken;

		public ThreadedAlgorithmWorker(int bitSize)
		{
			bits = bitSize;
			RunTime = TimeSpan.Zero;
			
			bgWorker = new BackgroundWorker();
			bgWorker.DoWork += bgWorker_DoWork;
			bgWorker.RunWorkerCompleted += bgWorker_RunWorkerCompleted;
			bgWorker.WorkerSupportsCancellation = true;
		}

		public void StartWorker(CancellationToken cancellationToken)
		{
			if (cancellationToken != null)
			{
				cancelToken = cancellationToken;
			}

			startTime = new DateTime();
			startTime = DateTime.Now;
			bgWorker.RunWorkerAsync(bits);
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

		private void bgWorker_DoWork(object sender, DoWorkEventArgs e)
		{
			int argument = (int)e.Argument;
			algorithm = new Algorithm(cancelToken);
			algorithm.LoggingEnabled = this.LoggingEnabled;
			BigInteger result = algorithm.ProvablePrime(argument);			

			if (cancelToken.IsCancellationRequested)
			{
				e.Cancel = true;
			}
			else
			{
				e.Result = result;
			}
		}

		private void bgWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			if (startTime != DateTime.MinValue)
			{
				RunTime = DateTime.Now.Subtract(startTime);
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
