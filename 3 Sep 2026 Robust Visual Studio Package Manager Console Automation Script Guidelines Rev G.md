# Robust Visual Studio Package Manager Console Automation Script Guidelines

Revision: G 
Last Updated: 3 September 2026

## Revision F Scope

Revision F incorporates the transaction behavior learned from the first full Revision E repair run and supersedes Revision E where the rules below differ.

Revision F retains Revision E's DTE-owned Solution/project topology, junction-aware path identity, ReSharper suspension, one transaction-wide VCmd opening round, paced project-owned source opening, ReSharper resumption before VCmd, and generation-time source-quality acceptance rules. It adds and clarifies the following mandatory requirements:

- **VCmd return is not the end of IDE cleanup.** `VCmd.CCommandStripLineBreaksFromAllComments` can trigger a downstream `ReSharper_SilentCleanupOpenCode` execution that continues modifying open source after the VCmd command call itself returns. A Change Transaction Script must therefore establish a bounded **post-VCmd cleanup-convergence barrier** before any Git staging or commit begins.
- **The post-VCmd barrier is state-based, not a fixed sleep.** Pump the WinForms/Visual Studio message loop, allow ReSharper cleanup to run, invoke `File.SaveAll` at bounded intervals, and observe the transaction-owned files until their mechanical filesystem state remains unchanged for a meaningful quiet interval. A short unconditional sleep by itself is insufficient. If convergence cannot be established within the finite maximum wait, stop before Git capture while preserving the source/project progress already made.
- **Preexisting Git dirt is preserved, not rejected.** After the initial `File.SaveAll`, if an affected repository already contains tracked, staged, deleted, renamed, or untracked changes, intentionally stage that preexisting state, generate a repository-compliant commit message from the actual staged diff, commit it as a pre-transaction preservation checkpoint, and verify that the repository is clean before synchronization and transaction-owned mutation begin. Do this separately for each affected work tree.
- **A pre-transaction preservation checkpoint is not transaction-owned history.** Never remove, reset, rewrite, or roll back that commit merely because the later Change Transaction fails. It protects the maintainer's preexisting work and becomes part of the baseline from which the transaction proceeds.
- **`.Constants` and `.Interfaces` projects are dependency exceptions.** They do not require references to `xyLOGIX.Core.Debug` or to any `xyLOGIX.Core.Extensions*` project merely by repository convention. Add such references only when the actual code contract requires them.
- **`.Interfaces` dependency closure is still real.** An `.Interfaces` project may legitimately require `xyLOGIX.Core.Extensions` or another specific `xyLOGIX.Core.Extensions*` project when an interface inheritance/member-type dependency requires it—for example, when an interface extends `IForm`, `IControl`, or another contract whose assembly dependency makes that reference necessary. Determine the dependency from the actual inherited/member type closure rather than from the `.Interfaces` suffix alone.
- **`xyLOGIX.Core.Debug` is likewise demand-driven for `.Constants` and `.Interfaces`.** Do not add it to those project families merely to satisfy a blanket DBR-wide rule. If code in one of those projects actually calls `DebugUtils` or otherwise consumes that assembly, then the concrete dependency is legitimate and must be added through DTE.
- **Generation-time acceptance must honor these dependency exceptions.** Audits must reject both missing required dependencies and unnecessary blanket dependency additions to `.Constants`/`.Interfaces` projects.
- **Maintainer-authored source edits are generation-time authority.** When the maintainer changes a file after an earlier AI-generated transaction, tarball, desired-state payload, or assistant response, that newer maintainer-authored file state supersedes the older AI state for that path. The next transaction must preserve those edits and merge only the newly requested difference(s); never recreate a full-file payload from an earlier AI version merely because doing so is convenient.
- **Full-file clobbering is not permission to roll back maintainer work.** The clobbering rule applies only after the generator has constructed the complete desired file from the latest authoritative maintainer-edited state. If the generator knows a target has changed since its available snapshot and does not possess the current file contents, it must obtain the current file before producing a full-file replacement rather than guessing from stale source.

All prior requirements concerning exact-payload clobbering, progress-first failure handling, `$dte`, Windows PowerShell 5.1 compatibility, DTE-owned Visual Studio topology, junction-safe identity, the schema-version-2 noninteractive/Git-disabled VCmd sidecar, positive-only reference handling, exact staged-diff Git capture, and the two-pass exact-artifact audit remain in force.

## Purpose

This document is a reusable engineering specification for an AI system that generates **agentic PowerShell Change Transaction Scripts intended to be dot-sourced from the Visual Studio Package Manager Console (PMC)**.

A Change Transaction Script is an **in-IDE, repository-aware maintenance transaction**. It is used after the AI has inspected the current authoritative workspace and determined a concrete desired change, and the maintainer wants one downloadable `.ps1` artifact to impose that change inside the already-loaded Visual Studio Solution, prepare the resulting changed source for the normal Visual Studio/VCmd cleanup workflow, capture the result in Git, and leave the workspace in a useful state without forcing the maintainer to debug the automation itself.

Typical use cases include:

- clobbering one or more existing source/project files with audited desired-state replacements for a bug fix, behavioral correction, refactor, logging change, documentation change, UI adjustment, or configuration change;
- applying a coordinated multi-file change while keeping ordinary implementation commits file-by-file unless an explicit source-family, rename, scaffold, or topology exception applies;
- updating WinForms source and `*.Designer.cs` files while ensuring that only VCmd-eligible C# source is opened for cleanup, with `*.Designer.cs` remaining a mutation/Git artifact rather than a VCmd processing target;
- adding required project/assembly/package references without policing unrelated existing references;
- creating complete repository-standard project/module scaffolds and adding them to the loaded Solution through DTE;
- performing project/Solution topology operations such as renames when the task genuinely requires them;
- retrying a prior partially completed transaction without treating harmless source divergence, formatting changes, or an orphaned empty transaction boundary as a reason to fail;
- deriving the narrow VCmd-eligible C# processing set from the complete transaction-created changed-path set, supplying the one-run noninteractive/Git-disabled configuration for `VCmd.CCommandStripLineBreaksFromAllComments`, running that cleanup pass without modal prompts or VCmd-owned Git activity, and saving the IDE state before the script's own Git capture; and
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

Maintainer-authored source is stronger authority than AI-authored history. If the maintainer edits a file after an AI-generated transaction, those edits are an explicit live-state correction and become authoritative immediately for that path. An earlier tarball, earlier generated payload, prior Change Transaction Script, prior assistant response, or reconstruction of what the file "should" look like must never override newer maintainer-authored source.

If a target is known to have been edited after the newest source snapshot available to the generator, do not generate an exact full-file replacement from the stale snapshot. Obtain the current authoritative file contents first. If the current task genuinely cannot obtain the file and preserving unmodeled live content is essential, use the exceptional narrow structural-edit model from Section 18 only when it can preserve the unknown content safely.

Repository-specific commit-message instructions govern commit-message formatting. Current repository engineering guidance and the current xyLOGIX Software Engineering Manifesto govern source architecture and coding conventions.

### Generation-time audit and runtime clobbering rule

A Change Transaction Script is, by design, a **clobbering transaction** for the source/project targets that the current transaction authorizes.

The AI must do the source-shape reasoning **before delivery**, against the current authoritative workspace/tarball and any explicit live-state corrections supplied by the user. From that audit, generate the intended desired-state payloads and transaction operations.

### Maintainer-authored source preservation

The generator's first obligation is to preserve the maintainer's current source state. Exact-payload clobbering is a runtime transport/mutation technique; it does not grant the generator authority to replace newer maintainer code with an older AI-authored version.

For every existing-file payload:

1. Start from the newest authoritative version of that exact file, including maintainer edits made after earlier AI-generated scripts or tarballs.
2. Treat earlier AI-generated payloads, prior scripts, prior assistant responses, and older tarballs as historical context only when a newer maintainer-authored version exists.
3. Merge only the change(s) required by the current task into that authoritative file. Preserve unrelated implementation choices, logging, comments, XML documentation, formatting-sensitive content, method shapes, and other maintainer edits unless the current task specifically requires changing them.
4. Follow the repository's existing "Read Before You Write" rule: unchanged source remains faithful to the authoritative file and unrelated code/documentation is not opportunistically rewritten.
5. Before freezing an exact full-file payload, compare the desired file with the authoritative input and confirm that every substantive difference is transaction-owned or is a necessary direct consequence of the requested change.
6. If the generator knows the maintainer changed the target after the newest available snapshot but does not have those current bytes/text, obtain the current file before producing a full-file payload. Do not reconstruct it from stale AI output.

