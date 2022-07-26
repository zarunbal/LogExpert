using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace LogExpert.Interface
{
    //LogExpert.Interface.ColorMode.backgroundColor  System.Drawing.SystemColors.ControlDarkDark
    //LogExpert.Interface.ColorMode.foreColor LogExpert.Interface.ColorMode.foreColor

    public static class ColorMode
    {
        public static System.Drawing.Color backgroundColor = System.Drawing.SystemColors.Control;
        public static System.Drawing.Color foreColor = System.Drawing.SystemColors.ControlDarkDark;

        private static System.Drawing.Color whiteBackgroundColor = System.Drawing.SystemColors.Control;
        private static System.Drawing.Color whiteForeColor = System.Drawing.SystemColors.ControlDarkDark;

        private static System.Drawing.Color darkBackgroundColor = System.Drawing.SystemColors.ControlDarkDark;
        private static System.Drawing.Color darkForeColor = System.Drawing.SystemColors.ControlLightLight;

        public static void LoadColorMode()
        {
            var preferences = Config.ConfigManager.Settings.preferences;

            if (preferences.darkMode)
            {
                SetDarkMode();
            }
            else
            {
                SetDefaultMode();
            }
        }

        private static void SetDarkMode()
        {
            backgroundColor = darkBackgroundColor;
            foreColor = darkForeColor;
        }

        private static void SetDefaultMode()
        {
            backgroundColor = whiteBackgroundColor;
            foreColor = whiteForeColor;
        }

        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

        private const int DWMWA_USE_IMMERSIVE_DARK_MODE_BEFORE_20H1 = 19;
        private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;

        public static bool UseImmersiveDarkMode(IntPtr handle, bool enabled)
        {

            var attribute = DWMWA_USE_IMMERSIVE_DARK_MODE_BEFORE_20H1;
            if (IsWindows10OrGreater(18985))
            {
                attribute = DWMWA_USE_IMMERSIVE_DARK_MODE;
            }

            int useImmersiveDarkMode = enabled ? 1 : 0;
            return DwmSetWindowAttribute(handle, (int)attribute, ref useImmersiveDarkMode, sizeof(int)) == 0;

        }

        private static bool IsWindows10OrGreater(int build = -1)
        {
            return Environment.OSVersion.Version.Major >= 10 && Environment.OSVersion.Version.Build >= build;
        }

    }
}
