using System;
using Microsoft.Win32;
using System.Collections.Generic;
using PockeTwit.SpecialTimelines;

namespace PockeTwit.NotificationsCode
{
    class NotificationHandler
    {
        public delegate void DelNotificationClicked();
        public event DelNotificationClicked MessagesNotificationClicked;
        private readonly christec.windowsce.forms.NotificationWithSoftKeys _messagesBubbler;
        [Flags]
        public enum Options
        {
            Sound = 1,
            Vibrate = 2,
            Flash = 4,
            Message = 8
        }
        public class NotificationInfoClass
        {
            public string Name;
            public string GUID;
            public Options Options;
            public string Sound;
            public TimelineManagement.TimeLineType Type;
            public ISpecialTimeLine Group;
            public string LastSeenID;

            public string  ListName
            {
                get
                {
                    if(Group!=null)
                    {
                        return Group.ListName;
                    }
                    if(Type==TimelineManagement.TimeLineType.Friends)
                    {
                        return "Friends_TimeLine";
                    }
                    return "Messages_TimeLine";
                }
            }

            public override string ToString()
            {
                return Name;
            }
        }

        private static Dictionary<string, NotificationInfoClass> _notifications = new Dictionary<string, NotificationInfoClass>();
        private static readonly NotificationTexts MessagesTexts = new NotificationTexts();
       
        public const string FriendsTweets = "{DF293090-5095-49ce-A626-AE6D6629437F}";
        public const string MessageTweets = "{B4D35E62-A83F-4add-B421-F7FC28E14310}";

        public NotificationHandler()
        {
            if (DetectDevice.DeviceType == DeviceType.Professional)
            {
                _messagesBubbler = new christec.windowsce.forms.NotificationWithSoftKeys
                                       {
                                           Icon = Properties.Resources.MyIco,
                                           Caption = "PockeTwit",
                                           RightSoftKey =
                                               new christec.windowsce.forms.NotificationSoftKey(
                                               christec.windowsce.forms.SoftKeyType.Dismiss, "Dismiss"),
                                           LeftSoftKey =
                                               new christec.windowsce.forms.NotificationSoftKey(
                                               christec.windowsce.forms.SoftKeyType.StayOpen, "Show")
                                       };
                _messagesBubbler.LeftSoftKeyClick += MessagesBubblerLeftSoftKeyClick;
                _messagesBubbler.Silent = true;
                _messagesBubbler.SpinnerClick += MessagesBubblerSpinnerClick;
            }
            LoadAll();
        }

        void MessagesBubblerSpinnerClick(object sender, christec.windowsce.forms.SpinnerClickEventArgs e)
        {
            if(e.Forward)
            {
                MessagesTexts.NextMessage();
            }
            else
            {
                MessagesTexts.PrevMessage();
            }
            _messagesBubbler.Text = MessagesTexts.GetMessage();
            _messagesBubbler.Caption = MessagesTexts.GetCaption();
        }

        public static NotificationInfoClass[] GetList()
        {
            var ret = new List<NotificationInfoClass>();
            foreach (var notification in _notifications.Values)
            {
                ret.Add(notification);
            }
            return ret.ToArray();
        }
        public static NotificationInfoClass GetItem(string GUID)
        {
            return _notifications[GUID];
        }

        private static void LoadAll()
        {
            _notifications = new Dictionary<string, NotificationInfoClass>();
            string name = PockeTwit.Localization.XmlBasedResourceManager.GetString("PockeTwit: Friends Update");
            var friends = new NotificationInfoClass {Name = name, Group = null, GUID = FriendsTweets, Type= TimelineManagement.TimeLineType.Friends};
            name = PockeTwit.Localization.XmlBasedResourceManager.GetString("PockeTwit: Messages");
            var messages = new NotificationInfoClass{Name = name, Group = null, GUID = MessageTweets, Type=TimelineManagement.TimeLineType.Messages};

            _notifications.Add(FriendsTweets, friends);
            _notifications.Add(MessageTweets, messages);

            foreach (var line in SpecialTimeLinesRepository.GetList())
            {
                AddSpecialTimeLineNotifications(line);
            }

            LoadAllRegistries();
        }

        private static void LoadAllRegistries()
        {
            foreach (var info in _notifications.Values)
            {
                RegistryKey infoKey = Registry.CurrentUser.CreateSubKey("\\ControlPanel\\Notifications\\" + info.GUID);
                LoadSettings(info, infoKey);
            }
        }

