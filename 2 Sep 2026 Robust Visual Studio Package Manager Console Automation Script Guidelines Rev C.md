# Robust Visual Studio Package Manager Console Automation Script Guidelines

## Purpose

This document is a reusable engineering specification for an AI system that generates **agentic PowerShell Change Transaction Scripts intended to be dot-sourced from the Visual Studio Package Manager Console (PMC)**.

A Change Transaction Script is an **in-IDE, repository-aware maintenance transaction**. It is used after the AI has inspected the current authoritative workspace and determined a concrete desired change, and the maintainer wants one downloadable `.ps1` artifact to impose that change inside the already-loaded Visual Studio Solution, prepare the resulting changed source for the normal Visual Studio/VCmd cleanup workflow, capture the result in Git, and leave the workspace in a useful state without forcing the maintainer to debug the automation itself.

Typical use cases include:

- clobbering one or more existing source/project files with audited desired-state replacements for a bug fix, behavioral correction, refactor, logging change, documentation change, UI adjustment, or configuration change;
- applying a coordinated multi-file change while keeping ordinary implementation commits file-by-file unless an explicit source-family, rename, scaffold, or topology exception applies;
- updating WinForms source and `*.Designer.cs` files while ensuring cleanup opens them as source text rather than activating the WinForms Designer;
- adding required project/assembly/package references without policing unrelated existing references;
- creating complete repository-standard project/module scaffolds and adding them to the loaded Solution through DTE;
- performing project/Solution topology operations such as renames when the task genuinely requires them;
- retrying a prior partially completed transaction without treating harmless source divergence, formatting changes, or an orphaned empty transaction boundary as a reason to fail;
- opening the complete changed text/source set, supplying the one-run noninteractive/Git-disabled configuration for `VCmd.CCommandStripLineBreaksFromAllComments`, running that cleanup pass without modal prompts or VCmd-owned Git activity, and saving the IDE state before the script's own Git capture; and
- staging, committing, synchronizing, and pushing transaction-owned work without allowing unrelated paths to hitchhike.

These scripts are **not** intended to be general-purpose CI/CD pipelines, substitute compilers, build validators, test harnesses, source analyzers, or autonomous architectural reviewers. They are controlled maintainer-side change vehicles: the AI performs the source/code reasoning before delivery; the script performs the mechanical transaction inside the maintainer's current Visual Studio session.

The design target is deliberately operational rather than theoretical. A successful script should normally be something the maintainer runs once, watches through concise `Write-Host` progress messages, and then reviews in Visual Studio/Git. It should not require the maintainer to become the human debugger of generated PowerShell.

The goal is simple:

> A user should not have to act as the runtime debugger for an AI-generated PMC script.

### Operating authority and source-of-truth rules

When generating a Change Transaction Script, reconcile the current task against the current repository and workspace rather than relying on an older remembered snapshot.

Use this precedence for transaction behavior:

1. The user's current prompt.
2. The most recent project handoff when it contains transaction-specific history or corrections.
3. The current authoritative workspace/tarball and repository conventions.
4. This reusable document.

When a current tarball or workspace snapshot is supplied, treat it as authoritative over earlier tarballs. Explicit user corrections to live workspace state after that tarball supersede the tarball for the specifically identified paths.

Repository-specific commit-message instructions govern commit-message formatting. Current repository engineering guidance and the current xyLOGIX Software Engineering Manifesto govern source architecture and coding conventions.

### Generation-time audit and runtime clobbering rule

A Change Transaction Script is, by design, a **clobbering transaction** for the source/project targets that the current transaction authorizes.

The AI must do the source-shape reasoning **before delivery**, against the current authoritative workspace/tarball and any explicit live-state corrections supplied by the user. From that audit, generate the intended desired-state payloads and transaction operations.

At runtime, once the script has established the correct Solution/repository and resolved an authorized target path:

- assume the live source/project file is the transaction input that was already supplied and audited during generation;
- impose the transaction's intended desired state directly;
- prefer complete exact full-file payload replacement whenever the target can be deterministically modeled;
- do **not** reread, parse, regex-match, search for methods/markers, hash-check, compare formatting/layout, AST-compare, or otherwise inspect the existing source text merely to decide whether the script is allowed to overwrite it;
- do **not** require the live file to retain the exact method shape, whitespace, comments, XML documentation, line wrapping, or old code block seen during generation; and
- do **not** invent a runtime source-shape precondition that can turn an otherwise straightforward overwrite into a fatal error.

A newer authoritative workspace/tarball or an explicit current-prompt correction supersedes an older generation-time snapshot. Otherwise, for an already-authorized target, the transaction's desired-state payload intentionally wins at runtime.

This rule does **not** eliminate mechanical safety checks that are actually needed to know where the transaction is operating: Solution identity, repository identity, authorized target pathname, Git availability/cleanliness, filesystem writability, required tool availability, exact staging scope, and genuine topology/ownership ambiguity remain valid concerns. The distinction is that **source contents are not a runtime permission gate for a clobbering write**.

The script must assume that real development environments contain directory junctions, partially completed prior runs, open editor documents, older PowerShell binding behavior, COM/DTE quirks, generated files that must not be touched, and legitimate no-op conditions. Every consequential action must therefore have a direct reason to occur and a meaningful postcondition. Runtime preconditions are appropriate only when they determine whether the next mechanical action is possible or safe; current source bytes, hashes, formatting, layout, comments, or semantic equivalence to an earlier snapshot are **not** transaction preconditions.

The maintainer's normal operating policy is to commit all preexisting work before running a Change Transaction Script. Accordingly, the default transaction precondition is a clean Git working tree and clean Git index in every repository the transaction will mutate. A current prompt may explicitly authorize a different starting state, but scripts must not silently absorb unrelated dirty work merely because it exists.

A Change Transaction Script is also presumed to generate source and project state that build correctly. The transaction itself must therefore not use a Visual Studio build, MSBuild invocation, compilation result, or test run as a fatal verification gate. By default, do not invoke a build merely to prove that generated code compiles. If a build or test is explicitly requested for informational purposes, report its outcome without throwing, aborting, rolling back source, or rewriting transaction-owned Git history solely because that build or test failed.

### Progress-first / non-blocking transaction policy

The maintainer explicitly prefers rapid forward progress over defensive self-validation.

Accordingly:

- **Design the script to keep moving and to avoid dying unless execution truly cannot continue mechanically. Do not manufacture failure opportunities through defensive source-shape validation.**
- **Assume positive source-code and project-file modification actions completed correctly once issued successfully.**
- **Once an authorized source/project target is identified, overwrite/clobber it with the transaction's intended state without requiring its current bytes, hash, formatting, layout, comments, method shape, markers, or semantics to match an earlier snapshot or generated payload.**
- **When a complete desired file can be generated ahead of time, prefer that exact full-file payload over runtime structural searching or method-body replacement.**
- **Linting, formatting/style diagnostics, static analysis, and similar advisory checks are warning-only. They must never throw, abort, restore files, remove commits, reset history, or otherwise block forward progress.**
- Do not convert source/project verification into a reason to terminate the transaction.
- Do not automatically restore source/project files because a later check reports an unexpected state.
- Do not automatically erase transaction-created commits because a later source/project correctness check reports an unexpected state.
- When a nonessential check detects something surprising, report it as `*** WARNING ***` and continue.
- Reserve `*** ERROR ***`/termination for situations where the script cannot mechanically continue at all, such as the requested Solution not being loaded, a required target path being impossible to identify, Git being unavailable when Git operations are required, or an underlying mutation command itself throwing before the requested action can be issued.
- Even for such infrastructure failures, do not perform broad source/history rollback unless the current prompt specifically asks for rollback behavior.
- **Emit useful, concise transaction diagnostics through `Write-Host` at meaningful phase and decision boundaries so the maintainer can tell what the script is doing without being flooded by low-level native-tool chatter.**

The normal remediation loop is intentionally simple:

1. Run the transaction.
2. Preserve the resulting changes.
3. Let the maintainer inspect the IDE/ReSharper/CodeMaid/Git state.
4. Address any defect with the **next** Change Transaction Script.

---

## 1. Core Operating Principle

The primary objective of a Change Transaction Script is to **make forward progress**. It is not a substitute for the maintainer's IDE inspections, ReSharper analysis, CodeMaid cleanup review, reference analysis, compilation feedback, or subsequent engineering judgment.

For positive source/project modification actions, use this model:

1. **Establish enough mechanical context to target the intended Solution/repository/path.**
2. **Impose the pre-audited desired state, normally by clobbering the authorized file with its complete exact payload.**
3. **Assume the mutation succeeded once the underlying operation returned normally.**
4. **Record that meaningful positive mutation has occurred and continue the transaction.**
5. **Let the maintainer identify any remaining problem and address it with the next Change Transaction Script.**

The runtime script is not expected to rediscover the source shape that the generator already audited. If an exact payload can be supplied, a helper that first has to locate a particular method body, old code block, marker, declaration layout, or regex match before it can write is the wrong default design.

Do not turn source-code correctness, project-reference cleanliness, formatting, semantic expectations, compilation expectations, generated-file expectations, or architectural preferences into runtime blockers.

In particular:

- Do not add speculative correctness gates before or after writing source.
- Do not mechanically verify old source text merely to authorize a clobbering write.
- Do not fail because a source marker, regex, hash, comment, declaration, method signature/shape, old code block, or other semantic/textual check differs from an expectation.
- Do not roll back positive source/project changes merely because a later validation disagrees with them.
- Do not police project, assembly, or package references.
- Do not run builds, tests, or compiler checks as transaction gates.
- Do not semantically verify source after VCmd.
- Prefer a concise warning and continued execution over a thrown exception whenever continued execution remains mechanically possible.
- Keep the maintainer informed with concise `Write-Host` diagnostics for significant progress, no-op decisions, warnings, and failures.

The maintainer will review the resulting state in Visual Studio and provide the next correction if one is needed.

A legitimate no-op is success, not an error.

---

## 2. Target Runtime Assumptions

Assume the script runs inside the Visual Studio 2022 NuGet Package Manager Console with a live DTE/COM automation environment.

For the current DiagnosticBatchRunner workflow, the verified host/runtime baseline is:

- Visual Studio 2022 Enterprise 17.14.39.
- NuGet Package Manager Console Host 6.14.3.1.
- Windows PowerShell 5.1.26100.9168.
- `PSEdition` = `Desktop`.
- CLR 4.0.30319.42000.

The script is a **PMC/DTE transaction first**, but its source must parse and execute correctly under the underlying Windows PowerShell 5.1 Desktop engine. Write for the least surprising, most conservative behavior supported there; do not assume PowerShell 7 syntax or binder behavior.

When a future host reports a different `$PSVersionTable` or `$Host.Version`, prefer the actual host values supplied by the user over the historical version numbers above.

---

## 3. Script Scope and PMC Session Hygiene

Because the user normally dot-sources the script:

```powershell
. "C:\path\to\script.ps1"
```

wrap the entire implementation in a child scope:

```powershell
& {
    $ErrorActionPreference = 'Stop'

    try {
        # work
    }
    finally {
        # cleanup
    }
}
```

This prevents helper functions and ordinary local variables from permanently polluting the long-lived PMC runspace.

### Top-level transaction failure boundary

The outermost `try`/`catch` is the transaction failure boundary. A mechanically unrecoverable error should stop the **transaction**, report actionable diagnostics, perform only the cleanup that is safe for the transaction state, and then normally return control to the existing PMC runspace without rethrowing merely to make the console emit another terminating error.

A fatal transaction error therefore does **not** mean that the script should try to terminate Visual Studio, destroy the PMC session, or convert an already-diagnosed failure into additional host noise. In the top-level `catch`:

1. report the exception message, invocation position, and script stack trace when available;
2. if no meaningful positive mutation occurred, remove the transaction-owned empty boundary when the Section 15 conditions permit it;
3. if meaningful positive mutation occurred, preserve that progress unless an explicit rollback contract applies; and
4. return control to PMC after transaction-owned cleanup.

Rethrow only when the current prompt explicitly requires the exception to escape the transaction boundary or when the host cannot otherwise be left in a coherent state.

### Do

