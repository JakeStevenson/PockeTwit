using System;
using System.Threading;
using System.Runtime.InteropServices;

//---Based on code written by Dale Lane
//---Source: http://dalelane.co.uk/blog/?p=241
namespace PockeTwit
{
    struct CellTowerInformation
    {
        public uint CellID;
        public uint MCC;
        public uint LAC;

    };

    class RILWrapper
    {
        // string used to store the CellID string
        private static CellTowerInformation celltowerinfo;

        /*
         * Uses RIL to get CellID from the phone.
         */
        public static CellTowerInformation GetCellTowerInfo()
        {
            // initialise handles
            IntPtr hRil = IntPtr.Zero;
            IntPtr hRes = IntPtr.Zero;

            // initialise result
            celltowerinfo.CellID = celltowerinfo.MCC = celltowerinfo.LAC = 0;

            // initialise RIL
            hRes = RIL_Initialize(1,                      // RIL port 1
                new RILRESULTCALLBACK(rilResultCallback), // function to call with result
                null,                                     // function to call with notify
                0,                                        // classes of notification to enable
                0,                                        // RIL parameters
                out hRil);                                // RIL handle returned

            if (hRes != IntPtr.Zero)
            {
                return celltowerinfo;
            }

            // initialised successfully

            // use RIL to get cell tower info with the RIL handle just created
            hRes = RIL_GetCellTowerInfo(hRil);

            // wait for cell tower info to be returned
            waithandle.WaitOne();

            // finished - release the RIL handle
            RIL_Deinitialize(hRil);

            // return the result from GetCellTowerInfo
            return celltowerinfo;
        }

        // event used to notify user function that a response has
        //  been received from RIL
        private static AutoResetEvent waithandle = new AutoResetEvent(false);

        public static void rilResultCallback(uint dwCode,
                                             IntPtr hrCmdID,
                                             IntPtr lpData,
                                             uint cbData,
                                             uint dwParam)
        {
            // create empty structure to store cell tower info in
            RILCELLTOWERINFO rilCellTowerInfo = new RILCELLTOWERINFO();

            // copy result returned from RIL into structure
            Marshal.PtrToStructure(lpData, rilCellTowerInfo);

            // get the bits out of the RIL cell tower response that we want
            celltowerinfo.CellID = rilCellTowerInfo.dwCellID;
            celltowerinfo.LAC = rilCellTowerInfo.dwLocationAreaCode;
            celltowerinfo.MCC = rilCellTowerInfo.dwMobileCountryCode;

            // notify caller function that we have a result
            waithandle.Set();
        }

       // -------------------------------------------------------------------
       //  RIL function definitions
       // -------------------------------------------------------------------
        /* 
         * Function definition converted from the definition 
         *  RILRESULTCALLBACK from MSDN:
         * 
         * http://msdn2.microsoft.com/en-us/library/aa920069.aspx
         */
        public delegate void RILRESULTCALLBACK(uint dwCode,
                                               IntPtr hrCmdID,
                                               IntPtr lpData,
                                               uint cbData,
                                               uint dwParam);

        /*
         * Function definition converted from the definition 
         *  RILNOTIFYCALLBACK from MSDN:
         * 
         * http://msdn2.microsoft.com/en-us/library/aa922465.aspx
         */
        public delegate void RILNOTIFYCALLBACK(uint dwCode,
                                               IntPtr lpData,
                                               uint cbData,
                                               uint dwParam);

        /*
         * Class definition converted from the struct definition 
         *  RILCELLTOWERINFO from MSDN:
         * 
         * http://msdn2.microsoft.com/en-us/library/aa921533.aspx
         */
        public class RILCELLTOWERINFO
        {
            public uint cbSize;
            public uint dwParams;
            public uint dwMobileCountryCode;
            public uint dwMobileNetworkCode;
            public uint dwLocationAreaCode;
            public uint dwCellID;
            public uint dwBaseStationID;
            public uint dwBroadcastControlChannel;
            public uint dwRxLevel;
            public uint dwRxLevelFull;
            public uint dwRxLevelSub;
            public uint dwRxQuality;
            public uint dwRxQualityFull;
            public uint dwRxQualitySub;
            public uint dwIdleTimeSlot;
            public uint dwTimingAdvance;
            public uint dwGPRSCellID;
            public uint dwGPRSBaseStationID;
            public uint dwNumBCCH;
        }

       // -------------------------------------------------------------------
       //  RIL DLL functions 
       // -------------------------------------------------------------------

       /* Definition from: http://msdn2.microsoft.com/en-us/library/aa919106.aspx */
        [DllImport("ril.dll")]
        private static extern IntPtr RIL_Initialize(
            uint dwIndex,
            RILRESULTCALLBACK pfnResult,
            RILNOTIFYCALLBACK pfnNotify,
            uint dwNotificationClasses,
            uint dwParam,
            out IntPtr lphRil);

        /* Definition from: http://msdn2.microsoft.com/en-us/library/aa923065.aspx */
        [DllImport("ril.dll")]
        private static extern IntPtr RIL_GetCellTowerInfo(IntPtr hRil);

        /* Definition from: http://msdn2.microsoft.com/en-us/library/aa919624.aspx */
        [DllImport("ril.dll")]
        private static extern IntPtr RIL_Deinitialize(IntPtr hRil);

    }
}

