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
    public UInt32 cbSize;
    public IntPtr hwnd;
    public Int32 dwFlags;
    public UInt32 uCount;
    public Int32 dwTimeout;
  }



  public class Win32Stuff
  {
    [DllImport("user32.dll")]
    public static extern Int32 FlashWindowEx(ref FLASHWINFO pwfi);
  }
}
