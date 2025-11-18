# Plugin Hash Management

## Overview

LogExpert uses SHA256 hashes to verify plugin integrity and prevent tampering. When plugins are rebuilt, their hashes change, and the system needs to be updated.

## Current Manual Process (BEFORE)

1. Build plugins in Release mode
2. Run the `GenerateBuiltInPluginHashes` test (marked with `[Explicit]`)
3. Copy hash values from test output
4. Manually update `GetBuiltInPluginHashes()` method in `PluginValidator.cs`
5. User's `trusted-plugins.json` files are NOT automatically updated

## Automated Process (NEW)

### Option 1: Using the Hash Generator Tool (Recommended)

After building plugins:

```powershell
# From repository root
dotnet run --project build/PluginHashGenerator.Tool/PluginHashGenerator.Tool.csproj -- "bin/Release/" "src/PluginRegistry/PluginHashGenerator.Generated.cs" Release
```

This will:
- Scan all plugin DLLs in `bin/Release/plugins/` and `bin/Release/pluginsx86/`
- Calculate SHA256 hashes for each plugin
- Generate `PluginHashGenerator.Generated.cs` with the `GetBuiltInPluginHashes()` method
- Automatically integrate with the partial class `PluginValidator`

### Option 2: MSBuild Integration (Automatic)

The plugin hashes are automatically regenerated during Release builds if `GeneratePluginHashesEnabled` is set to `true`.

To enable/disable:

```xml
<!-- In PluginRegistry/LogExpert.PluginRegistry.csproj -->
<PropertyGroup>
  <GeneratePluginHashesEnabled>true</GeneratePluginHashesEnabled>
</PropertyGroup>
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
build/
└── PluginHashGenerator.Tool/
    ├── Program.cs                       # Hash generator implementation
    └── PluginHashGenerator.Tool.csproj
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

1. Regenerate hashes: `dotnet run --project build/PluginHashGenerator.Tool/...`
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

## Integration with CI/CD

In your build pipeline (AppVeyor, GitHub Actions):

```yaml
- name: Generate Plugin Hashes
  run: dotnet run --project build/PluginHashGenerator.Tool/PluginHashGenerator.Tool.csproj -- "bin/Release/" "src/PluginRegistry/PluginHashGenerator.Generated.cs" Release

- name: Commit Updated Hashes
  run: |
    git add src/PluginRegistry/PluginHashGenerator.Generated.cs
    git commit -m "chore: update plugin hashes" || true
```

## Benefits

✅ No manual hash copying from test output
✅ Hashes always match current plugin builds  
✅ Reduced human error  
✅ CI/CD integration ready
✅ Easy to regenerate after any plugin change
✅ Timestamped and documented in generated code

## Migration from Old Process

1. Remove old `GetBuiltInPluginHashes()` method from `PluginValidator.cs`
2. Add `partial` keyword to `PluginValidator` class declaration
3. Run hash generator tool
4. Commit `PluginHashGenerator.Generated.cs` to git
5. Update documentation to reference new process

# 1. Build the tool first
```powershell
cd C:\Github\LogExpert
dotnet build build/PluginHashGenerator.Tool/PluginHashGenerator.Tool.csproj
```

# 2. Build your plugins in Release mode
```powershell
dotnet build src/LogExpert.sln --configuration Release
```

# 3. Generate the hashes
```powershell
dotnet run --project build/PluginHashGenerator.Tool/PluginHashGenerator.Tool.csproj -- "bin/Release/net10.0-windows/" "src/PluginRegistry/PluginHashGenerator.Generated.cs" Release
```

# 4. Rebuild to include the generated file
```powershell
dotnet build src/PluginRegistry/LogExpert.PluginRegistry.csproj
```
