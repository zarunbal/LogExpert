using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;

namespace WeifenLuo.WinFormsUI.Docking
{
    #region DockPanelSkin classes

    /// <summary>
    ///     The skin to use when displaying the DockPanel.
    ///     The skin allows custom gradient color schemes to be used when drawing the
    ///     DockStrips and Tabs.
    /// </summary>
    [TypeConverter(typeof(DockPanelSkinConverter))]
    public class DockPanelSkin
    {
        #region Ctor

        public DockPanelSkin()
        {
            AutoHideStripSkin = new AutoHideStripSkin();
            DockPaneStripSkin = new DockPaneStripSkin();
        }

        #endregion

        #region Properties / Indexers

        /// <summary>
        ///     The skin used to display the auto hide strips and tabs.
        /// </summary>
        public AutoHideStripSkin AutoHideStripSkin { get; set; }

        /// <summary>
        ///     The skin used to display the Document and ToolWindow style DockStrips and Tabs.
        /// </summary>
        public DockPaneStripSkin DockPaneStripSkin { get; set; }

        #endregion
    }

    /// <summary>
    ///     The skin used to display the auto hide strip and tabs.
    /// </summary>
    [TypeConverter(typeof(AutoHideStripConverter))]
    public class AutoHideStripSkin
    {
        #region Ctor

        public AutoHideStripSkin()
        {
            DockStripGradient = new DockPanelGradient();
            DockStripGradient.StartColor = SystemColors.ControlLight;
            DockStripGradient.EndColor = SystemColors.ControlLight;

            TabGradient = new TabGradient();
            TabGradient.TextColor = SystemColors.ControlDarkDark;

            TextFont = SystemFonts.MenuFont;
        }

        #endregion

        #region Properties / Indexers

        /// <summary>
        ///     The gradient color skin for the DockStrips.
        /// </summary>
        public DockPanelGradient DockStripGradient { get; set; }

        /// <summary>
        ///     The gradient color skin for the Tabs.
        /// </summary>
        public TabGradient TabGradient { get; set; }

        /// <summary>
        ///     Font used in AutoHideStrip elements.
        /// </summary>
        public Font TextFont { get; set; }

        #endregion
    }

    /// <summary>
    ///     The skin used to display the document and tool strips and tabs.
    /// </summary>
    [TypeConverter(typeof(DockPaneStripConverter))]
    public class DockPaneStripSkin
    {
        #region Ctor

        public DockPaneStripSkin()
        {
            DocumentGradient = new DockPaneStripGradient();
            DocumentGradient.DockStripGradient.StartColor = SystemColors.Control;
            DocumentGradient.DockStripGradient.EndColor = SystemColors.Control;
            DocumentGradient.ActiveTabGradient.StartColor = SystemColors.ControlLightLight;
            DocumentGradient.ActiveTabGradient.EndColor = SystemColors.ControlLightLight;
            DocumentGradient.InactiveTabGradient.StartColor = SystemColors.ControlLight;
            DocumentGradient.InactiveTabGradient.EndColor = SystemColors.ControlLight;

            ToolWindowGradient = new DockPaneStripToolWindowGradient();
            ToolWindowGradient.DockStripGradient.StartColor = SystemColors.ControlLight;
            ToolWindowGradient.DockStripGradient.EndColor = SystemColors.ControlLight;

            ToolWindowGradient.ActiveTabGradient.StartColor = SystemColors.Control;
            ToolWindowGradient.ActiveTabGradient.EndColor = SystemColors.Control;

            ToolWindowGradient.InactiveTabGradient.StartColor = Color.Transparent;
            ToolWindowGradient.InactiveTabGradient.EndColor = Color.Transparent;
            ToolWindowGradient.InactiveTabGradient.TextColor = SystemColors.ControlDarkDark;

            ToolWindowGradient.ActiveCaptionGradient.StartColor = SystemColors.GradientActiveCaption;
            ToolWindowGradient.ActiveCaptionGradient.EndColor = SystemColors.ActiveCaption;
            ToolWindowGradient.ActiveCaptionGradient.LinearGradientMode = LinearGradientMode.Vertical;
            ToolWindowGradient.ActiveCaptionGradient.TextColor = SystemColors.ActiveCaptionText;

            ToolWindowGradient.InactiveCaptionGradient.StartColor = SystemColors.GradientInactiveCaption;
            ToolWindowGradient.InactiveCaptionGradient.EndColor = SystemColors.InactiveCaption;
            ToolWindowGradient.InactiveCaptionGradient.LinearGradientMode = LinearGradientMode.Vertical;
            ToolWindowGradient.InactiveCaptionGradient.TextColor = SystemColors.InactiveCaptionText;

            TextFont = SystemFonts.MenuFont;
        }

        #endregion

        #region Properties / Indexers

        /// <summary>
        ///     The skin used to display the Document style DockPane strip and tab.
        /// </summary>
        public DockPaneStripGradient DocumentGradient { get; set; }

        /// <summary>
        ///     Font used in DockPaneStrip elements.
        /// </summary>
        public Font TextFont { get; set; }

        /// <summary>
        ///     The skin used to display the ToolWindow style DockPane strip and tab.
        /// </summary>
        public DockPaneStripToolWindowGradient ToolWindowGradient { get; set; }

        #endregion
    }

    /// <summary>
    ///     The skin used to display the DockPane ToolWindow strip and tab.
    /// </summary>
    [TypeConverter(typeof(DockPaneStripGradientConverter))]
    public class DockPaneStripToolWindowGradient : DockPaneStripGradient
    {
        #region Ctor

