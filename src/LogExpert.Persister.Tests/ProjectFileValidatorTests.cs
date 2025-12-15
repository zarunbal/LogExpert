using System.Globalization;

using LogExpert.Core.Classes.Persister;

namespace LogExpert.Persister.Tests;

/// <summary>
/// Unit tests for the Project File Validator implementation (Issue #514).
/// Tests validation logic for missing files in project/session loading.
/// </summary>
[TestFixture]
public class ProjectFileValidatorTests
{
    private string _testDirectory;
    private string _projectFile;
    private List<string> _testLogFiles;

    [SetUp]
    public void Setup ()
    {
        // Create temporary test directory
        _testDirectory = Path.Join(Path.GetTempPath(), "LogExpertTests", "ProjectValidator", Guid.NewGuid().ToString());
        _ = Directory.CreateDirectory(_testDirectory);

        // Initialize test log files list
        _testLogFiles = [];

        // Create a project file path (will be created in individual tests)
        _projectFile = Path.Join(_testDirectory, "test_project.lxj");

        // Initialize PluginRegistry for tests
        _ = PluginRegistry.PluginRegistry.Create(_testDirectory, 1000);
    }

    [TearDown]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Unit Test")]
    public void TearDown ()
    {
        // Clean up test directory
        if (Directory.Exists(_testDirectory))
        {
            try
            {
                Directory.Delete(_testDirectory, true);
            }
            catch
            {
                // Ignore cleanup errors
            }
        }
    }

    #region Helper Methods

    /// <summary>
    /// Creates test log files with specified names.
    /// </summary>
    private void CreateTestLogFiles (params string[] fileNames)
    {
        foreach (var fileName in fileNames)
        {
            var filePath = Path.Join(_testDirectory, fileName);
            File.WriteAllText(filePath, $"Test log content for {fileName}");
            _testLogFiles.Add(filePath);
        }
    }

    /// <summary>
    /// Creates a test project file with specified log file references.
    /// </summary>
    private void CreateTestProjectFile (params string[] logFileNames)
    {
        var projectData = new ProjectData
        {
            FileNames = [.. logFileNames.Select(name => Path.Join(_testDirectory, name))],
            TabLayoutXml = "<layout><dockpanel>test</dockpanel></layout>"
        };

        ProjectPersister.SaveProjectData(_projectFile, projectData);
    }

    /// <summary>
    /// Deletes specified log files to simulate missing files.
    /// </summary>
    private void DeleteLogFiles (params string[] fileNames)
    {
        foreach (var fileName in fileNames)
        {
            var filePath = Path.Join(_testDirectory, fileName);
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }
    }

    #endregion

    #region ProjectLoadResult Tests

    [Test]
    public void ProjectLoadResult_HasValidFiles_AllFilesValid_ReturnsTrue ()
    {
        // Arrange
        var projectData = new ProjectData();
        var validationResult = new ProjectValidationResult();
        validationResult.ValidFiles.Add("file1.log");
        validationResult.ValidFiles.Add("file2.log");

        var result = new ProjectLoadResult
        {
            ProjectData = projectData,
            ValidationResult = validationResult
        };

        // Act
        var hasValidFiles = result.HasValidFiles;

        // Assert
        Assert.That(hasValidFiles, Is.True, "Should have valid files");
    }

    [Test]
    public void ProjectLoadResult_HasValidFiles_NoValidFiles_ReturnsFalse ()
    {
        // Arrange
        var projectData = new ProjectData();
        var validationResult = new ProjectValidationResult();
        validationResult.MissingFiles.Add("file1.log");
        validationResult.MissingFiles.Add("file2.log");

        var result = new ProjectLoadResult
        {
            ProjectData = projectData,
            ValidationResult = validationResult
        };

        // Act
        var hasValidFiles = result.HasValidFiles;

        // Assert
        Assert.That(hasValidFiles, Is.False, "Should not have valid files");
    }

    [Test]
    public void ProjectLoadResult_HasValidFiles_SomeValidFiles_ReturnsTrue ()
    {
        // Arrange
        var projectData = new ProjectData();
        var validationResult = new ProjectValidationResult();
        validationResult.ValidFiles.Add("file1.log");
        validationResult.MissingFiles.Add("file2.log");
        validationResult.MissingFiles.Add("file3.log");

        var result = new ProjectLoadResult
        {
            ProjectData = projectData,
            ValidationResult = validationResult
        };

        // Act
        var hasValidFiles = result.HasValidFiles;

        // Assert
        Assert.That(hasValidFiles, Is.True, "Should have at least one valid file");
    }

