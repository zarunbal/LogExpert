using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using LogExpert;

namespace FlashIconHighlighter
{
    internal class FlashIconPlugin : IKeywordAction
    {
        #region Delegates

        private delegate void FlashWindowFx(Form form);

        #endregion

        #region Interface IKeywordAction

        public void Execute(string keyword, string param, ILogExpertCallback callback, ILogLineColumnizer columnizer)
        {
            FormCollection openForms = Application.OpenForms;
            foreach (Form form in openForms)
            {
                if (form.TopLevel && form.Name.Equals("LogTabWindow") && form.Text.Contains(callback.GetFileName()))
                {
                    FlashWindowFx fx = FlashWindow;
                    form.BeginInvoke(fx, form);
                }
            }
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

        #region Properties / Indexers

        public string Text => GetName();

        #endregion

        #region Private Methods

        private void FlashWindow(Form form)
        {
            FLASHWINFO fw = new FLASHWINFO();

            fw.cbSize = Convert.ToUInt32(Marshal.SizeOf(typeof(FLASHWINFO)));
            fw.hwnd = form.Handle;
            fw.dwFlags = 14;
            fw.uCount = 0;

            Win32Stuff.FlashWindowEx(ref fw);
        }

        #endregion
    }
}
