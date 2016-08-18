/*
 *
 * Developed by Adam Rakaska
 *  http://www.csharpprogramming.tips
 *    http://arakaska.wix.com/intelligentsoftware
 * 
 */
using System;
using System.Numerics;
using System.Security.Cryptography;

namespace AlgorithmLibrary
{
	public static class CryptoRandomSingleton
	{
		private static RNGCryptoServiceProvider _rng;
		static CryptoRandomSingleton() { _rng = new RNGCryptoServiceProvider(); }

		private static void ClearBuffer(byte[] buffer)
		{
			if (buffer != null)
			{
				int counter = buffer.Length-1;
				while(counter>=0)
				{
					buffer[counter] = byte.MinValue;
					counter--;
				}
				counter = 0;
			}
		}

		public static void Dispose()
		{
			if (_rng != null)
			{
				_rng.Dispose();
				_rng = null;
				_rng = new RNGCryptoServiceProvider();
			}
		}
		
		public static void NextBytes(byte[] buffer)
		{
			_rng.GetBytes(buffer);
		}

		public static long Next(long maxValue)
		{
			if (maxValue < 1) { throw new ArgumentOutOfRangeException("maxValue must be greater than zero"); }
			return Math.Abs(Next() % maxValue);
		}

		public static long Next()
		{
			byte[] rngBytes8 = new byte[8];
			_rng.GetBytes(rngBytes8);
			long result = Math.Abs(BitConverter.ToInt64(rngBytes8, 0));
			ClearBuffer(rngBytes8);
			return result;
		}

		public static double NextDouble()
		{
			byte[] rngBytes8 = new byte[8];
			_rng.GetBytes(rngBytes8);
			double result = Math.Abs(BitConverter.ToInt64(rngBytes8, 0) * (1.0 / long.MaxValue));
			ClearBuffer(rngBytes8);
			return result;
		}

		public static BigInteger RandomRange(BigInteger lower, BigInteger upper)
		{
			if (lower > upper) { throw new ArgumentOutOfRangeException("Upper must be greater than upper"); }

			// long implementation
			if (lower <= long.MaxValue && upper <= long.MaxValue)
			{
				BigInteger range;

				while (true)
				{
					range = lower + (long)(((long)upper - (long)lower) * NextDouble());

					if (range >= lower && range <= upper)
					{
						return range;
					}
				}
			}
			else // BigInteger implementation
			{
				return RandomRangeBigInteger(lower, upper);
			}
		}

		public static BigInteger RandomRangeBigInteger(BigInteger lower, BigInteger upper)
		{
			if (lower > upper) { throw new ArgumentOutOfRangeException("Upper must be greater than upper"); }

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
