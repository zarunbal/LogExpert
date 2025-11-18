using System.Globalization;
using System.Runtime.Versioning;

using LogExpert.PluginRegistry;

using Newtonsoft.Json;

namespace LogExpert.UI.Dialogs;

[SupportedOSPlatform("windows")]
internal partial class PluginTrustDialog : Form
{
    #region Fields

    private TrustedPluginConfig _config;
    private readonly string _configPath;
    private bool _configModified;

    #endregion

    #region cTor

    public PluginTrustDialog (Form parent)
    {
        SuspendLayout();

        AutoScaleDimensions = new SizeF(96F, 96F);
        AutoScaleMode = AutoScaleMode.Dpi;

        InitializeComponent();
        ApplyResources();

        Owner = parent;

        _configPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "LogExpert", "trusted-plugins.json");

        LoadConfiguration();
        PopulatePluginList();
        UpdateButtonStates();

        ResumeLayout();
    }

    #endregion

    #region Private Methods

    private void ApplyResources ()
    {
        // Dialog title
        Text = Resources.PluginTrustDialog_UI_Title;

        // Labels
        pluginCountLabel.Text = string.Format(Resources.PluginTrustDialog_UI_Label_TotalPlugins, 0);
        groupBoxPlugins.Text = Resources.PluginTrustDialog_UI_GroupBox_TrustedPlugins;

        // Buttons
        addPluginButton.Text = Resources.PluginTrustDialog_UI_Button_AddPlugin;
        removePluginButton.Text = Resources.PluginTrustDialog_UI_Button_Remove;
        viewHashButton.Text = Resources.PluginTrustDialog_UI_Button_ViewHash;
        saveButton.Text = Resources.LogExpert_Common_UI_Button_Save;
        cancelButton.Text = Resources.LogExpert_Common_UI_Button_Cancel;

        // ListView columns
        columnName.Text = Resources.PluginTrustDialog_UI_Column_PluginName;
        columnHashVerified.Text = Resources.PluginTrustDialog_UI_Column_HashVerified;
        columnHashPartial.Text = Resources.PluginTrustDialog_UI_Column_HashPartial;
        columnStatus.Text = Resources.PluginTrustDialog_UI_Column_Status;
    }

    private void LoadConfiguration ()
    {
        try
        {
            if (File.Exists(_configPath))
            {
                var json = File.ReadAllText(_configPath);
                _config = JsonConvert.DeserializeObject<TrustedPluginConfig>(json)
                    ?? CreateDefaultConfiguration();
            }
            else
            {
                // Create minimal configuration for UI display
                // PluginValidator will create the real config with hashes when plugins load
                _config = CreateDefaultConfiguration();

                // Don't save yet - let PluginValidator create it with proper hashes
                // If user makes changes, they'll be saved then
            }
        }
        catch (Exception ex)
        {
            _ = MessageBox.Show(
                string.Format(CultureInfo.InvariantCulture, Resources.PluginTrustDialog_UI_Message_LoadError, ex.Message),
                Resources.PluginTrustDialog_UI_Message_ErrorTitle,
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            _config = CreateDefaultConfiguration();
        }
    }

    private static TrustedPluginConfig CreateDefaultConfiguration ()
    {
        // Create configuration with built-in trusted plugins
        // Get hashes from PluginValidator to ensure consistency
        var config = new TrustedPluginConfig
        {
            PluginNames =
            [
                "AutoColumnizer.dll",
                "CsvColumnizer.dll",
                "JsonColumnizer.dll",
                "JsonCompactColumnizer.dll",
                "RegexColumnizer.dll",
                "Log4jXmlColumnizer.dll",
                "GlassfishColumnizer.dll",
                "DefaultPlugins.dll",
                "FlashIconHighlighter.dll",
                "SftpFileSystem.dll",
                "SftpFileSystem.dll (x86)"
            ],
            PluginHashes = PluginValidator.GetBuiltInPluginHashes(),
            AllowUserTrustedPlugins = true,
            HashAlgorithm = "SHA256",
            LastUpdated = DateTime.UtcNow
        };

        return config;
    }

    private void PopulatePluginList ()
    {
        pluginListView.Items.Clear();

        foreach (var pluginName in _config.PluginNames)
        {
            var hasHash = _config.PluginHashes.ContainsKey(pluginName)
                ? Resources.PluginTrustDialog_UI_Value_Yes
                : Resources.PluginTrustDialog_UI_Value_No;
            var hash = _config.PluginHashes.TryGetValue(pluginName, out var h)
                ? (h.Length > 16 ? h[..16] + "..." : h)
                : "-";

            var item = new ListViewItem(pluginName);
            _ = item.SubItems.Add(hasHash);
            _ = item.SubItems.Add(hash);
            _ = item.SubItems.Add(Resources.PluginTrustDialog_UI_Value_Trusted);

            _ = pluginListView.Items.Add(item);
        }

        pluginCountLabel.Text = string.Format(CultureInfo.InvariantCulture, Resources.PluginTrustDialog_UI_Label_TotalPlugins, _config.PluginNames.Count);
    }

    private void UpdateButtonStates ()
    {
        var hasSelection = pluginListView.SelectedItems.Count > 0;
        removePluginButton.Enabled = hasSelection;
        viewHashButton.Enabled = hasSelection && HasHashForSelection();
    }

    private bool HasHashForSelection ()
    {
        if (pluginListView.SelectedItems.Count == 0)
            return false;

        var pluginName = pluginListView.SelectedItems[0].Text;
        return _config.PluginHashes.ContainsKey(pluginName);
    }

    #endregion

    #region Event Handlers

    private void AddPluginButton_Click (object sender, EventArgs e)
    {
        using var openDialog = new OpenFileDialog
        {
            Filter = Resources.PluginTrustDialog_UI_FileDialog_Filter,
            Title = Resources.PluginTrustDialog_UI_FileDialog_Title,
            Multiselect = false
        };

        if (openDialog.ShowDialog() != DialogResult.OK)
        {
            return;
        }

        var fileName = Path.GetFileName(openDialog.FileName);
        var hash = PluginHashCalculator.CalculateHash(openDialog.FileName);

        if (_config.PluginNames.Contains(fileName, StringComparer.OrdinalIgnoreCase))
        {
            _ = MessageBox.Show(
                string.Format(CultureInfo.InvariantCulture, Resources.PluginTrustDialog_UI_Message_AlreadyTrusted, fileName),
                Resources.PluginTrustDialog_UI_Message_AlreadyTrustedTitle,
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
            return;
        }

        var hashDisplay = hash.Length > 32 ? hash[..32] + "..." : hash;
        var result = MessageBox.Show(
            string.Format(CultureInfo.InvariantCulture, Resources.PluginTrustDialog_UI_Message_ConfirmTrust, fileName, openDialog.FileName, hashDisplay),
            Resources.PluginTrustDialog_UI_Message_ConfirmTrustTitle,
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question);

        if (result == DialogResult.Yes)
        {
            _config.PluginNames.Add(fileName);
            _config.PluginHashes[fileName] = hash;
            _config.LastUpdated = DateTime.UtcNow;
            _configModified = true;

            PopulatePluginList();
            UpdateButtonStates();
        }
    }

    private void RemovePluginButton_Click (object sender, EventArgs e)
    {
        if (pluginListView.SelectedItems.Count == 0)
        {
            return;
        }

        var pluginName = pluginListView.SelectedItems[0].Text;

        var result = MessageBox.Show(
            string.Format(CultureInfo.InvariantCulture, Resources.PluginTrustDialog_UI_Message_ConfirmRemove, pluginName),
            Resources.PluginTrustDialog_UI_Message_ConfirmRemoveTitle,
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning);

        if (result == DialogResult.Yes)
        {
            _ = _config.PluginNames.Remove(pluginName);
            _ = _config.PluginHashes.Remove(pluginName);
            _config.LastUpdated = DateTime.UtcNow;
            _configModified = true;

            PopulatePluginList();
            UpdateButtonStates();
        }
    }

    private void ViewHashButton_Click (object sender, EventArgs e)
    {
        if (pluginListView.SelectedItems.Count == 0)
        {
            return;
        }

        var pluginName = pluginListView.SelectedItems[0].Text;

        if (_config.PluginHashes.TryGetValue(pluginName, out var hash))
        {
            using var hashDialog = new PluginHashDialog(this, pluginName, hash);
            _ = hashDialog.ShowDialog();
        }
        else
        {
            _ = MessageBox.Show(
                string.Format(CultureInfo.InvariantCulture, Resources.PluginTrustDialog_UI_Message_NoHash, pluginName),
                Resources.PluginTrustDialog_UI_Message_NoHashTitle,
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }
    }

    private void SaveButton_Click (object sender, EventArgs e)
    {
        if (!_configModified)
        {
            DialogResult = DialogResult.OK;
            Close();
            return;
        }

        try
        {
            _ = Directory.CreateDirectory(Path.GetDirectoryName(_configPath)!);
            var json = JsonConvert.SerializeObject(_config, Formatting.Indented);
            File.WriteAllText(_configPath, json);

            _ = MessageBox.Show(
                Resources.PluginTrustDialog_UI_Message_SaveSuccess,
                Resources.PluginTrustDialog_UI_Message_SuccessTitle,
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);

            DialogResult = DialogResult.OK;
            Close();
        }
        catch (Exception ex)
        {
            _ = MessageBox.Show(
                string.Format(CultureInfo.InvariantCulture, Resources.PluginTrustDialog_UI_Message_SaveError, ex.Message),
                Resources.PluginTrustDialog_UI_Message_ErrorTitle,
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    private void CancelButton_Click (object sender, EventArgs e)
    {
        if (_configModified)
        {
            var result = MessageBox.Show(
                Resources.PluginTrustDialog_UI_Message_UnsavedChanges,
                Resources.PluginTrustDialog_UI_Message_UnsavedChangesTitle,
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result == DialogResult.No)
            {
                return;
            }
        }

        DialogResult = DialogResult.Cancel;
        Close();
    }

    private void PluginListView_SelectedIndexChanged (object sender, EventArgs e)
    {
        UpdateButtonStates();
    }

    #endregion
}
