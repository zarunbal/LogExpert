using LogExpert.Core.Classes.Bookmark;
using LogExpert.Core.Classes.Highlight;
using LogExpert.Core.Entities;

using NUnit.Framework;

namespace LogExpert.Tests;

[TestFixture]
public class HighlightBookmarkTriggerTests
{
    #region GetHighlightActions Tests

    [Test]
    public void GetHighlightActions_WhenIsSetBookmarkTrue_ReturnsSetBookmarkTrue ()
    {
        // Arrange
        var entry = new HighlightEntry
        {
            SearchText = "ERROR",
            IsSetBookmark = true,
            BookmarkComment = "Error found"
        };
        IList<HighlightEntry> matchingList = [entry];

        // Act — GetHighlightActions is a private static method in LogWindow.
        // We replicate its logic here to test the contract.
        var (_, _, setBookmark, bookmarkComment) = ExtractHighlightActions(matchingList);

        // Assert
        Assert.That(setBookmark, Is.True);
        Assert.That(bookmarkComment, Is.EqualTo("Error found"));
    }

    [Test]
    public void GetHighlightActions_WhenIsSetBookmarkFalse_ReturnsSetBookmarkFalse ()
    {
        // Arrange
        var entry = new HighlightEntry
        {
            SearchText = "INFO",
            IsSetBookmark = false
        };
        IList<HighlightEntry> matchingList = [entry];

        // Act
        var (_, _, setBookmark, _) = ExtractHighlightActions(matchingList);

        // Assert
        Assert.That(setBookmark, Is.False);
    }

    [Test]
    public void GetHighlightActions_WhenMultipleEntriesWithBookmarks_ConcatenatesComments ()
    {
        // Arrange
        var entry1 = new HighlightEntry
        {
            SearchText = "ERROR",
            IsSetBookmark = true,
            BookmarkComment = "First"
        };

        var entry2 = new HighlightEntry
        {
            SearchText = "WARN",
            IsSetBookmark = true,
            BookmarkComment = "Second"
        };

        IList<HighlightEntry> matchingList = [entry1, entry2];

        // Act
        var (_, _, setBookmark, bookmarkComment) = ExtractHighlightActions(matchingList);

        // Assert
        Assert.That(setBookmark, Is.True);
        Assert.That(bookmarkComment, Is.EqualTo("First\r\nSecond"));
    }

    [Test]
    public void GetHighlightActions_WhenEmptyList_ReturnsAllFalse ()
    {
        // Arrange
        IList<HighlightEntry> matchingList = [];

        // Act
        var (noLed, stopTail, setBookmark, bookmarkComment) = ExtractHighlightActions(matchingList);

        // Assert
        Assert.That(noLed, Is.False);
        Assert.That(stopTail, Is.False);
        Assert.That(setBookmark, Is.False);
        Assert.That(bookmarkComment, Is.Empty);
    }

    [Test]
    public void GetHighlightActions_WhenBookmarkCommentIsEmpty_ReturnsSetBookmarkTrueWithEmptyComment ()
    {
        // Arrange
        var entry = new HighlightEntry
        {
            SearchText = "ERROR",
            IsSetBookmark = true,
            BookmarkComment = string.Empty
        };
        IList<HighlightEntry> matchingList = [entry];

        // Act
        var (_, _, setBookmark, bookmarkComment) = ExtractHighlightActions(matchingList);

        // Assert
        Assert.That(setBookmark, Is.True);
        Assert.That(bookmarkComment, Is.Empty);
    }

    [Test]
    public void GetHighlightActions_WhenBookmarkCommentIsNull_ReturnsSetBookmarkTrueWithEmptyComment ()
    {
        // Arrange
        var entry = new HighlightEntry
        {
            SearchText = "ERROR",
            IsSetBookmark = true,
            BookmarkComment = null
        };
        IList<HighlightEntry> matchingList = [entry];

        // Act
        var (_, _, setBookmark, bookmarkComment) = ExtractHighlightActions(matchingList);

        // Assert
        Assert.That(setBookmark, Is.True);
        Assert.That(bookmarkComment, Is.Empty);
    }

    /// <summary>
    /// Replicates the logic from LogWindow.GetHighlightActions (private static).
    /// This must stay in sync with the actual implementation.
    /// If the implementation is refactored to be testable directly, remove this helper.
    /// </summary>
    private static (bool NoLed, bool StopTail, bool SetBookmark, string BookmarkComment) ExtractHighlightActions (IList<HighlightEntry> matchingList)
    {
        var noLed = false;
        var stopTail = false;
        var setBookmark = false;
        var bookmarkComment = string.Empty;

        foreach (var entry in matchingList)
        {
            if (entry.IsLedSwitch)
            {
                noLed = true;
            }

            if (entry.IsSetBookmark)
            {
                setBookmark = true;
                if (!string.IsNullOrEmpty(entry.BookmarkComment))
                {
                    bookmarkComment += entry.BookmarkComment + "\r\n";
                }
            }

            if (entry.IsStopTail)
            {
                stopTail = true;
            }
        }

        bookmarkComment = bookmarkComment.TrimEnd(['\r', '\n']);

        return (noLed, stopTail, setBookmark, bookmarkComment);
    }