- Set `$ErrorActionPreference = 'Stop'` inside the child scope.
- Clear `$Error` at sensible boundaries if useful.
- Dispose `System.Diagnostics.Process`, cryptographic objects, and other disposable objects created by the script.
- Delete temporary files created by the script.
- Run garbage collection/finalizer cleanup only as a finishing hygiene measure when appropriate.
- Keep cleanup in `finally` blocks from obscuring the original failure.

### Do not

- Unload NuGet/PMC modules.
- Destroy or release the global `$dte` object.
- Erase PowerShell providers or host state indiscriminately.
- Use `$script:` variables unless persistent script-scope state is truly required.
- Assume session cleanup can repair malformed PowerShell source before parsing begins.

---

## 4. Encoding and PowerShell Source Integrity

Encoding is not the main engineering problem, but malformed source can prevent the script from executing at all.

Use a normal UTF-8 encoding that is known to work in the target PMC environment. When generating the file programmatically, avoid accidentally writing multiple BOM markers or embedding `U+FEFF` as literal source text before the first token.

The important rule is:

> Verify that the first PowerShell token is actually the intended token, rather than an invisible character followed by that token.

Do not obsess over encoding at the expense of runtime correctness, but do not ship a file whose first statement is lexically corrupted.

For temporary Git commit-message files, UTF-8 **without BOM** is a good default because Git and GitHub handle it predictably.

### 4.1 Exact payload transport for clobbering writes

When a complete desired-state file is generated ahead of time, transport the **exact audited bytes** into the Change Transaction Script in a way that cannot be damaged by PowerShell quoting, interpolation, here-string delimiters, newline conversion, or default encoding behavior.

For large or quote-rich source/project payloads, a proven robust pattern is:

1. generate the complete desired file at generation time using its intended BOM/encoding/newline convention;
2. Base64-encode those exact bytes before the `.ps1` artifact is delivered;
3. embed the Base64 text in the transaction keyed by the authorized repository-relative pathname; and
4. at runtime, decode with `[System.Convert]::FromBase64String(...)` and clobber the target with `[System.IO.File]::WriteAllBytes(...)`.

Base64 is a **payload transport mechanism**, not a runtime source verifier. Do not decode the live file, compare hashes, or compare old bytes before writing. The point is to make the replacement boring and deterministic.

A plain PowerShell string/here-string payload remains acceptable for small/simple files when its encoding and interpolation behavior are completely controlled. If a string payload creates any meaningful quoting, escaping, BOM, CRLF, or interpolation risk, prefer exact-byte/Base64 transport instead.

Before delivery, decode every embedded Base64 payload and verify that the resulting bytes exactly match the generation-time audited desired file. That check belongs to the generator's artifact audit, not to the runtime transaction.

---

## 5. Windows PowerShell 5.1 Compatibility Rules

### 5.1 Avoid generic-list-to-array binder traps

Do not build a `System.Collections.Generic.List[object]` and then return or coerce it with:

```powershell
@($genericList)
```

Windows PowerShell 5.1 can throw:

```text
Argument types do not match
```

Prefer ordinary PowerShell arrays:

```powershell
$items = @()
$items += $item
```

or, when repeated append performance matters, use non-generic `System.Collections.ArrayList` and explicitly discard `Add()` return values:

```powershell
$items = New-Object System.Collections.ArrayList
[void]$items.Add($item)
```

Also avoid coercing COM collections such as `$dte.Documents` through unnecessary `@(...)` wrappers. Enumerate them directly.

### 5.2 Compute bitwise enum options before constructor calls

Do not embed ambiguous `-bor` expressions directly inside overloaded constructor argument lists.

Prefer:

```powershell
$options = [System.Text.RegularExpressions.RegexOptions]::Multiline -bor
           [System.Text.RegularExpressions.RegexOptions]::Singleline
```

and then pass `$options` as one argument.

This avoids PowerShell parsing an argument list as an array and then attempting `op_BitwiseOr` on `System.Object[]`.

### 5.3 Avoid speculative COM enum string comparisons

Do not convert a DTE enum to a string and compare it to an enum member name unless that exact behavior has been verified.

For example, a COM enum may stringify as `"1"` even though its logical member is `dbgDesignMode`.

Better approaches are:

- Avoid the gate if it is not directly required for the action.
- Compare the actual enum/numeric value correctly if the gate is truly required.

Do not invent debugger/build-state checks just because they sound cautious. A false-positive safety gate is a reliability defect.

### 5.4 PowerShell regex escaping and commit-message validators

Remember that .NET regular expressions are still .NET regular expressions when their pattern text is stored in a PowerShell string. In a **single-quoted PowerShell string**, a regex word boundary is written `\b`, not `\\b`. The latter asks the regex engine to match a literal backslash followed by `b` and can reject valid text unexpectedly.

Do not build commit-message validation around brittle finite verb whitelists or finite past-tense-word whitelists. Repository commit-message instructions are authoritative and may require exact forms such as:

```text
Create <file name>
Update <file name>
```

Validation must implement the repository specification directly. In particular:

- a required single-file `Create <file name>` topline is valid by definition when the staged diff is exactly one added file;
- a required single-file `Update <file name>` topline is valid by definition when the staged diff is exactly one modified file;
- multi-file toplines must satisfy the repository's present-tense/sentence-case/length rules, but should not be rejected merely because their first verb is absent from a hand-maintained list; and
- body bullets must be checked for required structure and scope. Do not pretend a small hard-coded list of words is a complete English past-tense grammar.

Whenever practical, test any regex used by a transaction validator against the exact examples mandated by the repository instructions before delivery.

---

## 6. Visual Studio/DTE Discovery

### 6.1 `$dte` is host-provided, read-only, and must never be assigned

The Visual Studio NuGet Package Manager Console host provides `$dte` automatically. It refers to the exact Visual Studio instance whose Package Manager Console is executing the script. Treat it as a reserved host-owned input.

**Never assign, rebind, shadow, clear, remove, release, or otherwise write to `$dte` in any casing.** PowerShell variable names are case-insensitive. The prohibition includes function/script-block parameter binding, loop variables, destructuring targets, and `Set-Variable`/`New-Variable` targets.

Do not attempt to rediscover or attach to another DTE/devenv instance. If the host-provided object is unusable, fail with a precise diagnostic.

Reading or invoking it is correct, for example:

```powershell
$solutionFullName = [string]$dte.Solution.FullName
$dte.ExecuteCommand('File.SaveAll')
```

If a helper needs a local reference, use a differently named variable such as `$visualStudioAutomation`. Never use `dte` as the binding name.

### 6.2 Establish the loaded Solution identity

Before touching source, project state, or Git:

1. Verify `$dte` and `$dte.Solution`.
2. Verify `$dte.Solution.FullName` is nonblank.
3. Derive the Solution directory with `[System.IO.Path]::GetDirectoryName($dte.Solution.FullName)`.
4. Verify the Solution file and directory exist.
5. If the transaction targets a particular Solution, verify its expected filename.

Never derive Solution identity from the PMC current directory. The Solution directory is an identity/coordination anchor, not necessarily a containment boundary.

### 6.3 Discover loaded projects from DTE

Recursively enumerate the actually loaded project graph and use each project's real project-file pathname. Support Solution folders and loaded sibling/absolute-path projects. Do not reconstruct project paths from the Solution directory and project name when DTE can provide the authoritative pathname.

---

## 7. `File.SaveAll`: Required Flush Boundaries

`File.SaveAll` is one of the most important synchronization points between Visual Studio's in-memory state and the filesystem.

Call it through DTE:

```powershell
$dte.ExecuteCommand('File.SaveAll')
```

and optionally allow the IDE message loop to process pending work afterward:

```powershell
[System.Windows.Forms.Application]::DoEvents()
```

### Required checkpoint A: before Git observes the initial disk state

After DTE/Solution validation, run `File.SaveAll` before the first meaningful Git status/synchronization validation.

Reason:

- Git sees files on disk, not unsaved editor buffers.
- Project-system changes can also be pending in Visual Studio.
- The initial Git gate should evaluate the state the user actually sees in the IDE.

`File.SaveAll` does **not** require checking whether documents are open first.

### Required checkpoint B: after source/project mutations

After the script writes source files or changes project references, run `File.SaveAll` so Visual Studio/project-system state is flushed before editor cleanup and Git capture continue.

After this checkpoint, refresh the transaction-created changed-path set. Before executing VCmd, open every changed text/source-editable file in Visual Studio's **source-code/text editor**, explicitly avoiding the WinForms Designer or any other designer surface. The complete changed-file opening pass must finish before VCmd is invoked.

### Required checkpoint C: after VCmd and immediately before post-change Git capture

This is the critical final flush:

1. Perform the requested source/project changes.
2. Run `File.SaveAll` after those mutations.
3. Resolve the complete transaction-created changed-path set.
4. Open all changed text/source-editable files in the Visual Studio source-code/text editor; never activate the WinForms Designer.
5. After the opening pass is complete, write the mandatory one-run JSON sidecar described in Section 9 so `VCmd.CCommandStripLineBreaksFromAllComments` is noninteractive and its Git awareness/check-in behavior is disabled for this invocation.
6. Invoke `VCmd.CCommandStripLineBreaksFromAllComments` **without command arguments** only when the sidecar write succeeded. If the sidecar cannot be prepared, emit `*** WARNING ***` and skip VCmd rather than risking a modal prompt or VCmd-owned Git activity. If the VCmd command itself is unavailable or throws, report `*** WARNING ***` and preserve forward progress; cleanup-tool failure is not a reason to erase otherwise-successful source mutations.
7. Run `File.SaveAll` **unconditionally**, whether VCmd succeeded, was skipped, or reported a warning.
8. Close script-owned documents if the workflow requires isolation and documents are open.
9. Refresh Git status/dirty-scope state as required.
10. Only then allow Git staging/committing.

The final `File.SaveAll` must not depend on document dirty state or document count.

Do **not** semantically verify, reparse, regex-check, marker-check, or otherwise inspect the source code after VCmd cleanup. VCmd cleanup is a trusted editor-cleanup boundary. Once the VCmd command returns successfully and the final `File.SaveAll` completes, assume that VCmd performed its cleanup perfectly.

Do not run a build, compile, or test operation merely as a transaction-verification gate. The Change Transaction Script is presumed to have generated perfectly-building code. If the current prompt explicitly requests a build or test for informational purposes, report the result but do not throw, abort, or roll back solely because it fails.

---

## 8. Closing Visual Studio Documents Safely

Never execute `Window.CloseAllDocuments` blindly.

First inspect:

```powershell
$documentCount = [int]$dte.Documents.Count
```

### If the count is zero

Log that there is nothing to close and continue.

Do **not** call:

```powershell
$dte.ExecuteCommand('Window.CloseAllDocuments')
```

because Visual Studio may report the command as unavailable when it is meaningless in the current context.

### If one or more documents are open

Attempt the IDE command only when the transaction actually requires the relevant documents to be closed. If the command is unavailable, fall back to closing the actual document objects owned by the transaction.

Afterward, when closing was required, verify:

```powershell
[int]$dte.Documents.Count -eq 0
```

or otherwise verify that the specific transaction-owned documents intended to be closed are no longer open.

If required transaction-owned documents remain open when isolation is necessary, stop rather than pretending cleanup succeeded.

### Failure cleanup ownership

If the script opens documents specifically for cleanup and later fails, cleanup should focus on the documents/state the script took ownership of. Do not casually destroy unrelated user editor state during exception handling.

The fact that changed files are intentionally opened before VCmd does not authorize a blanket close of documents that were already open before the transaction.

---
## 9. Changed-File Editor Opening and VCmd / Visual Commander Cleanup

When the repository workflow requires:

```text
VCmd.CCommandStripLineBreaksFromAllComments
```

use the following discipline.

### Open the complete changed-file set first

After source/project mutations and the required `File.SaveAll`, resolve the complete set of files changed by the transaction. For every changed file that Visual Studio can meaningfully open as text/source, open it **before** executing VCmd.

The purpose of this opening pass is deliberate: VCmd automatically strips line breaks from comments and formats the files it sees open in the editor. Therefore the transaction must finish opening the changed files first and invoke VCmd only afterward.

Do not restrict the opening set merely because a file is:

