using System;

using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
namespace PockeTwit
{
    public class Led
    {
        // Fields
        private const int NLED_COUNT_INFO_ID = 0;
        private const int NLED_SETTINGS_INFO_ID = 2;
        private const int NLED_SUPPORTS_INFO_ID = 1;

        // Methods
        public Led()
        {
        }

        [DllImport("coredll.dll", SetLastError = true)]
        internal static extern bool NLedSetDevice(short nID, ref Led.NLED_SETTINGS_INFO pOutput);


        public void SetLedStatus(int led, LedState newState)
        {
            NLED_SETTINGS_INFO pOutput = new NLED_SETTINGS_INFO();
            pOutput.LedNum = led;
            pOutput.OffOnBlink = (int)newState;
            NLedSetDevice(2, ref pOutput);
        }

 


        // Properties
        public int Count { get; set; }

        // Nested Types
        public enum LedState
        {
            Off,
            On,
            Blink
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct NLED_COUNT_INFO
        {
            public uint cLeds;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct NLED_SETTINGS_INFO
        {
            public int LedNum;
            public int OffOnBlink;
            public int TotalCycleTime;
            public int OnTime;
            public int OffTime;
            public int MetaCycleOn;
            public int MetaCycleOff;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct NLED_SUPPORTS_INFO
        {
            public uint LedNum;
            public int lCycleAdjust;
            public bool fAdjustTotalCycleTime;
            public bool fAdjustOnTime;
            public bool fAdjustOffTime;
            public bool fMetaCycleOn;
            public bool fMetaCycleOff;
        }
    }


}