A maintainer edit can intentionally undo, restyle, expand, simplify, or otherwise alter an earlier AI change. That is not "source divergence" to be corrected by a later script. It is the new baseline. A later transaction may re-touch the same area only when the current task actually requires doing so, and it must otherwise preserve the maintainer's version.

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

The transaction must preserve preexisting Git work rather than requiring the maintainer to clean it manually. After the initial `File.SaveAll`, inspect each affected work tree. If it is dirty, intentionally stage the complete preexisting tracked/untracked state of that repository, generate a repository-compliant commit message from the actual staged diff, commit it as a pre-transaction preservation checkpoint, and verify that the work tree/index are then clean before synchronization and transaction-owned mutation begin. This preservation commit is maintainer history, not transaction-owned bookkeeping, and must never be removed by later transaction rollback/no-op cleanup. Dirt that appears only after the transaction baseline is established remains unrelated post-baseline work and must not hitchhike into transaction-owned commits.

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

### 6.4 IDE topology operations are DTE-owned

When Visual Studio exposes a project-system operation, use it instead of writing topology XML/text directly. In particular:

- add a new project with `$dte.Solution.AddFromFile(<csprojPath>)`;
- add a project reference through the consuming loaded project's VSProject/DTE reference collection, normally `References.AddProject(<targetProject>)`;
- add an assembly reference through the loaded project's DTE reference collection;
- add/remove source project items through `ProjectItems`/the project system when membership must change; and
- use DTE/Solution Items operations for Solution-item membership.

Do not hand-edit `.sln` project entries or `<ProjectReference>` XML merely because they are text. The IDE is the authority for those relationships and is responsible for writing the correct relative path representation. Exact-payload clobbering remains the default for authored source/project content, but topology operations are not ordinary text-payload mutations.

When adding a file already located under a project directory, construct the file pathname from the directory spelling exposed by that loaded project's project file. This reduces junction-spelling mismatches that can cause legacy project systems to persist an absolute `Compile Include` path.

### 6.5 Development-machine junction topology

On the current development machine, `%USERPROFILE%` is `C:\Users\Brian Hart`, while `%USERPROFILE%\source` is a directory junction to `D:\Users\Brian Hart\source`. Visual Studio, ReSharper, Git, and Win32/.NET APIs can therefore expose either spelling for the same physical object.

A Change Transaction Script must not:

- infer duplicate repositories solely from `C:` versus `D:` path spelling;
- require DTE and Git repository-root strings to compare literally equal;
- rewrite a relative `.sln` path into the physical `D:` target merely because canonicalization exposed it; or
- pass a differently-spelled junction target into DTE when a project-local path can instead be derived from the loaded project itself.

Use the Solution-containing directory as the Git working directory/repoRoot convention, let Git establish work-tree membership, and use canonical/physical identity only transiently when a true identity comparison is necessary. Never persist that canonicalized identity back into `.sln`/`.csproj` topology.

---

## 6.6 ReSharper suspension and IDE settling

Every Change Transaction Script running in the maintainer's Visual Studio environment must suspend ReSharper before performing source/project/Solution mutations. This prevents ReSharper and Visual Studio's Asset Synchronization Service from repeatedly re-indexing partially-written source and partially-mutated project topology.

After validating the loaded Solution and performing the initial `File.SaveAll`:

1. invoke `$dte.ExecuteCommand('ReSharper_Suspend')`;
2. mark ReSharper as transaction-suspended only after the command returns normally;
3. pump `[System.Windows.Forms.Application]::DoEvents()` and sleep in short bounded intervals so the IDE can settle; and
4. keep ReSharper suspended through every source write, source-item membership operation, project/Solution add operation, and reference operation.

A failure to suspend ReSharper before the first mutation is a legitimate pre-mutation stop condition for this environment because continuing can produce project-association/Miscellaneous-Files failures. If suspension was successful and the transaction later exits early, the outer cleanup must make a best-effort `ReSharper_Resume` call and pump the message loop before returning control to PMC.

Normal resumption occurs only after the transaction's **single final VCmd-eligible editor-opening pass** has completed. At that point:

1. invoke `$dte.ExecuteCommand('ReSharper_Resume')`;
2. clear the transaction's suspended-state flag when the command returns normally;
3. perform a bounded wait/message-pump settling period; and
4. only then prepare the VCmd sidecar and invoke VCmd.

Do not suspend/resume ReSharper separately for scaffold and implementation phases. One transaction run has one suspension interval.

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

### Required checkpoint B: after all source/project/Solution mutations

After **all** transaction source, source-item membership, project-reference, project-add, Solution-item, and other topology mutations are complete, run `File.SaveAll` so Visual Studio/project-system state is flushed before the one-time editor-cleanup pass begins.

There is no phase-local VCmd checkpoint. In particular, creating a scaffold is **not** followed by opening its `AssemblyInfo.cs`, invoking VCmd, closing it, and later repeating the process for implementation source. All VCmd-eligible affected files are accumulated into one transaction-wide registry and processed together at the final cleanup convergence point.

### Required checkpoint C: one final editor/VCmd convergence point before Git capture

The final cleanup boundary is:

1. complete every requested source/project/Solution mutation while ReSharper remains suspended;
2. run `File.SaveAll`;
3. resolve the complete transaction-created changed-path set for Git/scope purposes;
4. combine that status with the transaction-wide registry of VCmd-eligible affected paths so files changed and possibly checkpointed earlier in the run are not forgotten;
5. derive the final VCmd-eligible C# set using Section 9;
6. open that complete eligible set **once** in the explicit source/text editor, with a short bounded delay and message-pump cycle between files plus a final settling interval;
7. invoke `ReSharper_Resume`, then wait/pump for ReSharper/project synchronization to settle;
8. write the mandatory one-run VCmd sidecar;
9. invoke `VCmd.CCommandStripLineBreaksFromAllComments` **once and without command arguments** when the sidecar write succeeded;
10. enter the mandatory post-VCmd cleanup-convergence barrier described in Section 9;
11. while that barrier is active, pump `Application.DoEvents()`, perform bounded waits, and invoke `File.SaveAll` at bounded intervals so downstream `ReSharper_SilentCleanupOpenCode` activity can finish;
12. require the transaction-owned source set to remain mechanically unchanged for the configured quiet interval before Git capture begins;
13. run a final `File.SaveAll` after convergence;
14. refresh Git status/dirty-scope state; and
15. only then perform ordered Git capture.

The script must not close eligible documents between opening and VCmd. Leave the final opened set available to the IDE unless a current prompt expressly requires document closure.

Do **not** semantically verify, reparse, regex-check, marker-check, or compare source against expected payloads after VCmd cleanup. Mechanical state observation performed solely to determine whether IDE cleanup has become quiescent is allowed; it is a synchronization barrier, not source-correctness verification.

Do not run a build, compile, or test operation merely as a transaction-verification gate. If explicitly requested for information, report the result without turning it into a rollback trigger.

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

The fact that VCmd-eligible changed files are intentionally opened before VCmd does not authorize a blanket close of documents that were already open before the transaction.

---
## 9. One-Time VCmd Source Opening and Visual Commander Cleanup

When the repository workflow requires `VCmd.CCommandStripLineBreaksFromAllComments`, use one transaction-wide cleanup pass.

### Maintain a transaction-wide affected-file registry

As mutations are issued, record every authorized path that could be VCmd-eligible. This registry survives scaffold/topology/implementation phase boundaries and Git checkpoint decisions. The final VCmd set is derived from the union of:

- the transaction-wide affected-path registry; and
- the complete current Git changed-path set used for scope accounting.

This is necessary because a file such as a new project's `Properties\AssemblyInfo.cs` can be affected early in the run and must still participate in the one final editor-opening pass even if another transaction step has already checkpointed related topology.

The complete Git path set and the VCmd set remain different concepts. Git captures everything the transaction owns; VCmd sees only eligible C# source.

### VCmd eligibility rules

Include:

- ordinary changed/affected hand-authored `.cs` source; and
- `AssemblyInfo.cs`, which is an explicit supported exception because the VCmd command has special processing rules for it.

Exclude:

