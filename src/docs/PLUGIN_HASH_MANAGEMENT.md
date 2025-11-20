# Plugin Hash Management

## Overview

LogExpert uses SHA256 hashes to verify plugin integrity and prevent tampering. When plugins are rebuilt, their hashes change, and the system needs to be updated.

## Automated Process

### Option 1: Using the Hash Generator Tool (Recommended)

After building plugins:

```powershell
# From repository root
dotnet run --project src/PluginHashGenerator.Tool/PluginHashGenerator.Tool.csproj -- "bin/Release/" "src/PluginRegistry/PluginHashGenerator.Generated.cs" Release
```

This will:
- Scan all plugin DLLs in `bin/Release/plugins/` and `bin/Release/pluginsx86/`
- Calculate SHA256 hashes for each plugin
- Generate `PluginHashGenerator.Generated.cs` with the `GetBuiltInPluginHashes()` method
- Automatically integrate with the partial class `PluginValidator`

### Option 2: MSBuild Integration (Automatic)

The plugin hashes are automatically regenerated during Release builds if `GeneratePluginHashesEnabled` is set to `true`.

**When Does It Run?**
The hash generation only runs when ALL of these conditions are met:
1. Building in **Release** configuration
2. `GeneratePluginHashesEnabled` is set to `true`
3. The `plugins` folder exists in the output directory

This means:
- ✅ Building the test projects won't trigger hash generation (plugins folder doesn't exist yet)
- ✅ Building individual projects won't trigger it until plugins are actually built
- ✅ Only runs when there are actually plugins to hash

To enable/disable:

```xml
<!-- In PluginRegistry/LogExpert.PluginRegistry.csproj -->
<PropertyGroup>
  <GeneratePluginHashesEnabled>true</GeneratePluginHashesEnabled>
</PropertyGroup>
```

**Note**: The `GeneratePluginHashes` target only exists in the `LogExpert.PluginRegistry` project. To manually trigger hash generation:

```powershell
# From repository root - build the PluginRegistry project
dotnet build src/PluginRegistry/LogExpert.PluginRegistry.csproj /t:GeneratePluginHashes --configuration Release
```

**Important**: The target requires that plugins have already been built and are present in the output directory. Make sure to build the entire solution first:

```powershell
# 1. Build all projects (including plugins) in Release mode
dotnet build src/LogExpert.sln --configuration Release

# 2. The hash generation happens automatically if plugins exist
# You can manually trigger it again if needed:
dotnet build src/PluginRegistry/LogExpert.PluginRegistry.csproj /t:GeneratePluginHashes --configuration Release
```

### Option 3: Nuke Build Target

```powershell
# From repository root
./build.ps1 --target GeneratePluginHashes
```

## File Structure

```
src/
├── PluginRegistry/
│   ├── PluginValidator.cs              # Original partial class
│   ├── PluginHashGenerator.Generated.cs # Auto-generated (gitignored)
│   ├── PluginHashGenerator.targets      # MSBuild integration
│   └── LogExpert.PluginRegistry.csproj
├── PluginHashGenerator.Tool/
│   ├── Program.cs                       # Hash generator implementation
│   └── PluginHashGenerator.Tool.csproj
```

## Generated Code Example

```csharp
// PluginHashGenerator.Generated.cs (auto-generated)
public static partial class PluginValidator
{
    /// <summary>
    /// Gets pre-calculated SHA256 hashes for built-in plugins.
    /// Generated: 2025-01-11 15:30:00 UTC
    /// Configuration: Release
    /// Plugin count: 11
    /// </summary>
    public static Dictionary<string, string> GetBuiltInPluginHashes()
    {
        return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["AutoColumnizer.dll"] = "F64B30FB8A4DF1C5...",
            ["CsvColumnizer.dll"] = "19B94F2396423345...",
            // ... rest of plugins
        };
    }
}
```

## User Configuration Update

The `trusted-plugins.json` file on user machines is managed separately:

1. **First Launch**: Created with default hashes from `GetBuiltInPluginHashes()`
2. **Plugin Updates**: When users install an updated version of LogExpert:
   - The hardcoded hashes in `GetBuiltInPluginHashes()` reflect the new plugin versions
   - `PluginValidator.ValidatePlugin()` will detect mismatches
   - Users see a warning: "Plugin hash mismatch - file may have been modified"
   - Users can re-trust plugins via Settings > Plugin Management

### Automatic Trust Update (Future Enhancement)

