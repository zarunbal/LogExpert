# LogExpert — Context & Ubiquitous Language

Glossary of domain terms used in this codebase. Terms here have a single, agreed
meaning; do not redefine them locally.

## Highlighting & triggers

- **Highlight Entry** (`HighlightEntry`) — One search-text rule plus the visual
  styling and trigger flags applied to lines that match it. Belongs to exactly
  one Highlight Group.
- **Highlight Group** (`HighlightGroup`) — A named, ordered list of Highlight
  Entries. The Log Window evaluates lines against the currently selected group.
- **Trigger** — A side-effecting action attached to a Highlight Entry that fires
  when a line matches. Current triggers: Set Bookmark, Stop Tail, Don't Dirty
  LED, Plugin (keyword action), and Audio Alert.
- **Action Entry** (`ActionEntry`) — Plugin name + parameters bound to the
  Plugin trigger of a Highlight Entry.
- **Tail trigger path** — The single code path in `LogWindow.CheckFilterAndHighlight`
  that evaluates highlight entries against *newly appended* lines. Triggers
  that have user-perceivable side effects (currently: Audio Alert) fire **only**
  on this path; bulk/scanner paths intentionally skip them.

## Audio alerts

- **Audio Alert** — A sound played when a tail-only highlight match occurs.
  Toggled per Highlight Entry via `AlertOnHit`.
- **Sound File** (`SoundFilePath`) — Per-entry absolute path to an audio file
  (WAV/MP3/WMA/AIFF, anything NAudio can decode). Empty path means "default
  system beep". A missing or unreadable file falls back silently to the system
  beep and is logged at warn level.
- **Cooldown** (`CooldownSeconds`) — Per-entry minimum seconds between two
  audio alerts. `0` disables throttling.
- **Active Cooldown** — The cooldown value of the *most recently played* alert.
  It gates every subsequent attempt across the whole application until it
  expires, regardless of which entry's cooldown was passed in for the new
  attempt. See ADR 0001.
- **Audio Player** (`LogExpert.Audio.AudioPlayer`) — Static, process-wide
  fire-and-forget player. Owns the single global last-played timestamp and
  active-cooldown state.

## Sessions

- **Session** (`.lxj`) — Persisted workspace: a list of log file paths plus a
  tab/window layout XML blob. Loading a session reopens those files in their
  saved arrangement. Per-file state (bookmarks, filters, columnizer, …) is
  *not* in the session — it loads independently from each file's **Session
  File** (`.lxp`). A session is created or restored only by explicit user
  action ("Save Project" / "Load Project" menu items, to be renamed to
  Session terminology).
- **Session File** (`.lxp`) — JSON file saved beside (or in a central
  directory next to) a log file, restoring per-file state on reload:
  columnizer name, bookmarks, filter pipes, highlight group, multi-file
  state, follow-tail, encoding. Created automatically on save; one per log
  file. Composed by a Session via its `FileNames` list. The columnizer
  field of a Session File is one of several sources that can be selected
  by **Columnizer Selection Priority**; the other fields always load when
  the Session File exists.

