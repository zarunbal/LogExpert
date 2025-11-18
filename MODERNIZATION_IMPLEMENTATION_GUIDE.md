# LogExpert Modernization - Implementation Guide

**For Developers:** This guide provides step-by-step instructions for implementing modernization tasks.

**Last Updated:** November 11, 2025

---

## Getting Started

### Prerequisites

1. **Development Environment**
   - Windows 10/11 (required for Windows Forms)
   - Visual Studio 2022 or VS Code with C# extension
   - .NET 9.0.301 SDK (specified in global.json)
   - Git for version control

2. **Recommended Tools**
   - ReSharper or Visual Studio IntelliCode
   - BenchmarkDotNet for performance testing
   - dotMemory for memory profiling
   - dotTrace for performance profiling

3. **Clone and Build**
   ```bash
   git clone https://github.com/LogExperts/LogExpert.git
   cd LogExpert
   ./build.ps1 --target Clean Compile Test
   ```

### Branch Strategy

- **Development:** Main development branch
- **Feature branches:** `feature/[issue-number]-[short-description]`
- **Modernization:** `modernization/[phase]-[task-number]`

**Example:** `modernization/phase1-regex-timeout`

---

## Phase 1 Implementation Guide

### Task 1.1.1: Regex Timeout Protection

**Priority:** P0 - CRITICAL  
**Estimated Time:** 3-5 days

#### Step 1: Audit Current Usage

```bash
# Find all Regex instantiations
grep -r "new Regex(" src/

# Find Regex.IsMatch calls
grep -r "Regex\.IsMatch" src/

# Find Regex.Match calls
grep -r "Regex\.Match" src/
```

**Expected Locations:**
- `src/LogExpert.Core/FilterParams.cs`
- `src/RegexColumnizer/`
- `src/LogExpert.UI/Controls/LogWindow/LogWindow.cs`
- Search functionality
- Highlight functionality

#### Step 2: Create RegexHelper Class

Create: `src/LogExpert.Core/Helpers/RegexHelper.cs`

```csharp
using System;
using System.Collections.Concurrent;
using System.Text.RegularExpressions;

namespace LogExpert.Core.Helpers;

/// <summary>
/// Helper class for creating and managing regex instances with safety features.
/// </summary>
public static class RegexHelper
{
    /// <summary>
    /// Default timeout for all regex operations to prevent DoS attacks.
    /// </summary>
    public static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(2);

    private static readonly ConcurrentDictionary<RegexCacheKey, Regex> _cache = new();
    private const int MaxCacheSize = 100;

    /// <summary>
    /// Creates a regex with timeout protection.
    /// </summary>
    public static Regex CreateSafeRegex(
        string pattern, 
        RegexOptions options = RegexOptions.None,
        TimeSpan? timeout = null)
    {
        ArgumentNullException.ThrowIfNull(pattern);
        
        return new Regex(
            pattern, 
            options, 
            timeout ?? DefaultTimeout);
    }

    /// <summary>
    /// Gets or creates a cached regex instance.
    /// </summary>
    public static Regex GetOrCreateCached(
        string pattern, 
        RegexOptions options = RegexOptions.None)
    {
        var key = new RegexCacheKey(pattern, options);
        
        return _cache.GetOrAdd(key, k =>
        {
            // Evict oldest entries if cache is full
            if (_cache.Count >= MaxCacheSize)
            {
                var toRemove = _cache.Keys.Take(_cache.Count - MaxCacheSize / 2);
                foreach (var oldKey in toRemove)
                    _cache.TryRemove(oldKey, out _);
            }
            
            return CreateSafeRegex(k.Pattern, k.Options);
        });
    }

    /// <summary>
    /// Validates regex pattern without executing it.
    /// </summary>
    public static bool IsValidPattern(string pattern, out string? error)
    {
        try
        {
            _ = new Regex(pattern, RegexOptions.None, TimeSpan.FromMilliseconds(100));
            error = null;
            return true;
        }
        catch (ArgumentException ex)
        {
            error = ex.Message;
            return false;
        }
    }

    private record RegexCacheKey(string Pattern, RegexOptions Options);
}
```

#### Step 3: Update Program.Main()

