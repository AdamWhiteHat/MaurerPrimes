using System;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Drawing;
using System.Collections.Generic;
using System.Threading;
using AlgorithmLibrary;

namespace AlgorithmLibrary.MaurerPrimes
{
	public delegate void LogMethodDelegate(string message);
	public delegate void ReportProgressMethodDelegate();

	public class Algorithm : IDisposable
	{
		public static readonly BigInteger Two = new BigInteger(2);
		private bool IsDisposed = false;
		private CancellationToken cancelToken;

		private int recursionDepthCount;

		public Algorithm()
		{
			disposeCheck();
			cancelToken = new CancellationToken();
			
			recursionDepthCount = 0;
		}

		public Algorithm(CancellationToken cancellationToken)
			: this()
		{
			if (cancellationToken != null)
			{
				cancelToken = cancellationToken;
			}
		}

		#region IDisposable

		public void Dispose()
		{
			if (!IsDisposed)
			{
				IsDisposed = true;
				CryptoRandomSingleton.Dispose();
			}
		}

		private void disposeCheck()
		{
			if (IsDisposed)
			{
				throw new ObjectDisposedException(nameof(Algorithm));
			}
		}

		#endregion

		#region Logging Members

		private void EnterMethod(string methodName, params object[] args)
		{
			LoggerSingleton.Log("{0}{1}({2})", GetDepthPadding(), methodName, string.Join(", ", args.ToString()));
			LoggerSingleton.Log(GetDepthPadding() + "{");
			recursionDepthCount++;
		}

		private void LeaveMethod()
		{
			recursionDepthCount--;
			LoggerSingleton.Log(GetDepthPadding() + "}");
		}

		private void LogMethod(string message, params object[] args)
		{
			LoggerSingleton.Log(GetDepthPadding() + string.Format(message, args));
		}

		private string GetDepthPadding()
		{
			if (recursionDepthCount > 0)
			{
				return new string(Enumerable.Repeat(' ', recursionDepthCount).ToArray());
			}
			else
			{
				return "";
			}
		}

		#endregion

		public BigInteger ProvablePrime(int bits)
		{
			disposeCheck();
			BigInteger hopeful = 0;

			Console.Write(".");
			EnterMethod(nameof(ProvablePrime), bits);


			if (cancelToken.IsCancellationRequested)
			{
				LogMethod("{0}: CancellationToken.IsCancellationRequested", nameof(ProvablePrime));
				LeaveMethod();
				return -1;
			}

			if (bits <= 20)
			{				
				LogMethod("***MAXIMUM RECURSION DEPT REACHED: {0}", recursionDepthCount);
				hopeful = CheckForSmallComposites(bits);
				LogMethod("***Hopeful prime: {0}", hopeful);
			}
			else
			{
				bool done = false;
				double c = 0.1;
				int m = 20;
				double r = 0.5;

				if (bits > 2 * m)
				{
					double rnd = 0;
					done = false;
					while (!done)
					{
						rnd = CryptoRandomSingleton.NextDouble();
						r = Math.Pow(2, rnd - 1);
						done = (bits - r * bits) > m;
					}
				}

				int newBits = (int)Math.Floor(r * bits) + 1;

				BigInteger smallPrime = ProvablePrime(newBits);

				if (smallPrime == -1)
				{
					LeaveMethod();
					return -1;
				}
				LogMethod("After Recursion: Length = {0}", smallPrime.ToString().Length);

				BigInteger pow = BigInteger.Pow(Two, bits - 1);
				BigInteger Q = Two * smallPrime;
				BigInteger I = pow / Q;

				long sieveMax = (long)(c * bits * bits);
				List<long> primes = Sieve(sieveMax);

				bool success = false;
				while (!success)
				{
					if (cancelToken.IsCancellationRequested)
					{
						LogMethod("CancellationToken.IsCancellationRequested");
						LeaveMethod();
						return -1;
					}

					//LogMethod("Loop[{0}]: TestComposite({1})", _loopCount, result);

					done = false;
					BigInteger J = I + 1;
					BigInteger K = 2 * I;
					BigInteger rand1 = CryptoRandomSingleton.RandomRange(J, K);
					hopeful = 2 * rand1;
					hopeful = hopeful * smallPrime;
					hopeful = hopeful + 1;

					BigInteger mod = new BigInteger();
					for (int i = 0; !done && i < primes.Count; i++)
					{
						mod = hopeful % primes[i];
						done = mod == 0;
					}

					if (!done)
					{
						//LogMethod("ProvablePrime.RandomRange(J: {0}, K: {1}) = {2}", J, K, rand1);
						if (MillerRabinPrimalityTest(hopeful, 20))
						{
							string cert = GetCertificateOfPrimality(hopeful, rand1);

							if (cert != null)
							{
								LoggerSingleton.Log(cert);
								success = true;
							}
						}
					}
				}
			}

			LeaveMethod();
			return hopeful;
		}