- `Global*.cs`, including `GlobalAspects.cs`;
- `*.Designer.cs`, including resource and WinForms designer files;
- `*.g.cs`, `*.i.cs`, `*.generated.cs`, and other known generated/derived/fixed-format C# artifacts;
- `bin`/`obj` or other build-output/intermediate source;
- `.csproj`, `.sln`, `.props`, `.targets`;
- `.resx`, `.config`, `.json`, `.xml`;
- `.md`, `.txt`;
- `.snk`, `.ico`, images, compiled outputs, or any other binary/resource/signing/scaffold artifacts; and
- any additional transaction-known generated/fixed-format artifact outside VCmd's intended cleanup domain.

The `AssemblyInfo.cs` inclusion rule takes precedence over the generic infrastructure classification. Eligibility is pathname/classification based; do not parse source contents at runtime to decide eligibility.

### Exactly one source-file-opening round per transaction

Do not run VCmd during scaffold creation. Do not open scaffold `AssemblyInfo.cs` files early and close them. Do not perform a second implementation opening pass.

After **all** mutations are complete and `File.SaveAll` has flushed project-system state:

1. derive the final eligible set;
2. process it in deterministic order;
3. for each file, verify it exists and remains an authorized/registered path;
4. resolve the file through its loaded project and prefer the real DTE `ProjectItem.Open(...)` operation with the explicit text/source view kind `{7651a700-06e5-11d1-8ebd-00a0c90f26ea}`; this keeps the document attached to project-system identity instead of opening the pathname as an arbitrary file;
5. use `$dte.ItemOperations.OpenFile(...)` only as a deliberately justified fallback when no usable project item can be resolved, and never use that fallback merely for convenience when a project-owned item exists;
6. call `Application.DoEvents()` and perform a short bounded wait after each open; and
7. after the last file, perform an additional bounded settling wait/message-pump cycle before resuming ReSharper.

The pacing and project-item-driven opening are deliberate. Opening a large set at full speed, or opening project-owned paths as arbitrary files before project-system association has settled, can cause otherwise valid project files to appear under **Miscellaneous Files**, after which CodeMaid/other IDE tooling may skip them. When practical, verify that the opened document still exposes a non-null project association after the bounded settling interval; if association is temporarily unavailable, wait/pump again within a finite retry budget rather than immediately spawning another arbitrary-file document.

An individual eligible-file open failure is warning-only; continue opening the rest. Excluded/non-C# paths are not opening failures because they were never candidates.

### Resume ReSharper, then invoke VCmd once

After the opening pass:

1. invoke `ReSharper_Resume`;
2. pump/wait for synchronization to settle;
3. write `%LOCALAPPDATA%\xyLOGIX, LLC\Visual Commander\Commands\Strip Line Breaks from All Comments\Config\.config.json` using exactly:

```json
{
  "SchemaVersion": 2,
  "SuppressPrompts": true,
  "EnableGitAwareness": false,
  "AutomaticallyCheckInChangesToGitWhenGitAwarenessIsSuppressed": false
}
```

4. when sidecar preparation succeeds, invoke `$dte.ExecuteCommand('VCmd.CCommandStripLineBreaksFromAllComments')` **once and without arguments**;
5. if sidecar preparation fails, warn and skip VCmd;
6. if VCmd is unavailable or throws, warn and preserve forward progress; and
7. run final `File.SaveAll` unconditionally.

The sidecar is rewritten immediately before the one invocation. VCmd Git awareness and VCmd automatic check-in remain disabled so the Change Transaction Script owns all Git behavior.

Do not semantically inspect source after VCmd. Do not run a second VCmd invocation merely because new projects were part of the transaction.

### Mandatory post-VCmd cleanup-convergence barrier

The return of `$dte.ExecuteCommand('VCmd.CCommandStripLineBreaksFromAllComments')` is **not** proof that all editor cleanup has finished. In the maintainer's environment, VCmd can trigger a downstream `ReSharper_SilentCleanupOpenCode` command that continues changing one or more open source files after the outer VCmd call has returned.

Therefore Git capture must not begin immediately after VCmd.

Use a bounded quiescence algorithm:

1. Immediately after VCmd returns (or after the VCmd attempt completes), call `File.SaveAll`.
2. Begin a bounded settling loop with a finite maximum duration.
3. On each iteration:
   - pump `[System.Windows.Forms.Application]::DoEvents()`;
   - sleep for a short bounded interval;
   - periodically call `File.SaveAll`; and
   - observe only the **mechanical state** of transaction-owned files needed to detect further editor writes, such as existence, length, and last-write timestamp.
4. Reset the quiet-interval timer whenever any transaction-owned file's observed mechanical state changes.
5. Require the complete transaction-owned file set to remain unchanged for a meaningful continuous quiet interval before declaring cleanup converged.
6. After convergence, call `File.SaveAll` one final time and pump the message loop again before refreshing Git status.
7. Only then begin staging/committing.

A single unconditional `Start-Sleep` is not sufficient because the duration of ReSharper cleanup depends on project-system/indexing load.

This barrier must not compare current source against expected hashes, parse source, inspect identifiers, or judge semantic correctness. Its only purpose is to prevent Git from committing a file while ReSharper/IDE cleanup is still rewriting it.

If the finite maximum settling duration expires without achieving the required quiet interval:

- report the condition as an actionable synchronization error/warning;
- do **not** begin Git staging/commit capture;
- preserve all source/project progress already made; and
- return control to the maintainer so the next transaction can resume safely.

This is a legitimate mechanical pre-Git gate because staging while the IDE is still writing transaction-owned source can produce a commit followed immediately by a newly dirty file.

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

The transaction's fatal runtime gates are limited to conditions required to continue mechanically and safely: authoritative Solution/repository/path identity, required Git availability for a Git-integrated transaction, ability to preserve preexisting Git work and establish a clean baseline, ability to issue the authorized mutation, successful post-VCmd cleanup convergence before staging, and exact Git staging/commit postconditions. Changed-file source-editor opening, preparation of the noninteractive/Git-disabled VCmd sidecar, and VCmd cleanup are **best-effort cleanup operations**: failures are diagnosed as warnings and do not invalidate successful source/project mutations. A sidecar-preparation failure means **skip VCmd**, not invoke it interactively. Source-byte/hash/layout/semantic equivalence and lint/style/static-analysis results are not runtime gates.

### If a build or test is explicitly requested

A current prompt may expressly ask the script to invoke a build, compilation, or test operation. In that case:

1. Run it only after the source/project mutation and required VCmd-eligible editor-opening/VCmd/SaveAll work are complete.
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

The required VCmd pass is not a speculative formatting verifier. It is an intentional editor-cleanup action performed after the transaction derives and opens only the VCmd-eligible changed C# source set in the source-code editor.

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

A DTE pathname and Git's canonical pathname can spell the same physical repository differently because of directory junctions or symlinks. Never require literal pathname equality. On the current development machine, the `C:\Users\Brian Hart\source` junction and its `D:\Users\Brian Hart\source` target are the canonical example of this rule.

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

## 13. Preserve Preexisting Git Work and Establish a Clean Baseline

A dirty repository at transaction start is **not** a reason to refuse to run. The transaction first preserves that work in Git, then establishes the clean baseline required for exact transaction-owned staging.

After the initial `File.SaveAll` and before initial pull/rebase or transaction-owned source/project mutation, process each affected Git work tree separately:

1. Parse `git status --porcelain` or an equivalent machine-readable status.
2. If the repository is already clean, record the clean baseline and continue.
3. If the repository is dirty, emit a concise diagnostic such as `*** INFO *** Committing preexisting repository changes before the transaction begins...`.
4. Snapshot the complete set of preexisting tracked, staged, deleted, renamed, copied, and untracked paths for that repository.
5. Intentionally stage that preexisting repository state. This is the one phase where staging all preexisting dirt in the affected work tree is deliberate rather than hitchhiking.
6. Verify the staged names represent the preexisting dirty set and that no path from another Git work tree is present.
7. Generate a repository-compliant commit message from the **actual staged diff**. The console diagnostic may describe the operation generically, but the Git commit message itself must still obey the repository's dedicated commit-message instructions.
8. Commit through the normal UTF-8-no-BOM temporary-message-file workflow.
9. Verify commit success and verify that the work tree/index are clean.
10. Record that commit as the **pre-transaction preservation checkpoint**.
11. Only after the repository is clean may the script continue to initial pull/rebase and the transaction boundary.

