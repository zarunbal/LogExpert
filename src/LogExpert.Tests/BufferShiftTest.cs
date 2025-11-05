using System.Text;

using LogExpert.Core.Classes.Log;
using LogExpert.Core.Entities;
using LogExpert.PluginRegistry.FileSystem;

using NUnit.Framework;

namespace LogExpert.Tests;

[TestFixture]
internal class BufferShiftTest : RolloverHandlerTestBase
{
    [TearDown]
    public void TearDown ()
    {
        Cleanup();
    }

    [OneTimeSetUp]
    public void Boot ()
    {
        Cleanup();
    }


    [Test]
    public void TestShiftBuffers1 ()
    {
        var linesPerFile = 10;
        MultiFileOptions options = new()
        {
            MaxDayTry = 0,
            FormatPattern = "*$J(.)"
        };

        var files = CreateTestFilesWithoutDate();

        EncodingOptions encodingOptions = new()
        {
            Encoding = Encoding.Default
        };

        PluginRegistry.PluginRegistry.Instance.Create(TestDirectory.FullName, 500);
        LogfileReader reader = new(files.Last.Value, encodingOptions, true, 40, 50, options, false, PluginRegistry.PluginRegistry.Instance);
        reader.ReadFiles();

        var lil = reader.GetLogFileInfoList();
        Assert.That(lil.Count, Is.EqualTo(files.Count));

        var enumerator = files.GetEnumerator();
        enumerator.MoveNext();

        foreach (var li in lil.Cast<LogFileInfo>())
        {
            var fileName = enumerator.Current;
            Assert.That(li.FullName, Is.EqualTo(fileName));
            enumerator.MoveNext();
        }

        var oldCount = lil.Count;

        // Simulate rollover
        //
        files = RolloverSimulation(files, "*$J(.)", false);

        // Simulate rollover detection
        //
        reader.ShiftBuffers();

        lil = reader.GetLogFileInfoList();

        Assert.That(lil.Count, Is.EqualTo(oldCount + 1));

        Assert.That(reader.LineCount, Is.EqualTo(linesPerFile * lil.Count));

        // Check if rollover'd file names have been handled by LogfileReader
        //
        Assert.That(lil.Count, Is.EqualTo(files.Count));
        enumerator = files.GetEnumerator();
        enumerator.MoveNext();

        foreach (LogFileInfo li in lil)
        {
            var fileName = enumerator.Current;
            Assert.That(li.FullName, Is.EqualTo(fileName));
            enumerator.MoveNext();
        }

        // Check if file buffers have correct files. Assuming here that one buffer fits for a
        // complete file
        //
        enumerator = files.GetEnumerator();
        enumerator.MoveNext();

        var logBuffers = reader.GetBufferList();
        var startLine = 0;

        foreach (var logBuffer in logBuffers)
        {
            Assert.That(enumerator.Current, Is.EqualTo(logBuffer.FileInfo.FullName));
            Assert.That(logBuffer.StartLine, Is.EqualTo(startLine));
            startLine += 10;
            enumerator.MoveNext();
        }

        // Checking file content
        //
        enumerator = files.GetEnumerator();
        enumerator.MoveNext();
        enumerator.MoveNext(); // move to 2nd entry. The first file now contains 2nd file's content (because rollover)
        logBuffers = reader.GetBufferList();
        int i;

        for (i = 0; i < logBuffers.Count - 2; ++i)
        {
            var logBuffer = logBuffers[i];
            var line = logBuffer.GetLineOfBlock(0);
            Assert.That(line.FullLine.Contains(enumerator.Current, StringComparison.Ordinal));
            enumerator.MoveNext();
        }

        enumerator.MoveNext();
        // the last 2 files now contain the content of the previously watched file
        for (; i < logBuffers.Count; ++i)
        {
            var logBuffer = logBuffers[i];
            var line = logBuffer.GetLineOfBlock(0);
            Assert.That(line.FullLine.Contains(enumerator.Current, StringComparison.Ordinal));
        }

        oldCount = lil.Count;

        // Simulate rollover again - now latest file will be deleted (simulates logger's rollover history limit)
        //
        files = RolloverSimulation(files, "*$J(.)", true);

        // Simulate rollover detection
        //
        reader.ShiftBuffers();
        lil = reader.GetLogFileInfoList();

        Assert.That(lil.Count, Is.EqualTo(oldCount)); // same count because oldest file is deleted
        Assert.That(lil.Count, Is.EqualTo(files.Count));
        Assert.That(reader.LineCount, Is.EqualTo(linesPerFile * lil.Count));

        // Check first line to see if buffers are correct
        //
        var firstLine = reader.GetLogLine(0);
        var names = new string[files.Count];
        files.CopyTo(names, 0);
        Assert.That(firstLine.FullLine.Contains(names[2], StringComparison.Ordinal));
    }
}