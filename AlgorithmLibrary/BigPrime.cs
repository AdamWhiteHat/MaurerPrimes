using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace AlgorithmLibrary
{
	public class BigPrime
	{
		private static int QuickTestCount;
		private static int FullTestCount;

		static BigPrime()
		{
			QuickTestCount = 2;
			FullTestCount = 9;
		}

		public static BigInteger GetNextPrime(BigInteger fromValue)
		{
			bool isPrime = false;
			BigInteger currentValue = fromValue % 2 == 0 ? fromValue + 1 : fromValue + 2;
			while (!isPrime)
			{
				isPrime = MillerRabin.IsProbablyPrime(currentValue, QuickTestCount); // Test just a few bases here, as a quick elimination test

				if (isPrime)
				{
					isPrime = MillerRabin.IsProbablyPrime(currentValue, FullTestCount); // Test more bases here to ensure candidate is really prime
				}

				currentValue += 2;
			}

			return BigInteger.MinusOne;
		}

		public static BigInteger GetPreviousPrime(BigInteger fromValue)
		{
			bool isPrime = false;
			BigInteger currentValue = fromValue % 2 == 0 ? fromValue - 1 : fromValue - 2;
			while (!isPrime)
			{


				currentValue += 2;
			}

			return BigInteger.MinusOne;
		}

		private static bool TestForPrimality(BigInteger testValue)
		{
			bool result = MillerRabin.IsProbablyPrime(testValue, QuickTestCount); // Test just a few bases here, as a quick elimination test

			if (result)
			{
				return MillerRabin.IsProbablyPrime(testValue, FullTestCount); // Test more bases here to ensure candidate is really prime
			}
			else
			{
				return false;

			}
		}
	}
}