- named `GlobalAspects.cs` or otherwise begins with `Global`;
- named `AssemblyInfo.cs`;
- a `*.Designer.cs` source file;
- a generated/derived C# text file such as `*.g.cs` or `*.i.cs`; or
- another changed text/source artifact that is part of the authorized transaction.

If the transaction changed such a text/source file, it belongs in the editor-opening pass unless Visual Studio cannot mechanically open it as text.

Binary artifacts such as signing keys, icons, compiled outputs, or other non-text files cannot be meaningfully opened in the source-code editor. Treat those as informational skips, report the skip through `Write-Host` when useful, and continue. Do not manufacture an editor failure merely to satisfy an impossible text-view operation.

### Always force the source-code/text editor; never the WinForms Designer

Opening a changed file must not activate the WinForms Designer or another designer surface. This is especially important for a WinForms form/control primary `.cs` file and its `*.Designer.cs` companion, because the default project-item view can otherwise select a designer.

Use an explicit source/text/code view through DTE, such as the text-view kind or another verified mechanism that opens the file as text, rather than relying on the default project-item view. A DTE text-view kind that has been proven reliable in this workflow is:

```text
{7651a700-06e5-11d1-8ebd-00a0c90f26ea}
```

When using `$dte.ItemOperations.OpenFile(...)`, pass an explicit text/source view kind rather than a default view for WinForms-related paths.

The rule is:

> Changed files are opened for VCmd in the source-code/text editor only. The transaction must never open the WinForms Designer merely to prepare editor cleanup.

### Only open real changed files

For each changed-path candidate:

1. Verify the path belongs to the transaction's authorized changed set.
2. Verify the file exists when the status represents an existing file rather than a deletion/rename-away path.
3. Determine whether it is text/source-editable in Visual Studio.
4. Attempt to open it explicitly in the source-code/text editor.
5. Count only the files that actually opened successfully.
6. If one file cannot be opened mechanically, emit `*** WARNING ***` with the filename/error and continue opening the rest; do not make one editor-open failure fatal to the source transaction.
7. Emit useful `Write-Host` diagnostics identifying each file opened or explaining a legitimate non-text/deleted/unopenable skip.

Do not open unrelated files and do not open an arbitrary document merely to test whether VCmd is registered or executable.

### Mandatory noninteractive/Git-disabled VCmd configuration

Every Change Transaction Script that invokes `VCmd.CCommandStripLineBreaksFromAllComments` must supply the command's one-run JSON sidecar immediately before that invocation. This keeps VCmd in the formatting/cleanup-only role: no confirmation message box(es), no VCmd-owned Git pull/check-in/push behavior, and no competition with the Change Transaction Script's own staged-diff commit workflow.

The canonical configuration pathname is:

```text
%LOCALAPPDATA%\xyLOGIX, LLC\Visual Commander\Commands\Strip Line Breaks from All Comments\Config\.config.json
```

The filename is exactly `.config.json`.

For Change Transaction Scripts, the required schema/version and values are exactly:

```json
{
  "SchemaVersion": 2,
  "SuppressPrompts": true,
  "EnableGitAwareness": false,
  "AutomaticallyCheckInChangesToGitWhenGitAwarenessIsSuppressed": false
}
```

These values have specific transaction semantics:

- `SuppressPrompts: true` prevents the selected-project/Solution-wide processing confirmation and the late Git check-in confirmation from becoming modal UI.
- `EnableGitAwareness: false` prevents VCmd from running its pre-formatting Git workflow. The Change Transaction Script remains solely responsible for pull/rebase, staging, commit-message generation, commits, and push.
- `AutomaticallyCheckInChangesToGitWhenGitAwarenessIsSuppressed: false` prevents VCmd from performing a late automatic Git check-in after cleanup.
- `SchemaVersion: 2` uses the command's one-run configuration contract. When the command's `Run` invocation exits normally through .NET control flow, VCmd itself clobbers `.config.json` back to its default configuration, so a scripted noninteractive invocation does not intentionally make prompt suppression persistent.

A PowerShell 5.1-compatible preparation/invocation shape is:

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

The command must be invoked argumentlessly. Do **not** use a command-argument workaround such as:

```powershell
$dte.ExecuteCommand('VCmd.CCommandStripLineBreaksFromAllComments', 'NoPrompt')
```

Visual Commander does not expose this command as accepting command arguments; the JSON sidecar is the supported runtime handoff.

Treat successful sidecar preparation as an immediate mechanical precondition **only for invoking VCmd**, not for the source transaction as a whole. If the directory or `.config.json` cannot be written, emit `*** WARNING ***`, skip VCmd, perform the unconditional final `File.SaveAll`, and continue to the script-controlled Git phase. Never invoke VCmd without the known noninteractive/Git-disabled sidecar merely because formatting cleanup is desirable.

Because the sidecar is a one-run input and VCmd resets it at the end of each normal invocation, write these values again **before every VCmd invocation**. Do not write configuration intended for a later invocation while an earlier invocation is still running.

### Execute VCmd only after the opening pass completes

If zero changed text/source-editable files can be opened, skip VCmd and report the legitimate no-op through `Write-Host`.

If one or more changed text/source-editable files are open:

1. Complete the requested source/project mutations without runtime semantic/hash/layout verification.
2. Run `File.SaveAll`.
3. Resolve the complete transaction-created changed-file set.
4. Open **all** changed text/source-editable files in the source-code/text editor, never the WinForms Designer.
5. Confirm the opening pass itself completed mechanically; do not semantically inspect the source.
6. Write the mandatory one-run `.config.json` sidecar with the exact Section 9 noninteractive/Git-disabled values.
7. If the sidecar write succeeded, emit a `Write-Host` diagnostic that VCmd is about to run noninteractively against the already-open changed files with VCmd Git behavior disabled. If the sidecar write failed, emit `*** WARNING ***` and skip VCmd.
8. When configured successfully, attempt the **argumentless** `VCmd.CCommandStripLineBreaksFromAllComments` invocation once. If the command is unavailable or throws, catch that cleanup failure, emit `*** WARNING ***`, and continue.
9. Process pending IDE events if useful.
10. Run `File.SaveAll` again unconditionally.
11. Close only transaction-owned documents when the workflow requires it; do not indiscriminately close unrelated documents.
12. Continue without any post-VCmd semantic source verification.

VCmd is trusted when it executes successfully, but **VCmd availability is not a source-mutation correctness gate**. The mandatory sidecar keeps VCmd noninteractive and disables both its pre-formatting Git awareness and its late automatic check-in path, so all Git synchronization, staging, custom commit-message generation, commits, and push operations remain owned by the Change Transaction Script. After a successful VCmd invocation and the required final `File.SaveAll`, do not re-read the source to validate identifiers, signatures, comments, markers, regular-expression matches, hashes, formatting, or other semantic/textual expectations. If sidecar preparation or VCmd itself fails, report the warning, perform the final `File.SaveAll`, preserve the changed source, and continue to Git capture rather than rolling back the transaction.

Do not use a Visual Studio build, MSBuild invocation, compilation result, or test run as a post-VCmd transaction gate. A build/test failure must never, by itself, cause the transaction to throw, die, roll back source/project changes, remove commits, or reset the repository. If the current prompt expressly asks for a build or test to be run, treat it as informational only and continue the transaction after reporting the result.

---

## 9.1 Build, Compilation, and Test Policy

Change Transaction Scripts are presumed to create perfectly-building code and coherent project state.

### Default behavior

Do **not** invoke any of the following merely to validate the transaction:

- Visual Studio project or Solution builds through DTE.
- `MSBuild.exe`.
- `dotnet build`.
- `devenv /Build`.
- compiler executables.
- NUnit or other test runners.

The transaction's fatal runtime gates are limited to conditions required to continue mechanically and safely: authoritative Solution/repository/path identity, required Git availability for a Git-integrated transaction, clean initial Git scope, ability to issue the authorized mutation, and exact Git staging/commit postconditions. Changed-file source-editor opening, preparation of the noninteractive/Git-disabled VCmd sidecar, and VCmd cleanup are **best-effort cleanup operations**: failures are diagnosed as warnings and do not invalidate successful source/project mutations. A sidecar-preparation failure means **skip VCmd**, not invoke it interactively. Source-byte/hash/layout/semantic equivalence and lint/style/static-analysis results are not runtime gates.

### If a build or test is explicitly requested

A current prompt may expressly ask the script to invoke a build, compilation, or test operation. In that case:

1. Run it only after the source/project mutation and required changed-file editor-opening/VCmd/SaveAll work are complete.
2. Capture and report the outcome.
3. Do not throw merely because it failed.
4. Do not abort the transaction merely because it failed.
5. Do not restore source/project bytes merely because it failed.
6. Do not reset Git history merely because it failed.
7. Do not remove the transaction boundary or implementation commits merely because it failed.
8. Continue with the normal Git capture/synchronization flow unless some independent transaction invariant has actually failed.

A build/test operation can reveal information useful to the maintainer, but it is not an agentic rollback trigger.

---

## 9.2 Linting, Formatting, and Static-Analysis Policy

Linting, formatting/style diagnostics, ReSharper/CodeMaid analysis, compiler-like static analysis, and similar advisory tooling must never be fatal transaction gates.

### Default behavior

Do not invoke a linter, formatter verifier, style checker, or static analyzer merely to prove that generated source is acceptable. Source quality is a generation-time responsibility and the maintainer's IDE remains the authoritative review environment.

The required VCmd pass is not a speculative formatting verifier. It is an intentional editor-cleanup action performed after the transaction opens the complete changed text/source file set in the source-code editor.

### If explicitly requested for information

If the current prompt expressly asks the transaction to run such a tool:

1. Run it only as an informational/advisory operation.
2. Capture and summarize its result.
3. Convert nonzero exit codes, warnings, style findings, and formatting differences to `*** WARNING ***` diagnostics.
4. Continue the transaction.
5. Do not throw, abort, restore source/project bytes, remove commits, reset Git history, or suppress later staging/committing solely because the lint/style/static-analysis operation reported a problem.

A linting or style result can guide the next Change Transaction Script; it cannot invalidate forward progress in the current one.

---

## 10. Git Must Be Invoked Quietly

PMC can misinterpret native Git stdout/stderr as PowerShell errors or otherwise clutter the console.

Do not invoke Git directly when the script depends on clean diagnostic behavior.

Run Git through `System.Diagnostics.Process` with:

- `UseShellExecute = $false`
- `CreateNoWindow = $true`
- `RedirectStandardOutput = $true`
- `RedirectStandardError = $true`

Start asynchronous reads of **both** redirected streams immediately after successful process creation, before waiting for process exit. A proven PowerShell 5.1-compatible pattern is to call `ReadToEndAsync()` for stdout and stderr, then perform a bounded `WaitForExit(...)`, and finally consume the two task results. This localized asynchronous I/O exists only to drain both redirected pipes concurrently and prevent a native-process deadlock; it does not justify spreading `async`/`await` through the product codebase.

Use a finite Git timeout appropriate to the operation. If the timeout expires, make a best-effort attempt to terminate the Git process, report the timeout as an actionable transaction error, and dispose the `Process` object in `finally`.

Return a small result object containing:

- exit code;
- captured stdout;
- captured stderr.

Only the script's own `Write-Host` messages should normally appear in PMC.

Never rely on Git textual output without first checking its exit code. When a Git operation fails, prefer stderr for the diagnostic detail, fall back to stdout when necessary, and include the exit code.

---

## 11. Git Repository Detection Must Be Junction/Symlink Safe and Multi-Repository Aware

A DTE pathname and Git's canonical pathname can spell the same physical repository differently because of directory junctions or symlinks. Never require literal pathname equality.

### 11.1 xyLOGIX Solution-root/repository-root convention

The folder containing each xyLOGIX `.sln` is normally that Solution's local Git `repoRoot`. The active Solution repository and every sibling Solution repository are independent work trees with independent index/status/history/remote state.

### 11.2 Map every authorized target to its owning work tree

For each target path:

1. Start Git from its actual existing directory.
2. Require `git rev-parse --is-inside-work-tree` to report `true`.
3. Obtain `--show-toplevel` for diagnostics/grouping.
4. For the active Solution repository, verify `git rev-parse --show-prefix` is empty when the Solution directory is expected to be the repo root.
5. Group authorized targets by owning `repoRoot`.

