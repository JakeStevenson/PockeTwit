using System;
using Microsoft.WindowsCE.Forms;

namespace PockeTwit
{
    //http://msdn.microsoft.com/en-us/library/microsoft.windowsce.forms.messagewindow.aspx

    // Derive MessageWindow to respond to
    // Windows messages and to notify the
    // form when they are received.
    public class MsgWindow : MessageWindow
    {
        public const int WM_COPYDATA = 0x004A;

        public unsafe struct COPYDATASTRUCT
        {
            public int dwData;
            public int cbData;
            public int lpData;
        }

        // Create an instance of the form.
        private TweetList parentForm;

        // Save a reference to the form so it can
        // be notified when messages are received.
        public MsgWindow(TweetList parentForm)
        {
            this.parentForm = parentForm;
            this.Text = "PockeTwitWndProc";
        }

        // Override the default WndProc behavior to examine messages.
        protected override void WndProc(ref Message msg)
        {
            if (msg.Msg == WM_COPYDATA)
            {
                unsafe
                {
                    COPYDATASTRUCT data = *((COPYDATASTRUCT*)msg.LParam);
                    if (data.dwData == 7)
                    {
                        string s = new string((char*)(data.lpData), 0, data.cbData / sizeof(char));
                        this.parentForm.ProcessArgs(s.Split('\0'));
                    }
                }
            }
            // Call the base WndProc method
            // to process any messages not handled.
            base.WndProc(ref msg);
        }
    }
}