		public bool MillerRabinPrimalityTest(BigInteger hopeful, int accuracy)
		{
			disposeCheck();
			EnterMethod(nameof(MillerRabinPrimalityTest), hopeful, accuracy);

			if (hopeful == 2 || hopeful == 3)
			{
				LeaveMethod();
				return true;
			}

			BigInteger m = hopeful % 2;

			if (m == 0)
			{
				LeaveMethod();
				return false;
			}

			BigInteger n1 = hopeful - 1;
			BigInteger r = n1;

			m = r % 2;

			long s = 0;
			while (m == 0)
			{
				r = r / 2;
				m = r % 2;
				s++;
			}

			BigInteger n2 = hopeful - 2;
			BigInteger a = 0;
			BigInteger y;

			int i = 1;
			int j = 1;
			for (i = 1; i <= accuracy; i++)
			{
				a = CryptoRandomSingleton.RandomRange(2, n2);
				y = BigInteger.ModPow(a, r, hopeful);

				if (y != 1 && y != n1)
				{
					j = 1;

					while (j <= s && y != n1)
					{
						y = BigInteger.ModPow(y, 2, hopeful);

						if (y == 1)
						{
							LeaveMethod();
							return false;
						}

						j++;
					}

					if (y != n1)
					{
						LeaveMethod();
						return false;
					}
				}
			}

			LeaveMethod();
			return true;
		}

		public string GetCertificateOfPrimality(BigInteger probable, BigInteger accuracy)
		{
			BigInteger witness = CryptoRandomSingleton.RandomRange(Two, probable - Two);
			BigInteger modPow = BigInteger.ModPow(witness, probable - 1, probable);

			string result = null;
			if (modPow == 1) // a^n-1 mod n == 1
			{
				modPow = BigInteger.ModPow(witness, 2 * accuracy, probable);
				BigInteger gcd = BigInteger.GreatestCommonDivisor(modPow - 1, probable);
				bool success = (gcd == 1);

				if (success)
				{
					LogMethod("GetCertificateOfPrimality.RandomRange({0}, {1}) = {2}", Two, (probable - Two), witness);

					Console.Write(".");
					result = GetDepthPadding() + string.Format("Certificate of primality for: {0}{1}", probable, Environment.NewLine);
					result += GetDepthPadding() + "{" + Environment.NewLine;
					result += GetDepthPadding() + string.Format(" {0} ^ {1}-1 mod {1} == 1{2}", witness, probable, Environment.NewLine);
					result += GetDepthPadding() + string.Format(" GCD({0}, {1}) == 1{2}", (modPow - 1), probable, Environment.NewLine);
					result += GetDepthPadding() + "}";
				}
			}

			return result;
		}

		// Do trial division
		private BigInteger CheckForSmallComposites(int bits)
		{
			disposeCheck();
			if (!(bits <= 20))
			{
				throw new ArgumentException("bits > 20");
			}

			EnterMethod(nameof(CheckForSmallComposites), bits);

			BigInteger result = 0;
			List<long> lucky = null;

			bool composite = true;
			while (composite)
			{
				long n = 1 << (bits - 1);

				for (int i = 0; i < bits - 1; i++)
				{
					n |= CryptoRandomSingleton.Next(2) << i;
				}

				long bound = (long)Math.Sqrt(n);

				lucky = Sieve(bound);
				composite = false;

				for (int i = 0; !composite && i < lucky.Count; i++)
				{
					composite = n % lucky[i] == 0;
				}

				if (!composite)
				{
					result = n;
				}
			}

			LeaveMethod();
			return result;
		}

		/// <summary>
		/// Sieve of Eratosthenes. Find all prime numbers less than or equal ceiling
		/// </summary>
		/// <param name="ceiling"></param>
		private List<long> Sieve(long ceiling)
		{
			disposeCheck();
			LogMethod(nameof(Sieve), ceiling);

			long counter = 0;
			long inc;
			long sqrt = 3;
			bool[] sieve = new bool[ceiling + 1];

			sieve[2] = true;

			for (counter = 3; counter <= ceiling; counter += 2)
			{
				if (counter % 2 == 1)
				{
					sieve[counter] = true;
				}
			}
			do
			{
				counter = sqrt * sqrt;
				inc = sqrt + sqrt;

				while (counter <= ceiling)
				{
					sieve[counter] = false;
					counter += inc;
				}

				sqrt += 2;

				while (!sieve[sqrt])
				{
					sqrt++;
				}
			} while (sqrt * sqrt <= ceiling);


			List<long> result = Enumerable.Range(2, (int)ceiling - 2).Select(n => (long)n).Where(l => sieve[l]).ToList();

			LeaveMethod();
			return result;
		}

		public static double Log2(BigInteger n)
		{
			return BigInteger.Log10(n) / Math.Log10(2);
		}
	}
}