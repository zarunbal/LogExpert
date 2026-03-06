using ColumnizerLib;

using LogExpert.Core.Classes.Log;
using LogExpert.Core.Entities;
using LogExpert.Core.Interface;

using Moq;

using NUnit.Framework;

namespace LogExpert.Tests;

[TestFixture]
internal class RolloverFilenameHandlerNullTests
{
    /// <summary>
    /// Verifies that GetNameList does not throw when GetLogfileInfo returns null
    /// for rollover file candidates. This simulates the SFTP scenario where
    /// constructing SftpLogFileInfo fails for non-existent files.
    /// </summary>
    [Test]
    public void GetNameList_WhenGetLogfileInfoReturnsNull_DoesNotThrow ()
    {
        // Arrange: Create a mock ILogFileInfo for the "base" file
        var baseFileInfo = new Mock<ILogFileInfo>();
        _ = baseFileInfo.Setup(f => f.FileName).Returns("app.log");
        _ = baseFileInfo.Setup(f => f.DirectoryName).Returns("sftp://host/var/log");
        _ = baseFileInfo.Setup(f => f.DirectorySeparatorChar).Returns('/');
        _ = baseFileInfo.Setup(f => f.FileExists).Returns(true);

        // Arrange: Create a mock IFileSystemPlugin that returns null for rollover files
        var mockFs = new Mock<IFileSystemPlugin>();
        _ = mockFs.Setup(fs => fs.CanHandleUri(It.IsAny<string>())).Returns(true);
        // Return null for any GetLogfileInfo call — simulates constructor failure
        _ = mockFs.Setup(fs => fs.GetLogfileInfo(It.IsAny<string>())).Returns((ILogFileInfo)null);

        // Arrange: Create a mock IPluginRegistry
        var mockRegistry = new Mock<IPluginRegistry>();
        _ = mockRegistry.Setup(r => r.FindFileSystemForUri(It.IsAny<string>())).Returns(mockFs.Object);

        MultiFileOptions options = new()
        {
            FormatPattern = "*$J(.)",
            MaxDayTry = 5
        };

        RolloverFilenameHandler handler = new(baseFileInfo.Object, options);

        // Act & Assert: Should not throw NullReferenceException
        LinkedList<string> result = null;
        Assert.DoesNotThrow(() => result = handler.GetNameList(mockRegistry.Object));

        // The list should contain only the base file
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count, Is.EqualTo(1));
        Assert.That(result.First.Value, Does.Contain("app.log"));
    }

    /// <summary>
    /// Verifies that GetNameList does not throw when FindFileSystemForUri returns null.
    /// This could happen with an unrecognized URI scheme.
    /// </summary>
    [Test]
    public void GetNameList_WhenFindFileSystemReturnsNull_DoesNotThrow ()
    {
        // Arrange
        var baseFileInfo = new Mock<ILogFileInfo>();
        _ = baseFileInfo.Setup(f => f.FileName).Returns("app.log");
        _ = baseFileInfo.Setup(f => f.DirectoryName).Returns("custom://host/logs");
        _ = baseFileInfo.Setup(f => f.DirectorySeparatorChar).Returns('/');
        _ = baseFileInfo.Setup(f => f.FileExists).Returns(true);

        var mockRegistry = new Mock<IPluginRegistry>();
        _ = mockRegistry.Setup(r => r.FindFileSystemForUri(It.IsAny<string>())).Returns((IFileSystemPlugin)null);

        MultiFileOptions options = new()
        {
            FormatPattern = "*$J(.)",
            MaxDayTry = 3
        };

        RolloverFilenameHandler handler = new(baseFileInfo.Object, options);

        // Act & Assert
        LinkedList<string> result = null;
        Assert.DoesNotThrow(() => result = handler.GetNameList(mockRegistry.Object));

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count, Is.EqualTo(1));
    }

    /// <summary>
    /// Verifies that GetNameList correctly finds rollover files when GetLogfileInfo
    /// returns a valid ILogFileInfo with FileExists = true. Ensures the null guard
    /// does not break the normal happy path.
    /// </summary>
    [Test]
    public void GetNameList_WhenRolloverFilesExist_ReturnsAllFiles ()
    {
        // Arrange: base file
        var baseFileInfo = new Mock<ILogFileInfo>();
        _ = baseFileInfo.Setup(f => f.FileName).Returns("app.log");
        _ = baseFileInfo.Setup(f => f.DirectoryName).Returns("/var/log");
        _ = baseFileInfo.Setup(f => f.DirectorySeparatorChar).Returns('/');

        // Arrange: rollover file .log.1 exists, .log.2 does not
        var rollover1Info = new Mock<ILogFileInfo>();
        _ = rollover1Info.Setup(f => f.FileExists).Returns(true);

        var mockFs = new Mock<IFileSystemPlugin>();
        _ = mockFs.Setup(fs => fs.CanHandleUri(It.IsAny<string>())).Returns(true);

        // First call (for .log.1) returns a file that exists
        // Second call (for .log.2) returns null (simulating constructor failure)
        _ = mockFs.SetupSequence(fs => fs.GetLogfileInfo(It.IsAny<string>()))
            .Returns(rollover1Info.Object)
            .Returns((ILogFileInfo)null);

        var mockRegistry = new Mock<IPluginRegistry>();
        _ = mockRegistry.Setup(r => r.FindFileSystemForUri(It.IsAny<string>())).Returns(mockFs.Object);

        MultiFileOptions options = new()
        {
            FormatPattern = "*$J(.)",
            MaxDayTry = 3
        };

        RolloverFilenameHandler handler = new(baseFileInfo.Object, options);

        // Act
        var result = handler.GetNameList(mockRegistry.Object);

        // Assert: base file + 1 rollover file
        Assert.That(result.Count, Is.EqualTo(2));
    }
}