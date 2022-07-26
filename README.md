# LogExpert [![Build status](https://ci.appveyor.com/api/projects/status/hxwxyyxy81l4tee8/branch/master?svg=true)](https://ci.appveyor.com/project/Zarunbal/logexpert/branch/master)

This is a clone from (no longer exists) https://logexpert.codeplex.com/

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
* Portable (all options / settings saved in application startup directory)

# Download
Follow the [Link](https://github.com/zarunbal/LogExpert/releases/latest) and download the latest package. Just extract it where you want and execute the application or download the Setup and install it

Or Install via chocolatey

```choco install logexpert```

Requirements
- .NET 4.7.2

## CI
This is a continous integration build. So always the latest and greates changes. It should be stable but no promises. Can be viewed as Beta.

[CI Download](https://ci.appveyor.com/project/Zarunbal/logexpert)

# How to Build

- Clone / Fork / Download the source code
- Open the Solution (src/LogExpert.sln) with Visual Studio 2017 (e.g. Community Edition)
- Restore Nuget Packages on Solution
- Build
- The output is under bin/(Debug/Release)/

Nuke.build Requirements
- Chocolatey must be installed
- Optional for Setup Inno Script 5 or 6

# Pull Request
- Use Development branch as target

# FAQ / HELP / Informations / Examples
Please checkout the wiki for FAQ / HELP / Informations / Examples

https://github.com/zarunbal/LogExpert/wiki

# Discord Server
https://discord.gg/SjxkuckRe9

## Credits
### Contributors

This project exists thanks to all the people who contribute.
<a href="https://github.com/zarunbal/LogExpert/graphs/contributors">
  <img src="https://contrib.rocks/image?repo=zarunbal/LogExpert" />
</a>

Made with [contrib.rocks](https://contrib.rocks).