        public DockPaneStripToolWindowGradient()
        {
            ActiveCaptionGradient = new TabGradient();
            InactiveCaptionGradient = new TabGradient();
        }

        #endregion

        #region Properties / Indexers

        /// <summary>
        ///     The skin used to display the active ToolWindow caption.
        /// </summary>
        public TabGradient ActiveCaptionGradient { get; set; }

        /// <summary>
        ///     The skin used to display the inactive ToolWindow caption.
        /// </summary>
        public TabGradient InactiveCaptionGradient { get; set; }

        #endregion
    }

    /// <summary>
    ///     The skin used to display the DockPane strip and tab.
    /// </summary>
    [TypeConverter(typeof(DockPaneStripGradientConverter))]
    public class DockPaneStripGradient
    {
        #region Ctor

        public DockPaneStripGradient()
        {
            DockStripGradient = new DockPanelGradient();
            ActiveTabGradient = new TabGradient();
            InactiveTabGradient = new TabGradient();
        }

        #endregion

        #region Properties / Indexers

        /// <summary>
        ///     The skin used to display the active DockPane tabs.
        /// </summary>
        public TabGradient ActiveTabGradient { get; set; }

        /// <summary>
        ///     The gradient color skin for the DockStrip.
        /// </summary>
        public DockPanelGradient DockStripGradient { get; set; }

        /// <summary>
        ///     The skin used to display the inactive DockPane tabs.
        /// </summary>
        public TabGradient InactiveTabGradient { get; set; }

        #endregion
    }

    /// <summary>
    ///     The skin used to display the dock pane tab
    /// </summary>
    [TypeConverter(typeof(DockPaneTabGradientConverter))]
    public class TabGradient : DockPanelGradient
    {
        #region Ctor

        public TabGradient()
        {
            TextColor = SystemColors.ControlText;
        }

        #endregion

        #region Properties / Indexers

        /// <summary>
        ///     The text color.
        /// </summary>
        [DefaultValue(typeof(SystemColors), "ControlText")]
        public Color TextColor { get; set; }

        #endregion
    }

    /// <summary>
    ///     The gradient color skin.
    /// </summary>
    [TypeConverter(typeof(DockPanelGradientConverter))]
    public class DockPanelGradient
    {
        #region Ctor

        public DockPanelGradient()
        {
            StartColor = SystemColors.Control;
            EndColor = SystemColors.Control;
            LinearGradientMode = LinearGradientMode.Horizontal;
        }

        #endregion

        #region Properties / Indexers

        /// <summary>
        ///     The ending gradient color.
        /// </summary>
        [DefaultValue(typeof(SystemColors), "Control")]
        public Color EndColor { get; set; }

        /// <summary>
        ///     The gradient mode to display the colors.
        /// </summary>
        [DefaultValue(LinearGradientMode.Horizontal)]
        public LinearGradientMode LinearGradientMode { get; set; }

        /// <summary>
        ///     The beginning gradient color.
        /// </summary>
        [DefaultValue(typeof(SystemColors), "Control")]
        public Color StartColor { get; set; }

        #endregion
    }

    #endregion

    #region Converters

    public class DockPanelSkinConverter : ExpandableObjectConverter
    {
        #region Overrides

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            if (destinationType == typeof(DockPanelSkin))
            {
                return true;
            }

            return base.CanConvertTo(context, destinationType);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture,
                                         object value, Type destinationType)
        {
            if (destinationType == typeof(string) && value is DockPanelSkin)
            {
                return "DockPanelSkin";
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }

        #endregion
    }

    public class DockPanelGradientConverter : ExpandableObjectConverter
    {
        #region Overrides

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            if (destinationType == typeof(DockPanelGradient))
            {
                return true;
            }

            return base.CanConvertTo(context, destinationType);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture,
                                         object value, Type destinationType)
        {
            if (destinationType == typeof(string) && value is DockPanelGradient)
            {
                return "DockPanelGradient";
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }

        #endregion
    }

    public class AutoHideStripConverter : ExpandableObjectConverter
    {
        #region Overrides

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            if (destinationType == typeof(AutoHideStripSkin))
            {
                return true;
            }

            return base.CanConvertTo(context, destinationType);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture,
                                         object value, Type destinationType)
        {
            if (destinationType == typeof(string) && value is AutoHideStripSkin)
            {
                return "AutoHideStripSkin";
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }

        #endregion
    }

    public class DockPaneStripConverter : ExpandableObjectConverter
    {
        #region Overrides

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            if (destinationType == typeof(DockPaneStripSkin))
            {
                return true;
            }

            return base.CanConvertTo(context, destinationType);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture,
                                         object value, Type destinationType)
        {
            if (destinationType == typeof(string) && value is DockPaneStripSkin)
            {
                return "DockPaneStripSkin";
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }

        #endregion
    }

    public class DockPaneStripGradientConverter : ExpandableObjectConverter
    {
        #region Overrides

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            if (destinationType == typeof(DockPaneStripGradient))
            {
                return true;
            }

            return base.CanConvertTo(context, destinationType);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture,
                                         object value, Type destinationType)
        {
            if (destinationType == typeof(string) && value is DockPaneStripGradient)
            {
                return "DockPaneStripGradient";
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }

        #endregion
    }

    public class DockPaneTabGradientConverter : ExpandableObjectConverter
    {
        #region Overrides

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            if (destinationType == typeof(TabGradient))
            {
                return true;
            }

            return base.CanConvertTo(context, destinationType);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture,
                                         object value, Type destinationType)
        {
            if (destinationType == typeof(string) && value is TabGradient)
            {
                return "DockPaneTabGradient";
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }

        #endregion
    }

    #endregion
}
