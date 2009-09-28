using System;
using System.Drawing;
using System.Collections.Generic;
using System.Text;

namespace Gradient
{
    public sealed class GradientFill
    {
        // This method wraps the PInvoke to GradientFill.
        // Parmeters:
        //  gr - The Graphics object we are filling
        //  rc - The rectangle to fill
        //  startColor - The starting color for the fill
        //  endColor - The ending color for the fill
        //  fillDir - The direction to fill
        //
        // Returns true if the call to GradientFill succeeded; false
        // otherwise.
        public static bool Fill(
            Graphics gr,
            Rectangle rc,
            Color startColor, Color endColor,
            FillDirection fillDir)
        {

            // Initialize the data to be used in the call to GradientFill.
            Win32Helper.TRIVERTEX[] tva = new Win32Helper.TRIVERTEX[2];
            tva[0] = new Win32Helper.TRIVERTEX(rc.X, rc.Y, startColor);
            tva[1] = new Win32Helper.TRIVERTEX(rc.Right, rc.Bottom, endColor);
            Win32Helper.GRADIENT_RECT[] gra = new Win32Helper.GRADIENT_RECT[] {
    new Win32Helper.GRADIENT_RECT(0, 1)};

            // Get the hDC from the Graphics object.
            IntPtr hdc = gr.GetHdc();

            // PInvoke to GradientFill.
            bool b;

            b = Win32Helper.GradientFill(
                    hdc,
                    tva,
                    (uint)tva.Length,
                    gra,
                    (uint)gra.Length,
                    (uint)fillDir);
            System.Diagnostics.Debug.Assert(b, string.Format(
                "GradientFill failed: {0}",
                System.Runtime.InteropServices.Marshal.GetLastWin32Error()));

            // Release the hDC from the Graphics object.
            gr.ReleaseHdc(hdc);

            return b;
        }

        // The direction to the GradientFill will follow
        public enum FillDirection
        {
            //
            // The fill goes horizontally
            //
            LeftToRight = Win32Helper.GRADIENT_FILL_RECT_H,
            //
            // The fill goes vertically
            //
            TopToBottom = Win32Helper.GRADIENT_FILL_RECT_V
        }
    }
}
