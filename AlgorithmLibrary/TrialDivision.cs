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

			Log.MethodEnter("TrialDivision.CheckForSmallComposites", bits);

			using (executionTimer.StartTimer())
			{
				BigInteger result = 0;
				List<long> lucky = null;

				long n = 0;
				long upperBound = 0;
				bool composite = true;
				while (composite)
				{
					n = 1 << (bits - 1);

					for (int i = 0; i < bits - 1; i++)
					{
						n |= CryptoRandomSingleton.Next(2) << i;
					}

					upperBound = (long)Math.Sqrt(n);
					lucky = Eratosthenes.Sieve(upperBound);
					composite = lucky.Any(l => ((n % l) == 0));

					if (!composite)
					{
						result = n;
					}
				}

				Log.MethodLeave();
				return result;
			}
		}
	}
}