To automatically update user trust on official updates:

```csharp
// In PluginValidator.LoadTrustedPluginConfiguration()
if (UpdatesAvailable())
{
    // Merge new hashes from GetBuiltInPluginHashes() into user config
    foreach (var (plugin, hash) in GetBuiltInPluginHashes())
    {
        if (_trustedPluginConfig.PluginNames.Contains(plugin))
        {
            _trustedPluginConfig.PluginHashes[plugin] = hash;
        }
    }
    SaveTrustedPluginConfiguration();
}
```

## Troubleshooting

### "Plugin hash mismatch" after rebuild

1. Regenerate hashes: `dotnet run --project src/PluginHashGenerator.Tool/PluginHashGenerator.Tool.csproj -- "bin/Release/net10.0-windows/" "src/PluginRegistry/PluginHashGenerator.Generated.cs" Release`
2. Rebuild solution
3. Delete `%APPDATA%\LogExpert\trusted-plugins.json` for testing

### Generated file not found during build

The generated file is created during the first build. If missing:
1. Run the hash generator tool manually (see Option 1 above)
2. OR build twice (first build creates it, second build includes it)

### Hashes don't match expected values

- Ensure you're building in Release mode
- Check that plugin DLLs haven't been modified after calculation
- Verify the correct plugins folder is being scanned

### Error: "The target 'GeneratePluginHashes' does not exist in the project"

**Cause**: You're trying to run the target on the wrong project (e.g., test project or solution file).

**Solution**: The `GeneratePluginHashes` target only exists in the `LogExpert.PluginRegistry` project:

```powershell
# ❌ WRONG - Running on test project
dotnet build src/PluginRegistry.Tests/LogExpert.PluginRegistry.Tests.csproj /t:GeneratePluginHashes

# ❌ WRONG - Running on solution
dotnet build src/LogExpert.sln /t:GeneratePluginHashes

# ✅ CORRECT - Running on PluginRegistry project
dotnet build src/PluginRegistry/LogExpert.PluginRegistry.csproj /t:GeneratePluginHashes --configuration Release
```

**Prerequisites**: Make sure plugins are already built before generating hashes:
```powershell
# 1. Build everything first
dotnet build src/LogExpert.sln --configuration Release

# 2. Then generate hashes
dotnet build src/PluginRegistry/LogExpert.PluginRegistry.csproj /t:GeneratePluginHashes --configuration Release
```

### Error: Build fails with "exited with code 1" when building test projects in Release mode

**Cause**: The hash generator was trying to run before plugins were built, or the `plugins` folder didn't exist.

**Solution**: This has been fixed in the targets file. The hash generation now only runs when:
1. Building in Release configuration
2. The `plugins` folder actually exists in the output directory

If you still encounter this:
```powershell
# Build solution first to create all plugins
dotnet build src/LogExpert.sln --configuration Release

# Then build the test project
dotnet build src/PluginRegistry.Tests/LogExpert.PluginRegistry.Tests.csproj --configuration Release
```

### Error: "WARNING: No plugin DLLs found. Skipping hash generation."

**Cause**: The hash generator ran but couldn't find any plugin DLLs in the expected location.

**Solution**: Make sure plugins are built before running hash generation:
```powershell
# 1. Build all plugin projects
dotnet build src/LogExpert.sln --configuration Release

# 2. Verify plugins exist
dir bin/Release/net10.0-windows/plugins/

# 3. Then generate hashes
dotnet run --project src/PluginHashGenerator.Tool/PluginHashGenerator.Tool.csproj -- "bin/Release/net10.0-windows/" "src/PluginRegistry/PluginHashGenerator.Generated.cs" Release
```
## Quick Start Guide

After modifying plugins, regenerate hashes:

```powershell
# 1. Build the tool first (if not already built)
cd G:\Github\LogExpert
dotnet build src/PluginHashGenerator.Tool/PluginHashGenerator.Tool.csproj

# 2. Build your plugins in Release mode
dotnet build src/LogExpert.sln --configuration Release

# 3. Generate the hashes
dotnet run --project src/PluginHashGenerator.Tool/PluginHashGenerator.Tool.csproj -- "bin/Release/net10.0-windows/" "src/PluginRegistry/PluginHashGenerator.Generated.cs" Release

# 4. Rebuild to include the generated file
dotnet build src/PluginRegistry/LogExpert.PluginRegistry.csproj --configuration Release
