# Tickets: Filter Spread extraction

Extract the Filter Spread rules (back/fore spread context expansion, duplicate-suppression history, rollover shifting) out of the Log Window into one pure, tested Core module, per `docs/specs/filter-spread-extraction.md`.

Work the **frontier**: any ticket whose blockers are all done. After the first ticket, tickets 2–4 can proceed in any order (or in parallel); the final ticket waits for all three.

## Filter Spread module in Core with table tests; multi-threaded filter delegates

**What to build:** A pure Filter Spread module in Core that owns hit expansion (back-spread lines, the hit, fore-spread lines — deduplicated against an already-emitted history, clamped to the file's line range) and the history-trim rule (window unified at 99, the UI knob's maximum spread). The multi-threaded filter delegates its per-hit accumulation to the module and its private spread copy is deleted. This is the tracer bullet: multi-threaded filtering immediately gains the line-0 fix and stops emitting duplicate context lines at spreads over 50.

**Blocked by:** None — can start immediately.

- [x] Module is pure: no dependency on controls, callbacks, readers, or locks; line count is passed in as a value
- [x] Expansion output order matches today's: ascending back-spread, hit, ascending fore-spread
- [x] Line 0 is a valid back-spread line (behavior change, pinned by an explicit test)
- [x] History window constant is 99 and lives only in the module
- [x] Table-driven NUnit edge-case tests pass: hit at line 0, hit within back-spread of line 0, hit near/at end-of-file, spread 0/0, asymmetric spreads, overlapping hits, hit already in history, trim at the 2×99 boundary
- [x] Multi-threaded filter's private spread arithmetic and trim snippet are deleted; it delegates to the module
- [x] Full solution builds and the existing test suite stays green

## Log Window serial filter delegates to the module

**What to build:** Single-threaded filtering in the Log Window produces module-owned spread: its per-hit accumulation delegates expansion and history trimming to the Filter Spread module, keeping its own locking and progress/GUI concerns at the call site. The line-0 fix and the 99 window go live for serial filtering, making it consistent with the parallel path.

**Blocked by:** Filter Spread module in Core with table tests; multi-threaded filter delegates.

- [x] Serial per-hit accumulation calls the module for expansion and trimming; no spread arithmetic remains inline in that path
- [x] Equivalence-guard test: an identical hit sequence fed the way the serial and parallel call sites do produces identical result/hit/history lists
- [x] Filtering a file with spread configured behaves identically whether the single- or multi-threaded filter runs
- [x] Full solution builds and the test suite stays green

## Filter Pipe processing delegates to the module

**What to build:** Lines written to a Filter Pipe tab receive the same module-owned spread expansion and history trimming as the filter result view, so derived tabs stay consistent with their source. The pipe path's inline trim (which today removes one entry at a time against the divergent constant) is replaced by the module's trim rule.

**Blocked by:** Filter Spread module in Core with table tests; multi-threaded filter delegates.

- [x] Filter Pipe per-hit processing calls the module for expansion and history trimming
- [x] A pipe tab fed by a filter with spread configured shows the same context lines as the filter view for the same hits
- [x] Full solution builds and the test suite stays green

## Rollover shifting moves into the module

**What to build:** After a multi-file rollover, previously collected filter results still point at the right content: the module gains the shift operation (shift result and hit line numbers by the rollover offset, drop lines that rolled off the start, rebuild the duplicate-suppression history from the tail of the shifted results) with its own test rows. The Log Window's rollover handler delegates, keeping only its lock and GUI refresh.

**Blocked by:** Filter Spread module in Core with table tests; multi-threaded filter delegates.

- [x] Shift operation is pure and lives in the module alongside expansion and trim
- [x] Tests cover: plain shift, lines dropping out when the offset exceeds them, history rebuilt from the result tail, results shorter than the history window
- [x] Log Window rollover handling delegates; no shift arithmetic remains inline
- [x] Full solution builds and the test suite stays green

## Contract: delete Log Window spread remnants, pin the knob, add glossary entry

**What to build:** The Log Window carries no spread logic or constants of its own. Its private spread-expansion copy and local spread constant are deleted; the UI spread knobs' maximum references the module's constant so the knob range and the suppression window can never drift apart again; CONTEXT.md gains a canonical **Filter Spread** glossary entry.

**Blocked by:** Log Window serial filter delegates to the module; Filter Pipe processing delegates to the module; Rollover shifting moves into the module.

- [ ] No spread expansion, trim, or shift arithmetic remains anywhere in the Log Window
- [ ] The spread knobs' maximum value references the module's constant (99)
- [ ] CONTEXT.md defines Filter Spread (back spread / fore spread context expansion) in the project's glossary voice
- [ ] Full solution builds and the test suite stays green