Edit: `src/LogExpert/Program.cs`

```csharp
[STAThread]
[SupportedOSPlatform("windows")]
private static void Main(string[] args)
{
    // Set global regex timeout as fallback
    AppDomain.CurrentDomain.SetData(
        "REGEX_DEFAULT_MATCH_TIMEOUT", 
        TimeSpan.FromSeconds(2));

    AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
    Application.ThreadException += Application_ThreadException;

    // ... rest of Main()
}
```

#### Step 4: Update FilterParams

Edit: `src/LogExpert.Core/FilterParams.cs`

```csharp
// OLD
public Regex CreateRegex()
{
    if (string.IsNullOrEmpty(SearchText))
        return null;
        
    return new Regex(SearchText, GetRegexOptions());
}

// NEW
public Regex CreateRegex()
{
    if (string.IsNullOrEmpty(SearchText))
        return null;
        
    return RegexHelper.GetOrCreateCached(SearchText, GetRegexOptions());
}
```

#### Step 5: Update Search Functionality

Edit search methods to use cached regex:

```csharp
// OLD
private void SearchInternal(SearchParams searchParams)
{
    var regex = new Regex(searchParams.Pattern, GetRegexOptions());
    // ... search logic
}

// NEW
private void SearchInternal(SearchParams searchParams)
{
    var regex = RegexHelper.GetOrCreateCached(
        searchParams.Pattern, 
        GetRegexOptions());
    // ... search logic
}
```

#### Step 6: Add Unit Tests

Create: `src/LogExpert.Tests/Helpers/RegexHelperTests.cs`

```csharp
using NUnit.Framework;
using LogExpert.Core.Helpers;
using System.Text.RegularExpressions;

namespace LogExpert.Tests.Helpers;

[TestFixture]
public class RegexHelperTests
{
    [Test]
    public void CreateSafeRegex_ShouldHaveTimeout()
    {
        var regex = RegexHelper.CreateSafeRegex("test");
        
        Assert.That(regex.MatchTimeout, Is.EqualTo(RegexHelper.DefaultTimeout));
    }

    [Test]
    public void CreateSafeRegex_ShouldPreventCatastrophicBacktracking()
    {
        var regex = RegexHelper.CreateSafeRegex("^(a+)+$");
        
        Assert.Throws<RegexMatchTimeoutException>(() =>
        {
            regex.IsMatch("aaaaaaaaaaaaaaaaaX");
        });
    }

    [Test]
    public void GetOrCreateCached_ShouldReturnSameInstance()
    {
        var regex1 = RegexHelper.GetOrCreateCached("test");
        var regex2 = RegexHelper.GetOrCreateCached("test");
        
        Assert.That(regex1, Is.SameAs(regex2));
    }

    [Test]
    public void IsValidPattern_ShouldDetectInvalidPattern()
    {
        var isValid = RegexHelper.IsValidPattern("[invalid", out var error);
        
        Assert.That(isValid, Is.False);
        Assert.That(error, Is.Not.Null);
    }
}
```

#### Step 7: Test and Validate

1. Run all unit tests: `./build.ps1 --target Test`
2. Test with known dangerous patterns
3. Verify timeout exceptions are caught gracefully
4. Test performance with cached regex

#### Step 8: Documentation

Update documentation:
- Add XML comments to RegexHelper
- Update user documentation about regex safety
- Add developer guidelines for regex usage

---

### Task 1.1.2: BinaryFormatter Elimination

**Priority:** P0 - CRITICAL  
**Estimated Time:** 1-2 weeks

#### Step 1: Identify BinaryFormatter Usage

```bash
# Find BinaryFormatter usages
grep -r "BinaryFormatter" src/

# Find Serialize/Deserialize calls
grep -r "formatter\.Serialize\|formatter\.Deserialize" src/
```

#### Step 2: Create JSON Serialization Helper

Create: `src/LogExpert.Core/Serialization/JsonConfigSerializer.cs`