    #endregion

    #region BookmarkDataProvider Tests

    [Test]
    public void BookmarkDataProvider_AddBookmark_IsBookmarkAtLineReturnsTrue ()
    {
        // Arrange
        var provider = new BookmarkDataProvider();
        var bookmark = new Bookmark(42, "Test comment");

        // Act
        provider.AddBookmark(bookmark);

        // Assert
        Assert.That(provider.IsBookmarkAtLine(42), Is.True);
    }

    [Test]
    public void BookmarkDataProvider_AddBookmark_GetBookmarkReturnsCorrectBookmark ()
    {
        // Arrange
        var provider = new BookmarkDataProvider();
        var bookmark = new Bookmark(42, "Test comment");

        // Act
        provider.AddBookmark(bookmark);
        var result = provider.GetBookmarkForLine(42);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.LineNum, Is.EqualTo(42));
        Assert.That(result.Text, Is.EqualTo("Test comment"));
    }

    [Test]
    public void BookmarkDataProvider_RemoveBookmark_IsBookmarkAtLineReturnsFalse ()
    {
        // Arrange
        var provider = new BookmarkDataProvider();
        provider.AddBookmark(new Bookmark(42, "Test"));

        // Act
        provider.RemoveBookmarkForLine(42);

        // Assert
        Assert.That(provider.IsBookmarkAtLine(42), Is.False);
    }

    [Test]
    public void BookmarkDataProvider_AddBookmark_FiresBookmarkAddedEvent ()
    {
        // Arrange
        var provider = new BookmarkDataProvider();
        var eventFired = false;
        provider.BookmarkAdded += (_, _) => eventFired = true;

        // Act
        provider.AddBookmark(new Bookmark(42));

        // Assert
        Assert.That(eventFired, Is.True);
    }

    #endregion

    #region HighlightEntry Serialization Tests

    [Test]
    public void HighlightEntry_Clone_PreservesIsSetBookmark ()
    {
        // Arrange
        var entry = new HighlightEntry
        {
            SearchText = "ERROR",
            IsSetBookmark = true,
            BookmarkComment = "Critical error"
        };

        // Act
        var clone = entry.Clone();

        // Assert
        Assert.That(((HighlightEntry)clone).IsSetBookmark, Is.True);
        Assert.That(((HighlightEntry)clone).BookmarkComment, Is.EqualTo("Critical error"));
    }

    [Test]
    public void HighlightEntry_Clone_PreservesIsSetBookmarkWhenFalse ()
    {
        // Arrange
        var entry = new HighlightEntry
        {
            SearchText = "INFO",
            IsSetBookmark = false,
            BookmarkComment = string.Empty
        };

        // Act
        var clone = entry.Clone();

        // Assert
        Assert.That(((HighlightEntry)clone).IsSetBookmark, Is.False);
    }

    #endregion

    #region Closure Regression Tests

    /// <summary>
    /// Demonstrates the closure-over-loop-variable bug pattern.
    /// This test proves the bug exists when loop variables are captured directly.
    /// If this test fails in the future, the C# language has changed loop variable capture semantics for `for` loops.
    /// </summary>
    [Test]
    public void ClosureBug_ForLoopVariable_CapturedByReference_DemonstratesBug ()
    {
        // Arrange
        var capturedValues = new List<int>();
        var tasks = new List<Task>();

        // Act — simulate the buggy pattern
        for (var i = 0; i < 5; i++)
        {
            // -- BUG PATTERN: capturing `i` directly
            tasks.Add(Task.Run(() =>
            {
                lock (capturedValues)
                {
                    capturedValues.Add(i);
                }
            }));
        }

        Task.WaitAll([.. tasks]);

        // Assert — at least one value should be wrong (likely all are 5)
        // The key observation: NOT all values 0..4 are present
        var hasAllExpected = capturedValues.Order().SequenceEqual([0, 1, 2, 3, 4]);
        Assert.That(hasAllExpected, Is.False,
            "If this fails, the closure-over-loop-variable issue no longer applies to `for` loops in this C# version.");
    }

    /// <summary>
    /// Demonstrates the correct pattern — capturing the loop variable in a local.
    /// This is the pattern that must be applied in CheckFilterAndHighlight().
    /// </summary>
    [Test]
    public void ClosureFix_LocalCapture_AllValuesCorrect ()
    {
        // Arrange
        var capturedValues = new List<int>();
        var tasks = new List<Task>();

        // Act — correct pattern: capture in local variable
        for (var i = 0; i < 5; i++)
        {
            var captured = i; // FIX: local capture
            tasks.Add(Task.Run(() =>
            {
                lock (capturedValues)
                {
                    capturedValues.Add(captured);
                }
            }));
        }

        Task.WaitAll([.. tasks]);

        // Assert — all values 0..4 must be present
        capturedValues.Sort();
        Assert.That(capturedValues, Is.EquivalentTo([0, 1, 2, 3, 4]));
    }

    #endregion
}