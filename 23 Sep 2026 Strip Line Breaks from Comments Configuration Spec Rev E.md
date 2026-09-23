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

When `false`, no CodeMaid/ReSharper cleanup confirmation is displayed and no CodeMaid or ReSharper cleanup command is executed. `AssemblyInfo.cs` cleanup-only processing is skipped as well.

When `true` and `SuppressPrompts` is `false`, comment line-break processing completes first and the command displays a Visual Studio-owned Yes/No question asking whether CodeMaid/ReSharper cleanup should run. The Question icon is used and **No is the default button**. Cleanup runs only if the user selects Yes.

When `true` and `SuppressPrompts` is `true`, the cleanup phase runs without asking the question. This is the one-shot configuration path used by a `.ps1` script. The cleanup implementation is identical to the confirmed interactive path; suppressing prompts does not select a bulk/open-document cleanup algorithm.

When cleanup runs, the command iterates the complete resolved `processingFilePaths` scope one source file at a time. For each file it opens or activates that exact file in the Visual Studio code editor and allows the active-document context to settle before invoking cleanup commands. Files that were already open remain open; files opened only for cleanup are saved and then closed by the command.

For each non-`AssemblyInfo.cs` source file, the cleanup sequence is:

1. activate the source file;
2. wait for and invoke `CodeMaid.CleanupCode` against the active document;
3. pump Visual Studio messages for a settling interval;
4. independently wait for and invoke ReSharper silent cleanup against the active file, preferring `ReSharper_SilentCleanupCode` and falling back to `ReSharper.ReSharper_SilentCleanupCode`;
5. save the active document.

The CodeMaid and ReSharper attempts are intentionally independent. Failure, unavailability, or an exception from CodeMaid MUST NOT prevent the ReSharper silent-cleanup attempt. The command waits for cleanup commands to become available while pumping Visual Studio messages so an extension that temporarily disables a command while the editor is settling does not cause the subsequent cleanup step to be silently skipped.

`AssemblyInfo.cs` continues to bypass the command's comment-line-break transformation and CodeMaid cleanup; when cleanup is enabled it receives active-file ReSharper silent cleanup followed by a save.

The command MUST NOT use `CodeMaid.CleanupOpenCode`, `CodeMaid.CleanupAllCode`, or `ReSharper.ReSharper_SilentCleanupOpenFiles` for this cleanup phase. The cleanup scope is established by the command's own `processingFilePaths` list and is processed deterministically one activated source file at a time rather than depending on whichever documents happen to be open.

When cleanup runs, CodeMaid/ReSharper is the final source-formatting stage for the invocation. The command MUST NOT reapply comment-line-break formatting after cleanup. Any subsequent Git prompt, Git preparation, commit, push, cancellation, or decline path must preserve the exact editor/source state produced by cleanup and must not rewrite source merely to re-establish the pre-cleanup comment-line-break invariant.

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
5. If cleanup runs, every in-scope source file is activated and cleaned one file at a time: CodeMaid active-file cleanup first (except `AssemblyInfo.cs`), then ReSharper active-file silent cleanup, then save. A CodeMaid failure does not suppress ReSharper.
6. CodeMaid/ReSharper becomes the final source-formatting authority for the invocation. No comment-line-break formatting pass runs afterward; later Git control flow must not mutate the cleaned source state.
7. When pre-formatting Git awareness was suppressed, the existing post-processing Git check-in question is displayed.
8. The configuration file is reset to the canonical defaults before `Run` exits.

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