```csharp
using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace LogExpert.Core.Serialization;

public static class JsonConfigSerializer
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true,
        AllowTrailingCommas = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters =
        {
            new JsonStringEnumConverter()
        }
    };

    public static T? Deserialize<T>(string json)
    {
        try
        {
            return JsonSerializer.Deserialize<T>(json, Options);
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException(
                $"Failed to deserialize {typeof(T).Name}", ex);
        }
    }

    public static T? DeserializeFromFile<T>(string filePath)
    {
        var json = File.ReadAllText(filePath);
        return Deserialize<T>(json);
    }

    public static string Serialize<T>(T obj)
    {
        return JsonSerializer.Serialize(obj, Options);
    }

    public static void SerializeToFile<T>(T obj, string filePath)
    {
        var json = Serialize(obj);
        File.WriteAllText(filePath, json);
    }
}
```

#### Step 3: Create Migration Utility

Create: `src/LogExpert.Core/Serialization/ConfigMigrationHelper.cs`

```csharp
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.Json;

namespace LogExpert.Core.Serialization;

public static class ConfigMigrationHelper
{
    public static T? MigrateFromBinary<T>(string binaryFilePath)
    {
        if (!File.Exists(binaryFilePath))
            return default;

        try
        {
            // Create backup
            File.Copy(binaryFilePath, $"{binaryFilePath}.backup", overwrite: true);

            // Read old binary format
#pragma warning disable SYSLIB0011 // BinaryFormatter is obsolete
            using var stream = File.OpenRead(binaryFilePath);
            var formatter = new BinaryFormatter();
            var obj = (T)formatter.Deserialize(stream);
#pragma warning restore SYSLIB0011

            // Write new JSON format
            var jsonPath = Path.ChangeExtension(binaryFilePath, ".json");
            JsonConfigSerializer.SerializeToFile(obj, jsonPath);

            return obj;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                "Failed to migrate config from binary to JSON", ex);
        }
    }

    public static bool NeedsMigration(string configPath)
    {
        if (!File.Exists(configPath))
            return false;

        // Check if file is binary format (not JSON)
        try
        {
            var firstChar = (char)File.OpenRead(configPath).ReadByte();
            return firstChar != '{' && firstChar != '[';
        }
        catch
        {
            return false;
        }
    }
}
```

#### Step 4: Update Config Loading

Update configuration loading to try JSON first, then migrate from binary:

```csharp
public T LoadConfig<T>(string configName)
{
    var jsonPath = Path.Combine(_configDir, $"{configName}.json");
    var binaryPath = Path.Combine(_configDir, configName);

    // Try JSON first
    if (File.Exists(jsonPath))
    {
        return JsonConfigSerializer.DeserializeFromFile<T>(jsonPath);
    }

    // Migrate from binary if exists
    if (File.Exists(binaryPath))
    {
        var config = ConfigMigrationHelper.MigrateFromBinary<T>(binaryPath);
        if (config != null)
        {
            // Save as JSON
            JsonConfigSerializer.SerializeToFile(config, jsonPath);
            return config;
        }
    }

    return default;
}
```

#### Step 5: Update All Config Classes

Ensure all config classes are JSON-serializable:

```csharp
public class ColumnizerConfig
{
    // Add parameterless constructor if needed
    public ColumnizerConfig() { }

    // Properties should have getters and setters
    public string Name { get; set; } = string.Empty;
    public List<string> Columns { get; set; } = new();
    
    // Complex types need JsonConverter or be serializable
    [JsonIgnore]
    public Regex? CompiledRegex { get; set; }
    
    public string? RegexPattern { get; set; }
}
```

#### Step 6: Add Tests

```csharp
[Test]
public void JsonSerializer_ShouldSerializeConfig()
{
    var config = new ColumnizerConfig
    {
        Name = "Test",
        Columns = new List<string> { "Col1", "Col2" }
    };

    var json = JsonConfigSerializer.Serialize(config);
    var deserialized = JsonConfigSerializer.Deserialize<ColumnizerConfig>(json);

    Assert.That(deserialized.Name, Is.EqualTo("Test"));
    Assert.That(deserialized.Columns, Has.Count.EqualTo(2));
}

[Test]
public void Migration_ShouldPreserveBinaryBackup()
{
    // Test that migration creates backup
    var testPath = "test-config.dat";
    // ... create binary file
    
    ConfigMigrationHelper.MigrateFromBinary<TestConfig>(testPath);
    
    Assert.That(File.Exists($"{testPath}.backup"), Is.True);
}
```

