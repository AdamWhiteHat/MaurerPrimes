using System;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Collections;
using System.Windows.Forms;
using System.ComponentModel;

using AlgorithmLibrary.MaurerPrimes;
using System.IO;

namespace MaurerWinform
{
	public partial class MainForm : Form
	{
		public bool IsBusy { get { return _isBusy; } private set { _isBusy = value; } }
		private volatile bool _isBusy = false;
		
		private int bitSize;
		private CancellationToken cancelToken;
		private CancellationTokenSource cancelSource;
		private ThreadedAlgorithmWorker algorithmWorker;

		public MainForm()
		{
			InitializeComponent();
			tbNumberOfBits.Text = "512";
			bitSize = 0;
		}

		private void btnSearch_Click(object sender, EventArgs e)
		{
			tbOutput.Clear();

			if (IsBusy)
			{
				CancelBackgroundWorker();
			}
			else
			{
				IsBusy = true;
				SetButtonText(false);

				int bits = 0;
				if (!int.TryParse(tbNumberOfBits.Text, out bits))
				{
					displayErrorMessage("Error parsing integer: Try entering only numeric characters in the input");
				}
				else if (bits < 7)
				{
					displayErrorMessage("If you need to find prime numbers smaller than seven 7 bits, use a non-specialized calculator.");
				}
				else
				{
					bitSize = bits;

					algorithmWorker = new ThreadedAlgorithmWorker(bitSize);
					algorithmWorker.WorkerComplete += algorithmWorker_WorkerComplete;
					algorithmWorker.SetLoggingBehavior(true, LoggingMethodType.Console);

					if (cancelSource != null)
					{
						cancelSource.Dispose();
						cancelSource = null;
					}

					if (cancelSource == null)
					{
						cancelSource = new CancellationTokenSource();
						cancelToken = cancelSource.Token;
					}

					algorithmWorker.StartWorker(cancelToken);
				}
			}
		}

		private void CancelBackgroundWorker()
		{
			cancelSource.Cancel();
		}

		private void displayErrorMessage(string message)
		{
			MessageBox.Show(message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
		}

		private void SetButtonText(bool enable)
		{
			if (enable)
			{
				btnSearch.Text = "Search for primes...";
			}
			else
			{
				btnSearch.Text = "Cancel";
			}
		}

		void algorithmWorker_WorkerComplete(object sender, RunWorkerCompletedEventArgs e)
		{
			if (e.Error != null)
			{
				MessageBox.Show(e.Error.ToString(), "Exception Message", MessageBoxButtons.OK, MessageBoxIcon.Warning);
			}
			else if (e.Cancelled)
			{
				tbOutput.AppendText("Task was canceled.");
			}
			else
			{
				BigInteger prime1 = (BigInteger)e.Result;

				string primeNumber = prime1.ToString();
				string log10prime = (Math.Round(BigInteger.Log10(prime1))).ToString("F0");
				string log2prime = (Math.Round(Algorithm.Log2(prime1))).ToString("F0");

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

				tbOutput.AppendText(algorithmWorker.Log);
				tbOutput.AppendText(Environment.NewLine);

				//.... PRINT
				tbOutput.AppendText(string.Format("{1} bit prime ({2} digits):{0}", Environment.NewLine, log2prime, log10prime));
				tbOutput.AppendText(string.Format("{1}{0}{0}", Environment.NewLine, primeNumber));
				//tbOutput.AppendText(string.Format("Bits ({1}):{0}{2}{0}{0}", Environment.NewLine, log2prime, bitString1));

				//... Print run/processing time 
				if (algorithmWorker.RunTime != null && algorithmWorker.RunTime != TimeSpan.Zero)
				{
					tbOutput.AppendText(string.Format("Run time: {1}{0}", Environment.NewLine, ThreadedAlgorithmWorker.FormatTimeSpan(algorithmWorker.RunTime)));
					tbOutput.AppendText(string.Format("Time to render: {1}{0}", Environment.NewLine, ThreadedAlgorithmWorker.FormatTimeSpan(bitStringTime1)));
				}
			}

			IsBusy = false;
			SetButtonText(true);
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

		private void btnPrimalityTest_Click(object sender, EventArgs e)
		{
			string input = tbOutput.Text;
			BigInteger intTest = BigInteger.Parse(input);
			Algorithm alg = new Algorithm();
			DateTime startTime = DateTime.Now;
			bool result = alg.MillerRabinPrimalityTest(intTest, 20);
			TimeSpan timeElapsed = DateTime.Now.Subtract(startTime);
			alg.Dispose();
			tbOutput.Text = string.Format("Is prime: {0}{1}Time elapsed: {2}", result.ToString(), Environment.NewLine, ThreadedAlgorithmWorker.FormatTimeSpan(timeElapsed));
		}


	}
}