*Avoid*: "project" / "project file" / "workspace" (use **Session**),
"persistence file" / "per-file persistence" (use **Session File**),
bare "session file" when you mean the workspace (that's a **Session**).

## Control character display

- **Control Character Substitution** — Display-only replacement of selected
  control characters with visible glyphs, performed inside `LogWindow.PaintCell`.
  Never modifies raw column data, never affects search, filter, highlight
  regex, or columnizer parsing. Off by default. See ADR 0003.
- **Substitution Style** — One of five rendering modes applied globally to
  every enabled control character: Caret notation (`^G`), C escape (`\a` /
  `\xNN`), Abbreviation (`BEL`), Unicode Control Pictures (`␇`), ISO 2047
  (`⍾`). Default: Control Pictures.
- **Enabled Code Points** (`ControlCharSettings.EnabledCodepoints`) — The
  subset of C0 + DEL the user has opted in to substituting. First-time
  default is the "non-whitespace preset": all 33 characters except `HT`,
  `LF`, `CR`.
- **Substitution Style Fallback** — Per-style rule for characters the style
  has no defined glyph for. C escape falls back to `\xNN`; ISO 2047 falls
  back to the Control Pictures glyph; the other three styles cover all 33
  in-scope characters.
- **Copy Displayed Form** (`ControlCharSettings.CopyDisplayedForm`) —
  Opt-in setting that makes clipboard copy (and other "export selection"
  paths) use the substituted text instead of the raw bytes. Default: off,
  i.e. clipboard always carries raw data.

*Avoid*: "control char rendering" (use **Substitution**), "escape" alone
(ambiguous between the **C escape** style and the general concept).

## Columnizer selection

- **Columnizer** (`ILogLineMemoryColumnizer`) — A plugin that parses a log
  line into columns. Each loaded log window has exactly one active
  columnizer at a time. The set of available columnizers is owned by
  `PluginRegistry`.
- **Columnizer Mask Entry** (`ColumnizerMaskEntry`) — One user-configured
  row on the Settings → Columnizers tab. Pairs a **Mask**, a **Mask Type**,
  and a **Columnizer Name**. Stored in `Preferences.ColumnizerMaskList`.
- **Mask Type** — `Glob` or `Regex`. Glob uses `*` and `?` wildcards and
  matches against the short file name; Regex uses .NET regular-expression
  syntax. New rows default to `Glob`; rows that existed in `settings.json`
  before this field was introduced deserialize as `Regex` for backward
  compatibility. See ADR 0004.
- **Stale Mask Entry** — A Columnizer Mask Entry whose `ColumnizerName`
  does not resolve to any currently-registered columnizer. Stale entries
  are skipped at match time (the loop continues to the next entry), kept
  in the settings file (the plugin may return), and flagged in the
  Settings dialog with a leading warning icon.
- **Columnizer History** — Auto-maintained list (`Settings.ColumnizerHistoryList`,
  capped at 40 entries) recording the columnizer last used per absolute
  file path. Stale entries (columnizer no longer registered) are removed
  on lookup; this list, unlike the Mask list, is not user-curated.
- **Columnizer Selection Priority** (`ColumnizerSelectionPriority` enum) —
  The user-configured rule that orders the four sources of a "which
  columnizer should this file open with?" decision: Session File,
  Columnizer History, Columnizer Mask Entry, and AutoPick. Three modes,
  mutually exclusive:
  - `HistoryThenMask` *(default; today's behaviour)* — Session File →
    History → Mask → AutoPick → built-in default.
  - `MaskThenHistory` — Session File → Mask → History → AutoPick → default.
  - `MaskOverridesPersistence` — Mask → Session File → History → AutoPick
    → default. The only mode in which a matching mask outranks an
    existing Session File. Only the columnizer field of the Session File
    is overridden; bookmarks, filters, etc. still restore.

  See ADR 0005. Replaces the deprecated `Preferences.MaskPrio` bool.
  (Note: the enum member name `MaskOverridesPersistence` retains the old
  "Persistence" wording for backward compatibility of serialized settings;
  the user-facing concept is **Session File**.)
- **AutoPick** (`Preferences.AutoPick`) — When on, runs
  `ColumnizerPicker.FindBetterMemoryColumnizer` against the loaded file
  content to auto-detect a columnizer. Fires only when the Columnizer
  Selection Priority chain above produced no result — never overrides
  an explicit Mask, History, or Session File hit.

*Avoid*: "file mask" alone (ambiguous between glob and regex — say
**Glob Mask** or **Regex Mask**), "mask priority" (use **Columnizer
Selection Priority**).

## Flagged ambiguities

- "**session file**" was used by old UI resource keys (e.g.
  `LoadProject_UI_Message_Error_Title_FailedToUpdateSessionFile`) to mean
  the workspace (`.lxj`). Resolved: from now on **Session File** = `.lxp`
  (per-file state) and **Session** = `.lxj` (workspace). Existing resource
  keys named `*SessionFile*` today refer to the workspace and need
  renaming.
- "**project**" / "**project file**" historically referred to `.lxj`.
  Resolved: use **Session**. The `ProjectFileHandler` /
  `ProjectPersister` type names and `OnLoadProjectToolStripMenuItemClick`
  handler are scheduled for renaming.
- "**Per-file Persistence**" was the prior term for `.lxp`. Resolved:
  use **Session File**. The enum member
  `ColumnizerSelectionPriority.MaskOverridesPersistence` keeps its old
  name for serialization compatibility — internal name only, not a
  domain term.
