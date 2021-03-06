﻿using System;
using System.Numerics;
using System.Threading;
using System.Collections.Generic;
using AlgorithmLibrary;
using System.Diagnostics;

namespace MaurerConsole
{
	internal enum WorkerResultType
	{
		None,
		Error,
		Success,
		Canceled
	}

	internal class ConsoleWorker
	{
		public TimeSpan RunTime { get; private set; }
		public WorkerResultType Result { get; private set; }
		public bool IsBusy { get { return _isBusy; } private set { _isBusy = value; } }
		private volatile bool _isBusy = false;

		private CancellationToken cancelToken;
		private CancellationTokenSource cancelSource;
		private ThreadedAlgorithmWorker algorithmWorker;

		private Queue<Exception> errors;
		private Queue<BigInteger> primes;
		private Queue<WorkerResultType> results;

		private Stopwatch primeFindTimer;
		public TimeSpan primeFindTimeElapsed;

		public ConsoleWorker(bool loggingEnabled)
		{
			errors = new Queue<Exception>();
			primes = new Queue<BigInteger>();
			results = new Queue<WorkerResultType>();			

			Result = WorkerResultType.None;

			algorithmWorker = new ThreadedAlgorithmWorker(Settings.Search_Depth, loggingEnabled);
			algorithmWorker.DoWorkFunc = algorithmWorker.DoWork_FindPrime;
			algorithmWorker.WorkerComplete += algorithmWorker_WorkerComplete;
			
			this.RunTime = algorithmWorker.RuntimeTimer;

			primeFindTimer = new Stopwatch();
		}

		public Exception RemoveErrorResult()
		{
			Exception ex = null;
			if (Result == WorkerResultType.Error && errors.Count > 0)
			{
				ex = errors.Dequeue();
				Result = WorkerResultType.None;
				SetNextResult();
			}
			return ex;
		}

		public BigInteger RemoveSuccessResult()
		{
			BigInteger bi = BigInteger.MinusOne;
			if (Result == WorkerResultType.Success && primes.Count > 0)
			{
				bi = primes.Dequeue();
				Result = WorkerResultType.None;
				SetNextResult();
			}
			return bi;
		}

		private void SetNextResult()
		{
			if (results.Count < 1)
			{
				Result = WorkerResultType.None;
			}
			else if (Result == WorkerResultType.None && results.Count > 0)
			{
				Result = results.Dequeue();
			}
		}

		public void CancelWorker()
		{
			cancelSource.Cancel();			
			this.results.Enqueue(WorkerResultType.Canceled);
			SetNextResult();
		}

		public bool StartWorker(int primeBitSize)
		{
			if (!IsBusy)
			{
				IsBusy = true;

				if (cancelSource != null)
				{
					cancelSource.Dispose();
					cancelSource = null;
				}
				if (cancelSource == null)
				{
					NewCancelTokenAndSource();
				}

				primeFindTimer.Reset();
				primeFindTimer.Start();
				algorithmWorker.StartWorker(cancelToken, primeBitSize);
				return true;
			}
			return false;
		}

		private void NewCancelTokenAndSource()
		{
			cancelSource = new CancellationTokenSource();
			cancelToken = cancelSource.Token;
		}

		private void algorithmWorker_WorkerComplete(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
		{
			primeFindTimer.Stop();
			primeFindTimeElapsed = primeFindTimer.Elapsed;

			if (e.Error != null)
			{
				this.errors.Enqueue(e.Error);
				this.results.Enqueue(WorkerResultType.Error);
				SetNextResult();
			}
			else if (e.Cancelled)
			{
			}
			else
			{
				BigInteger prime = (BigInteger)e.Result;

				this.RunTime = algorithmWorker.RuntimeTimer;

				this.primes.Enqueue(prime);
				this.results.Enqueue(WorkerResultType.Success);				
				SetNextResult();
			}

			IsBusy = false;
		}
	}
}
