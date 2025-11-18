# Debug Steps for VersionRange.Parse Issue

**Problem:** `VersionRange.Parse(">= 1.10.0")` throws exception

## Step 1: Run Debug Test

Run the test `Debug VersionParsing_WithExactInputFromUser` in `PluginManifestVersionParsingTests.cs` 

This will show:
- The original requirement string
- The normalized version
- The exact exception details

## Step 2: Check NLog Output

Set NLog minimum level to Debug in `NLog.config`:

```xml
<rules>
    <logger name="*" minlevel="Debug" writeTo="file" />
</rules>
```

Then run the application and check the log file for:
- "Normalized version requirement:"
- "Validating version requirement:"
- Any exception messages

## Step 3: Verify NuGet.Versioning Version

Current version: 7.0.0 (latest)

Check if it's actually loading the correct version:
```csharp
var assembly = typeof(VersionRange).Assembly;
Console.WriteLine($"NuGet.Versioning version: {assembly.GetName().Version}");
```

## Step 4: Test Normalization

The normalization should convert:
- Input: `">= 1.10.0"`
- After `.Replace(">= ", ">=")`: `">=1.10.0"`
- After `.Trim()`: `">=1.10.0"`

Expected result: `VersionRange.Parse(">=1.10.0")` should work

## Step 5: Possible Issues

### Issue A: Multiple Spaces
If input is `">=  1.10.0"` (double space), current code won't handle it.

**Fix:** Add recursive space removal
```csharp
while (normalized.Contains("  ", StringComparison.Ordinal))
{
    normalized = normalized.Replace("  ", " ", StringComparison.Ordinal);
}
```

### Issue B: Space BEFORE Version
If input is `">= 1.10.0 "` (trailing space), current code handles it with `.Trim()`

But if input is `">=  1.10.0"` (space between operator and version), need:
```csharp
// Remove all spaces between operator and version number
normalized = Regex.Replace(normalized, @"([><=~^]+)\s+", "$1");
```

### Issue C: Wrong Operator Format
NuGet.Versioning might not accept certain formats. Check documentation:
https://docs.microsoft.com/en-us/nuget/concepts/package-versioning

Valid formats:
- `">=1.0.0"` ?
- `"[1.0.0, 2.0.0)"` ?  
- `"(1.0.0,)"` ?
- `"~1.0.0"` ? (tilde)
- `"^1.0.0"` ? (caret) **ONLY in NuGet.Versioning 6.0+**

### Issue D: Version Format
NuGet.Versioning requires at least Major.Minor:
- `"1.10.0"` ?
- `"1.10"` ?
- `"1"` ? Might fail

## Step 6: Enhanced Normalization

If basic normalization doesn't work, try comprehensive version:

```csharp
private static string NormalizeVersionRequirement(string requirement)
{
    if (string.IsNullOrWhiteSpace(requirement))
    {
        return requirement;
    }

    // Remove all extra spaces first
    var normalized = Regex.Replace(requirement, @"\s+", " ");
    
    // Remove spaces after operators
    normalized = Regex.Replace(normalized, @"([><=~^]+)\s+", "$1");
    
    // Remove spaces in version ranges like "[ 1.0.0 , 2.0.0 )"
    normalized = Regex.Replace(normalized, @"\[\s+", "[");
    normalized = Regex.Replace(normalized, @"\s+\]", "]");
    normalized = Regex.Replace(normalized, @"\(\s+", "(");
    normalized = Regex.Replace(normalized, @"\s+\)", ")");
    normalized = Regex.Replace(normalized, @"\s+,\s+", ",");
    
    return normalized.Trim();
}
```

## Step 7: Ultimate Fallback

If all else fails, parse version string manually:

```csharp
private static VersionRange ParseVersionRangeSafe(string requirement)
{
    // Try direct parsing first
    try
    {
        return VersionRange.Parse(requirement);
    }
    catch
    {
        // Manual parsing fallback
        // Extract operator and version
        var match = Regex.Match(requirement, @"^([><=~^]+)(.+)$");
        if (match.Success)
        {
            var op = match.Groups[1].Value.Trim();
            var version = match.Groups[2].Value.Trim();
            
            // Rebuild without spaces
            var clean = $"{op}{version}";
            return VersionRange.Parse(clean);
        }
        throw;
    }
}
```

## Expected Output from Debug Test

When you run `DebugVersionParsing_WithExactInputFromUser`, you should see:

**Success case:**
```
Original: '>= 1.10.0'
Normalized: '>=1.10.0'
Parsed successfully: [>=1.10.0, )
```

**Failure case (what you're seeing):**
```
Original: '>= 1.10.0'
Normalized: '>=1.10.0'
Exception: ArgumentException
Message: [exact message here]
```

## Next Steps

1. Run the debug test
2. Copy the EXACT exception message here
3. I'll provide a targeted fix based on the actual error

The issue is likely one of:
- Multiple spaces between operator and version
- Wrong version format
- NuGet.Versioning API change in 7.0.0
- Culture-specific parsing issue
