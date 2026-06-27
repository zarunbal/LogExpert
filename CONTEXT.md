# LogExpert

A Windows log file viewer that supports real-time tailing, filtering, highlighting, bookmarking, and columnar parsing of structured log formats.

## Language

### Log Viewing

**Log Window**: The control that displays, tails, and filters a single log file. One Log Window per open file.
_Avoid_: Tab, panel, view

**Session**: A named set of Log Windows and their layout, persisted to disk and restored together.
_Avoid_: Project, workspace, profile

**Columnizer**: A plugin that parses a raw log line into typed columns for structured display.
_Avoid_: Parser, formatter

**Highlight Group**: A named set of rules that colour-code log lines by content pattern.
_Avoid_: Colour scheme, filter rule

### External Tools

**External Tool**: A user-configured command-line program that LogExpert can launch from the Tool Launcher Bar, optionally with Sysout Capture.
_Avoid_: Plugin, script

**Sysout Capture**: A mode for an External Tool where the tool's stdout is redirected to a temp file and opened as a live Log Window.
_Avoid_: Stdout pipe, output redirect

**Tool Launcher Bar**: The toolbar strip populated from the configured External Tool entries.
_Avoid_: Toolbar, tool strip

### Sessions & Persistence

**Session File**: A `.lxj` file that stores the list of log files, per-file settings, and the dock layout for a Session.
_Avoid_: Project file, config file

**Session File Reference** (`.lxp`): An indirection file that maps to one or more actual log files, allowing a Session to track a logical source rather than a fixed path.
_Avoid_: Log pointer, alias
