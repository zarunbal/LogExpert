using System.Runtime.Versioning;
using System.Text;

using ColumnizerLib;

using LogExpert.Core.Classes.Filter;
using LogExpert.Core.Classes.Persister;
using LogExpert.Core.Entities;
using LogExpert.Core.Interfaces;
using LogExpert.UI.Controls.LogWindow;
using LogExpert.UI.Interface;

using NLog;

namespace LogExpert.UI.Services.FileOperationService;

[SupportedOSPlatform("windows")]
internal sealed class FileOperationService (
    IConfigManager configManager,
    ITabController tabController,
    ILedIndicatorService ledIndicatorService,
    IPluginRegistry pluginRegistry,
    Func<FileTabRequest, EncodingOptions, LogWindow> logWindowFactory,
    Func<string?> clipboardTextProvider,
    Action<string, bool> projectFileCallback) : IFileOperationService
{
    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

    private readonly IConfigManager _configManager = configManager;
    private readonly ITabController _tabController = tabController;
    private readonly ILedIndicatorService _ledIndicatorService = ledIndicatorService;
    private readonly IPluginRegistry _pluginRegistry = pluginRegistry;
    private readonly Func<FileTabRequest, EncodingOptions, LogWindow> _logWindowFactory = logWindowFactory;
    private readonly Func<string?> _clipboardTextProvider = clipboardTextProvider;
    private readonly Action<string, bool> _projectFileCallback = projectFileCallback;

    public event EventHandler? FileHistoryChanged;
    public event EventHandler<FileOpenedEventArgs>? FileOpened;


    // TODO TabColor was part of older Versions, this is a feature that should be readded, sooner or later, but it is not a priority right now.
    //    var data = logWindow.Tag as LogWindowData;
    //    data.Color = _defaultTabColor;
    //    //TODO SetTabColor and the Coloring must be reimplemented with a different UI Framework
    //    //SetTabColor(logWindow, _defaultTabColor);
    //    //data.tabPage.BorderColor = this.defaultTabBorderColor;
    //    //if (!isTempFile)
    //    //{
    //    //    foreach (var colorEntry in ConfigManager.Settings.FileColors)
    //    //    {
    //    //        if (colorEntry.FileName.ToUpperInvariant().Equals(logFileName.ToUpperInvariant(), StringComparison.Ordinal))
    //    //        {
    //    //            data.Color = colorEntry.Color;
    //    //            //SetTabColor(logWindow, colorEntry.Color);
    //    //            break;
    //    //        }
    //    //    }
    //    //}

    public LogWindow AddFileTab (FileTabRequest request)
    {
        var logFileName = PersisterHelpers.FindFilenameForSettings(request.FileName, _pluginRegistry);
        var normalizedPath = Path.GetFullPath(logFileName);

        var existingWindow = FindWindowForFile(normalizedPath);
        if (existingWindow != null)
        {
            if (!request.IsTempFile)
            {
                AddToFileHistory(request.FileName);
            }

            _tabController.ActivateWindow(existingWindow);
            return existingWindow;
        }

        EncodingOptions encodingOptions = new();
        FillDefaultEncodingFromSettings(encodingOptions);

        if (request.IsTempFile)
        {
            encodingOptions.Encoding = new UnicodeEncoding(false, false);
        }

        var logWindow = _logWindowFactory(request, encodingOptions);

        if (!request.IsTempFile)
        {
            AddToFileHistory(request.FileName);
        }

        if (request.FileName.EndsWith(".lxp", StringComparison.Ordinal))
        {
            logWindow.ForcedPersistenceFileName = request.FileName;
        }

        _ = Task.Run(() => logWindow.LoadFile(logFileName, encodingOptions));

        FileOpened?.Invoke(this, new FileOpenedEventArgs
        {
            LogWindow = logWindow,
            Request = request,
            ResolvedFileName = logFileName,
            EncodingOptions = encodingOptions
        });

        return logWindow;
    }

    public LogWindow? FindWindowForFile (string fileName)
    {
        return _tabController.FindWindowByFileName(fileName);
    }

    public void AddToFileHistory (string fileName)
    {
        _configManager.AddToFileHistory(fileName);
        FileHistoryChanged?.Invoke(this, EventArgs.Empty);
    }

    private void FillDefaultEncodingFromSettings (EncodingOptions encodingOptions)
    {
        if (_configManager.Settings.Preferences.DefaultEncoding != null)
        {
            try
            {
                encodingOptions.DefaultEncoding = Encoding.GetEncoding(_configManager.Settings.Preferences.DefaultEncoding);
            }
            catch (ArgumentException)
            {
                _logger.Warn($"### FillDefaultEncodingFromSettings: Encoding {_configManager.Settings.Preferences.DefaultEncoding} is not a valid encoding");
                encodingOptions.DefaultEncoding = null;
            }
        }
    }

    public LogWindow AddFilterTab (FilterPipe pipe, string title, ILogLineMemoryColumnizer? preProcessColumnizer)
    {
        var request = new FileTabRequest
        {
            FileName = pipe.FileName,
            IsTempFile = true,
            Title = title,
            PreProcessColumnizer = preProcessColumnizer
        };

        var logWindow = AddFileTab(request);

        // The filter-specific tooltip needs the pipe's FilterParams, which is not part of
        // the generic FileTabRequest. We raise FileOpened again with the FilterPipe attached
        // so LogTabWindow can set up the filter tooltip in OnFileOperationServiceFileOpened.
        // Note: AddFileTab already raised FileOpened once (for tab color/tooltip setup).
        // This second raise carries the FilterPipe for the filter-specific tooltip.
        if (pipe.FilterParams.SearchText?.Length > 0)
        {
            FileOpened?.Invoke(this, new FileOpenedEventArgs
            {
                LogWindow = logWindow,
                Request = request,
                ResolvedFileName = pipe.FileName,
                FilterPipe = pipe
            });
        }

        return logWindow;
    }

    public LogWindow AddTempFileTab (string fileName, string title)
    {
        return AddFileTab(new FileTabRequest
        {
            FileName = fileName,
            IsTempFile = true,
            Title = title
        });
    }

    public LogWindow? AddMultiFileTab (string[] fileNames)
    {
        if (fileNames.Length < 1)
        {
            return null;
        }

        EncodingOptions encodingOptions = new();
        FillDefaultEncodingFromSettings(encodingOptions);

        var request = new FileTabRequest
        {
            FileName = fileNames[^1],
            IsTempFile = false
        };

        var logWindow = _logWindowFactory(request, encodingOptions);

        // BeginInvoke for LoadFilesAsMulti must be done by the caller (LogTabWindow)
        // since it requires a Control. The factory already added the window to the DockPanel.
        // We raise FileOpened with MultiFileNames so LogTabWindow can call BeginInvoke(logWindow.LoadFilesAsMulti, ...).
        AddToFileHistory(fileNames[0]);

        FileOpened?.Invoke(this, new FileOpenedEventArgs
        {
            LogWindow = logWindow,
            Request = request,
            ResolvedFileName = fileNames[^1],
            EncodingOptions = encodingOptions,
            MultiFileNames = fileNames
        });

        return logWindow;
    }

    public MultiFileDecision LoadFilesWithOption (string[] fileNames, bool invertLogic)
    {
        Array.Sort(fileNames);

        if (fileNames.Length == 1)
        {
            if (fileNames[0].EndsWith(".lxj", StringComparison.OrdinalIgnoreCase))
            {
                _projectFileCallback(fileNames[0], true);
                return MultiFileDecision.Cancel; // Already handled
            }

            _ = AddFileTab(new FileTabRequest { FileName = fileNames[0] });
            return MultiFileDecision.SingleFiles;
        }

        var option = _configManager.Settings.Preferences.MultiFileOption;

        if (option == Core.Config.MultiFileOption.Ask)
        {
            return MultiFileDecision.AskUser;
        }

        if (invertLogic)
        {
            option = option == Core.Config.MultiFileOption.SingleFiles
                ? Core.Config.MultiFileOption.MultiFile
                : Core.Config.MultiFileOption.SingleFiles;
        }

        if (option == Core.Config.MultiFileOption.SingleFiles)
        {
            AddFileTabs(fileNames);
            return MultiFileDecision.SingleFiles;
        }

        _ = AddMultiFileTab(fileNames);
        return MultiFileDecision.MultiFile;
    }

    public void AddFileTabs (string[] fileNames)
    {
        foreach (var fileName in fileNames.Where(filename => !string.IsNullOrEmpty(filename)))
        {
            if (fileName.EndsWith(".lxj", StringComparison.OrdinalIgnoreCase))
            {
                _projectFileCallback(fileName, false);
            }
            else
            {
                _ = AddFileTab(new FileTabRequest { FileName = fileName });
            }
        }
    }

    public LogWindow? PasteFromClipboard ()
    {
        var text = _clipboardTextProvider();
        if (string.IsNullOrEmpty(text))
        {
            return null;
        }

        var fileName = Path.GetTempFileName();

        using (FileStream fStream = new(fileName, FileMode.Append, FileAccess.Write, FileShare.Read))
        using (StreamWriter writer = new(fStream, Encoding.Unicode))
        {
            writer.Write(text);
            writer.Close();
        }

        var title = "Clipboard"; // LogTabWindow will use the resource string via FileOpened event
        return AddTempFileTab(fileName, title);
    }

    public bool CanHandleDrop (IDataObject data)
    {
        return data?.GetDataPresent(DataFormats.FileDrop) == true;
    }

    public LogWindow AddFileTabDeferred (string fileName, bool isTempFile, string? title, bool forcePersistenceLoading, ILogLineMemoryColumnizer? preProcessColumnizer)
    {
        return AddFileTab(new FileTabRequest
        {
            FileName = fileName,
            IsTempFile = isTempFile,
            Title = title,
            ForcePersistenceLoading = forcePersistenceLoading,
            PreProcessColumnizer = preProcessColumnizer,
            DoNotAddToDockPanel = true
        });
    }

    public void LoadFiles (string[] fileNames)
    {
        AddFileTabs(fileNames);
    }

    public void SaveLastOpenFilesList ()
    {
        foreach (var logWin in _tabController.GetAllWindowsFromDockPanel().Where(logwin => !logwin.IsTempFile))
        {
            _configManager.Settings.LastOpenFilesList.Add(logWin.GivenFileName);
        }
    }

    public void LoadStartupFiles (IList<string> lastOpenFiles, string[]? startupFileNames)
    {
        if (startupFileNames != null && startupFileNames.Length > 0)
        {
            _ = LoadFilesWithOption(startupFileNames, false);
            return;
        }

        if (_configManager.Settings.Preferences.OpenLastFiles)
        {
            foreach (var name in lastOpenFiles.Where(filename => !string.IsNullOrEmpty(filename)))
            {
                _ = AddFileTab(new FileTabRequest { FileName = name });
            }
        }

        _configManager.ClearLastOpenFilesList();
    }
}