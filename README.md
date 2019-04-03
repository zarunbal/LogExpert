# LogExpert [![Build status](https://ci.appveyor.com/api/projects/status/hxwxyyxy81l4tee8/branch/master?svg=true)](https://ci.appveyor.com/project/Zarunbal/logexpert/branch/master)

Clone from https://logexpert.codeplex.com/

# Overview
LogExpert is a Windows tail program (a GUI replacement for the Unix tail command).

Summary of (most) features:

* Tail mode
* MDI-Interface with Tabs
* Search function (including RegEx)
* Bookmarks
* A very flexible filter view and possibility to filter to tab
* Highlighting lines via search criteria
* Triggers (e.g. create Bookmark or execute a plugin) via search criteria
* Columnizers: Plugins which split log lines into columns
* Unicode support
* log4j XML file support
* 3rd party plugin support
* Plugin API for more log file data sources
* Automatical determine columnizer with given file name and content (Experimental)
* Serilog.Formatting.Compact format support (Experimental)

# Download
Follow the [Link](https://github.com/zarunbal/LogExpert/releases/latest) and download the latest package. Just extract it where you want and execute the application.

Or Install via chocolatey

```choco install logexpert```

Requirements
- .NET 4.0

## CI
This is a continous integration build. So always the latest and greates changes. It should be stable but no promises. Can be viewed as Beta.

[CI Download](https://ci.appveyor.com/project/Zarunbal/logexpert)

# How to Build

- Clone / Fork / Download the source code
- Open the Solution (src/LogExpert.sln) with Visual Studio 2017 (e.g. Community Edition)
- Restore Nuget Packages on Solution
- Build
- The output is under bin/(Debug/Release)/

# Pull Request
- Use Development branch as target
