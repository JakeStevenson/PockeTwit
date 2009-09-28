using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using Microsoft.WindowsCE.Forms;
using System.ComponentModel;
using System.Drawing;
using System.Text;

namespace christec.windowsce.forms
{
  /// <summary>
  /// Implements Windows CE functionality for displaying and responding to user notifications.
  /// <para><b>New in v1.3</b></para>
  /// </summary>
  /// <remarks>This class provides a managed implementation of the Windows CE notification functions.
  /// This class is supported only on the Pocket PC.
  /// <para>You can create notifications and then display them as needed using the <see cref="Visible"/> property.
  /// The <see cref="InitialDuration"/> property sets the time the message balloon initially displays.
  /// If you set <see cref="InitialDuration"/> to zero and Visible to true, the message balloon does not appear but its icon is available in the title bar for reactivation when clicked.
  /// The <see cref="BalloonChanged"/> event occurs whenever the balloon is shown or hidden, either programmatically using the <see cref="Visible"/> property or through user interaction.
  /// In addition to plain text, you can create a user notification with HTML content in the message balloon.
  /// The HTML is rendered by the Pocket PC HTML control, and you can respond to values in an HTML form by parsing a response string provided by the <see cref="ResponseSubmittedEventArgs"/> class, through the <see cref="ResponseSubmittedEventArgs.Response"/> property.</para>
  /// <b>Cmd:2 Identifier</b>
  /// <para>The identifier "cmd:2" has a special purpose in Windows CE and is used to dismiss notifications.
  /// If cmd:2 is the name of an HTML button or other element in a message balloon, the <see cref="ResponseSubmitted"/> event is not raised.
  /// The notification is dismissed, but its icon is placed on the title bar to be responded to at a later time.</para></remarks>
#if DESIGN
	[ToolboxItemFilter("NETCF",ToolboxItemFilterType.Require),
	ToolboxItemFilter("System.CF.Windows.Forms", ToolboxItemFilterType.Custom)]
#endif
  public class NotificationWithSoftKeys : Component
  {

#if !DESIGN
    private static NotificationMessageWindow msgwnd;
    private static Dictionary<int, NotificationWithSoftKeys> notifications;
    private static int id = 1;
    private static Guid clsid = Guid.NewGuid();
    
    private SHNOTIFICATIONDATA m_data;
    private Icon mIcon = null;
    private bool mVisible = false;
    private bool mCreated = false;
#endif

    private NotificationSoftKey mLeftSoftKey = null;
    private NotificationSoftKey mRightSoftKey = null;

