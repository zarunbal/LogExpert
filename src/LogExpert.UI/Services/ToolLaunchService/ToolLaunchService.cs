using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.Versioning;

using LogExpert.Core.Classes;
using LogExpert.Core.Classes.Columnizer;
using LogExpert.Core.Interfaces;
using LogExpert.UI.Interface;

using NLog;

namespace LogExpert.UI.Services.ToolLaunchService;

[SupportedOSPlatform("windows")]
internal sealed class ToolLaunchService (IPluginRegistry pluginRegistry) : IToolLaunchService
{
    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

    public ToolLaunchResult Launch (ToolLaunchRequest request)
    {
        if (string.IsNullOrEmpty(request.Cmd))
        {
            return new ToolLaunchResult
            {
                HasError = true,
                ErrorMessage = "Command must not be empty."
            };
        }

        if (request.SysoutPipe)
        {
            return LaunchWithSysoutPipe(request);
        }

        return LaunchExternal(request);
    }

    private static ToolLaunchResult LaunchExternal (ToolLaunchRequest request)
    {
        var startInfo = BuildStartInfo(request);
        startInfo.UseShellExecute = false;
        Process process = new() { StartInfo = startInfo, EnableRaisingEvents = true };

        try
        {
            _ = process.Start();
        }
        catch (Exception e) when (e is Win32Exception or
                                       InvalidOperationException or
                                       ObjectDisposedException or
                                       PlatformNotSupportedException)
        {
            _logger.Error(e);
            return new ToolLaunchResult { HasError = true, ErrorMessage = e.Message };
        }

        return new ToolLaunchResult { HasError = false };
    }

    private ToolLaunchResult LaunchWithSysoutPipe (ToolLaunchRequest request)
    {
        var columnizer = string.IsNullOrEmpty(request.ColumnizerName)
            ? null
            : ColumnizerPicker.DecideMemoryColumnizerByName(request.ColumnizerName, pluginRegistry.RegisteredColumnizers);

        var startInfo = BuildStartInfo(request);
        startInfo.UseShellExecute = false;
        startInfo.RedirectStandardOutput = true;

        Process process = new() { StartInfo = startInfo, EnableRaisingEvents = true };

        try
        {
            _ = process.Start();
        }
        catch (Exception e) when (e is Win32Exception or
                                       InvalidOperationException or
                                       ObjectDisposedException or
                                       PlatformNotSupportedException)
        {
            _logger.Error(e);
            return new ToolLaunchResult { HasError = true, ErrorMessage = e.Message };
        }

        // TODO: SysoutPipe temp file is never deleted — fire-and-forget lifetime by design.
        SysoutPipe pipe = new(process.StandardOutput);
        process.Exited += pipe.ProcessExitedEventHandler;

        return new ToolLaunchResult
        {
            HasError = false,
            PipeFileName = pipe.FileName,
            Columnizer = columnizer
        };
    }

    private static ProcessStartInfo BuildStartInfo (ToolLaunchRequest request)
    {
        var startInfo = new ProcessStartInfo(request.Cmd, request.Args);
        if (!string.IsNullOrEmpty(request.WorkingDir))
        {
            startInfo.WorkingDirectory = request.WorkingDir;
        }

        return startInfo;
    }
}
