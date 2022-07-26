using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace LogExpert.Config
{
    //LogExpert.Config.ColorMode.backgroundColor  System.Drawing.SystemColors.ControlDarkDark
    //LogExpert.Config.ColorMode.foreColor LogExpert.Config.ColorMode.foreColor

    public static class ColorMode
    {
        public static System.Drawing.Color BackgroundColor = System.Drawing.SystemColors.Control;
        public static System.Drawing.Color ForeColor = System.Drawing.SystemColors.ControlText;

        private static System.Drawing.Color WhiteBackgroundColor = System.Drawing.SystemColors.Control;
        private static System.Drawing.Color WhiteForeColor = System.Drawing.SystemColors.ControlText;

        private static System.Drawing.Color DarkBackgroundColor = System.Drawing.SystemColors.ControlDarkDark;
        private static System.Drawing.Color DarkForeColor = System.Drawing.SystemColors.ControlLightLight;

        public static bool DarkModeEnabled = false;

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
            BackgroundColor = DarkBackgroundColor;
            ForeColor = DarkForeColor;
            DarkModeEnabled = true;
        }

        private static void SetDefaultMode()
        {
            BackgroundColor = WhiteBackgroundColor;
            ForeColor = WhiteForeColor;
            DarkModeEnabled = false;
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
