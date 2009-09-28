using System;

using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Drawing.Imaging;
using System.Drawing;
using System.IO;

namespace GraphicsLibs
{
    class DIB
    {
        protected class BITMAPFILEHEADER
        {
            // Constants used in the file data structure
            protected const ushort fBitmapFileDesignator = 19778;
            protected const uint fBitmapFileOffsetToData = 54;

            /// <summary>
            /// Creates a BITMAPFILEHEADER class instance from the information provided
            /// by a BITMAPINFOHEADER instance.
            /// </summary>
            /// <param name="infoHdr">Filled in bitmap info header</param>
            public BITMAPFILEHEADER(BITMAPINFOHEADER infoHdr)
            {
                // These are constant for our example
                bfType = fBitmapFileDesignator;
                bfOffBits = fBitmapFileOffsetToData;
                bfReserved1 = 0;
                bfReserved2 = 0;

                // Determine the size of the pixel data given the bit depth, width, and
                // height of the bitmap.  Note: Bitmap pixel data is always aligned to 4 byte
                // boundaries.
                uint bytesPerPixel = (uint)(infoHdr.biBitCount >> 3);
                uint extraBytes = ((uint)infoHdr.biWidth * bytesPerPixel) % 4;
                uint adjustedLineSize = bytesPerPixel * ((uint)infoHdr.biWidth + extraBytes);

                // Store the size of the pixel data
                sizeOfImageData = (uint)(infoHdr.biHeight) * adjustedLineSize;

                // Store the total file size
                bfSize = bfOffBits + sizeOfImageData;
            }

            /// <summary>
            /// Write the class data to a binary stream.
            /// </summary>
            /// <param name="bw">Target stream writer</param>
            public void Store(BinaryWriter bw)
            {
                // Must, obviously, maintain the proper order for file writing
                bw.Write(bfType);
                bw.Write(bfSize);
                bw.Write(bfReserved1);
                bw.Write(bfReserved2);
                bw.Write(bfOffBits);
            }

            public uint sizeOfImageData;	// Size of the pixel data
            public ushort bfType;				// File type designator "BM"
            public uint bfSize;				// File size in bytes
            public short bfReserved1;		// Unused
            public short bfReserved2;		// Unused
            public uint bfOffBits;			// Offset to get to pixel info
        }
        protected class BITMAPINFOHEADER
        {
            // Constants used in the info data structure
            const uint kBitmapInfoHeaderSize = 40;

            /// <summary>
            /// Creates a BITMAPINFOHEADER instance based on a the pixel bit depth, width,
            /// and height of the bitmap.
            /// </summary>
            /// <param name="bpp">Bits per pixel</param>
            /// <param name="w">Bitmap width (pixels)</param>
            /// <param name="h">Bitmap height (pixels)</param>
            public BITMAPINFOHEADER(short bpp, int w, int h)
            {
                biSize = kBitmapInfoHeaderSize;
                biWidth = w;			// Set the width
                biHeight = h;			// Set the height
                biPlanes = 1;			// Only use 1 color plane
                biBitCount = bpp;		// Set the bpp
                biCompression = 0;		// No compression for file bitmaps
                biSizeImage = 0;		// No compression so this can be 0
                biXPelsPerMeter = 0;	// Not used
                biYPelsPerMeter = 0;	// Not used
                biClrUsed = 0;			// Not used
                biClrImportant = 0;		// Not used
            }