The preservation checkpoint is not a transaction-owned empty boundary and is not an implementation commit. It protects work that existed before the Change Transaction began. Never erase, reset, amend away, or roll it back merely because the later transaction fails, becomes a no-op, or requires a follow-up correction.

If the preexisting state cannot be committed mechanically—for example because Git author identity is unavailable, the staging set cannot be proven, a Git hook prevents the commit, or the repository is in an unresolved merge/rebase/conflict state—stop **before transaction-owned mutation** with an actionable diagnostic. Do not discard the preexisting work.

After the preservation checkpoint, and again after any initial pull/rebase, verify that the repository is clean. That clean point is the transaction baseline used to distinguish later transaction-owned changes from unrelated changes that appear concurrently.

If an unrelated dirty path appears **after** the transaction baseline was established:

1. report it as `*** WARNING ***`;
2. never stage, reset, restore, or otherwise absorb that unrelated path;
3. continue staging/committing the authorized transaction paths when their exact staging scope can still be proven; and
4. skip the final pull/rebase and push while unrelated dirty work remains, because synchronization could interfere with work the transaction does not own.

When parsing porcelain output, use ordinary PowerShell arrays or `ArrayList`; avoid generic-list binder patterns that are fragile in Windows PowerShell 5.1.

## 14. Initial Git Synchronization

When the current branch has a configured upstream, perform the initial synchronization before source modification.

The normal default, after Section 13 has preserved any preexisting dirt and established a clean baseline, is:

```text
git pull --rebase
```

Do not use `--autostash` merely to bypass the preservation-checkpoint workflow. Preexisting work should already have been committed before synchronization.

Preconditions:

- Git is available.
- Repository identity is established.
- Any preexisting dirt has been preserved through the Section 13 checkpoint.
- The working tree and index are clean.
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

This runtime rule assumes the payload already passed the generation-time maintainer-source-preservation audit above. It must never be interpreted as permission for the generator to build that payload from stale AI-authored source and thereby roll back newer maintainer edits. The runtime script remains intentionally non-semantic; source precedence and preservation are resolved before delivery.

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
2. for every previously existing target, begin from the newest maintainer-authored version of that file and preserve all unrelated maintainer changes;
3. produce the complete desired file by applying only the current transaction-owned change(s) to that authoritative baseline;
4. compare the desired file with the authoritative baseline and reject any unrelated rollback, reversion, documentation rewrite, or source reshaping that the current task did not require;
5. audit that payload for architectural/style/correctness requirements before delivery; and
6. embed or otherwise supply that exact payload to the Change Transaction Script.

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

- pre-transaction preservation of preexisting Git dirt;
- initial pull;
- boundary commit;
- one or more file writes;
- project-reference modification;
- VCmd-eligible changed-file editor opening;
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
- Re-derive the current transaction's VCmd-eligible changed C# set and open those eligible files in the source-code editor before VCmd even when some are already open from an earlier attempt; opening an already-open eligible document is a harmless retry condition.
- Skip VCmd when no VCmd-eligible changed C# files can be opened.
- Preserve partial forward progress after advisory/runtime correctness concerns and fix those concerns in the next transaction rather than automatically erasing the work.

---

## 20. Visual Studio Project/Reference/Topology Changes

Topology changes are allowed when required by the architecture. They must be coherent and verified at the filesystem, project-system, Solution, namespace, and Git levels.

### 20.1 Adding and verifying assembly/project/package references

Reference handling in a Change Transaction Script is **positive-only**.

The script is not the authoritative judge of whether every preexisting project, assembly, or package reference is necessary. Its responsibility is limited to references that the current transaction itself requires or that the current prompt explicitly instructs it to remove.

When adding a required reference:

1. Locate the actual loaded consuming and target projects through DTE.
2. Confirm the intended target reference.
3. Check whether that required reference already exists when convenient.
4. For a project reference, call the consuming project's DTE/VSProject `References.AddProject(targetProject)` operation.
5. For a framework/assembly reference, use the corresponding DTE reference-add API.
6. Save with `File.SaveAll`.
7. If the DTE reference-add operation returned normally, mark the positive mutation and continue.

Do not hand-edit `<ProjectReference>` XML when DTE can express the operation. Do not reread the `.csproj` merely to prove persistence as a fatal gate. An optional diagnostic observation may produce a warning, but a successful DTE/reference-add call is the positive mutation boundary.

### Validator-interface dependency closure

When a class-library project declares or consumes a singleton validator dependency property typed as an `IXXXValidator` interface, ensure that the project also references `xyLOGIX.Validators.Data.Interfaces`. xyLOGIX validator interfaces extend the shared `IDataValidator` contract defined by that project, so this reference is part of the required dependency closure.

For a Change Transaction Script, this is a positive required-reference operation:

1. Identify each project touched by the transaction that declares or newly consumes such an `IXXXValidator` dependency.
2. Ensure `xyLOGIX.Validators.Data.Interfaces` is loaded/locatable through the actual Solution project graph.
3. Add the project reference when absent.
4. Save through the normal DTE/`File.SaveAll` boundary.
5. Continue without policing or removing unrelated existing references.

This rule is especially important when the consuming source appears to reference only the specialized validator-interface assembly: the inherited `IDataValidator` contract still makes `xyLOGIX.Validators.Data.Interfaces` a required project dependency.

### `.Constants` and `.Interfaces` dependency exceptions

Do not impose a blanket `xyLOGIX.Core.Debug` or `xyLOGIX.Core.Extensions*` project-reference requirement on projects whose names end in `.Constants` or `.Interfaces`.

For a `.Constants` project:

- do not add `xyLOGIX.Core.Debug` merely by convention;
- do not add `xyLOGIX.Core.Extensions` or another `xyLOGIX.Core.Extensions*` project merely by convention; and
- add one of those dependencies only when the actual code in the project genuinely consumes a type/member from that assembly.

For an `.Interfaces` project, apply the same default, but evaluate the **actual contract dependency closure**. An interface project may legitimately require `xyLOGIX.Core.Extensions` or a specific related extensions assembly when the declared interface contract itself depends on it. Examples include an interface that extends `IForm`, `IControl`, or another base interface/type whose defining assembly requires that project reference, or an interface member whose exposed type resides in that dependency.

Likewise, an `.Interfaces` project may require `xyLOGIX.Core.Debug` if its own source genuinely consumes that assembly. The rule is therefore not “interfaces may never reference these projects”; it is “do not add them as boilerplate.”

When generation-time auditing dependencies:

1. inspect actual base interfaces and exposed member types;
2. identify the narrowest project dependency closure needed to resolve those contracts;
3. add only the concrete required project reference(s) through DTE; and
4. do not classify the absence of `xyLOGIX.Core.Debug`/`xyLOGIX.Core.Extensions*` as a defect merely because the project suffix is `.Constants` or `.Interfaces`.

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
4. Add the project to the loaded Solution specifically with `$dte.Solution.AddFromFile(<csprojPath>)`; never synthesize the `.sln` project entry.
5. Let Visual Studio persist the normal relative Solution path; never replace it with a canonicalized `C:`/`D:` absolute path.
6. Run `File.SaveAll`.
7. Add implementation source/items and required references through the loaded project system/DTE, preserving scaffold-first mutation order.
8. Defer VCmd/editor cleanup until the one final transaction-wide pass described in Section 9.
9. During ordered Git capture, stage/commit the scaffold/Solution-membership work item before implementation work items so history still presents a complete scaffold first.

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
7. While closed, update only filesystem-coupled authored/project metadata that cannot be expressed through the loaded IDE, such as the renamed project directory/`.csproj` filename, assembly/root namespace metadata, authored namespaces/usings, and other convention-coupled file content. Do **not** hand-edit `.sln` project entries or `<ProjectReference>` paths.
8. Preserve project GUIDs unless identity is intentionally changed.
9. Reopen the same captured Solution through `$dte.Solution.Open(...)`.
10. Pump/wait with finite retries.
11. Rediscover projects from DTE; use DTE to remove stale/missing project membership, add the renamed project with `$dte.Solution.AddFromFile(...)`, and restore required project references through `References.AddProject(...)`.
12. `File.SaveAll` again and verify the loaded graph.

Rollback in reverse order on failure when feasible and surface rollback failures explicitly.

### 20.4 Rename-aware Git capture

