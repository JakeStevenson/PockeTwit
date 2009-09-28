using System;

using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace PockeTwit
{
    static class GlobalEventHandler
    {
        // Fields
        public static bool FriendsUpdating = false;
        public static bool MessagesUpdating = false;
        public static bool SearchesUpdating = false;
        // Delegates (1) 
        public delegate void delNoData(Yedda.Twitter.Account t, Yedda.Twitter.ActionType Action);
        public delegate void delTimelineIsFetching(TimelineManagement.TimeLineType TType);
        public delegate void delTimelineIsDone(TimelineManagement.TimeLineType TType);
        public delegate void delshowErrorMessage(string Message);
        public delegate void delEmpty();


        // Events (1) 

        public static event delEmpty PauseConnections;
        public static event delEmpty ResumeConnections;
        public static event delTimelineIsFetching TimeLineFetching;
        public static event delTimelineIsDone TimeLineDone;
        public static event delshowErrorMessage ShowErrorMessage = delegate { };

        private static uint thisPid;

        [DllImport("coredll.dll", SetLastError = true)]
        private static extern IntPtr GetForegroundWindow();



        [DllImport("coredll.dll", SetLastError = true)]
        public static extern uint GetWindowThreadProcessId(IntPtr hwnd, out uint lpdwProcessId);

        public static void setPid()
        {
            IntPtr forePtr = GetForegroundWindow();
            GetWindowThreadProcessId(forePtr, out thisPid);
        }

        public static bool isInForeground()
        {
            IntPtr forePtr = GetForegroundWindow();
            uint pID;
            GetWindowThreadProcessId(forePtr, out pID);
            return pID == thisPid;
        }

        static GlobalEventHandler()
        {
            if (System.IO.File.Exists(ClientSettings.AppPath + "\\commerrors.txt"))
            {
                System.IO.File.Delete(ClientSettings.AppPath + "\\commerrors.txt");
            }
        }

        public static void LogCommError(Exception ex)
        {
            try
            {
                using (System.IO.StreamWriter w = new System.IO.StreamWriter(ClientSettings.AppPath + "\\commerrors.txt", true))
                {
                    w.WriteLine("____");
                    w.WriteLine(DateTime.Now.ToString());
                    w.WriteLine(ex.Message);
                    w.WriteLine(ex.StackTrace);
                }
            }
            //Toss it if we can't write to the log
            catch { }
        }

        public static void CallShowErrorMessage(string Message, Exception ex)
        {
            using (System.IO.StreamWriter w = new System.IO.StreamWriter(ClientSettings.AppPath + "\\commerrors.txt", true))
            {
                w.WriteLine("____");
                w.WriteLine(DateTime.Now.ToString());
                w.WriteLine(ex.Message);
            }
            CallShowErrorMessage(Message);
        }
        public static void CallShowErrorMessage(string Message)
        {
            ShowErrorMessage(PockeTwit.Localization.XmlBasedResourceManager.GetString(Message));
        }

        public static void NotifyTimeLineFetching(TimelineManagement.TimeLineType TType)
        {
            if (TType == TimelineManagement.TimeLineType.Friends) { FriendsUpdating = true; }
            if (TType == TimelineManagement.TimeLineType.Messages) { MessagesUpdating = true; }
            if (TType== TimelineManagement.TimeLineType.Searches){ SearchesUpdating = true; }
            if (TimeLineFetching != null)
            {
                TimeLineFetching(TType);
            }
        }
        public static void NotifyTimeLineDone(TimelineManagement.TimeLineType TType)
        {
            if (TType == TimelineManagement.TimeLineType.Friends) { FriendsUpdating = false; }
            if (TType == TimelineManagement.TimeLineType.Messages) {MessagesUpdating = false; }
            if (TType == TimelineManagement.TimeLineType.Searches) { SearchesUpdating = false; }
            if (TimeLineDone != null)
            {
                TimeLineDone(TType);
            }
        }
        
        public static void PauseFetches()
        {
            PauseConnections();
        }
        public static void ResumeFetches()
        {
            ResumeConnections();
        }
    }
}
