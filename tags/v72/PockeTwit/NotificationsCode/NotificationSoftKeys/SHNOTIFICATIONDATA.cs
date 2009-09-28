using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace christec.windowsce.forms
{
  [StructLayout(LayoutKind.Sequential)]
	internal struct SHNOTIFICATIONDATA
	{
		// for verification and versioning
		public int     cbStruct;
		// identifier for this particular notification
		public int      dwID;
		// priority
		public SHNP		npPriority;
		/// <summary>
		/// Duration to display the bubble (in seconds).
		/// </summary>
		public int		csDuration;
		// the icon for the notification
		public IntPtr	hicon;
		/// <summary>
		/// Flags that affect the behaviour of the Notification bubble
		/// </summary>
		public SHNF	grfFlags;
		// unique identifier for the notification class
		public Guid		clsid;
		// window to receive command choices, dismiss, etc.
		public IntPtr	hwndSink;
		// HTML content for the bubble
    [MarshalAs(UnmanagedType.LPTStr)]
    public string pszHTML;
		// Optional title for bubble
		[MarshalAs(UnmanagedType.LPTStr)]
    public string pszTitle;
		/// <summary>
		/// User defined parameter
		/// </summary>
		public int		lParam;

    // Structure fields below this point were added in WM5.0
    public SOFTKEYNOTIFY leftSoftKey;
    public SOFTKEYNOTIFY rightSoftKey;
    [MarshalAs(UnmanagedType.LPTStr)]
    public string pszTodaySK;
    [MarshalAs(UnmanagedType.LPTStr)]
    public string pszTodayExec;
	}

  [StructLayout(LayoutKind.Sequential)]
  struct SOFTKEYCMD
  {
    public UInt32 wpCmd;
    public UInt32 grfFlags;

  }

  [StructLayout(LayoutKind.Sequential)]
  struct SOFTKEYNOTIFY
  {
    [MarshalAs(UnmanagedType.LPTStr)]
    public String pszTitle;
    public SOFTKEYCMD skc;
  }

	internal enum SHNP :int
	{
		/// <summary>
		/// Bubble shown for duration, then goes away
		/// </summary>
		INFORM = 0x1B1,
		/// <summary>
		/// No bubble, icon shown for duration then goes away
		/// </summary>
		ICONIC = 0,
	}

  // notification update mask
  internal enum SHNUM : int
  {
    PRIORITY    = 0x0001,
    DURATION    = 0x0002,
    ICON        = 0x0004,
    HTML        = 0x0008,
    TITLE       = 0x0010,
    SOFTKEYS    = 0x0020,
    TODAYKEY    = 0x0040,
    TODAYEXEC   = 0x0080,
    SOFTKEYCMDS = 0x0100,
    FLAGS       = 0x0200
  }

	/// <summary>
	/// Flags that affect the display behaviour of the Notification
	/// </summary>
	internal enum SHNF : int
	{
		/// <summary>
		/// For SHNP_INFORM priority and above, don't display the notification bubble when it's initially added;
		/// the icon will display for the duration then it will go straight into the tray.
		/// The user can view the icon / see the bubble by opening the tray.
		/// </summary>
		STRAIGHTTOTRAY  = 0x00000001,
		/// <summary>
		/// Critical information - highlights the border and title of the bubble.
		/// </summary>
		CRITICAL        = 0x00000002,
		/// <summary>
		/// Force the message (bubble) to display even if settings says not to.
		/// </summary>
		FORCEMESSAGE    = 0x00000008,
		/// <summary>
		/// Force the display to turn on for notification. Added for Windows Mobile 2003.
		/// </summary>
		DISPLAYON       = 0x00000010,
		/// <summary>
		/// Force the notification to be silent and not vibrate, regardless of Settings. Added for Windows Mobile 2003.
		/// </summary>
		SILENT          = 0x00000020,
    
    // Draw the current time with the title
    TITLETIME   =    0x00000080,

    // A notification with "stack" support
    SPINNERS   =     0x00000100,

    // RE-play physical alerts on an update
    SHNF_ALERTONUPDATE  = 0x00000200
	}

	internal struct NMSHN
	{
		public IntPtr hwndFrom; 
		public int idFrom; 
		public SHNN code; 
		public int lParam;
		public int dwReturn;
		public int union1;
		public int union2;
  }
}
