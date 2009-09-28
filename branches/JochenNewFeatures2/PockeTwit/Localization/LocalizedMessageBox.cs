using System;

using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace PockeTwit.Localization
{
    public class LocalizedMessageBox
    {
        public static DialogResult Show(string text)
        {
            return Show(text, "", MessageBoxButtons.OKCancel, MessageBoxIcon.None,
                                MessageBoxDefaultButton.Button1, null);
        }

        public static DialogResult Show(string text, string caption)
        {
            return Show(text, caption, MessageBoxButtons.OKCancel, MessageBoxIcon.None,
                                MessageBoxDefaultButton.Button1, null);
        }

        public static DialogResult Show(string text, params object[] args)
        {
            return Show(text, "", MessageBoxButtons.OKCancel, MessageBoxIcon.None,
                                MessageBoxDefaultButton.Button1, args);
        }


        public static DialogResult Show(string text, string caption, params object[] args)
        {
            return Show(text, caption, MessageBoxButtons.OKCancel, MessageBoxIcon.None,
                                MessageBoxDefaultButton.Button1, args);
            
        }

        public static DialogResult Show(string text, string caption, MessageBoxButtons buttons,
            MessageBoxIcon icon, MessageBoxDefaultButton defaultButton, params object[] args)
        {
            var translatedText = Localization.XmlBasedResourceManager.GetString(text);
            if (args!=null)
                translatedText = string.Format(translatedText, args);

            var translatedCaption = Localization.XmlBasedResourceManager.GetString(caption);
            return MessageBox.Show(translatedText, translatedCaption, buttons, icon, defaultButton);
        }
    }
}
