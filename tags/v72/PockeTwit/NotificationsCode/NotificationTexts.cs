using System;

using System.Collections.Generic;
using System.Text;
using Microsoft.Win32;
using Microsoft.WindowsMobile.Status;

namespace PockeTwit
{
    class NotificationTexts
    {
        private const string UnreadCountRegistryPath = @"\Software\Apps\JustForFun PockeTwit\UnreadCount\";
        private RegistryKey UnreadCountRoot;
        private List<string> _Messages = new List<string>();
        private int _Position = 0;


        public NotificationTexts()
        {

            UnreadCountRoot = Registry.LocalMachine.OpenSubKey(UnreadCountRegistryPath, true);
            if (UnreadCountRoot == null)
            {
                RegistryKey ParentKey = Registry.LocalMachine.OpenSubKey(@"\Software\Apps\", true);
                if (ParentKey != null) UnreadCountRoot = ParentKey.CreateSubKey("JustForFun PockeTwit\\UnreadCount");
            }
            TimeLines.LastSelectedItems.UnreadCountChanged += new PockeTwit.TimeLines.LastSelectedItems.delUnreadCountChanged(LastSelectedItems_UnreadCountChanged);
        }

        void LastSelectedItems_UnreadCountChanged(string TimeLine, int Count)
        {
            _Messages.Clear();
            foreach (var UnreadName in UnreadCountRoot.GetValueNames())
            {
                if (UnreadName != "UnreadCountChanged")
                {
                    int unreadCount = (int) UnreadCountRoot.GetValue(UnreadName);
                    if (unreadCount > 0)
                    {
                        _Messages.Add(GetMessagesText(unreadCount + " unread messages in " + UnreadName + "."));
                    }
                }
            }
        }

        
        public  void NextMessage()
        {
            _Position++;
            if( _Position>= _Messages.Count)
            {
                _Position = 0;
            }
        }
        public void PrevMessage()
        {
            _Position--;
            if(_Position<0)
            {
                _Position = _Messages.Count;
            }
        }
        public string GetMessage()
        {
            if (_Messages.Count > 0)
            {
                return _Messages[_Position];
            }
            return null;
        }
        public string GetCaption()
        {
            if(_Messages.Count>1)
            {
                return "PockeTwit\t" + (_Position+1) + " of " + _Messages.Count;
            }
            return "PockeTwit";
        }
        public bool MultipleMessages()
        {
            return _Messages.Count > 1;
        }

        private string GetMessagesText(string sentence)
        {
            System.Text.StringBuilder HTMLString = new StringBuilder();
            HTMLString.Append("<html><body>");
            HTMLString.Append(sentence);
            HTMLString.Append("</body></html>");
            return HTMLString.ToString();
        }

    }
}
