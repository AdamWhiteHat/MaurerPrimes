﻿using System;
using System.Linq;
using System.Collections.Generic;

namespace AlgorithmLibrary
{
	public class Eratosthenes
	{
		private static List<long> longestSieve;
		private static List<bool> longestprimeMembershipArray;

		static Eratosthenes()
		{
			longestSieve = new List<long>();
			longestprimeMembershipArray = new List<bool>();
		}
		
		/// <summary>
		/// Sieve of Eratosthenes. Find all prime numbers less than or equal ceiling
		/// </summary>
		public static List<long> Sieve(long ceiling)
		{
			LoggerSingleton.Log("Sieve("+ceiling+");");

			long cacheMaxValue = 0;
			if (longestSieve.Count > 0)
			{
				cacheMaxValue = longestSieve.Last();
			}

			if (cacheMaxValue >= ceiling || longestprimeMembershipArray.Count >= ceiling)
			{
				LoggerSingleton.Log("Primes = [(Cached Value)];");
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
			}

			primeMembershipArray[2] = true;
			for (counter = counterStart; counter <= ceiling; counter += 2)
			{
				if ((counter & 1) == 1)//% 2 == 1)
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


			List<long> result = Enumerable.Range(2, (int)ceiling - 2).Select(n => (long)n).Where(l => primeMembershipArray[l]).ToList();

			if (result.Count > longestSieve.Count)
			{
				longestSieve = result;
			}

			if (primeMembershipArray.Length > longestprimeMembershipArray.Count)
			{
				longestprimeMembershipArray = primeMembershipArray.ToList();
			}

			LoggerSingleton.Log("Primes = [" + string.Join(",", result) + "];");
			return result;
		}
	}
}
