using ColumnizerLib;

using LogExpert.Core.Callback;
using LogExpert.Core.Interfaces;
using LogExpert.UI.Controls.LogWindow;

using Moq;

using NUnit.Framework;

namespace LogExpert.Tests.Controls;

[TestFixture]
public class ColumnCacheTests
{
    /// <summary>
    /// Regression test for a cache-poisoning bug in <see cref="ColumnCache.GetColumnsForLine"/>:
    /// when the underlying <see cref="ILogfileReader.GetLogLineMemoryWithWait"/> returned null
    /// (e.g. fast-fail timeout) for a given line, the cache stored that null and
    /// permanently returned null for every subsequent request of the same line number.
    /// This manifested in the GUI as a blank data row in a CSV file containing a header
    /// plus exactly one data line (header was dropped by CsvColumnizer's PreProcessLine,
    /// leaving a single grid row that was repeatedly requested for paint).
    /// The fix adds <c>_cachedColumns == null</c> to the re-fetch condition so a null
    /// result is never cached.
    /// </summary>
    [Test]
    public void GetColumnsForLine_NullThenValidLine_ReturnsColumnsOnSecondCall ()
    {
        const int lineNumber = 0;

        var validLine = new Mock<ILogLineMemory>().Object;
        var splitResult = new Mock<IColumnizedLogLineMemory>().Object;

        var readerMock = new Mock<ILogfileReader>();
        readerMock
            .SetupSequence(r => r.GetLogLineMemoryWithWait(lineNumber))
            .Returns(Task.FromResult<ILogLineMemory>(null))
            .Returns(Task.FromResult(validLine));

        var columnizerMock = new Mock<ILogLineMemoryColumnizer>();
        columnizerMock
            .Setup(c => c.SplitLine(It.IsAny<ILogLineMemoryColumnizerCallback>(), validLine))
            .Returns(splitResult);

        var logWindowMock = new Mock<ILogWindow>();
        var callback = new ColumnizerCallback(logWindowMock.Object);

        var cache = new ColumnCache();

        // First call: reader returns null -> cache must NOT store this null.
        var firstResult = cache.GetColumnsForLine(readerMock.Object, lineNumber, columnizerMock.Object, callback);
        Assert.That(firstResult, Is.Null, "First call should return null because the reader returned null.");

        // Second call for the SAME line: reader now returns a valid line.
        // Before the fix this would return the cached null and never call SplitLine.
        var secondResult = cache.GetColumnsForLine(readerMock.Object, lineNumber, columnizerMock.Object, callback);

        Assert.That(secondResult, Is.SameAs(splitResult), "Second call must re-fetch and return the freshly split columns instead of a cached null.");
        readerMock.Verify(r => r.GetLogLineMemoryWithWait(lineNumber), Times.Exactly(2));
        columnizerMock.Verify(c => c.SplitLine(It.IsAny<ILogLineMemoryColumnizerCallback>(), validLine), Times.Once);
    }

    /// <summary>
    /// Sanity check that a valid result IS cached: requesting the same line twice
    /// with a successful first fetch must not call the reader/columnizer a second time.
    /// </summary>
    [Test]
    public void GetColumnsForLine_SameLineTwice_UsesCachedValue ()
    {
        const int lineNumber = 0;

        var validLine = new Mock<ILogLineMemory>().Object;
        var splitResult = new Mock<IColumnizedLogLineMemory>().Object;

        var readerMock = new Mock<ILogfileReader>();
        readerMock
            .Setup(r => r.GetLogLineMemoryWithWait(lineNumber))
            .Returns(Task.FromResult(validLine));

        var columnizerMock = new Mock<ILogLineMemoryColumnizer>();
        columnizerMock
            .Setup(c => c.SplitLine(It.IsAny<ILogLineMemoryColumnizerCallback>(), validLine))
            .Returns(splitResult);

        var logWindowMock = new Mock<ILogWindow>();
        var callback = new ColumnizerCallback(logWindowMock.Object);

        var cache = new ColumnCache();

        var firstResult = cache.GetColumnsForLine(readerMock.Object, lineNumber, columnizerMock.Object, callback);
        // callback.LineNum is now equal to lineNumber (set by GetColumnsForLine), so the
        // second call should hit the cache.
        var secondResult = cache.GetColumnsForLine(readerMock.Object, lineNumber, columnizerMock.Object, callback);

        Assert.That(firstResult, Is.SameAs(splitResult));
        Assert.That(secondResult, Is.SameAs(splitResult));
        readerMock.Verify(r => r.GetLogLineMemoryWithWait(lineNumber), Times.Once);
        columnizerMock.Verify(c => c.SplitLine(It.IsAny<ILogLineMemoryColumnizerCallback>(), validLine), Times.Once);
    }
}
