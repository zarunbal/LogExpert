using System.Runtime.Versioning;

using LogExpert.Core.Classes.Persister;

namespace LogExpert.UI.Dialogs;

/// <summary>
/// Enhanced dialog for handling missing files with browsing and alternative selection.
/// Phase 2 implementation of the Project File Validator.
/// </summary>
[SupportedOSPlatform("windows")]
public partial class MissingFilesDialog : Form
{
    #region Fields

    private readonly ProjectValidationResult _validationResult;
    private readonly Dictionary<string, MissingFileItem> _fileItems;

    #endregion

    #region Properties

    /// <summary>
    /// Gets the dialog result indicating the user's choice.
    /// </summary>
    public MissingFilesDialogResult Result { get; private set; }

    /// <summary>
    /// Gets the dictionary of selected alternative paths for missing files.
    /// Key: original path, Value: selected alternative path
    /// </summary>
    public Dictionary<string, string> SelectedAlternatives { get; private set; }

    #endregion

    #region Constructor

    /// <summary>
    /// Constructor for MissingFilesDialog.
    /// </summary>
    /// <param name="validationResult">Validation result containing file information</param>
    public MissingFilesDialog (ProjectValidationResult validationResult)
    {
        ArgumentNullException.ThrowIfNull(validationResult);

        _validationResult = validationResult;
        _fileItems = [];
        SelectedAlternatives = [];
        Result = MissingFilesDialogResult.Cancel;

        InitializeComponent();
        InitializeFileItems();
        PopulateListView();
        UpdateSummary();
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Shows the dialog and returns the user's choice.
    /// </summary>
    /// <param name="validationResult">Validation result</param>
    /// <returns>Dialog result</returns>
    public static MissingFilesDialogResult ShowDialog (ProjectValidationResult validationResult)
    {
        using var dialog = new MissingFilesDialog(validationResult);
        _ = dialog.ShowDialog();
        return dialog.Result;
    }

    /// <summary>
    /// Shows the dialog and returns alternatives if selected.
    /// </summary>
    /// <param name="validationResult">Validation result</param>
    /// <param name="selectedAlternatives">Dictionary of selected alternatives</param>
    /// <returns>Dialog result</returns>
    public static MissingFilesDialogResult ShowDialog (ProjectValidationResult validationResult, out Dictionary<string, string> selectedAlternatives)
    {
        using var dialog = new MissingFilesDialog(validationResult);
        _ = dialog.ShowDialog();
        selectedAlternatives = dialog.SelectedAlternatives;
        return dialog.Result;
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Creates status icons for the ImageList.
    /// </summary>
    private void CreateStatusIcons ()
    {
        // Create simple colored circles as status indicators

        // Valid - Green circle
        var validIcon = new Bitmap(16, 16);
        using (var g = Graphics.FromImage(validIcon))
        {
            g.Clear(Color.Transparent);
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            g.FillEllipse(Brushes.Green, 2, 2, 12, 12);
        }

        imageListStatus.Images.Add("Valid", validIcon);

        // Missing - Red circle
        var missingIcon = new Bitmap(16, 16);
        using (var g = Graphics.FromImage(missingIcon))
        {
            g.Clear(Color.Transparent);
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            g.FillEllipse(Brushes.Red, 2, 2, 12, 12);
        }

        imageListStatus.Images.Add("Missing", missingIcon);

        // Alternative available - Orange circle
        var alternativeIcon = new Bitmap(16, 16);
        using (var g = Graphics.FromImage(alternativeIcon))
        {
            g.Clear(Color.Transparent);
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            g.FillEllipse(Brushes.Orange, 2, 2, 12, 12);
        }

        imageListStatus.Images.Add("Alternative", alternativeIcon);

        // Alternative selected - Blue circle
        var selectedIcon = new Bitmap(16, 16);
        using (var g = Graphics.FromImage(selectedIcon))
        {
            g.Clear(Color.Transparent);
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            g.FillEllipse(Brushes.Blue, 2, 2, 12, 12);
        }

        imageListStatus.Images.Add("Selected", selectedIcon);
    }

    /// <summary>
    /// Initializes the file items dictionary from validation result.
    /// </summary>
    private void InitializeFileItems ()
    {
        // Add valid files
        foreach (var validPath in _validationResult.ValidFiles)
        {
            var item = new MissingFileItem(validPath, FileStatus.Valid);
            _fileItems[validPath] = item;
        }

        // Add missing files
        foreach (var missingPath in _validationResult.MissingFiles)
        {
            var alternatives = _validationResult.PossibleAlternatives.TryGetValue(missingPath, out List<string>? value)
                ? value
                : [];

            var status = alternatives.Count > 0
                ? FileStatus.MissingWithAlternatives
                : FileStatus.Missing;

            var item = new MissingFileItem(missingPath, status)
            {
                Alternatives = alternatives
            };

            _fileItems[missingPath] = item;
        }
    }

    /// <summary>
    /// Populates the ListView with file items.
    /// </summary>
    private void PopulateListView ()
    {
        listViewFiles.BeginUpdate();
        listViewFiles.Items.Clear();

        foreach (var fileItem in _fileItems.Values)
        {
            var listItem = new ListViewItem(fileItem.DisplayName)
            {
                Tag = fileItem,
                ImageKey = fileItem.Status switch
                {
                    FileStatus.Valid => "Valid",
                    FileStatus.MissingWithAlternatives => "Alternative",
                    FileStatus.AlternativeSelected => "Selected",
                    _ => "Missing"
                }
            };

            _ = listItem.SubItems.Add(fileItem.StatusText);
            _ = listItem.SubItems.Add(fileItem.SelectedPath);

            // Color code the row based on status
            if (fileItem.Status == FileStatus.Missing)
            {
                listItem.ForeColor = Color.Red;
            }
            else if (fileItem.Status == FileStatus.MissingWithAlternatives)
            {
                listItem.ForeColor = Color.DarkOrange;
            }
            else if (fileItem.Status == FileStatus.AlternativeSelected)
            {
                listItem.ForeColor = Color.Blue;
            }

            _ = listViewFiles.Items.Add(listItem);
        }

        listViewFiles.EndUpdate();
    }

    /// <summary>
    /// Updates the summary label and control states.
    /// </summary>
    private void UpdateSummary ()
    {
        var validCount = _fileItems.Values.Count(f => f.IsAccessible);
        var totalCount = _fileItems.Count;
        var missingCount = totalCount - validCount;

        labelSummary.Text = $"Found: {validCount} of {totalCount} files ({missingCount} missing)";

        // Enable "Load and Update Session" only if user has selected alternatives
        var hasSelectedAlternatives = _fileItems.Values.Any(f => f.Status == FileStatus.AlternativeSelected);
        buttonLoadAndUpdate.Enabled = hasSelectedAlternatives;

        // Update button text based on selection
        if (hasSelectedAlternatives)
        {
            var alternativeCount = _fileItems.Values.Count(f => f.Status == FileStatus.AlternativeSelected);
            buttonLoadAndUpdate.Text = $"Load && Update Session ({alternativeCount})";
        }
        else
        {
            buttonLoadAndUpdate.Text = "Load && Update Session";
        }
    }

    /// <summary>
    /// Opens a file browser dialog for the specified missing file.
    /// </summary>
    /// <param name="fileItem">The file item to browse for</param>
    private void BrowseForFile (MissingFileItem fileItem)
    {
        using var openFileDialog = new OpenFileDialog
        {
            Title = $"Locate: {fileItem.DisplayName}",
            Filter = "Log Files (*.log;*.txt)|*.log;*.txt|All Files (*.*)|*.*",
            FileName = fileItem.DisplayName,
            CheckFileExists = true,
            Multiselect = false
        };

        // Try to set initial directory from original path
        try
        {
            var directory = Path.GetDirectoryName(fileItem.OriginalPath);
            if (!string.IsNullOrEmpty(directory) && Directory.Exists(directory))
            {
                openFileDialog.InitialDirectory = directory;
            }
        }
        catch
        {
            // Ignore if path is invalid
        }

        if (openFileDialog.ShowDialog(this) == DialogResult.OK)
        {
            // User selected a file
            fileItem.SelectedPath = openFileDialog.FileName;
            fileItem.Status = FileStatus.AlternativeSelected;

            // Store the alternative
            SelectedAlternatives[fileItem.OriginalPath] = fileItem.SelectedPath;

            // Refresh the ListView
            PopulateListView();
            UpdateSummary();
        }
    }

    #endregion

    #region Event Handlers

    private void OnListViewSelectedIndexChanged (object sender, EventArgs e)
    {
        if (listViewFiles.SelectedItems.Count > 0)
        {
            var selectedItem = listViewFiles.SelectedItems[0];
            var fileItem = selectedItem.Tag as MissingFileItem;

            // Enable browse button for any file that is not valid (allow browsing/re-browsing)
            buttonBrowse.Enabled = fileItem?.Status is
                FileStatus.Missing or
                FileStatus.MissingWithAlternatives or
                FileStatus.AlternativeSelected;
        }
        else
        {
            buttonBrowse.Enabled = false;
        }
    }

    private void OnListViewDoubleClick (object sender, EventArgs e)
    {
        // Double-click to browse for missing file
        if (listViewFiles.SelectedItems.Count > 0)
        {
            var selectedItem = listViewFiles.SelectedItems[0];
            var fileItem = selectedItem.Tag as MissingFileItem;

            if (fileItem?.Status is
                FileStatus.Missing or
                FileStatus.MissingWithAlternatives or
                FileStatus.AlternativeSelected)
            {
                BrowseForFile(fileItem);
            }
        }
    }

    private void OnButtonBrowseClick (object sender, EventArgs e)
    {
        if (listViewFiles.SelectedItems.Count > 0)
        {
            var selectedItem = listViewFiles.SelectedItems[0];

            if (selectedItem.Tag is MissingFileItem fileItem)
            {
                BrowseForFile(fileItem);
            }
        }
    }

    private void OnButtonLoadClick (object sender, EventArgs e)
    {
        Result = MissingFilesDialogResult.LoadValidFiles;
        DialogResult = DialogResult.OK;
        Close();
    }

    private void OnButtonLoadAndUpdateClick (object sender, EventArgs e)
    {
        Result = MissingFilesDialogResult.LoadAndUpdateSession;
        DialogResult = DialogResult.OK;
        Close();
    }

    private void OnButtonCancelClick (object sender, EventArgs e)
    {
        Result = MissingFilesDialogResult.Cancel;
        DialogResult = DialogResult.Cancel;
        Close();
    }

    #endregion
}