Never stage a repository-A path while Git is running in repository B. Never carry repository-relative path/index assumptions from one sibling repository to another.

Read-only repository discovery may use bounded parallelism when it materially improves performance. Git staging/commit/pull/rebase/push mutation is sequential per repository.

---

## 12. Git Availability, Remote, and Upstream Gating

For an agentic source-modification script using this workflow:

### Git unavailable

If Git cannot be found, stop **before source modification** when the requested workflow requires Git-integrated history.

### Solution not in a Git repository

Stop before modification rather than silently making unmanaged changes.

### Repository with no remote

Local Git work can still proceed:

- create local boundary/feature commits;
- skip initial pull;
- skip final pull/push;
- explain the skip with `Write-Host`.

### Remote exists but the current branch has no upstream

Do not infer, create, or change an upstream automatically merely because one or more remotes exist. The transaction may still complete locally:

- create the boundary and implementation commits;
- skip initial pull/rebase;
- skip final pull/rebase and push; and
- report that the completed transaction remains local because the current branch has no configured upstream.

A configured remote and a configured branch upstream are different facts. Pull/rebase and push behavior should be keyed to the branch's actual upstream relationship, not merely to the existence of a remote named `origin` or any other remote.

### Current branch has an upstream

Use the synchronization workflow described below. Do not assume the remote is named `origin`; use the branch/upstream relationship already configured by Git.

---

## 13. Require a Clean Baseline and Validate Transaction Scope

The maintainer normally commits all preexisting changes before running a Change Transaction Script. Use that as the default transaction contract.

Before initial pull or source mutation:

1. Parse `git status --porcelain` or an equivalent machine-readable status.
2. Verify the Git index is clean.
3. Verify the working tree is clean.
4. Stop before mutation if preexisting tracked, staged, untracked, or otherwise dirty paths are present, unless the current prompt explicitly authorizes a specific preexisting dirty state.
5. Build the allowed path set for the requested work item so that later transaction-created changes can be distinguished from unrelated changes.

This clean-baseline rule makes rollback deterministic and prevents the script from absorbing unrelated user work.

After an initial pull, validate the clean baseline again because synchronization may have changed repository state.

After source/project mutation and VCmd cleanup, refresh Git status and distinguish transaction-owned dirty paths from any unrelated path that appeared **after the clean baseline was established**. This is a Git-scope check, not post-VCmd semantic source verification.

If an unrelated dirty path appears after the transaction began (for example because the maintainer, Visual Studio, another tool, or another process changed it concurrently):

1. report it as `*** WARNING ***`;
2. never stage, reset, restore, or otherwise absorb that unrelated path;
3. continue staging/committing the authorized transaction paths when their exact staging scope can still be proven; and
4. skip the final pull/rebase and push while unrelated dirty work remains, because synchronization could interfere with work the transaction does not own.

The **initial** dirty baseline remains a hard precondition unless the current prompt authorizes it. The rule above applies only to unrelated dirt that appears after a clean baseline and prevents such external activity from needlessly destroying already-completed transaction progress.

When parsing porcelain output, use ordinary PowerShell arrays or `ArrayList`; avoid generic-list binder patterns that are fragile in Windows PowerShell 5.1.

---

## 14. Initial Git Synchronization

When the current branch has a configured upstream, perform the initial synchronization before source modification.

The normal default, because the maintainer starts from a clean working tree and index, is:

```text
git pull --rebase
```

Use `--autostash` only when the current prompt explicitly authorizes a known dirty starting state and the transaction has a concrete reason to preserve it.

Preconditions:

- Git is available.
- Repository identity is established.
- The working tree and index satisfy the clean-baseline rule unless an explicit exception applies.
- A usable `HEAD` exists.
- Git author/committer identity is configured if the script will commit.
- A usable branch upstream is configured.

Postconditions:

- Pull exits successfully.
- The working tree and index still satisfy the expected baseline.
- Authorized target paths are resolved again if synchronization changed repository topology; source contents are not compared with an earlier snapshot before overwrite.

Do not assume the codebase still matches the pre-pull state.

---

## 15. Empty History-Boundary Commit and Rollback Anchor

When this workflow requires a pre-change history boundary, create an **empty commit before making the actual source changes**.

Immediately before creating or adopting the boundary:

1. Verify the Git index and working tree satisfy the clean-baseline rule.
2. Capture the current `HEAD` commit as the transaction's **pre-boundary rollback anchor**.
3. Check whether a matching boundary from a previous failed attempt already exists at `HEAD`.
4. If adopting a matching existing boundary, verify it is truly empty and use its first parent as the rollback anchor.
5. If creating a new boundary, use `git commit --allow-empty -F <tempfile>`.
6. Verify the new boundary commit contains no changed paths.
7. Record that the boundary is transaction-owned for rollback purposes.

The boundary commit is a local transaction marker until the entire transaction succeeds. Do not push the empty boundary independently. Remote synchronization/push occurs only during the successful final Git phase.

### Recognized orphan-boundary cleanup before a new transaction

A previous script iteration may have failed before the newer pre-mutation cleanup rule existed and left a known empty boundary at `HEAD`. A later transaction may proactively remove such a **recognized orphan** before establishing its own boundary when all of the following are true:

1. the current `HEAD` subject exactly matches a boundary subject that the generator explicitly recognizes as belonging to an earlier failed iteration of the same work;
2. `HEAD` is verified to be an empty commit;
3. the working tree and index are clean; and
4. the parent commit can be resolved safely.

Then reset `HEAD` to that parent and report the cleanup. Perform this recognized-orphan cleanup before the transaction's initial pull/rebase when practical, so stale local bookkeeping is not rebased or carried into later synchronization. Never remove a non-empty commit merely because its subject looks familiar, and never use fuzzy subject matching to classify arbitrary history as transaction-owned.

### Successful no-op boundary cleanup

A transaction can also become a legitimate no-op: the audited payload may already be present, or VCmd/IDE cleanup may leave no implementation difference to commit. If the transaction-owned boundary exists, **zero implementation commits were created**, the working tree/index are clean, and the pre-boundary anchor is known, remove the bookkeeping-only boundary before final synchronization.

A successful no-op should leave neither source dirt nor an otherwise-useless empty history marker. Report the no-op as success.

### Failure handling rule

The default policy is **preserve meaningful forward progress**, not automatic rollback. However, an empty history-boundary commit is bookkeeping, not meaningful source/project progress.

The script should explicitly track whether any meaningful positive mutation has completed successfully. Set that transaction state immediately after the first authorized source write, project/reference mutation, scaffold/topology mutation, or other positive work returns normally.

#### Failure before any meaningful positive mutation

If the script must terminate after creating or adopting a transaction-owned empty boundary but **before any meaningful positive mutation has succeeded**:

1. Confirm that the failure occurred before the first successful positive mutation.
2. Preserve any unexpected user/source changes if they somehow appeared; do not discard them merely to clean history.
3. When the working tree/index still represent the clean pre-mutation baseline, move `HEAD` back to the captured pre-boundary rollback anchor so the transaction-owned empty boundary is erased.
4. Remove any transaction-owned temporary state.
5. Report the cleanup through `Write-Host`.
6. Surface the actual infrastructure/mechanical error.

This cleanup is not considered a broad rollback of forward progress because there is no positive source/project progress to preserve. A failed transaction should not strand a useless empty marker commit when it never actually changed the codebase.

If a matching orphaned empty boundary from a prior failed attempt is adopted at `HEAD`, treat it the same way: if the retry terminates before meaningful positive mutation, remove that recognized transaction boundary back to its pre-boundary anchor when doing so will not discard unrelated work.

#### Failure after meaningful positive mutation

Once a meaningful positive mutation has completed successfully:

1. Preserve source/project changes already made.
2. Preserve transaction-created commits already made.
3. Emit `*** WARNING ***` for advisory/nonessential concerns and continue whenever mechanically possible.
4. Do not automatically `git reset --hard`, restore prior source bytes, delete implementation commits, or erase the transaction boundary merely because generated state may need a follow-up correction.
5. Let the maintainer decide what should be corrected in the next Change Transaction Script.

A broad rollback/hard-reset workflow after meaningful mutation should be generated only when the current prompt specifically requests it or when a narrowly defined topology operation has its own explicit rollback contract.

The boundary message should describe the work about to begin and follow the repository's commit-message conventions.

---

## 16. Commit Messages Through Temporary Files

Write commit messages to a uniquely named file in `%TEMP%`, for example:

```text
%TEMP%\5c363663b88a4b3094096e11591f6397.tmp
```

Use UTF-8 without BOM when practical.

Commit with:

```text
git commit -F "<message file>"
```

Advantages:

- avoids quoting problems;
- preserves Unicode correctly;
- supports multiline bodies reliably;
- cleanly implements repository-specific commit format rules.

Delete the temporary file in `finally`.

Do not allow a temp-file deletion failure to mask a more important Git/source failure.

---

## 17. File Mutation: Target, Clobber, Continue

Before writing a file:

1. Resolve it from the intended Solution/repository-relative path.
2. Ensure the target path is authorized for the transaction.
3. Ensure the script can mechanically issue the write.

Then **clobber the authorized target with the pre-audited desired-state payload and continue**.

Once an authorized target path is established, its existing source bytes, formatting, comments, XML documentation layout, hashes, markers, method bodies, declaration shape, or semantics are irrelevant to whether the transaction may overwrite it. The runtime script must not require the target to match an earlier tarball, remembered snapshot, previously generated payload, old method body, regex capture, AST shape, or retry-state hash.

For an exact desired-state replacement, do not read the target file merely to prove that it still looks like the generation-time source. The generator already performed that reasoning against the authoritative source before delivery. Runtime source reads are justified only when the transaction genuinely requires preservation of unmodeled content and therefore must use the exceptional structural-edit model described in Section 18.

Once the underlying write operation returns normally:

1. **Assume the positive source-code modification succeeded.**
2. Mark that meaningful positive mutation has occurred for transaction failure-cleanup purposes.
3. Emit a concise `Write-Host` success/progress diagnostic.
4. Continue.

Do not make transaction continuation depend on either pre-write or post-write source-content policing, including:

- requiring the current file to equal an earlier snapshot, hash, or generated payload;
- finding a particular method body or parameter list before the write is permitted;
- locating an exact old code block before replacement;
- requiring a marker/comment/declaration/regex match to authorize the mutation;
- requiring existing formatting/layout to match the generator's expectation;
- rereading the file after an exact-payload write;
- matching an expected hash;
- proving old text disappeared;
- proving new text appeared;
- proving XML documentation formatting;
- proving public/internal accessibility beyond what the payload itself intentionally writes;
- proving project references are minimal;
- proving the resulting source compiles; or
- any other source-level correctness assertion.

Such checks may be used during generation/audit **before the script is delivered**, but they are not runtime transaction gates.

If an optional runtime diagnostic detects something surprising after a successful write, emit `*** WARNING ***`, preserve the written state, and continue. Do not automatically restore saved prior bytes merely because an optional validation disagrees with the generated state.

After VCmd cleanup, do not perform semantic/textual source verification under any circumstance.

---

## 18. Exact Desired-State Clobbering Is the Default; Structural Edits Are Exceptional

There are two possible mutation strategies, but they are **not peers of equal preference**.

### 18.1 Default: exact desired-state payload clobbering

When the generator can deterministically produce the complete desired file, this is the required/default runtime strategy.

The generator should:

1. audit the current authoritative workspace/tarball and current-prompt corrections before delivery;
2. produce the complete desired file;
3. audit that payload for architectural/style/correctness requirements before delivery; and
4. embed or otherwise supply that exact payload to the Change Transaction Script.

The runtime script should then simply overwrite the authorized target and continue. It must not compare the current file with an old hash, snapshot, expected formatting, method body, marker, declaration shape, or regex before writing.

Conceptually, the runtime mutation should be no more defensive than an appropriate `WriteAllText`/equivalent operation against the already-authorized path. For large/complex textual payloads, prefer generation-time exact-byte/Base64 transport plus runtime `WriteAllBytes` as described in Section 4.1 so PowerShell never has to reinterpret the source text.

Advantages:

