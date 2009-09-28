using System;

using System.Collections.Generic;
using System.Text;

namespace PockeTwit.Blog
{
    #region blog services delegates

    /// <summary>
    /// Delegate for when the post is ready
    /// </summary>
    /// <param name="sender">sender object</param>
    /// <param name="eventArgs">resulting arguments</param>
    public delegate void PostFinishEventHandler(object sender, BlogServiceEventArgs eventArgs);

    /// <summary>
    /// Delegate for when an error occurs during posting
    /// </summary>
    /// <param name="sender">sender object</param>
    /// <param name="eventArgs">resulting arguments</param>
    public delegate void ErrorOccuredEventHandler(object sender, BlogServiceEventArgs eventArgs);

    /// <summary>
    /// Delegate for sending a message to calling class
    /// </summary>
    /// <param name="sender">sender object</param>
    /// <param name="eventArgs">resulting arguments</param>
    public delegate void MessageReadyEventHandler(object sender, BlogServiceEventArgs eventArgs);

    #endregion
}
