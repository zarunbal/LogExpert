using System.Runtime.InteropServices;
using System.Runtime.Versioning;

using ColumnizerLib;

using static Vanara.PInvoke.User32;

[assembly: SupportedOSPlatform("windows")]
namespace FlashIconHighlighter;

internal class FlashIconPlugin : IKeywordAction
{
    #region Properties

    public string Text => GetName();

    #endregion

    #region IKeywordAction Member

    public void Execute (string keyword, string param, ILogExpertCallbackMemory callback, ILogLineMemoryColumnizer columnizer)
    {
        var openForms = Application.OpenForms;
        foreach (Form form in openForms)
        {
            if (form.TopLevel && form.Name.Equals("LogTabWindow", StringComparison.OrdinalIgnoreCase) && form.Text.Contains(callback.GetFileName(), StringComparison.Ordinal))
            {
                _ = form.BeginInvoke(FlashWindow, [form]);
            }
        }
    }

    /// <summary>
    /// Flash Window http://blogs.x2line.com/al/archive/2008/04/19/3392.aspx
    /// </summary>
    /// <param name="form"></param>
    private void FlashWindow (Form form)
    {
        FLASHWINFO fw = new()
        {
            cbSize = Convert.ToUInt32(Marshal.SizeOf<FLASHWINFO>()),
            hwnd = form.Handle,
            dwFlags = FLASHW.FLASHW_TRAY | FLASHW.FLASHW_CAPTION | FLASHW.FLASHW_TIMER,
            uCount = 0
        };

        _ = FlashWindowEx(fw);
    }

    public string GetDescription ()
    {
        return "Let the taskbar icon flash ";
    }

    public string GetName ()
    {
        return "Flash Icon";
    }

    #endregion
}