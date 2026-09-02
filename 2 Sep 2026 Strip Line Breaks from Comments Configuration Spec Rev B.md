# Strip Line Breaks from All Comments Configuration Specification

## Purpose

The Visual Commander command `VCmd.CCommandStripLineBreaksFromAllComments` does not accept command arguments through `EnvDTE.ExecuteCommand`. Runtime behavior is therefore supplied through a command-specific JSON sidecar file.

The sidecar is a **one-run input handoff**, not persistent command state. A Package Manager Console script writes the desired values, invokes the Visual Commander command **without** an `arguments` value, and the command consumes those values for that invocation only.

## Configuration file

Canonical path:

```text
%LOCALAPPDATA%\xyLOGIX, LLC\Visual Commander\Commands\Strip Line Breaks from All Comments\Config\.config.json
```

The filename is exactly:

```text
.config.json
```

If the directory or file does not exist, the command creates the directory and writes the default configuration before reading it.

## Read-only invocation contract

For purposes of command behavior, the configuration is **read-only** during an invocation:

1. Near the beginning of `Run`, the command reads `.config.json` once into a new `C.StripLineBreaksFromAllCommentsConfig` instance.
2. That in-memory snapshot is used for the entire invocation. The command does not write changed runtime values back to the file while processing is in progress.
3. At the **outermost end of every `Run` invocation**, the command forcibly overwrites `.config.json` with a freshly-constructed default `StripLineBreaksFromAllCommentsConfig`.
4. The reset is executed from the outermost `finally` block, so it applies to successful completion, user cancellation, ordinary early-return paths, and exception-unwinding paths that leave `Run` normally through .NET control flow.
5. A script must therefore write its desired configuration **before each invocation** for which non-default behavior is required.

The only command-originated writes to `.config.json` are:

- first-use creation of the missing file with default values; and
- the unconditional end-of-run clobber back to default values.

A script must not write a configuration intended for a future invocation while a current invocation is still running, because the current invocation's final reset will intentionally overwrite it.

As with any `finally`-based cleanup, abrupt termination of the Visual Studio process or operating system can prevent finalization from executing. Under normal command execution, however, the reset is unconditional.

## Schema version 2

Default configuration:

```json
{
  "SchemaVersion": 2,
  "SuppressPrompts": false,
  "EnableGitAwareness": true,
  "AutomaticallyCheckInChangesToGitWhenGitAwarenessIsSuppressed": false
}
```

### Properties

| Property | Type | Default | Meaning |
| --- | --- | --- | --- |
| `SchemaVersion` | integer | `2` | Identifies the configuration schema. Version `2` establishes the unconditional end-of-run reset contract. |
| `SuppressPrompts` | Boolean | `false` | Suppresses the selected-project/Solution-wide processing confirmation and the late Git check-in confirmation. Intended for scripted/noninteractive invocation. |
| `EnableGitAwareness` | Boolean | `true` | Permits the **pre-formatting** Git workflow when the command's safety policy also permits it. It does not override the rule that pre-formatting Git awareness is suppressed when one or more user-visible documents were open when the command began. |
| `AutomaticallyCheckInChangesToGitWhenGitAwarenessIsSuppressed` | Boolean | `false` | When `SuppressPrompts` is `true` and pre-formatting Git awareness was not used, automatically performs the late Git check-in workflow after formatting/cleanup. The late workflow commits and pushes when appropriate, but deliberately does **not** pull because source changes have already been made. |

`ResetToDefaultsAfterRead` was removed in schema version 2. Resetting is no longer configurable: the command always restores the default file at the end of every invocation. A schema-version-1 file that still contains `ResetToDefaultsAfterRead` remains harmless because unknown properties are ignored.

## Git behavior

Pre-formatting Git awareness can run only when all of the following are true:

1. `EnableGitAwareness` is `true`.
2. No user-visible document was open when the command began.
3. The selected processing scope has been authorized, either by the user answering **Yes** or by `SuppressPrompts` being `true`.

When pre-formatting Git awareness is active, the command may pull from a tracked upstream before changing files, then create granular commits and push after processing.