    [Test]
    public void ProjectLoadResult_RequiresUserIntervention_AllFilesValid_ReturnsFalse ()
    {
        // Arrange
        var projectData = new ProjectData();
        var validationResult = new ProjectValidationResult();
        validationResult.ValidFiles.Add("file1.log");
        validationResult.ValidFiles.Add("file2.log");

        var result = new ProjectLoadResult
        {
            ProjectData = projectData,
            ValidationResult = validationResult
        };

        // Act
        var requiresIntervention = result.RequiresUserIntervention;

        // Assert
        Assert.That(requiresIntervention, Is.False, "Should not require user intervention");
    }

    [Test]
    public void ProjectLoadResult_RequiresUserIntervention_SomeMissingFiles_ReturnsTrue ()
    {
        // Arrange
        var projectData = new ProjectData();
        var validationResult = new ProjectValidationResult();
        validationResult.ValidFiles.Add("file1.log");
        validationResult.MissingFiles.Add("file2.log");

        var result = new ProjectLoadResult
        {
            ProjectData = projectData,
            ValidationResult = validationResult
        };

        // Act
        var requiresIntervention = result.RequiresUserIntervention;

        // Assert
        Assert.That(requiresIntervention, Is.True, "Should require user intervention");
    }

    #endregion

    #region ProjectValidationResult Tests

    [Test]
    public void ProjectValidationResult_HasMissingFiles_WithMissingFiles_ReturnsTrue ()
    {
        // Arrange
        var result = new ProjectValidationResult();
        result.ValidFiles.Add("file1.log");
        result.MissingFiles.Add("file2.log");

        // Act
        var hasMissing = result.HasMissingFiles;

        // Assert
        Assert.That(hasMissing, Is.True, "Should have missing files");
    }

    [Test]
    public void ProjectValidationResult_HasMissingFiles_WithoutMissingFiles_ReturnsFalse ()
    {
        // Arrange
        var result = new ProjectValidationResult();
        result.ValidFiles.Add("file1.log");
        result.ValidFiles.Add("file2.log");

        // Act
        var hasMissing = result.HasMissingFiles;

        // Assert
        Assert.That(hasMissing, Is.False, "Should not have missing files");
    }

    #endregion

    #region ProjectPersister.LoadProjectData - All Files Valid

    [Test]
    public void LoadProjectData_AllFilesExist_ReturnsSuccessResult ()
    {
        // Arrange
        CreateTestLogFiles("log1.log", "log2.log", "log3.log");
        CreateTestProjectFile("log1.log", "log2.log", "log3.log");

        // Act
        var result = ProjectPersister.LoadProjectData(_projectFile, PluginRegistry.PluginRegistry.Instance);

        // Assert
        Assert.That(result, Is.Not.Null, "Result should not be null");
        Assert.That(result.ProjectData, Is.Not.Null, "ProjectData should not be null");
        Assert.That(result.ValidationResult, Is.Not.Null, "ValidationResult should not be null");
        Assert.That(result.HasValidFiles, Is.True, "Should have valid files");
        Assert.That(result.RequiresUserIntervention, Is.False, "Should not require intervention");
        Assert.That(result.ValidationResult.ValidFiles.Count, Is.EqualTo(3), "Should have 3 valid files");
        Assert.That(result.ValidationResult.MissingFiles.Count, Is.EqualTo(0), "Should have 0 missing files");
    }

    [Test]
    public void LoadProjectData_AllFilesExist_ProjectDataContainsCorrectFiles ()
    {
        // Arrange
        CreateTestLogFiles("alpha.log", "beta.log", "gamma.log");
        CreateTestProjectFile("alpha.log", "beta.log", "gamma.log");

        // Act
        var result = ProjectPersister.LoadProjectData(_projectFile, PluginRegistry.PluginRegistry.Instance);

        // Assert
        var fileNames = result.ProjectData.FileNames.Select(Path.GetFileName).ToList();
        Assert.That(fileNames, Does.Contain("alpha.log"), "Should contain alpha.log");
        Assert.That(fileNames, Does.Contain("beta.log"), "Should contain beta.log");
        Assert.That(fileNames, Does.Contain("gamma.log"), "Should contain gamma.log");
    }

