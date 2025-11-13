using System.Runtime.Versioning;

using LogExpert.Core.Interface;

namespace LogExpert.Dialogs;

[SupportedOSPlatform("windows")]
internal partial class FilterSelectorForm : Form //TODO: Can this be changed to UserControl?
{
    #region Fields

    private readonly ILogLineColumnizerCallback _callback;
    private readonly IList<ILogLineColumnizer> _columnizerList;

    #endregion

    #region cTor

    public FilterSelectorForm (IList<ILogLineColumnizer> existingColumnizerList, ILogLineColumnizer currentColumnizer, ILogLineColumnizerCallback callback, IConfigManager configManager)
    {
        SuspendLayout();

        SelectedColumnizer = currentColumnizer;
        _callback = callback;
        InitializeComponent();

        AutoScaleDimensions = new SizeF(96F, 96F);
        AutoScaleMode = AutoScaleMode.Dpi;

        ApplyResources();

        ConfigManager = configManager;

        filterComboBox.SelectedIndexChanged += OnFilterComboBoxSelectedIndexChanged;

        // for the currently selected columnizer use the current instance and not the template instance from
        // columnizer registry. This ensures that changes made in columnizer config dialogs
        // will apply to the current instance
        _columnizerList = [];

        foreach (var col in existingColumnizerList)
        {
            _columnizerList.Add(col.GetType() == SelectedColumnizer.GetType() ? SelectedColumnizer : col);
        }

        foreach (var col in _columnizerList)
        {
            _ = filterComboBox.Items.Add(col);
        }

        foreach (var columnizer in _columnizerList)
        {
            if (columnizer.GetType() == SelectedColumnizer.GetType())
            {
                filterComboBox.SelectedItem = columnizer;
                break;
            }
        }

        ResumeLayout();
    }

    private void ApplyResources ()
    {
        Text = Resources.FilterSelectorForm_UI_Title;
        label1.Text = Resources.FilterSelectorForm_UI_Label_ChooseColumnizer;
        applyToAllCheckBox.Text = Resources.FilterSelectorForm_UI_CheckBox_ApplyToAll;
        configButton.Text = Resources.FilterSelectorForm_UI_Button_Config;
        okButton.Text = Resources.LogExpert_Common_UI_Button_OK;
        cancelButton.Text = Resources.LogExpert_Common_UI_Button_Cancel;
    }

    #endregion

    #region Properties

    public ILogLineColumnizer SelectedColumnizer { get; private set; }

    public bool ApplyToAll => applyToAllCheckBox.Checked;

    public bool IsConfigPressed { get; private set; }
    public IConfigManager ConfigManager { get; }

    #endregion

    #region Events handler

    private void OnFilterComboBoxSelectedIndexChanged (object sender, EventArgs e)
    {
        var col = _columnizerList[filterComboBox.SelectedIndex];
        SelectedColumnizer = col;
        var description = col.GetDescription();
        var timeshiftSupported = SelectedColumnizer.IsTimeshiftImplemented()
            ? Resources.FilterSelectorForm_UI_Text_SupportsTimeshift_Yes
            : Resources.FilterSelectorForm_UI_Text_SupportsTimeshift_No;
        description += string.Format(System.Globalization.CultureInfo.CurrentCulture,
            Resources.FilterSelectorForm_UI_Text_SupportsTimeshift_Format,
            timeshiftSupported);
        commentTextBox.Text = description;
        configButton.Enabled = SelectedColumnizer is IColumnizerConfigurator;
    }


    //TODO: Check if this logic can be remoed from this class and remove all the config manager instances from here.
    private void OnConfigButtonClick (object sender, EventArgs e)
    {
        if (SelectedColumnizer is IColumnizerConfigurator configurator)
        {
            var configDir = ConfigManager.ConfigDir;

            if (ConfigManager.Settings.Preferences.PortableMode)
            {
                configDir = ConfigManager.PortableModeDir;
            }

            configurator.Configure(_callback, configDir);
            IsConfigPressed = true;
        }
    }

    #endregion
}