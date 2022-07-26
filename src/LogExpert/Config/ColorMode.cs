using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace LogExpert.Config
{
    public static class ColorMode
    {
        // Bright Theme
        // https://paletton.com/#uid=15-0u0k00sH00kJ0pq+00RL00RL
        private static System.Drawing.Color LessBrightBackgroundColor = System.Drawing.Color.FromArgb(208, 205, 206);
        private static System.Drawing.Color BrightBackgroundColor = System.Drawing.Color.FromArgb(221, 221, 221);
        private static System.Drawing.Color BrighterBackgroundColor = System.Drawing.Color.FromArgb(253, 253, 253);
        private static System.Drawing.Color BrightForeColor = System.Drawing.Color.FromArgb(0,0,0);        

        // Dark Theme
        // https://paletton.com/#uid=15-0u0k005U0670008J003Y003Y
        private static System.Drawing.Color LessDarkBackgroundColor = System.Drawing.Color.FromArgb(67, 67, 67);
        private static System.Drawing.Color DarkBackgroundColor =  System.Drawing.Color.FromArgb(45, 45, 45);
        private static System.Drawing.Color DarkerBackgroundColor = System.Drawing.Color.FromArgb(30, 30, 30);
        private static System.Drawing.Color DarkForeColor = System.Drawing.Color.FromArgb(255,255,255);

        // Default
        public static System.Drawing.Color BackgroundColor = LessBrightBackgroundColor;
        public static System.Drawing.Color DockBackgroundColor = BrighterBackgroundColor;
        public static System.Drawing.Color ForeColor = BrightForeColor;
        public static System.Drawing.Color MenuBackgroundColor = BrighterBackgroundColor;


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
                SetBrightMode();
            }
        }

        private static void SetDarkMode()
        {
            BackgroundColor = DarkBackgroundColor;
            ForeColor = DarkForeColor;
            MenuBackgroundColor = DarkerBackgroundColor;
            DockBackgroundColor = LessDarkBackgroundColor;
            DarkModeEnabled = true;
        }

        private static void SetBrightMode()
        {
            BackgroundColor = LessBrightBackgroundColor;
            ForeColor = BrightForeColor;
            MenuBackgroundColor = BrighterBackgroundColor;
            DockBackgroundColor = BrighterBackgroundColor;
            DarkModeEnabled = false;
        }

        #region TitleBarDarkMode
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

        #endregion TitleBarDarkMode

    }
}
