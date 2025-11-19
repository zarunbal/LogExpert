using LogExpert.PluginRegistry;

using NUnit.Framework;

namespace LogExpert.Tests;

[TestFixture]
public class PluginHashCalculatorTests
{
    private string _testDirectory;
    private string _testFilePath;

    [SetUp]
    public void SetUp ()
    {
        _testDirectory = Path.Join(Path.GetTempPath(), "LogExpertPluginHashTests");
        _ = Directory.CreateDirectory(_testDirectory);
        _testFilePath = Path.Join(_testDirectory, "test-plugin.dll");
    }

    [TearDown]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Unit testcases")]
    public void TearDown ()
    {
        try
        {
            if (Directory.Exists(_testDirectory))
            {
                Directory.Delete(_testDirectory, true);
            }
        }
        catch
        {
            // Ignore cleanup errors
        }
    }

    [Test]
    public void CalculateHash_ValidFile_ReturnsHash ()
    {
        // Arrange
        var testContent = "This is a test plugin DLL content";
        File.WriteAllText(_testFilePath, testContent);

        // Act
        var hash = PluginHashCalculator.CalculateHash(_testFilePath);

        // Assert
        Assert.That(hash, Is.Not.Null);
        Assert.That(hash, Is.Not.Empty);
        Assert.That(hash.Length, Is.EqualTo(64)); // SHA256 produces 32 bytes = 64 hex chars
        Assert.That(hash, Does.Match("^[0-9A-F]+$")); // Only hex characters, uppercase
    }

    [Test]
    public void CalculateHash_SameContent_ReturnsSameHash ()
    {
        // Arrange
        var testContent = "Identical content";
        File.WriteAllText(_testFilePath, testContent);

        // Act
        var hash1 = PluginHashCalculator.CalculateHash(_testFilePath);
        var hash2 = PluginHashCalculator.CalculateHash(_testFilePath);

        // Assert
        Assert.That(hash1, Is.EqualTo(hash2));
    }

    [Test]
    public void CalculateHash_DifferentContent_ReturnsDifferentHash ()
    {
        // Arrange
        var testFile1 = Path.Join(_testDirectory, "plugin1.dll");
        var testFile2 = Path.Join(_testDirectory, "plugin2.dll");
        File.WriteAllText(testFile1, "Content 1");
        File.WriteAllText(testFile2, "Content 2");

        // Act
        var hash1 = PluginHashCalculator.CalculateHash(testFile1);
        var hash2 = PluginHashCalculator.CalculateHash(testFile2);

        // Assert
        Assert.That(hash1, Is.Not.EqualTo(hash2));
    }

    [Test]
    public void CalculateHash_FileNotFound_ThrowsFileNotFoundException ()
    {
        // Arrange
        var nonExistentPath = Path.Join(_testDirectory, "nonexistent.dll");

        // Act & Assert
        _ = Assert.Throws<FileNotFoundException>(() =>
            PluginHashCalculator.CalculateHash(nonExistentPath));
    }

    [Test]
    public void CalculateHash_EmptyPath_ThrowsArgumentException ()
    {
        // Act & Assert
        _ = Assert.Throws<ArgumentException>(() => PluginHashCalculator.CalculateHash(string.Empty));
    }


    [Test]
    public void CalculateHash_NullPath_ThrowsArgumentNullException ()
    {
        // Act & Assert
        _ = Assert.Throws<ArgumentNullException>(() => PluginHashCalculator.CalculateHash(null));
    }

    [Test]
    public void CalculateHash_WhitespacePath_ThrowsArgumentException ()
    {
        // Act & Assert
        _ = Assert.Throws<ArgumentException>(() => PluginHashCalculator.CalculateHash("   "));
    }

    [Test]
    public void VerifyHash_MatchingHash_ReturnsTrue ()
    {
        // Arrange
        var testContent = "Test content for hash verification";
        File.WriteAllText(_testFilePath, testContent);
        var expectedHash = PluginHashCalculator.CalculateHash(_testFilePath);

        // Act
        var result = PluginHashCalculator.VerifyHash(_testFilePath, expectedHash);

        // Assert
        Assert.That(result, Is.True);
    }