    [Test]
    public void LoadProjectData_AllFilesExist_PreservesTabLayoutXml ()
    {
        // Arrange
        CreateTestLogFiles("test.log");
        CreateTestProjectFile("test.log");

        // Act
        var result = ProjectPersister.LoadProjectData(_projectFile, PluginRegistry.PluginRegistry.Instance);

        // Assert
        Assert.That(result.ProjectData.TabLayoutXml, Is.Not.Null.And.Not.Empty, "TabLayoutXml should be preserved");
        Assert.That(result.ProjectData.TabLayoutXml, Does.Contain("<layout>"), "Should contain layout XML");
    }

    #endregion

    #region ProjectPersister.LoadProjectData - Some Files Missing

    [Test]
    public void LoadProjectData_SomeFilesMissing_ReturnsPartialSuccessResult ()
    {
        // Arrange
        CreateTestLogFiles("exists1.log", "exists2.log", "missing.log");
        DeleteLogFiles("missing.log"); // Delete to simulate missing
        CreateTestProjectFile("exists1.log", "exists2.log", "missing.log");

        // Act
        var result = ProjectPersister.LoadProjectData(_projectFile, PluginRegistry.PluginRegistry.Instance);

        // Assert
        Assert.That(result, Is.Not.Null, "Result should not be null");
        Assert.That(result.HasValidFiles, Is.True, "Should have some valid files");
        Assert.That(result.RequiresUserIntervention, Is.True, "Should require user intervention");
        Assert.That(result.ValidationResult.ValidFiles.Count, Is.EqualTo(2), "Should have 2 valid files");
        Assert.That(result.ValidationResult.MissingFiles.Count, Is.EqualTo(1), "Should have 1 missing file");
    }

    [Test]
    public void LoadProjectData_SomeFilesMissing_ValidFilesListIsCorrect ()
    {
        // Arrange
        CreateTestLogFiles("valid1.log", "valid2.log", "invalid.log");
        DeleteLogFiles("invalid.log");
        CreateTestProjectFile("valid1.log", "valid2.log", "invalid.log");

        // Act
        var result = ProjectPersister.LoadProjectData(_projectFile, PluginRegistry.PluginRegistry.Instance);

        // Assert
        var validFileNames = result.ValidationResult.ValidFiles.Select(Path.GetFileName).ToList();
        Assert.That(validFileNames, Does.Contain("valid1.log"), "Should contain valid1.log");
        Assert.That(validFileNames, Does.Contain("valid2.log"), "Should contain valid2.log");
        Assert.That(validFileNames, Does.Not.Contain("invalid.log"), "Should not contain invalid.log");
    }

    [Test]
    public void LoadProjectData_SomeFilesMissing_MissingFilesListIsCorrect ()
    {
        // Arrange
        CreateTestLogFiles("present.log", "absent1.log", "absent2.log");
        DeleteLogFiles("absent1.log", "absent2.log");
        CreateTestProjectFile("present.log", "absent1.log", "absent2.log");

        // Act
        var result = ProjectPersister.LoadProjectData(_projectFile, PluginRegistry.PluginRegistry.Instance);

        // Assert
        var missingFileNames = result.ValidationResult.MissingFiles.Select(Path.GetFileName).ToList();
        Assert.That(missingFileNames, Does.Contain("absent1.log"), "Should contain absent1.log");
        Assert.That(missingFileNames, Does.Contain("absent2.log"), "Should contain absent2.log");
        Assert.That(missingFileNames, Does.Not.Contain("present.log"), "Should not contain present.log");
    }

    [Test]
    public void LoadProjectData_MajorityFilesMissing_StillReturnsValidFiles ()
    {
        // Arrange
        CreateTestLogFiles("only_valid.log", "missing1.log", "missing2.log", "missing3.log", "missing4.log");
        DeleteLogFiles("missing1.log", "missing2.log", "missing3.log", "missing4.log");
        CreateTestProjectFile("only_valid.log", "missing1.log", "missing2.log", "missing3.log", "missing4.log");

        // Act
        var result = ProjectPersister.LoadProjectData(_projectFile, PluginRegistry.PluginRegistry.Instance);

        // Assert
        Assert.That(result.HasValidFiles, Is.True, "Should have at least one valid file");
        Assert.That(result.ValidationResult.ValidFiles.Count, Is.EqualTo(1), "Should have 1 valid file");
        Assert.That(result.ValidationResult.MissingFiles.Count, Is.EqualTo(4), "Should have 4 missing files");
    }

    #endregion

    #region ProjectPersister.LoadProjectData - All Files Missing