A rename is normally one logical work item. Keep old/new paths together. Include directly coupled topology files only when splitting them would leave an invalid intermediate project/Solution state. Temporary scoped staging may be used for Git rename detection, but it must be reset immediately before final granular staging.

### 20.5 WinForms `*.Designer.cs` partial-class accessibility and VCmd exclusion

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
6. Preserve and capture the changed `*.Designer.cs` path through the normal Git workflow.
7. Exclude the `*.Designer.cs` path from the VCmd processing set. Do **not** open it merely so `VCmd.CCommandStripLineBreaksFromAllComments` can process it.
8. If some other explicit transaction operation genuinely requires opening the designer source file, force the source-code/text editor and never activate the WinForms Designer.

A changed `*.Designer.cs` file remains an authorized source/Git artifact, but it is **not** a VCmd cleanup target. The complete Git changed-path set and the VCmd processing set must remain distinct.

---
## 21. Source Correctness Is a Generation-Time Responsibility

The AI generating the Change Transaction Script should audit its intended source/project payloads carefully **before delivering the script**.

At runtime, however, the script must not become a second compiler, analyzer, architectural reviewer, or semantic arbiter.

Therefore:

- source-shape reasoning belongs to generation-time audit, not runtime rediscovery;
- exact desired-state full-file payloads are the default mutation mechanism whenever feasible;
- positive source/project mutations are presumed correct once their underlying operations return normally;
- source hashes, regexes, marker checks, declaration checks, method-body locators, old-block searches, AST/source-shape checks, reference checks, API checks, and similar semantic inspections are not fatal runtime gates;
- the VCmd-eligible editor-opening pass is a mechanical preparation step over a deliberately filtered C# subset, not a source-validation step and not a proxy for the complete Git changed-path set;
- VCmd is invoked only after the exact one-run noninteractive/Git-disabled sidecar has been written successfully, and VCmd cleanup is trusted completely once invoked successfully;
- no semantic/textual source verification occurs after VCmd;
- no build, compile, or test gate occurs after VCmd or elsewhere by default; and
- Git capture should preserve the transaction's resulting state so the maintainer can inspect it in the IDE.

Runtime diagnostics should use `Write-Host` to report useful progress and warnings, but they must not undo successful positive modifications.

---


### 21.1 Repository coding-preference acceptance audit

A Change Transaction Script may be mechanically perfect and still be defective if the source payloads embedded into it violate the repository's coding contract. The generator must therefore treat the current maintainer instructions, `CONTRIBUTING.md`, `.github/copilot-instructions.md`, and the authoritative source conventions as **generation-time acceptance criteria** before payload bytes are frozen.

For the current DiagnosticBatchRunner repository, the pre-delivery source audit must include, at minimum:

- every non-void method that is required by the repository's fault-tolerance convention to use a return-value accumulator declares its correctly typed `result` variable near the top, initializes it to the documented default/invalid value, routes eager failure returns through `result`, resets `result` in exception handling where applicable, and returns `result` through the method's intended return path rather than bypassing it with direct literals, direct `new ...` expressions, or direct subordinate-method returns;
- newly introduced factory APIs use fluent names that describe the supplied collaborators or selection semantics rather than retaining a generic name such as `FromScratch` when the method is materially parameterized; for example, a terminal-control Presenter factory receiving a View and terminal buffer should use a fluent form such as `ForViewAndTerminalBuffer(...)`;
- every source file containing a call to `DebugUtils` includes `using xyLOGIX.Core.Debug;`;
- every source file using `[DebuggerStepThrough]` has the required `System.Diagnostics` namespace available through an appropriate `using` directive or an already-established equivalent;
- every DiagnosticBatchRunner/`DBR.*` project that actually requires `xyLOGIX.Core.Debug` has that project reference, added through the loaded project's DTE/VSProject reference collection rather than by writing `<ProjectReference>` XML; projects ending in `.Constants` or `.Interfaces` are not given `xyLOGIX.Core.Debug` merely by convention;
- `.Constants` and `.Interfaces` projects are not given `xyLOGIX.Core.Extensions*` references merely by convention; an `.Interfaces` project receives the specific extensions dependency when its inherited/member type closure requires it, including contracts such as `IForm`/`IControl` when applicable;
- required `using` directives and required positive project dependencies are audited together so a generated source call cannot be delivered in a state that ReSharper will immediately report as an unresolved symbol;
- fields precede the properties they back, property accessors follow the repository's `[DebuggerStepThrough]`/statement-body conventions, and generated source does not introduce prohibited direct-return, expression-bodied, region, local-function, or other shapes identified by the current repository instructions; and
- the exact payload set is scanned for the same class of violation across **all** files introduced or substantively modified by the transaction, rather than correcting only the first file or first ReSharper error that exposed the pattern; and
- every payload for an existing file is diffed against the newest authoritative maintainer-authored version of that file, with all non-transaction-owned maintainer changes preserved and no older AI-generated version allowed to overwrite them.

ReSharper's Errors/Warnings in Solution export is valuable generation-time evidence when the maintainer supplies it. Treat the latest export as an acceptance input and repair the reported compiler-resolution problems in the generated desired state, but do not limit the audit to those reported locations: inspect sibling/generated transaction source for the same underlying defect class before delivery.

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

### 22.5 New projects/modules: complete scaffold first, single cleanup pass, scaffold commit first

Project creation remains a deliberate commit-granularity exception, but it must not create an extra VCmd/opening cycle.

Mutation order:

1. Create the complete repository-standard scaffold.
2. Add each project using `$dte.Solution.AddFromFile(...)`.
3. Add source project items and required project/assembly references through DTE/project-system operations.
4. Add functional implementation only after the scaffold exists and the project is loaded.
5. Keep ReSharper suspended through all of those mutations.
6. After **all** transaction mutations are complete, perform the single final Section 9 opening/resume/VCmd pass.

Git history order is then created by exact staging:

7. Stage and commit the complete scaffold/module-family plus its DTE-produced Solution membership as the first project-creation work item. Functional source that already exists in the working tree remains unstaged.
8. Stage and commit implementation work afterward using normal file-by-file/source-family/rename-aware selection.

Thus the repository history still contains a scaffold checkpoint before implementation commits, while the IDE performs only one source-file-opening/VCmd round for the entire transaction.

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

After the mandatory post-VCmd convergence barrier and ordered commits:

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
- detection and preservation-commit capture of preexisting repository dirt when present;
- clean-baseline establishment and synchronization progress;
- boundary/scaffold/implementation phase transitions;
- meaningful source/project mutation progress;
- legitimate no-op decisions;
- complete changed-path discovery after mutation for Git/scope accounting;
- derivation of the narrower VCmd-eligible C# set, including useful summary diagnostics for excluded categories when applicable;
- each VCmd-eligible changed C# file being opened in the Visual Studio source-code editor;
- confirmation that the complete VCmd-eligible opening pass finished before VCmd;
- VCmd sidecar preparation, confirmation that the invocation is noninteractive/Git-disabled, and VCmd invocation/completion or warning-only skip/failure;
- post-VCmd `ReSharper_SilentCleanupOpenCode` convergence progress and the quiet-interval barrier before Git capture;
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

- Refusing to run solely because an affected repository contains preservable preexisting tracked/untracked changes; preserve them in a repository-compliant checkpoint first.
- Beginning Git staging immediately when the outer VCmd command returns, without waiting for downstream `ReSharper_SilentCleanupOpenCode`/IDE writes to converge.
- Adding `xyLOGIX.Core.Debug` or `xyLOGIX.Core.Extensions*` to every `.Constants` or `.Interfaces` project merely as boilerplate.
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
- Opening `.csproj`, `.sln`, `.resx`, `.config`, `.json`, `.xml`, `.props`, `.targets`, `.md`, `.txt`, `.snk`, resource, or other non-C# artifacts merely because Visual Studio can open some of them as text.
- Opening `Global*.cs`, `*.Designer.cs`, `*.g.cs`, `*.i.cs`, `*.generated.cs`, or another known generated/fixed-format C# artifact merely because its extension is `.cs`.
- Excluding a changed `AssemblyInfo.cs` from VCmd merely because it is infrastructure/fixed-format source; `AssemblyInfo.cs` is the explicit supported exception.

Good examples:

