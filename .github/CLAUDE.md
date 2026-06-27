# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## 1. Think Before Coding

**Don't assume. Don't hide confusion. Surface tradeoffs.**

Before implementing:
- State your assumptions explicitly. If uncertain, ask.
- If multiple interpretations exist, present them - don't pick silently.
- If a simpler approach exists, say so. Push back when warranted.
- If something is unclear, stop. Name what's confusing. Ask.

## 2. Simplicity First

**Minimum code that solves the problem. Nothing speculative.**

- No features beyond what was asked.
- No abstractions for single-use code.
- No "flexibility" or "configurability" that wasn't requested.
- No error handling for impossible scenarios.
- If you write 200 lines and it could be 50, rewrite it.

Ask yourself: "Would a senior engineer say this is overcomplicated?" If yes, simplify.

## 3. Surgical Changes

**Touch only what you must. Clean up only your own mess.**

When editing existing code:
- Don't "improve" adjacent code, comments, or formatting.
- Don't refactor things that aren't broken.
- Match existing style, even if you'd do it differently.
- If you notice unrelated dead code, mention it - don't delete it.

When your changes create orphans:
- Remove imports/variables/functions that YOUR changes made unused.
- Don't remove pre-existing dead code unless asked.

The test: Every changed line should trace directly to the user's request.

## 4. Goal-Driven Execution

**Define success criteria. Loop until verified.**

Transform tasks into verifiable goals:
- "Add validation" → "Write tests for invalid inputs, then make them pass"
- "Fix the bug" → "Write a test that reproduces it, then make it pass"
- "Refactor X" → "Ensure tests pass before and after"

For multi-step tasks, state a brief plan:
```
1. [Step] → verify: [check]
2. [Step] → verify: [check]
3. [Step] → verify: [check]
```

Strong success criteria let you loop independently. Weak criteria ("make it work") require constant clarification.

## Project Overview

LogExpert is a Windows log file viewer and analyzer built with C# and Windows Forms. It's a GUI replacement for the Unix `tail` command with extensive features including tail mode, filtering, bookmarks, highlighting, and a plugin-based architecture for parsing custom log formats.

**Key Technologies:**
- .NET 10.0 (Windows target framework)
- Windows Forms for UI
- Nuke Build System with MSBuild
- NUnit for testing
- Plugin-based architecture

## Build Commands

### Using Nuke Build (Recommended)

```powershell
# Build the solution
./build.ps1

# Clean and build
./build.ps1 --target Clean Compile

# Run tests
./build.ps1 --target Test

# Full release build with packages
./build.ps1 --target Clean Pack CreateSetup --configuration Release
```

### Using .NET CLI Directly

```bash
# From src/ directory
dotnet restore
dotnet build --no-restore
dotnet test --no-build --verbosity normal

# Run specific test project
dotnet test src/LogExpert.Tests/LogExpert.Tests.csproj
```

### Important Build Notes

- **Windows-only**: This project requires Windows and .NET 10.0.100 SDK (specified in [global.json](global.json))
- **Cannot build on Linux/macOS**: Uses Windows Desktop SDK and Windows Forms
- Nuke build automatically downloads the correct .NET SDK version if needed
- Output directory: `bin/(Debug|Release)/`

## Architecture

### High-Level Structure

The codebase follows a modular architecture with clear separation of concerns:

```
LogExpert/
├── LogExpert/              # Main application entry point and UI orchestration
├── LogExpert.Core/         # Core business logic, log reading, filtering
├── LogExpert.UI/           # Windows Forms UI components and dialogs
├── LogExpert.Resources/    # Localization resources
├── LogExpert.Configuration/# Configuration management
├── ColumnizerLib/          # Plugin interface definitions
├── PluginRegistry/         # Plugin discovery and security
└── Columnizers/            # Built-in columnizer plugins (CSV, JSON, Regex, etc.)
```

### Key Architectural Components

#### 1. Single Instance Mode with IPC
- Application uses a Mutex to ensure single instance per session
- Named pipes (`LogExpertInstanceMutex{sessionId}`) for inter-process communication
- Secondary instances send file paths to primary instance via JSON over named pipes
- See [Program.cs](src/LogExpert/Program.cs) for implementation

