using System;
using System.Linq;
using System.Text;
using System.Numerics;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;

namespace AlgorithmLibrary
{
	public class MillerRabin
	{
		public static TimeSpan InsideExecutionTime { get { return insideExecutionTime.TotalTime; } }
		public static TimeSpan OutsideExecutionTime { get { return outsideExecutionTime.TotalTime; } }
		private static AggregateTimer insideExecutionTime = new AggregateTimer();
		private static AggregateTimer outsideExecutionTime = new AggregateTimer();

		private static readonly BigInteger Two = new BigInteger(2);

		public static bool IsProbablyPrime(BigInteger primeHopeful, int testCount)
		{
			using (outsideExecutionTime.StartTimer())
			{
				Log.MethodEnter("MillerRabinPrimalityTest", primeHopeful, testCount);

				if (primeHopeful == 2 || primeHopeful == 3)
				{
					Log.MethodLeave();
					return true;
				}

				BigInteger remainder = primeHopeful & 1; // % Two;

				if (remainder == 0)
				{
					Log.MethodLeave();
					return false;
				}

				BigInteger hopefulLess1 = primeHopeful - 1;
				BigInteger quotientD = hopefulLess1;

				remainder = quotientD & 1; // % Two;

				long divisionCountR = 0;
				while (remainder == 0)
				{
					quotientD = quotientD >> 1; // / Two;
					remainder = quotientD & 1; // % Two;
					divisionCountR++;
				}

				BigInteger hopefulLess2 = primeHopeful - Two;
				BigInteger randomA = 0;
				BigInteger residueX;

				int counter = 1;
				int modCount = 1;
				for (counter = 1; counter <= testCount; counter++)
				{
					randomA = CryptoRandomSingleton.RandomRange(Two, hopefulLess2);
					using (insideExecutionTime.StartTimer())
					{
						residueX = BigInteger.ModPow(randomA, quotientD, primeHopeful);
					}
					if (residueX == 1 || residueX == hopefulLess1)
					{
						BigInteger gcd = BigInteger.GreatestCommonDivisor(residueX - 1, primeHopeful); // Chance to factor: GCD((a^d mod n) − 1, n)
						if (gcd != 1)
						{
							FoundFactor(primeHopeful, gcd);
						}
						continue;
					}

					modCount = 1;
					while (modCount <= divisionCountR)
					{
						residueX = BigInteger.ModPow(residueX, 2, primeHopeful);

						if (residueX == 1)
						{
							// Chance to factor: a^(d*2^r)-1

							BigInteger twoR = BigInteger.Pow(2, (int)divisionCountR);
							BigInteger dTwoR = BigInteger.Multiply(twoR, quotientD);

							if (dTwoR < int.MaxValue)
							{
								int pow = (int)dTwoR;
								BigInteger potentialFactor = BigInteger.Pow(randomA, pow);
								BigInteger gcd = BigInteger.GreatestCommonDivisor(potentialFactor - 1, primeHopeful);
								if (gcd != 1)
								{
									FoundFactor(primeHopeful, gcd);
								}
							}

							Log.MethodLeave();
							return false;
						}

						if (residueX == hopefulLess1)
						{
							// Chance to factor: GCD((a^(d*2^r) mod n) − 1, n)
							BigInteger twoR = BigInteger.Pow(2, (int)divisionCountR);
							BigInteger dTwoR = BigInteger.Multiply(twoR, quotientD);
							BigInteger potentialFactor = BigInteger.ModPow(randomA, dTwoR, primeHopeful);
							BigInteger gcd = BigInteger.GreatestCommonDivisor(potentialFactor - 1, primeHopeful);
							if (gcd != 1)
							{
								FoundFactor(primeHopeful, gcd);
							}

							break;
						}

						modCount++;
					}

					if (residueX != hopefulLess1)
					{
						// ??? Chance to factor ???

						BigInteger twoR = BigInteger.Pow(2, (int)divisionCountR);
						BigInteger dTwoR = BigInteger.Multiply(twoR, quotientD);
						BigInteger potentialFactor1 = BigInteger.ModPow(randomA, dTwoR, primeHopeful);
						BigInteger gcd1 = BigInteger.GreatestCommonDivisor(potentialFactor1 - 1, primeHopeful);
						if (gcd1 != 1)
						{
							FoundFactor(primeHopeful, gcd1);
						}

						if (dTwoR < int.MaxValue)
						{
							int pow = (int)dTwoR;
							BigInteger potentialFactor2 = BigInteger.Pow(randomA, pow);
							BigInteger gcd2 = BigInteger.GreatestCommonDivisor(potentialFactor2 - 1, primeHopeful);
							if (gcd2 != 1)
							{
								FoundFactor(primeHopeful, gcd2);
							}
						}

						Log.MethodLeave();
						return false;
					}
				}
				Log.MethodLeave();
				return true;
			}
		}

		public static void FoundFactor(BigInteger n, BigInteger factor)
		{
			File.AppendAllText("!FoundFactors.txt", $"{n}: {factor}{Environment.NewLine}");
		}

		public static string GetCertificateOfPrimality(BigInteger probable, BigInteger accuracy)
		{
			BigInteger witness = CryptoRandomSingleton.RandomRange(Two, probable - Two);
			BigInteger modPow = BigInteger.ModPow(witness, probable - 1, probable);

			string result = null;
			if (modPow == 1) // a^n-1 mod n == 1
			{
				modPow = BigInteger.ModPow(witness, 2 * accuracy, probable);
				BigInteger gcd = BigInteger.GreatestCommonDivisor(modPow - 1, probable);

				if (gcd == 1)
				{
					result = Environment.NewLine;
					result += string.Format("Certificate of primality({0})", probable) + Environment.NewLine;
					result += "{" + Environment.NewLine;
					result += "   " + string.Format("{0} ^ {1}-1 mod {1} == 1{2}", witness, probable, Environment.NewLine);
					result += "   " + string.Format("GCD({0}, {1}) == 1{2}", (modPow - 1), probable, Environment.NewLine);
					result += "}" + Environment.NewLine;
				}
			}

			return result;
		}

	}
}
