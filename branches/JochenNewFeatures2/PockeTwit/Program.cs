using System;

using System.Collections.Generic;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using FingerUI;
using Microsoft.WindowsCE.Forms;

namespace PockeTwit
{
    static class Program
    {
        public static bool IgnoreDisposed = false;

		#region Methods (2) 

		// Private Methods (2) 
       
        [MTAThread]
        static void Main(string[] Args)
        {            
            bool bBackGround = false;
            if (Args.Length > 0)
            {
                string Arg =  Args[0];

                if (Arg == "/BackGround")
                {
                    bBackGround = true;
                }

                if (Arg == "/QuickPost")
                {
                    ClientSettings.LoadSettings();
                    if (ClientSettings.AccountsList.Count == 0)
                    {
                        PockeTwit.Localization.LocalizedMessageBox.Show("You must configure PockeTwit before using QuickPost.", "PockeTwit QuickPost");
                        return;
                    }
                    PostUpdate PostForm = new PostUpdate(true);
                    PostForm.AccountToSet = ClientSettings.DefaultAccount;
                    Application.Run(PostForm);
                    PostForm.Close();
                    return;
                }
            }
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);

            ClientSettings.LoadSettings();
            Application.Run(new TweetList(bBackGround, Args));
            LocalStorage.DataBaseUtility.CleanDB(10);

        }

        static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {

            string ErrorPath = ClientSettings.AppPath;
            Exception ex = (Exception)e.ExceptionObject;
            if (ex is ObjectDisposedException && IgnoreDisposed)
            {
                return;
            }
            if (ex is LowMemoryException)
            {
                PockeTwit.Localization.LocalizedMessageBox.Show("You do not currently have enough graphics memory to run PockeTwit.  Please close some applications or soft-reset and try again.", "Low Memory", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1);
                return;
            }
            System.Text.StringBuilder b = new System.Text.StringBuilder();
            b.Append("From v" + UpgradeChecker.currentVersion.ToString());
            b.Append("\r\n");
            b.Append(ex.Message);
            b.Append("\r\n");
            b.Append("_________________");
            b.Append("\r\n");
            b.Append(ex.StackTrace);
            b.Append("\r\n");
            if (ex.InnerException != null)
            {
                b.Append("\r\n");
                b.Append("\r\n");
                b.Append("Inner exception:");
                b.Append("\r\n");
                b.Append(ex.InnerException.Message);
                b.Append("\r\n");
                b.Append("_______________________");
                b.Append("\r\n");
                b.Append(ex.InnerException.StackTrace);
                b.Append("\r\n");
            }
            using (System.IO.StreamWriter w = new System.IO.StreamWriter(ErrorPath + "\\crash.txt"))
            {
                w.Write(b.ToString());
            }
            PockeTwit.Localization.LocalizedMessageBox.Show("An unexpected error has occured and PockeTwit must shut down.\n\nYou will have an opportunity to submit a crash report to the developer on the next run.", "PockeTwit");

        }
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
		#endregion Methods 

        }
}
