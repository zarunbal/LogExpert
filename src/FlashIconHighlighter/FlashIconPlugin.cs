using System;
using System.Collections.Generic;
using System.Text;
using LogExpert;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace FlashIconHighlighter
{
  class FlashIconPlugin : IKeywordAction
  {
    private delegate void FlashWindowFx(Form form);

    #region IKeywordAction Member

    public void Execute(string keyword, string param, ILogExpertCallback callback, ILogLineColumnizer columnizer)
    {
      FormCollection openForms = Application.OpenForms;
      foreach (Form form in openForms)
      {
        if (form.TopLevel && form.Name.Equals("LogTabWindow") && form.Text.Contains(callback.GetFileName()))
        {
          FlashWindowFx fx = FlashWindow;
          form.BeginInvoke(fx, new object[] {form});
        }
      }
    }

    private void FlashWindow(Form form)
    {
      FLASHWINFO fw = new FLASHWINFO();

      fw.cbSize = Convert.ToUInt32(Marshal.SizeOf(typeof(FLASHWINFO)));
      fw.hwnd = form.Handle;
      fw.dwFlags = 14;
      fw.uCount = 0;

      Win32Stuff.FlashWindowEx(ref fw);
    }

    public string GetDescription()
    {
      return "Let the taskbar icon flash ";
    }

    public string GetName()
    {
      return "Flash Icon";
    }

    #endregion

    public string Text
    {
      get { return GetName(); }
    }
  }
}
