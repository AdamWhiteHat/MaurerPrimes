using System;
using System.Linq;
using System.Numerics;
using System.Collections.Generic;

namespace AlgorithmLibrary
{
	public static class Eratosthenes
	{
		public static TimeSpan TotalExecutionTime { get { return _executionTimer.TotalTime; } }
		private static AggregateTimer _executionTimer { get; }

		private static bool[] _membershipArray;
		private static BigInteger[] _primeCache;
		private static BigInteger _cacheCeiling;
		private static BigInteger _cacheLargestPrimeCurrently;

		static Eratosthenes()
		{
			_executionTimer = new AggregateTimer();
			_cacheCeiling = BigInteger.Pow(11, 7);
			_membershipArray = new bool[0];
			_primeCache = new BigInteger[0];
			//_membershipArray = new bool[] { false, false, true, true, false, true, false, true, false, false, false, true, false, true, false, false, false, true, false, true, false, false, false, true, false, false, false, false, false, true, false, true, false, false, false, false, false, true, false, false, false, true, false, true, false, false, false, true };
			//_primeCache = new BigInteger[] { 2, 3, 5, 7, 11, 13, 17, 19, 23, 29, 31, 37, 41, 43, 47 };
			_primeCache = Sieve(2, (long)_cacheCeiling);
			_cacheLargestPrimeCurrently = _primeCache.Last();
		}

		public static bool EnsurePrimeCacheSize(BigInteger maxPrime)
		{
			BigInteger boundedPrimeRequest = BigInteger.Min(maxPrime, _cacheCeiling);
			if (IsTooLarge(boundedPrimeRequest))
			{
				_primeCache = Sieve(2, (long)boundedPrimeRequest);
				_cacheLargestPrimeCurrently = _primeCache.Last();
				return false;
			}
			return true;
		}

		public static bool IsPrime(BigInteger value)
		{
			Log.MethodEnter("Eratosthenes.IsPrime", nameof(value), value);
			var absP = BigInteger.Abs(value);
			bool existedInCache = EnsurePrimeCacheSize(absP);
			Log.Message($"Prime existed in cache: {existedInCache}");
			Log.MethodLeave();
			return _primeCache.Contains(absP);
		}

		public static bool IsTooLarge(BigInteger value)
		{
			bool result = (value > _cacheLargestPrimeCurrently);
			if (result) { Log.Message($"***Prime cache size exceeded: {value}***"); }
			return result;
		}

		public static BigInteger[] Sieve(int floor, long ceiling)
		{
			if (floor < 2)
			{
				floor = 2;
			}
			using (_executionTimer.StartTimer())
			{
				Log.MethodEnter("Eratosthenes.Sieve", nameof(ceiling), ceiling);

				if (_cacheLargestPrimeCurrently >= ceiling || _membershipArray.Length >= ceiling)
				{
					Log.Message("Primes = [(Cached Value)];");
					Log.MethodLeave();
					return _primeCache;
				}

				long counter = 0;
				long counterStart = 3;
				bool[] primeMembershipArray = new bool[ceiling + 1];

				if (_membershipArray.Length > counterStart /*&& longestprimeMembershipArray.Length < ceiling+1*/)
				{
					long newStart = Math.Min(_membershipArray.Length, ceiling + 1);
					Array.ConstrainedCopy(_membershipArray.ToArray(), 0, primeMembershipArray, 0, (int)newStart);
					counterStart = newStart - 2;
					if (counterStart % 2 != 0)
					{
						counterStart++;
					}
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

				long inc;
				long sqrt = 3;
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


				BigInteger[] result = Enumerable.Range(2, (int)ceiling - 2).Where(l => l >= floor && primeMembershipArray[l]).Select(n => (BigInteger)n).ToArray();

				if (result.Length > _primeCache.Length)
				{
					_primeCache = result;
				}

				if (primeMembershipArray.Length > _membershipArray.Length)
				{
					_membershipArray = primeMembershipArray;
				}

				Log.Message("Primes = [Len:{0}];", result.Length);
				Log.MethodLeave();
				return result;
			}
		}
	}
}