#### 2. Plugin System
- **Columnizers** (`ILogLineColumnizer`): Parse log lines into columns
- **File System Plugins** (`IFileSystemPlugin`): Support non-local file sources (e.g., SFTP)
- **Context Menu Plugins** (`IContextMenuEntry`): Add custom menu items
- **Keyword Actions** (`IKeywordAction`): React to keywords in logs
- Plugin discovery happens at startup via `PluginRegistry`
- Plugins are loaded from `plugins/` and `pluginsx86/` directories
- Security: Plugin hashes are verified against generated hashes (Release builds only)
- See [PLUGIN_DEVELOPMENT_GUIDE.md](src/docs/PLUGIN_DEVELOPMENT_GUIDE.md) for details

#### 3. Log File Reading
- Abstract base class: `PositionAwareStreamReaderBase`
- Implementations: `PositionAwareStreamReaderSystem`, `PositionAwareStreamReaderLegacy`
- Uses buffered streams for efficient reading of large files
- Supports encoding detection (UTF-8, UTF-16, UTF-32 with BOM)
- Position tracking for tail mode and seeking
- See [src/LogExpert.Core/Classes/Log/](src/LogExpert.Core/Classes/Log/) for implementations

#### 4. Configuration Management
- Centralized via `ConfigManager.Instance`
- Initialized with application startup path and screen information
- Supports import/export of settings
- Persists user preferences, columnizer history, highlight masks, etc.
- Configuration stored in application startup directory (portable mode)

#### 5. Windows Forms UI Architecture
- MDI interface with tab support via `AbstractLogTabWindow`
- Main window created in [LogTabWindow.cs](src/LogExpert.UI/Dialogs/LogTabWindow/)
- Custom controls: `BufferedDataGridView`, `LogTabControl`, `DateTimeDragControl`
- High DPI considerations: Avoid `AutoScaleMode` and `AutoScaleDimensions` on individual controls
- Dark mode support via `Application.SetColorMode()`

### Critical Files and Their Purposes

- [Program.cs](src/LogExpert/Program.cs) - Application entry point, IPC setup, single instance handling
- [AbstractLogTabWindow.cs](src/LogExpert.UI/Extensions/LogWindow/AbstractLogTabWindow.cs) - Main window factory and orchestration
- [ILogLineColumnizer.cs](src/ColumnizerLib/ILogLineColumnizer.cs) - Core plugin interface for columnizers
- [ColumnizerPicker.cs](src/LogExpert.Core/Classes/Columnizer/ColumnizerPicker.cs) - Automatic columnizer detection
- [PluginRegistry.cs](src/PluginRegistry/) - Plugin discovery and security verification
- [ConfigManager.cs](src/LogExpert.Configuration/) - Configuration persistence and management
- [LogBuffer.cs](src/LogExpert.Core/Classes/Log/LogBuffer.cs) - In-memory log line caching

## Development Workflow

### Adding a New Columnizer Plugin

1. Create new project in `src/` following naming pattern `*Columnizer`
2. Add project reference to `ColumnizerLib`
3. Implement `ILogLineColumnizer` interface
4. Add project to `src/LogExpert.sln`
5. Create corresponding test project in `Tests/` folder
6. Plugin will be auto-discovered at runtime from output directory

### Modifying Core Log Reading Logic

- Core reading classes are in [src/LogExpert.Core/Classes/Log/](src/LogExpert.Core/Classes/Log/)
- Inherit from `PositionAwareStreamReaderBase` for custom stream readers
- Key methods to implement: `ReadLine()`, `Position` property, `Seek()`
- Always maintain position tracking for tail mode support

### Working with Windows Forms UI

- UI components in `LogExpert.UI` project
- Follow existing High DPI patterns (no AutoScale on controls)
- Test with both light and dark mode (see `SetDarkMode()` in Program.cs)
- Use localization resources from `LogExpert.Resources` project
- Windows Forms designer files: `*.designer.cs`

### Testing