- Detecting preexisting repository dirt after `File.SaveAll`, committing it with a repository-compliant message, and then establishing the clean transaction baseline.
- Waiting after VCmd until the transaction-owned file set remains mechanically stable for a continuous quiet interval while pumping the IDE and periodically saving.
- Evaluating `.Interfaces` dependencies from actual base-interface/member-type closure, adding `xyLOGIX.Core.Extensions` only when a contract such as `IForm`/`IControl` genuinely requires it.
- Resolving the authorized target pathname and then clobbering it with the pre-audited exact desired-state payload without inspecting old source contents.
- Transporting large/complex audited payloads as exact Base64-encoded bytes and writing them with `WriteAllBytes` to avoid PowerShell text-encoding/quoting hazards.
- Tracking whether the first meaningful positive mutation has succeeded so a pre-mutation failure can clean up only the transaction-owned empty boundary.
- Resolving the complete transaction-created changed-path set for Git, then deriving a separate VCmd set containing only ordinary hand-authored changed `.cs` files plus changed `AssemblyInfo.cs`.
- Excluding `Global*.cs`, `*.Designer.cs`, generated C# source, project/scaffold metadata, resources, configuration, documentation, signing material, and binary artifacts from VCmd preparation.
- Warning and continuing when one VCmd-eligible source file cannot be opened, while running cleanup against the eligible files that did open.
- Writing the exact schema-version-2 noninteractive/Git-disabled VCmd sidecar immediately before invocation so the command cannot display its confirmation message box(es) or perform Git work.
- Warning and skipping VCmd when its sidecar cannot be prepared, rather than invoking the command with default interactive/Git-aware behavior.
- Warning and continuing when VCmd itself is unavailable, followed by the unconditional final `File.SaveAll`.
- Checking document count before a close-all operation that is actually required.
- Verifying that a VCmd-eligible changed path exists before trying to open it as source text.
- Applying simple pathname/classification eligibility rules before asking Visual Studio to open a changed file for VCmd, without inspecting source contents.
- Explicitly requesting the source-code/text editor so an eligible WinForms primary `.cs` file cannot default to the Designer.
- Verifying Git staging contains exactly the intended path before commit.
- Confirming that the required DTE reference-add operation returned normally and then flushing with `File.SaveAll`, without making a `.csproj` reread a fatal gate.
- Re-resolving authorized target paths after `git pull` when repository topology may have changed.

The question to ask before every gate is:

> Does failure of this condition actually make the next action unsafe or nonsensical?

If not, do not make it a hard gate.

---

## 26. Recommended High-Level Execution Order

### Phase 1 — establish Visual Studio and Git context

1. Enter child scope; strict errors.
2. Validate host `$dte` without binding/assigning it.
3. Validate the loaded Solution and derive its directory.
4. Discover loaded projects from DTE.
5. `File.SaveAll`.
6. Invoke `ReSharper_Suspend`, mark the suspension state, and perform a bounded wait/message-pump settling cycle.
7. Resolve authorized targets and Git repoRoot(s), using junction-safe identity rather than path-string equality.
8. Inspect each affected repository for preexisting dirt.
9. For every dirty affected repository, intentionally stage the preexisting state, generate a repository-compliant message from the actual staged diff, commit it as the pre-transaction preservation checkpoint, and verify the repository is clean.
10. Remove a recognized orphan transaction boundary when allowed, synchronize from upstream when configured, revalidate the clean baseline, and establish the new transaction boundary.

### Phase 2 — perform every topology/source mutation while ReSharper is suspended

1. Create complete new-project scaffold(s) first when required.
2. Add new projects with `$dte.Solution.AddFromFile(...)`; never hand-edit `.sln` project entries.
3. Add/remove source project items through the loaded project system when membership changes.
4. Add project references with DTE/VSProject `References.AddProject(...)` and assembly references through DTE.
5. Apply exact audited source payloads in reference/dependency order.
6. Record each VCmd-eligible affected path into the transaction-wide registry as mutations occur.
7. Mark meaningful positive mutation immediately after the first successful positive operation.
8. Do not run phase-local VCmd/editor-opening passes.
9. Run `File.SaveAll` after all mutations.

### Phase 3 — one final IDE cleanup convergence point

1. Resolve the complete Git changed-path set.
2. Union it with the transaction-wide affected-path registry and derive the final VCmd-eligible C# set.
3. Open the eligible set exactly once in the explicit source/text editor, pacing each open with bounded wait/message pumping.
4. Perform a final opening-pass settling interval.
5. Invoke `ReSharper_Resume`, clear the suspension flag, and perform a bounded ReSharper/project-system settling interval.
6. Prepare the exact schema-version-2 noninteractive/Git-disabled VCmd sidecar.
7. Invoke argumentless `VCmd.CCommandStripLineBreaksFromAllComments` once when sidecar preparation succeeds.
8. Enter the bounded post-VCmd cleanup-convergence barrier, pumping the IDE, periodically saving, and resetting the quiet interval whenever transaction-owned file state changes.
9. Require a continuous quiet interval before Git capture; if convergence times out, stop before staging while preserving forward source/project progress.
10. Run final `File.SaveAll` after convergence.
11. Leave opened eligible source files open by default.

### Phase 4 — ordered Git capture

1. Refresh status and scope.
2. Capture scaffold/module-family topology first by exact staging when new projects were created; implementation files remain unstaged until their turn.
3. Capture ordinary implementation file-by-file except explicit source-family/rename/topology units.
4. Generate commit messages from the actual staged diff.
5. Never stage unrelated dirt.
6. Perform final pull/rebase/push only when upstream is configured and no unrelated dirt prevents safe synchronization.

### Phase 5 — cleanup

1. Remove temp files and dispose owned resources.
2. If ReSharper remains marked suspended because normal resumption was not reached, make a best-effort `ReSharper_Resume` call and pump/wait before returning to PMC.
3. Preserve meaningful forward progress; only clean up bookkeeping-only boundaries under the established rules.
4. Never release/rebind/destroy `$dte`.

## 27. Side-Effect Gate Matrix

| Action | Immediate precondition | Required postcondition | No-op / recovery |
|---|---|---|---|
| `File.SaveAll` | Valid host DTE + loaded Solution | Command completes | Execute at defined checkpoints |
| Preserve preexisting Git dirt | affected repository identified + initial `File.SaveAll` complete + preexisting dirt present | all preexisting tracked/untracked state committed with repository-compliant message; repository clean | already clean: no-op; commit failure: stop before transaction mutation without discarding work |
| Clobber authorized source/project file | authorized target path + pre-audited desired-state payload + writable target | exact payload bytes written; mark meaningful mutation | do not inspect old source shape; exact-byte/Base64 transport is preferred for complex payloads |
| Pre-mutation failure after empty boundary | transaction-owned boundary exists + no meaningful positive mutation succeeded + baseline still clean | `HEAD` returned to pre-boundary anchor; boundary removed | preserve/report unexpected dirty work instead of discarding it |
| Successful no-op transaction | transaction-owned boundary exists + zero implementation commits + repo clean | `HEAD` returned to pre-boundary anchor; bookkeeping-only boundary removed | report no-op as success |
| Create project scaffold | Correct repo + analogous scaffold identified | Full expected scaffold exists and metadata is coherent | Empty dir: populate; known partial scaffold: complete; unexpected ownership ambiguity: stop |
| Add project to Solution | Scaffold `.csproj` exists | DTE add operation returns normally + `File.SaveAll` completes | Already loaded correctly: skip |
| Scaffold commit | Complete verified scaffold(s) + Solution membership; clean index | Entire project/module-family scaffold committed as one atomic add | Existing verified scaffold commit: reuse/skip |
| Add required reference | Target loaded + required reference absent | DTE reference-add returns normally + `File.SaveAll` completes | Already present: skip; unrelated extra references: ignore |
| Rename project/folder | Solution closed + rollback state captured | old absent/new present; topology updated; same Solution reopened | bounded retry/rollback |
| Discover changed files for Git/scope accounting | source/project mutations saved | complete transaction-created changed-path set resolved | no changed paths: successful no-op |
| Derive VCmd processing set | complete changed-path set resolved | pathname/classification filter yields only eligible changed C# files; `AssemblyInfo.cs` included; generated/fixed-format/non-C# artifacts excluded | zero eligible paths: skip VCmd |
| Open eligible file for VCmd | authorized changed path + VCmd-eligible existing C# file | project-owned files open through their DTE `ProjectItem` in the source-code/text editor, not WinForms Designer or Miscellaneous Files | unresolved/unopenable eligible path: bounded wait/retry then warn/skip; already open as project-owned text: reuse; excluded paths are not opening candidates |
| Prepare VCmd one-run sidecar | complete VCmd-eligible opening pass finished + at least one eligible C# file opened | canonical `.config.json` written with schema `2`, prompt suppression enabled, both VCmd Git behaviors disabled | preparation failure: warn, skip VCmd, continue to unconditional SaveAll/Git capture |
| VCmd cleanup | sidecar preparation succeeded + at least one VCmd-eligible C# file opened | one **argumentless** VCmd attempt + unconditional final SaveAll; successful cleanup trusted with no post-VCmd semantic check | zero eligible/opened files: skip; VCmd failure: warn and continue; command resets sidecar defaults on normal `Run` exit |
| Post-VCmd cleanup convergence | VCmd attempt completed or was skipped after the final opened-source state was established | transaction-owned files remain mechanically unchanged for the configured continuous quiet interval; final `File.SaveAll` completed before Git capture | timeout: stop before staging/commit, preserve forward source/project progress, report synchronization problem |
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
- [ ] `ReSharper_Suspend` is invoked through `$dte.ExecuteCommand(...)` before the first mutation, followed by bounded wait/message pumping.
- [ ] ReSharper suspension state is tracked so early-failure cleanup can best-effort resume it.
- [ ] The exact delivered `.ps1` artifact has been parsed/static-checked for PowerShell 5.1 compatibility after it was written to disk.

