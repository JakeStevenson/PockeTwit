using System;

using System.IO;
using System.Runtime.InteropServices;

using System.Collections.Generic;
using System.Text;

namespace PockeTwit
{
    public class Sound
    {
        private static class CheckAYG
        {
            static CheckAYG()
            {
                if (System.IO.File.Exists("Windows\\aygshell.dll"))
                {
                    AYGExists = true;
                }
            }
            public static bool AYGExists = false;
        }

        internal sealed class SafeNativeMethods
        {
            [DllImport("coredll.dll", SetLastError = true)]
            public static extern IntPtr LocalFree(IntPtr hMem);

            [DllImport("aygshell.dll")]
            internal static extern uint SndPlaySync(string file, uint flags);

            [DllImport("aygshell.dll")]
            internal static extern uint SndOpen(string file, ref IntPtr phSound);

            [DllImport("aygshell.dll")]
            internal static extern uint SndPlayAsync(IntPtr hSound, uint flags);

            [DllImport("aygshell.dll")]
            internal static extern uint SndStop(int soundScope, IntPtr hSound);

            [DllImport("aygshell.dll")]
            internal static extern uint SndClose(IntPtr hSound);

            [DllImport("aygshell.dll")]
            internal static extern uint SndGetSoundDirectoriesList(
                uint soundEvent,
                uint location,
                ref IntPtr soundDirectories,
                ref IntPtr directoriesCount);
        }


        private string m_fileName;

        private enum Flags
        {
            SND_SYNC = 0x0000,  /* play synchronously (default) */
            SND_ASYNC = 0x0001,  /* play asynchronously */
            SND_NODEFAULT = 0x0002,  /* silence (!default) if sound not found */
            SND_MEMORY = 0x0004,  /* pszSound points to a memory file */
            SND_LOOP = 0x0008,  /* loop the sound until next sndPlaySound */
            SND_NOSTOP = 0x0010,  /* don't stop any currently playing sound */
            SND_NOWAIT = 0x00002000, /* don't wait if the driver is busy */
            SND_ALIAS = 0x00010000, /* name is a registry alias */
            SND_ALIAS_ID = 0x00110000, /* alias is a predefined ID */
            SND_FILENAME = 0x00020000, /* name is file name */
            SND_RESOURCE = 0x00040004  /* name is resource name or atom */
        }


        [DllImport("CoreDll.DLL", EntryPoint = "PlaySoundW", SetLastError = true)]
        private extern static int WCE_PlaySound(string szSound, IntPtr hMod, int flags);

        [DllImport("CoreDll.DLL", EntryPoint = "PlaySoundW", SetLastError = true)]
        private extern static int WCE_PlaySoundBytes(byte[] szSound, IntPtr hMod, int flags);


        /// <summary>
        /// Construct the Sound object to play sound data from the specified file.
        /// </summary>
        public Sound(string fileName)
        {
            m_fileName = fileName;
        }

        
        /// <summary>
        /// Play the sound
        /// </summary>
        public void Play()
        {

            if (CheckAYG.AYGExists)
            {
                try
                {
                    SafeNativeMethods.SndPlaySync(m_fileName, 0);
                }
                catch
                {
                    CheckAYG.AYGExists = false;
                    WCE_PlaySound(m_fileName, IntPtr.Zero, (int)(Flags.SND_FILENAME));
                }
            }
            else
            {
                WCE_PlaySound(m_fileName, IntPtr.Zero, (int)(Flags.SND_FILENAME));
            }
        }
    }
}
