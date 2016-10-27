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
using System.Threading.Tasks;

namespace MaurerWinform
{
    public partial class MainForm : Form
    {
        public bool IsBusy { get { return _isBusy; } private set { _isBusy = value; } }
        private volatile bool _isBusy = false;

        public int SearchDepth { get { return int.TryParse(StripNonnumericCharacters(tbSearchDepth.Text), out _searchDepth) ? _searchDepth : DefaultSearchDepth; } }
        private int _searchDepth = 0;

        //private CancellationToken cancelToken;
        private CancellationTokenSource cancelSource;
        private ThreadedAlgorithmWorker algorithmWorker;
        private System.Windows.Forms.Timer testPrimalityChecker;

        private static readonly int DefaultSearchDepth = Settings.Search_Depth;
        private static readonly string Numbers = "0123456789";
        private static readonly string MessageBoxCaption = "Oops!";
        private static readonly string PrimalityTestInstructions = Environment.NewLine + "Please enter the number you wish to factor into the input TextBox.";
        private static readonly string MultiplyInstructions = Environment.NewLine + "Please put the two numbers you wish to multiply onto separate lines into the input TextBox.";
        private static readonly string TrialDivisionInstructions = Environment.NewLine + "Please put the number you wish to divide into the input TextBox.";
        private static readonly string JacobiInstructions = Environment.NewLine + "Input accepted in the form of: D/n";

        public MainForm()
        {
            InitializeComponent();
            tbInput.Text = "512";
            tbSearchDepth.Text = DefaultSearchDepth.ToString();
            testPrimalityChecker = new System.Windows.Forms.Timer();
            Log.LogFilename = Settings.LogFile_Methods;
            Log.SetLoggingPreference(Settings.Logging_Enabled);
        }

        public void WriteOutputLine(string message, bool atBegining = false)
        {
            WriteOutput(message + Environment.NewLine, atBegining);
        }

        public void WriteOutput(string message, bool atBegining = false)
        {
            if (tbOutput.InvokeRequired)
            {
                tbOutput.Invoke(new MethodInvoker(() => { WriteOutput(message); }));
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(message))
                {
                    if (atBegining)
                    {
                        tbOutput.Text = tbOutput.Text.Insert(0, message);
                    }
                    else
                    {
                        tbOutput.AppendText(message);
                    }
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
                    algorithmWorker = new ThreadedAlgorithmWorker(SearchDepth, Settings.Logging_Enabled);
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

        private string StripNonnumericCharacters(string input)
        {
            return string.IsNullOrEmpty(input) ? "" : new string(input.Where(c => Numbers.Contains(c)).ToArray());
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


        //System.Windows.Forms.Timer testPrimalityChecker = new System.Windows.Forms.Timer();
        private void btnTestPrimalityBrowse_Click(object sender, EventArgs e)
        {
            string filename = "";
            using (OpenFileDialog fileDialog = new OpenFileDialog())
            {
                if (fileDialog.ShowDialog() == DialogResult.OK)
                {
                    filename = fileDialog.FileName;
                }
            }

            if (string.IsNullOrWhiteSpace(filename) || !File.Exists(filename)) { return; }

            string[] lines = File.ReadAllLines(filename);

            int counter = 1;
            foreach (string line in lines)
            {
                BigInteger temp = BigInteger.Parse(line);

                bool isPrime = MillerRabin.CompositeTest(temp, SearchDepth);

                tbOutput.AppendText(string.Format("Line #{0} : {1}{2}", counter.ToString().PadRight(4, ' '), isPrime ? "Prime" : "Composite", Environment.NewLine));

                counter++;
            }
        }


        private List<string> GetInputLines()
        {
            List<string> result = tbInput.Lines.Where(s => !string.IsNullOrWhiteSpace(s)).Select(s => StripNonnumericCharacters(s)).ToList();

            if (tbInput.Lines.Any(s => s.Any(c => !Numbers.Contains(c))))  // Check for non-numerical characters
            {
                DisplayErrorMessage("Some lines contain non-numeric characters; these will be stripped out automatically. Please review the output carefully before relying on it.");
            }
            return result;
        }

        private void btnTestPrimality_Click(object sender, EventArgs e)
        {
            if (IsBusy)
            {
                return;
            }

            List<string> inputLines = GetInputLines();

            DateTime startTime = DateTime.Now;

            List<Task<bool>> taskQueue = new List<Task<bool>>();
            foreach (string line in inputLines)
            {
                int searchDepth = SearchDepth;
                BigInteger number = BigInteger.Parse(line);
                Task<bool> newTask = Task.Run(() => MillerRabin.CompositeTest(number, searchDepth));
                taskQueue.Add(newTask);
            }

            IsBusy = true;

            tbOutput.Clear();

            Thread compositeTestThread = new Thread(() =>
            {
                List<Task<bool>> taskList = new List<Task<bool>>(taskQueue);
                int counter = 1;
                foreach (Task<bool> tsk in taskList)
                {
                    bool result = tsk.Result;
                    WriteOutputLine(string.Format("#{0}: {1}", counter.ToString().PadRight(3, ' '), result.ToString()));
                    counter++;
                }
                MainForm.ActiveForm.Invoke(new MethodInvoker(() => IsBusy = false));
            });
            compositeTestThread.Start();
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

        private void btnDivide_Click(object sender, EventArgs e)
        {
            if (IsBusy)
            {
                return;
            }

            string filename = "";
            using (OpenFileDialog fileDialog = new OpenFileDialog())
            {
                if (fileDialog.ShowDialog() == DialogResult.OK)
                {
                    filename = fileDialog.FileName;
                }
            }

            if (string.IsNullOrWhiteSpace(filename))
            {
                return;
            }

            List<string> inputLines = GetInputLines();

            string[] fileLines = File.ReadAllLines(filename);
            if (fileLines == null || fileLines.Length < 1)
            {
                return;
            }

            List<BigInteger> dividends = inputLines.Select(s => BigInteger.Parse(StripNonnumericCharacters(s))).ToList();
            IEnumerable<BigInteger> divisors = fileLines.Select(s => BigInteger.Parse(StripNonnumericCharacters(s)));

            BigInteger quotient = new BigInteger();
            BigInteger remainder = new BigInteger();

            foreach (BigInteger divisor in divisors)
            {
                foreach (BigInteger dividend in dividends)
                {
                    quotient = BigInteger.DivRem(dividend, divisor, out remainder);

                    if (remainder == BigInteger.Zero)
                    {
                        WriteOutputLine(string.Format("{0} / {1} = {2}", dividend, divisor, quotient));
                    }
                    //else
                    //{
                    //    WriteOutput(".");
                    //}
                }
            }

            WriteOutputLine("--- END ---");
        }
    }
}