        public static void RemoveSpecialTimeLineNotifications(ISpecialTimeLine line)
        {
            if (_notifications.ContainsKey(line.name))
            {
                _notifications.Remove(line.name);
                try
                {
                    Registry.CurrentUser.DeleteSubKeyTree("\\ControlPanel\\Notifications\\" + line.name);
                }
                catch (Exception)
                {}
                LoadAllRegistries();
            }
        }
        public static void AddSpecialTimeLineNotifications(ISpecialTimeLine line)
        {
            if (!_notifications.ContainsKey(line.name))
            {
                var c = new NotificationInfoClass
                                              {
                                                  Name = "PockeTwit: " + line.name,
                                                  Group = line,
                                                  GUID = line.name,
                                                  Type = TimelineManagement.TimeLineType.Friends
                                              };
                _notifications.Add(c.GUID, c);
                LoadAllRegistries();
            }
        }

        public void ShutDown()
        {
            DismissBubbler();
        }


/*
        void RegistryWatcher_Changed(object sender, Microsoft.WindowsMobile.Status.ChangeEventArgs args)
        {
            LoadAll();
        }
*/

        

        void MessagesBubblerLeftSoftKeyClick(object sender, EventArgs e)
        {
            DismissBubbler();
            if (MessagesNotificationClicked != null)
            {
                MessagesNotificationClicked();
            }
        }

        public void DismissBubbler()
        {
            if (_messagesBubbler != null)
            {
                _messagesBubbler.Visible = false;
            }
        }



        public static void LoadSettings(NotificationInfoClass infoClass, RegistryKey key)
        {
            try
            {
                infoClass.Sound = (string)key.GetValue("Wave");
                if (infoClass.Sound == null)
                {
                    key.SetValue("Wave", "");
                }
                if (key.GetValue("Options") != null)
                {
                    infoClass.Options = (Options)key.GetValue("Options");
                }
                else
                {
                    key.SetValue("Options", 0);
                }

                
            }
            catch (NullReferenceException)
            {
            }
        }

        public static void SaveSettings(NotificationInfoClass infoSet)
        {
            RegistryKey theKey = Registry.CurrentUser.OpenSubKey("\\ControlPanel\\Notifications\\" + infoSet.GUID, true) ??
                                 Registry.CurrentUser.CreateSubKey("\\ControlPanel\\Notifications\\" + infoSet.GUID);
            if (infoSet.Sound != null)
            {
                if (theKey != null) theKey.SetValue("Wave", infoSet.Sound);
            }

            if (theKey != null) theKey.SetValue("Options", (int)infoSet.Options);
        }

        private void ShowNotifications()
        {
            if (_messagesBubbler == null) { return; }
            if (GlobalEventHandler.isInForeground()) { return; }
            _messagesBubbler.Spinners = MessagesTexts.MultipleMessages();
            if (!_messagesBubbler.Visible)
            {
                _messagesBubbler.Text = MessagesTexts.GetMessage();
                _messagesBubbler.Caption = MessagesTexts.GetCaption();
                _messagesBubbler.Visible = true;
            }
        }


        //This gets called every time a fetch finds new items.
        public void NewItems()
        {
            foreach (var infoClass in _notifications.Values)
            {
                if(TimeLines.LastSelectedItems.GetUnreadItems(infoClass.ListName)>0)
                {
                    string constraints ="";
                    if (infoClass.Group != null) { constraints = infoClass.Group.GetConstraints(); }
                    if (infoClass.LastSeenID != LocalStorage.DataBaseUtility.GetNewestItem(infoClass.Type, constraints))
                    {
                        if ((infoClass.Options & Options.Vibrate) == Options.Vibrate && SoundProfileCheck.VibrateOn())
                        {
                            VibrateStart();
                            if ((infoClass.Options & Options.Sound) == Options.Sound)
                            {
                                var s = new Sound(infoClass.Sound);
                                s.Play();
                            }
                            else
                            {
                                System.Threading.Thread.Sleep(1000);
                            }
                            VibrateStop();
                        }
                        else if ((infoClass.Options & Options.Sound) == Options.Sound && SoundProfileCheck.VolumeOn())
                        {
                            var s = new Sound(infoClass.Sound);
                            s.Play();
                        }

                        if ((infoClass.Options & Options.Message) == Options.Message)
                        {
                            ShowNotifications();
                        }
                        infoClass.LastSeenID = LocalStorage.DataBaseUtility.GetNewestItem(infoClass.Type, constraints);
                    }
                }
            }
        }
        



        #region VibrateCode
        private void VibrateStop()
        {
            _vib.SetLedStatus(1, Led.LedState.Off);
        }
        
        void VibrateStart() 
        {
            _vib.SetLedStatus(1, Led.LedState.On);
        }
        #endregion
        private readonly Led _vib = new Led();
    }
}