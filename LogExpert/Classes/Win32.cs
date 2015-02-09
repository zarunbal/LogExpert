using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Runtime.InteropServices;

namespace LogExpert
{
	class Win32
	{
		public const long SM_CYVSCROLL = 20;
		public const long SM_CXHSCROLL = 21;
		public const long SM_CXVSCROLL = 2;
		public const long SM_CYHSCROLL = 3;

		/*
		UINT ExtractIconEx(          
		LPCTSTR lpszFile,
		int nIconIndex,
		HICON *phiconLarge,
		HICON *phiconSmall,
		UINT nIcons
		);    
		* */

		[DllImport("shell32.dll")]
		static extern UInt32 ExtractIconEx(String fileName,
			int iconIndex,
			ref IntPtr iconsLarge,
			ref IntPtr iconsSmall,
			UInt32 numIcons);

		[DllImport("user32.dll", CharSet = CharSet.Auto)]
		public static extern bool DestroyIcon(IntPtr hIcon);

		[DllImport("User32.dll")]
		public static extern Int32 SetForegroundWindow(IntPtr hWnd);

		public static Icon LoadIconFromExe(String fileName, int index)
		{
			//IntPtr[] smallIcons = new IntPtr[1];
			//IntPtr[] largeIcons = new IntPtr[1];
			IntPtr smallIcons = new IntPtr();
			IntPtr largeIcons = new IntPtr();
			int num = (int)ExtractIconEx(fileName, index, ref largeIcons, ref smallIcons, 1);
			if (num > 0 && smallIcons.ToInt32() != 0)
			{
				Icon icon = Icon.FromHandle(smallIcons).Clone() as Icon;
				DestroyIcon(smallIcons);
				return icon;
			}
			if (num > 0 && largeIcons.ToInt32() != 0)
			{
				Icon icon = Icon.FromHandle(largeIcons).Clone() as Icon;
				DestroyIcon(largeIcons);
				return icon;
			}
			return null;
		}

		public static Icon[,] ExtractIcons(String fileName)
		{
			IntPtr smallIcon = IntPtr.Zero;
			IntPtr largeIcon = IntPtr.Zero;
			int iconCount = (int)ExtractIconEx(fileName, -1, ref largeIcon, ref smallIcon, 0);
			if (iconCount <= 0)
				return null;

			IntPtr smallIcons = new IntPtr();
			IntPtr largeIcons = new IntPtr();
			Icon[,] result = new Icon[2, iconCount];

			for (int i = 0; i < iconCount; ++i)
			{
				int num = (int)ExtractIconEx(fileName, i, ref largeIcons, ref smallIcons, 1);
				if (smallIcons.ToInt32() != 0)
				{
					result[0, i] = Icon.FromHandle(smallIcons).Clone() as Icon;
					DestroyIcon(smallIcons);
				}
				else
				{
					result[0, i] = null;
				}
				if (num > 0 && largeIcons.ToInt32() != 0)
				{
					result[1, i] = Icon.FromHandle(largeIcons).Clone() as Icon;
					DestroyIcon(largeIcons);
				}
				else
				{
					result[1, i] = null;
				}
			}
			return result;
		}

		[DllImport("user32.dll")]
		public static extern long GetSystemMetrics(long index);

		[DllImport("user32.dll")]
		public static extern short GetKeyState(int vKey);
	}
}