    [Test]
    public void LoadProjectData_AllFilesMissing_ReturnsFailureResult ()
    {
        // Arrange
        CreateTestLogFiles("missing1.log", "missing2.log");
        DeleteLogFiles("missing1.log", "missing2.log");
        CreateTestProjectFile("missing1.log", "missing2.log");

        // Act
        var result = ProjectPersister.LoadProjectData(_projectFile, PluginRegistry.PluginRegistry.Instance);

        // Assert
        Assert.That(result, Is.Not.Null, "Result should not be null");
        Assert.That(result.HasValidFiles, Is.False, "Should not have valid files");
        Assert.That(result.ValidationResult.ValidFiles.Count, Is.EqualTo(0), "Should have 0 valid files");
        Assert.That(result.ValidationResult.MissingFiles.Count, Is.EqualTo(2), "Should have 2 missing files");
    }

    [Test]
    public void LoadProjectData_AllFilesMissing_MissingFilesListComplete ()
    {
        // Arrange
        CreateTestLogFiles("gone1.log", "gone2.log", "gone3.log");
        DeleteLogFiles("gone1.log", "gone2.log", "gone3.log");
        CreateTestProjectFile("gone1.log", "gone2.log", "gone3.log");

        // Act
        var result = ProjectPersister.LoadProjectData(_projectFile, PluginRegistry.PluginRegistry.Instance);

        // Assert
        Assert.That(result.ValidationResult.MissingFiles.Count, Is.EqualTo(3), "Should have 3 missing files");
        var missingFileNames = result.ValidationResult.MissingFiles.Select(Path.GetFileName).ToList();
        Assert.That(missingFileNames, Does.Contain("gone1.log"));
        Assert.That(missingFileNames, Does.Contain("gone2.log"));
        Assert.That(missingFileNames, Does.Contain("gone3.log"));
    }

    #endregion

    #region ProjectPersister.LoadProjectData - Empty/Invalid Projects

    [Test]
    public void LoadProjectData_EmptyProject_ReturnsEmptyResult ()
    {
        // Arrange
        CreateTestProjectFile(); // Empty project with no files

        // Act
        var result = ProjectPersister.LoadProjectData(_projectFile, PluginRegistry.PluginRegistry.Instance);

        // Assert
        Assert.That(result, Is.Not.Null, "Result should not be null");
        Assert.That(result.ProjectData.FileNames, Is.Empty, "FileNames should be empty");
        Assert.That(result.ValidationResult.ValidFiles, Is.Empty, "ValidFiles should be empty");
        Assert.That(result.ValidationResult.MissingFiles, Is.Empty, "MissingFiles should be empty");
    }

    [Test]
    public void LoadProjectData_NonExistentProjectFile_ReturnsNull ()
    {
        // Arrange
        var nonExistentProject = Path.Join(_testDirectory, "does_not_exist.lxj");

        // Act
        var result = ProjectPersister.LoadProjectData(nonExistentProject, PluginRegistry.PluginRegistry.Instance);

        // Assert
        Assert.That(result, Is.Null, "Result should be null for non-existent project file");
    }

    [Test]
    public void LoadProjectData_CorruptedProjectFile_ReturnsNull ()
    {
        // Arrange
        var corruptedProject = Path.Join(_testDirectory, "corrupted.lxj");
        File.WriteAllText(corruptedProject, "This is not valid XML or JSON");

        // Act
        var result = ProjectPersister.LoadProjectData(corruptedProject, PluginRegistry.PluginRegistry.Instance);

        // Assert
        Assert.That(result, Is.Null, "Result should be null for corrupted project file");
    }

    #endregion

    #region Edge Cases and Special Scenarios

    [Test]
    public void LoadProjectData_DuplicateFileReferences_HandlesCorrectly ()
    {
        // Arrange
        CreateTestLogFiles("duplicate.log");
        var projectData = new ProjectData
        {
            FileNames =
            [
                Path.Join(_testDirectory, "duplicate.log"),
                Path.Join(_testDirectory, "duplicate.log"),
                Path.Join(_testDirectory, "duplicate.log")
            ]
        };
        ProjectPersister.SaveProjectData(_projectFile, projectData);

        // Act
        var result = ProjectPersister.LoadProjectData(_projectFile, PluginRegistry.PluginRegistry.Instance);

        // Assert
        Assert.That(result, Is.Not.Null, "Result should not be null");
        Assert.That(result.HasValidFiles, Is.True, "Should have valid files");
        // Validation should handle duplicates gracefully
    }

