using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace FlashIconHighlighter
{
    /*
     * Flash stuff stolen from http://blogs.x2line.com/al/archive/2008/04/19/3392.aspx
     */

    [StructLayout(LayoutKind.Sequential)]
    public struct FLASHWINFO
    {
        public uint cbSize;
        public IntPtr hwnd;
        public int dwFlags;
        public uint uCount;
        public int dwTimeout;
    }


    public class Win32Stuff
    {
        #region Public methods

        [DllImport("user32.dll")]
        public static extern int FlashWindowEx(ref FLASHWINFO pwfi);

        #endregion
    }
}