using System;
using System.Collections.Generic;
using System.Text;

namespace LogExpert
{
  internal class ConfigChangedEventArgs : EventArgs
  {
    private SettingsFlags flags;

    internal ConfigChangedEventArgs(SettingsFlags changeFlags)
    {
      this.flags = changeFlags;
    }


    public SettingsFlags Flags
    {
      get { return this.flags; }
    }
  }
}