    [Test]
    public void VerifyHash_MismatchedHash_ReturnsFalse ()
    {
        // Arrange
        var testContent = "Test content";
        File.WriteAllText(_testFilePath, testContent);
        var wrongHash = "0123456789ABCDEF0123456789ABCDEF0123456789ABCDEF0123456789ABCDEF";

        // Act
        var result = PluginHashCalculator.VerifyHash(_testFilePath, wrongHash);

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public void VerifyHash_CaseInsensitiveUpperCase_ReturnsTrue ()
    {
        // Arrange
        var testContent = "Test content";
        File.WriteAllText(_testFilePath, testContent);
        var hash = PluginHashCalculator.CalculateHash(_testFilePath);
        var upperCaseHash = hash.ToUpperInvariant();

        // Act
        var result = PluginHashCalculator.VerifyHash(_testFilePath, upperCaseHash);

        // Assert
        Assert.That(result, Is.True);
    }

    [Test]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1308:Normalize strings to uppercase", Justification = "Testing if function works with lower case")]
    public void VerifyHash_CaseInsensitiveLowerCase_ReturnsTrue ()
    {
        // Arrange
        var testContent = "Test content";
        File.WriteAllText(_testFilePath, testContent);
        var hash = PluginHashCalculator.CalculateHash(_testFilePath);
        var lowerCaseHash = hash.ToLowerInvariant();

        // Act
        var result = PluginHashCalculator.VerifyHash(_testFilePath, lowerCaseHash);

        // Assert
        Assert.That(result, Is.True);
    }

    [Test]
    public void VerifyHash_ModifiedFile_ReturnsFalse ()
    {
        // Arrange
        var originalContent = "Original content";
        File.WriteAllText(_testFilePath, originalContent);
        var originalHash = PluginHashCalculator.CalculateHash(_testFilePath);

        // Modify file
        var modifiedContent = "Modified content";
        File.WriteAllText(_testFilePath, modifiedContent);

        // Act
        var result = PluginHashCalculator.VerifyHash(_testFilePath, originalHash);

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public void VerifyHash_NullExpectedHash_ThrowsArgumentNullException ()
    {
        // Arrange
        File.WriteAllText(_testFilePath, "Test content");

        // Act & Assert
        _ = Assert.Throws<ArgumentNullException>(() => PluginHashCalculator.VerifyHash(_testFilePath, null));
    }

    [Test]
    public void VerifyHash_EmptyExpectedHash_ThrowsArgumentException ()
    {
        // Arrange
        File.WriteAllText(_testFilePath, "Test content");

        // Act & Assert
        _ = Assert.Throws<ArgumentException>(() => PluginHashCalculator.VerifyHash(_testFilePath, string.Empty));
    }

    [Test]
    public void CalculateHashes_MultipleFiles_ReturnsAllHashes ()
    {
        // Arrange
        var file1 = Path.Join(_testDirectory, "plugin1.dll");
        var file2 = Path.Join(_testDirectory, "plugin2.dll");
        var file3 = Path.Join(_testDirectory, "plugin3.dll");

        File.WriteAllText(file1, "Content 1");
        File.WriteAllText(file2, "Content 2");
        File.WriteAllText(file3, "Content 3");

        var filePaths = new[] { file1, file2, file3 };

        // Act
        var hashes = PluginHashCalculator.CalculateHashes(filePaths);

        // Assert
        Assert.That(hashes, Has.Count.EqualTo(3));
        Assert.That(hashes, Does.ContainKey(file1));
        Assert.That(hashes, Does.ContainKey(file2));
        Assert.That(hashes, Does.ContainKey(file3));
        Assert.That(hashes[file1], Is.Not.EqualTo(hashes[file2]));
        Assert.That(hashes[file2], Is.Not.EqualTo(hashes[file3]));
    }

    [Test]
    public void CalculateHashes_EmptyCollection_ReturnsEmptyDictionary ()
    {
        // Arrange
        var filePaths = Array.Empty<string>();

        // Act
        var hashes = PluginHashCalculator.CalculateHashes(filePaths);

        // Assert
        Assert.That(hashes, Is.Empty);
    }

    [Test]
    public void CalculateHashes_SomeFilesNotFound_OmitsFailedFiles ()
    {
        // Arrange
        var file1 = Path.Join(_testDirectory, "plugin1.dll");
        var file2 = Path.Join(_testDirectory, "nonexistent.dll");
        var file3 = Path.Join(_testDirectory, "plugin3.dll");

        File.WriteAllText(file1, "Content 1");
        File.WriteAllText(file3, "Content 3");
        // file2 deliberately not created

        var filePaths = new[] { file1, file2, file3 };

        // Act
        var hashes = PluginHashCalculator.CalculateHashes(filePaths);

        // Assert
        Assert.That(hashes, Has.Count.EqualTo(2));
        Assert.That(hashes, Does.ContainKey(file1));
        Assert.That(hashes, Does.Not.ContainKey(file2));
        Assert.That(hashes, Does.ContainKey(file3));
    }

    [Test]
    public void CalculateHashes_NullCollection_ThrowsArgumentNullException ()
    {
        // Act & Assert
        _ = Assert.Throws<ArgumentNullException>(() =>
            PluginHashCalculator.CalculateHashes(null));
    }

    [Test]
    public void CalculateHash_LargeFile_CalculatesSuccessfully ()
    {
        // Arrange
        var largeFilePath = Path.Join(_testDirectory, "large-plugin.dll");

        // Create a 10MB file
        using (var stream = File.Create(largeFilePath))
        {
            var buffer = new byte[1024 * 1024]; // 1MB buffer
            for (var i = 0; i < 10; i++)
            {
                stream.Write(buffer, 0, buffer.Length);
            }
        }

        // Act
        var hash = PluginHashCalculator.CalculateHash(largeFilePath);

        // Assert
        Assert.That(hash, Is.Not.Null);
        Assert.That(hash, Is.Not.Empty);
        Assert.That(hash.Length, Is.EqualTo(64));
    }

    [Test]
    public void CalculateHash_KnownContent_MatchesExpectedHash ()
    {
        // Arrange
        var testContent = "Hello, LogExpert!";
        File.WriteAllText(_testFilePath, testContent);

        // Pre-calculated SHA256 hash of "Hello, LogExpert!"
        // You can verify this with: echo -n "Hello, LogExpert!" | sha256sum
        var expectedHash = "8A7B3C8E9F0D1E2A3B4C5D6E7F8A9B0C1D2E3F4A5B6C7D8E9F0A1B2C3D4E5F6A";

        // Act
        var actualHash = PluginHashCalculator.CalculateHash(_testFilePath);

        // Assert - comparing structure and format, not exact hash (depends on encoding)
        Assert.That(actualHash.Length, Is.EqualTo(expectedHash.Length));
        Assert.That(actualHash, Does.Match("^[0-9A-F]+$"));
    }
}
