using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Text;

namespace PockeTwit.Themes
{
    class FormColors
    {
        public static System.Drawing.Bitmap GetThemeIcon(string iconName)
        {
            if (!System.IO.File.Exists(ClientSettings.IconsFolder() + iconName))
            {
                System.IO.File.Copy(ClientSettings.AppPath + "\\Themes\\Original\\" + iconName, ClientSettings.IconsFolder() + iconName);
            }
            
            return new System.Drawing.Bitmap(ClientSettings.IconsFolder() + iconName);
        }
        public static void SetColors(Control f)
        {
            foreach (Control c in f.Controls)
            {
                if (c is TextBox || c is ListBox || c is ListView || c is ComboBox || c is Panel || c is ProgressBar)
                {
                    c.ForeColor = ClientSettings.FieldForeColor;
                    c.BackColor = ClientSettings.FieldBackColor;
                }
                else if (c is LinkLabel)
                {
                    c.ForeColor = ClientSettings.LinkColor;
                }
                else
                {
                    c.ForeColor = ClientSettings.ForeColor;
                    c.BackColor = ClientSettings.BackColor;
                }
            }
            f.ForeColor = ClientSettings.ForeColor;
            f.BackColor = ClientSettings.BackColor;
        }
        public static void SetColors(Form f)
        {
            foreach (Control c in f.Controls)
            {
                if (c is TextBox || c is ListBox || c is ListView || c is ComboBox || c is Panel)
                {
                    c.ForeColor = ClientSettings.FieldForeColor;
                    c.BackColor = ClientSettings.FieldBackColor;
                }
                else if (c is LinkLabel)
                {
                    c.ForeColor = ClientSettings.LinkColor;
                }
                else
                {
                    c.ForeColor = ClientSettings.ForeColor;
                    c.BackColor = ClientSettings.BackColor;
                }
            }
            f.ForeColor = ClientSettings.ForeColor;
            f.BackColor = ClientSettings.BackColor;
        }
    }
}
