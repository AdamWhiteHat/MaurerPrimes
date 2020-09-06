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
	public class ThreadedAlgorithmWorker : IDisposable
	{
		public bool IsDisposed = false;
		public object Argument { get; private set; }
		public BigInteger Result { get; private set; }
		public TimeSpan RuntimeTimer { get; private set; }
		public Func<CancellationToken, object, BigInteger> DoWorkFunc;
		public event RunWorkerCompletedEventHandler WorkerComplete;
		public int CompositeSearchDepth { get; set; } = 4;

		private DateTime startTime;
		private CryptographicPrimeGenerator algorithm;
		private BackgroundWorker bgWorker;
		private CancellationToken cancelToken;
		private bool LoggingEnabled;

		public ThreadedAlgorithmWorker(int compositeSearchDepth, bool loggingEnabled = false)
		{
			RuntimeTimer = TimeSpan.Zero;
			CompositeSearchDepth = compositeSearchDepth;
			this.LoggingEnabled = loggingEnabled;
		}

		public void Dispose()
		{
			if (!IsDisposed)
			{
				IsDisposed = true;

				if (bgWorker != null)
				{
					bgWorker.DoWork -= bgWorker_DoWork;
					bgWorker.RunWorkerCompleted -= bgWorker_RunWorkerCompleted;
					bgWorker.Dispose();
				}

				if (DoWorkFunc != null)
				{
					DoWorkFunc = null;
				}

				if (Argument != null)
				{
					Argument = null;
				}
			}
		}

		public void StartWorker(CancellationToken cancellationToken, object argument)
		{
			if (cancellationToken != null)
			{
				cancelToken = cancellationToken;
			}
			Argument = argument;

			bgWorker = new BackgroundWorker();
			bgWorker.WorkerSupportsCancellation = true;
			bgWorker.DoWork += bgWorker_DoWork;
			bgWorker.RunWorkerCompleted += bgWorker_RunWorkerCompleted;

			startTime = new DateTime();
			startTime = DateTime.Now;
			bgWorker.RunWorkerAsync(Argument);
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

		public BigInteger DoWork_FindPrime(CancellationToken token, object argument)
		{
			algorithm = new CryptographicPrimeGenerator(token, LoggingEnabled);
			return algorithm.GetProbablePrime((int)argument, CompositeSearchDepth);
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
	}
}
