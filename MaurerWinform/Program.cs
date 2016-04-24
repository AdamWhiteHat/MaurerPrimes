using System;
using System.IO;
using System.Windows.Forms;
using System.Runtime.ExceptionServices;

namespace MaurerWinform
{
    internal class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static internal void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());

			AppDomain.CurrentDomain.FirstChanceException += CurrentDomain_FirstChanceException;
        }

		static void CurrentDomain_FirstChanceException(object sender, FirstChanceExceptionEventArgs e)
		{
			File.AppendAllText("Exception.log.txt", string.Format("A {0} exception was caught with the message: \"{1}\".", e.Exception.GetType().Name, e.Exception.Message));
		}
    }
}