    static NotificationWithSoftKeys()
    {
#if !DESIGN
      notifications = new Dictionary<int, NotificationWithSoftKeys>();

      msgwnd = new NotificationMessageWindow(notifications);
#endif
    }


#if !DESIGN
    /// <summary>
    /// Initializes a new instance of the <see cref="Notification"/> class.
    /// </summary>
    /// <remarks>This class is not supported on the Smartphone or other Windows CE devices that are not Pocket PCs.
    /// You can create multiple notifications, such as an array of notifications, and display them as needed with the Visible property.</remarks>
    public NotificationWithSoftKeys()
    {
      m_data = new SHNOTIFICATIONDATA();
    
      if (PlatformSupportsCustomSoftKeyButtons)
        m_data.cbStruct = Marshal.SizeOf(m_data);
      else
        m_data.cbStruct = Marshal.SizeOf(m_data) - 32; // "hide" the 20 bytes that were added to this struct in WM5.0    
  
      m_data.clsid = clsid;
      m_data.dwID = id;
      m_data.hwndSink = msgwnd.Hwnd;
      m_data.csDuration = 10;
      notifications.Add(id, this);
     
      id++;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="disposing"></param>
    protected override void Dispose(bool disposing)
    {
      // remove from notification collection
      Visible = false;
      notifications.Remove(m_data.dwID);
      
      base.Dispose(disposing);
    }

    // Fill out the SOFTKEYNOTIFY structure to create the notification soft key type specified
    // within 'data'.
    private void ConfigureSoftKey(ref SOFTKEYNOTIFY softKey, NotificationSoftKey data, uint index)
    {
      if (data != null)
      {
        softKey.pszTitle = String.IsNullOrEmpty(data.Title) ? " " : data.Title;
        softKey.skc.grfFlags = (uint)data.Type;
      }
      else
      {
        // NULL means an "empty" soft key, the best way
        // to simulate this is to make a soft key which has
        // a blank title (does not render any text) and
        // is disabled (so it doesn't respond to stylus
        // taps).
        softKey.pszTitle = " ";
        softKey.skc.grfFlags = (uint)SoftKeyType.Disabled;
      }

      // Each softkey must have a unique id for use with WM_COMMAND
      // so fudge the notification id and softkey index into a unique
      // id which we can easily reverse back into it's components.
      softKey.skc.wpCmd = (uint)(m_data.dwID << 8) + index;
    }

    private void RebuildSoftKeys()
    {
      if (PlatformSupportsCustomSoftKeyButtons)
      {
        // Configure the soft key part of the structure
        ConfigureSoftKey(ref m_data.leftSoftKey, mLeftSoftKey, 0);
        ConfigureSoftKey(ref m_data.rightSoftKey, mRightSoftKey, 1);
      }
    }

    // Custom SoftKey menus are supported on Pocket PC devices running
    // Windows Mobile 5.0 or higher.
    public bool PlatformSupportsCustomSoftKeyButtons
    {
      get
      {
        bool isPocketPC = (GetSystemParameter(SPI_GETPLATFORMTYPE) == "PocketPC");

        return isPocketPC && Environment.OSVersion.Version.Major >= 5;
      }
    }

#endif
    /// <summary>
    /// Gets or sets a string specifying the title text for the message balloon.
    /// </summary>
    /// <value>A string that specifies the caption text.
    /// The default value is an empty string ("").</value>
    /// <remarks>The background color of the caption is dependent on the current theme of the user.
    /// Use the <see cref="Text"/> property to modify the text in the body of the message balloon.</remarks>
#if DESIGN
		[DefaultValue(""),
		Description("Gets or sets a string specifying the title text for the message balloon.")]
#endif
    public string Caption
    {
      get
      {
        return m_data.pszTitle;
      }
      set
      {
        m_data.pszTitle = value;

        if (mCreated)
        {
          SHNotificationUpdate(SHNUM.TITLE, ref m_data);
        }
      }
    }

    /// <summary>
    /// Gets or sets a value indicating whether the notification is of urgent importance.
    /// </summary>
    /// <value>true if the notification is critical; otherwise, false. The default is false.</value>
    /// <remarks>Critical notifications have a red background caption color, or other color, depending on the current Pocket PC theme.</remarks>
#if DESIGN
		[DefaultValue(false),
		Description("Gets or sets a value indicating whether the notification is of urgent importance.")]
#endif
    public bool Critical
    {
      get
      {
        return (m_data.grfFlags & SHNF.CRITICAL) != 0;
      }
      set
      {
        if (value)
        {
          m_data.grfFlags |= SHNF.CRITICAL;
        }
        else
        {
          m_data.grfFlags ^= (m_data.grfFlags & SHNF.CRITICAL);
        }

        if (mCreated)
        {
          SHNotificationUpdate(SHNUM.FLAGS, ref m_data);
        }
      }
    }

    /// <summary>
    /// Gets or sets the current native icon handle for the message balloon on the title bar.
    /// </summary>
#if !DESIGN

    public Icon Icon
    {
      get
      {
        return mIcon;
      }
      set
      {
        mIcon = value;
        m_data.hicon = mIcon.Handle;

        if (mCreated)
        {
          SHNotificationUpdate(SHNUM.ICON, ref m_data);
        }
      }
    }
#endif

    /// <summary>
    /// Gets or sets the number of seconds the message balloon remains visible after initially shown.
    /// </summary>
    /// <remarks>The amount of time the balloon is visible, in seconds. The default is 10.</remarks>
#if DESIGN
		[DefaultValue(10),
		Description("Gets or sets the number of seconds the message balloon remains visible after initially shown.")]
#endif
    public int InitialDuration
    {
      get
      {
        return m_data.csDuration;
      }
      set
      {
        if (value < 0)
        {
          throw new ArgumentException();
        }

        m_data.csDuration = value;

        if (m_data.csDuration == 0)
        {
          m_data.npPriority = SHNP.ICONIC;
        }
        else
        {
          m_data.npPriority = SHNP.INFORM;
        }

        if (mCreated)
        {
          SHNotificationUpdate(SHNUM.DURATION | SHNUM.PRIORITY, ref m_data);
        }
      }
    }

    /// <summary>
    /// Gets or sets the text for the message balloon.
    /// </summary>
#if DESIGN
		[DefaultValue(""),
		Description("Gets or sets the text for the message balloon.")]
#endif
    public string Text
    {
      get
      {
        return m_data.pszHTML;
      }
      set
      {
        m_data.pszHTML = value;

        if (mCreated)
        {
          SHNotificationUpdate(SHNUM.HTML, ref m_data);
        }
      }
    }

    public NotificationSoftKey LeftSoftKey
    {
      get { return mLeftSoftKey;
      }
      set {
        mLeftSoftKey = value;
        RebuildSoftKeys();

        if (mCreated)
        {
          SHNotificationUpdate(SHNUM.SOFTKEYS, ref m_data);
        }
      }
    }

    public NotificationSoftKey RightSoftKey
    {
      get { return mRightSoftKey;
      }
      set
      {
        mRightSoftKey = value;
        RebuildSoftKeys();

        if (mCreated)
        {
          SHNotificationUpdate(SHNUM.SOFTKEYS, ref m_data);
        }
      }
    }

    public bool Silent
    {
        get { return (m_data.grfFlags & SHNF.SILENT) != 0; }
        set
        {
            if (value)
            {
                m_data.grfFlags |= SHNF.SILENT;
            }
            else
            {
                m_data.grfFlags ^= (SHNF.SILENT & m_data.grfFlags);
            }

            if (mCreated)
            {
                SHNotificationUpdate(SHNUM.FLAGS, ref m_data);
            }
        }
    }
    /// <summary>
    /// Gets or sets a value indicating whether the current time is displayed with the notification title. 
    /// </summary>
    public bool TitleTime
    {
      get { return (m_data.grfFlags & SHNF.TITLETIME) != 0; }
      set
      {
        if (value)
        {
          m_data.grfFlags |= SHNF.TITLETIME;
        }
        else
        {
          m_data.grfFlags ^= (SHNF.TITLETIME & m_data.grfFlags);
        }

        if (mCreated)
        {
          SHNotificationUpdate(SHNUM.FLAGS, ref m_data);
        }
      }
    }

    /// <summary>
    /// ???
    /// </summary>
    public bool Spinners
    {
      get { return (m_data.grfFlags & SHNF.SPINNERS) != 0; }
      set
      {
        if (value)
        {
          m_data.grfFlags |= SHNF.SPINNERS;
        }
        else
        {
          m_data.grfFlags ^= (SHNF.SPINNERS & m_data.grfFlags);
        }

        if (mCreated)
        {
          SHNotificationUpdate(SHNUM.FLAGS, ref m_data);
        }
      }
    }

    /// <summary>
    /// Gets or sets a value indicating whether the message balloon is visible.
    /// </summary>
#if DESIGN
		[DefaultValue(false),
		Description("Gets or sets a value indicating whether the message balloon is visible.")]
#endif
    public bool Visible
    {
      get
      {
        return mVisible;
      }
      set
      {
#if !DESIGN
        if (mVisible != value)
        {
          mVisible = value;

          if (mVisible)
          {
          
            int hresult = SHNotificationAdd(ref m_data);
            mCreated = true;
          }
          else
          {
            int hresult = SHNotificationRemove(ref clsid, m_data.dwID);
            mCreated = false;
          }
        }
#endif
      }
    }

    /// <summary>
    /// Occurs when a message balloon is displayed or hidden.
    /// </summary>
#if DESIGN
		[Description("Occurs when a message balloon is displayed or hidden.")]
#endif
    public event BalloonChangedEventHandler BalloonChanged;

    internal void OnBalloonChanged(BalloonChangedEventArgs e)
    {
      //update visible state
      mVisible = e.Visible;

      if (this.BalloonChanged != null)
      {
        this.BalloonChanged(this, e);
      }
    }

    /// <summary>
    /// Occurs when the user clicks a button or link in the message balloon.
    /// </summary>
#if DESIGN
		[Description("Occurs when the user clicks a button or link in the message balloon.")]
#endif
    public event ResponseSubmittedEventHandler ResponseSubmitted;

    internal void OnResponseSubmitted(ResponseSubmittedEventArgs e)
    {
      if (this.ResponseSubmitted != null)
      {
        this.ResponseSubmitted(this, e);
      }
    }

    /// <summary>
    /// Occurs when the user clicks a button or link in the message balloon.
    /// </summary>
#if DESIGN
		[Description("Occurs when the user clicks a button or link in the message balloon.")]
#endif
    public event SpinnerClickEventHandler SpinnerClick;

    internal void OnSpinnerClick(SpinnerClickEventArgs e)
    {
      if (this.SpinnerClick != null)
      {
        this.SpinnerClick(this, e);
      }
    }

    /// <summary>
    /// Occurs when the user clicks the left soft key if it is of type SoftKeyType.StayOpen.
    /// </summary>
#if DESIGN
		[Description("Occurs when the user clicks the left soft key if it is of type SoftKeyType.StayOpen")]
#endif
    public event EventHandler LeftSoftKeyClick;

    internal void OnLeftSoftKeyClick(EventArgs e)
    {
      if (this.LeftSoftKeyClick != null)
      {
        this.LeftSoftKeyClick(this, e);
      }
    }

    /// <summary>
    /// Occurs when the user clicks the right soft key if it is of type SoftKeyType.StayOpen.
    /// </summary>
#if DESIGN
		[Description("Occurs when the user clicks the right soft key if it is of type SoftKeyType.StayOpen")]
#endif
    public event EventHandler RightSoftKeyClick;

    internal void OnRightSoftKeyClick(EventArgs e)
    {
      if (this.RightSoftKeyClick != null)
      {
        this.RightSoftKeyClick(this, e);
      }
    }

    #region API Declares

    //Add
    [DllImport("aygshell.dll", EntryPoint = "#155", SetLastError = true)]
    private static extern int SHNotificationAdd(ref SHNOTIFICATIONDATA shinfo);

    //Remove
    [DllImport("aygshell.dll", EntryPoint = "#157", SetLastError = true)]
    private static extern int SHNotificationRemove(ref Guid clsid, int dwID);

    //Update
    [DllImport("aygshell.dll", EntryPoint = "#156", SetLastError = true)]
    private static extern int SHNotificationUpdate(SHNUM grnumUpdateMask, ref SHNOTIFICATIONDATA shinfo);

    //Get Data
    [DllImport("aygshell.dll", EntryPoint = "#173", SetLastError = true)]
    private static extern int SHNotificationGetData(ref Guid clsid, int dwID, ref SHNOTIFICATIONDATA shinfo);

    // GetSystemParameter
    private static readonly uint SPI_GETPLATFORMTYPE = 257;

    [DllImport("coredll.dll", SetLastError = true)]
    private static extern int SystemParametersInfo(uint uiAction, uint uiParam, StringBuilder pvParam, uint fWinini);

    private static string GetSystemParameter(uint uiParam)
    {
      StringBuilder sb = new StringBuilder(128);
      if (SystemParametersInfo(uiParam, (uint)sb.Capacity, sb, 0) == 0)
        throw new ApplicationException("Failed to get system parameter");
      return sb.ToString();
    }

    #endregion
  }


}