---

### Task 1.2.1: Thread.Sleep Elimination

**Priority:** P1 - HIGH  
**Estimated Time:** 1 week

#### Step 1: Find All Thread.Sleep Calls

```bash
grep -rn "Thread\.Sleep" src/
```

#### Step 2: Replace with Async Patterns

```csharp
// OLD - Synchronous blocking
private void PollForChanges()
{
    while (_running)
    {
        Thread.Sleep(100);
        CheckForFileChanges();
    }
}

// NEW - Async non-blocking
private async Task PollForChangesAsync(CancellationToken cancellationToken)
{
    while (!cancellationToken.IsCancellationRequested)
    {
        await Task.Delay(100, cancellationToken);
        await CheckForFileChangesAsync();
    }
}
```

#### Step 3: Update XML Reader

```csharp
// OLD
private XmlBlock ReadBlock()
{
    for (int retry = 0; retry < 10; retry++)
    {
        var block = TryReadBlock();
        if (block != null)
            return block;
        
        Thread.Sleep(100);
    }
    return null;
}

// NEW
private async Task<XmlBlock?> ReadBlockAsync(CancellationToken cancellationToken)
{
    const int maxRetries = 10;
    const int delayMs = 100;
    
    for (int retry = 0; retry < maxRetries; retry++)
    {
        var block = TryReadBlock();
        if (block != null)
            return block;
        
        try
        {
            await Task.Delay(delayMs, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            return null;
        }
    }
    
    // Return partial block after timeout
    return TryReadPartialBlock();
}
```

---

## Testing Guidelines

### Unit Testing

1. **Use NUnit Framework**
   ```csharp
   [TestFixture]
   public class MyTests
   {
       [Test]
       public void MyTest()
       {
           // Arrange
           // Act
           // Assert
       }
   }
   ```

2. **Mock Dependencies with Moq**
   ```csharp
   var mockReader = new Mock<ILogFileReader>();
   mockReader.Setup(r => r.ReadLine()).Returns("test");
   ```

3. **Test Async Code**
   ```csharp
   [Test]
   public async Task AsyncTest()
   {
       var result = await MyAsyncMethod();
       Assert.That(result, Is.Not.Null);
   }
   ```

### Performance Testing

Use BenchmarkDotNet:

```csharp
using BenchmarkDotNet.Attributes;

[MemoryDiagnoser]
public class RegexBenchmarks
{
    private const string Pattern = @"\d{4}-\d{2}-\d{2}";
    private const string Input = "2025-11-11";

    [Benchmark(Baseline = true)]
    public void OldRegex()
    {
        var regex = new Regex(Pattern);
        regex.IsMatch(Input);
    }

    [Benchmark]
    public void CachedRegex()
    {
        var regex = RegexHelper.GetOrCreateCached(Pattern);
        regex.IsMatch(Input);
    }
}
```

---

## Code Review Checklist

Before submitting PR, verify:

- [ ] All tests pass
- [ ] Code follows .editorconfig style
- [ ] XML documentation on public APIs
- [ ] No compiler warnings
- [ ] Performance benchmarks show improvement
- [ ] Security implications considered
- [ ] Breaking changes documented
- [ ] Migration path provided for users

---

## Troubleshooting

### Common Issues

**Issue:** Build fails with missing references  
**Solution:** Run `dotnet restore` or `./build.ps1 --target Restore`

**Issue:** Tests fail on CI but pass locally  
**Solution:** Check for file path assumptions, ensure cross-platform compatibility

**Issue:** Regex timeout exceptions in production  
**Solution:** Increase timeout or simplify pattern, log pattern for review

---

## Resources

- [Full Modernization Plan](./MODERNIZATION_PLAN.md)
- [Quick Reference](./MODERNIZATION_QUICK_REFERENCE.md)
- [Progress Tracker](./MODERNIZATION_PROGRESS.md)
- [Performance Analysis](./performanceandsecurityanalyses.md)

---

**Questions?** Open a GitHub discussion or ask in Discord.

---

*This guide is updated as implementation progresses.*
