using System;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Collections.Generic;

namespace AlgorithmLibrary
{
	public delegate void LogMethodDelegate(string message);
	public delegate void ReportProgressMethodDelegate();

	public class Algorithm : IDisposable
	{
		public static readonly BigInteger Two = new BigInteger(2);

		private bool IsDisposed = false;
		private CancellationToken cancelToken;

		public Algorithm(CancellationToken cancellationToken, bool loggingEnabled = false)
		{
			disposeCheck();
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
				throw new ObjectDisposedException("Algorithm");
			}
		}

		#endregion

		/// <summary>
		/// Returns a probable prime of size bits and will search n rounds for a composite
		/// </summary>
		/// <param name="bits">Size of prime, in bits</param>
		/// <param name="compositeSearchRounds">Quantity of rounds to search for composites as evidence of primality</param>
		/// <returns></returns>
		public BigInteger ProvablePrime(int bits, int compositeSearchRounds)
		{
			disposeCheck();
			BigInteger hopeful = 0;

			Console.Write(".");
			Log.MethodEnter("ProvablePrime", bits);


			if (cancelToken.IsCancellationRequested)
			{
				Log.Message("ProvablePrime.CancellationToken.IsCancellationRequested();");
				Log.MethodLeave();
				return -1;
			}

			if (bits <= 20)
			{
				Log.Message("***MAXIMUM RECURSION DEPT REACHED");
				hopeful = TrialDivision.CheckForSmallComposites(bits);
				Log.Message("***Hopeful prime: {0}", hopeful);
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

				BigInteger smallPrime = ProvablePrime(newBits, compositeSearchRounds);

				if (smallPrime == -1)
				{
					Log.MethodLeave();
					return -1;
				}
				Log.Message("After Recursion: Length = {0}", smallPrime.ToString().Length);

				BigInteger pow = BigInteger.Pow(Two, bits - 1);
				BigInteger Q = Two * smallPrime;
				BigInteger I = pow / Q;

				long sieveMax = (long)(c * bits * bits);
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
						if (MillerRabin.CompositeTest(hopeful, compositeSearchRounds))
						{
							success = true;
							string cert = MillerRabin.GetCertificateOfPrimality(hopeful, rand1);

							if (cert != null)
							{
								Log.Message(cert);
							}
						}
					}
				}
			}

			Console.Write(".");
			Log.MethodLeave();
			return hopeful;
		}

		public static double Log2(BigInteger n)
		{
			return BigInteger.Log10(n) / Math.Log10(2);
		}
	}
}