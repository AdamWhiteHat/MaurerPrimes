using System;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Threading;

using AlgorithmLibrary.MaurerPrimes;

namespace MaurerConsole
{
	class Program
	{
		internal static void Main()
		{
			int counter = 1;
			int sizeTextPosition = 0;
			int max = Settings.Quantity;
			int primeBitSize = Settings.Prime_BitSize;
			string dashedLine = "---------------------------------------------";

			Console.CursorVisible = false;
			ConsoleWorker worker = new ConsoleWorker(primeBitSize);
			Console.ForegroundColor = ConsoleColor.White;

			Console.WriteLine("Maurer Prime Finding Algorithm (Console)");
			Console.WriteLine(dashedLine);
			Console.WriteLine("TO EXIT: [ESC] or [Q]");
			Console.WriteLine();

			while (counter <= max)
			{
				Console.WriteLine(dashedLine);
				Console.WriteLine("({0} of {1})", counter, max);
				Console.WriteLine();
				Console.WriteLine(" TARGET SIZE: {0} bits", primeBitSize);

				sizeTextPosition = Console.CursorTop;
				Console.WriteLine();
				Console.WriteLine();
				Console.Write(" [");
				if (!worker.StartWorker())
				{
					break;
				}

				while (worker.IsBusy)
				{
					Thread.Sleep(Settings.ThreadSleep_Duration); // Sleep a bit

					if (Console.KeyAvailable)
					{
						ConsoleKeyInfo cki = Console.ReadKey(true);
						if (cki.Key == ConsoleKey.Escape || cki.Key == ConsoleKey.Q)
						{
							worker.CancelWorker();
							Console.WriteLine("Abort key received.");
							Console.WriteLine();
							break;
						}
					}
				}

				if (worker.Result == WorkerResultType.Canceled)
				{
					break;
				}
				else if (worker.Result == WorkerResultType.None)
				{
					Console.WriteLine("Result object empty! Aborting...");
					break;
				}
				else if (worker.Result == WorkerResultType.Error)
				{
					Exception error = worker.RemoveErrorResult();
					DisplayError(error);
					break;
				}
				else if (worker.Result == WorkerResultType.Success)
				{
					BigInteger prime = worker.RemoveSuccessResult();
					LogPrime(prime, worker.RunTime, sizeTextPosition);
				}

				counter++;
			}


			if (worker.Result == WorkerResultType.Canceled)
			{
				Console.WriteLine(dashedLine);
				Console.WriteLine();
				Console.WriteLine("Aborted. Hit any key to terminate...");
				Console.ReadLine();
				return;
			}

			while (worker.Result != WorkerResultType.None)
			{
				if (worker.Result == WorkerResultType.Error)
				{
					Exception error = worker.RemoveErrorResult();
					DisplayError(error);
				}
				else if (worker.Result == WorkerResultType.Success)
				{
					BigInteger prime = worker.RemoveSuccessResult();
					LogPrime(prime, worker.RunTime, sizeTextPosition);
				}
			}

			Console.WriteLine(dashedLine);
			Console.WriteLine();
			Console.WriteLine("Finished. Hit any key to terminate...");
			Console.ReadLine();
			Console.ResetColor();
		}

		private static void DisplayError(Exception error)
		{
			Console.ForegroundColor = ConsoleColor.Red;
			Console.WriteLine(string.Format("Worker returned a {0} exception:", error.GetType().Name));
			Console.WriteLine(string.Format("Message: \"{0}\"", error.Message));
			Console.ForegroundColor = ConsoleColor.White;
		}

		private static void LogPrime(BigInteger prime, TimeSpan timeElapsed, int cursorTopPosition)
		{
			string primeText = prime.ToString();
			string log10prime = (Math.Round(BigInteger.Log10(prime))).ToString("F0");
			string log2prime = (Math.Round(Algorithm.Log2(prime))).ToString("F0");

			Console.WriteLine("] {0} (time elapsed)", ThreadedAlgorithmWorker.FormatTimeSpan(timeElapsed));
			Console.WriteLine();
			Console.WriteLine(" OUTPUT:  ..\\{0}", Settings.File_Output.ToUpperInvariant());

			int saveCursorTop = Console.CursorTop;
			Console.SetCursorPosition(0, cursorTopPosition);
			Console.WriteLine(" ACTUAL SIZE: {0} bits ({1} decimal digits)", log2prime, log10prime);
			Console.SetCursorPosition(0, saveCursorTop);
			Console.WriteLine();

			string fileOutput = string.Format("{1} bit prime ({2} digits):{0}{3}{0}{0}", Environment.NewLine, log2prime, log10prime, primeText);
			File.AppendAllText(Settings.File_Output, fileOutput);
		}
	}
}
