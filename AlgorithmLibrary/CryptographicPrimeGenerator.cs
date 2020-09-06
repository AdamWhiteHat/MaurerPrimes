using System;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Collections.Generic;

namespace AlgorithmLibrary
{
	public delegate void LogMethodDelegate(string message);
	public delegate void ReportProgressMethodDelegate();

	public class CryptographicPrimeGenerator
	{
		public static readonly BigInteger Two = new BigInteger(2);

		private bool IsDisposed = false;
		private CancellationToken cancelToken;

		public CryptographicPrimeGenerator(CancellationToken cancellationToken, bool loggingEnabled = false)
		{
			cancelToken = new CancellationToken();
			if (cancellationToken != null)
			{
				cancelToken = cancellationToken;
			}
			if (loggingEnabled)
			{
				Log.SetLoggingPreference(true);
			}
		}

		/// <summary>
		/// Returns a probable prime of size bits and will search n rounds for a composite
		/// </summary>
		/// <param name="bitSize">Size of prime, in bits</param>
		/// <param name="testCount">Number of different bases to use when testing probable prime  for composites as evidence of primality</param>
		/// <returns></returns>
		public BigInteger GetProbablePrime(int bitSize, int testCount)
		{
			BigInteger result = 0;

			//Log.Message(".");
			Log.MethodEnter("ProvablePrime", bitSize);


			if (cancelToken.IsCancellationRequested)
			{
				Log.Message("ProvablePrime.CancellationToken.IsCancellationRequested();");
				Log.MethodLeave();
				return -1;
			}

			if (bitSize <= 20)
			{
				Log.Message("***MAXIMUM RECURSION DEPT REACHED");
				result = TrialDivision.FindSmallPrimes(bitSize);
				Log.Message("***Hopeful prime: {0}", result);
			}
			else
			{
				bool done = false;
				double c = 0.1;
				int m = 20;
				double r = 0.5;

				if (bitSize > 2 * m)
				{
					double rnd = 0;
					done = false;
					while (!done)
					{
						rnd = CryptoRandomSingleton.NextDouble();
						r = Math.Pow(2, rnd - 1);
						done = (bitSize - r * bitSize) > m;
					}
				}

				int newBits = (int)Math.Floor(r * bitSize) + 1;

				BigInteger smallPrime = GetProbablePrime(newBits, testCount);

				if (smallPrime == -1)
				{
					Log.MethodLeave();
					return -1;
				}
				Log.Message("After Recursion: Length = {0}", smallPrime.ToString().Length);

				BigInteger pow = BigInteger.Pow(Two, bitSize - 1);
				BigInteger Q = Two * smallPrime;
				BigInteger I = pow / Q;

				long sieveMax = (long)(c * bitSize * bitSize);
				List<long> primes = Eratosthenes.Sieve(sieveMax);

				bool success = false;
				while (!success)
				{
					if (cancelToken.IsCancellationRequested)
					{
						Log.Message("ProvablePrime.CancellationToken.IsCancellationRequested();");
						Log.MethodLeave();
						return -1;
					}

					//LogMethod("Loop[{0}]: TestComposite({1})", _loopCount, result);

					done = false;
					BigInteger J = I + 1;
					BigInteger K = 2 * I;
					BigInteger rand1 = CryptoRandomSingleton.RandomRange(J, K);
					result = 2 * rand1;
					result = result * smallPrime;
					result = result + 1;

					BigInteger mod = new BigInteger();
					for (int i = 0; !done && i < primes.Count; i++)
					{
						mod = result % primes[i];
						done = mod == 0;
					}

					if (!done)
					{
						//LogMethod("ProvablePrime.RandomRange(J: {0}, K: {1}) = {2}", J, K, rand1);
						if (MillerRabin.IsProbablyPrime(result, testCount))
						{
							success = true;
							string cert = MillerRabin.GetCertificateOfPrimality(result, rand1);

							if (cert != null)
							{
								Log.Message(cert);
							}
						}
					}
				}
			}

			//Log.Message(".");
			Log.MethodLeave();
			return result;
		}

		public static double Log2(BigInteger n)
		{
			return BigInteger.Log10(n) / Math.Log10(2);
		}
	}
}