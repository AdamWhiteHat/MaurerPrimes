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
	public enum LoggingMethodType
	{
		Stream,
		Console,
		None
	}

	public class Algorithm : IDisposable
	{
		private CryptoRNG cryptoRandom;
		private bool IsDisposed = false;
		private CancellationToken cancelToken;

		private bool isLoggingEnabled;
		private MemoryStream logMemoryStream;
		private StreamWriter logStreamWriter;		
		private static string logFilename = "Algorithm.MethodCalls.log.txt";
		private LoggingMethodType loggingType { get; set; }

		private int recursionDepthCount;

		public Algorithm()
		{
			disposeCheck();
			cryptoRandom = new CryptoRNG();
			cancelToken = new CancellationToken();

			isLoggingEnabled = false;
			loggingType = LoggingMethodType.None;

			logMemoryStream = new MemoryStream();
			logStreamWriter = new StreamWriter(logMemoryStream);
			logStreamWriter.AutoFlush = true;

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
				cryptoRandom.Dispose();

				File.WriteAllText(logFilename, GetLogData());

				logStreamWriter.Close();
				logMemoryStream.Close();

				logStreamWriter.Dispose();
				logMemoryStream.Dispose();
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

		#region Logging Members

		public string GetLogData()
		{
			//disposeCheck();

			if (logMemoryStream != null)
			{
				FileStream logFileStream = new FileStream(logFilename, FileMode.Create);
				logMemoryStream.WriteTo(logFileStream);
				logFileStream.Flush();
				logFileStream.Close();
				logFileStream.Dispose();				
				return File.ReadAllText(logFilename);
			}
			return "";
		}
		
		public void SetLoggingBehavior(bool enable, LoggingMethodType loggingMethod)
		{
			isLoggingEnabled = enable;
			loggingType = loggingMethod;
		}

		int repeatLogCount = 0;
		string lastLogString = " ";
		/// <summary>
		/// LOGGING
		/// </summary>
		private void Log(string format, params object[] args)
		{
			disposeCheck();
			if (isLoggingEnabled)
			{
				if (string.IsNullOrWhiteSpace(format))
				{
					InvokeLoggingMethod(Environment.NewLine);
				}
				else
				{
					string input = "";
					if (args == null || args.Count() < 1) { input = string.Format(format); }
					else { input = string.Format(format, args); }

					//logStreamWriter.WriteLine(input);

					if (input == lastLogString)
					{
						repeatLogCount += 1;
					}
					else
					{
						InvokeLoggingMethod(lastLogString);
						if (repeatLogCount > 0)
						{
							InvokeLoggingMethod(string.Format(" [{0}]", repeatLogCount));
							repeatLogCount = 0;
						}
						InvokeLoggingMethod(Environment.NewLine);
						lastLogString = input;
					}
				}
			}
		}

		private void InvokeLoggingMethod(string textToLog)
		{
			if (loggingType == LoggingMethodType.None || string.IsNullOrEmpty(textToLog))
			{
				return;
			}
			else if (loggingType == LoggingMethodType.Stream)
			{
				logStreamWriter.Write(textToLog);
			}
			else if (loggingType == LoggingMethodType.Console)
			{
				Console.Write(textToLog);
			}
		}
		
		private int cachedDepthCount = 0;
		private string cachedPadding = "";
		private string GetDepthPadding()
		{
			if (cachedDepthCount != recursionDepthCount)
			{				
				cachedDepthCount = recursionDepthCount;
				if (recursionDepthCount < 1)
				{
					cachedPadding = "";
				}
				else
				{
					cachedPadding = new string(Enumerable.Repeat(' ', recursionDepthCount + 1).ToArray());
				}			
			}
			return cachedPadding;
		}
		
		#endregion

		public BigInteger ProvablePrime(int bits)
		{
			disposeCheck();
			BigInteger result = 0;

			if (loggingType == LoggingMethodType.None)
			{
				Console.Write(".");
			}
			else
			{
				Log("{0}-->ENTERING: ProvablePrime({1})", GetDepthPadding(), bits);
			}

			if (cancelToken.IsCancellationRequested)
			{
				return -1;
			}

			if (bits <= 20)
			{
				if (loggingType == LoggingMethodType.None)
				{
					Console.Write(string.Format("\b] ({0})", recursionDepthCount));
					Console.WriteLine();
					Console.Write(" [");

					int top = Console.CursorTop;
					int left = Console.CursorLeft;
					Console.SetCursorPosition(left + recursionDepthCount, top);
					Console.Write("]");
					Console.SetCursorPosition(left, top);
				}
				else
				{
					Log("{0}***MAXIMUM RECURSION DEPT REACHED: {1}", GetDepthPadding(), recursionDepthCount);
				}
				result = FindSmallPrime(bits);
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
						rnd = cryptoRandom.NextDouble();
						r = Math.Pow(2, rnd - 1);
						done = (bits - r * bits) > m;
					}
				}

				int newBits = (int)Math.Floor(r * bits) + 1;

				recursionDepthCount++;
				BigInteger smallPrime = ProvablePrime(newBits);
				recursionDepthCount--;

				if (smallPrime == -1)
				{
					return -1;
				}

				//Log("After Recursion: Length = {0}{1}", smallPrime.ToString().Length, Environment.NewLine);

				BigInteger two = 2;
				BigInteger pow = BigInteger.Pow(two, bits - 1);
				BigInteger Q = two * smallPrime;
				BigInteger I = pow / Q;

				long sieveMax = (long)(c * bits * bits);
				List<long> primes = Sieve(sieveMax);

				bool success = false;
				while (!success)
				{
					if (cancelToken.IsCancellationRequested)
					{
						return -1;
					}

					//Log(" Loop[{0}]: TestComposite({1})", _loopCount, result);

					done = false;
					BigInteger J = I + 1;
					BigInteger K = 2 * I;
					BigInteger randRange = RandomRange(J, K);

					result = 2 * randRange;
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
						if (MillerRabinPrimalityTest(result, 20))
						{
							//Log("  Passed: Composite Test ({0})", result);
							BigInteger rand = RandomRange(two, result - two);
							BigInteger modPow = BigInteger.ModPow(rand, result - 1, result);

							if (modPow == 1)
							{
								//Log("   Passed: ModPow({0}, {1}, {2}) == 1", rand, result-1, result);
								modPow = BigInteger.ModPow(rand, 2 * randRange, result);
								BigInteger gcd = BigInteger.GreatestCommonDivisor(modPow - 1, result);
								success = (gcd == 1);
								if (success)
								{
									//string log10prime = (Math.Round(BigInteger.Log10(result))).ToString("F0");
									if (loggingType == LoggingMethodType.None)
									{
										Console.Write(".");
									}
									else
									{
										string log2prime = (Math.Round(Algorithm.Log2(result))).ToString("F0");
										Log("{0}<--LEAVING: ProvablePrime(): Return {1,3} bit prime", GetDepthPadding(), log2prime);//, result.ToString());
									}									
								}
							}
						}
					}
				}
			}

			return result;
		}

		public static double Log2(BigInteger n)
		{
			return BigInteger.Log10(n) / Math.Log10(2);
		}

		public bool MillerRabinPrimalityTest(BigInteger n, int t)
		{
			disposeCheck();
			//string nStr = n.ToString().Substring(0, nStr.Length > 8 ? 8 : nStr.Length);
			//Log("{0}Composite()",GetDepthPadding());//("Composite({0}, {1})", nStr, t);

			if (n == 2 || n == 3)
			{
				return true;
			}

			BigInteger m = n % 2;

			if (m == 0)
			{
				return false;
			}

			BigInteger n1 = n - 1;
			BigInteger r = n1;

			m = r % 2;

			long s = 0;
			while (m == 0)
			{
				r = r / 2;
				m = r % 2;
				s++;
			}

			BigInteger n2 = n - 2;
			BigInteger a;
			BigInteger y;

			int i = 1;
			int j = 1;
			for (i = 1; i <= t; i++)
			{
				a = RandomRange(2, n2);
				y = BigInteger.ModPow(a, r, n);

				if (y != 1 && y != n1)
				{
					j = 1;

					while (j <= s && y != n1)
					{
						y = BigInteger.ModPow(y, 2, n);

						if (y == 1)
						{
							return false;
						}

						j++;
					}

					if (y != n1)
					{
						return false;
					}
				}
			}

			return true;
		}

		public BigInteger RandomRange(BigInteger lower, BigInteger upper)
		{
			disposeCheck();
			if (lower <= long.MaxValue && upper <= long.MaxValue && lower < upper)
			{
				//string lStr = lower.ToString().Substring(0, lStr.Length > 8 ? 8 : lStr.Length);
				//string uStr = upper.ToString().Substring(0, uStr.Length > 8 ? 8 : uStr.Length);
				//Log("{0}RandomRange()", GetDepthPadding());//("RandomRange({0}, {1})", lower, upper);

				BigInteger range;

				while (true)
				{
					range = lower + (long)(((long)upper - (long)lower) * cryptoRandom.NextDouble());

					if (range >= lower && range <= upper)
					{
						return range;
					}
				}
			}

			BigInteger delta = upper - lower;
			byte[] deltaBytes = delta.ToByteArray();
			byte[] buffer = new byte[deltaBytes.Length];

			BigInteger result;
			while (true)
			{
				cryptoRandom.NextBytes(buffer);

				result = new BigInteger(buffer) + lower;

				if (result >= lower && result <= upper)
				{
					return result;
				}
			}
		}

		private BigInteger FindSmallPrime(int bits)
		{
			disposeCheck();
			if (!(bits <= 20)) { throw new ArgumentException("bits > 20"); }

			//Log("{0}FindSmallPrime({1})", GetDepthPadding(), bits);

			BigInteger result = 0;
			List<long> primes = null;

			bool composite = true;
			while (composite)
			{
				long n = 1 << (bits - 1);

				for (int i = 0; i < bits - 1; i++)
				{
					n |= (long)cryptoRandom.Next(2) << i;
				}

				long bound = (long)Math.Sqrt(n);

				primes = Sieve(bound);
				composite = false;

				for (int i = 0; !composite && i < primes.Count; i++)
				{
					composite = n % primes[i] == 0;
				}

				if (!composite)
				{
					result = n;
				}
			}

			return result;
		}

		/// <summary>
		/// Sieve of Eratosthenes. Find all prime numbers less than or equal ceiling
		/// </summary>
		private List<long> Sieve(long ceiling)
		{
			//Log("{0}Sieve()",GetDepthPadding());
			disposeCheck();
			long i = 0;
			long inc;
			long sqrt = 3;
			bool[] sieve = new bool[ceiling + 1];

			sieve[2] = true;

			for (i = 3; i <= ceiling; i++)
			{
				if (i % 2 == 1)
				{
					sieve[i] = true;
				}
			}

			do
			{
				i = sqrt * sqrt;
				inc = sqrt + sqrt;

				while (i <= ceiling)
				{
					sieve[i] = false;
					i += inc;
				}

				sqrt += 2;

				while (!sieve[sqrt])
				{
					sqrt++;
				}
			} while (sqrt * sqrt <= ceiling);

			return Enumerable.Range(2, (int)ceiling - 2).Select(n => (long)n).Where(l => sieve[l]).ToList();
		}
	}
}