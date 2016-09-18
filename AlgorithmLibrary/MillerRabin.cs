using System;
using System.Linq;
using System.Text;
using System.Numerics;
using System.Collections.Generic;

namespace AlgorithmLibrary
{
	public class MillerRabin
	{
		private static readonly BigInteger Two = new BigInteger(2);

		public static bool CompositeTest(BigInteger hopeful, int accuracy)
		{
			Log.MethodEnter("MillerRabinPrimalityTest", hopeful, accuracy);

			if (hopeful == 2 || hopeful == 3)
			{
				Log.MethodLeave();
				return true;
			}

			BigInteger remainder = hopeful & 1;// % Two;

			if (remainder == 0)
			{
				Log.MethodLeave();
				return false;
			}

			BigInteger hopefulLess1 = hopeful - 1;
			BigInteger quotient = hopefulLess1;

			remainder = quotient & 1;// % Two;

			long divisionCount = 0;
			while (remainder == 0)
			{
				quotient = quotient >> 1;/// Two;
				remainder = quotient & 1;//% Two;
				divisionCount++;
			}

			BigInteger hopefulLess2 = hopeful - Two;
			BigInteger random = 0;
			BigInteger residue;

			int testCount = 1;
			int modCount = 1;
			for (testCount = 1; testCount <= accuracy; testCount++)
			{
				random = CryptoRandomSingleton.RandomRange(Two, hopefulLess2);
				residue = BigInteger.ModPow(value: random,exponent: quotient, modulus: hopeful);

				if (residue == 1 || residue == hopefulLess1)
				{
					continue;
				}

				modCount = 1;
				while (modCount <= divisionCount && residue != hopefulLess1)
				{
					residue = BigInteger.ModPow(residue, 2, hopeful);

					if (residue == 1)
					{
						Log.MethodLeave();
						return false;
					}

					modCount++;
				}

				if (residue != hopefulLess1)
				{
					Log.MethodLeave();
					return false;
				}
			}

			Log.MethodLeave();
			return true;
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
					//LogMethod("GetCertificateOfPrimality.RandomRange({0}, {1}) = {2}", Two, (probable - Two), witness);

					//Console.Write(".");
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
