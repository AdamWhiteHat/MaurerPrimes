using System;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;
using System.ComponentModel;
using AlgorithmLibrary;

namespace MaurerWinform
{
	public partial class MainForm : Form
	{
		public bool IsBusy { get { return _isBusy; } private set { _isBusy = value; } }
		private volatile bool _isBusy = false;

		public int SearchDepth { get { return int.TryParse(CleanupString(tbSearchDepth.Text), out _searchDepth) ? _searchDepth : DefaultSearchDepth; } }
		private int _searchDepth = 0;

		//private CancellationToken cancelToken;
		private CancellationTokenSource cancelSource;
		private ThreadedAlgorithmWorker algorithmWorker;

		private static readonly int DefaultSearchDepth = 5;
		private static readonly string Numbers = "1023456789";
		private static readonly string MessageBoxCaption = "Oops!";
		private static readonly string PrimalityTestInstructions = Environment.NewLine + "Please enter the number you wish to factor into the input TextBox.";
		private static readonly string MultiplyInstructions = Environment.NewLine + "Please put the two numbers you wish to multiply onto separate lines into the input TextBox.";
		private static readonly string TrialDivisionInstructions = Environment.NewLine + "Please put the number you wish to divide into the input TextBox.";

		public MainForm()
		{
			InitializeComponent();
			tbInput.Text = "512";
			tbSearchDepth.Text = DefaultSearchDepth.ToString();
		}

		public void WriteOutputLine(string message, bool atBegining = false)
		{
			if (tbOutput.InvokeRequired)
			{
				tbOutput.Invoke(new MethodInvoker(() => { WriteOutputLine(message); }));
			}
			else
			{
				string text = Environment.NewLine;
				if (!string.IsNullOrWhiteSpace(message))
				{
					text = string.Concat(message, Environment.NewLine);
				}
				if (atBegining)
				{
					tbOutput.Text = tbOutput.Text.Insert(0, text);
				}
				else
				{
					tbOutput.AppendText(text);
				}
			}
		}

		private void btnSearch_Click(object sender, EventArgs e)
		{
			//tbOutput.Clear();

			if (IsBusy)
			{
				CancelBackgroundWorker();
			}
			else
			{
				IsBusy = true;
				SetButtonText(false);

				string input = CleanupString(tbInput.Text);

				int bits = 0;
				if (!int.TryParse(input, out bits))
				{
					DisplayErrorMessage("Error parsing number of bits: Try entering only numeric characters into the input TextBox.");
				}
				else if (bits < 7)
				{
					DisplayErrorMessage("If you need to find prime numbers smaller than seven 7 bits, use a non-specialized calculator.");
				}
				else
				{
					algorithmWorker = new ThreadedAlgorithmWorker(SearchDepth);
					algorithmWorker.DoWorkFunc = algorithmWorker.DoWork_FindPrime;
					algorithmWorker.WorkerComplete += FindPrimes_WorkerComplete;

					ResetCancellationTokenSource();

					algorithmWorker.StartWorker(cancelSource.Token, bits);
				}
			}
		}
			
		private void ResetCancellationTokenSource()
		{
			if (cancelSource != null)
			{
				cancelSource.Dispose();
				cancelSource = null;
			}

			if (cancelSource == null)
			{
				cancelSource = new CancellationTokenSource();
			}
		}

		private void CancelBackgroundWorker()
		{
			cancelSource.Cancel();
		}

		private void DisplayErrorMessage(string message, string caption = "")
		{
			MessageBox.Show(message, string.IsNullOrWhiteSpace(caption) ? MessageBoxCaption : caption, MessageBoxButtons.OK, MessageBoxIcon.Warning);
		}

		private void SetButtonText(bool enable, string resetText = "")
		{
			if (enable)
			{
				btnSearch.Text = resetText;
			}
			else
			{
				btnSearch.Text = "Cancel";
			}
		}

