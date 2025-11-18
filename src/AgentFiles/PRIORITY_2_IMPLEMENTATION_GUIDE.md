# Priority 2: Reliability & User Experience Implementation Guide

## Overview

**Timeline:** Weeks 4-6  
**Risk Level:** ?? MEDIUM  
**Effort:** High  
**Impact:** HIGH  

This guide focuses on improving reliability and user experience through better version handling, UI improvements, progress reporting, and enhanced error messages.

---

## Prerequisites

? **Priority 1 must be completed before starting Priority 2**

Ensure the following are in place:
- [ ] Hash verification working
- [ ] Path traversal protection active
- [ ] Regex safety validation functional
- [ ] Audit logging operational
- [ ] All Priority 1 tests passing

---

## Table of Contents

1. [Week 4: Enhanced Version Compatibility](#week-4-enhanced-version-compatibility)
2. [Week 4-5: Plugin Trust Management UI](#week-4-5-plugin-trust-management-ui)
3. [Week 5: Plugin Load Progress Reporting](#week-5-plugin-load-progress-reporting)
4. [Week 6: Improved Error Messages](#week-6-improved-error-messages)
5. [Testing Checklist](#testing-checklist)
6. [Completion Criteria](#completion-criteria)

---

## Week 4: Enhanced Version Compatibility

### Task 2.1: Semantic Versioning Support

**Estimated Time:** 2 days  
**Complexity:** Medium  
**Impact:** Medium

#### Step 1: Add NuGet.Versioning Package

**File:** `src/Directory.Packages.props`

Add the package reference:

```xml
<PackageVersion Include="NuGet.Versioning" Version="6.8.0" />
```

**File:** `src/PluginRegistry/LogExpert.PluginRegistry.csproj`

Add package reference:

```xml
<ItemGroup>
  <PackageReference Include="NuGet.Versioning" />
</ItemGroup>
```

**Verification:**
- [ ] Package restores successfully
- [ ] Project compiles with new dependency

---

#### Step 2: Update PluginManifest Version Compatibility

**File:** `src/PluginRegistry/PluginManifest.cs`

**Add using statements:**

```csharp
using NuGet.Versioning;
```

**Update `IsCompatibleWith` method:**

```csharp
/// <summary>
/// Checks if this plugin is compatible with the current LogExpert version using semantic versioning.
/// </summary>
/// <param name="logExpertVersion">Current LogExpert version to check against</param>
/// <returns>True if compatible, false otherwise</returns>
public bool IsCompatibleWith(Version logExpertVersion)
{
    if (Requires == null || string.IsNullOrWhiteSpace(Requires.LogExpert))
    {
        // No requirement specified, assume compatible
        return true;
    }

    try
    {
        // Convert System.Version to SemanticVersion
        var semVersion = new SemanticVersion(
            logExpertVersion.Major,
            logExpertVersion.Minor,
            logExpertVersion.Build >= 0 ? logExpertVersion.Build : 0,
            logExpertVersion.Revision >= 0 ? logExpertVersion.Revision.ToString() : null);

        // Parse version range (supports >=, <=, ~, ^, etc.)
        var versionRange = VersionRange.Parse(Requires.LogExpert);
        var isCompatible = versionRange.Satisfies(semVersion);

        if (!isCompatible)
        {
            _logger.Warn("Plugin {Name} v{Version} requires LogExpert {Requirement}, current: {Current}",
                Name, Version, Requires.LogExpert, logExpertVersion);
        }

        return isCompatible;
    }
    catch (Exception ex)
    {
        _logger.Error(ex, "Error checking version compatibility for {Name}: {Requirement}",
            Name, Requires.LogExpert);
        return false; // Fail closed on error
    }
}
```

**Update `IsValidVersion` method:**

```csharp
/// <summary>
/// Validates if a version string follows semantic versioning format.
/// </summary>
private static bool IsValidVersion(string versionString)
{
    if (string.IsNullOrWhiteSpace(versionString))
    {
        return false;
    }

    // Try parsing as semantic version (supports pre-release tags)
    return SemanticVersion.TryParse(versionString, out _);
}
```

**Update `IsValidVersionRequirement` method:**

```csharp
/// <summary>
/// Validates if a version requirement string is properly formatted.
/// </summary>
private static bool IsValidVersionRequirement(string requirement)
{
    if (string.IsNullOrWhiteSpace(requirement))
    {
        return false;
    }

    try
    {
        // Try to parse as version range
        _ = VersionRange.Parse(requirement);
        return true;
    }
    catch (Exception ex) when (ex is ArgumentException or FormatException)
    {
        return false;
    }
}
```

**Verification:**
- [ ] Semantic versions parse correctly (e.g., "1.0.0-beta")
- [ ] Version ranges work (e.g., ">=1.10.0", "~2.0.0", "^1.5.0")
- [ ] Pre-release versions are handled
- [ ] Invalid version strings are rejected

---

## Week 4-5: Plugin Trust Management UI

### Task 2.2: Plugin Trust Management Dialog

**Estimated Time:** 3 days  
**Complexity:** High  
**Impact:** HIGH

#### Step 1: Create Plugin Trust Dialog Form

**File:** `src/LogExpert.UI/Dialogs/PluginTrustDialog.cs`

```csharp
using System;
using System.IO;
using System.Windows.Forms;
using LogExpert.PluginRegistry;

namespace LogExpert.UI.Dialogs;

public partial class PluginTrustDialog : Form
{
    private TrustedPluginConfig _config;
    private readonly string _configPath;
    private bool _configModified;

    public PluginTrustDialog()
    {
        InitializeComponent();
        
        _configPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "LogExpert", "trusted-plugins.json");
        
        LoadConfiguration();
        PopulatePluginList();
        UpdateButtonStates();
    }

    private void LoadConfiguration()
    {
        try
        {
            if (File.Exists(_configPath))
            {
                var json = File.ReadAllText(_configPath);
                _config = Newtonsoft.Json.JsonConvert.DeserializeObject<TrustedPluginConfig>(json)
                    ?? new TrustedPluginConfig();
            }
            else
            {
                _config = new TrustedPluginConfig();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Error loading configuration: {ex.Message}",
                "Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            _config = new TrustedPluginConfig();
        }
    }

    private void PopulatePluginList()
    {
        pluginListView.Items.Clear();

        foreach (var pluginName in _config.PluginNames)
        {
            var hasHash = _config.PluginHashes.ContainsKey(pluginName) ? "Yes" : "No";
            var hash = _config.PluginHashes.TryGetValue(pluginName, out var h) 
                ? h[..Math.Min(8, h.Length)] + "..." 
                : "-";

            var item = new ListViewItem(pluginName);
            item.SubItems.Add(hasHash);
            item.SubItems.Add(hash);
            item.SubItems.Add("Trusted");
            
            pluginListView.Items.Add(item);
        }

        pluginCountLabel.Text = $"Total Plugins: {_config.PluginNames.Count}";
    }

    private void AddPluginButton_Click(object sender, EventArgs e)
    {
        using var openDialog = new OpenFileDialog
        {
            Filter = "Plugin Files (*.dll)|*.dll|All Files (*.*)|*.*",
            Title = "Select Plugin to Trust",
            Multiselect = false
        };

        if (openDialog.ShowDialog() != DialogResult.OK)
        {
            return;
        }

        var fileName = Path.GetFileName(openDialog.FileName);
        var hash = PluginValidator.CalculateFileHash(openDialog.FileName);

        if (_config.PluginNames.Contains(fileName, StringComparer.OrdinalIgnoreCase))
        {
            MessageBox.Show(
                $"Plugin '{fileName}' is already in the trusted list.",
                "Already Trusted",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
            return;
        }

        var result = MessageBox.Show(
            $"Trust plugin:\n\n" +
            $"Name: {fileName}\n" +
            $"Path: {openDialog.FileName}\n" +
            $"Hash: {hash[..16]}...\n\n" +
            $"Do you want to trust this plugin?",
            "Confirm Trust",
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

            // Log the trust addition
            PluginAuditLogger.LogTrustChange(fileName, "ADDED", hash);
        }
    }

    private void RemovePluginButton_Click(object sender, EventArgs e)
    {
        if (pluginListView.SelectedItems.Count == 0)
        {
            return;
        }

        var pluginName = pluginListView.SelectedItems[0].Text;

        var result = MessageBox.Show(
            $"Remove trust for plugin:\n\n{pluginName}\n\n" +
            $"The plugin will not be loaded until re-added to the trusted list.\n\n" +
            $"Continue?",
            "Confirm Removal",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning);

        if (result == DialogResult.Yes)
        {
            _config.PluginNames.Remove(pluginName);
            _config.PluginHashes.Remove(pluginName);
            _config.LastUpdated = DateTime.UtcNow;
            _configModified = true;

            PopulatePluginList();
            UpdateButtonStates();

            // Log the trust removal
            PluginAuditLogger.LogTrustChange(pluginName, "REMOVED");
        }
    }

    private void ViewHashButton_Click(object sender, EventArgs e)
    {
        if (pluginListView.SelectedItems.Count == 0)
        {
            return;
        }

        var pluginName = pluginListView.SelectedItems[0].Text;
        
        if (_config.PluginHashes.TryGetValue(pluginName, out var hash))
        {
            using var hashDialog = new PluginHashDialog(pluginName, hash);
            hashDialog.ShowDialog(this);
        }
        else
        {
            MessageBox.Show(
                $"No hash found for plugin: {pluginName}",
                "No Hash",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }
    }

    private void UpdateButtonStates()
    {
        var hasSelection = pluginListView.SelectedItems.Count > 0;
        removePluginButton.Enabled = hasSelection;
        viewHashButton.Enabled = hasSelection;
    }

    private void SaveButton_Click(object sender, EventArgs e)
    {
        if (!_configModified)
        {
            DialogResult = DialogResult.OK;
            Close();
            return;
        }

        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_configPath)!);
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(_config, Newtonsoft.Json.Formatting.Indented);
            File.WriteAllText(_configPath, json);

            MessageBox.Show(
                "Plugin trust configuration saved successfully.",
                "Success",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);

            DialogResult = DialogResult.OK;
            Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Failed to save configuration:\n\n{ex.Message}",
                "Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    private void CancelButton_Click(object sender, EventArgs e)
    {
        if (_configModified)
        {
            var result = MessageBox.Show(
                "Configuration has been modified. Discard changes?",
                "Unsaved Changes",
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

    private void PluginListView_SelectedIndexChanged(object sender, EventArgs e)
    {
        UpdateButtonStates();
    }
}
```

---

#### Step 2: Create Designer File

**File:** `src/LogExpert.UI/Dialogs/PluginTrustDialog.Designer.cs`

```csharp
namespace LogExpert.UI.Dialogs
{
    partial class PluginTrustDialog
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.ListView pluginListView;
        private System.Windows.Forms.Button addPluginButton;
        private System.Windows.Forms.Button removePluginButton;
        private System.Windows.Forms.Button viewHashButton;
        private System.Windows.Forms.Button saveButton;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.Label pluginCountLabel;
        private System.Windows.Forms.GroupBox groupBox1;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.pluginListView = new System.Windows.Forms.ListView();
            this.addPluginButton = new System.Windows.Forms.Button();
            this.removePluginButton = new System.Windows.Forms.Button();
            this.viewHashButton = new System.Windows.Forms.Button();
            this.saveButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.pluginCountLabel = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            
            // pluginListView
            this.pluginListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
                new System.Windows.Forms.ColumnHeader() { Text = "Plugin Name", Width = 200 },
                new System.Windows.Forms.ColumnHeader() { Text = "Hash Verified", Width = 100 },
                new System.Windows.Forms.ColumnHeader() { Text = "Hash (Partial)", Width = 150 },
                new System.Windows.Forms.ColumnHeader() { Text = "Status", Width = 100 }
            });
            this.pluginListView.FullRowSelect = true;
            this.pluginListView.GridLines = true;
            this.pluginListView.Location = new System.Drawing.Point(12, 45);
            this.pluginListView.MultiSelect = false;
            this.pluginListView.Name = "pluginListView";
            this.pluginListView.Size = new System.Drawing.Size(560, 300);
            this.pluginListView.TabIndex = 0;
            this.pluginListView.UseCompatibleStateImageBehavior = false;
            this.pluginListView.View = System.Windows.Forms.View.Details;
            this.pluginListView.SelectedIndexChanged += new System.EventHandler(this.PluginListView_SelectedIndexChanged);
            
            // addPluginButton
            this.addPluginButton.Location = new System.Drawing.Point(12, 355);
            this.addPluginButton.Name = "addPluginButton";
            this.addPluginButton.Size = new System.Drawing.Size(100, 30);
            this.addPluginButton.TabIndex = 1;
            this.addPluginButton.Text = "Add Plugin...";
            this.addPluginButton.UseVisualStyleBackColor = true;
            this.addPluginButton.Click += new System.EventHandler(this.AddPluginButton_Click);
            
            // removePluginButton
            this.removePluginButton.Enabled = false;
            this.removePluginButton.Location = new System.Drawing.Point(118, 355);
            this.removePluginButton.Name = "removePluginButton";
            this.removePluginButton.Size = new System.Drawing.Size(100, 30);
            this.removePluginButton.TabIndex = 2;
            this.removePluginButton.Text = "Remove";
            this.removePluginButton.UseVisualStyleBackColor = true;
            this.removePluginButton.Click += new System.EventHandler(this.RemovePluginButton_Click);
            
            // viewHashButton
            this.viewHashButton.Enabled = false;
            this.viewHashButton.Location = new System.Drawing.Point(224, 355);
            this.viewHashButton.Name = "viewHashButton";
            this.viewHashButton.Size = new System.Drawing.Size(100, 30);
            this.viewHashButton.TabIndex = 3;
            this.viewHashButton.Text = "View Hash...";
            this.viewHashButton.UseVisualStyleBackColor = true;
            this.viewHashButton.Click += new System.EventHandler(this.ViewHashButton_Click);
            
            // saveButton
            this.saveButton.Location = new System.Drawing.Point(416, 400);
            this.saveButton.Name = "saveButton";
            this.saveButton.Size = new System.Drawing.Size(75, 30);
            this.saveButton.TabIndex = 4;
            this.saveButton.Text = "Save";
            this.saveButton.UseVisualStyleBackColor = true;
            this.saveButton.Click += new System.EventHandler(this.SaveButton_Click);
            
            // cancelButton
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Location = new System.Drawing.Point(497, 400);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(75, 30);
            this.cancelButton.TabIndex = 5;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.Click += new System.EventHandler(this.CancelButton_Click);
            
            // pluginCountLabel
            this.pluginCountLabel.AutoSize = true;
            this.pluginCountLabel.Location = new System.Drawing.Point(12, 20);
            this.pluginCountLabel.Name = "pluginCountLabel";
            this.pluginCountLabel.Size = new System.Drawing.Size(100, 15);
            this.pluginCountLabel.TabIndex = 6;
            this.pluginCountLabel.Text = "Total Plugins: 0";
            
            // PluginTrustDialog
            this.AcceptButton = this.saveButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cancelButton;
            this.ClientSize = new System.Drawing.Size(584, 441);
            this.Controls.Add(this.pluginCountLabel);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.saveButton);
            this.Controls.Add(this.viewHashButton);
            this.Controls.Add(this.removePluginButton);
            this.Controls.Add(this.addPluginButton);
            this.Controls.Add(this.pluginListView);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "PluginTrustDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Plugin Trust Management";
            this.groupBox1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}
```

---

#### Step 3: Create Plugin Hash View Dialog

**File:** `src/LogExpert.UI/Dialogs/PluginHashDialog.cs`

```csharp
using System;
using System.Windows.Forms;

namespace LogExpert.UI.Dialogs;

public partial class PluginHashDialog : Form
{
    public PluginHashDialog(string pluginName, string hash)
    {
        InitializeComponent();
        
        pluginNameLabel.Text = $"Plugin: {pluginName}";
        hashTextBox.Text = hash;
        hashTextBox.Select(0, 0); // Deselect
    }

    private void CopyButton_Click(object sender, EventArgs e)
    {
        try
        {
            Clipboard.SetText(hashTextBox.Text);
            MessageBox.Show(
                "Hash copied to clipboard.",
                "Success",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Failed to copy hash: {ex.Message}",
                "Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    private void CloseButton_Click(object sender, EventArgs e)
    {
        Close();
    }
}
```

**File:** `src/LogExpert.UI/Dialogs/PluginHashDialog.Designer.cs`

```csharp
namespace LogExpert.UI.Dialogs
{
    partial class PluginHashDialog
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.Label pluginNameLabel;
        private System.Windows.Forms.TextBox hashTextBox;
        private System.Windows.Forms.Button copyButton;
        private System.Windows.Forms.Button closeButton;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.pluginNameLabel = new System.Windows.Forms.Label();
            this.hashTextBox = new System.Windows.Forms.TextBox();
            this.copyButton = new System.Windows.Forms.Button();
            this.closeButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            
            // pluginNameLabel
            this.pluginNameLabel.AutoSize = true;
            this.pluginNameLabel.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.pluginNameLabel.Location = new System.Drawing.Point(12, 15);
            this.pluginNameLabel.Name = "pluginNameLabel";
            this.pluginNameLabel.Size = new System.Drawing.Size(100, 15);
            this.pluginNameLabel.TabIndex = 0;
            this.pluginNameLabel.Text = "Plugin: ";
            
            // hashTextBox
            this.hashTextBox.Font = new System.Drawing.Font("Consolas", 9F);
            this.hashTextBox.Location = new System.Drawing.Point(12, 40);
            this.hashTextBox.Multiline = true;
            this.hashTextBox.Name = "hashTextBox";
            this.hashTextBox.ReadOnly = true;
            this.hashTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.hashTextBox.Size = new System.Drawing.Size(460, 80);
            this.hashTextBox.TabIndex = 1;
            
            // copyButton
            this.copyButton.Location = new System.Drawing.Point(316, 130);
            this.copyButton.Name = "copyButton";
            this.copyButton.Size = new System.Drawing.Size(75, 30);
            this.copyButton.TabIndex = 2;
            this.copyButton.Text = "Copy";
            this.copyButton.UseVisualStyleBackColor = true;
            this.copyButton.Click += new System.EventHandler(this.CopyButton_Click);
            
            // closeButton
            this.closeButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.closeButton.Location = new System.Drawing.Point(397, 130);
            this.closeButton.Name = "closeButton";
            this.closeButton.Size = new System.Drawing.Size(75, 30);
            this.closeButton.TabIndex = 3;
            this.closeButton.Text = "Close";
            this.closeButton.UseVisualStyleBackColor = true;
            this.closeButton.Click += new System.EventHandler(this.CloseButton_Click);
            
            // PluginHashDialog
            this.AcceptButton = this.closeButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(484, 171);
            this.Controls.Add(this.closeButton);
            this.Controls.Add(this.copyButton);
            this.Controls.Add(this.hashTextBox);
            this.Controls.Add(this.pluginNameLabel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "PluginHashDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Plugin Hash";
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}
```

---

#### Step 4: Integrate into Main Menu

**File:** `src/LogExpert.UI/Dialogs/LogTabWindow/LogTabWindow.cs`

**Add menu item to Settings or Tools menu:**

```csharp
// In the form designer or InitializeComponent:
private ToolStripMenuItem pluginTrustManagementMenuItem;

// In menu initialization:
pluginTrustManagementMenuItem = new ToolStripMenuItem();
pluginTrustManagementMenuItem.Name = "pluginTrustManagementMenuItem";
pluginTrustManagementMenuItem.Text = "Plugin Trust Management...";
pluginTrustManagementMenuItem.Click += new EventHandler(PluginTrustManagement_Click);

// Add to appropriate menu (e.g., Settings menu)
settingsToolStripMenuItem.DropDownItems.Add(pluginTrustManagementMenuItem);

// Handler:
private void PluginTrustManagement_Click(object sender, EventArgs e)
{
    using var dialog = new PluginTrustDialog();
    var result = dialog.ShowDialog(this);
    
    if (result == DialogResult.OK)
    {
        // Optional: Prompt to restart application
        var restartPrompt = MessageBox.Show(
            "Plugin trust configuration has been updated.\n\n" +
            "Changes will take effect on next application restart.\n\n" +
            "Restart now?",
            "Restart Required",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question);
        
        if (restartPrompt == DialogResult.Yes)
        {
            Application.Restart();
        }
    }
}
```

**Verification:**
- [ ] Dialog opens from menu
- [ ] Can add plugins
- [ ] Can remove plugins
- [ ] Can view hashes
- [ ] Configuration saves correctly
- [ ] Changes apply on restart

---

## Week 5: Plugin Load Progress Reporting

### Task 2.3: Progress Reporting

**Estimated Time:** 1 day  
**Complexity:** Low  
**Impact:** Low

#### Step 1: Create Progress Event Args

**File:** `src/PluginRegistry/PluginLoadProgress.cs`

```csharp
using System;

namespace LogExpert.PluginRegistry;

/// <summary>
/// Event args for plugin load progress reporting.
/// </summary>
public class PluginLoadProgressEventArgs : EventArgs
{
    public int TotalPlugins { get; set; }
    public int LoadedPlugins { get; set; }
    public int SkippedPlugins { get; set; }
    public int FailedPlugins { get; set; }
    public string? CurrentPlugin { get; set; }
    public string? Status { get; set; }
    public int PercentComplete => TotalPlugins > 0 
        ? (LoadedPlugins + SkippedPlugins + FailedPlugins) * 100 / TotalPlugins 
        : 0;
}
```

---

#### Step 2: Update PluginRegistry with Progress Events

**File:** `src/PluginRegistry/PluginRegistry.cs`

**Add event:**

```csharp
/// <summary>
/// Raised when plugin loading progress changes.
/// </summary>
public event EventHandler<PluginLoadProgressEventArgs>? PluginLoadProgressChanged;

protected virtual void OnPluginLoadProgressChanged(PluginLoadProgressEventArgs e)
{
    PluginLoadProgressChanged?.Invoke(this, e);
}
```

**Update `LoadPlugins` method:**

```csharp
internal void LoadPlugins()
{
    _logger.Info("Loading plugins with security validation...");
    
    PluginAuditLogger.RotateLogIfNeeded();
    PluginPermissionManager.LoadPermissions(_applicationConfigurationFolder);
    
    RegisteredColumnizers =
    [
        new DefaultLogfileColumnizer(),
        new TimestampColumnizer(),
        new SquareBracketColumnizer(),
        new ClfColumnizer(),
    ];
    RegisteredFileSystemPlugins.Add(new LocalFileSystem());
    
    var pluginDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "plugins");
    if (!Directory.Exists(pluginDir))
    {
        _logger.Warn("Plugin directory not found: {PluginDir}", pluginDir);
        pluginDir = ".";
    }
    
    AppDomain.CurrentDomain.AssemblyResolve += ColumnizerResolveEventHandler;
    
    var interfaceName = typeof(ILogLineColumnizer).FullName
        ?? throw new NotImplementedException("Interface name is null");
    
    var pluginFiles = Directory.GetFiles(pluginDir, "*.dll").ToList();
    var totalCount = pluginFiles.Count;
    var loadedCount = 0;
    var skippedCount = 0;
    var failedCount = 0;
    
    foreach (var dllName in pluginFiles)
    {
        var fileName = Path.GetFileName(dllName);
        
        // Report progress - validating
        OnPluginLoadProgressChanged(new PluginLoadProgressEventArgs
        {
            TotalPlugins = totalCount,
            LoadedPlugins = loadedCount,
            SkippedPlugins = skippedCount,
            FailedPlugins = failedCount,
            CurrentPlugin = fileName,
            Status = "Validating..."
        });
        
        try
        {
            if (!PluginValidator.ValidatePlugin(dllName, out var manifest))
            {
                skippedCount++;
                _logger.Info("Skipped plugin: {FileName}", fileName);
                PluginAuditLogger.LogPluginLoad(manifest?.Name ?? fileName, dllName, false, "Validation failed");
                
                // Report progress - skipped
                OnPluginLoadProgressChanged(new PluginLoadProgressEventArgs
                {
                    TotalPlugins = totalCount,
                    LoadedPlugins = loadedCount,
                    SkippedPlugins = skippedCount,
                    FailedPlugins = failedCount,
                    CurrentPlugin = fileName,
                    Status = "Validation failed"
                });
                
                continue;
            }
            
            if (manifest != null)
            {
                _logger.Info("Plugin {PluginName} v{Version} by {Author}",
                    manifest.Name, manifest.Version, manifest.Author ?? "Unknown");
            }
            
            // Report progress - loading
            OnPluginLoadProgressChanged(new PluginLoadProgressEventArgs
            {
                TotalPlugins = totalCount,
                LoadedPlugins = loadedCount,
                SkippedPlugins = skippedCount,
                FailedPlugins = failedCount,
                CurrentPlugin = fileName,
                Status = "Loading..."
            });
            
            if (LoadPluginAssemblySafe(dllName, interfaceName))
            {
                loadedCount++;
                PluginAuditLogger.LogPluginLoad(manifest?.Name ?? fileName, dllName, true);
                
                // Report progress - loaded
                OnPluginLoadProgressChanged(new PluginLoadProgressEventArgs
                {
                    TotalPlugins = totalCount,
                    LoadedPlugins = loadedCount,
                    SkippedPlugins = skippedCount,
                    FailedPlugins = failedCount,
                    CurrentPlugin = fileName,
                    Status = "Loaded"
                });
            }
            else
            {
                failedCount++;
                PluginAuditLogger.LogPluginLoad(manifest?.Name ?? fileName, dllName, false, "Load failed");
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Exception loading plugin: {FileName}", fileName);
            failedCount++;
            PluginAuditLogger.LogPluginLoad(fileName, dllName, false, $"Exception: {ex.Message}");
        }
    }
    
    // Final progress report
    OnPluginLoadProgressChanged(new PluginLoadProgressEventArgs
    {
        TotalPlugins = totalCount,
        LoadedPlugins = loadedCount,
        SkippedPlugins = skippedCount,
        FailedPlugins = failedCount,
        CurrentPlugin = null,
        Status = "Complete"
    });
    
    _logger.Info("Plugin loading complete. Loaded: {LoadedCount}, Skipped: {SkippedCount}, Failed: {FailedCount}",
        loadedCount, skippedCount, failedCount);
    
    PluginPermissionManager.SavePermissions(_applicationConfigurationFolder);
}
```

**Verification:**
- [ ] Progress events fire correctly
- [ ] Progress percentage calculates correctly
- [ ] Current plugin name is reported
- [ ] Status messages are appropriate
- [ ] Final status shows completion

---

## Week 6: Improved Error Messages

### Task 2.4: User-Friendly Error Messages

**Estimated Time:** 2 days  
**Complexity:** Medium  
**Impact:** Medium

#### Step 1: Create Resource Strings

**File:** `src/LogExpert.Resources/PluginRegistry.resx`

Add the following resource strings:

```xml
<data name="Error_PluginNotTrusted" xml:space="preserve">
  <value>Plugin '{0}' is not trusted.

To use this plugin, add it to the trusted plugins list:
1. Go to Settings > Plugin Trust Management
2. Click 'Add Plugin...'
3. Select the plugin file
4. Confirm trust

For security, only trust plugins from known sources.</value>
</data>

<data name="Error_PluginHashMismatch" xml:space="preserve">
  <value>Security Warning: Plugin '{0}' failed integrity check.

The file hash does not match the expected value. This could indicate:
• The file has been modified
• The file is corrupted
• A security breach

Expected hash: {1}
Actual hash: {2}

Action required:
• If this is expected (plugin updated), remove and re-add the plugin
• If unexpected, do NOT use this plugin and investigate further</value>
</data>

<data name="Error_ManifestNotFound" xml:space="preserve">
  <value>Plugin manifest not found for '{0}'.

Modern plugins require a manifest.json file that describes:
• Plugin capabilities
• Version requirements
• Required permissions

The plugin may still load with default settings, but this is not recommended for security.</value>
</data>

<data name="Error_IncompatibleVersion" xml:space="preserve">
  <value>Plugin '{0}' version {1} is incompatible.

Required LogExpert version: {2}
Current LogExpert version: {3}

Please:
• Update LogExpert to version {2} or higher, or
• Find a compatible version of the plugin</value>
</data>

<data name="Error_PathTraversal" xml:space="preserve">
  <value>Security Warning: Plugin '{0}' contains suspicious paths.

The manifest specifies files outside the plugin directory.
This is not allowed for security reasons.

Plugin will not be loaded.</value>
</data>

<data name="Error_UnsafeRegex" xml:space="preserve">
  <value>Unsafe regular expression pattern detected.

Pattern: {0}
Issue: {1}

This pattern could cause performance problems or security issues.
Please use a simpler pattern.</value>
</data>
```

---

#### Step 2: Update PluginValidator with Resource Strings

**File:** `src/PluginRegistry/PluginValidator.cs`

**Add method for user-friendly errors:**

```csharp
/// <summary>
/// Validates a plugin and provides user-friendly error messages.
/// </summary>
public static bool ValidatePlugin(string dllPath, out PluginManifest manifest, out string userFriendlyError)
{
    manifest = null;
    userFriendlyError = string.Empty;
    
    if (!File.Exists(dllPath))
    {
        userFriendlyError = $"Plugin file not found: {dllPath}";
        return false;
    }
    
    var fileName = Path.GetFileName(dllPath);
    
    // Check if known dependency
    if (_knownDependencies.Contains(fileName))
    {
        return false; // Not an error, just not a plugin
    }
    
    // Calculate hash
    var fileHash = CalculateFileHash(dllPath);
    
    // Check trust
    var isTrustedByName = _trustedPluginConfig.PluginNames.Contains(fileName, StringComparer.OrdinalIgnoreCase);
    var isTrustedByHash = _trustedPluginConfig.PluginHashes.ContainsValue(fileHash);
    
    if (!isTrustedByName && !isTrustedByHash)
    {
        userFriendlyError = string.Format(
            LogExpert.Resources.PluginRegistry_Resources.Error_PluginNotTrusted,
            fileName);
        _logger.Warn("Plugin not trusted: {FileName}", fileName);
        return false;
    }
    
    // Verify hash for known plugins
    if (isTrustedByName && _trustedPluginConfig.PluginHashes.TryGetValue(fileName, out var expectedHash))
    {
        if (!expectedHash.Equals(fileHash, StringComparison.OrdinalIgnoreCase))
        {
            userFriendlyError = string.Format(
                LogExpert.Resources.PluginRegistry_Resources.Error_PluginHashMismatch,
                fileName,
                expectedHash[..16] + "...",
                fileHash[..16] + "...");
            
            PluginAuditLogger.LogSecurityEvent(fileName, "Hash Mismatch", 
                $"Expected: {expectedHash}, Actual: {fileHash}");
            
            return false;
        }
    }
    
    // Load manifest
    var manifestPath = Path.ChangeExtension(dllPath, ".manifest.json");
    if (File.Exists(manifestPath))
    {
        try
        {
            manifest = PluginManifest.Load(manifestPath);
            if (manifest != null)
            {
                // Check version compatibility
                var assembly = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();
                var version = assembly.GetName().Version;
                
                if (version != null && !manifest.IsCompatibleWith(version))
                {
                    userFriendlyError = string.Format(
                        LogExpert.Resources.PluginRegistry_Resources.Error_IncompatibleVersion,
                        manifest.Name,
                        manifest.Version,
                        manifest.Requires?.LogExpert ?? "unknown",
                        version.ToString(3));
                    
                    return false;
                }
                
                // Validate paths
                var pluginDir = Path.GetDirectoryName(dllPath);
                if (!ValidateManifestPaths(manifest, pluginDir))
                {
                    userFriendlyError = string.Format(
                        LogExpert.Resources.PluginRegistry_Resources.Error_PathTraversal,
                        manifest.Name);
                    
                    PluginAuditLogger.LogSecurityEvent(manifest.Name, "Path Traversal", 
                        "Manifest paths escape plugin directory");
                    
                    return false;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error loading manifest for {FileName}", fileName);
            // Continue without manifest
        }
    }
    
    // Validate assembly
    if (!CanLoadAssembly(dllPath) || !IsValidDotNetAssembly(dllPath))
    {
        userFriendlyError = $"Plugin '{fileName}' is not a valid .NET assembly or cannot be loaded.";
        return false;
    }
    
    return true;
}
```

---

#### Step 3: Show Errors in UI

**File:** `src/LogExpert.UI/Dialogs/PluginErrorDialog.cs`

```csharp
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace LogExpert.UI.Dialogs;

public partial class PluginErrorDialog : Form
{
    public PluginErrorDialog(List<string> errors)
    {
        InitializeComponent();
        
        errorListBox.Items.Clear();
        foreach (var error in errors)
        {
            errorListBox.Items.Add(error);
        }
        
        errorCountLabel.Text = $"Total Errors: {errors.Count}";
    }

    private void CloseButton_Click(object sender, EventArgs e)
    {
        Close();
    }
    
    private void CopyButton_Click(object sender, EventArgs e)
    {
        if (errorListBox.SelectedItem != null)
        {
            Clipboard.SetText(errorListBox.SelectedItem.ToString());
            MessageBox.Show("Error copied to clipboard.", "Success");
        }
    }
}
```

---

## Testing Checklist

### Manual Testing

- [ ] **Version Compatibility**
  - [ ] Plugin with compatible version loads
  - [ ] Plugin with incompatible version is rejected with clear message
  - [ ] Pre-release versions are handled correctly

- [ ] **Plugin Trust UI**
  - [ ] Can open dialog from menu
  - [ ] Can add plugin and hash is calculated
  - [ ] Can remove plugin with confirmation
  - [ ] Can view full hash
  - [ ] Can copy hash to clipboard
  - [ ] Configuration saves and persists
  - [ ] Changes require restart (optional)

- [ ] **Progress Reporting**
  - [ ] Progress updates during plugin load
  - [ ] Status messages are shown
  - [ ] Final status shows completion
  - [ ] Percentages calculate correctly

- [ ] **Error Messages**
  - [ ] Untrusted plugin error is clear and actionable
  - [ ] Hash mismatch error shows both hashes
  - [ ] Version incompatibility error shows requirements
  - [ ] Path traversal error is clear
  - [ ] Errors can be copied to clipboard

---

## Completion Criteria

- [ ] All code compiles without warnings
- [ ] All existing tests pass
- [ ] New functionality tested manually
- [ ] UI is responsive and user-friendly
- [ ] Error messages are clear and actionable
- [ ] Progress reporting works smoothly
- [ ] Configuration persists correctly
- [ ] No regressions in existing functionality

---

## Next Steps

After completing Priority 2:
? Proceed to `PRIORITY_3_IMPLEMENTATION_GUIDE.md`
