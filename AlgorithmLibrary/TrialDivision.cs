using System;
using System.Linq;
using System.Numerics;
using System.Collections.Generic;

namespace AlgorithmLibrary
{
	public static class TrialDivision
	{
		public static TimeSpan TotalExecutionTime { get { return executionTimer.TotalTime; } }
		private static AggregateTimer executionTimer = new AggregateTimer();

		public static BigInteger FindSmallPrimes(int bits)
		{
			if (bits > 20)
			{
				throw new ArgumentException("bits > 20");
			}

			Log.MethodEnter("TrialDivision.CheckForSmallComposites", nameof(bits), bits);
			BigInteger result = 0;

			using (executionTimer.StartTimer())
			{
				long n = 0;
				bool composite = true;
				while (composite)
				{
					n = 1 << (bits - 1);

					for (int i = 0; i < bits - 1; i++)
					{
						n |= CryptoRandomSingleton.Next(2) << i;
					}

					composite = !Eratosthenes.IsPrime(n);
				}
				result = n;
			}

			Log.MethodLeave();
			return result;
		}
	}
}
