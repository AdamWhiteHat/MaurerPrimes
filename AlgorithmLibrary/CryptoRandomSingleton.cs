/*
 *
 * Developed by Adam White
 *  https://csharpcodewhisperer.blogspot.com
 *    http://aWhite.wix.com/intelligentsoftware
 * 
 */
using System;
using System.Numerics;
using System.Security.Cryptography;

namespace AlgorithmLibrary
{
	public static class CryptoRandomSingleton
	{
		public static TimeSpan TotalExecutionTime { get { return executionTimer.TotalTime; } }
		private static AggregateTimer executionTimer { get; }

		private static RNGCryptoServiceProvider rng;

		static CryptoRandomSingleton()
		{
			executionTimer = new AggregateTimer();
			rng = new RNGCryptoServiceProvider();
		}

		private static void ClearBuffer(byte[] buffer)
		{
			if (buffer != null)
			{
				int counter = buffer.Length - 1;
				while (counter >= 0)
				{
					buffer[counter] = byte.MinValue;
					counter--;
				}
				counter = 0;
			}
		}

		public static void Dispose()
		{
			if (rng != null)
			{
				rng.Dispose();
				rng = null;
				rng = new RNGCryptoServiceProvider();
			}
		}

		private static void NextBytes(byte[] buffer)
		{
			rng.GetBytes(buffer);
		}

		public static long Next(long maxValue)
		{
			if (maxValue < 1) { throw new ArgumentOutOfRangeException("maxValue must be greater than zero"); }
			return Math.Abs(Next() % maxValue);
		}

		public static double NextDouble()
		{
			long next = Next();
			if (next < 0)
			{
				throw new ArithmeticException();
			}
			double result = Math.Abs(next * (1.0 / long.MaxValue));
			return result;
		}

		private static double notTimedNextDouble()
		{
			byte[] rngBytes8 = new byte[8];
			rng.GetBytes(rngBytes8);
			double result = Math.Abs(BitConverter.ToInt64(rngBytes8, 0) * (1.0 / long.MaxValue));
			ClearBuffer(rngBytes8);
			return result;
		}

		private static long Next()
		{
			using (executionTimer.StartTimer())
			{
				byte[] rngBytes8 = new byte[8];
				rng.GetBytes(rngBytes8);
				long result = Math.Abs(BitConverter.ToInt64(rngBytes8, 0));
				ClearBuffer(rngBytes8);
				return result;
			}
		}

		/// <summary>
		/// Generates a random number uniformly distributed over the entire range 
		/// </summary>
		public static BigInteger RandomRange(BigInteger lower, BigInteger upper)
		{
			if (lower > upper) { throw new ArgumentOutOfRangeException("Upper must be greater than lower"); }

			// long implementation
			if (lower <= long.MaxValue && upper <= long.MaxValue)
			{
				using (executionTimer.StartTimer())
				{
					BigInteger range;

					while (true)
					{
						range = lower + (long)(((long)upper - (long)lower) * notTimedNextDouble());

						if (range >= lower && range <= upper)
						{
							return range;
						}
					}
				}
			}
			else // BigInteger implementation
			{
				using (executionTimer.StartTimer())
				{
					return RandomRangeBigInteger(lower, upper);
				}
			}
		}

		/// <summary>
		/// Generates a random number uniformly distributed over an entire range of arbitrary size
		/// </summary>
		private static BigInteger RandomRangeBigInteger(BigInteger lower, BigInteger upper)
		{
			if (lower > upper) { throw new ArgumentOutOfRangeException("Upper must be greater than lower"); }

			BigInteger delta = upper - lower;
			byte[] deltaBytes = delta.ToByteArray();
			byte[] buffer = new byte[deltaBytes.Length];
			ClearBuffer(deltaBytes);

			BigInteger result;
			while (true)
			{
				NextBytes(buffer);

				result = new BigInteger(buffer) + lower;

				if (result >= lower && result <= upper)
				{
					ClearBuffer(buffer);

					return result;
				}
			}
		}
	}
}