### Project creation

- [ ] The closest analogous existing project(s) were inspected.
- [ ] Every new project has the complete expected scaffold, not a minimal skeleton.
- [ ] `GlobalAspects.cs`, `AssemblyInfo.cs`, resources, README, signing key, packages, config, icon, and other normal peer files are included/adapted when the analogous project has them.
- [ ] New-project creation correctly treats `GlobalAspects.cs`/`AssemblyInfo.cs` as scaffold exceptions to the ordinary no-regeneration rule.
- [ ] Empty or known-partial project directories are retry-safe; unexpected contents stop the transaction.
- [ ] New projects are added with `$dte.Solution.AddFromFile(...)`; `.sln` project entries are never hand-authored.
- [ ] Project references are added through the consuming loaded project's DTE/VSProject reference collection, not by writing `<ProjectReference>` XML.
- [ ] Visual Studio is allowed to persist relative project/reference paths; junction-canonicalized absolute `C:`/`D:` spellings are not injected into `.sln`/`.csproj`.
- [ ] New project/module-family scaffolds are captured atomically as the first project-creation commit before implementation commits; functional source may already exist unstaged because the single VCmd pass occurs after all mutations.
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
- [ ] Preexisting tracked/untracked repository dirt is preserved in a repository-compliant pre-transaction checkpoint rather than causing an abort; the transaction baseline is clean only after that preservation step.
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
- [ ] `.Constants` and `.Interfaces` projects are not given `xyLOGIX.Core.Debug` or `xyLOGIX.Core.Extensions*` references merely by convention.
- [ ] For an `.Interfaces` project, actual base-interface/member-type dependency closure was inspected; a required `xyLOGIX.Core.Extensions*` reference (for example through `IForm`/`IControl`) is added when genuinely needed.
- [ ] No existing project/assembly/package reference is rejected, removed, or treated as an error merely because it appears unused or unnecessary.
- [ ] Existing references are removed only when the current task explicitly requires removal of the specific reference.
- [ ] Project renames occur only while the Solution is closed and use finite retries/rollback.
- [ ] After mutations and `File.SaveAll`, the script resolves the complete transaction-created changed-path set for Git/scope accounting.
- [ ] The script maintains one transaction-wide registry of VCmd-eligible affected files across scaffold/topology/implementation phases.
- [ ] There is exactly one VCmd-eligible source-file opening pass per transaction run; no scaffold-local opening/VCmd cycle exists.
- [ ] Eligible files are opened with bounded pacing and `Application.DoEvents()` pumping so project association can settle.
- [ ] `ReSharper_Resume` occurs only after the full opening pass, followed by bounded wait/message pumping before VCmd.
- [ ] The script derives a separate VCmd processing set rather than equating "changed" or "text/source-editable" with VCmd eligibility.
- [ ] The VCmd processing set contains ordinary changed hand-authored `.cs` files and explicitly includes changed `AssemblyInfo.cs`.
- [ ] The VCmd processing set excludes `Global*.cs`, `*.Designer.cs`, `*.g.cs`, `*.i.cs`, `*.generated.cs`, other known generated/fixed-format C# artifacts, and build-output/intermediate source.
- [ ] The VCmd processing set excludes all non-C# artifacts, including project/Solution/build metadata, resources, configuration/data files, documentation/text files, signing keys, icons, and other binary/scaffold artifacts.
- [ ] Every VCmd-eligible changed C# file is **attempted** before VCmd in the Visual Studio source-code/text editor.
- [ ] Project-owned VCmd-eligible files are preferentially opened through their actual DTE `ProjectItem.Open(...)` relationship; `$dte.ItemOperations.OpenFile(...)` is not used as the default for project-owned source.
- [ ] The paced opening loop allows bounded project-association settling so files are not unnecessarily relegated to **Miscellaneous Files**.
- [ ] Individual eligible source-editor open failures are warning-only and do not roll back successful source/project mutations.
- [ ] Excluded changed paths remain part of the transaction/Git changed set and are not opened merely for VCmd.
- [ ] Eligible WinForms primary `.cs` files are explicitly opened as source text; the transaction never activates the WinForms Designer for cleanup.
- [ ] VCmd eligibility is determined mechanically from path/classification rules established during generation-time audit; runtime source contents are not parsed to decide eligibility.
- [ ] Immediately before the single VCmd invocation, the script writes `%LOCALAPPDATA%\xyLOGIX, LLC\Visual Commander\Commands\Strip Line Breaks from All Comments\Config\.config.json` using schema `2` with `SuppressPrompts = true`, `EnableGitAwareness = false`, and `AutomaticallyCheckInChangesToGitWhenGitAwarenessIsSuppressed = false`.
- [ ] VCmd is never invoked when that sidecar preparation fails; the failure is warning-only, VCmd is skipped, and final `File.SaveAll`/script-owned Git capture continue.
- [ ] `VCmd.CCommandStripLineBreaksFromAllComments` is invoked without command arguments; no `NoPrompt` argument or equivalent is used.
- [ ] The VCmd sidecar disables all VCmd-owned Git behavior so the Change Transaction Script remains solely responsible for synchronization, staging, custom commit-message generation, commits, and push.
- [ ] VCmd is attempted only after the complete VCmd-eligible opening pass has finished and successful sidecar preparation, and normally once for the successfully opened eligible set; VCmd failure is warning-only and final `File.SaveAll` still occurs.
- [ ] After VCmd returns, the script does not begin Git capture immediately; it enters a bounded post-VCmd convergence barrier that pumps the IDE, periodically calls `File.SaveAll`, and waits for a continuous quiet interval across transaction-owned files.
- [ ] The convergence barrier accounts for downstream `ReSharper_SilentCleanupOpenCode` work and treats a fixed short sleep as insufficient.
- [ ] If post-VCmd convergence cannot be established within the finite maximum wait, the script stops before Git staging/commit while preserving source/project progress.
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
- every existing-file target is based on the newest authoritative maintainer-authored source available for that path; earlier AI payloads/tarballs are not used to roll back later maintainer edits;
- the generation-time diff from authoritative source to desired payload contains only transaction-owned changes and necessary direct consequences, with unrelated implementation/logging/comments/XML documentation preserved;
- exact full-file desired-state payloads are used wherever feasible after that preservation audit succeeds;
- source/project payloads are generation-time audited for correctness/style/documentation/reference requirements;
- commit messages are scoped to their intended staged work items and obey repository rules;
- boundary/retry/no-op behavior is coherent;
- preexisting repository dirt is preserved through a repository-compliant checkpoint before the transaction baseline is established;
- `.Constants`/`.Interfaces` dependency exceptions are honored while actual interface inheritance/member-type dependency closure is still satisfied;
- the post-VCmd convergence barrier prevents Git capture until downstream ReSharper/IDE cleanup has remained mechanically quiet for the required interval;
- the complete changed-path set and narrower VCmd-eligible C# set are distinguished correctly; VCmd eligibility includes `AssemblyInfo.cs`, excludes generated/fixed-format/non-C# artifacts, and editor-opening/VCmd cleanup remains best-effort and unable to erase source progress, while the single VCmd invocation is preceded by the exact noninteractive/Git-disabled one-run sidecar so no modal prompt or VCmd-owned Git workflow can occur;
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
12. verify ReSharper is suspended before any mutation, the suspension state is tracked, and early-failure cleanup can resume it;
13. verify new projects/references/source memberships use DTE/project-system operations rather than hand-authored `.sln`/`ProjectReference` topology, and no junction-canonicalized absolute path is persisted;
14. verify the script maintains a transaction-wide eligible-path registry and performs exactly one paced source-file opening pass after all mutations; verify `AssemblyInfo.cs` is included and `Global*.cs`, `*.Designer.cs`, generated/derived C# source, and all non-C# artifacts are excluded;
15. verify `ReSharper_Resume` plus bounded wait/message pumping occurs after that opening pass and before VCmd;
16. verify the single VCmd invocation is immediately preceded by a write to the canonical `.config.json` path with **exactly** schema `2`, `SuppressPrompts: true`, `EnableGitAwareness: false`, and `AutomaticallyCheckInChangesToGitWhenGitAwarenessIsSuppressed: false`;
17. verify the script skips VCmd rather than invoking it when sidecar preparation fails, and verify the VCmd call is argumentless (no `NoPrompt` or other command argument);
18. verify VCmd cannot perform Git synchronization/check-in/push and therefore cannot compete with the script's own custom commit-message/staging workflow;
19. verify eligible-file editor-open failure, sidecar-preparation failure, and VCmd failure are warning-only, excluded files are never opened merely for VCmd, and the final `File.SaveAll` is unconditional;
20. verify any preexisting repository dirt is committed in a repository-compliant preservation checkpoint before transaction-owned mutation and that this checkpoint cannot be removed by transaction rollback/no-op cleanup;
21. verify the post-VCmd convergence barrier explicitly accounts for downstream `ReSharper_SilentCleanupOpenCode`, pumps the message loop, periodically saves, uses a continuous quiet interval rather than one fixed sleep, and prevents Git capture on timeout;
22. verify `.Constants`/`.Interfaces` projects are exempt from blanket `xyLOGIX.Core.Debug`/`xyLOGIX.Core.Extensions*` additions while genuine interface dependency closure is still satisfied;
23. verify no post-VCmd semantic source verification or fatal lint/style/static-analysis/build/compile/test gate exists;
24. verify reference handling is positive-only unless the current prompt explicitly authorizes removal;
25. verify public WinForms `*.Designer.cs` payloads use the required explicit `public partial class` declaration when applicable;
26. verify top-level exception handling reports message/position/stack, performs safe transaction cleanup, preserves meaningful progress, and normally does not rethrow redundantly;
27. verify the script's final success/no-op/error paths all leave the repository and PMC session in a state the maintainer can understand from `Write-Host` output; and
28. verify the artifact's payload manifest corresponds to the generation-time desired files that were produced from the newest authoritative maintainer-authored baselines, not from an older AI-generated snapshot.

