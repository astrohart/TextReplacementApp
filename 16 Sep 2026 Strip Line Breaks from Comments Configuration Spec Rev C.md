# Strip Line Breaks from All Comments ??? Configuration Specification

## Purpose

The **Strip Line Breaks from All Comments** Visual Commander command reads a small JSON sidecar file once at the beginning of each invocation. The configuration is an input-only handoff mechanism for interactive and scripted execution.

The command implementation is compiled for C# 4.0 / .NET Framework 4.0. Source files processed by the command are assumed, by contract, to contain C# 7.3 syntax only.

## Canonical location

The configuration file is always named `.config.json` and is stored at:

```text
%LOCALAPPDATA%\xyLOGIX, LLC\Visual Commander\Commands\Strip Line Breaks from All Comments\Config\.config.json
```

If the directory or file does not exist, the command creates it using the default configuration before reading it.

## Schema version 3

The canonical default file is:

```json
{
  "SchemaVersion": 3,
  "SuppressPrompts": false,
  "EnableGitAwareness": true,
  "EnableCodeMaidAndReSharperCleanup": true,
  "AutomaticallyCheckInChangesToGitWhenGitAwarenessIsSuppressed": false
}
```

### `SchemaVersion`

Type: `integer`

Default: `3`

Identifies the configuration schema written by the current command.

### `SuppressPrompts`

Type: `boolean`

Default: `false`

When `false`, interactive Yes/No confirmation prompts are shown where applicable.

When `true`, confirmation prompts are suppressed for non-interactive/scripted execution. Other configuration properties then directly determine the applicable behavior.

### `EnableGitAwareness`

Type: `boolean`

Default: `true`

Permits the pre-formatting Git-aware workflow when the command's existing invocation-safety rules also permit it. In particular, Git awareness remains suppressed when the invocation begins with user-visible documents open.

This setting does not force Git awareness when another command policy prohibits it.

### `EnableCodeMaidAndReSharperCleanup`

Type: `boolean`

Default: `true`

This is the master gate for the CodeMaid/ReSharper cleanup phase.

When `false`:

- no CodeMaid/ReSharper cleanup confirmation is displayed;
- CodeMaid cleanup is skipped;
- `ReSharper.ReSharper_SilentCleanupCode` is skipped;
- `ReSharper.ReSharper_SilentCleanupOpenFiles` is skipped; and
- `AssemblyInfo.cs` cleanup-only processing is skipped.

When `true` and `SuppressPrompts` is `false`, comment line-break processing completes first and the command displays a Visual Studio-owned Yes/No question asking whether CodeMaid/ReSharper cleanup should run. The Question icon is used and **No is the default button**. Cleanup runs only if the user selects Yes.

When `true` and `SuppressPrompts` is `true`, the cleanup phase runs without asking the question. This makes the property directly usable by a `.ps1` script.

After cleanup completes, the command performs a second source-formatting verification pass over the processing scope. Any eligible comment line breaks reintroduced by CodeMaid/ReSharper are stripped again before Git completion/check-in. This post-cleanup pass does not invoke CodeMaid/ReSharper a second time.

### `AutomaticallyCheckInChangesToGitWhenGitAwarenessIsSuppressed`

Type: `boolean`

Default: `false`

For a non-interactive invocation in which normal pre-formatting Git awareness is suppressed, controls whether the command performs its late Git check-in workflow after source processing. The late workflow never pulls because source changes already exist by that point.

## One-run lifetime and forced reset

Configuration is read once near the beginning of `Run` and treated as read-only for the duration of that invocation.

The command does **not** persist the values it read. At the outermost end of every run, including normal completion, cancellation, an early return, or exception unwinding, the command forcibly overwrites `.config.json` with a newly constructed default configuration.

Consequently, script settings such as `SuppressPrompts: true` cannot leak into a later interactive invocation.

## Interactive default behavior

With the canonical default configuration:

1. Normal source-processing confirmations are displayed where required by scope.
2. Git awareness is permitted subject to the existing open-document/invocation policy.
3. Comment line breaks are processed.
4. The command asks whether CodeMaid/ReSharper cleanup should run. No is the default answer.
5. If cleanup runs, the command re-verifies/reapplies comment line-break formatting afterward so cleanup cannot reintroduce the line breaks as the final source state.
6. When pre-formatting Git awareness was suppressed, the existing post-processing Git check-in question is displayed.
7. The configuration file is reset to the canonical defaults before `Run` exits.

## Scripted format-only example

A Package Manager Console `.ps1` script that wants comment processing only, with no prompts, cleanup, or Git activity, can write:

```json
{
  "SchemaVersion": 3,
  "SuppressPrompts": true,
  "EnableGitAwareness": false,
  "EnableCodeMaidAndReSharperCleanup": false,
  "AutomaticallyCheckInChangesToGitWhenGitAwarenessIsSuppressed": false
}
```

Then invoke the Visual Commander command with no command argument, for example:

```powershell
$dte.ExecuteCommand('VCmd.CCommandStripLineBreaksFromAllComments')
```

The command consumes that configuration for the invocation and overwrites the file with defaults before returning.

## Scripted formatting plus cleanup example

To run comment formatting and CodeMaid/ReSharper cleanup non-interactively while leaving Git disabled:

```json
{
  "SchemaVersion": 3,
  "SuppressPrompts": true,
  "EnableGitAwareness": false,
  "EnableCodeMaidAndReSharperCleanup": true,
  "AutomaticallyCheckInChangesToGitWhenGitAwarenessIsSuppressed": false
}
```

Because prompts are suppressed and cleanup is enabled, cleanup runs without a Yes/No question.

## Compatibility with earlier configuration files

If an older configuration file does not contain `EnableCodeMaidAndReSharperCleanup`, deserialization starts from the current constructor defaults, so the missing property evaluates to `true`. At the end of that invocation, the file is replaced by the complete schema-version-3 default configuration.