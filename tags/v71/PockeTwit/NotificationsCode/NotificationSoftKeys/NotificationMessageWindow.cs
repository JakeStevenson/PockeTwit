using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace christec.windowsce.forms
{
  //exclude from documentation
#if !NDOC && !DESIGN
  using Microsoft.WindowsCE.Forms;

  /// <summary>
  /// Handles messages received from the Notification system and throws events in the parent NotificationEngine object
  /// </summary>
  internal class NotificationMessageWindow : MessageWindow
  {
    private Dictionary<int, NotificationWithSoftKeys> m_notifications;

    public NotificationMessageWindow(Dictionary<int, NotificationWithSoftKeys> notifications)
    {
      m_notifications = notifications;
    }

    protected override void WndProc(ref Message m)
    {
      switch (m.Msg)
      {
        //WM_NOTIFY
        case 78:
          NMSHN nm = (NMSHN)Marshal.PtrToStructure(m.LParam, typeof(NMSHN));
          NotificationWithSoftKeys n = (NotificationWithSoftKeys)m_notifications[nm.idFrom];

          switch (nm.code)
          {
            case SHNN.DISMISS:
              n.OnBalloonChanged(new BalloonChangedEventArgs(false));
              break;
            case SHNN.SHOW:
              n.OnBalloonChanged(new BalloonChangedEventArgs(true));
              break;
            case SHNN.LINKSEL:
              string link = Marshal.PtrToStringUni((IntPtr)nm.union1);
              n.OnResponseSubmitted(new ResponseSubmittedEventArgs(link));
              break;
            case SHNN.NAVNEXT:
              n.OnSpinnerClick(new SpinnerClickEventArgs(true));
              break;
            case SHNN.NAVPREV:
              n.OnSpinnerClick(new SpinnerClickEventArgs(false));
              break;
          }
          break;

        //WM_COMMAND
        case 0x0111:
          {
            uint value = ((uint)m.WParam & 0xFFFF);
            int id =  (int)(value >> 8);
            byte index = (byte)(value & 0xFF);

            NotificationWithSoftKeys z = (NotificationWithSoftKeys)m_notifications[id];
            if (index == 0)
              z.OnLeftSoftKeyClick(EventArgs.Empty);
            else
              z.OnRightSoftKeyClick(EventArgs.Empty);
          }
          break;
      }

      //do base wndproc
      base.WndProc(ref m);
    }

  }
#endif

  internal enum SHNN : int
  {
    LINKSEL = -1000,
    DISMISS = -1001,
    SHOW = -1002, 
    NAVPREV = -1003,  // Toast stack left spinner clicked / DPAD LEFT
    NAVNEXT = -1004,  // Toast stack right spinner clicked / DPAD RIGHT
    /*    
    #define SHNN_ACTIVATE           (SHNN_FIRST-5)  // Toast DPAD Action
    #define SHNN_ICONCLICKED        (SHNN_FIRST-6)  // nmshn.pt contains the point where the user clicked
    #define SHNN_HOTKEY             (SHNN_FIRST-7)  
      // A hotkey has been pressed - modifiers are in the loword of the nmshn.lParam, 
      // the virtual key code is in the hiword.
      // If the sink window returns 0 in response to this notification, then
      // the notification toast will be hidden and VK_TTALK key default behavior
      // will be performed.
    */
  }
}