		private void FindPrimes_WorkerComplete(object sender, RunWorkerCompletedEventArgs e)
		{
			if (e.Error != null)
			{
				DisplayErrorMessage(e.Error.ToString(), "Exception Message");
			}
			else if (e.Cancelled)
			{
				WriteOutputLine("Task was canceled.", true);
			}
			else
			{
				BigInteger prime1 = (BigInteger)e.Result;

				string primeNumber = prime1.ToString();
				string base10size = (Math.Round(BigInteger.Log10(prime1))).ToString("F0");
				string base2size = (Math.Round(Algorithm.Log2(prime1))).ToString("F0");

				//.... Print bit array
				DateTime bitStringStart1 = DateTime.Now;
				byte[] byteArray1 = prime1.ToByteArray();
				BitArray bitArray1 = new BitArray(byteArray1);
				string bitString1 =
					new string(
						bitArray1.Cast<bool>()
						.Reverse()
						.SkipWhile(b => !b)
						.Select(b => b ? '1' : '0')
						.ToArray()
					);
				//string bitCountString1 = bitString1.Length.ToString();
				TimeSpan bitStringTime1 = DateTime.Now.Subtract(bitStringStart1);
				WriteOutputLine("");

				//.... PRINT
				WriteOutputLine(string.Format("{0} bit prime ({1} digits):", base2size, base10size));
				WriteOutputLine(primeNumber);
				//WriteOutputLine(string.Format("Bits ({0}):", log2prime));
				//WriteOutputLine(bitString1+Environment.NewLine);

				//... Print run/processing time 
				if (algorithmWorker.RuntimeTimer != null && algorithmWorker.RuntimeTimer != TimeSpan.Zero)
				{
					WriteOutputLine(string.Format("Run time: {0}", ThreadedAlgorithmWorker.FormatTimeSpan(algorithmWorker.RuntimeTimer)));
					WriteOutputLine(string.Format("Time to render: {0}", ThreadedAlgorithmWorker.FormatTimeSpan(bitStringTime1)));
				}
			}

			IsBusy = false;
			SetButtonText(true, "Search for primes...");
		}

		private void tbOutput_KeyUp(object sender, KeyEventArgs e)
		{
			if (e.Control)
			{
				if (e.KeyCode == Keys.A) // CTRL + A, Select all
				{
					tbOutput.SelectAll();
				}
				else if (e.KeyCode == Keys.S) // CTRL + S, Save as
				{
					using (SaveFileDialog saveFileDialog = new SaveFileDialog())
					{
						if (saveFileDialog.ShowDialog() == DialogResult.OK)
						{
							File.WriteAllLines(saveFileDialog.FileName, tbOutput.Lines);
						}
					}
				}
			}
		}

		private string CleanupString(string input, bool removeNewlines = true)
		{
			string result = input ?? "";
			if (!string.IsNullOrEmpty(result))
			{
				result = result.Trim();        // Trim all leading and trailing whitespace
				result = result.Replace(",", "");   // Remove any commas, often used as place-value separators (in USA)
				if (removeNewlines)
				{
					result = result.Replace("\r", "");  // Remove any line-feeds or carriage returns in case
					result = result.Replace("\n", ""); //   pasted from shitty text editor with word wrapping
				}
			}
			return result;
		}

		private void btnPrimalityTest_Click(object sender, EventArgs e)
		{
			string input = tbInput.Lines.FirstOrDefault();
			input = CleanupString(input);
			if (string.IsNullOrEmpty(input))
			{
				DisplayErrorMessage(PrimalityTestInstructions);
				return;
			}

			if (input.Any(c => !Numbers.Contains(c))) // Check for non-numerical characters
			{
				DisplayErrorMessage("Non-numeric characters detected in the input! " + PrimalityTestInstructions);
				return;
			}
			BigInteger intTest = BigInteger.Parse(input);
			DateTime startTime = DateTime.Now;
			bool result = MillerRabin.CompositeTest(intTest, SearchDepth);
			TimeSpan timeElapsed = DateTime.Now.Subtract(startTime);
			WriteOutputLine("");
			WriteOutputLine(string.Format("Is prime: {0}", result.ToString()));
			WriteOutputLine(string.Format("Time elapsed: {0}", ThreadedAlgorithmWorker.FormatTimeSpan(timeElapsed)));
		}

		private void btnMultiply_Click(object sender, EventArgs e)
		{
			tbInput.Text = CleanupString(tbInput.Text, false);
			if (string.IsNullOrWhiteSpace(tbInput.Text))
			{
				DisplayErrorMessage("Input empty! " + MultiplyInstructions);
			}
			else if (tbInput.Text.All(c => !Numbers.Contains(c)))
			{
				DisplayErrorMessage("No numeric input detected! " + MultiplyInstructions);
			}
			else if (tbInput.Lines.Length != 2)
			{
				DisplayErrorMessage("Only one input line detected! " + MultiplyInstructions);
			}
			else
			{
				string num1 = tbInput.Lines[0].Trim();
				string num2 = tbInput.Lines[1].Trim();

				if (num1.Any(c => !Numbers.Contains(c)) || num2.Any(c => !Numbers.Contains(c)))
				{
					DisplayErrorMessage("Non-numeric characters detected in the input! " + MultiplyInstructions);
					return;
				}

				BigInteger bigInt1 = BigInteger.Parse(num1);
				BigInteger bigInt2 = BigInteger.Parse(num2);

				BigInteger result = BigInteger.Multiply(bigInt1, bigInt2);

				WriteOutputLine(string.Format("=\n{0}", result.ToString()), true);
			}
		}

	}
}