            /// <summary>
            /// Write the class data to a binary stream.
            /// </summary>
            /// <param name="bw">Target stream writer</param>
            /// <param name="bFromDIB">Is this a memory DIB in memory?</param>
            /// true: Memory DIB so use compression to format pixels if 16-bit
            /// false: File DIB so do not use compression
            public void Store(BinaryWriter bw, bool bFromDIB)
            {
                // Must, obviously, maintain the proper order for file writing
                bw.Write(biSize);
                bw.Write(biWidth);
                bw.Write(biHeight);
                bw.Write(biPlanes);
                bw.Write(biBitCount);

                // Only use compression for memory DIB (file loads choke if this is used)
                if (bFromDIB && biBitCount == 16)
                    bw.Write(3);
                else
                    bw.Write(biCompression);

                bw.Write(biSizeImage);
                bw.Write(biXPelsPerMeter);
                bw.Write(biYPelsPerMeter);
                bw.Write(biClrUsed);
                bw.Write(biClrImportant);

                // RGBQUAD bmiColors[0]
                if (bFromDIB && biBitCount == 16)
                {
                    bw.Write((uint)0x7c00);		// red
                    bw.Write((uint)0x03e0);		// green
                    bw.Write((uint)0x001f);		// blue
                }
            }

            public uint biSize;				// Size of this structure
            public int biWidth;			// Width of bitmap (pixels)
            public int biHeight;			// Height of bitmap (pixels)
            public short biPlanes;			// Number of color planes
            public short biBitCount;			// Pixel bit depth
            public uint biCompression;		// Compression type
            public uint biSizeImage;		// Size of uncompressed image
            public int biXPelsPerMeter;	// Horizontal pixels per meter
            public int biYPelsPerMeter;	// Vertical pixels per meter
            public uint biClrUsed;			// Number of colors used
            public uint biClrImportant;		// Important colors
        }



        [DllImport("coredll.dll")]
        extern public static IntPtr LocalAlloc(uint uFlags, uint uBytes);

        [DllImport("coredll.dll", SetLastError = true)]
        static extern IntPtr CreateCompatibleDC(IntPtr hdc);

        [DllImport("coredll.dll", PreserveSig = true, SetLastError = true)]
        static extern IntPtr SelectObject(IntPtr hdc, IntPtr hgdiobj);

        [DllImport("coredll.dll")]
        static extern IntPtr CreateDIBSection(IntPtr hdc, [In] ref BITMAPINFOHEADER pbmi, uint iUsage, out IntPtr ppvBits, IntPtr hSection, uint dwOffset);

        [DllImport("coredll.dll")]
        private static extern IntPtr GetDesktopWindow();

        [DllImport("coredll", EntryPoint = "GetDC", SetLastError = true)]
        private static extern IntPtr GetDC(IntPtr hWnd);


        [StructLayout(LayoutKind.Sequential)]
        public struct RGBQUAD
        {
            public byte rgbBlue;
            public byte rgbGreen;
            public byte rgbRed;
            public byte rgbReserved;
        }



        public static Bitmap CreateDIB(int x, int y)
        {

            //Set up a BitmapHeader
            Int32 Depth = DetermineColorDepth.getdepth();
            BITMAPINFOHEADER bmpInfo = new BITMAPINFOHEADER((short) Depth, x, y);
            BITMAPFILEHEADER bmpFile = new BITMAPFILEHEADER(bmpInfo);


            byte[] buffer = new byte[bmpFile.bfSize];

            MemoryStream m = new MemoryStream(buffer);
            using (BinaryWriter writer = new BinaryWriter(m))
            {
                bmpFile.Store(writer);
                bmpInfo.Store(writer, false);
                m.Seek(0, SeekOrigin.Begin);
                return new Bitmap(m);
            }

        }

        private static void Store(BinaryWriter bw, BITMAPINFOHEADER bm)
        {
            bw.Write(bm.biSize);
            bw.Write(bm.biWidth);
            bw.Write(bm.biHeight);
            bw.Write(bm.biPlanes);
            bw.Write(bm.biBitCount);
            bw.Write(bm.biCompression);
            bw.Write(bm.biSizeImage);
            bw.Write(bm.biXPelsPerMeter);
            bw.Write(bm.biYPelsPerMeter);
            bw.Write(bm.biClrUsed);
            bw.Write(bm.biClrImportant);
        }
    }
}