Only after both passes succeed should the artifact be delivered.

---

## 30. Final Standard

> A Change Transaction Script is a **clobbering, progress-first, in-IDE transaction that lets Visual Studio own Visual Studio topology and lets Git preserve the maintainer's preexisting work before transaction-owned changes begin**. Use the host-provided `$dte`; never assign or bind to it. Audit source/project shape before delivery against the current authoritative workspace/tarball and current-prompt corrections. Generate complete exact desired-state source payloads whenever feasible, but do not treat `.sln` membership or project-reference relationships as ordinary text payloads when DTE exposes the corresponding operation. Add projects with `$dte.Solution.AddFromFile(...)`, add project references through the consuming loaded project's DTE/VSProject reference collection, and let Visual Studio persist relative topology.
>
> **Clobbering is a runtime mechanism, not a source-precedence rule.** A maintainer-authored edit made after an earlier AI transaction becomes the authoritative baseline for that path. Never use an older tarball, earlier generated payload, previous assistant response, or prior script to silently restore the AI's former version. Before producing an exact full-file payload, start from the newest maintainer-authored file, merge only the currently requested change(s), and generation-time-audit the diff so unrelated maintainer code, logging, comments, XML documentation, formatting-sensitive content, and implementation choices remain intact. If the generator knows a file changed but does not have its current contents, obtain them before generating a full-file replacement.
>
> The current development environment is junction-sensitive: `%USERPROFILE%` is `C:\Users\Brian Hart`, while `%USERPROFILE%\source` resolves through a directory junction to `D:\Users\Brian Hart\source`. Those `C:` and `D:` spellings can identify the same physical repository/project/file. Never infer duplicate repositories from path-string inequality, and never leak a canonicalized physical path back into `.sln`/`.csproj` topology merely because a filesystem or IDE API exposed it.
>
> After the initial `File.SaveAll`, inspect every affected Git work tree. If preexisting tracked/untracked dirt exists, do **not** abort. Intentionally stage that preexisting state, generate a repository-compliant commit message from the actual staged diff, commit it as a pre-transaction preservation checkpoint, and verify that the repository is clean before synchronization and transaction-owned mutation. This preservation checkpoint is maintainer history, not transaction-owned bookkeeping, and must never be erased by later boundary/no-op/failure cleanup. Dirt that appears only after the baseline is established remains unrelated work and must never hitchhike into transaction commits.
>
> Suspend ReSharper with `ReSharper_Suspend`, wait/pump, and keep it suspended through **all** source/project/Solution mutations. Maintain one transaction-wide registry of VCmd-eligible affected C# files. Do not run VCmd or open/close source separately for scaffold and implementation phases. After every mutation is complete, save once, derive the final eligible set, and perform **exactly one** paced source-file-opening pass in the explicit source/text editor. The pacing/message pumping exists so Visual Studio's project system/Asset Synchronization Service can associate files with their real projects instead of classifying them as **Miscellaneous Files**.
>
> Only after the full opening pass is complete should the script invoke `ReSharper_Resume`, wait/pump for synchronization to settle, prepare the exact schema-version-2 noninteractive/Git-disabled VCmd sidecar, and invoke argumentless `VCmd.CCommandStripLineBreaksFromAllComments` once. `AssemblyInfo.cs` remains eligible because VCmd has special rules for it. `Global*.cs`, `*.Designer.cs`, generated/fixed-format C#, and all non-C# artifacts remain excluded.
>
> **VCmd returning is not the Git-capture boundary.** The command can trigger downstream `ReSharper_SilentCleanupOpenCode` work that continues rewriting source after the outer VCmd call returns. Enter a bounded post-VCmd convergence barrier: pump `Application.DoEvents()`, wait in short bounded intervals, periodically invoke `File.SaveAll`, and observe the mechanical state of transaction-owned files. Reset the quiet timer whenever any observed file changes and require a meaningful continuous quiet interval before staging begins. A fixed short sleep alone is insufficient. If the finite maximum wait expires without convergence, stop before Git capture while preserving source/project progress. Do not compare against expected source hashes or judge semantics during this barrier; it exists only to prove the IDE has become quiescent.
>
> `.Constants` and `.Interfaces` projects are exceptions to blanket dependency conventions. Do not add `xyLOGIX.Core.Debug` or `xyLOGIX.Core.Extensions*` merely because such a project exists. Add a dependency only when the actual code contract needs it. In particular, an `.Interfaces` project may legitimately require `xyLOGIX.Core.Extensions` (or another specific extensions project) when its inheritance/member-type closure depends on contracts such as `IForm` or `IControl`. Generation-time auditing must therefore verify the real dependency closure rather than applying either a blanket requirement or a blanket prohibition.
>
> Scaffold-first project creation remains an architectural/history rule, not a reason for multiple cleanup rounds. Create the scaffold first, add the project through DTE, then add implementation/reference state through DTE while ReSharper is suspended. After the single cleanup/convergence pass, use exact Git staging to commit the scaffold/topology work item before implementation commits. Keep Git work trees isolated, stage only transaction-owned paths, use repository commit-message rules, and preserve forward progress.
>
> Finally, repository coding preferences are generation-time acceptance criteria. A transaction should not deliver C# that depends on ReSharper to reveal missing `using` directives, missing genuinely required project references, non-fluent factory APIs, or return-value patterns that contradict the maintainer's established style. The two-pass audit must inspect both the intended C#/topology state and the exact GUID-named `.ps1` artifact actually delivered.
