using System;

using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace PockeTwit.NotificationsCode
{
    class SoundProfileCheck
    {
        private enum SoundEvent
        {
            All = 0,
            RingLine1,
            RingLine2,
            KnownCallerLine1,
            RoamingLine1,
            RingVoip
        }
        public enum SoundType
        {
            On = 0,
            File = 1,
            Vibrate = 2,
            None = 3
        }
        private struct SNDFILEINFO
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string szPathNameNative;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string szDisplayNameNative;

            public SoundType SstType;
        }
        [DllImport("aygshell.dll", SetLastError = true)]
        private static extern uint SndGetSound(SoundEvent seSoundEvent, ref SNDFILEINFO pSoundFileInfo);

        public static bool VolumeOn()
        {
            SoundType current = GetCurrentProfile();
            return current == SoundType.On;
        }

        public static bool VibrateOn()
        {
            SoundType current = GetCurrentProfile();
            return current == SoundType.Vibrate || current == SoundType.On;
        }


        public static SoundType GetCurrentProfile()
        {
            SNDFILEINFO currentInfo = new SNDFILEINFO();
            uint ret = SndGetSound(SoundEvent.All, ref currentInfo);
            return currentInfo.SstType;
        }
    }
}