    [Test]
    public void LoadProjectData_FilesWithSpecialCharacters_ValidatesCorrectly ()
    {
        // Arrange
        CreateTestLogFiles("file with spaces.log", "file-with-dashes.log", "file_with_underscores.log");
        CreateTestProjectFile("file with spaces.log", "file-with-dashes.log", "file_with_underscores.log");

        // Act
        var result = ProjectPersister.LoadProjectData(_projectFile, PluginRegistry.PluginRegistry.Instance);

        // Assert
        Assert.That(result, Is.Not.Null, "Result should not be null");
        Assert.That(result.ValidationResult.ValidFiles.Count, Is.EqualTo(3), "Should validate all files with special characters");
    }

    [Test]
    public void LoadProjectData_VeryLargeProject_ValidatesEfficiently ()
    {
        // Arrange
        const int fileCount = 100;
        var fileNames = new List<string>();

        for (int i = 0; i < fileCount; i++)
        {
            var fileName = $"log_{i:D4}.log";
            fileNames.Add(fileName);
        }

        CreateTestLogFiles([.. fileNames]);
        CreateTestProjectFile([.. fileNames]);

        // Act
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var result = ProjectPersister.LoadProjectData(_projectFile, PluginRegistry.PluginRegistry.Instance);
        stopwatch.Stop();

        // Assert
        Assert.That(result, Is.Not.Null, "Result should not be null");
        Assert.That(result.ValidationResult.ValidFiles.Count, Is.EqualTo(fileCount), $"Should validate all {fileCount} files");
        Assert.That(stopwatch.ElapsedMilliseconds, Is.LessThan(5000), "Should complete validation in reasonable time");
    }

    #endregion

    #region Performance and Stress Tests

    [Test]
    public void LoadProjectData_ManyMissingFiles_PerformsEfficiently ()
    {
        // Arrange
        const int totalFiles = 50;
        var fileNames = new List<string>();

        // Create only first 10 files, rest will be missing
        for (int i = 0; i < 10; i++)
        {
            var fileName = $"exists_{i}.log";
            fileNames.Add(fileName);
            CreateTestLogFiles(fileName);
        }

        for (int i = 10; i < totalFiles; i++)
        {
            fileNames.Add($"missing_{i}.log");
        }

        CreateTestProjectFile([.. fileNames]);

        // Act
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var result = ProjectPersister.LoadProjectData(_projectFile, PluginRegistry.PluginRegistry.Instance);
        stopwatch.Stop();

        // Assert
        Assert.That(result, Is.Not.Null, "Result should not be null");
        Assert.That(result.ValidationResult.ValidFiles.Count, Is.EqualTo(10), "Should have 10 valid files");
        Assert.That(result.ValidationResult.MissingFiles.Count, Is.EqualTo(40), "Should have 40 missing files");
        Assert.That(stopwatch.ElapsedMilliseconds, Is.LessThan(2000), "Should handle many missing files efficiently");
    }

    #endregion

    #region Null and Exception Handling

    [Test]
    public void LoadProjectData_NullProjectFile_ThrowsArgumentNullException ()
    {
        // Act & Assert
        _ = Assert.Throws<ArgumentNullException>(() =>
            ProjectPersister.LoadProjectData(null, PluginRegistry.PluginRegistry.Instance));
    }

    [Test]
    public void LoadProjectData_EmptyProjectFile_ThrowsArgumentException ()
    {
        // Act & Assert
        _ = Assert.Throws<ArgumentException>(() => ProjectPersister.LoadProjectData(string.Empty, PluginRegistry.PluginRegistry.Instance));
    }

    [Test]
    public void LoadProjectData_NullPluginRegistry_ThrowsArgumentNullException ()
    {
        // Arrange
        CreateTestProjectFile("test.log");

        // Act & Assert
        _ = Assert.Throws<ArgumentNullException>(() => ProjectPersister.LoadProjectData(_projectFile, null));
    }

    #endregion

    #region Backward Compatibility

    [Test]
    public void LoadProjectData_LegacyProjectFormat_StillWorks ()
    {
        // Arrange
        CreateTestLogFiles("legacy.log");

        // Create a legacy format project file (XML)
        var legacyXml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<LogExpertProject>
  <Files>
    <member fileName=""{0}"" />
  </Files>
</LogExpertProject>";

        var legacyContent = string.Format(CultureInfo.InvariantCulture, legacyXml, Path.Join(_testDirectory, "legacy.log"));
        File.WriteAllText(_projectFile, legacyContent);

        // Act
        var result = ProjectPersister.LoadProjectData(_projectFile, PluginRegistry.PluginRegistry.Instance);

        // Assert
        Assert.That(result, Is.Not.Null, "Should handle legacy format");
        Assert.That(result.HasValidFiles, Is.True, "Should load legacy files");
    }

    #endregion
}