- straightforward and idempotent;
- deliberately clobbers the target into the intended state;
- insensitive to prior formatting/reflow differences;
- avoids fragile runtime regex/method-body replacement logic;
- minimizes opportunities for the script to die;
- makes the transaction's intended end state explicit; and
- keeps source correctness where it belongs: generation-time audit.

A runtime helper whose success depends on finding an exact method declaration/body, old source fragment, comment marker, or formatting shape is **not** an acceptable substitute for exact-payload clobbering when the full desired file could have been generated ahead of time.

### 18.2 Exception: structural edits only when unmodeled live content must be preserved

Use a structural edit only when:

- preserving unmodeled live content is materially necessary;
- the complete desired file genuinely cannot reasonably be generated ahead of time; and
- the transaction cannot safely express its intended change as a deterministic full-file replacement.

Structural targeting may inspect enough live content to identify the ownership boundary necessary to preserve that unmodeled content. Even then:

- do not require incidental whitespace, formatting, comments, or line wrapping to match;
- do not add semantic correctness gates unrelated to locating the minimal safe ownership boundary;
- do not choose structural replacement merely because it seems cleverer or more reusable than embedding the audited desired file; and
- if source-shape matching becomes fragile enough that the script can fail on harmless code-layout differences, redesign the transaction to use an exact desired-state payload instead.

If a structural edit truly cannot mechanically identify a safe authorized target boundary, stop because the requested mutation cannot be issued safely. That is a legitimate mechanical inability, not permission to turn routine full-file changes into source-shape validation exercises.

---

## 19. Idempotency and Retry Design

Assume a prior run may have failed after any of these points:

- initial pull;
- boundary commit;
- one or more file writes;
- project-reference modification;
- changed-file editor opening;
- VCmd cleanup;
- staging;
- one or more per-file commits;
- final pull but before push.

A rerun should therefore distinguish:

- **already done** → overwrite/no-op safely or skip when convenient;
- **not done** → perform it;
- **partially done or source-divergent** → overwrite authorized targets with the intended desired state;
- **unexpected repository/topology/path state that makes the next mechanical action unsafe or impossible** → stop with an actionable error.

Do not blindly replay destructive actions, but do not treat source-content divergence as a reason to stop.

Examples:

- A transaction-owned empty boundary left by a prior failed attempt should normally have been cleaned up if no meaningful mutation occurred; if one is nevertheless found and recognized at `HEAD`, remove/reuse it only under the exact empty-commit/clean-tree rules in Section 15.
- Reuse an existing recognizable empty boundary commit rather than creating another when practical.
- If a rerun produces no implementation diff and no implementation commit, remove the transaction-owned no-op boundary so the repository returns to the pre-boundary `HEAD`.
- Overwrite/clobber an authorized source payload regardless of whether a prior run reformatted, partially changed, or otherwise reshaped it; do not rediscover old method bodies or markers first.
- Skip a work-item commit if its path is no longer dirty.
- Add a project/assembly reference only if the current implementation needs it.
- Reopen the current transaction-created changed text/source files in the source-code editor before VCmd even when some are already open from an earlier attempt; opening an already-open document is a harmless retry condition.
- Skip VCmd only when no changed text/source-editable files can be opened.
- Preserve partial forward progress after advisory/runtime correctness concerns and fix those concerns in the next transaction rather than automatically erasing the work.

---

## 20. Visual Studio Project/Reference/Topology Changes

Topology changes are allowed when required by the architecture. They must be coherent and verified at the filesystem, project-system, Solution, namespace, and Git levels.

### 20.1 Adding and verifying assembly/project/package references

Reference handling in a Change Transaction Script is **positive-only**.

The script is not the authoritative judge of whether every preexisting project, assembly, or package reference is necessary. Its responsibility is limited to references that the current transaction itself requires or that the current prompt explicitly instructs it to remove.

When adding a required reference:

1. Locate the actual loaded project.
2. Confirm the intended target reference.
3. Check whether that required reference already exists when convenient.
4. Add it if needed.
5. Save and execute `File.SaveAll`.
6. If the DTE/reference-add operation returned normally, mark the positive mutation and continue.

Do not reread the `.csproj` merely to prove persistence as a fatal gate. An optional diagnostic observation may produce a warning, but a successful DTE/reference-add call is the positive mutation boundary. The maintainer's IDE tooling will reveal any follow-up problem.

### Validator-interface dependency closure

When a class-library project declares or consumes a singleton validator dependency property typed as an `IXXXValidator` interface, ensure that the project also references `xyLOGIX.Validators.Data.Interfaces`. xyLOGIX validator interfaces extend the shared `IDataValidator` contract defined by that project, so this reference is part of the required dependency closure.

For a Change Transaction Script, this is a positive required-reference operation:

1. Identify each project touched by the transaction that declares or newly consumes such an `IXXXValidator` dependency.
2. Ensure `xyLOGIX.Validators.Data.Interfaces` is loaded/locatable through the actual Solution project graph.
3. Add the project reference when absent.
4. Save through the normal DTE/`File.SaveAll` boundary.
5. Continue without policing or removing unrelated existing references.

This rule is especially important when the consuming source appears to reference only the specialized validator-interface assembly: the inherited `IDataValidator` contract still makes `xyLOGIX.Validators.Data.Interfaces` a required project dependency.

### Reference-preservation rule

Unless the current prompt explicitly identifies a specific reference for removal, a Change Transaction Script must:

- leave existing project references alone;
- leave existing assembly references alone;
- leave existing package references alone;
- never classify an existing reference as `forbidden`, `unused`, `unnecessary`, `stale`, or equivalent for the purpose of failing the transaction;
- never fail because additional references exist beyond the transaction's required set;
- never remove an existing reference merely because generated source no longer appears to consume it; and
- never make absence of an unrelated reference a transaction invariant.

Reference handling is therefore one-directional: **ensure the transaction issues/adds the references it requires; do not police harmless extras or turn post-add source/project-file inspection into a fatal gate.**

If the current task explicitly requires removal of a named reference, then that removal becomes part of the transaction's authorized mutation set and may be verified normally. Such explicit removal is the exception, not the default.

### 20.2 Creating projects: impose the complete repository-standard scaffold

A new project must look like a real peer of the existing projects **before** functional implementation is added. Do not manufacture a minimal `.csproj`-only skeleton. Inspect one or more analogous existing projects in the current authoritative repository and copy/adapt their scaffold conventions.

For the current DiagnosticBatchRunner class-library family, the audit establishes the normal project-level scaffold as:

- `<ProjectName>.csproj`;
- `GlobalAspects.cs`;
- `Properties\AssemblyInfo.cs`;
- `Properties\Resources.Designer.cs`;
- `Properties\Resources.resx`;
- `README.md`;
- `packages.config`;
- `key.snk`;
- `app.config` when analogous class-library projects have it; and
- `1382_cogs.ico` when analogous class-library projects have it.

The executable project may legitimately differ; therefore infer the scaffold from the closest analogous project role rather than blindly copying a universal list. Preserve/adapt project GUID, assembly metadata, signing settings, documentation settings, package declarations, resource metadata, and project icon/application metadata according to the repository's existing pattern.

**Creating a brand-new project is an explicit exception to the ordinary instruction not to regenerate `GlobalAspects.cs` or `AssemblyInfo.cs`.** Those files are part of the scaffold and must be copied/adapted from the repository's standard analogous project. Do not omit them.

Project creation workflow:

1. Determine the correct owning Solution/repository.
2. Choose the closest analogous existing project(s) by role (`Constants`, `Interfaces`, root/concrete, `Factories`, `Playbooks`, `Algorithms`, etc.).
3. Generate the complete scaffold first, without functional implementation source.
4. Add the project to the loaded Solution through DTE.
5. Run `File.SaveAll`.
6. Assume successful scaffold writes/DTE additions persisted when their underlying operations return normally.
7. Commit the complete project scaffold as the scaffold-phase historical boundary described in Section 22.5.
8. Only after the scaffold commit, add/update functional source and project references required by that implementation.

Do not terminate merely because a secondary scaffold verifier, regex, hash, metadata comparison, or reread disagrees with the generated scaffold. Report such findings as warnings and continue.

Retry states for a **new-project target directory whose ownership/topology is itself being established by the transaction**:

- directory absent: create it;
- directory exists but is empty: treat as a recognized harmless partial state and populate it;
- directory contains only the known scaffold subset created by a prior failed transaction: complete/clobber the transaction-owned scaffold files as needed;
- directory contains unexpected files that make ownership of the directory genuinely ambiguous: stop instead of assuming those unrelated paths belong to the transaction.

This is a topology/ownership safety exception, not permission to compare or reject the contents of already-authorized source files. Once a scaffold/source file is established as an authorized transaction target, its contents follow the clobbering rules in Sections 17 and 18.

Do not invent a new Module merely because a new class/namespace exists.

### 20.3 Renaming projects requires a closed Solution

Do not rename a loaded project, its containing directory, or `.csproj` while the active Solution remains open.

Required workflow:

1. `File.SaveAll`; capture the exact `$dte.Solution.FullName`.
2. Capture rollback bytes/path mappings.
3. Close the Solution through the existing host `$dte`; do not quit Visual Studio.
4. Pump messages and use bounded retries until file handles are released.
5. Rename the project directory/`.csproj` and other convention-coupled files as required.
6. Verify old paths absent/new paths present after every step.
7. While closed, update `.sln`, `ProjectReference` paths/names, assembly/root namespace metadata, authored namespaces/usings, friend-assembly/config/test references, and build mappings as applicable.
8. Preserve project GUIDs unless identity is intentionally changed.
9. Reopen the same captured Solution through `$dte.Solution.Open(...)`.
10. Pump/wait with finite retries.
11. Rediscover projects from DTE and verify the renamed graph.
12. `File.SaveAll` again.

Rollback in reverse order on failure when feasible and surface rollback failures explicitly.

### 20.4 Rename-aware Git capture

A rename is normally one logical work item. Keep old/new paths together. Include directly coupled topology files only when splitting them would leave an invalid intermediate project/Solution state. Temporary scoped staging may be used for Git rename detection, but it must be reset immediately before final granular staging.

### 20.5 WinForms `*.Designer.cs` partial-class accessibility and editor handling

When a Change Transaction Script creates or modifies a WinForms `*.Designer.cs` file for a public `Form`, `UserControl`, or other public partial WinForms type, explicitly declare the designer-side type part with `public` before `partial`.

For example:

```csharp
public partial class OptionsDialog
```

Do not leave the designer declaration as:

```csharp
partial class OptionsDialog
```

This rule prevents CodeMaid or another cleanup tool from making the accessibility explicit as `internal`, which can conflict with the public declaration in another partial-class file and create a breaking compile-time inconsistency.

During **generation-time audit** for a WinForms designer payload:

1. Inspect the corresponding non-designer partial declaration in the authoritative source snapshot.
2. If the logical type is public, generate the `*.Designer.cs` payload with an explicit `public partial class` declaration.
3. Preserve the same type name and applicable base/type-part relationship in the generated payload.
4. Audit this declaration before delivering the script.

At runtime:

5. Clobber the authorized designer source file with the pre-audited payload without rechecking the old declaration shape.
6. If the `*.Designer.cs` file changed, include it in the changed-file opening set and force it into the source-code/text editor.
7. Never activate the WinForms Designer for that file as part of cleanup.

The prohibition is against opening the **WinForms Designer surface**, not against opening a changed `*.Designer.cs` file as source text.

---
## 21. Source Correctness Is a Generation-Time Responsibility

The AI generating the Change Transaction Script should audit its intended source/project payloads carefully **before delivering the script**.

At runtime, however, the script must not become a second compiler, analyzer, architectural reviewer, or semantic arbiter.

Therefore:

- source-shape reasoning belongs to generation-time audit, not runtime rediscovery;
- exact desired-state full-file payloads are the default mutation mechanism whenever feasible;
- positive source/project mutations are presumed correct once their underlying operations return normally;
- source hashes, regexes, marker checks, declaration checks, method-body locators, old-block searches, AST/source-shape checks, reference checks, API checks, and similar semantic inspections are not fatal runtime gates;
- the changed-file editor-opening pass is a mechanical preparation step, not a source-validation step;
- VCmd is invoked only after the exact one-run noninteractive/Git-disabled sidecar has been written successfully, and VCmd cleanup is trusted completely once invoked successfully;
- no semantic/textual source verification occurs after VCmd;
- no build, compile, or test gate occurs after VCmd or elsewhere by default; and
- Git capture should preserve the transaction's resulting state so the maintainer can inspect it in the IDE.

