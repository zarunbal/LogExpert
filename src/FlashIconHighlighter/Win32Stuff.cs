using System;
using System.Runtime.InteropServices;

namespace FlashIconHighlighter
{
/*
     * Flash stuff stolen from http://blogs.x2line.com/al/archive/2008/04/19/3392.aspx
     */
    [StructLayout(LayoutKind.Sequential)]
    public struct FLASHWINFO
    {
        #region Private Fields

        public uint cbSize;
        public int dwFlags;
        public int dwTimeout;
        public IntPtr hwnd;
        public uint uCount;

        #endregion
    }


    public class Win32Stuff
    {
        #region Externals

        [DllImport("user32.dll")]
        public static extern int FlashWindowEx(ref FLASHWINFO pwfi);

        #endregion
    }
}
