using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Collections;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Reflection;

namespace PockeTwit
{

    public partial class ColorPick : Form
    {
        private string filename;
        private string ThemeName;
        ArrayList m_arraylist;
        public ColorPick(string Theme)
        {
            m_arraylist = new ArrayList();
            filename = ClientSettings.AppPath + "\\Themes\\" + Theme + "\\" + Theme + ".txt";
            ThemeName = Theme;
            InitializeComponent();
            if (ClientSettings.IsMaximized){this.WindowState = FormWindowState.Maximized;}
            PockeTwit.Themes.FormColors.SetColors(this);
            PockeTwit.Localization.XmlBasedResourceManager.LocalizeForm(this);

            LoadColorFile();
        }
        void LoadColorFile()
        {
            StreamReader Reader;
            try
            {
                Reader = new StreamReader(filename);
            }
            catch
            {
                StreamWriter writer = new StreamWriter(filename);
                writer.Write(
                    "BackColor:10:10:10\n" +
                    "ForeColor:211:211:211\n" +
                    "LinkColor:150:150:255\n" +
                    "SelectedBackColor:110:110:200\n" +
                    "SelectedForeColor:255:255:255\n" +
                    "SelectedSmallTextColor:200:200:200\n" +
                    "SmallTextColor:128:128:128\n" +
                    "ErrorColor:255:0:0\n" +
                    "FieldBackColor:255:255:255\n" +
                    "FieldForeColor:0:0:0\n");
                writer.Close();
                Reader = new StreamReader(filename);
            }
            
            string line = "";
            int pos = 0;
            while (true)
            {
                line = Reader.ReadLine();
                if (line == null)
                {
                    break;
                }
                string[] parsed = line.Split(new char[] { ':' });
                if (parsed.Length < 4)
                {
                    continue;
                }
                CreateColorItem(pos,parsed[0], parsed[1], parsed[2], parsed[3]);
                pos++;
            }
            Reader.Close();
        }
        void CreateColorItem(int pos,string strLabel, string strR, string strG, string strB)
        {
            int topPos = pos * (ClientSettings.TextSize + 10);
            int txtWidth = (ClientSettings.TextSize+5) * 2;
            if (strLabel.Length <= 2) return;
            Int16 R, G, B;
            try
            {
                R = Int16.Parse(strR);
                G = Int16.Parse(strG);
                B = Int16.Parse(strB);
            }
            catch
            {
                return;
            }
            Label lbl = new Label();
            lbl.Text = strLabel;
            lbl.Left = 5;
            lbl.Top = topPos;
            lbl.Width = ClientSettings.TextSize*8;
            lbl.ForeColor = ClientSettings.ForeColor;

            TextBox rTxt = new TextBox();
            rTxt.Text = R.ToString();
            rTxt.Top = topPos;
            rTxt.Left = lbl.Right;
            rTxt.Width = txtWidth;
            rTxt.ForeColor = ClientSettings.FieldForeColor;
            rTxt.BackColor = ClientSettings.FieldBackColor;
            
            lbl.Height = rTxt.Height;

            TextBox gTxt = new TextBox();
            gTxt.Text = G.ToString();
            gTxt.Top = topPos;
            gTxt.Left = rTxt.Right;
            gTxt.Width = txtWidth;
            gTxt.ForeColor = ClientSettings.FieldForeColor;
            gTxt.BackColor = ClientSettings.FieldBackColor;
            
            TextBox bTxt = new TextBox();
            bTxt.Text = B.ToString();
            bTxt.Top = topPos;
            bTxt.Left = gTxt.Right;
            bTxt.Width = txtWidth;
            bTxt.ForeColor = ClientSettings.FieldForeColor;
            bTxt.BackColor = ClientSettings.FieldBackColor;
            
            
            PictureBox pb = new PictureBox();
            pb.Left = bTxt.Right;
            pb.Top = topPos;
            pb.Width = ClientSettings.TextSize;
            pb.Height = bTxt.Height;
            pb.BackColor = Color.FromArgb(R, G, B);
            

            rTxt.TextChanged += new EventHandler(delegate(object sender, EventArgs e)
            {
                try
                {
                    int c = int.Parse(rTxt.Text) % 256;
                    pb.BackColor = Color.FromArgb(c, pb.BackColor.G, pb.BackColor.B);
                }
                catch { }
            });
            bTxt.TextChanged += new EventHandler(delegate(object sender, EventArgs e)
            {
                try
                {
                    int c = int.Parse(bTxt.Text) % 256;
                    pb.BackColor = Color.FromArgb(pb.BackColor.R, pb.BackColor.G, c);
                }
                catch { }
            });
            gTxt.TextChanged += new EventHandler(delegate(object sender, EventArgs e)
            {
                try
                {
                    int c = int.Parse(gTxt.Text) % 256;
                    pb.BackColor = Color.FromArgb(pb.BackColor.R, c, pb.BackColor.B);
                }
                catch { }
            });

            this.Controls.Add(rTxt);
            this.Controls.Add(gTxt);
            this.Controls.Add(bTxt);
            this.Controls.Add(lbl);
            this.Controls.Add(pb);
            ColorSetting cs=new ColorSetting(strLabel,pb);
            
            m_arraylist.Add(cs);

        }

               
        
        private void menuAccept_Click(object sender, EventArgs e)
        {
            if (System.IO.File.Exists(filename))
            {
                if (PockeTwit.Localization.LocalizedMessageBox.Show("That theme already exists, would you like to create a new one?", "New Theme?", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1) == DialogResult.Yes)
                {
                    string newFile = CloneTheme(filename);
                    if (newFile == filename)
                    {
                        return;
                    }
                    filename = newFile;
                }
            }
            string ColorSet = "";
            for (int i = 0; i < m_arraylist.Count; ++i)
            {
                ColorSetting cs = (ColorSetting)m_arraylist[i];
                ColorSet += cs.GetSetting();
            }
            try
            {
                StreamWriter writer = new StreamWriter(filename);
                writer.Write(ColorSet);
                writer.Close();

            }
            catch (Exception exp)
            {
                MessageBox.Show(exp.Message);
            }
            this.Close();
        }

        private string CloneTheme(string filename)
        {
            using (SettingsHandler.ThemeCreator.NewThemeName n = new PockeTwit.SettingsHandler.ThemeCreator.NewThemeName())
            {
                if (n.ShowDialog() == DialogResult.OK)
                {
                    string newName = n.ThemeName;
                    string newFolder = ClientSettings.AppPath + "\\Themes\\" + newName;
                    System.IO.Directory.CreateDirectory(ClientSettings.AppPath + "\\Themes\\" + newName);
                    foreach (string oldItem in System.IO.Directory.GetFiles(ClientSettings.AppPath + "\\Themes\\" + ThemeName))
                    {
                        if (System.IO.Path.GetExtension(oldItem) != ".txt")
                        {
                            System.IO.File.Copy(oldItem, newFolder + "\\" + System.IO.Path.GetFileName(oldItem));
                        }
                    }
                    return newFolder + "\\" + newName + ".txt";
                }
            }
            return filename;
        }

        private void menuCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
    class ColorSetting
    {
        public string name;
        public PictureBox pb;
        public ColorSetting(string strName, PictureBox pBox)
        {
            this.name = strName;
            this.pb = pBox;
        }
        public string GetSetting()
        {
            if (name != null || pb != null)
            {
                return name + ":" + pb.BackColor.R + ":" + pb.BackColor.G + ":" + pb.BackColor.B + "\r\n";
            }
            else
            {
                return "";
            }
        }
    }
}