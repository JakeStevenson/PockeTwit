using System;

using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace PockeTwit.FingerUI
{
    static public class DisplayItemDrawingHelper
    {
        public static void DrawItemBackground(Graphics g, Rectangle bounds, bool selected)
        {
            var innerBounds = new Rectangle(bounds.Left, bounds.Top, bounds.Width, bounds.Height);
            innerBounds.Offset(1, 1);
            innerBounds.Width--; innerBounds.Height--;
            var foreBrush = new SolidBrush(ClientSettings.ForeColor);
            if (selected)
            {
                foreBrush = new SolidBrush(ClientSettings.SelectedForeColor);
                if (ClientSettings.SelectedBackColor != ClientSettings.SelectedBackGradColor)
                {
                    try
                    {
                        Gradient.GradientFill.Fill(g, innerBounds, ClientSettings.SelectedBackColor, ClientSettings.SelectedBackGradColor, Gradient.GradientFill.FillDirection.TopToBottom);
                    }
                    catch
                    {
                        using (Brush backBrush = new SolidBrush(ClientSettings.SelectedBackColor))
                        {
                            g.FillRectangle(backBrush, innerBounds);
                        }
                    }
                }
                else
                {
                    using (Brush backBrush = new SolidBrush(ClientSettings.SelectedBackColor))
                    {
                        g.FillRectangle(backBrush, innerBounds);
                    }
                }
            }
            else
            {
                if (ClientSettings.BackColor != ClientSettings.BackGradColor)
                {
                    try
                    {
                        Gradient.GradientFill.Fill(g, innerBounds, ClientSettings.BackColor, ClientSettings.BackGradColor, Gradient.GradientFill.FillDirection.TopToBottom);
                    }
                    catch
                    {
                        using (Brush backBrush = new SolidBrush(ClientSettings.BackColor))
                        {
                            g.FillRectangle(backBrush, innerBounds);
                        }
                    }
                }
                else
                {
                    using (Brush backBrush = new SolidBrush(ClientSettings.BackColor))
                    {
                        g.FillRectangle(backBrush, innerBounds);
                    }
                }
            }
        }
    }
}
