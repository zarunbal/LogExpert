# GeneratePluginHashes.ps1
# Generates SHA256 hashes for all built-in LogExpert plugins

param(
    [string]$PluginsDirectory = ".\bin\Release\plugins",
    [string]$OutputFile = "plugin-hashes.json",
    [switch]$GenerateCSharpCode
)

Write-Host "LogExpert Plugin Hash Generator" -ForegroundColor Cyan
Write-Host "================================" -ForegroundColor Cyan
Write-Host ""

# List of built-in plugins
$builtInPlugins = @(
    "AutoColumnizer.dll",
    "CsvColumnizer.dll",
    "JsonColumnizer.dll",
    "JsonCompactColumnizer.dll",
    "RegexColumnizer.dll",
    "Log4jXmlColumnizer.dll",
    "GlassfishColumnizer.dll",
    "DefaultPlugins.dll",
    "FlashIconHighlighter.dll",
    "SftpFileSystem.dll",
    "SftpFileSystemx86.dll",
    "SftpFileSystemx64.dll"
)

# Check if plugins directory exists
if (-not (Test-Path $PluginsDirectory)) {
    Write-Host "ERROR: Plugins directory not found: $PluginsDirectory" -ForegroundColor Red
    Write-Host ""
    Write-Host "Please build LogExpert in Release mode first, or specify the correct path:" -ForegroundColor Yellow
    Write-Host "  .\GeneratePluginHashes.ps1 -PluginsDirectory 'path\to\plugins'" -ForegroundColor Yellow
    Write-Host ""
    exit 1
}

Write-Host "Plugins Directory: $PluginsDirectory" -ForegroundColor Gray
Write-Host "Output File: $OutputFile" -ForegroundColor Gray
Write-Host ""

# Calculate hashes
$hashes = @{}
$foundCount = 0
$missingCount = 0

foreach ($plugin in $builtInPlugins) {
    $pluginPath = Join-Path $PluginsDirectory $plugin
    
    if (Test-Path $pluginPath) {
        try {
            $hash = Get-FileHash -Path $pluginPath -Algorithm SHA256
            $hashes[$plugin] = $hash.Hash
            Write-Host "? $plugin" -ForegroundColor Green
            Write-Host "  Hash: $($hash.Hash.Substring(0,32))..." -ForegroundColor Gray
            $foundCount++
        }
        catch {
            Write-Host "? $plugin : ERROR - $($_.Exception.Message)" -ForegroundColor Red
        }
    }
    else {
        Write-Host "? $plugin : FILE NOT FOUND" -ForegroundColor Yellow
        $missingCount++
    }
}

Write-Host ""
Write-Host "Summary:" -ForegroundColor Cyan
Write-Host "  Found: $foundCount" -ForegroundColor Green
Write-Host "  Missing: $missingCount" -ForegroundColor $(if ($missingCount -gt 0) { "Yellow" } else { "Gray" })
Write-Host "  Total: $($builtInPlugins.Count)" -ForegroundColor Gray
Write-Host ""

# Save to JSON
$output = @{
    PluginHashes = $hashes
    GeneratedAt = (Get-Date).ToString("o")
    PluginsDirectory = $PluginsDirectory
    TotalPlugins = $builtInPlugins.Count
    HashedPlugins = $hashes.Count
    MissingPlugins = $missingCount
} | ConvertTo-Json -Depth 10

$output | Out-File -FilePath $OutputFile -Encoding UTF8
Write-Host "? Hashes saved to: $OutputFile" -ForegroundColor Green

# Generate C# code if requested
if ($GenerateCSharpCode) {
    $csharpCode = @"
// Auto-generated plugin hashes
// Generated: $(Get-Date -Format "yyyy-MM-dd HH:mm:ss")
// DO NOT EDIT MANUALLY - Run GeneratePluginHashes.ps1 to regenerate

private static Dictionary<string, string> GetBuiltInPluginHashes()
{
    return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
"@

    foreach ($entry in $hashes.GetEnumerator() | Sort-Object Name) {
        $csharpCode += "`n        [`"$($entry.Key)`"] = `"$($entry.Value)`","
    }

    # Remove trailing comma from last entry
    $csharpCode = $csharpCode.TrimEnd(',')

    $csharpCode += @"

    };
}
"@

    $csharpFile = "BuiltInPluginHashes.cs"
    $csharpCode | Out-File -FilePath $csharpFile -Encoding UTF8
    Write-Host "? C# code saved to: $csharpFile" -ForegroundColor Green
    Write-Host ""
    Write-Host "Copy the GetBuiltInPluginHashes() method into PluginValidator.cs" -ForegroundColor Yellow
}

Write-Host ""

# Warn if plugins are missing
if ($missingCount -gt 0) {
    Write-Host "WARNING: $missingCount plugin(s) not found!" -ForegroundColor Yellow
    Write-Host "Make sure to build LogExpert in Release mode before generating hashes." -ForegroundColor Yellow
    Write-Host ""
    exit 1
}

Write-Host "? All $($hashes.Count) plugin hashes generated successfully!" -ForegroundColor Green
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Cyan
Write-Host "1. Review the generated hashes in $OutputFile" -ForegroundColor White
Write-Host "2. Update PluginValidator.cs with the GetBuiltInPluginHashes() method" -ForegroundColor White
Write-Host "3. Test hash verification with: dotnet test" -ForegroundColor White
Write-Host ""

exit 0
