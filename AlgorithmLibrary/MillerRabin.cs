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
			bool result = false;

			using (outsideExecutionTime.StartTimer())
			{
				Log.MethodEnter("MillerRabinPrimalityTest", nameof(primeHopeful), $"{primeHopeful}, {nameof(testCount)}: {testCount}");

				if (primeHopeful == 2 || primeHopeful == 3 || primeHopeful == 5 || primeHopeful == 7)
				{
					FoundFactor(primeHopeful, BigInteger.GreatestCommonDivisor(primeHopeful, 210));
					result = true;
					goto exit;
				}

				BigInteger remainder = primeHopeful & 1; // % Two;
				if (remainder == 0)
				{
					FoundFactor(primeHopeful, 2);
					result = false;
					goto exit;
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
				BigInteger residueX;
				BigInteger priorResidueX;
				BigInteger randomA = 0;

				// Tracks previous random values, prevents testing with the same value twice, 
				// which would dramatically reduce our confidence in the primality of a probable prime.
				// Seems unlikely to happen, but its not a hit to performance to check.
				List<BigInteger> previousRandomValues = new List<BigInteger>();

				int counter = 1;
				int modCount = 1;
				for (counter = 1; counter <= testCount; counter++)
				{
					do
					{
						randomA = CryptoRandomSingleton.RandomRange(Two, hopefulLess2);
					}
					while (previousRandomValues.Contains(randomA));
					previousRandomValues.Add(randomA);

					Log.Message($"MillerRabin check prime hopeful (P) against random n: {randomA}");

					using (insideExecutionTime.StartTimer())
					{
						residueX = BigInteger.ModPow(randomA, quotientD, primeHopeful);
						Log.Message($"MillerRabin check a^d ≡ r (mod primeHopeful): {randomA}^{quotientD} ≡ {residueX} (mod {primeHopeful})");
					}
					if (residueX == 1 || residueX == hopefulLess1)
					{
						BigInteger gcd = BigInteger.GreatestCommonDivisor(residueX - 1, primeHopeful); // Chance to factor: GCD((a^d mod n) − 1, n)
						Log.Message($"A) Chance to factor:  GCD((a^d mod n) − 1, primeHopeful)");
						if (gcd != 1 && gcd != primeHopeful)
						{
							FoundFactor(primeHopeful, gcd);
							result = false;
							goto exit;
						}
						continue;
					}

					modCount = 1;
					while (modCount <= divisionCountR)
					{
						priorResidueX = residueX;
						residueX = BigInteger.ModPow(residueX, 2, primeHopeful);
						Log.Message($"MillerRabin check a^2 ≡ r (mod primeHopeful): {priorResidueX}^2 ≡ {residueX} (mod {primeHopeful})");

						if (residueX == 1)
						{
							// Chance to factor: a^(d*2^r)-1							
							BigInteger twoR = BigInteger.Pow(2, (int)divisionCountR);
							BigInteger dTwoR = BigInteger.Multiply(quotientD, twoR);

							if (dTwoR < int.MaxValue)
							{
								int pow = (int)dTwoR;
								BigInteger potentialFactor = BigInteger.Pow(randomA, pow) - 1;
								BigInteger gcd = BigInteger.GreatestCommonDivisor(potentialFactor, primeHopeful);
								Log.Message($"B) Chance to factor: GCD(a^(d*(2^r))-1, primeHopeful): GCD({randomA}^({quotientD}*2^{divisionCountR})-1 == {potentialFactor - 1}, primeHopeful) == {gcd}");
								if (gcd != 1 && gcd != primeHopeful)
								{
									FoundFactor(primeHopeful, gcd);
									result = false;
									goto exit;
								}
							}

							Log.MethodLeave();
							return false;
						}

						if (residueX == hopefulLess1)
						{
							// Chance to factor: GCD((a^(d*2^r) mod n) − 1, n)							
							BigInteger twoR = BigInteger.Pow(2, (int)divisionCountR);
							BigInteger dTwoR = BigInteger.Multiply(quotientD, twoR);
							BigInteger potentialFactor = BigInteger.ModPow(randomA, dTwoR, primeHopeful);
							BigInteger gcd = BigInteger.GreatestCommonDivisor(potentialFactor - 1, primeHopeful);
							Log.Message($"C) Chance to factor: GCD( a^(d*(2^r)) ≡ m−1 (mod primeHopeful) , primeHopeful): GCD( {randomA}^({quotientD}*(2^{divisionCountR})) ≡ {potentialFactor}-1 (mod primeHopeful) == {potentialFactor - 1}, primeHopeful) == {gcd}");
							if (gcd != 1 && gcd != primeHopeful)
							{
								FoundFactor(primeHopeful, gcd);
								result = false;
								goto exit;
							}

							break;
						}

						modCount++;
					}

					if (residueX != hopefulLess1)
					{
						// ??? Chance to factor ???

						BigInteger twoR = BigInteger.Pow(2, (int)divisionCountR);
						BigInteger dTwoR = BigInteger.Multiply(quotientD, twoR);
						BigInteger potentialFactor1 = BigInteger.ModPow(randomA, dTwoR, primeHopeful);
						BigInteger gcd1 = BigInteger.GreatestCommonDivisor(potentialFactor1 - 1, primeHopeful);
						Log.Message($"D) Chance to factor: GCD( a^(d*(2^r)) ≡ m−1 (mod primeHopeful) , primeHopeful): GCD( {randomA}^({quotientD}*(2^{divisionCountR})) ≡ {potentialFactor1}-1 (mod primeHopeful) == {potentialFactor1 - 1}, primeHopeful) == {gcd1}");
						if (gcd1 != 1 && gcd1 != primeHopeful)
						{
							FoundFactor(primeHopeful, gcd1);
							result = false;
							goto exit;
						}

						result = false;
						goto exit;
					}
				}

				result = true;
				goto exit;
			}

		exit:
			Log.MethodLeave();
			return result;
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
