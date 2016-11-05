using System;
using System.Linq;
using System.Collections.Generic;

namespace AlgorithmLibrary
{
	public static class Eratosthenes
	{
		public static TimeSpan TotalExecutionTime { get { return executionTimer.TotalTime; } }
		private static AggregateTimer executionTimer { get; }

		private static List<long> longestSieve;
		private static List<bool> longestprimeMembershipArray;
		
		static Eratosthenes()
		{
			longestSieve = new List<long>();
			longestprimeMembershipArray = new List<bool>();
			executionTimer = new AggregateTimer();
		}

        /// <summary>
        /// Sieve of Eratosthenes. Find all prime numbers less than or equal ceiling
        /// </summary>
        public static List<long> Sieve(long ceiling)
        {
            return Sieve(2, ceiling);
        }

        public static List<long> Sieve(int floor, long ceiling)
		{
            if(floor < 2)
            {
                floor = 2;
            }
			using (executionTimer.StartTimer())
			{
				Log.MethodEnter("Eratosthenes.Sieve", ceiling);

				long cacheMaxValue = 0;
				if (longestSieve.Count > 0)
				{
					cacheMaxValue = longestSieve.Last();
				}

				if (cacheMaxValue >= ceiling || longestprimeMembershipArray.Count >= ceiling)
				{
					Log.Message("Primes = [(Cached Value)];");
					Log.MethodLeave();
					return longestSieve.TakeWhile(l => l < ceiling).ToList();
				}

				long counter = 0;
				long counterStart = 3;
				long inc;
				long sqrt = 3;
				bool[] primeMembershipArray = new bool[ceiling + 1];

                if (longestprimeMembershipArray.Count > counterStart /*&& longestprimeMembershipArray.Length < ceiling+1*/)
                {
                    Array.ConstrainedCopy(longestprimeMembershipArray.ToArray(), 0, primeMembershipArray, 0, (int)Math.Min(longestprimeMembershipArray.Count, ceiling + 1));
                    //counterStart = longestprimeMembershipArray.Count - 2;
                }
            
                    primeMembershipArray[2] = true;
                                
                // Set all odds as true
				for (counter = counterStart; counter <= ceiling; counter += 2)
				{
					if ((counter & 1) == 1)//% 2 == 1) // Check if odd
					{
						primeMembershipArray[counter] = true;
					}
				}

				do
				{
					counter = sqrt * sqrt;
					inc = sqrt + sqrt;
                                       
                    while (counter <= ceiling)
					{
						primeMembershipArray[counter] = false;
						counter += inc;
					}

					sqrt += 2;

					while (!primeMembershipArray[sqrt])
					{
						sqrt++;
					}
				} while (sqrt * sqrt <= ceiling);


                List<long> result = Enumerable.Range(2, (int)ceiling - 2).Select(n => (long)n).Where(l => l >= floor && primeMembershipArray[l]).ToList();

				if (result.Count > longestSieve.Count)
				{
					longestSieve = result;
				}

				if (primeMembershipArray.Length > longestprimeMembershipArray.Count)
				{
					longestprimeMembershipArray = primeMembershipArray.ToList();
				}

				Log.Message("Primes = [Len:{0}];", result.Count);
				Log.MethodLeave();
				return result;
			}
		}
	}
}
