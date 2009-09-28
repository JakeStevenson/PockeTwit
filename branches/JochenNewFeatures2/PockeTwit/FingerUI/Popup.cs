using System;
using System.Drawing;
using System.Collections.Generic;
using System.Text;

namespace PockeTwit.FingerUI
{
    class Popup
    {
        
        public bool AtTop = false;
        private string _DisplayText;
        public Font TextFont;
        private int AnimationStep = -1;
        private int _AnimationPixels = -1;
        private int AnimationPixels
        {
            get
            {
                if (_AnimationPixels < 0)
                {
                    if (ClientSettings.TextHeight == 192)
                    {
                        _AnimationPixels = 4;
                    }
                    else
                    {
                        _AnimationPixels = 1;
                    }
                }
                return _AnimationPixels;
            }
        }
        private bool Visibility = false;
        private int MaxHeight = ClientSettings.TextSize + (ClientSettings.Margin * 2);
        public KListControl parentControl;
        
        public Popup()
        {
            
        }
        
        public void ShowNotification(string Text)
        {
            if (!Visibility)
            {
                _DisplayText = PockeTwit.Localization.XmlBasedResourceManager.GetString(Text);
                parentControl.startAnimation();
                AnimationStep = 0;
                Visibility = true;
            }
            else
            {
                if (Text != _DisplayText)
                {
                    _DisplayText = PockeTwit.Localization.XmlBasedResourceManager.GetString(Text);
                }
            }
        }

        public void HideNotification()
        {
            if (Visibility)
            {
                parentControl.startAnimation();
            }
            Visibility = false;
        }
        public bool isAnimating
        {
            get
            {
                return ((AnimationStep != -1 && AnimationStep!=MaxHeight));
            }
        }
        public void DrawNotification(Graphics g, int Bottom, int Width)
        {
            if (AnimationStep < 0) { return; }
            if (Visibility)
            {
                if (AnimationStep < MaxHeight) { AnimationStep = AnimationStep+AnimationPixels; }
                if (AnimationStep >= MaxHeight) 
                { 
                    AnimationStep = MaxHeight;
                }
            }
            else
            {
                if (AnimationStep > 0) 
                {
                    AnimationStep = AnimationStep - AnimationPixels;
                    if (AnimationStep <= 0)
                    {
                        AnimationStep = -1;
                    }
                }
                else 
                {
                    AnimationStep = -1;
                    //parentControl.stopAnimation(); 
                }
            }
            if (AnimationStep > 0)
            {
                Width = Width - (ClientSettings.Margin * 2);
                
                int Left = ClientSettings.Margin;
                using (Brush ForeBrush = new SolidBrush(ClientSettings.PopUpTextColor))
                {
                    using (Pen p = new Pen(ClientSettings.LineColor))
                    {
                        Rectangle boxPos;
                        Rectangle textPos;
                        if (AtTop)
                        {
                            boxPos = new Rectangle(Left, AnimationStep - (MaxHeight+3), Width, ClientSettings.TextSize + (ClientSettings.Margin * 2));
                            textPos = new Rectangle(Left + ClientSettings.Margin, (AnimationStep - (MaxHeight+3)) + ClientSettings.Margin, Width - ClientSettings.Margin, ClientSettings.TextSize + ClientSettings.Margin);
                        }
                        else
                        {
                            boxPos = new Rectangle(Left, Bottom - AnimationStep, Width, ClientSettings.TextSize + (ClientSettings.Margin * 2));
                            textPos = new Rectangle(Left + ClientSettings.Margin, (Bottom - AnimationStep) + ClientSettings.Margin, Width - ClientSettings.Margin, ClientSettings.TextSize + ClientSettings.Margin);
                        }
                        //Gradients are expensive -- will this make it run smoother?
                        //Gradient.GradientFill.Fill(g, boxPos, ClientSettings.SelectedBackColor, ClientSettings.SelectedBackGradColor, Gradient.GradientFill.FillDirection.TopToBottom);
                        using (Brush backBrush = new SolidBrush(ClientSettings.PopUpBackgroundColor))
                        {
                            g.FillRectangle(backBrush, boxPos);
                        }
                        g.DrawRectangle(p, boxPos);
                        g.DrawString(_DisplayText, TextFont, ForeBrush, textPos);
                    }
                }
            }
        }
    }
}