Runtime diagnostics should use `Write-Host` to report useful progress and warnings, but they must not undo successful positive modifications.

---

## 22. Ordered Git Commit Phase

Change Transaction Scripts must reproduce the supplied Visual Commander/CreateStagedGitDiff work-item behavior for commit selection and ordering. The mandatory VCmd sidecar sets `EnableGitAwareness` and `AutomaticallyCheckInChangesToGitWhenGitAwarenessIsSuppressed` to `false`, so VCmd performs formatting/cleanup only; it must not pull, stage, generate commit messages, commit, or push on behalf of the transaction. The Change Transaction Script remains the sole owner of Git synchronization and capture.

The default for existing-source implementation work is **file-by-file granularity**, subject only to explicit selector/source-family/rename/topology exceptions. Architectural conceptual grouping by itself is not a reason to batch files.

### 22.1 Repository traversal

Process the active Solution repository first, then affected sibling Solution repositories in deterministic order unless a known cross-repository dependency requires otherwise. Git mutation is sequential. Refresh status/index/branch/remote/path state whenever switching repositories.

### 22.2 Normative work-item selection chain

Within one repository, select the next work item in this priority order:

1. recognizable Strategy/module-family semantic order (`Constants`, `Interfaces`, root/concrete, `Factories`, `Actions`, with support roles ordered by actual references);
2. reference/dependency-ready C# source family or project-structural change;
3. stable project fallback by project-directory leaf/full path;
4. unscoped status priority `A`, `R`, `C`, `T`, `D`, `M`, then stable leaf/full path;
5. Solution file last unless scaffold/topology atomicity requires it earlier.

Recompute the next work item from fresh Git status after every commit.

### 22.3 Default file-by-file granularity and source-family exceptions

For ordinary implementation changes, one selected Git entry is normally one commit. Keep multiple paths together only when the selector deliberately identifies one logical artifact, for example:

- rename old/new counterparts;
- directly coupled partial/generated/designer/resource members of one source family;
- a source family the supplied selector treats as inseparable; or
- topology paths whose separation would create an invalid intermediate state.

Do **not** batch files merely because they all participate in the same feature, refactor, architectural increment, Playbook, Pipeline, or module.

### 22.4 Rename-aware selection

If Git shows a rename as add/delete, use bounded scoped temporary staging when needed to let Git identify the rename, inspect staged status/diff, reset, then stage the final rename work item as one coherent set. Never leave rename-detection staging behind.

### 22.5 New projects/modules: complete scaffold first, then implementation

Project creation is a deliberate exception to ordinary file-by-file commit granularity. The scaffold is a **project-level/module-family historical unit**, not a series of commits for `key.snk`, `README.md`, `AssemblyInfo.cs`, etc.

Follow the supplied project-creation transaction model:

1. Create the complete standard scaffold(s), including all conventional project-level files described in Section 20.2.
2. Add the new project(s) to the Solution.
3. Save the Solution/project state; treat successful scaffold writes/DTE additions plus `File.SaveAll` as the positive persistence boundary rather than rereading source/project files to prove them.
4. Commit the scaffold as **one atomic project-add work item**.

When several tightly related new projects constitute one newly introduced module family, their complete scaffolds **and the corresponding `.sln` membership change** may be committed together as one module-family scaffold checkpoint, as in the supplied reference transaction. This is preferred when those projects are intentionally being introduced together and the commit describes the new module family cleanly.

The scaffold commit must contain no functional domain implementation source beyond project-level infrastructure. After that commit:

5. add/move/update functional code;
6. add/update project references required by implementation; and
7. commit implementation using the normal CreateStagedGitDiff file-by-file/source-family/rename-aware selection behavior.

A scaffold commit topline should describe the project/module creation, e.g. `Create dialog ownership policy projects`, rather than allowing every scaffold artifact to become its own `Create key.snk`/`Create README.md` commit.

### 22.6 Commit-message generation/validation

Generate the message from the actual staged diff and validate it against the repository's current dedicated instructions. Single-file special cases (`Create <file name>`, `Update <file name>`) take precedence over generic topline validation. Do not reject a repository-mandated message because of a flawed verb regex or finite grammar whitelist.

### 22.7 Per-work-item discipline

For each work item:

1. Refresh status.
2. If already clean, treat as retry-safe success.
3. Require an otherwise clean index.
4. Stage the complete intended path set.
5. Verify staged names exactly.
6. Generate/validate message from staged diff.
7. Commit via UTF-8-no-BOM temp file.
8. Verify commit success and work-item cleanliness.
9. On a recoverable post-staging failure, unstage/reset the work item.
10. If the transaction aborts before any meaningful positive mutation ever succeeded, apply the Section 15 pre-mutation cleanup rule and erase the transaction-owned empty boundary back to the pre-boundary anchor when the clean baseline is still intact.
11. If meaningful positive mutation has occurred, preserve forward source/project/Git progress unless the current prompt or a narrowly defined topology rollback contract explicitly requires otherwise.
12. Select the next work item from fresh status when the transaction remains healthy.

Never allow unrelated staged paths to hitchhike. Never span two Git repositories in one commit.

---

## 23. Final Git Synchronization and Push

After ordered commits:

1. Refresh repository status.
2. If transaction-owned implementation paths remain dirty or staged unexpectedly, stop with an actionable Git-scope error because capture is incomplete.
3. If unrelated dirty paths appeared after the clean baseline, warn, leave them untouched, and skip final pull/rebase and push; the authorized local implementation commits remain valid progress.
4. If zero implementation commits were created, the transaction-owned boundary exists, and the repository is clean, remove the bookkeeping-only no-op boundary back to the pre-boundary anchor before any final synchronization.
5. If the current branch has no configured upstream, report that local commits are complete and stop successfully.
6. If an upstream is configured and the working tree/index are clean, run a final `git pull --rebase`.
7. Verify it succeeds and leaves the expected clean state.
8. Push using the configured branch/upstream relationship.
9. Verify push succeeds.

Do not create/change an upstream automatically merely to make the transaction push. Do not claim remote synchronization success until the pull/rebase and push postconditions are established.

If the final pull changes history, ensure the push is operating on the intended branch/upstream relationship.

---

## 24. `Write-Host` Diagnostics, Git Output, and Error Reporting

Every Change Transaction Script should report useful, concise, script-owned diagnostics through `Write-Host`. The maintainer should be able to follow the transaction's significant progress and understand what it did without needing to infer state from silent execution or debug raw native output.

Useful diagnostics normally include, when applicable:

- validated Solution identity and relevant repository context;
- initial clean-baseline and synchronization progress;
- boundary/scaffold/implementation phase transitions;
- meaningful source/project mutation progress;
- legitimate no-op decisions;
- changed-file discovery after mutation;
- each changed text/source file being opened in the Visual Studio source-code editor, and legitimate skips such as deleted or non-text/binary files;
- confirmation that the complete changed-file opening pass finished before VCmd;
- VCmd sidecar preparation, confirmation that the invocation is noninteractive/Git-disabled, and VCmd invocation/completion or warning-only skip/failure;
- Git work-item selection, staging, commit completion, and repository transitions;
- final pull/rebase/push progress when applicable;
- warnings that do not block forward progress; and
- actionable error context when execution truly cannot continue.

Use concise prefixes such as:

```text
*** INFO *** ...
*** SUCCESS *** ...
*** WARNING *** ...
*** ERROR *** ...
```

Do not flood PMC with trivial implementation chatter, per-byte details, or raw native-tool streams merely because diagnostics are required.

PMC can misinterpret native Git stdout/stderr as PowerShell errors or otherwise clutter the console. Native Git stdout/stderr should remain captured unless it is intentionally summarized in a useful `Write-Host` diagnostic or an error message.

On a PowerShell exception, report enough context to diagnose the failure immediately:

- exception message;
- invocation position/line when available;
- script stack trace when available.

At the outer transaction boundary, report that information once, run the Section 15 pre-mutation-boundary cleanup when applicable, preserve meaningful positive progress otherwise, and normally **do not rethrow** merely to make PMC print the same failure again. The user should receive one coherent transaction diagnostic narrative.

Do not leave the user with only:

```text
Argument types do not match
```

and no indication of where it happened.

---

## 25. Avoid Over-Engineering Safety Checks

“Abundance of caution” does **not** mean adding every imaginable gate.

A safety check is valuable only when it answers a question that matters to the next action.

Bad examples:

- Reading/parsing an authorized source file at runtime solely to prove that a method still has the generation-time signature/body before replacing it.
- Using a regex/marker/old-code search helper that throws when harmless source formatting or method shape differs, when an exact desired-state file could simply be clobbered into place.
- Treating one changed file's source-editor open failure as fatal to otherwise-successful source mutation.
- Invoking `VCmd.CCommandStripLineBreaksFromAllComments` without first writing the required one-run noninteractive/Git-disabled `.config.json` sidecar.
- Passing a `NoPrompt` or other command argument to `VCmd.CCommandStripLineBreaksFromAllComments` instead of using its JSON sidecar.
- Enabling VCmd Git awareness or automatic late check-in during a Change Transaction Script, which would compete with the script's own staged-diff/custom-commit workflow.
- Treating VCmd sidecar-preparation/unavailability/cleanup failure as a reason to roll back source or suppress Git capture; sidecar-preparation failure should instead skip VCmd.
- Rejecting a script because a DTE debugger enum string does not spell the enum member name.
- Requiring the Git canonical path string to equal the DTE Solution path string.
- Opening a document solely to test whether a cleanup command might work later.
- Requiring an exact XML documentation layout when ReSharper is allowed to reflow it.
- Rejecting a project because it contains additional references that the transaction did not add or does not currently use.
- Removing a preexisting reference merely because the script believes it is unnecessary.
- Excluding a changed source file from the VCmd opening pass merely because it is named `GlobalAspects.cs`, `AssemblyInfo.cs`, or `*.Designer.cs`.

Good examples:

- Resolving the authorized target pathname and then clobbering it with the pre-audited exact desired-state payload without inspecting old source contents.
- Transporting large/complex audited payloads as exact Base64-encoded bytes and writing them with `WriteAllBytes` to avoid PowerShell text-encoding/quoting hazards.
- Tracking whether the first meaningful positive mutation has succeeded so a pre-mutation failure can clean up only the transaction-owned empty boundary.
- Warning and continuing when one source file cannot be opened for VCmd, while running cleanup against the files that did open.
- Writing the exact schema-version-2 noninteractive/Git-disabled VCmd sidecar immediately before invocation so the command cannot display its confirmation message box(es) or perform Git work.
- Warning and skipping VCmd when its sidecar cannot be prepared, rather than invoking the command with default interactive/Git-aware behavior.
- Warning and continuing when VCmd itself is unavailable, followed by the unconditional final `File.SaveAll`.
- Checking document count before a close-all operation that is actually required.
- Verifying that a changed path exists before trying to open it as source text.
- Determining whether a changed file is text/source-editable before asking Visual Studio to open it in a text view.
- Explicitly requesting the source-code/text editor so a WinForms file cannot default to the Designer.
- Verifying Git staging contains exactly the intended path before commit.
- Confirming that the required DTE reference-add operation returned normally and then flushing with `File.SaveAll`, without making a `.csproj` reread a fatal gate.
- Re-resolving authorized target paths after `git pull` when repository topology may have changed.

The question to ask before every gate is:

> Does failure of this condition actually make the next action unsafe or nonsensical?

If not, do not make it a hard gate.

---

## 26. Recommended High-Level Execution Order

### Phase 1 — establish Visual Studio context

1. Enter child scope; strict errors.
2. Validate host `$dte` without binding/assigning it.
3. Validate loaded Solution and derive its directory.
4. Discover relevant loaded project paths from DTE.
5. Emit useful `Write-Host` diagnostics describing the established context.
6. `File.SaveAll`.

### Phase 2 — establish target/Git contexts

