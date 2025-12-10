using System.Text.RegularExpressions;

using LogExpert.Core.Helpers;

using NUnit.Framework;

namespace LogExpert.Tests.Helpers;

[TestFixture]
public class RegexHelperTests
{
    [SetUp]
    public void Setup ()
    {
        // Clear cache before each test to ensure isolation
        RegexHelper.ClearCache();
    }

    [Test]
    public void CreateSafeRegex_ShouldHaveDefaultTimeout ()
    {
        // Arrange & Act
        var regex = RegexHelper.CreateSafeRegex("test");

        // Assert
        Assert.That(regex.MatchTimeout, Is.EqualTo(RegexHelper.DefaultTimeout));
    }

    [Test]
    public void CreateSafeRegex_WithCustomTimeout_ShouldUseCustomTimeout ()
    {
        // Arrange
        var customTimeout = TimeSpan.FromSeconds(5);

        // Act
        var regex = RegexHelper.CreateSafeRegex("test", RegexOptions.None, customTimeout);

        // Assert
        Assert.That(regex.MatchTimeout, Is.EqualTo(customTimeout));
    }

    [Test]
    public void CreateSafeRegex_WithNullPattern_ShouldThrowArgumentNullException ()
    {
        // Act & Assert
        _ = Assert.Throws<ArgumentNullException>(() => RegexHelper.CreateSafeRegex(null!));
    }

    [Test]
    public void CreateSafeRegex_ShouldPreventCatastrophicBacktracking ()
    {
        // Arrange - Use a more aggressive pattern that will reliably timeout
        // This pattern causes exponential backtracking on non-matching input
        var maliciousPattern = @"(a*)*b";
        var maliciousInput = new string('a', 50); // 50 'a's with no 'b' at the end
        var shortTimeout = TimeSpan.FromMilliseconds(100);
        var regex = RegexHelper.CreateSafeRegex(maliciousPattern, RegexOptions.None, shortTimeout);

        // Act & Assert
        _ = Assert.Throws<RegexMatchTimeoutException>(() => _ = regex.IsMatch(maliciousInput));
    }

    [Test]
    public void CreateSafeRegex_WithComplexPattern_ShouldTimeout ()
    {
        // Arrange - Another catastrophic backtracking pattern
        // This pattern exhibits exponential behavior with nested quantifiers
        var pattern = @"(a+)+b";
        var input = new string('a', 50) + "c"; // Many 'a's but ends with 'c' instead of 'b'
        var shortTimeout = TimeSpan.FromMilliseconds(100);
        var regex = RegexHelper.CreateSafeRegex(pattern, RegexOptions.None, shortTimeout);

        // Act & Assert
        _ = Assert.Throws<RegexMatchTimeoutException>(() => _ = regex.IsMatch(input));
    }

    [Test]
    public void GetOrCreateCached_ShouldReturnSameInstance ()
    {
        // Arrange & Act
        var regex1 = RegexHelper.GetOrCreateCached("test");
        var regex2 = RegexHelper.GetOrCreateCached("test");

        // Assert
        Assert.That(regex1, Is.SameAs(regex2));
    }

    [Test]
    public void GetOrCreateCached_WithDifferentPatterns_ShouldReturnDifferentInstances ()
    {
        // Arrange & Act
        var regex1 = RegexHelper.GetOrCreateCached("test1");
        var regex2 = RegexHelper.GetOrCreateCached("test2");

        // Assert
        Assert.That(regex1, Is.Not.SameAs(regex2));
    }

    [Test]
    public void GetOrCreateCached_WithDifferentOptions_ShouldReturnDifferentInstances ()
    {
        // Arrange & Act
        var regex1 = RegexHelper.GetOrCreateCached("test", RegexOptions.None);
        var regex2 = RegexHelper.GetOrCreateCached("test", RegexOptions.IgnoreCase);

        // Assert
        Assert.That(regex1, Is.Not.SameAs(regex2));
    }

    [Test]
    public void GetOrCreateCached_ShouldCacheUpToMaxSize ()
    {
        // Arrange - Create more patterns than cache size
        var cacheSize = 100;

        // Act - Fill the cache
        for (int i = 0; i < cacheSize; i++)
        {
            _ = RegexHelper.GetOrCreateCached($"pattern{i}");
        }

        // Assert - Cache should be at max size
        Assert.That(RegexHelper.CacheSize, Is.EqualTo(cacheSize));

        // Act - Add more to trigger eviction
        _ = RegexHelper.GetOrCreateCached("pattern_overflow");

        // Assert - Cache should have evicted some entries
        Assert.That(RegexHelper.CacheSize, Is.LessThanOrEqualTo(cacheSize));
    }

