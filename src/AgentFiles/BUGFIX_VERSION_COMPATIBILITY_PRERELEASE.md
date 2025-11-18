# Bug Fix: Version Compatibility Check Incorrectly Creating Prerelease Versions

## Issue Summary
The `IsCompatibleWith` method in `PluginManifest.cs` was incorrectly converting `System.Version` to `NuGetVersion` by passing the `Revision` component as a release label parameter. This caused LogExpert version `1.20.0.0` to be converted to `1.20.0-0` (a prerelease version), which failed to satisfy version requirements like `>=1.20.0` that expect stable releases.

## Root Cause
```csharp
// BEFORE - Incorrect code that created prerelease version
var nugetVersion = new NuGetVersion(
    logExpertVersion.Major,
    logExpertVersion.Minor,
    logExpertVersion.Build >= 0 ? logExpertVersion.Build : 0,
    logExpertVersion.Revision >= 0 ? logExpertVersion.Revision.ToString(...) : null);
    // ? This parameter is for release labels (prerelease identifiers), not revision numbers
```

When `Revision` is 0 and passed as `"0"`, it creates version `1.20.0-0` which:
- Has `IsPrerelease = true`
- Does NOT satisfy version range `[1.20.0, )` (requires stable version >= 1.20.0)
- NuGet semantic versioning: prerelease versions are LESS than their stable counterparts

## Solution
Use the 3-parameter constructor to create a stable (non-prerelease) version:

```csharp
// AFTER - Correct code that creates stable version
var nugetVersion = new NuGetVersion(
    logExpertVersion.Major,
    logExpertVersion.Minor,
    logExpertVersion.Build >= 0 ? logExpertVersion.Build : 0);
```

This creates version `1.20.0` which:
- Has `IsPrerelease = false`
- DOES satisfy version range `[1.20.0, )`
- Correctly represents LogExpert's stable release version

## Impact
**Before Fix:**
- Plugins with requirement `>=1.20.0` would be rejected for LogExpert version `1.20.0.0`
- All plugins would fail validation during loading
- Application would skip loading valid, compatible plugins

**After Fix:**
- Version `1.20.0.0` correctly matches requirement `>=1.20.0`
- Plugins load successfully when version requirements are satisfied
- Correct semantic versioning behavior

## Testing
- ? Build successful
- ? No compilation errors
- ? Version `1.20.0` (stable) now satisfies range `[1.20.0, )`

## Files Modified
- `PluginRegistry/PluginManifest.cs` - `IsCompatibleWith` method

## Notes
The `System.Version.Revision` property is not a semantic versioning prerelease indicator. It's a build/revision number in the 4-part version scheme (Major.Minor.Build.Revision). For NuGet semantic versioning, only explicit prerelease labels like "beta", "rc", etc. should be used in the release label parameter.