1. Resolve exact allowed targets.
2. Locate Git.
3. Map every target to its owning repoRoot.
4. Validate junction-safe repository identity.
5. Validate the initial clean status/index state per repo.
6. Remove a recognized orphaned prior empty boundary when the strict Section 15 conditions apply; do this before pull/rebase so stale local bookkeeping is not carried into synchronization.
7. Capture remote/upstream state per repo.
8. Initial pull/rebase when an upstream exists.
9. Revalidate the clean baseline after pull.
10. Capture the pre-boundary rollback anchor.
11. Reuse/create the required transaction boundary.
12. Report meaningful synchronization/boundary progress through `Write-Host`.

### Phase 3 — topology/scaffold phase

1. For each new project/module family, impose the **complete analogous scaffold**, not a minimal project skeleton.
2. Add project(s) to the Solution through DTE.
3. Save and verify.
4. Commit the whole project/module-family scaffold atomically, including Solution membership when that is the modeled scaffold unit.
5. Only after the scaffold commit, proceed to functional implementation.
6. For renames, follow the closed-Solution workflow in Section 20.3.

### Phase 4 — implementation/editor cleanup

1. Apply source/project-reference changes in dependency order. For deterministically modelable files, clobber authorized targets with complete pre-audited desired-state payloads; do not rediscover old method bodies/markers/source shape at runtime.
2. Immediately after the first successful positive write/reference/topology mutation, mark that meaningful positive mutation has occurred for failure-cleanup purposes.
3. Assume successful positive mutation operations produced the intended state.
4. Do not run lint/style/static-analysis gates; if explicitly requested for information, report findings as warnings and continue.
5. `File.SaveAll`.
6. Refresh the complete transaction-created changed-path set.
7. Attempt to open **all changed text/source-editable files** in Visual Studio's source-code/text editor, explicitly preventing WinForms Designer activation. Include changed `GlobalAspects.cs`, `AssemblyInfo.cs`, and `*.Designer.cs` files rather than excluding them by name.
8. Report changed-file opening progress through `Write-Host`; deleted/non-text/binary paths and individual source-editor open failures are warning/no-op skips, not transaction-fatal errors.
9. Only after the complete opening pass finishes, write the exact Section 9 one-run `.config.json` values that suppress prompts and disable both VCmd Git paths. If the sidecar cannot be prepared, warn and skip VCmd.
10. When sidecar preparation succeeded, attempt the **argumentless** `VCmd.CCommandStripLineBreaksFromAllComments` invocation once against the successfully opened set. If VCmd fails, warn and continue.
11. Final unconditional `File.SaveAll` regardless of sidecar/VCmd outcome.
12. Close only transaction-owned documents when required; do not indiscriminately close unrelated documents.
13. Do **not** semantically verify source after VCmd.
14. Refresh Git status for capture; do not convert source/project diagnostics into blockers.

### Phase 5 — Git capture

1. Do not run build/compile/test operations as transaction gates.
2. Traverse repositories deterministically.
3. Select work items using Section 22.
4. For ordinary implementation work, default to file-by-file commits; preserve only selector-defined families/renames/topology units.
5. Stage exactly, verify, commit from staged diff, verify postconditions.
6. Refresh status and repeat.
7. If zero implementation commits remain after cleanup and the repository is clean, remove the transaction-owned no-op boundary before final synchronization.
8. If the current prompt expressly requested an informational build/test, it may be run and reported without becoming a rollback trigger.
9. Final repo status check; pull/rebase/push only when the current branch has an upstream and no unrelated post-baseline dirty paths remain.
10. Use `Write-Host` to report meaningful commit and synchronization progress without dumping raw Git streams.

### Phase 6 — cleanup

Remove temp files, dispose owned resources, restore only script-owned editor state, and never damage/release/rebind `$dte`.

If the transaction terminates before any meaningful positive mutation succeeded, erase its transaction-owned empty boundary back to the pre-boundary anchor when the clean baseline is still intact. Do not strand bookkeeping-only commits from failed pre-mutation runs.

After meaningful positive mutation, do not automatically roll back source/project changes or transaction-created Git history because of advisory correctness concerns. Preserve the resulting state for maintainer review and follow-up.

---

## 27. Side-Effect Gate Matrix

| Action | Immediate precondition | Required postcondition | No-op / recovery |
|---|---|---|---|
| `File.SaveAll` | Valid host DTE + loaded Solution | Command completes | Execute at defined checkpoints |
| Clobber authorized source/project file | authorized target path + pre-audited desired-state payload + writable target | exact payload bytes written; mark meaningful mutation | do not inspect old source shape; exact-byte/Base64 transport is preferred for complex payloads |
| Pre-mutation failure after empty boundary | transaction-owned boundary exists + no meaningful positive mutation succeeded + baseline still clean | `HEAD` returned to pre-boundary anchor; boundary removed | preserve/report unexpected dirty work instead of discarding it |
| Successful no-op transaction | transaction-owned boundary exists + zero implementation commits + repo clean | `HEAD` returned to pre-boundary anchor; bookkeeping-only boundary removed | report no-op as success |
| Create project scaffold | Correct repo + analogous scaffold identified | Full expected scaffold exists and metadata is coherent | Empty dir: populate; known partial scaffold: complete; unexpected ownership ambiguity: stop |
| Add project to Solution | Scaffold `.csproj` exists | DTE add operation returns normally + `File.SaveAll` completes | Already loaded correctly: skip |
| Scaffold commit | Complete verified scaffold(s) + Solution membership; clean index | Entire project/module-family scaffold committed as one atomic add | Existing verified scaffold commit: reuse/skip |
| Add required reference | Target loaded + required reference absent | DTE reference-add returns normally + `File.SaveAll` completes | Already present: skip; unrelated extra references: ignore |
| Rename project/folder | Solution closed + rollback state captured | old absent/new present; topology updated; same Solution reopened | bounded retry/rollback |
| Discover changed files for editor cleanup | source/project mutations saved | complete transaction-created changed-path set resolved | no changed paths: successful no-op |
| Open changed file for VCmd | authorized changed path + existing text/source-editable file | successful opens use source-code/text editor, not WinForms Designer | deleted/non-text/binary/unopenable path: warn/skip; already open as text: reuse; never roll back source |
| Prepare VCmd one-run sidecar | complete opening pass finished + at least one changed text/source file opened | canonical `.config.json` written with schema `2`, prompt suppression enabled, both VCmd Git behaviors disabled | preparation failure: warn, skip VCmd, continue to unconditional SaveAll/Git capture |
| VCmd cleanup | sidecar preparation succeeded + at least one changed text/source file opened | one **argumentless** VCmd attempt + unconditional final SaveAll; successful cleanup trusted with no post-VCmd semantic check | zero opened files: skip; VCmd failure: warn and continue; command resets sidecar defaults on normal `Run` exit |
| Optional informational lint/style/static analysis | explicitly requested by current prompt | outcome captured/reported | findings and nonzero exit are warning-only; continue |
| Optional informational build/test | explicitly requested by current prompt | outcome captured/reported | failure is non-fatal and never triggers rollback |
| Select implementation work item | fresh repo status | CreateStagedGitDiff-compatible next path set selected | no transaction changes: repo complete |
| `git add` | work item dirty + index otherwise clean | staged paths exactly intended | clean item: skip; failure: reset |
| Git commit | staged set + repository-valid message | commit succeeds; item clean | already committed/clean: skip |
| Advisory/runtime correctness concern | positive mutations already completed | warning reported; forward state preserved | continue and address in next transaction |
| Switch repo | previous repo postconditions satisfied | fresh path/status/index/branch/remote state | never reuse old repo-relative state |
| Final pull/push | configured branch upstream + clean tree/index with no unrelated post-baseline dirt | pull/rebase and push exit successfully | no upstream: local success; unrelated dirt: warn and skip synchronization |

---

## 28. Review Checklist Before Delivering a PMC Script

### PowerShell / PMC compatibility

- [ ] Target is Visual Studio PMC and the verified PowerShell 5.1 Desktop parser/binder behavior.
- [ ] Entire implementation is child-scoped when dot-sourced.
- [ ] No assignment/binding/shadowing/removal of `$dte` in any casing.
- [ ] No PowerShell 7-only syntax.
- [ ] Regex escapes were checked for PowerShell/.NET semantics (`'\b'`, not `'\\b'`, for a word boundary in a single-quoted pattern).
- [ ] Native Git stdout/stderr is redirected and both streams are drained concurrently before/while waiting for process exit.
- [ ] Git waits are bounded; timed-out processes are terminated best-effort and disposed.
- [ ] The top-level transaction catch reports actionable context and normally returns control to PMC without redundant rethrowing.
- [ ] Errors report invocation/stack context.
- [ ] Useful transaction progress, no-op, warning, and failure diagnostics are emitted through `Write-Host` without flooding PMC.
- [ ] The exact delivered `.ps1` artifact has been parsed/static-checked for PowerShell 5.1 compatibility after it was written to disk.

### Project creation

- [ ] The closest analogous existing project(s) were inspected.
- [ ] Every new project has the complete expected scaffold, not a minimal skeleton.
- [ ] `GlobalAspects.cs`, `AssemblyInfo.cs`, resources, README, signing key, packages, config, icon, and other normal peer files are included/adapted when the analogous project has them.
- [ ] New-project creation correctly treats `GlobalAspects.cs`/`AssemblyInfo.cs` as scaffold exceptions to the ordinary no-regeneration rule.
- [ ] Empty or known-partial project directories are retry-safe; unexpected contents stop the transaction.
- [ ] New project/module-family scaffolds are committed atomically before functional source.
- [ ] The `.sln` membership change is included in the scaffold checkpoint when the modeled project-creation transaction treats it as part of that atomic scaffold.
- [ ] Functional implementation is committed afterward using file-by-file/source-family/rename-aware selector rules.

### Commit selection/messages

- [ ] Ordinary implementation commits default to file-by-file granularity.
- [ ] Multi-file implementation commits occur only for explicit CreateStagedGitDiff/source-family/rename/topology exceptions.
- [ ] Files are not grouped merely because they share a feature/refactor/increment.
- [ ] Scaffold project/module-family creation is recognized as an explicit granularity exception.
- [ ] Message generation uses the actual staged diff.
- [ ] Single-file `Create <file>`/`Update <file>` rules are honored before generic validation.
- [ ] No brittle finite verb/past-tense whitelist can reject an otherwise specification-compliant message.
- [ ] Staging is exact and reset on failure.

### Visual Studio/Git/source safety