    [Test]
    public void IsValidPattern_WithValidPattern_ShouldReturnTrue ()
    {
        // Arrange
        var pattern = @"\d{4}-\d{2}-\d{2}";

        // Act
        var (isValid, error) = RegexHelper.IsValidPattern(pattern);

        // Assert
        Assert.That(isValid, Is.True);
        Assert.That(error, Is.Null);
    }

    [Test]
    public void IsValidPattern_WithInvalidPattern_ShouldReturnFalse ()
    {
        // Arrange
        var pattern = "[invalid";

        // Act
        var (isValid, error) = RegexHelper.IsValidPattern(pattern);

        // Assert
        Assert.That(isValid, Is.False);
        Assert.That(error, Is.Not.Null);
        Assert.That(error, Does.Contain("Invalid pattern").Or.Contain("parsing").Or.Contain("Unterminated"));
    }

    [Test]
    public void IsValidPattern_WithNullPattern_ShouldReturnFalse ()
    {
        // Act
        var (isValid, error) = RegexHelper.IsValidPattern(null!);

        // Assert
        Assert.That(isValid, Is.False);
        Assert.That(error, Is.Not.Null);
    }

    [Test]
    public void IsValidPattern_WithEmptyPattern_ShouldReturnFalse ()
    {
        // Act
        var (isValid, error) = RegexHelper.IsValidPattern(string.Empty);

        // Assert
        Assert.That(isValid, Is.False);
        Assert.That(error, Is.Not.Null);
    }

    [Test]
    public void ClearCache_ShouldRemoveAllCachedRegex ()
    {
        // Arrange
        _ = RegexHelper.GetOrCreateCached("test1");
        _ = RegexHelper.GetOrCreateCached("test2");
        _ = RegexHelper.GetOrCreateCached("test3");
        Assert.That(RegexHelper.CacheSize, Is.GreaterThan(0));

        // Act
        RegexHelper.ClearCache();

        // Assert
        Assert.That(RegexHelper.CacheSize, Is.EqualTo(0));
    }

    [Test]
    public void CachedRegex_ShouldWorkCorrectly ()
    {
        // Arrange
        var pattern = @"(\d{4})-(\d{2})-(\d{2})";
        var input = "Date: 2025-11-11";
        var regex = RegexHelper.GetOrCreateCached(pattern);

        // Act
        var match = regex.Match(input);

        // Assert
        Assert.That(match.Success, Is.True);
        Assert.That(match.Groups[1].Value, Is.EqualTo("2025"));
        Assert.That(match.Groups[2].Value, Is.EqualTo("11"));
        Assert.That(match.Groups[3].Value, Is.EqualTo("11"));
    }

    [Test]
    public void CachedRegex_WithIgnoreCase_ShouldMatchCaseInsensitively ()
    {
        // Arrange
        var pattern = "test";
        var regex = RegexHelper.GetOrCreateCached(pattern, RegexOptions.IgnoreCase);

        // Act
        var match1 = regex.IsMatch("TEST");
        var match2 = regex.IsMatch("Test");
        var match3 = regex.IsMatch("test");

        // Assert
        Assert.That(match1, Is.True);
        Assert.That(match2, Is.True);
        Assert.That(match3, Is.True);
    }

    [Test]
    public void CachedRegex_ShouldHaveTimeout ()
    {
        // Arrange & Act
        var regex = RegexHelper.GetOrCreateCached("test");

        // Assert
        Assert.That(regex.MatchTimeout, Is.EqualTo(RegexHelper.DefaultTimeout));
    }

    [Test]
    public void DefaultTimeout_ShouldBeTwoSeconds ()
    {
        // Assert
        Assert.That(RegexHelper.DefaultTimeout, Is.EqualTo(TimeSpan.FromSeconds(2)));
    }

    [TestCase(@"\d+", "123", true)]
    [TestCase(@"\d+", "abc", false)]
    [TestCase(@"[A-Z]+", "ABC", true)]
    [TestCase(@"[A-Z]+", "abc", false)]
    [TestCase(@"^\w+@\w+\.\w+$", "test@example.com", true)]
    [TestCase(@"^\w+@\w+\.\w+$", "invalid-email", false)]
    public void CreateSafeRegex_CommonPatterns_ShouldWorkCorrectly (string pattern, string input, bool expectedMatch)
    {
        // Arrange
        var regex = RegexHelper.CreateSafeRegex(pattern);

        // Act
        var result = regex.IsMatch(input);

        // Assert
        Assert.That(result, Is.EqualTo(expectedMatch));
    }
}
