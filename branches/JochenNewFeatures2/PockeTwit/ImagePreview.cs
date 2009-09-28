using System;

using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace PockeTwit
{
    public partial class ImagePreview : Form
    {
        private string fullURL;
        private string imagePathToShow = "";
        public ImagePreview(string ImageToShow, string FullURL)
        {
            InitializeComponent();
            menuZoom.Checked = ClientSettings.ZoomPreview;
            this.pictureBox1.Image = new Bitmap(this.pictureBox1.Width, this.pictureBox1.Height);
            PockeTwit.Themes.FormColors.SetColors(this);
            PockeTwit.Localization.XmlBasedResourceManager.LocalizeForm(this);
            if (ClientSettings.IsMaximized)
            {
                this.WindowState = FormWindowState.Maximized;
            }

            pictureBox1.Resize += new EventHandler(pictureBox1_Resize);
            fullURL = FullURL;
            imagePathToShow = ImageToShow;
            DrawPreview();
        }

        void pictureBox1_Resize(object sender, EventArgs e)
        {
            this.pictureBox1.Image.Dispose();
            this.pictureBox1.Image = new Bitmap(this.pictureBox1.Width, this.pictureBox1.Height);
            DrawPreview();
        }

        public Size GenerateImageDimensions(int currW, int currH, int destW, int destH)
        {
            //double to hold the final multiplier to use when scaling the image
            double multiplier = 0;

            //string for holding layout
            string layout;

            //determine if it's Portrait or Landscape
            if (currH > currW) layout = "portrait";
            else layout = "landscape";

            switch (layout.ToLower())
            {
                case "portrait":
                    //calculate multiplier on heights
                    if (destH > destW)
                    {
                        multiplier = (double)destW / (double)currW;
                    }

                    else
                    {
                        multiplier = (double)destH / (double)currH;
                    }
                    break;
                case "landscape":
                    //calculate multiplier on widths
                    if (destH > destW)
                    {
                        multiplier = (double)destW / (double)currW;
                    }

                    else
                    {
                        multiplier = (double)destH / (double)currH;
                    }
                    break;
            }

            //return the new image dimensions
            return new Size((int)(currW * multiplier), (int)(currH * multiplier));
        }

        private void DrawPreview()
        {
            if (string.IsNullOrEmpty(imagePathToShow))
            {
                return;
            }
            using (Bitmap imageToShow = new Bitmap(imagePathToShow))
            {
                if (menuZoom.Checked)
                {

                    DrawScaled(imageToShow);
                }
                else
                {
                    DrawOriginalSize(imageToShow);
                }
                pictureBox1.Refresh();
            }
        }

        private void DrawScaled(Bitmap imageToShow)
        {
            Size NewSize = GenerateImageDimensions(imageToShow.Width, imageToShow.Height, pictureBox1.Width, pictureBox1.Height);

            int leftOfImage = (pictureBox1.Image.Width - NewSize.Width) / 2;
            int topOfImage = (pictureBox1.Image.Height - NewSize.Height) / 2;

            Rectangle destRect = new Rectangle(leftOfImage, topOfImage, NewSize.Width, NewSize.Height);
            using (Graphics g = Graphics.FromImage(pictureBox1.Image))
            {
                g.Clear(ClientSettings.BackColor);
                g.DrawImage(imageToShow, destRect, new Rectangle(0, 0, imageToShow.Width, imageToShow.Height), GraphicsUnit.Pixel);
            }
        }

        private void DrawOriginalSize(Bitmap imageToShow)
        {
            int leftOfImage = (pictureBox1.Width - imageToShow.Width) / 2;
            int topOfImage = (pictureBox1.Height - imageToShow.Height) / 2;

            using (Graphics g = Graphics.FromImage(pictureBox1.Image))
            {
                g.Clear(ClientSettings.BackColor);
                g.DrawImage(imageToShow, leftOfImage, topOfImage);
            }
        }

        private void menuItem1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void menuItem2_Click(object sender, EventArgs e)
        {
            
        }

        private void menuItem4_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process p = new System.Diagnostics.Process();
            p.StartInfo.FileName = fullURL;
            p.StartInfo.UseShellExecute = true;
            p.Start();
        }

        private void menuItem3_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process p = new System.Diagnostics.Process();
            p.StartInfo.FileName = imagePathToShow;
            p.StartInfo.UseShellExecute = true;
            p.Start();
        }

        private void menuZoom_Click(object sender, EventArgs e)
        {
            menuZoom.Checked = !menuZoom.Checked;
            ClientSettings.ZoomPreview = menuZoom.Checked;
            DrawPreview();
        }
    }
}