- [ ] The generator audited the current authoritative source and produced complete exact desired-state payloads wherever feasible.
- [ ] Large/complex exact payloads use a transport that preserves exact audited bytes (prefer Base64 + `WriteAllBytes`); every embedded payload was decoded and compared with the generation-time desired bytes before delivery.
- [ ] Authorized target/payload/commit-message maps are internally consistent before delivery.
- [ ] The runtime script treats authorized source/project targets as clobber targets and does not reread/parse/regex-match/hash/AST-compare old source merely to authorize a write.
- [ ] No runtime helper can fail solely because an expected method body/signature/marker/old code block/formatting shape is absent when exact full-file replacement was feasible.
- [ ] Structural edits are used only where preserving genuinely unmodeled live content makes full-file clobbering impractical.
- [ ] The script tracks whether meaningful positive mutation has occurred.
- [ ] If termination occurs after a transaction-owned empty boundary but before meaningful positive mutation, the script removes that boundary back to the pre-boundary anchor when doing so will not discard unrelated work.
- [ ] Solution identity comes from `$dte.Solution.FullName`.
- [ ] Loaded project paths come from DTE when available.
- [ ] Junction/symlink path spelling is not used as identity equality.
- [ ] Every target is mapped to the correct repository.
- [ ] Git mutation is sequential per repository.
- [ ] The transaction begins from a clean working tree and clean index unless the current prompt explicitly authorizes a known exception.
- [ ] Dirty scope is checked after synchronization and before capture.
- [ ] Unrelated dirt that appears after the clean baseline is warned about, never staged/reset, and causes final pull/push to be skipped rather than destroying transaction progress.
- [ ] Pull/rebase/push is keyed to the current branch's configured upstream, not merely to the existence of a remote.
- [ ] Empty-boundary creation/reuse is retry-aware when practical.
- [ ] Recognized orphaned empty boundaries from earlier failed iterations are removed only when `HEAD`/empty/clean/ownership checks make the cleanup unambiguous.
- [ ] A successful transaction that produces no implementation commit removes its bookkeeping-only boundary when the repository is clean.
- [ ] Authorized source/project targets are overwritten without old-byte/hash/layout/semantic preconditions.
- [ ] Linting, formatting/style diagnostics, and static-analysis findings are warning-only and cannot throw, abort, roll back, remove commits, reset history, or block Git capture.
- [ ] Advisory source/project/reference/formatting diagnostics cannot trigger rollback or erase forward progress.
- [ ] Required reference-add operations are issued when needed; reference state is not policed afterward.
- [ ] Every touched class-library project that consumes an `IXXXValidator` singleton dependency has a required project reference to `xyLOGIX.Validators.Data.Interfaces`.
- [ ] No existing project/assembly/package reference is rejected, removed, or treated as an error merely because it appears unused or unnecessary.
- [ ] Existing references are removed only when the current task explicitly requires removal of the specific reference.
- [ ] Project renames occur only while the Solution is closed and use finite retries/rollback.
- [ ] After mutations and `File.SaveAll`, the script resolves the complete transaction-created changed-file set.
- [ ] Every changed text/source-editable file is **attempted** before VCmd in the Visual Studio source-code/text editor.
- [ ] Individual source-editor open failures are warning-only and do not roll back successful source/project mutations.
- [ ] No changed text/source file is excluded merely because it is `GlobalAspects.cs`, `AssemblyInfo.cs`, `*.Designer.cs`, `*.g.cs`, or `*.i.cs`.
- [ ] WinForms files are explicitly opened as source text; the transaction never activates the WinForms Designer for cleanup.
- [ ] Deleted and non-text/binary changed paths are treated as legitimate non-openable skips rather than editor failures.
- [ ] Immediately before every VCmd invocation, the script writes `%LOCALAPPDATA%\xyLOGIX, LLC\Visual Commander\Commands\Strip Line Breaks from All Comments\Config\.config.json` using schema `2` with `SuppressPrompts = true`, `EnableGitAwareness = false`, and `AutomaticallyCheckInChangesToGitWhenGitAwarenessIsSuppressed = false`.
- [ ] VCmd is never invoked when that sidecar preparation fails; the failure is warning-only, VCmd is skipped, and final `File.SaveAll`/script-owned Git capture continue.
- [ ] `VCmd.CCommandStripLineBreaksFromAllComments` is invoked without command arguments; no `NoPrompt` argument or equivalent is used.
- [ ] The VCmd sidecar disables all VCmd-owned Git behavior so the Change Transaction Script remains solely responsible for synchronization, staging, custom commit-message generation, commits, and push.
- [ ] VCmd is attempted only after the complete changed-file opening pass has finished and successful sidecar preparation, and normally once for the successfully opened set; VCmd failure is warning-only and final `File.SaveAll` still occurs.
- [ ] No source-byte/hash/layout/semantic match is required before overwriting an authorized target or before VCmd.
- [ ] No semantic/textual source verification is performed after successful VCmd cleanup.
- [ ] No build, compilation, or test operation is used as a fatal transaction gate.
- [ ] If an informational build/test was explicitly requested, failure cannot throw, abort, roll back source/project state, or reset transaction-owned Git history.
- [ ] Every modified public WinForms `*.Designer.cs` type part is explicitly declared `public partial class` when its corresponding logical type is public.

---

## 29. Change Transaction Script Delivery Requirements

Use a fresh unique lowercase 32-character hexadecimal GUID-style basename with `.ps1` for every iteration. Deliver the script as a downloadable file intended to be dot-sourced from PMC. Do not create branches/issues/PRs unless explicitly requested.

### Required two-pass pre-delivery audit

Do not stop at reviewing the generator's in-memory representation. **Double-check the exact artifact that will be delivered.** The user should not discover basic PowerShell, payload, or transaction-shape defects by running the script.

#### Pass 1 - transaction/content audit

Audit the planned transaction against the current authoritative workspace and current instructions:

- every authorized target is intentional and repository-correct;
- exact full-file desired-state payloads are used wherever feasible;
- source/project payloads are generation-time audited for correctness/style/documentation/reference requirements;
- commit messages are scoped to their intended staged work items and obey repository rules;
- boundary/retry/no-op behavior is coherent;
- editor-opening/VCmd cleanup is best-effort and cannot erase source progress, while every VCmd invocation is preceded by the exact noninteractive/Git-disabled one-run sidecar so no modal prompt or VCmd-owned Git workflow can occur;
- Git synchronization respects actual upstream state; and
- unrelated post-baseline dirty paths cannot hitchhike or be reset.

#### Pass 2 - exact artifact/static audit

After writing the final GUID-named `.ps1` file, reopen **that exact file** and audit it again. At minimum:

1. verify the first PowerShell token/encoding is sane and no stray BOM character is embedded as source text;
2. parse it with a PowerShell 5.1-compatible parser when available (or the closest reliable parser/static check available) and require zero syntax errors before delivery;
3. verify there is no assignment/binding/shadowing/removal of `$dte` in any casing;
4. verify all exact-byte/Base64 payloads decode successfully and exactly match the generation-time audited desired bytes;
5. verify the authorized path set, payload map, commit-message map, and any per-path metadata agree with one another;
6. verify all retries/timeouts are bounded;
7. verify Git stdout/stderr are drained safely and native output is not dumped directly into PMC;
8. verify no cross-repository staging or stale repository-relative state exists;
9. verify no runtime method-body/marker/regex/old-code/hash/source-shape discovery can unnecessarily kill an exact-payload transaction;
10. verify meaningful-mutation tracking and pre-mutation empty-boundary cleanup are present when a boundary is used;
11. verify successful no-op boundary cleanup is present when a boundary is used;
12. verify changed text/source paths are opened/attempted before VCmd with an explicit source/text view and cannot activate the WinForms Designer;
13. verify every VCmd invocation is immediately preceded by a write to the canonical `.config.json` path with **exactly** schema `2`, `SuppressPrompts: true`, `EnableGitAwareness: false`, and `AutomaticallyCheckInChangesToGitWhenGitAwarenessIsSuppressed: false`;
14. verify the script skips VCmd rather than invoking it when sidecar preparation fails, and verify the VCmd call is argumentless (no `NoPrompt` or other command argument);
15. verify VCmd cannot perform Git synchronization/check-in/push and therefore cannot compete with the script's own custom commit-message/staging workflow;
16. verify per-file editor-open failure, sidecar-preparation failure, and VCmd failure are warning-only and the final `File.SaveAll` is unconditional;
17. verify no post-VCmd semantic source verification or fatal lint/style/static-analysis/build/compile/test gate exists;
18. verify reference handling is positive-only unless the current prompt explicitly authorizes removal;
19. verify public WinForms `*.Designer.cs` payloads use the required explicit `public partial class` declaration when applicable;
20. verify top-level exception handling reports message/position/stack, performs safe transaction cleanup, preserves meaningful progress, and normally does not rethrow redundantly; and
21. verify the script's final success/no-op/error paths all leave the repository and PMC session in a state the maintainer can understand from `Write-Host` output.

Only after both passes succeed should the artifact be delivered.

---

## 30. Final Standard

> A Change Transaction Script is a **clobbering, progress-first, in-IDE transaction whose primary runtime goal is to impose the audited change and keep moving rather than invent reasons to die**. Use the host-provided `$dte`; never assign or bind to it. Audit source/project shape before delivery against the current authoritative workspace/tarball and current-prompt corrections. Generate complete exact desired-state file payloads whenever feasible. For large/complex payloads, transport the exact audited bytes safely (prefer Base64 plus `WriteAllBytes`) so PowerShell quoting, interpolation, encoding, BOM handling, or newline conversion cannot corrupt the desired file. At runtime, once the correct Solution/repository and authorized target path are established, assume the live file is the transaction input already audited by the generator and overwrite/clobber it directly. Do not reread, parse, regex-match, search for methods/markers/old code blocks, hash-check, AST-compare, compare formatting/layout, or otherwise mechanically verify existing source text merely to decide whether an authorized write may occur. Structural edits are exceptional and exist only when preserving genuinely unmodeled live content makes full-file replacement impractical. Once a positive write/project/reference/topology mutation returns normally, assume it succeeded, mark that meaningful positive mutation has occurred, and keep moving.
>
> Emit useful, concise `Write-Host` diagnostics so the maintainer can follow meaningful transaction progress without being flooded by raw tool chatter. Git runs quietly through `System.Diagnostics.Process`, with stdout/stderr drained concurrently and waits bounded. Flush at the defined synchronization boundaries. After mutations are saved, resolve the complete transaction-created changed-file set and **attempt** to open every changed text/source-editable file in Visual Studio's source-code/text editor before running VCmd; never activate the WinForms Designer for this cleanup pass, including for WinForms primary `.cs` or `*.Designer.cs` files. Individual editor-open failures are warnings, not reasons to erase source progress. Do not exclude changed source files merely because they are named `GlobalAspects.cs`, `AssemblyInfo.cs`, `*.Designer.cs`, `*.g.cs`, or `*.i.cs`. Once the opening pass is complete, write the canonical one-run VCmd `.config.json` with schema `2`, `SuppressPrompts: true`, `EnableGitAwareness: false`, and `AutomaticallyCheckInChangesToGitWhenGitAwarenessIsSuppressed: false`. This sidecar is mandatory: it keeps cleanup noninteractive and keeps all Git synchronization, staging, custom commit-message generation, commits, and push under Change Transaction Script control. If the sidecar cannot be prepared, warn and skip VCmd rather than risking modal UI or VCmd-owned Git behavior. Otherwise invoke `VCmd.CCommandStripLineBreaksFromAllComments` once **without arguments** against the successfully opened set. VCmd failure is warning-only. Perform the final unconditional `File.SaveAll` whether sidecar preparation/VCmd succeeded, failed, or was skipped. VCmd normally resets its schema-version-2 sidecar to defaults when its `Run` invocation exits, so the script must rewrite the required one-run values before each future VCmd invocation.
>
> Do not use source hashes, semantic checks, linting, style/formatting diagnostics, static analysis, reference analysis, builds, compilation, tests, cleanup expectations, or architectural diagnostics as fatal runtime gates. Trust successful VCmd cleanup completely and never semantically revalidate its output. Do not police references. Start from a clean Git baseline unless explicitly authorized otherwise; if unrelated dirt appears afterward, never stage/reset it, continue capturing exact authorized work when safe, and skip final synchronization while that unrelated dirt remains. Key pull/rebase/push behavior to the current branch's configured upstream, not merely to the existence of a remote. If the script encounters a recognized orphaned empty boundary from an earlier failed iteration, remove it only under strict ownership/empty/clean checks. If the script terminates after creating/adopting its transaction-owned boundary but before any meaningful positive mutation succeeds, clean up that bookkeeping-only boundary back to the captured pre-boundary anchor when the baseline is still clean. If the transaction finishes as a legitimate no-op with no implementation commit, remove its bookkeeping-only boundary as well. After meaningful positive mutation, preserve forward source/project/Git progress rather than automatically restoring files, deleting commits, or hard-resetting history because an advisory check disagrees with the generated state.
>
> The outer transaction catch should make a failure **diagnosable, not noisier**: report the exception message, invocation position, and stack; perform only safe transaction-owned cleanup; preserve meaningful progress; and normally return control to PMC without redundant rethrowing. The maintainer will inspect Visual Studio/ReSharper/CodeMaid/Git and direct any correction through the next Change Transaction Script. For new projects, impose the complete repository-standard scaffold and commit that scaffold atomically before implementation. For implementation, use the supplied CreateStagedGitDiff file-by-file default and group only explicit logical families/renames/topology units. Before delivery, perform a two-pass audit: first the planned transaction, then the **exact GUID-named `.ps1` artifact actually being delivered**, including PowerShell 5.1 parsing/static validation and exact payload-decode checks. Keep each Git work tree isolated, make scripts fast, make failures actionable only when execution truly cannot continue mechanically, and treat the actual Visual Studio PMC/PowerShell 5.1 host as the runtime it really is.
