using System.Diagnostics;
using System.Globalization;
using System.Runtime.Versioning;

// Expanded with some helpers from: https://code.msdn.microsoft.com/windowsapps/How-to-know-the-process-704839f4/
// Uses Windows Restart Manager.
// A more involved and cross platform solution to this problem is here: https://github.com/cklutz/LockCheck

namespace LogExpert.UI.Extensions;

internal static class LockFinder
{

    /// <summary>
    /// Method <c>FindLockedProcessName</c> Retrieve the first process name
    /// that is locking the file at the specified path
    /// </summary>
    /// <param name="path">The path of a file with a write lock held by a
    /// process</param>
    /// <resturns>The name of the first process found with a lock</resturns>
    /// <exception cref="Exception">
    /// Thrown when the file path is not locked
    /// </exception>
    [SupportedOSPlatform("windows")]
    public static string FindLockedProcessName (string path)
    {
        var list = FindLockProcesses(path);
        return list.Count == 0
            ? throw new LockFinderException(Resources.Lockfinder_Exception_NoProcessesAreLockingThePathSpecified)
            : list[0].ProcessName;
    }

    /// <summary>
    /// Method <c>CheckIfFileIsLocked</c> Check if the file specified has a
    /// write lock held by a process
    /// </summary>
    /// <param name="path">The path of a file being checked if a write lock
    /// held by a process</param>
    /// <returns>true when one or more processes with lock</returns>
    [SupportedOSPlatform("windows")]
    public static bool CheckIfFileIsLocked (string path)
    {
        var list = FindLockProcesses(path);
        return list.Count > 0;
    }

    /// <summary>
    /// Used to find processes holding a lock on the file. This would cause
    /// other usage, such as file truncation or write operations to throw
    /// IOException if an exclusive lock is attempted.
    /// </summary>
    /// <param name="path">Path being checked</param>
    /// <returns>List of processes holding file lock to path</returns>
    /// <exception cref="Exception"></exception>
    [SupportedOSPlatform("windows")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Constants always Upper Case")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Intentionally Catch All")]
    public static List<Process> FindLockProcesses (string path)
    {
        var key = new System.Text.StringBuilder(Guid.NewGuid().ToString());
        var processes = new List<Process>();

        var res = Vanara.PInvoke.RstrtMgr.RmStartSession(out var handle, 0, key);
        if (res != 0)
        {
            throw new LockFinderException(Resources.Lockfinder_Exception_CouldNotBeginRestartSessionUnableToDetermineFileLocker);
        }

        try
        {
            uint pnProcInfo = 0;
            string[] resources = [path];

            res = Vanara.PInvoke.RstrtMgr.RmRegisterResources(handle, (uint)resources.Length, resources, 0, null, 0, null);
            if (res != 0)
            {
                throw new LockFinderException(Resources.Lockfinder_Exception_CouldNotRegisterResource);
            }

            res = Vanara.PInvoke.RstrtMgr.RmGetList(handle, out var pnProcInfoNeeded, ref pnProcInfo, null, out Vanara.PInvoke.RstrtMgr.RM_REBOOT_REASON rebootReason);

            const int ERROR_MORE_DATA = 234;
            if (res == ERROR_MORE_DATA)
            {
                var processInfo = new Vanara.PInvoke.RstrtMgr.RM_PROCESS_INFO[pnProcInfoNeeded];
                pnProcInfo = pnProcInfoNeeded;
                // Get the list.
                res = Vanara.PInvoke.RstrtMgr.RmGetList(handle, out pnProcInfoNeeded, ref pnProcInfo, processInfo, out rebootReason);
                if (res == 0)
                {
                    processes = new List<Process>((int)pnProcInfo);
                    for (var i = 0; i < pnProcInfo; i++)
                    {
                        try
                        {
                            processes.Add(Process.GetProcessById((int)processInfo[i].Process.dwProcessId));
                        }
                        catch (ArgumentException) { }
                    }
                }
                else
                {
                    throw new LockFinderException(Resources.Lockfinder_Exception_CouldNotListProcessesLockingResource);
                }
            }
            else if (res != 0)
            {
                throw new LockFinderException(Resources.Lockfinder_Exception_CouldNotListProcessesLockingResourceFailedToGetSizeOfResult);
            }
        }
        catch (Exception e)
        {
            Trace.WriteLine(e.Message);
        }
        finally
        {
            Trace.WriteLine(string.Format(CultureInfo.InvariantCulture, Resources.Lockfinder_Trace_RmEndSessionNativeMethodsRmEndSessionHandle, Vanara.PInvoke.RstrtMgr.RmEndSession(handle)));
        }

        return processes;
    }
}