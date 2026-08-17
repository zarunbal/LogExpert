using System.Text;

using LogExpert.Core.Classes.Log;
using LogExpert.Core.Entities;
using LogExpert.Core.Enums;
using LogExpert.Core.Interfaces;

using NUnit.Framework;

namespace LogExpert.Tests.Buffers;

[TestFixture]
internal sealed class LogfileReaderSingleFileMonitoringTests
{
    private string _testDirectory = null!;
    private string _logFile = null!;

    [SetUp]
    public void SetUp ()
    {
        _testDirectory = Path.Combine(Path.GetTempPath(), "LogExpertTests", Guid.NewGuid().ToString());
        _ = Directory.CreateDirectory(_testDirectory);
        _logFile = Path.Combine(_testDirectory, "app.log");
        File.WriteAllLines(_logFile, Enumerable.Range(1, 100).Select(index => $"Line {index}"), Encoding.UTF8);

        _ = PluginRegistry.PluginRegistry.Create(_testDirectory, 500);
    }

    [TearDown]
    public void TearDown ()
    {
        if (Directory.Exists(_testDirectory))
        {
            Directory.Delete(_testDirectory, recursive: true);
        }
    }

    [Test]
    public void SingleFileReader_OnTruncation_ReportsNewFileNotRollover ()
    {
        using var reporter = new RecordingProgressReporter();
        using var reader = new LogfileReader(
            _logFile,
            new EncodingOptions { Encoding = Encoding.UTF8 },
            multiFile: false,
            bufferCount: 40,
            linesPerBuffer: 50,
            new MultiFileOptions(),
            ReaderType.System,
            PluginRegistry.PluginRegistry.Instance,
            maximumLineLength: 500,
            progressReporter: reporter);

        var rolloverReported = false;
        reader.FileSizeChanged += (_, args) => rolloverReported |= args.IsRollover;

        reader.StartMonitoring();
        Assert.That(reporter.LoadingFinished.Wait(TimeSpan.FromSeconds(5)), Is.True, "Initial load did not finish");

        File.WriteAllText(_logFile, "replacement\n", Encoding.UTF8);

        Assert.That(reporter.NewFileReported.Wait(TimeSpan.FromSeconds(5)), Is.True, "Truncation did not report a new file");
        Assert.That(rolloverReported, Is.False);
    }

    private sealed class RecordingProgressReporter : ILoadProgressReporter
    {
        public ManualResetEventSlim LoadingFinished { get; } = new(false);
        public ManualResetEventSlim NewFileReported { get; } = new(false);

        public void ReportProgress (string fileName, long position, long fileLength)
        {
        }

        public void ReportComplete (string fileName, long position, long fileLength)
        {
        }

        public void ReportNewFile (string fileName, long position, long fileLength)
        {
            NewFileReported.Set();
        }

        public void ReportLoadingStarted (string fileName)
        {
        }

        public void ReportLoadingFinished ()
        {
            LoadingFinished.Set();
        }

        public void Dispose ()
        {
            LoadingFinished.Dispose();
            NewFileReported.Dispose();
        }
    }
}
