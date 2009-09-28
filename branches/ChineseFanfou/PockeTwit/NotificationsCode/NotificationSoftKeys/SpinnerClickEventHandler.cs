using System;
using System.Collections.Generic;
using System.Text;

namespace christec.windowsce.forms
{
  public class SpinnerClickEventArgs : EventArgs
  {
    private bool forward;

    public SpinnerClickEventArgs(bool forward)
    {
      this.forward = forward;
    }

    // Returns true if the > spinner button is clicked
    // Returns false if the < spinner button is clicked
    public bool Forward { get { return forward; } }
  }

  public delegate void SpinnerClickEventHandler(object sender, SpinnerClickEventArgs e);
}
