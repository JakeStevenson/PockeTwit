using System;

using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace GraphicsLibs
{
    class DetermineColorDepth
    {
        public static  Int32 getdepth()
        {
            IntPtr deviceHandle = GetDC(IntPtr.Zero);
            if (IntPtr.Zero == deviceHandle)
            {
                return 0;
            }

            // if we get here, we can query the device capabilities
            Int32 colorDepth = GetDeviceCaps(deviceHandle, BitsPerPixel);

            
            ReleaseDC(IntPtr.Zero, deviceHandle);
            return colorDepth;
        }
    
        // GetDeviceCaps index value(s)
        private const Int32 BitsPerPixel = 12;

        // pinvoke methods
        [DllImport("coredll.dll", SetLastError=true)]
        private extern static IntPtr GetDC(IntPtr hwnd);
       
        [DllImport("coredll.dll", SetLastError=true)]
        private extern static Int32 ReleaseDC(IntPtr hwnd,
                                            IntPtr hdc);

        [DllImport("coredll.dll", SetLastError=true)]
        private extern static Int32 GetDeviceCaps(IntPtr hdc,
                                                Int32 index);
    }
}
