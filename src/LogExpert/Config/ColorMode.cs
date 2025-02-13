using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace LogExpert.Config
{
    public static class ColorMode
    {
        // Bright Theme
        // https://paletton.com/#uid=15-0u0k00sH00kJ0pq+00RL00RL
        private static readonly Color BrightBookmarkDefaultSystemColor = SystemColors.Control; // Important: only supports SystemColors
        private static readonly Color LessBrightBackgroundColor = Color.FromArgb(208, 205, 206);
        private static readonly Color BrightBackgroundColor = Color.FromArgb(221, 221, 221);
        private static readonly Color BrighterBackgroundColor = Color.FromArgb(253, 253, 253);
        private static readonly Color BrightForeColor = Color.FromArgb(0, 0, 0);

        // Dark Theme
        // https://paletton.com/#uid=15-0u0k005U0670008J003Y003Y
        private static readonly Color DarkBookmarkDefaultSystemColor = SystemColors.ControlDarkDark; // Important: only supports SystemColors
        private static readonly Color LessLessDarkBackgroundColor = Color.FromArgb(90, 90, 90);
        private static readonly Color LessDarkBackgroundColor = Color.FromArgb(67, 67, 67);
        private static readonly Color DarkBackgroundColor = Color.FromArgb(45, 45, 45);
        private static readonly Color DarkerBackgroundColor = Color.FromArgb(30, 30, 30);
        private static readonly Color DarkForeColor = Color.FromArgb(255, 255, 255);

        // Default
        public static Color BackgroundColor = BrightBackgroundColor;
        public static Color DockBackgroundColor = BrighterBackgroundColor;
        public static Color BookmarksDefaultBackgroundColor = BrightBookmarkDefaultSystemColor;
        public static Color ForeColor = BrightForeColor;
        public static Color MenuBackgroundColor = BrighterBackgroundColor;
        public static Color HoverMenuBackgroundColor = LessBrightBackgroundColor;
        public static Color ActiveTabColor = BrighterBackgroundColor;
        public static Color InactiveTabColor = LessBrightBackgroundColor;
        public static Color TabsBackgroundStripColor = LessBrightBackgroundColor;


        public static bool DarkModeEnabled;

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
            HoverMenuBackgroundColor = LessDarkBackgroundColor;
            BookmarksDefaultBackgroundColor = DarkBookmarkDefaultSystemColor;
            TabsBackgroundStripColor = LessDarkBackgroundColor;
            ActiveTabColor = LessLessDarkBackgroundColor;
            InactiveTabColor = LessDarkBackgroundColor;
            DarkModeEnabled = true;
        }

        private static void SetBrightMode()
        {
            BackgroundColor = BrightBackgroundColor;
            ForeColor = BrightForeColor;
            MenuBackgroundColor = BrighterBackgroundColor;
            DockBackgroundColor = BrighterBackgroundColor;
            BookmarksDefaultBackgroundColor = BrightBookmarkDefaultSystemColor;
            HoverMenuBackgroundColor = LessBrightBackgroundColor;
            TabsBackgroundStripColor = BrighterBackgroundColor;
            ActiveTabColor = BrighterBackgroundColor;
            InactiveTabColor = LessBrightBackgroundColor;
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