- Unit tests use NUnit framework with Moq for mocking
- Test projects follow naming pattern `*.Tests`
- Test data stored in `TestData/` directories within test projects
- Run all tests: `./build.ps1 --target Test`
- Run specific test: `dotnet test src/LogExpert.Tests/LogExpert.Tests.csproj`

## Important Patterns and Conventions

### Code Style
- Nullable reference types enabled (`<Nullable>enable</Nullable>`)
- Comprehensive `.editorconfig` with 4000+ rules
- ImplicitUsings enabled
- Assembly signing enabled (Key.snk)

### Configuration Files
- **Directory.Build.props** - Common MSBuild properties for all projects
- **Directory.Packages.props** - Centralized NuGet package version management
- **global.json** - .NET SDK version pinning (10.0.100)
- **.editorconfig** - Code style and analysis rules

### Project Organization
- Solution folders: "Columnizers", "Tests", "docs", "setup"
- Test projects nested under "Tests" solution folder
- Columnizer projects nested under "Columnizers" solution folder
- Documentation in `src/docs/` included in solution

### Git Workflow
- Default branch: `Development` (use for PRs)
- Commit format: Include "Co-Authored-By: Claude Sonnet 4.5 <noreply@anthropic.com>"
- GitHub Actions run on push to Development branch
- AppVeyor for CI builds and artifact creation

## Plugin Security System

**Release builds only:**
- After compilation, `PluginHashGenerator.Tool` generates SHA256 hashes of all plugins
- Hashes stored in [PluginHashGenerator.Generated.cs](src/PluginRegistry/PluginHashGenerator.Generated.cs)
- At runtime, `PluginRegistry` verifies plugin hashes before loading
- Users can trust new plugins via UI dialog
- Hash updates automated via GitHub Actions on successful builds
- See [PLUGIN_HASH_MANAGEMENT.md](src/docs/PLUGIN_HASH_MANAGEMENT.md)

## Common Gotchas

1. **Cross-platform builds fail**: This is Windows-only. Don't attempt Linux/macOS builds.
2. **SDK version mismatch**: Must use .NET 10.0.100 SDK (specified in global.json)
3. **Plugin not loading**: Check output directory - plugins must be in `plugins/` or `pluginsx86/`
4. **High DPI issues**: Never use AutoScaleMode on individual controls, only on forms
5. **IPC failures**: Named pipes require proper Windows permissions and session isolation
6. **Encoding detection**: BOM-less files default to encoding from EncodingOptions
7. **Plugin hashes**: Only verified in Release builds; Debug builds skip verification

## Key Dependencies

- **NLog**: Logging framework
- **Newtonsoft.Json**: JSON serialization (legacy, but widely used)
- **CsvHelper**: CSV parsing in CsvColumnizer
- **SSH.NET**: SFTP support in SftpFileSystem plugins
- **DockPanelSuite**: Docking panel UI controls
- **Moq/NUnit**: Testing frameworks

## References

- Main README: [README.md](README.md)
- Plugin Development: [PLUGIN_DEVELOPMENT_GUIDE.md](src/docs/PLUGIN_DEVELOPMENT_GUIDE.md)
- Plugin Hash System: [PLUGIN_HASH_MANAGEMENT.md](src/docs/PLUGIN_HASH_MANAGEMENT.md)
- Performance Benchmarks: [BENCHMARK_SUMMARY.md](src/docs/performance/BENCHMARK_SUMMARY.md)
- GitHub Wiki: https://github.com/LogExperts/LogExpert/wiki
- Discord: https://discord.gg/SjxkuckRe9

# Update Rules File
To update this file, ensure that all sections are kept current with the latest architectural decisions, build processes, and development workflows. Follow these guidelines:
- Review and update build commands if there are changes in the build system.
- Reflect any architectural changes in the "Architecture" section.
- Keep development workflow steps accurate for new contributors.
- Regularly verify links to other documentation files.
- Maintain clarity and conciseness for ease of understanding by new developers.
- Use consistent formatting throughout the document.
- Add new sections as needed for significant changes in the project structure or processes.
- Ensure all technical terms are explained or linked to relevant documentation.
- Periodically review for outdated information and remove or update as necessary.
- If told to not do something, ensure this is also added to the "Dont Do that" section.