When pre-formatting Git awareness is suppressed, the command can still offer or perform a late check-in. Interactive invocations display the Question-icon Yes/No check-in prompt, defaulting to **No**. Noninteractive invocations use `AutomaticallyCheckInChangesToGitWhenGitAwarenessIsSuppressed` instead of displaying that prompt.

## Prompt behavior

With `SuppressPrompts` set to `false`:

- Selected-project processing displays the Exclamation-icon **Are You Sure?** confirmation with **No** as the default.
- Solution-wide processing displays the corresponding Exclamation-icon confirmation with **No** as the default.
- If pre-formatting Git awareness was suppressed, the command later asks whether the user wants to check the changes in to Git, using a Question icon and **No** as the default.

With `SuppressPrompts` set to `true`, none of those confirmation prompts is displayed.

Because the command unconditionally restores the configuration file to defaults at the end, a scripted `SuppressPrompts: true` invocation cannot cause a later interactive invocation to inherit prompt suppression.

## Recommended Package Manager Console usage

### Noninteractive formatting/cleanup only

```powershell
$configPath = Join-Path $env:LOCALAPPDATA 'xyLOGIX, LLC\Visual Commander\Commands\Strip Line Breaks from All Comments\Config\.config.json'
New-Item -ItemType Directory -Force -Path (Split-Path -Parent $configPath) | Out-Null

@'
{
  "SchemaVersion": 2,
  "SuppressPrompts": true,
  "EnableGitAwareness": false,
  "AutomaticallyCheckInChangesToGitWhenGitAwarenessIsSuppressed": false
}
'@ | Set-Content -LiteralPath $configPath -Encoding UTF8

$dte.ExecuteCommand('VCmd.CCommandStripLineBreaksFromAllComments')
```

When the command returns, `.config.json` has been clobbered back to the default configuration automatically.

### Noninteractive run that checks changes in to Git

This configuration uses the safest available Git mode for the invocation context:

- If zero user-visible documents are open, pre-formatting Git awareness is permitted and may pull before changes are made.
- If one or more user-visible documents are open, pre-formatting Git awareness remains suppressed and the configured late check-in runs without a pull.

```powershell
$configPath = Join-Path $env:LOCALAPPDATA 'xyLOGIX, LLC\Visual Commander\Commands\Strip Line Breaks from All Comments\Config\.config.json'
New-Item -ItemType Directory -Force -Path (Split-Path -Parent $configPath) | Out-Null

@'
{
  "SchemaVersion": 2,
  "SuppressPrompts": true,
  "EnableGitAwareness": true,
  "AutomaticallyCheckInChangesToGitWhenGitAwarenessIsSuppressed": true
}
'@ | Set-Content -LiteralPath $configPath -Encoding UTF8

$dte.ExecuteCommand('VCmd.CCommandStripLineBreaksFromAllComments')
```

Again, the file is restored to the default configuration when `Run` exits.

## Serialization and compatibility

The configuration class is implemented as the nested `C.StripLineBreaksFromAllCommentsConfig` type. The command uses a small interface-based serializer/provider pair and does not require Newtonsoft.Json, `System.Text.Json`, or another NuGet package.

The command writes UTF-8 JSON without a BOM. UTF-8 files written by Windows PowerShell with a BOM are also readable.

Unknown properties are ignored. Missing or unreadable documented scalar values fall back to constructor defaults. Scripts should nevertheless emit valid JSON containing only the documented schema-version and Boolean properties.

The provider intentionally exposes `LoadOrCreate()` and `ResetToDefaults()` rather than a general-purpose configuration `Write(...)` method. This keeps persistence semantics aligned with the read-only-input contract: scripts supply runtime values; the command consumes them and then restores defaults.

## Deprecated command-argument mechanism

Do not use:

```powershell
$dte.ExecuteCommand('VCmd.CCommandStripLineBreaksFromAllComments', 'NoPrompt')
```

Visual Commander registers this command as not accepting arguments, so Visual Studio rejects the call before the command's `Run(DTE2, Package)` method receives control. Use `.config.json` followed by the argumentless `ExecuteCommand` call instead.
