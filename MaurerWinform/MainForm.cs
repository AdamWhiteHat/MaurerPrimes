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
        private static readonly string Numbers = "-0123456789";
        private static readonly string MessageBoxCaption = "Oops!";
        private static readonly string FinishMessage = "Finished";
        private static readonly string PrimalityTestInstructions = Environment.NewLine + "Please enter the number you wish to factor into the input TextBox.";
        private static readonly string MultiplyInstructions = Environment.NewLine + "Please put two or more numbers you wish to multiply on separate lines into the input TextBox.";
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


        #region HelperFunctions

        public void WriteOutputTextboxLine(string message, bool atBegining = false)
        {
            WriteOutputTextbox(message + Environment.NewLine, atBegining);
        }

        public void WriteOutputTextbox(string message, bool atBegining = false)
        {
            if (tbOutput.InvokeRequired)
            {
                tbOutput.Invoke(new MethodInvoker(() => { WriteOutputTextbox(message); }));
            }
            else
            {
                if (string.IsNullOrWhiteSpace(message))
                {
                    return;
                }

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

        private void WriteMessageBox(string message, string caption = "")
        {
            MessageBox.Show(message, string.IsNullOrWhiteSpace(caption) ? MessageBoxCaption : caption, MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private List<BigInteger> GetInputNumbers()
        {
            List<BigInteger> result = tbInput.Lines.Where(s => !string.IsNullOrWhiteSpace(StripNonnumericCharacters(s))).Select(s => BigInteger.Parse(s)).ToList();

            if (tbInput.Lines.Any(s => s.Any(c => !Numbers.Contains(c))))  // Check for non-numerical characters
            {
                // A MessageBox which could hold the thread for a long time is probably not expected from such a function
                // instead of remove the warning, we can just launch in thread.
                new Thread(() =>
                {
                    WriteMessageBox("Some lines contain non-numeric characters; these will be stripped out automatically. Please review the output carefully before relying on it.");
                })
                .Start();
            }

            return result;
        }

        private string StripNonnumericCharacters(string input)
        {
            return string.IsNullOrEmpty(input) ? "" : new string(input.Where(c => Numbers.Contains(c)).ToArray()).TrimStart('0');
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

        private void SetButtonText(bool enable, string resetText = "")
        {
            btnSearch.Text = enable ? resetText : "Cancel";
        }

        #endregion

        #region Hotkeys

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

        #endregion

        #region Background Worker

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

        private void FindPrimes_WorkerComplete(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                WriteMessageBox(e.Error.ToString(), "Exception Message");
            }
            else if (e.Cancelled)
            {
                WriteOutputTextboxLine("Task was canceled.", true);
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
                WriteOutputTextboxLine("");

                //.... PRINT
                WriteOutputTextboxLine(string.Format("{0} bit prime ({1} digits):", base2size, base10size));
                WriteOutputTextboxLine(primeNumber);
                //WriteOutputLine(string.Format("Bits ({0}):", log2prime));
                //WriteOutputLine(bitString1+Environment.NewLine);

                //... Print run/processing time 
                if (algorithmWorker.RuntimeTimer != null && algorithmWorker.RuntimeTimer != TimeSpan.Zero)
                {
                    WriteOutputTextboxLine(string.Format("Run time: {0}", ThreadedAlgorithmWorker.FormatTimeSpan(algorithmWorker.RuntimeTimer)));
                    WriteOutputTextboxLine(string.Format("Time to render: {0}", ThreadedAlgorithmWorker.FormatTimeSpan(bitStringTime1)));
                }
            }

            IsBusy = false;
            SetButtonText(true, "Search for primes...");
        }

        #endregion

        #region Form Button Events

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
                    WriteMessageBox("Error parsing number of bits: Try entering only numeric characters into the input TextBox.");
                }
                else if (bits < 7)
                {
                    WriteMessageBox("If you need to find prime numbers smaller than seven 7 bits, use a non-specialized calculator.");
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

        private void btnTestPrimality_Click(object sender, EventArgs e)
        {
            if (IsBusy)
            {
                return;
            }

            List<BigInteger> inputLines = GetInputNumbers();

            int searchDepth = SearchDepth;
            DateTime startTime = DateTime.Now;
            List<Task<bool>> taskList = new List<Task<bool>>();
            foreach (BigInteger number in inputLines)
            {
                Task<bool> newTask = Task.Run(() => MillerRabin.CompositeTest(number, searchDepth));
                taskList.Add(newTask);
            }

            IsBusy = true;
            tbOutput.Clear();

            // Thread to gather results
            new Thread(() =>
            {
                //List<Task<bool>> taskList = new List<Task<bool>>(taskLis);
                int counter = 1;
                foreach (Task<bool> tsk in taskList)
                {
                    bool result = tsk.Result;
                    WriteOutputTextboxLine(string.Format("#{0}: {1}", counter.ToString().PadRight(3, ' '), result.ToString()));
                    counter++;
                }
                MainForm.ActiveForm.Invoke(new MethodInvoker(() => IsBusy = false));
            }).Start();
        }

        private void btnMultiply_Click(object sender, EventArgs e)
        {
            tbInput.Text = CleanupString(tbInput.Text, false);
            if (string.IsNullOrWhiteSpace(tbInput.Text))
            {
                WriteMessageBox("Input empty! " + MultiplyInstructions);
            }
            else if (tbInput.Text.All(c => !Numbers.Contains(c)))
            {
                WriteMessageBox("No numeric input detected! " + MultiplyInstructions);
            }
            else
            {
                List<BigInteger> numbers = GetInputNumbers();

                if (numbers.Count < 2)
                {
                    WriteMessageBox("Need at least two numbers to perform the binary operation, multiplication. " + MultiplyInstructions);
                    return;
                }

                BigInteger a = BigInteger.MinusOne;
                BigInteger b = BigInteger.MinusOne;
                BigInteger result = BigInteger.MinusOne;

                a = numbers.First();
                numbers.Remove(a);

                while (numbers.Any())
                {
                    b = numbers.First();
                    numbers.Remove(b);

                    result = BigInteger.Multiply(a, b);

                    a = result;
                }

                WriteOutputTextboxLine(string.Format("=\n{0}", result.ToString()), true);
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

            string[] fileLines = File.ReadAllLines(filename);
            if (fileLines == null || fileLines.Length < 1)
            {
                return;
            }

            List<BigInteger> dividends = GetInputNumbers();
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
                        WriteOutputTextboxLine(string.Format("{0} / {1} = {2}", dividend, divisor, quotient));
                    }
                    //else
                    //{
                    //    WriteOutput(".");
                    //}
                }
            }

            WriteOutputTextboxLine(FinishMessage);
        }

        private static int lower = 2;
        private static int quantityPerRound = 10000000; // Ten million
        private int upper = lower + quantityPerRound;
        private bool isPrimesRunning = false;
        private CancellationTokenSource primesCancellationTokenSource;

        private void btnDumpPrimes_Click(object sender, EventArgs e)
        {
            if (isPrimesRunning)
            {
                primesCancellationTokenSource.Cancel();
            }
            else
            {
                string filename = "";
                using (SaveFileDialog fileDialog = new SaveFileDialog())
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

                isPrimesRunning = true;
                btnDumpPrimes.Text = "Stop";
                primesCancellationTokenSource = new CancellationTokenSource();

                CancellationToken cancelToken = primesCancellationTokenSource.Token;

                new Thread(() =>
                {
                    while (!cancelToken.IsCancellationRequested)
                    {
                        List<long> primes = Eratosthenes.Sieve(lower, upper);

                        File.AppendAllText(filename, string.Join(Environment.NewLine, primes.Select(l => l.ToString())));

                        primes.Clear();

                        lower = upper + 1;
                        upper = lower + quantityPerRound;
                    }

                    DumpPrimesCleanup();
                    WriteOutputTextboxLine(FinishMessage);

                }).Start();
            }
        }

        private void DumpPrimesCleanup()
        {
            isPrimesRunning = false;
            btnDumpPrimes.Invoke(new MethodInvoker(() => { btnDumpPrimes.Text = "Dump primes..."; }));

            if (primesCancellationTokenSource != null)
            {
                primesCancellationTokenSource.Dispose();
                primesCancellationTokenSource = null;
            }
        }

        private void WritePrimesAsBytes(List<long> primes, string filename)
        {
            File.WriteAllBytes(filename, primes.SelectMany(l => BitConverter.GetBytes(l)).ToArray());
        }

        private List<long> ReadPrimesAsBytes(string filename)
        {
            List<long> result = new List<long>();
            using (BinaryReader reader = new BinaryReader(File.Open(filename, FileMode.Open)))
            {
                while (reader.BaseStream.Position != reader.BaseStream.Length)
                {         
                    result.Add(reader.ReadInt64());              
                }
            }
            return result;
        }

        #endregion


    }
}
