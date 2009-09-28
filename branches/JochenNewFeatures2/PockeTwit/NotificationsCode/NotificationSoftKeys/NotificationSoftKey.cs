using System;
using System.Collections.Generic;
using System.Text;

namespace christec.windowsce.forms
{
  // NOTIF_SOFTKEY_FLAGS_xxx
  public enum SoftKeyType : uint
  {
    /// <summary>
    /// Remove the notification when the softkey is pressed
    /// </summary>
    Dismiss  = 0x0000,
    /// <summary>
    ///  Hide the notification when the softkey is pressed (but do not dismiss)
    /// </summary>
    Hide     = 0x0001,
    /// <summary>
    /// Do not dismiss or hide the notification when the softkey is pressed.
    /// </summary>
    StayOpen = 0x0002, 
    /// <summary>
    /// Submit the HTML form in the associated notification instead of sending WM_COMMAND to the sink
    /// </summary>
    Submit   = 0x0004,
    /// <summary>
    /// This softkey is disabled
    /// </summary>
    Disabled = 0x0008
  }

  public class NotificationSoftKey
  {
    private string title;
    private SoftKeyType type;

    // This overload is mainly here to allow a user to
    // use SoftKeyType.Disabled without needing to specify
    // a title. It serves little use otherwise.
    public NotificationSoftKey(SoftKeyType type)
      : this(type, " ")
    {
    }

    public NotificationSoftKey(SoftKeyType type, String title)
    {
      this.title = title;
      this.type = type;
    }

    public string Title
    {
      get { return title; }
    }

    public SoftKeyType Type
    {
      get { return type; }
    }
  }
}
