# Robust Visual Studio Package Manager Console Automation Script Guidelines

Revision: O
Last Updated: 7 September 2026

## Changelog

Revision O is the current controlling revision and supersedes Revision N where this document differs. Revision O replaces the former clean-work-tree/clean-index doctrine with an imposition model: a Change Transaction Script overlays its authorized changes onto whatever Git working-tree and index state already exists, never requires or manufactures repository cleanliness, never auto-commits unrelated preexisting dirt, and never generates an `Assert-CleanIndex`-style gate. It also formalizes transaction-private Git index capture so unrelated staged work can remain untouched while transaction-owned commits are created.

- **Revision N.** Added idempotent/nonfatal ReSharper state handling, fresh generated identity GUIDs, generation-time-known rename work items, bounded `.csproj` readiness before explicit `Solution.AddFromFile(..., false)`, and semantic before/after logging around potentially blocking DTE calls.
- **Revision M.** Added genuinely visible/activated VCmd editor tabs, preexisting-editor-state isolation/restoration, the detailed `%TEMP%` transaction log with terminal raw `git status`, zero-output Git hardening, and authorization-based rename/copy-aware staged-scope validation. It also aligned transaction behavior with the supplied Visual Commander source, including its open-document precedence and one-run sidecar reset.
- **Revision L.** Added the Git-recovery-only transaction mode for already-settled live changes, avoiding source mutation, VCmd/ReSharper work, and artificial content-stability waits while retaining full Git proof and unrelated-dirt isolation.
- **Revision K.** Made cleanup convergence workload-adaptive, added generation-time symbol-to-namespace/dependency closure, and changed Git completion into a Git-proven fixed-point process with observed commit identity and final upstream proof.
- **Revision J.** Strengthened content-based post-VCmd convergence, prohibited nested exception blocks in transaction scripts, and formalized Solution-level governance-document exclusion while retaining unrelated-work isolation.
- **Revision I.** Intentionally skipped; no controlling Revision I document was issued.
- **Revision H.** Consolidated maintainer-source authority, Solution-level governance/documentation ownership, unrelated-work isolation, and the post-VCmd synchronization model into the standing transaction contract.
- **Revision G.** Established that newer maintainer-authored live source supersedes earlier AI payloads and that full-file clobbering is valid only after the desired payload is built from the newest authoritative source.
- **Revision F.** Added a state-based post-VCmd cleanup barrier, automatic preservation of preexisting Git dirt, and demand-driven dependency exceptions for `.Constants` and `.Interfaces` projects.
- **Revision E.** Made Visual Studio/DTE authoritative for Solution/project topology, added junction-aware path handling, ReSharper suspend/resume discipline, and one paced transaction-wide VCmd opening round.
- **Revision D.** Narrowed VCmd eligibility to ordinary hand-authored C# plus the explicit `AssemblyInfo.cs` exception, excluding generated/designer/global source and non-C# artifacts.
- **Revision C.** Consolidated the early clobbering/retry, VCmd sidecar, and staged-diff Git-capture rules into a reusable Change Transaction Script specification.
- **Revision B.** Adapted the original PMC automation rules to the DiagnosticBatchRunner/xyLOGIX workflow, including repository-specific commit-message and Solution-root conventions.
- **Revision A.** Established the original Visual Studio PMC/DTE automation baseline: child-scope hygiene, host-owned `$dte`, `File.SaveAll` synchronization, quiet Git execution, bounded recovery, and GUID-named delivery.

## Purpose

This document is a reusable engineering specification for an AI system that generates **agentic PowerShell Change Transaction Scripts intended to be dot-sourced from the Visual Studio Package Manager Console (PMC)**.

A Change Transaction Script is an **in-IDE, repository-aware maintenance transaction**. It is used after the AI has inspected the current authoritative workspace and determined a concrete desired change or a concrete recovery/check-in need. Depending on transaction mode, the maintainer wants one downloadable `.ps1` artifact either to impose that change inside the already-loaded Visual Studio Solution and prepare/capture the resulting source, or to capture already-existing settled changes in Git without mutating them, and in either case to leave the workspace in a useful state without forcing the maintainer to debug the automation itself.

Typical use cases include:

- clobbering one or more existing source/project files with audited desired-state replacements for a bug fix, behavioral correction, refactor, logging change, documentation change, UI adjustment, or configuration change;
- applying a coordinated multi-file change while keeping ordinary implementation commits file-by-file unless an explicit source-family, rename, scaffold, or topology exception applies;
- updating WinForms source and `*.Designer.cs` files while ensuring that only VCmd-eligible C# source is opened for cleanup, with `*.Designer.cs` remaining a mutation/Git artifact rather than a VCmd processing target;
- adding required project/assembly/package references without policing unrelated existing references;
- creating complete repository-standard project/module scaffolds and adding them to the loaded Solution through DTE;
- performing project/Solution topology operations such as renames when the task genuinely requires them;
- retrying a prior partially completed transaction without treating harmless source divergence, formatting changes, or an orphaned empty transaction boundary as a reason to fail;
- recovering already-completed, already-settled source/project changes from a prior transaction whose Git capture did not occur or did not finish, by staging/committing/pushing the live dirty files without re-running source mutation, ReSharper suspension, VCmd cleanup, or artificial content-stability waits;
- deriving the narrow VCmd-eligible C# processing set from the complete transaction-created changed-path set, supplying the one-run noninteractive/Git-disabled configuration for `VCmd.CCommandStripLineBreaksFromAllComments`, running that cleanup pass without modal prompts or VCmd-owned Git activity, and saving the IDE state before the script's own Git capture; and
- staging, committing, synchronizing, and pushing transaction-owned work without allowing unrelated paths to hitchhike.

These scripts are **not** intended to be general-purpose CI/CD pipelines, substitute compilers, build validators, test harnesses, source analyzers, or autonomous architectural reviewers. They are controlled maintainer-side change vehicles: the AI performs the source/code reasoning before delivery; the script performs the mechanical transaction inside the maintainer's current Visual Studio session.

They are also **not** the delivery mechanism for the Solution-level xyLOGIX Software Engineering Manifesto, Solution-level `CONTRIBUTING.md`, or Solution-level `README.md`. Those three governance/documentation files are maintained through separate downloadable artifacts and are manually applied by the maintainer. This exclusion is role- and location-specific: project/module-level README files and other documentation may still be legitimate transaction targets when the current task explicitly requires them.

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

### Manual Solution-level governance/documentation deliverables

The Solution-level xyLOGIX Software Engineering Manifesto, Solution-level `CONTRIBUTING.md`, and Solution-level `README.md` are **maintainer-applied documents**. Even when Visual Studio lists them under **Solution Items**, exclude them from every transaction-owned mutation/payload/topology/staging/commit map. If the user asks for updated versions alongside a Change Transaction Script, produce the documents separately as downloadable artifacts; the `.ps1` must not install them.

These files remain maintainer-owned regardless of whether they are clean, modified, staged, untracked, or otherwise represented in Git. A Change Transaction Script must not alter, stage, commit, reset, stash, clean, or otherwise absorb them unless the current prompt explicitly makes one of them a manual deliverable outside the script. Their Git state is not a transaction precondition.

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

This rule does **not** eliminate mechanical safety checks that are actually needed to know where the transaction is operating: Solution identity, repository identity, authorized target pathname, Git availability when Git work is requested, filesystem writability, required tool availability, exact transaction-owned commit scope, and genuine topology/ownership ambiguity remain valid concerns. **Working-tree cleanliness and default-index cleanliness are not valid transaction gates.** The distinction is that source contents and unrelated Git dirt are not runtime permission gates for a clobbering write.

The script must assume that real development environments contain directory junctions, partially completed prior runs, open editor documents, older PowerShell binding behavior, COM/DTE quirks, generated files that must not be touched, and legitimate no-op conditions. Every consequential action must therefore have a direct reason to occur and a meaningful postcondition. Runtime preconditions are appropriate only when they determine whether the next mechanical action is possible or safe; current source bytes, hashes, formatting, layout, comments, or semantic equivalence to an earlier snapshot are **not** transaction preconditions.

A Change Transaction Script **does not care whether the repository is dirty or clean**. It does not manufacture a clean baseline, and it does not auto-commit, stash, reset, restore, clean, or otherwise reorganize unrelated preexisting Git state merely to make the transaction easier to reason about. The transaction imposes its authorized desired state on its own pathset and leaves every unrelated working-tree/index path alone. Git status/diff observations exist to identify repository/path ownership, construct transaction-owned commits, prevent unauthorized hitchhiking, and decide whether optional remote synchronization is mechanically non-disruptive; they are not repository-cleanliness gates.

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


### 1.1 Git-recovery-only transaction mode

A **Git-recovery-only Change Transaction Script** is a specialized transaction whose sole purpose is to capture changes that already exist in the maintainer's live work tree because an earlier transaction, manual correction, or interrupted workflow left the intended files uncommitted.

Use this mode only when all of the following are true:

1. The maintainer has explicitly identified the recovery purpose or supplied authoritative Git evidence such as `git status`, `git diff`, or `git diff --staged` showing the intended recovery set.
2. The desired source/project bytes already exist in the live work tree and no additional source, project, Solution, reference, resource, or topology mutation is requested.
3. The recovery script can identify the authorized recovery path set narrowly enough to commit only those paths.
4. No VCmd/ReSharper cleanup pass is requested or needed to produce the desired bytes.

In this mode:

- run `File.SaveAll` once before Git observes the work tree so unsaved editor buffers are flushed;
- do not create a new empty transaction boundary merely to check in already-existing work;
- do not invoke `ReSharper_Suspend`, `ReSharper_Resume`, VCmd, source-editor opening passes, or content-fingerprint convergence waits merely because files are dirty;
- do not require the work tree or default Git index to be clean, and do not auto-commit/stash/reset unrelated dirt;
- do not generate or call `Assert-CleanIndex`, and do not recreate an equivalent cleanliness gate under another name;
- capture the authorized recovery work with the same transaction-private index / path-isolated commit model used by Section 22 so unrelated staged/default-index state remains untouched;
- prove every commit from Git by observing `HEAD` before and after, resolving the actual SHA/subject, and refreshing transaction-owned path status;
- after a capture pass or push, use a **direct fresh Git-status check of the recovery-owned paths**, not an artificial quiet-period/content-fingerprint wait;
- if an authorized recovery path is dirty again on that direct check, recapture it from fresh status in a bounded number of rounds; and
- treat remote synchronization as optional: perform it only when it can be done without stashing, committing, resetting, or otherwise disturbing unrelated local state.

A recovery-only script may escalate to the normal source-mutating convergence model only when there is concrete evidence during execution that Visual Studio/ReSharper is actively rewriting recovery-owned files. Mere dirtiness, staged state, file count, or the fact that the earlier transaction once used VCmd is not such evidence.

This exception exists to avoid wasting time waiting for files that have already been sitting unchanged. It does **not** relax repository/path identity, authorized commit scope, commit-message discipline, Git-proven commit identity, or transaction-owned end-state proof.

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

The top-level transaction `try`/`catch` is the transaction failure boundary. A mechanically unrecoverable error should stop the **transaction**, report actionable diagnostics, perform only the cleanup that is safe for the transaction state, and then normally return control to the existing PMC runspace without rethrowing merely to make the console emit another terminating error.

A fatal transaction error therefore does **not** mean that the script should try to terminate Visual Studio, destroy the PMC session, or convert an already-diagnosed failure into additional host noise. In the top-level `catch`:

1. report the exception message, invocation position, and script stack trace when available;
2. if no meaningful positive mutation occurred, remove the transaction-owned empty boundary when the Section 15 conditions permit it;
3. if meaningful positive mutation occurred, preserve that progress unless an explicit rollback contract applies; and
4. return control to PMC after transaction-owned cleanup.

Rethrow only when the current prompt explicitly requires the exception to escape the transaction boundary or when the host cannot otherwise be left in a coherent state.

### No nested `try`/`catch` blocks

Do not nest a `try`/`catch`/`finally` block inside another `try`/`catch`/`finally` block in the PowerShell source of a Change Transaction Script. If one operation needs its own exception boundary while the caller is already within the top-level transaction exception boundary, extract that operation into a named helper function and let the helper own its own `try`/`catch`/`finally` structure.

The purpose of the extraction is SRP and readability, not merely indentation reduction. The helper should represent one coherent operation and should return or report the information the caller needs. Do not hide a nested exception block inside an anonymous script block or other inline construct merely to evade this rule.

For generated C# payloads, the current xyLOGIX Software Engineering Manifesto governs the analogous source shape: extract a focused helper method when the responsibility is local to one class, or an interface-backed singleton service when the responsibility is reusable across the software system.

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

### Transaction execution-log lifetime

Initialize the transaction's detailed file log at the earliest executable point after the script basename and `%TEMP%` can be resolved, before any source, DTE, Git, VCmd, or repository mutation. Section 24 defines the required filename, event detail, redaction rules, and the special rule that complete raw final `git status` output for every affected repository is the last substantive content written on both success and failure whenever mechanically possible.

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

### 5.5 Normalize zero-output native-command results

A successful native command can legitimately emit no stdout or stderr. This is especially important for clean-state Git commands such as `git status --porcelain`, whose correct successful output is an empty stream.

Every native-process wrapper and parsing helper must therefore normalize missing captured streams to non-null strings before string operations and normalize an empty status stream to an empty collection rather than `$null`. Never call `[string]::Join(...)`, `.Trim()`, `.Split()`, collection constructors, or equivalent operations on a value that can still be `$null`; a successful zero-output repository status must be represented as a normal empty/no-item result, not as a PowerShell exception.

The exact delivered artifact must include a generation-time/runtime audit path for the zero-output case. A helper that works only when Git prints at least one status line is defective.

### 5.6 Avoid PowerShell 5.1 parser traps in the exact artifact

Do not import C#/JSON punctuation habits into Windows PowerShell 5.1 source. In particular, avoid trailing commas in PowerShell array/argument/parameter constructs where the 5.1 parser rejects them, and do not write an expandable string such as `"$Label: ..."` when a colon immediately follows a variable name; use `"${Label}: ..."` or another unambiguous form.

These are exact-artifact requirements, not merely generator-source preferences. The final GUID-named `.ps1` must be reopened and parsed with the Windows PowerShell 5.1 parser when available, and targeted static checks must cover the known trailing-comma, ambiguous-variable-colon, and zero-output-native-result defect classes before delivery.

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

- add a new project with explicit `$dte.Solution.AddFromFile($fullProjectPath, $false)` after the Section 20.2 readiness/logging barrier;
- add a project reference through the consuming loaded project's VSProject/DTE reference collection, normally `References.AddProject(<targetProject>)`;
- add an assembly reference through the loaded project's DTE reference collection;
- add/remove source project items through `ProjectItems`/the project system when membership must change; and
- use DTE/Solution Items operations for Solution-item membership.

Do not hand-edit `.sln` project entries or `<ProjectReference>` XML merely because they are text. The IDE is the authority for those relationships and is responsible for writing the correct relative path representation. Exact-payload clobbering remains the default for authored source/project content, but topology operations are not ordinary text-payload mutations.

Do not infer transaction ownership merely from Solution membership. In particular, the Solution-level SEM, `CONTRIBUTING.md`, and `README.md` remain manual maintainer deliverables even when exposed as **Solution Items**. A Change Transaction Script must not add/remove/update those documents through DTE.

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

## 6.6 ReSharper state handling and IDE settling

The ReSharper rule in this section applies to transactions that perform source/project/Solution mutations. A Git-recovery-only transaction as defined in Section 1.1 performs no such mutation and must not change ReSharper state merely to capture already-existing files.

ReSharper state handling is **idempotent and non-fatal**. `ReSharper_Suspend` being unavailable is not, by itself, a transaction failure: in the maintainer's environment that commonly means ReSharper is already suspended. Likewise, `ReSharper_Resume` being unavailable can simply mean ReSharper is already resumed or that the command is otherwise not currently exposed. A Change Transaction Script must not die merely because one of these state-toggle commands is unavailable.

Before the first source/project/Solution mutation:

1. when practical, inspect availability of both `ReSharper_Suspend` and `ReSharper_Resume` through the current DTE command surface;
2. if `ReSharper_Suspend` is available, log the exact DTE operation before calling it, invoke `$dte.ExecuteCommand('ReSharper_Suspend')`, log the return immediately afterward, and mark that **the transaction itself** changed ReSharper to the suspended state;
3. if `ReSharper_Suspend` is unavailable while `ReSharper_Resume` is available, treat that as strong evidence that ReSharper was already suspended before the transaction, record that preexisting state, emit an informational/warning diagnostic, and continue without failing;
4. if neither state-toggle command can be used, record ReSharper state as unknown/uncontrolled, warn, and continue rather than aborting the source transaction; and
5. after any successful state change, pump `[System.Windows.Forms.Application]::DoEvents()` and use a short bounded settling interval before continuing.

Keep ReSharper effectively suspended through source/project/Solution mutation whenever that state is available. Do not suspend/resume separately for scaffold and implementation phases.

At the final VCmd preparation point:

- if the transaction itself suspended ReSharper from a previously running state, resume it after the complete visible editor-opening pass and record that the transaction restored its original running state;
- if ReSharper was already suspended before the transaction and VCmd/ReSharper cleanup is desired, the transaction may temporarily resume it for the cleanup pass, but must remember that this is a temporary transaction-owned state change and best-effort restore the original suspended state after VCmd/convergence;
- if a required toggle is unavailable, warn and continue with best-effort cleanup rather than treating command availability as a fatal gate; and
- cleanup must never blindly invert a preexisting user state. Only reverse a ReSharper state change that the transaction can positively attribute to itself.

The script should distinguish `transactionChangedReSharperState`, `preexistingReSharperState`, and the current effective state rather than using one Boolean that assumes every failed suspend/resume call is fatal.

## 6.7 Observable DTE call boundaries

Potentially blocking DTE/COM operations must be logged **before** the call is entered and immediately after it returns. A generic helper-entry record is not sufficient when the next statement can block Visual Studio. The pre-call record is the forensic boundary if the IDE becomes unresponsive or is externally terminated before PowerShell can execute `catch`/`finally`.

For consequential operations, log both a concise human-oriented PMC message and a detailed file-log record containing the important identities/arguments. Examples include:

- `Solution.AddFromFile(...)`: full project path, full Solution path, and explicit `Exclusive` value;
- `References.AddProject(...)`: consuming project and target project names/full paths;
- `ProjectItems.AddFromFile(...)`: project name and full source-item path;
- Solution/project removal or close/open operations: exact project/Solution identity;
- `ProjectItem.Open(...)`/`ItemOperations.OpenFile(...)`: full source path and requested view kind;
- `ReSharper_Suspend`/`ReSharper_Resume`: command name and inferred preexisting state;
- `VCmd.CCommandStripLineBreaksFromAllComments`: exact command name and intended visible-document count; and
- other DTE calls known to load projects, mutate topology, switch editor windows, or synchronously invoke large IDE extensions.

Preferred wording is explicit, for example:

```text
*** INFO *** Trying to add project 'C:\path\Foo\Foo.csproj' to the Solution 'C:\path\Bar.sln' through DTE Solution.AddFromFile(..., false)...
```

When the call returns, log the returned DTE object identity/outcome immediately before beginning unrelated work. If Visual Studio is forcibly terminated (for example `devenv.exe` is killed externally), the script cannot execute its normal finalizer or terminal `git status` logger; in that case the last pre-call boundary record is expected to be the final forensic evidence in the log.

Do not move DTE/project-system operations to arbitrary background PowerShell threads merely to manufacture a timeout. DTE is COM/UI-thread-sensitive; keep topology/editor calls on the PMC/DTE execution path and make them observable instead.

## 7. `File.SaveAll`: Required Flush Boundaries

`File.SaveAll` is one of the most important synchronization points between Visual Studio's in-memory state and the filesystem.

Call it through DTE:

```powershell
$dte.ExecuteCommand('File.SaveAll')
```

and pump the IDE message loop at synchronization boundaries when appropriate:

```powershell
[System.Windows.Forms.Application]::DoEvents()
```

### Required checkpoint A: before Git observes the initial disk state

After DTE/Solution validation, run `File.SaveAll` before the first meaningful Git status/synchronization validation. Git sees files on disk rather than unsaved editor buffers, and project-system changes can also still be pending in Visual Studio.

`File.SaveAll` does **not** require checking whether documents are open first.

At this early point, snapshot the user's current editor state for later restoration: record the set of user-visible document pathnames whose `Document.Windows.Count` is greater than zero and, when practical, the originally active document pathname. This snapshot is state-preservation metadata; it is not permission to close anything yet.

### Git-recovery-only checkpoint

For a Section 1.1 Git-recovery-only transaction, checkpoint A is normally the only IDE flush required before ordered Git capture. After that save:

1. verify only the authorized recovery path set and owning repository; do not require a clean work tree or clean default index;
2. proceed directly to Section 22 ordered capture using transaction-private/path-isolated Git capture;
3. after each commit/capture pass/push, use fresh Git status of the recovery-owned paths as the normal end-state observation; and
4. do not manufacture checkpoint B/C, VCmd opening, ReSharper resumption, fingerprint sampling, or quiet-period waits when the script performed no source/IDE mutation.

If direct status unexpectedly shows a recovery-owned path dirty again, recapture from fresh status within the bounded recovery-only round budget. Escalate to content-stability waiting only when there is concrete evidence of active IDE rewriting.

### Required checkpoint B: after all source/project/Solution mutations

After **all** transaction source, source-item membership, project-reference, project-add, Solution-item, and other topology mutations are complete, run `File.SaveAll` so Visual Studio/project-system state is flushed before the one-time editor-cleanup pass begins.

There is no phase-local VCmd checkpoint. Creating a scaffold is **not** followed by a scaffold-only VCmd pass; all VCmd-eligible affected files are accumulated into one transaction-wide registry and processed together at the final cleanup point.

### Required checkpoint C: one final editor/VCmd convergence point before Git capture

For a source-mutating transaction, the final cleanup boundary is:

1. complete every requested source/project/Solution mutation while ReSharper remains suspended;
2. run `File.SaveAll` and pump the IDE;
3. resolve the complete transaction-created changed-path set for Git/scope purposes;
4. union that status with the transaction-wide affected-path registry and derive the final VCmd-eligible C# set using Section 9;
5. using the editor-state snapshot from checkpoint A, temporarily close **only** preexisting user-visible C# documents that are outside the intended VCmd set, after saving them; do not use `Window.CloseAllDocuments`;
6. if that unrelated-C# isolation cannot be established safely, restore anything already closed, warn, skip VCmd, restore/transition ReSharper only according to the idempotent state model in Section 6.6, save, and continue with script-owned Git capture rather than processing unrelated source;
7. open the complete intended VCmd set exactly once using the visible/activated source-tab procedure in Section 9, recording only files that become observably user-visible document windows as successfully opened;
8. perform a final bounded message-pump/settling interval after the last open;
9. transition ReSharper into the cleanup-capable state using Section 6.6: resume when the transaction suspended it, or temporarily resume a preexisting suspended state when cleanup requires that and the command is available; keep unavailability nonfatal and perform a bounded ReSharper/project-system settling interval after any successful transition;
10. rewrite the exact one-run VCmd sidecar **immediately before** the invocation, because the VCmd command loads the configuration once and resets the canonical file to defaults at the end of every invocation;
11. invoke argumentless `VCmd.CCommandStripLineBreaksFromAllComments` once when at least one intended C# document is observably user-visible and sidecar preparation succeeded;
12. enter the mandatory adaptive post-VCmd cleanup-convergence barrier over the exact successfully opened path set; that path set remains authoritative even if VCmd itself closes an `AssemblyInfo.cs` document during cleanup;
13. pump `Application.DoEvents()`, wait in bounded intervals, periodically invoke `File.SaveAll`, and repeatedly fingerprint those paths until the complete set is continuously content-stable for the adaptive quiet interval and remains stable through the final save/pump/resample;
14. restore the preexisting editor state before Git capture: reopen every preexisting user-visible document that the transaction temporarily closed or that VCmd closed, including a preexisting `AssemblyInfo.cs` tab when applicable, and best-effort reactivate the document that was active before the cleanup pass; if ReSharper was preexisting-suspended and temporarily resumed for cleanup, best-effort re-suspend it now;
15. run one final `File.SaveAll`, pump the IDE, refresh Git status/dirty scope, and only then begin ordered Git capture.

The transaction-opened VCmd tabs may remain open after cleanup; restoring the user's prior active document does not require closing those newly opened tabs. Restoration is about preserving the user's preexisting editor state, not erasing the transaction's visible tab-opening behavior.

Do **not** semantically verify, reparse, regex-check, marker-check, or compare source against expected payloads after VCmd cleanup. Content fingerprints in the convergence barrier are temporal change detectors only and must never be compared with generation-time payload hashes.

If VCmd is skipped before invocation because isolation, sidecar preparation, or visible opening cannot be established, there is no VCmd-specific processing set to observe. Restore/transition ReSharper only according to Section 6.6, perform the normal unconditional save/short IDE settling, restore user editor state, and continue; VCmd remains best-effort cleanup and must not erase forward source/project progress.

## 8. Closing and Restoring Visual Studio Documents Safely

Never execute `Window.CloseAllDocuments` as part of the VCmd workflow. VCmd's supplied implementation gives user-visible open C# documents precedence when choosing its processing scope, so the transaction must isolate that scope narrowly without destroying unrelated editor state.

### Snapshot user-visible editor state

Near the start of a source-mutating transaction, after the initial `File.SaveAll`, enumerate `DTE.Documents` directly and record every document that owns one or more `Document.Windows`. Record stable pathnames where available and the active document pathname when practical. Do not coerce the COM collection through fragile generic-list/array patterns.

### Targeted temporary close for VCmd isolation

Immediately before the one-time VCmd opening pass, after saving:

1. identify preexisting user-visible **C#** documents that are not members of the intended VCmd path set;
2. close only those specific unrelated C# documents, preserving a restoration list;
3. leave unrelated non-C# tabs alone, because VCmd's open-document source resolver filters to open C# source;
4. never close an intended transaction VCmd document merely because it was already open before the transaction; reuse it and preserve the fact that it was preexisting; and
5. if any unrelated preexisting C# document cannot be closed/isolate safely, warn and skip VCmd instead of knowingly broadening VCmd's source scope.

This targeted close is a narrow scope-isolation operation, not a general editor-cleanup policy.

### Restoration after VCmd and on failure

After VCmd convergence and before Git capture, restore every preexisting user-visible document that is no longer open because the transaction temporarily closed it or because VCmd's `AssemblyInfo.cs` cleanup closed it. Best-effort restore the previously active document after reopening the preexisting set.

Failure cleanup must perform the same restoration before the transaction log writes its final raw Git-status block. A restoration failure is diagnostic information, but it does not authorize a blanket close/open cycle or rollback of successful source/project work.

## 9. One-Time VCmd Source Opening and Visual Commander Cleanup

### Git-recovery-only exception

A Section 1.1 Git-recovery-only transaction does **not** run VCmd, does not open source files for VCmd, does not suspend/resume ReSharper, and does not enter the adaptive post-VCmd convergence barrier. The desired source bytes already exist and the transaction's job is Git capture only.

### Supplied VCmd source is the behavioral authority

The supplied `VCmd.CCommandStripLineBreaksFromAllComments` implementation is a Visual Commander-hosted C# 4.0/.NET Framework 4.0 class and intentionally has no namespace. Transaction scripts must model the command that actually exists rather than assuming conventional DTE/editor behavior.

The important observed contracts are:

- VCmd considers a document user-visible only when the `Document` owns one or more `Document.Windows` entries; mere presence in `DTE.Documents` is not enough.
- Its scope resolver checks user-visible open C# documents first. If any are found, they become the processing scope before selected-project or Solution-wide discovery is considered.
- The command loads the JSON sidecar once per invocation and unconditionally resets the canonical file to constructor defaults in its final cleanup. Therefore a transaction must rewrite the automation sidecar immediately before **every** VCmd invocation and must never assume those values persist afterward.
- `AssemblyInfo.cs` receives cleanup-only handling: VCmd opens it as code, invokes `ReSharper.ReSharper_SilentCleanupCode` when available, pumps messages, then saves/closes the active document.
- For remaining open documents, VCmd runs `CodeMaid.CleanupOpenCode`, then `ReSharper.ReSharper_SilentCleanupOpenFiles` when available, followed by `File.SaveAll`.
- VCmd itself pumps Windows messages during synchronization/cleanup. The transaction still needs its own post-command convergence barrier because editor/ReSharper/project-system work can continue changing files after the command call returns.

These contracts explain both the visible-tab requirement and the unrelated-open-C# isolation rule.

### Maintain a transaction-wide affected-file registry

As mutations are issued, record every authorized path that could be VCmd-eligible. The final VCmd set is derived from the union of the transaction-wide affected-path registry and the complete current Git changed-path set used for scope accounting.

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

### Exactly one visible source-tab opening round per transaction

Do not run VCmd during scaffold creation. Do not perform a second implementation opening pass.

After **all** mutations are complete, `File.SaveAll` has run, and unrelated preexisting C# documents have been isolated:

1. derive the final eligible set in deterministic order;
2. for each file, verify it exists and remains an authorized/registered path;
3. resolve a real loaded `ProjectItem` when possible and call `ProjectItem.Open(...)` with the explicit text/source view kind `{7651a700-06e5-11d1-8ebd-00a0c90f26ea}`;
4. capture the returned `EnvDTE.Window`; do not discard it;
5. set the returned window's `Visible` property to `true` when necessary and call `Activate()`;
6. pump `[System.Windows.Forms.Application]::DoEvents()`, wait a short bounded per-file interval (normally about 150-250 ms), then pump again;
7. verify observable editor state before counting/reporting success: the window/document must correspond to the intended pathname and the document must own at least one window (`Document.Windows.Count > 0`); when accessible, also verify the window is visible/active as expected;
8. only after that observation emit the concise PMC diagnostic that the file was opened and activated, and add its pathname to the exact successfully opened VCmd set;
9. use `$dte.ItemOperations.OpenFile(...)` only when the project-item path genuinely cannot be used, and apply the same capture/visibility/activation/message-pump/verification requirements to the fallback window; and
10. after the final successful/attempted open, perform one additional bounded settling/message-pump interval before ReSharper is resumed.

`ProjectItem.Open(...)` returning normally is not enough. The entire purpose of this pass is to create **genuinely user-visible editor tabs** so VCmd's `IsUserVisibleDocument` predicate sees them and so the maintainer can watch the transaction's opening sequence.

An individual eligible-file open failure remains warning-only; continue opening the rest. If zero intended eligible files become observably user-visible, skip VCmd. If exact VCmd scope isolation cannot be proven because an unrelated preexisting C# document remains visible, skip VCmd rather than process unrelated source.

### Transition ReSharper for cleanup, then rewrite the sidecar and invoke VCmd once

After the opening pass:

1. use the Section 6.6 state model: resume ReSharper when the transaction suspended it; if ReSharper was already suspended before the transaction, temporarily resume it for VCmd cleanup only when that command is available and remember to restore the original suspended state afterward; if the required toggle is unavailable, warn and continue rather than failing;
2. after any successful state transition, pump/wait for synchronization to settle;
3. immediately before invocation, write `%LOCALAPPDATA%\xyLOGIX, LLC\Visual Commander\Commands\Strip Line Breaks from All Comments\Config\.config.json` using exactly:

```json
{
  "SchemaVersion": 2,
  "SuppressPrompts": true,
  "EnableGitAwareness": false,
  "AutomaticallyCheckInChangesToGitWhenGitAwarenessIsSuppressed": false
}
```

4. invoke `$dte.ExecuteCommand('VCmd.CCommandStripLineBreaksFromAllComments')` **once and without arguments**;
5. remember that VCmd resets the sidecar to defaults before control returns from its invocation cleanup, so any later invocation must rewrite the sidecar again;
6. if sidecar preparation fails, warn and skip VCmd;
7. if VCmd is unavailable or throws, warn and preserve forward progress; and
8. run `File.SaveAll` unconditionally.

Because the sidecar disables both pre-formatting Git awareness and automatic post-processing check-in, the Change Transaction Script remains the sole Git owner. Open-document invocation policy also suppresses VCmd's pre-formatting Git-awareness path, but the sidecar remains mandatory defense-in-depth against VCmd-owned Git behavior.

### Mandatory adaptive post-VCmd cleanup-convergence barrier

The return of `$dte.ExecuteCommand('VCmd.CCommandStripLineBreaksFromAllComments')` is **not** proof that every CodeMaid/ReSharper/project-system write has finished. VCmd itself invokes `ReSharper.ReSharper_SilentCleanupCode` for `AssemblyInfo.cs` and `ReSharper.ReSharper_SilentCleanupOpenFiles` for remaining open documents, and downstream IDE work can continue rewriting one or more paths after the VCmd command call returns.

Retain the exact pathnames that became observably user-visible in the opening round and use those paths as the cleanup-observation set even when VCmd closes an `AssemblyInfo.cs` document during its own processing.

### Adaptive timing rule

Let `N` be the count of paths in the exact successfully opened VCmd-processing set. For the default policy, compute these bounded values:

```powershell
$observedFileCount = [Math]::Max(1, $openedVcmdPaths.Count)
$scale = [Math]::Sqrt([double]$observedFileCount)
$quietSeconds = [int][Math]::Min(
    30,
    [Math]::Ceiling(6 + (1.5 * $scale))
)
$maximumSeconds = [int][Math]::Min(
    600,
    [Math]::Ceiling(90 + (20 * $scale))
)
```

This formula is the normal default and intentionally grows sublinearly. Emit the observed-file count and selected timing policy before waiting. Use a short bounded sampling interval (normally about 250-500 ms) and a bounded periodic `File.SaveAll` cadence (normally every 2-5 seconds), not a tight spin loop.

### Content-quiescence algorithm

1. Immediately after VCmd returns or after an invocation that may have begun cleanup, call `File.SaveAll`.
2. Materialize the exact successfully opened pathname set and compute an initial per-file mechanical fingerprint.
3. A fingerprint must include current existence state and a content digest of the actual file bytes, preferably SHA-256. Length/last-write metadata may be supplemental but is not sufficient by itself.
4. Compute/report the adaptive `quietSeconds` and `maximumSeconds` from the actual observed-file count.
5. Pump `Application.DoEvents()`, sleep for the bounded sampling interval, periodically call `File.SaveAll`, and recompute the fingerprint of every observed path.
6. Treat an unreadable/disappeared/reappeared path or changed digest as renewed activity and reset the **entire** quiet interval.
7. Require the whole observed set to remain unchanged together for the continuous quiet interval.
8. After that quiet interval, call `File.SaveAll` one final time, pump, wait at least one normal sampling interval, and resample the same path set. If it differs, reset the quiet interval and continue within the same finite maximum duration.
9. Only after the final post-save sample matches may the script restore preexisting editor state, refresh Git status, and begin ordered staging/committing.

A single unconditional sleep, a timestamp-only observation, the VCmd return itself, or a document-window count is not convergence. Content hashing here is a **temporal change detector only** and must never be compared to generation-time expected hashes or used to judge source semantics.

If VCmd was skipped before invocation, do not manufacture a VCmd-specific convergence wait. If VCmd was actually invoked and may have started cleanup before an exception was reported, use the same bounded observation set before Git capture when mechanically possible.

If the finite maximum settling duration expires without achieving the required quiet interval, report the condition as an actionable synchronization error/warning, do **not** begin Git staging/commit capture, preserve all source/project progress, restore preexisting editor state, and return control to the maintainer.

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

The transaction's fatal runtime gates are limited to conditions required to continue mechanically and safely: authoritative Solution/repository/path identity, required Git availability for a Git-integrated transaction, ability to issue the authorized mutation, successful post-VCmd cleanup convergence before transaction-owned capture, and exact transaction-owned commit-scope/commit-identity postconditions. A dirty work tree or dirty default index is never, by itself, a fatal condition. Changed-file source-editor opening, preparation of the noninteractive/Git-disabled VCmd sidecar, and VCmd cleanup are **best-effort cleanup operations**: failures are diagnosed as warnings and do not invalidate successful source/project mutations. A sidecar-preparation failure means **skip VCmd**, not invoke it interactively. Source-byte/hash/layout/semantic equivalence and lint/style/static-analysis results are not runtime gates.

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
- captured stdout; and
- captured stderr.

Normalize stdout and stderr to `''` when the asynchronous read produced no text. A successful zero-output command is normal; in particular, a clean `git status --porcelain` must parse as an empty status collection rather than `$null`. Downstream helpers must not call `[string]::Join`, `.Trim()`, `.Split()`, or collection coercion on nullable native-output values.

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

## 13. Dirty Work Trees and Indexes Are Normal; Impose Transaction-Owned Changes

A Change Transaction Script is an **imposition transaction**. It overlays its generation-time-authorized changes on top of whatever repository working-tree and default-index state already exists.

The script must therefore **not require or manufacture repository cleanliness**.

After the initial `File.SaveAll`, Git status may be observed for diagnostics, repository/path ownership, transaction-owned work-item selection, and optional synchronization feasibility. That observation does not create a baseline-cleanliness contract.

Standing rules:

1. Do **not** abort merely because the working tree is dirty.
2. Do **not** abort merely because the default Git index contains staged changes.
3. Do **not** auto-commit unrelated preexisting tracked, untracked, staged, deleted, renamed, or copied work merely to obtain a clean baseline.
4. Do **not** stash, reset, restore, checkout, clean, or otherwise rewrite unrelated paths to make the transaction easier to run.
5. Do **not** generate `Assert-CleanIndex`, call it, or recreate an equivalent helper/gate under another name.
6. Do **not** use whole-repository cleanliness as a precondition before mutation, before an implementation commit, before a boundary commit, or before declaring local transaction success.
7. The transaction owns only its generation-time-authorized pathsets. On those paths, the transaction's audited desired state intentionally wins at runtime. Everything else is left alone.
8. An unrelated dirty or staged path is not a transaction error. It matters only if a Git operation would accidentally include or disturb it.

### 13.1 Default Git-index isolation model

Because the maintainer's ordinary Git index may already contain unrelated staged work, transaction-owned commits should **prefer a transaction-private alternate index** rather than requiring the real index to be empty.

For each affected repository/work-item capture:

1. Resolve the repository's actual Git index location and create a unique temporary alternate-index pathname, preferably alongside the real index when practical.
2. Set `GIT_INDEX_FILE` only for the Git process(es) that belong to the transaction-private capture operation; do not mutate the PMC process environment globally when a per-process environment can be supplied.
3. Seed that private index from the current `HEAD` with `git read-tree HEAD`.
4. Stage only the current generation-time-authorized work-item pathspecs into that private index, for example with `git add -A -- <authorized paths>`.
5. Inspect `git diff --cached --name-status -M -z` (or equivalent) **through the same alternate index** and require every staged logical path to belong to the authorized work item. Git rename/copy presentation is descriptive only.
6. Generate the commit message from that actual private-index staged diff and commit through the same alternate index.
7. Prove the commit by observing the `HEAD` transition and actual SHA/subject.
8. Dispose/delete the transaction-private index in `finally`.
9. When the maintainer's real index already contains entries for a transaction-owned path, the transaction may normalize **only those authorized path entries** to the new `HEAD` (for example with a path-scoped index reset that does not alter working-tree bytes) so the transaction does not leave its own paths represented by stale pre-commit index entries. Never normalize unrelated paths.

This model allows the user's unrelated staged work to remain staged while the transaction creates its own commits. The private index is a commit-isolation mechanism, not a cleanliness test.

If a transaction-private staged diff somehow contains an unauthorized path, discard/recreate only the transaction-private capture state and recompute the work item. Do not blame, reset, or commit the maintainer's unrelated default-index state.

If an alternate index is mechanically unavailable in a future environment, use another Git mechanism that commits only the explicitly authorized pathset without absorbing unrelated staged entries. The fallback still must not require a clean default index.

### 13.2 Working-tree dirt is observational, not prohibitive

The working tree may contain unrelated modifications before, during, and after the transaction. Leave them untouched. The transaction may inspect whole-repository status to understand context, but its correctness checks focus on:

- whether each authorized target belongs to the intended repository;
- whether transaction-owned desired bytes/topology can be imposed;
- whether the transaction-private staged diff contains only authorized logical paths;
- whether each transaction-created commit actually advanced `HEAD`; and
- whether transaction-owned paths have reached the required stable/committed state.

The repository as a whole does **not** need to be clean when the transaction starts or ends.

## 14. Initial Git Synchronization Is Opportunistic, Not a Mutation Gate

Initial remote synchronization is useful only when it can be performed without disturbing unrelated local state. It is not a prerequisite for imposing the transaction.

For a source-mutating transaction with a configured upstream:

1. Inspect branch/upstream state for diagnostics.
2. If `git pull --rebase` can run without requiring the script to commit, stash, reset, clean, or otherwise alter unrelated working-tree/index state, the transaction may run it before source mutation.
3. If existing dirty/staged/untracked state makes pull/rebase unsafe or Git would reject it, emit a concise informational/warning diagnostic and **skip initial pull/rebase**. Continue with the local transaction.
4. Do not use `--autostash` as a workaround and do not manufacture a clean baseline.
5. A read-only/fetch-only operation may be used when it materially helps establish upstream relation without touching the working tree/index.
6. After any successful synchronization that can change `HEAD`, re-resolve authorized target paths/topology from the current repository state before clobbering; do not compare live source text to the older snapshot as a runtime permission gate.

A Git-recovery-only transaction normally skips initial pull/rebase and captures its authorized existing work first.

The key rule is:

> Local Git dirt may influence whether optional synchronization is attempted; it never determines whether the transaction is allowed to impose its authorized source/project changes.

## 15. Empty History-Boundary Commit and Rollback Anchor

### Git-recovery-only exception

Do not create a new empty history-boundary commit for a Git-recovery-only transaction. The source changes already exist, so a new pre-change marker would misrepresent chronology and add bookkeeping without protecting an upcoming mutation.

### Source-mutating boundary creation without a clean-index requirement

When a source-mutating workflow requires a pre-change history boundary, create a **truly empty** commit before the actual source changes without depending on the maintainer's real index being empty.

1. Capture the current `HEAD` as the pre-boundary rollback anchor.
2. Check whether a matching recognized transaction boundary already exists at `HEAD`.
3. If creating a new boundary, use the transaction-private alternate-index model from Section 13.1, seed it from `HEAD`, stage nothing, and run `git commit --allow-empty -F <tempfile>` through that private index.
4. Verify the resulting boundary commit changed no paths and record its actual SHA/subject.
5. Do not stage, commit, reset, or otherwise absorb unrelated default-index/work-tree state in order to create the boundary.

The boundary commit is a local transaction marker until the entire transaction succeeds. Do not push the empty boundary independently.

### Recognized orphan-boundary cleanup before a new transaction

A previous script iteration may have left a known empty boundary at `HEAD`. A later transaction may remove such a **recognized orphan** when all of the following are true:

1. the current `HEAD` subject exactly matches a boundary subject the generator explicitly recognizes as belonging to an earlier failed iteration of the same work;
2. `HEAD` is verified to be an empty commit; and
3. its parent commit can be resolved safely.

Remove only that empty history marker with a mechanism that preserves the current work tree and default index, such as a verified `git reset --soft <parent>` or equivalent expected-old-ref update. **Do not require the work tree or index to be clean.** Never remove a non-empty commit merely because its subject looks familiar.

### Successful no-op boundary cleanup

If the source-mutating transaction-owned boundary exists and zero implementation commits were created because the desired state was already effectively captured/no-op, remove the bookkeeping-only boundary with the same soft/ref-only mechanism that preserves whatever unrelated working-tree/index state exists. Report the no-op as success.

### Failure handling rule

The default policy is **preserve meaningful forward progress**, not broad rollback.

#### Failure before any meaningful positive mutation

If the transaction must terminate after creating/adopting its empty boundary but before any meaningful positive source/project mutation succeeded, remove only that transaction-owned empty boundary back to its known parent/anchor using a work-tree/index-preserving operation. Do not require cleanliness and do not touch unrelated local state.

#### Failure after meaningful positive mutation

Once a meaningful positive mutation has completed successfully:

1. Preserve source/project changes already made.
2. Preserve transaction-created commits already made.
3. Emit `*** WARNING ***` for advisory/nonessential concerns and continue whenever mechanically possible.
4. Do not automatically `git reset --hard`, restore prior source bytes, delete implementation commits, or erase meaningful forward progress merely because generated state may need a follow-up correction.
5. Let the maintainer decide what should be corrected in the next Change Transaction Script.

A broad rollback/hard-reset workflow after meaningful mutation should be generated only when the current prompt specifically requests it or when a narrowly defined topology operation has its own explicit rollback contract.

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

### Solution-level governance-document exclusion

Under the standing maintainer workflow, the Solution-level xyLOGIX Software Engineering Manifesto, Solution-level `CONTRIBUTING.md`, and Solution-level `README.md` are never authorized Change Transaction mutation targets. Do not embed them as exact payloads, overwrite them, add/remove them through DTE, or create transaction-owned Git work items for them. If updated versions are requested, deliver them separately for manual application. Their presence under **Solution Items** does not alter this rule.

This exclusion applies to the Solution-level governance documents specifically. A project/module-level README or other documentation file may still be an authorized transaction target when the current task requires it.

Before writing any transaction-owned file:

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

- arbitrary preexisting working-tree/default-index dirt;
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

- **already done** ??? overwrite/no-op safely or skip when convenient;
- **not done** ??? perform it;
- **partially done or source-divergent** ??? overwrite authorized targets with the intended desired state;
- **unexpected repository/topology/path state that makes the next mechanical action unsafe or impossible** ??? stop with an actionable error.

Do not blindly replay destructive actions, but do not treat source-content divergence as a reason to stop.

Examples:

- A transaction-owned empty boundary left by a prior failed attempt should normally have been cleaned up if no meaningful mutation occurred; if one is nevertheless found and recognized at `HEAD`, remove/reuse it only under the exact empty-commit/ownership rules in Section 15, without requiring a clean work tree or index.
- Reuse an existing recognizable empty boundary commit rather than creating another when practical.
- If a rerun produces no implementation diff and no implementation commit, remove the transaction-owned no-op boundary so the repository returns to the pre-boundary `HEAD`.
- Overwrite/clobber an authorized source payload regardless of whether a prior run reformatted, partially changed, or otherwise reshaped it; do not rediscover old method bodies or markers first.
- Skip a work-item commit if its path is no longer dirty.
- Add a project/assembly reference only if the current implementation needs it.
- Re-derive the current transaction's VCmd-eligible changed C# set and open those eligible files in the source-code editor before VCmd even when some are already open from an earlier attempt; opening an already-open eligible document is a harmless retry condition.
- Skip VCmd when no VCmd-eligible changed C# files can be opened.
- Preserve partial forward progress after advisory/runtime correctness concerns and fix those concerns in the next transaction rather than automatically erasing the work.
- For a Git-recovery-only retry, skip any recovery path that is already absent from recovery-owned status/committed, commit only the recovery-owned paths still present in status, do not recreate source payloads, and do not add content-stability waits merely because the earlier recovery attempt was interrupted.

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

Likewise, an `.Interfaces` project may require `xyLOGIX.Core.Debug` if its own source genuinely consumes that assembly. The rule is therefore not ???interfaces may never reference these projects???; it is ???do not add them as boilerplate.???

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

A new project must look like a real peer of the existing projects **before** functional implementation is added. Do not manufacture a minimal `.csproj`-only skeleton. Inspect one or more analogous existing projects in the current authoritative repository and copy/adapt their scaffold conventions, but never copy identity GUID values from the analogous project.

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

The executable project may legitimately differ; infer the scaffold from the closest analogous project role rather than blindly copying a universal list. Preserve/adapt assembly metadata, signing settings, documentation settings, package declarations, resource metadata, and project icon/application metadata according to repository patterns.

#### Fresh generated GUID identities are mandatory

Every GUID value that represents a **new transaction-generated identity** must be freshly generated for the current attempt. Never reuse a GUID from an analogous project, prior tarball, prior failed transaction, earlier generated script, scaffold template, or previous assistant artifact. At minimum, every newly created project receives:

- a fresh `<ProjectGuid>` in its `.csproj`; and
- a separate fresh COM/assembly GUID in `Properties\AssemblyInfo.cs` when that metadata exists.

The same rule applies to any other transaction-generated identity GUID. A rerun after rollback generates new identities again. This rule does **not** apply to externally defined fixed API/type/view GUID constants (for example Visual Studio's source/text view-kind GUID), which must retain their documented values.

**Creating a brand-new project is an explicit exception to the ordinary instruction not to regenerate `GlobalAspects.cs` or `AssemblyInfo.cs`.** Those files are part of the scaffold and must be copied/adapted from the repository's standard analogous project, with all identity GUID placeholders replaced by fresh values.

#### Project-file readiness before `Solution.AddFromFile`

`File.WriteAllBytes(...)`/equivalent writes are synchronous, but a newly materialized project tree can still benefit from a short bounded filesystem/IDE settling barrier before Visual Studio's project system is asked to load it. After the complete scaffold and `.csproj` payload have been written, and before crossing into `Solution.AddFromFile(...)`:

1. resolve the **actual full filesystem path** to the newly written `.csproj` using the Solution/project-local path spelling intended for DTE;
2. emit/log the intended project path and loaded Solution full path;
3. require `File.Exists(projectPath)` and an expected nonzero byte length; when the exact generated payload length/hash is already known, use it as a mechanical write-readiness check for the bytes the transaction itself just wrote, not as a semantic/source-shape gate;
4. open/read the `.csproj` successfully, close the read handle, and sample its length/last-write state across a small bounded number of observations;
5. pump `Application.DoEvents()` and sleep briefly between observations so filesystem notifications/project-system services can settle; and
6. stop only when the generated file cannot become readable/stable within a finite readiness timeout.

Immediately before the DTE call, emit/log wording that identifies both sides of the operation, for example:

```text
*** INFO *** Trying to add project 'C:\...\DBR.Config.Validators.Events\DBR.Config.Validators.Events.csproj' to the Solution 'C:\...\DiagnosticBatchRunner.sln' through DTE Solution.AddFromFile(..., false)...
```

Then call the explicit two-argument form:

```powershell
$project = $dte.Solution.AddFromFile($fullProjectPath, $false)
```

Use `Exclusive = $false` explicitly rather than relying on optional/default COM metadata. Capture the returned `EnvDTE.Project`, log immediately that `AddFromFile` returned, record the returned project name/full path when available, pump/settle, then continue. If `AddFromFile` hangs, the pre-call log entry must make that fact unambiguous.

Project creation workflow:

1. Determine the correct owning Solution/repository.
2. Choose the closest analogous existing project(s) by role (`Constants`, `Interfaces`, root/concrete, `Factories`, `Playbooks`, `Algorithms`, etc.).
3. Generate the complete scaffold first, without functional implementation source, replacing all copied identity GUIDs with fresh current-attempt values.
4. Write the complete scaffold and run the bounded project-file readiness barrier on the actual full `.csproj` path.
5. Add the project to the loaded Solution with `$dte.Solution.AddFromFile($fullProjectPath, $false)`; never synthesize the `.sln` project entry.
6. Let Visual Studio persist the normal relative Solution path; never replace it with a canonicalized `C:`/`D:` absolute path.
7. Capture/log the returned `EnvDTE.Project`, pump/settle, and run `File.SaveAll`.
8. Add implementation source/items and required references through the loaded project system/DTE, preserving scaffold-first mutation order and using observable before/after DTE call logging.
9. Defer VCmd/editor cleanup until the one final transaction-wide pass described in Section 9.
10. During ordered Git capture, stage/commit the scaffold/Solution-membership work item before implementation work items so history still presents a complete scaffold first.

Do not terminate merely because a secondary scaffold verifier, regex, metadata comparison, or reread disagrees with the generated scaffold. The readiness checks above are narrowly mechanical checks on a file that the transaction itself just created.

Retry states for a **new-project target directory whose ownership/topology is itself being established by the transaction**:

- directory absent: create it;
- directory exists but is empty: treat as a recognized harmless partial state and populate it;
- directory contains only the known scaffold subset created by a prior failed transaction: complete/clobber the transaction-owned scaffold files as needed, but generate fresh identity GUIDs for a newly regenerated project attempt unless the current authoritative live state is explicitly being recovered rather than recreated;
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
11. Rediscover projects from DTE; use DTE to remove stale/missing project membership, add the renamed project with explicit `$dte.Solution.AddFromFile($fullProjectPath, $false)` using semantic before/after call-boundary logging, and restore required project references through `References.AddProject(...)`.
12. `File.SaveAll` again and verify the loaded graph.

Rollback in reverse order on failure when feasible and surface rollback failures explicitly.

### 20.4 Generation-time-known renames are predeclared work items

For a deterministic generated source-mutating transaction, the generator already knows the authorized old/new pathname relationship before runtime. Do **not** temporarily stage repository changes merely to ask Git whether the operation is a rename. Predeclare the old/new pair as one atomic work item, include only directly coupled topology paths when required, and stage those authorized pathspecs directly (for example `git add -A -- old/path new/path`).

After staging, inspect the actual rename/copy-aware staged diff to prove scope isolation and to generate the commit message. Git may display the result as `R`/`C` or as separate `D` + `A` records according to its similarity heuristics; either representation is acceptable when all logical paths are preauthorized and no unrelated path hitchhikes. Git's rename presentation is descriptive, not a transaction correctness gate.

Temporary rename-detection staging is reserved, if ever necessary, for genuinely generic/unknown dirty-state recovery where the generator did not already know the move relationship. It is not part of the normal deterministic source-mutating Change Transaction workflow.

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
- every source file using `[Log]`, `[NotLogged]`, or another diagnostic aspect/type from `PostSharp.Patterns.Diagnostics` includes `using PostSharp.Patterns.Diagnostics;` unless each such symbol is intentionally fully qualified;
- every changed C# payload is audited for **symbol-to-namespace closure**: for each introduced or retained unqualified framework/library/repository symbol, verify that the source has the needed `using` directive (or an intentional fully qualified reference) and that the containing project has the positive assembly/project dependency required to resolve that namespace/type;
- every source file using `[DebuggerStepThrough]` has the required `System.Diagnostics` namespace available through an appropriate `using` directive or an already-established equivalent;
- every DiagnosticBatchRunner/`DBR.*` project that actually requires `xyLOGIX.Core.Debug` has that project reference, added through the loaded project's DTE/VSProject reference collection rather than by writing `<ProjectReference>` XML; projects ending in `.Constants` or `.Interfaces` are not given `xyLOGIX.Core.Debug` merely by convention;
- `.Constants` and `.Interfaces` projects are not given `xyLOGIX.Core.Extensions*` references merely by convention; an `.Interfaces` project receives the specific extensions dependency when its inherited/member type closure requires it, including contracts such as `IForm`/`IControl` when applicable;
- required `using` directives and required positive project dependencies are audited together so a generated source call cannot be delivered in a state that ReSharper will immediately report as an unresolved symbol;
- fields precede the properties they back, property accessors follow the repository's `[DebuggerStepThrough]`/statement-body conventions, and generated source does not introduce prohibited direct-return, expression-bodied, region, local-function, or other shapes identified by the current repository instructions; and
- the exact payload set is scanned for the same class of violation across **all** files introduced or substantively modified by the transaction, rather than correcting only the first file or first ReSharper error that exposed the pattern; and
- every payload for an existing file is diffed against the newest authoritative maintainer-authored version of that file, with all non-transaction-owned maintainer changes preserved and no older AI-generated version allowed to overwrite them.

ReSharper's Errors/Warnings in Solution export is valuable generation-time evidence when the maintainer supplies it. Treat the latest export as an acceptance input and repair the reported compiler-resolution problems in the generated desired state, but do not limit the audit to those reported locations: inspect sibling/generated transaction source for the same underlying defect class before delivery.

A missing `using` directive in generated source is therefore a **generation-time transaction defect**, not something the maintainer should have to discover at the end of a successful source-mutation run. The audit should be prodigious rather than minimalist with legitimate namespace imports: prefer an explicit, repository-consistent `using` when code intentionally consumes a namespace over relying on accidental transitive context or waiting for ReSharper to infer the omission.

## 22. Ordered Git Commit Phase

Change Transaction Scripts must reproduce the supplied Visual Commander/CreateStagedGitDiff work-item behavior for commit selection and ordering. The mandatory VCmd sidecar sets `EnableGitAwareness` and `AutomaticallyCheckInChangesToGitWhenGitAwarenessIsSuppressed` to `false`, so VCmd performs formatting/cleanup only; it must not pull, stage, generate commit messages, commit, or push on behalf of the transaction. The Change Transaction Script remains the sole owner of Git synchronization and capture.

The default for existing-source implementation work is **file-by-file granularity**, subject only to explicit selector/source-family/rename/topology exceptions. Architectural conceptual grouping by itself is not a reason to batch files.

The Solution-level SEM, Solution-level `CONTRIBUTING.md`, and Solution-level `README.md` are never transaction-owned work items. Do not select, stage, commit, reset, or otherwise absorb them during the ordered implementation phase merely because they are repository files or Solution Items. Their preexisting dirty/staged state is simply unrelated maintainer state unless the current prompt explicitly requests a separate manual deliverable.

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

### 22.4 Deterministic rename/move selection

For a generated source-mutating transaction, rename/move intent is generation-time knowledge. The work-item manifest must explicitly carry the authorized old and new pathnames before execution. Runtime Git staging is not a discovery mechanism for facts already established by the generator.

Stage the known old/new path pair directly as one logical work item, together with only any directly coupled topology paths that must be atomic. Then inspect a rename/copy-aware machine-readable staged view such as `git diff --cached --name-status -M -z` to understand Git's presentation and to ensure every staged logical path is authorized. Accept `R`/`C` or `D` + `A` representations without changing the work-item identity.

Do not perform temporary speculative staging/reset cycles merely to induce Git rename detection in a deterministic source-mutating transaction. Generic Git-recovery-only tooling may use bounded rename discovery only when the actual old/new relationship was genuinely not known at generation time.

### 22.5 New projects/modules: complete scaffold first, single cleanup pass, scaffold commit first

Project creation remains a deliberate commit-granularity exception, but it must not create an extra VCmd/opening cycle.

Mutation order:

1. Create the complete repository-standard scaffold.
2. Add each project using the Section 20.2 readiness barrier and explicit `$dte.Solution.AddFromFile($fullProjectPath, $false)`.
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

### 22.7 Per-work-item discipline, transaction-private staging, and Git-proven commit identity

For this specification, **exact staging means transaction-owned scope isolation, not repository cleanliness and not record-count equality**. The maintainer's default Git index may contain unrelated staged work throughout the transaction.

For each work item:

1. Refresh Git status for the transaction-owned pathset and resolve the current `HEAD` commit ID.
2. If the work item already has no working-tree difference to capture relative to the current `HEAD`, treat that as retry-safe/no-op and select the next item. Do not inspect unrelated paths as a gate.
3. Create a fresh transaction-private alternate index as described in Section 13.1 and seed it from the current `HEAD` with `git read-tree HEAD`.
4. Stage the complete **generation-time-declared authorized path set directly into that private index**. For a known rename/move, stage the predeclared old/new pair together; do not perform rename-discovery staging.
5. Read the actual private-index staged diff/status with rename/copy awareness, retaining both old and new pathnames for `R`/`C` entries. Verify that the private staged diff is non-cross-repository and contains no unauthorized logical path.
6. Do **not** compare `expectedPathCount` with `stagedRecordCount` as a fatal invariant. If Git folded old/new paths into a rename/copy record, an input path is already no-op, or status normalization changed record count, continue when the actual staged diff is wholly authorized.
7. If the private staged diff is empty, dispose the private index, report the work item as a legitimate no-op, refresh transaction-owned status, and select the next item. Do not leave transaction-owned staging residue behind.
8. If an unauthorized path appears in the private staged diff, discard/recreate only the private index, recompute the work item, and retry within a bounded budget. Never reset or clean unrelated maintainer index/work-tree state.
9. Generate/validate the repository-compliant commit message from the **actual private staged diff**.
10. Commit via the UTF-8-no-BOM temporary message file while the same private index is active.
11. Immediately resolve `HEAD` again and require the full commit ID to differ from the pre-commit `HEAD`.
12. Resolve the actual abbreviated SHA and subject from the new `HEAD` and emit a concise success diagnostic.
13. Normalize only the just-committed transaction-owned path entries in the maintainer's real index to the new `HEAD` when needed so stale pre-commit entries for those authorized paths do not survive. Do not change working-tree bytes and do not touch unrelated staged entries.
14. Refresh machine-readable status for the transaction-owned work-item paths. If one becomes immediately dirty again because the IDE rewrote it, route it through the post-capture stabilization/fixed-point workflow instead of invalidating the commit.
15. Dispose/delete the private index and temporary commit-message file in `finally`.
16. Record implementation-commit counts only as informational control state; observed Git `HEAD` transitions/SHA values are the evidence.
17. If the transaction aborts before any meaningful positive mutation ever succeeded, apply the Section 15 empty-boundary cleanup rule when applicable; that cleanup does not require a clean work tree/index.
18. If meaningful positive mutation has occurred, preserve forward source/project/Git progress unless an explicit narrow rollback contract applies.
19. Select the next work item from fresh transaction-owned Git status.

Never allow unrelated paths into the transaction-private staged diff. Never span two Git repositories in one commit. Never use a default-index-cleanliness assertion, a staged-path **count** assertion, a commit-helper success flag, or an implementation-commit counter as a substitute for inspecting the actual transaction-private staged diff and resulting Git history.

### 22.8 Post-capture IDE/Git fixed-point stabilization

One ordered capture pass is not necessarily the end of Git capture. Visual Studio, ReSharper, the project system, or a delayed document save can still rewrite transaction-owned files after one or more commits have been created. This specification therefore requires a bounded **post-capture fixed-point loop** before final synchronization.

1. After the current ordered capture pass appears complete, call `File.SaveAll`, pump `Application.DoEvents()`, and observe the complete transaction-owned path set.
2. Derive an adaptive quiet interval and maximum wait using the same bounded square-root policy from Section 9, substituting the count of transaction-owned paths currently subject to IDE writes for `N` when that set is larger/more appropriate than the VCmd-opened set.
3. During that adaptive stabilization window, repeatedly refresh content fingerprints/status while pumping the IDE and periodically saving.
4. If a transaction-owned path becomes dirty, do not immediately stage a moving target. Reset the quiet interval and wait until the transaction-owned set is content-stable for the adaptive interval.
5. Once stable, refresh Git status and re-enter the normal Section 22 selector/work-item commit loop for every transaction-owned path that is dirty.
6. Do **not** rerun VCmd simply because late IDE writes appeared after the original one-time VCmd pass. VCmd remains exactly once per transaction; this loop is about capturing the final stable bytes that the IDE produced.
7. After the recapture pass, repeat the adaptive stabilization check.
8. Bound the total fixed-point process by both a finite total duration and a finite recapture-round count (normally no more than three recapture rounds unless the current prompt expressly authorizes more). If transaction-owned paths still cannot remain captured/stable, stop before claiming synchronization success and preserve all commits/source progress.
9. Internal counters may describe how many recapture rounds or commits occurred, but the fixed point is established only by Git/status plus the content-stability observations.

The required fixed point is: **all transaction-owned paths are content-stable and fully represented by the intended transaction commits at the same observed boundary.** Unrelated working-tree or default-index dirt may still exist and is irrelevant to that fixed point.

### Git-recovery-only exception to Section 22.8

For a Section 1.1 Git-recovery-only transaction, skip the adaptive content-fingerprint/quiet-period loop entirely unless execution has concrete evidence that the IDE is actively rewriting recovery-owned files.

After the ordered capture pass:

1. run `File.SaveAll` once;
2. refresh Git status immediately;
3. if no recovery-owned path appears in fresh status, proceed directly to Section 23;
4. if one or more recovery-owned paths are dirty again, re-enter Section 22 ordered capture from fresh status without an artificial wait; and
5. bound these direct-status recapture rounds (normally no more than three unless the current prompt authorizes more).

For recovery-only work, **absence of recovery-owned paths from fresh Git status is the fixed-point observation**. The repository as a whole need not be clean. Content hashing and elapsed quiet intervals are unnecessary when the script did not create the delayed-write hazard they were designed to guard against.

## 23. Final Git Synchronization, Push, and End-State Proof

Finalization proves the transaction's own result. It does **not** require the repository as a whole, the work tree, or the maintainer's default index to be clean.

### 23.1 Pre-synchronization proof

After the Section 22 ordered-capture/fixed-point loop:

1. Run `File.SaveAll`, pump the IDE, and refresh status for the complete transaction-owned path set.
2. Require no transaction-owned path to remain represented by an uncommitted working-tree/default-index change that belongs to the transaction. If one is dirty again, return to the applicable Section 22.8 recapture workflow.
3. Ignore unrelated dirty/staged/untracked paths except to ensure the transaction will not accidentally touch or commit them.
4. If zero implementation commits were genuinely created and the transaction-owned empty boundary exists, apply the verified no-op boundary cleanup rule without requiring repository cleanliness.
5. Resolve and retain the actual current `HEAD` from Git.

### 23.2 Upstream synchronization without disturbing unrelated local state

If the current branch has no configured upstream, report proven local transaction completion and stop the remote-synchronization phase successfully.

If an upstream is configured:

1. A fetch/read-only upstream refresh may be performed because it does not require cleaning/stashing the working tree/index.
2. Resolve ahead/behind state from Git.
3. If the local branch is not behind the upstream, push the transaction commits as appropriate. A dirty working tree/default index is **not** a reason to suppress an otherwise-safe push because push operates on commits/refs rather than uncommitted files.
4. If the local branch is behind and reconciliation would require `pull --rebase`, merge, stash, auto-commit, reset, or otherwise disturbing unrelated local state, do **not** manufacture cleanliness. Report that the transaction-owned work is complete locally and that remote synchronization was deferred.
5. If the local state happens to permit a non-disruptive pull/rebase, the transaction may perform it, then re-prove transaction-owned paths and push.
6. A rejected/non-fast-forward push is not a reason to roll back source or commits; preserve local completion and report the synchronization limitation.

Do not assume a remote named `origin`; use the configured upstream relationship.

### 23.3 Post-push stabilization and recapture

For a source-mutating transaction, a successful push is not the terminal condition. Immediately afterward:

1. Call `File.SaveAll` again and pump the Visual Studio message loop.
2. Run the bounded adaptive stability observation over transaction-owned paths using the Section 9 timing policy.
3. Refresh transaction-owned Git status after the adaptive quiet interval and final save/resample.
4. If transaction-owned dirt appears, preserve it, wait for stability, return to ordered Section 22 capture, create the required new commit(s), and synchronize/push again when mechanically non-disruptive.
5. Bound this final capture/synchronize cycle by the finite recapture-round policy. If transaction-owned paths cannot reach a stable captured state, report an actionable error/warning and do not print full transaction success.

A Git-recovery-only transaction instead uses direct `File.SaveAll` + fresh status of recovery-owned paths with bounded recapture; it does not manufacture an adaptive wait without evidence of active rewriting.

### 23.4 Final Git proof

Immediately before the final success diagnostic, obtain fresh evidence from Git:

1. Verify every transaction-owned path is absent from uncommitted Git status (or otherwise exactly in the transaction's intended committed state).
2. Do **not** require the default Git index or whole work tree to be empty; unrelated staged/dirty paths are allowed to remain.
3. Resolve the final local `HEAD` full SHA, abbreviated SHA, and subject through Git and report them concisely.
4. When upstream synchronization actually occurred, verify the local/upstream relation with an ahead/behind query equivalent to `git rev-list --left-right --count HEAD...@{upstream}` and require `0 0` for the synchronized claim.
5. When synchronization was deferred because remote reconciliation would disturb unrelated local state, explicitly report **local transaction completion** rather than claiming remote synchronization.
6. Only after all applicable transaction-owned postconditions are true may the script emit `*** SUCCESS ***` for the transaction.

A zero exit code from `git commit`, `git pull`, or `git push`, an internal implementation-commit count, or the mere absence of a thrown PowerShell exception is not sufficient proof of transaction completion.

## 24. Execution Log, `Write-Host` Diagnostics, Git Output, and Error Reporting

Every Change Transaction Script has **two diagnostic surfaces with different purposes**:

- PMC receives concise, human-oriented `Write-Host` progress/warning/error messages; and
- a detailed transaction execution log in `%TEMP%` records enough mechanical detail to diagnose a failure without asking the maintainer to reconstruct the run from memory.

### 24.1 Mandatory transaction execution log

At execution start, derive the delivered script basename and create a fresh log pathname in `%TEMP%` using:

```text
<script-basename>_yyyyMMdd_HHmmss_fff.log
```

The timestamp is the execution-start local timestamp. Initialize the log before the first source/DTE/Git/VCmd mutation and print the log pathname concisely to PMC so the maintainer can find it.

The detailed log should timestamp records and, where applicable, capture:

- script artifact pathname/basename and execution-start timestamp;
- PowerShell/PMC/CLR and loaded Solution identity;
- phase transitions and transaction mode;
- every helper/function entry and exit;
- helper inputs/arguments and important return/outcome values;
- important variables, flags, path collections, item counts, retry counts, timing values, and ownership/scope decisions;
- DTE operations, editor window/document observations, ReSharper suspend/resume operations, VCmd sidecar/invocation outcomes, and editor-state restoration;
- **semantic DTE call-boundary records immediately before and after potentially blocking COM calls**, including the exact project/Solution/source/reference identities and important arguments; generic function entry/exit alone is not sufficient;
- project-file readiness observations before `Solution.AddFromFile(...)`, including full path, existence, expected/observed byte length, readability/stability outcome, and the explicit `Exclusive = false` call intent;
- Git repo roots, commands/arguments, exit codes, relevant captured stdout/stderr, staged-diff/status interpretation, actual commit SHA/subject evidence, pull/rebase/push/upstream proof, and recapture rounds;
- warnings, errors, exception messages, invocation position, and script stack traces; and
- the final transaction state.

Do **not** dump giant Base64 payload bodies or complete source payloads merely because the log is verbose. For large payloads log compact metadata such as authorized path, byte length, encoding/newline facts when useful, and a generation/runtime synchronization hash when one is already being computed. Redact genuine secrets if a future transaction ever handles them.

The file log may be verbose; PMC should remain concise. Native Git output that would clutter PMC belongs in the detailed log unless a short summary is useful to the maintainer.

### 24.2 Final raw Git status is the last substantive log content

On both normal completion and failure, whenever mechanically possible, the script must perform all cleanup/restoration that could change editor/repository state **before** finalizing the execution log. This includes best-effort ReSharper resumption, preexisting editor-document restoration, `File.SaveAll`, temporary-file cleanup that can affect a repository, and any transaction-owned boundary cleanup permitted by Section 15.

Then append a clearly delimited final block containing the complete raw human-readable `git status` output for **every affected repository**, executed from that repository. If more than one repository is affected, identify each repository immediately before its raw status text.

Those final raw status block(s) are the **last substantive content written to the log**. Do not append a function-exit record, success message, cleanup note, elapsed-time line, or any other substantive log entry afterward. The helper that performs final-status logging is therefore a special terminal logger and must not log its own exit after it begins the final status block; closing/flushing the file handle is not a substantive log record.

If final raw status cannot be obtained for one repository, record that failure **inside that repository's final status block** and continue to the next affected repository when possible. After the final repository block, write nothing substantive.

If Visual Studio/PMC is forcibly terminated externally, PowerShell cannot execute the top-level cleanup/final-status path at all. That abrupt-host-death case is the explicit mechanical exception to terminal raw-status finalization; the usefulness of the log then depends on the last semantic pre-call DTE boundary entry written before the hang/termination.

### 24.3 Concise PMC diagnostics

Useful `Write-Host` diagnostics normally include, when applicable:

- validated Solution/repository context and the detailed log pathname;
- current work-tree/default-index context when useful, without treating dirt as a gate;
- mutation/scaffold/implementation phase transitions;
- before/after diagnostics for consequential DTE topology calls, especially project admission/removal, project-reference/source-item addition, and ReSharper/VCmd command boundaries;
- visible source-tab opening/activation and legitimate VCmd skips;
- VCmd sidecar/invocation and convergence timing;
- Git work-item selection and each actual committed SHA/subject;
- bounded recapture/synchronization progress;
- final Git proof; and
- warnings/errors that require maintainer attention.

Use concise prefixes such as:

```text
*** INFO *** ...
*** SUCCESS *** ...
*** WARNING *** ...
*** ERROR *** ...
```

Do not flood PMC with raw native streams, per-byte details, or the detailed function/variable trace that belongs in the `%TEMP%` log.

### 24.4 Error reporting

On a PowerShell exception, report enough context to diagnose the failure immediately: exception message, invocation position/line when available, and script stack trace when available. At the top-level transaction boundary, report that information once, run the safe transaction cleanup applicable to the current state, preserve meaningful forward progress, and normally do not rethrow merely to make PMC print the same failure again.

Do not leave the user with only an opaque message such as `Argument types do not match`. Do not print `*** SUCCESS ***` merely because PowerShell reached the end of the script; the applicable Section 23 Git postconditions must already have been observed.

## 25. Avoid Over-Engineering Safety Checks

???Abundance of caution??? does **not** mean adding every imaginable gate.

A safety check is valuable only when it answers a question that matters to the next action.

Bad examples:

- Treating `ReSharper_Suspend` command unavailability as fatal when ReSharper may simply already be suspended, or blindly resuming ReSharper later without knowing whether the transaction changed that state.
- Calling `Solution.AddFromFile(...)` immediately after writing a new `.csproj` without a bounded readiness/settling check and without logging the exact project/Solution call boundary before entering the potentially blocking DTE operation.
- Temporarily staging a generation-time-known old/new path pair merely to make Git decide whether to label the change as a rename.
- Reusing a project/AssemblyInfo identity GUID from an analogous project or a prior failed transaction when generating a new project attempt.
- Failing a work item because the number of staged status records is lower than the number of nominal input paths when Git represented an old/new pair as one rename/copy record.
- Calling `ProjectItem.Open(...)` and assuming the file is a VCmd-visible tab without retaining the returned `Window`, making it visible, activating it, pumping the IDE, and verifying `Document.Windows.Count > 0`.
- Invoking VCmd while an unrelated preexisting user-visible C# document remains open, even though VCmd gives open C# documents precedence and would broaden its processing scope.
- Assuming the VCmd automation sidecar remains in the transaction-supplied state after an invocation even though the command resets it to defaults during final cleanup.
- Treating a successful zero-output `git status --porcelain` as `$null` and passing that value into `[string]::Join(...)` or another string/collection operation.
- Refusing to run because the work tree or default Git index is dirty, or auto-committing/stashing/resetting unrelated dirt merely to manufacture a clean baseline.
- Generating `Assert-CleanIndex` (or an equivalent renamed helper) anywhere in a Change Transaction Script.
- Beginning Git staging immediately when the VCmd command call returns, without waiting for downstream `ReSharper_SilentCleanupCode` writes to the VCmd-opened file set to become content-stable.
- Using one fixed quiet interval/maximum wait for every transaction regardless of whether VCmd opened one source file or dozens; the barrier must scale adaptively with workload.
- Treating an internal `CreatedTransactionCommitCount` (or similar variable) as proof that Git commits actually exist.
- Printing `*** SUCCESS ***` after `git push` without a final `File.SaveAll`/adaptive settle/Git-status proof, thereby allowing delayed IDE writes to leave transaction-owned files dirty after the script claims completion.
- Applying the adaptive VCmd/post-capture/post-push content-fingerprint waits to a Git-recovery-only transaction whose files are already sitting unchanged and whose script performs no source/IDE cleanup mutation.
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
- Treating the Solution-level SEM, Solution-level `CONTRIBUTING.md`, or Solution-level `README.md` as transaction-owned merely because the file is present in the repository or under **Solution Items**.
- Opening `Global*.cs`, `*.Designer.cs`, `*.g.cs`, `*.i.cs`, `*.generated.cs`, or another known generated/fixed-format C# artifact merely because its extension is `.cs`.
- Excluding a changed `AssemblyInfo.cs` from VCmd merely because it is infrastructure/fixed-format source; `AssemblyInfo.cs` is the explicit supported exception.

Good examples:

- Treating ReSharper suspend/resume commands as idempotent state transitions: use command availability to infer preexisting state when practical, continue on harmless unavailability, and only undo state changes positively attributable to the transaction.
- Waiting for a newly written `.csproj` to become readable/stable, logging `Trying to add project '<full path>' to the Solution '<full path>'...`, calling `Solution.AddFromFile($fullProjectPath, $false)`, and immediately logging the returned `EnvDTE.Project` identity.
- Generating fresh current-attempt project/AssemblyInfo GUID identities instead of copying analogous/prior values, while retaining externally defined fixed API GUID constants unchanged.
- Predeclaring a known old/new rename pair at generation time, staging those authorized pathspecs directly, and allowing Git to display `R` or `D+A` according to its heuristics.
- Defining exact staging as "no unauthorized staged logical paths," inspecting rename-aware staged status/diff, and generating the commit message from the actual staged diff without asserting record-count equality.
- Capturing the `EnvDTE.Window` returned by `ProjectItem.Open(vsViewKindTextView)`, setting it visible, activating it, pumping/waiting briefly, and counting it only after observable user-visible document-window state exists.
- Snapshotting preexisting user-visible C# tabs, temporarily closing only unrelated ones before VCmd, and restoring that user editor state after convergence or failure.
- Rewriting the exact noninteractive/Git-disabled sidecar immediately before every VCmd invocation because the command resets the canonical file afterward.
- Normalizing empty native stdout/stderr to non-null strings and treating clean zero-output Git status as an empty collection.
- Leaving unrelated dirty/staged work exactly where it is, using transaction-private/path-isolated Git capture for authorized paths, and never requiring repository cleanliness.
- Seeding a transaction-private index from `HEAD`, staging only the authorized work item into it, inspecting that private staged diff, committing from it, and leaving unrelated entries in the maintainer's default index untouched.
- Waiting after VCmd until repeated content fingerprints of every successfully VCmd-opened source file remain unchanged for a continuous quiet interval while pumping the IDE and periodically saving.
- Computing the quiet interval and maximum observation window from the number of successfully VCmd-opened files using the bounded square-root policy, and reporting the selected timings before waiting.
- Resolving `HEAD` before and after every commit, requiring it to advance, and reporting the actual abbreviated commit SHA/subject from Git.
- Re-entering ordered Git capture when delayed IDE writes make transaction-owned paths dirty after an earlier commit/push, then proving the final local/upstream state before printing success.
- For Git-recovery-only work, performing `File.SaveAll`, then immediately staging/committing the authorized dirty paths and using direct fresh Git-status checks after capture/push instead of artificial content-change waits.
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
- Verifying the actual staged diff contains only authorized logical paths before commit.
- Confirming that the required DTE reference-add operation returned normally and then flushing with `File.SaveAll`, without making a `.csproj` reread a fatal gate.
- Re-resolving authorized target paths after `git pull` when repository topology may have changed.
- Generating updated Solution-level SEM/`CONTRIBUTING.md`/`README.md` artifacts separately for manual application while keeping those paths out of the Change Transaction payload/staging maps.

The question to ask before every gate is:

> Does failure of this condition actually make the next action unsafe or nonsensical?

If not, do not make it a hard gate.

---

## 26. Recommended High-Level Execution Order

### Alternate flow ??? Git-recovery-only transaction

When Section 1.1 applies:

1. Enter child scope, initialize the detailed `%TEMP%` execution log, and enable strict error behavior.
2. Validate host `$dte`, loaded Solution, repository identity, branch, and configured upstream without assigning/binding `$dte`.
3. Snapshot preexisting user-visible editor state for restoration diagnostics, then run `File.SaveAll` once.
4. Resolve the authorized recovery path set. Observe but do not clean/commit/stash/reset unrelated working-tree/default-index state.
5. Do **not** create a boundary, suspend/resume ReSharper, open source for VCmd, invoke VCmd, write source payloads, mutate topology/references, or run artificial content-fingerprint waits.
6. Perform Section 22 ordered capture using transaction-private/path-isolated staging and prove each commit by `HEAD` transition and actual SHA/subject.
7. Use direct `File.SaveAll` + fresh status of recovery-owned paths after capture/push and bounded direct recapture if they reappear dirty.
8. Synchronize remotely only when doing so does not require disturbing unrelated local state; otherwise report local completion.
9. Perform transaction cleanup, then append complete raw final `git status` for every affected repository as the last substantive log content.

### Phase 1 ??? establish Visual Studio, logging, editor snapshot, and Git context

1. Enter child scope; strict errors.
2. Initialize the detailed `%TEMP%` execution log using the GUID script basename plus execution-start timestamp.
3. Validate host `$dte` without binding/assigning it, the loaded Solution, and loaded projects.
4. `File.SaveAll`, then snapshot all preexisting user-visible documents (`Document.Windows.Count > 0`) and the active document pathname when practical.
5. Resolve authorized targets and Git repoRoot(s) using junction-safe identity.
6. Observe current Git work-tree/default-index state for diagnostics only. Do not auto-preserve/commit/stash/reset unrelated dirt and do not require cleanliness.
7. Remove only a recognized orphan empty transaction boundary when allowed; optionally synchronize from upstream only when non-disruptive; create/adopt any required transaction boundary with the transaction-private index model.
8. Establish ReSharper state idempotently and nonfatally, with bounded message pumping after any successful transition.

### Phase 2 ??? perform every topology/source mutation while ReSharper is suspended when possible

1. Create complete new-project scaffold(s) first when required, generating fresh current-attempt identity GUIDs rather than copying/reusing prior values.
2. Before each new-project `AddFromFile`, wait boundedly for the actual full `.csproj` path to be readable/stable, log the exact project/Solution boundary, call `Solution.AddFromFile($fullProjectPath, $false)`, capture/log the returned `EnvDTE.Project`, then continue.
3. Add other source items/references through DTE/project-system operations with semantic before/after call-boundary logging.
4. Apply exact audited source payloads in dependency order, excluding Solution-level manual governance deliverables.
5. Record each VCmd-eligible affected path into the transaction-wide registry and mark meaningful mutation after the first successful positive operation.
6. Run `File.SaveAll` after all mutations.

### Phase 3 ??? isolate VCmd scope and visibly open the intended tabs

1. Resolve the complete transaction-owned changed-path set and derive the final VCmd-eligible C# set.
2. Save and temporarily close only unrelated preexisting user-visible C# documents outside that set; never use `Window.CloseAllDocuments`.
3. If exact isolation cannot be established, restore already-closed tabs, warn/skip VCmd, and continue.
4. Open each intended eligible path through `ProjectItem.Open(vsViewKindTextView)` when possible, retain/make-visible/activate the returned window, pump/wait, and verify `Document.Windows.Count > 0` before reporting success.
5. Use `ItemOperations.OpenFile(...)` only as a genuine fallback and perform a final bounded opening-pass settle.

### Phase 4 ??? resume ReSharper, run one VCmd pass, converge, and restore editor state

1. Restore/transition ReSharper for cleanup idempotently and nonfatally.
2. Rewrite the exact schema-version-2 noninteractive/Git-disabled VCmd sidecar immediately before invocation.
3. Invoke argumentless `VCmd.CCommandStripLineBreaksFromAllComments` once when at least one intended file is observably user-visible and isolation/sidecar preparation succeeded.
4. Observe the exact successfully opened pathname set with the adaptive content-fingerprint convergence barrier.
5. Restore preexisting editor/ReSharper state, run final `File.SaveAll`, and refresh transaction-owned Git status.

### Phase 5 ??? ordered Git capture from transaction-private indexes

1. Refresh transaction-owned status and select the next work item.
2. Capture scaffold/module-family topology first when applicable, then ordinary implementation file-by-file except explicit source-family/rename/topology units.
3. For each work item, create a fresh private index seeded from `HEAD`, stage only the generation-time-authorized pathset, and inspect the actual rename/copy-aware private staged diff.
4. Require no unauthorized logical path in that private diff; never require the maintainer's real index to be clean and never assert staged-record count equality.
5. Generate the commit message from the actual private staged diff, commit through that private index, prove the `HEAD` transition/SHA/subject, and normalize only transaction-owned entries in the real index when needed.
6. Dispose the private index and continue from fresh transaction-owned status.
7. Run the Section 22.8 fixed-point stabilization and recapture late transaction-owned writes without rerunning VCmd.

### Phase 6 ??? optional remote synchronization and transaction-owned end-state proof

1. `File.SaveAll`, pump, and prove transaction-owned paths are committed/stable.
2. Ignore unrelated dirty/staged state except to avoid disturbing or committing it.
3. If upstream synchronization can occur without reorganizing unrelated local state, synchronize/push and prove `0/0` ahead/behind.
4. If reconciliation would require stashing/committing/resetting unrelated dirt, defer remote synchronization and report proven local completion instead.
5. Run applicable post-push transaction-owned stabilization/recapture.

### Phase 7 ??? cleanup and terminal log finalization

1. Restore any remaining preexisting editor documents and best-effort ReSharper state.
2. Perform permitted boundary/no-op cleanup with work-tree/index-preserving operations, remove temp/private-index files, dispose owned resources, and run final `File.SaveAll` when DTE remains usable.
3. Log cleanup/restoration failures now; do not erase meaningful forward progress or unrelated local state.
4. Append complete raw `git status` output for every affected repository to the detailed log.
5. Write no substantive log content after the final repository status block.
6. Never release/rebind/destroy `$dte`.

## 27. Side-Effect Gate Matrix

| Action | Immediate precondition | Required postcondition | No-op / recovery |
|---|---|---|---|
| `File.SaveAll` | Valid host DTE + loaded Solution | Command completes | Execute at defined checkpoints |
| Initialize detailed transaction log | script basename + `%TEMP%` available | fresh `<basename>_yyyyMMdd_HHmmss_fff.log` exists before first mutation | log initialization failure is actionable before mutation |
| Observe Git status | repository identified | diagnostic/ownership information captured | dirty work tree/default index is normal and never a failure by itself |
| `Assert-CleanIndex` or equivalent | **Never valid** | **Must not exist/call** | remove from generated script; do not recreate under another name |
| Transaction-private Git index | repo + current `HEAD` + authorized work item | private index seeded from `HEAD`; only authorized paths staged/committed | default index may contain unrelated staged work throughout |
| Normalize real index for committed transaction paths | transaction commit advanced `HEAD` + authorized paths known | only those authorized index entries match new `HEAD`; working-tree bytes/unrelated index untouched | skip when no transaction-owned stale entry exists |
| Git-recovery-only mode | authoritative recovery path set + desired bytes already live | no source/VCmd/ReSharper mutation; authorized paths captured via private/path-isolated Git state | unrelated dirt/staging remains untouched |
| Clobber authorized source/project file | authorized path + audited payload + writable target | exact payload bytes written; meaningful mutation recorded | old source/Git cleanliness irrelevant |
| Pre-mutation failure after empty boundary | recognized transaction-owned empty boundary + no meaningful mutation | boundary removed with soft/ref-only operation preserving work tree/index | no clean-tree/index requirement |
| Successful no-op transaction | transaction-owned empty boundary + zero implementation commits | bookkeeping-only boundary removed without disturbing work tree/index | report no-op success |
| Create project scaffold | correct repo + analogous scaffold | complete scaffold exists with fresh generated GUIDs | known partial scaffold can be completed; ownership ambiguity stops |
| ReSharper state transition | source-mutating transaction + DTE available | transaction records successful owned transition | unavailable toggle is info/warn and nonfatal |
| Add project to Solution | scaffold written + `.csproj` readable/stable | explicit `Solution.AddFromFile($fullProjectPath, $false)` returns/logs project | already loaded correctly: skip |
| VCmd cleanup | isolated intended visible C# scope + sidecar prepared | one argumentless VCmd attempt + convergence/SaveAll | unavailable/failure warning-only |
| Post-VCmd convergence | VCmd ran against opened eligible paths | transaction-owned opened paths content-stable through final sample | timeout stops Git capture but preserves source progress |
| Select implementation work item | fresh transaction-owned status | next authorized work item chosen | no transaction-owned changes: capture phase complete |
| Stage work item | private index seeded from `HEAD` | private staged diff contains only authorized logical paths; no count-equality rule | empty private diff: no-op; recreate private index on scope defect |
| Git commit | authorized private staged diff + valid message + pre-commit `HEAD` | post-commit `HEAD` advances; actual SHA/subject observed | unrelated real-index staging is irrelevant |
| Post-capture fixed point | ordered capture pass completed | transaction-owned paths stable/committed | unrelated working-tree/index dirt may remain |
| Final synchronization | configured upstream + synchronization can be non-disruptive | push/sync proof when performed | if reconciliation would disturb unrelated local state, report local completion and defer sync |
| Final Git proof | transaction-owned capture complete | transaction-owned paths committed; final `HEAD` resolved; `0/0` only when sync occurred | whole repository/default index need not be clean |
| Finalize detailed log | cleanup/restoration complete | raw `git status` for every affected repo is final substantive log content | status failure recorded inside final block |

## 28. Review Checklist Before Delivering a PMC Script

### PowerShell / PMC compatibility

- [ ] Target is Visual Studio PMC and the verified PowerShell 5.1 Desktop parser/binder behavior.
- [ ] Entire implementation is child-scoped when dot-sourced.
- [ ] The PowerShell source contains no nested `try`/`catch`/`finally` blocks; distinct inner exception boundaries are extracted into named helper functions.
- [ ] No assignment/binding/shadowing/removal of `$dte` in any casing.
- [ ] No PowerShell 7-only syntax.
- [ ] Regex escapes were checked for PowerShell/.NET semantics (`'\b'`, not `'\\b'`, for a word boundary in a single-quoted pattern).
- [ ] Native Git stdout/stderr is redirected and both streams are drained concurrently before/while waiting for process exit.
- [ ] Git waits are bounded; timed-out processes are terminated best-effort and disposed.
- [ ] The top-level transaction catch reports actionable context and normally returns control to PMC without redundant rethrowing.
- [ ] Errors report invocation/stack context.
- [ ] Useful transaction progress, no-op, warning, and failure diagnostics are emitted through `Write-Host` without flooding PMC.
- [ ] For source-mutating transactions, ReSharper suspend/resume is idempotent and nonfatal: command availability/preexisting state is tracked when practical, harmless unavailability does not abort, and only transaction-owned state changes are reversed.
- [ ] Every successful ReSharper state-toggle DTE call is logged before entry and immediately after return, followed by bounded wait/message pumping; cleanup does not blindly resume a preexisting suspended state.
- [ ] Git-recovery-only artifacts intentionally omit ReSharper suspension/resumption because they perform no source/project/Solution mutation.
- [ ] The exact delivered `.ps1` artifact has been parsed/static-checked for PowerShell 5.1 compatibility after it was written to disk.
- [ ] Successful zero-output native/Git results are normalized to non-null strings/empty collections; no clean `git status --porcelain` path can feed `$null` into `[string]::Join`, `.Trim()`, `.Split()`, or collection coercion.
- [ ] The exact artifact contains no known PowerShell 5.1 trailing-comma parse traps and uses `${Name}:` (or equivalent) when a colon follows an interpolated variable name.
- [ ] The detailed `%TEMP%` execution log is initialized before mutation and uses the GUID script basename plus execution-start timestamp.
- [ ] Detailed logging covers function entry/exit, inputs/outcomes, important variables/collections/counts, DTE/Git/VCmd activity, warnings/errors/exceptions, without dumping giant Base64/source payload bodies.
- [ ] Every potentially blocking DTE/COM operation has a semantic pre-call log record containing exact identities/arguments and an immediate returned/outcome record; `Solution.AddFromFile` includes full project path, full Solution path, and `Exclusive = false`.
- [ ] Success and failure finalization perform cleanup/editor restoration first, then append complete raw `git status` for every affected repository as the last substantive file-log content.

### Project creation

- [ ] The closest analogous existing project(s) were inspected.
- [ ] Every new project has the complete expected scaffold, not a minimal skeleton.
- [ ] `GlobalAspects.cs`, `AssemblyInfo.cs`, resources, README, signing key, packages, config, icon, and other normal peer files are included/adapted when the analogous project has them.
- [ ] New-project creation correctly treats `GlobalAspects.cs`/`AssemblyInfo.cs` as scaffold exceptions to the ordinary no-regeneration rule.
- [ ] Empty or known-partial project directories are retry-safe; unexpected contents stop the transaction.
- [ ] Every newly generated project/assembly identity uses fresh current-attempt GUID values; no `ProjectGuid`, `AssemblyInfo` COM GUID, or other generated identity GUID is copied/reused from an analogous project, prior failed transaction, or earlier generated artifact. Externally defined fixed API GUID constants remain unchanged.
- [ ] Before adding a new project, the exact full `.csproj` path passes a bounded filesystem-readiness/readability/stability check using its generated expected metadata when available.
- [ ] New projects are added with explicit `$dte.Solution.AddFromFile($fullProjectPath, $false)`; `.sln` project entries are never hand-authored. The exact project/Solution call boundary is logged before entry and the returned `EnvDTE.Project` identity is logged immediately after return.
- [ ] Project references are added through the consuming loaded project's DTE/VSProject reference collection, not by writing `<ProjectReference>` XML.
- [ ] Visual Studio is allowed to persist relative project/reference paths; junction-canonicalized absolute `C:`/`D:` spellings are not injected into `.sln`/`.csproj`.
- [ ] New project/module-family scaffolds are captured atomically as the first project-creation commit before implementation commits; functional source may already exist unstaged because the single VCmd pass occurs after all mutations.
- [ ] The `.sln` membership change is included in the scaffold checkpoint when the modeled project-creation transaction treats it as part of that atomic scaffold.
- [ ] Functional implementation is committed afterward using file-by-file/source-family/rename-aware selector rules.

### Commit selection/messages

- [ ] The exact artifact contains no `Assert-CleanIndex` definition, call, alias, or equivalent fatal clean-index helper.
- [ ] No work-tree/default-index cleanliness assertion can abort a source-mutating or Git-recovery-only transaction.
- [ ] Transaction-owned commits use a private alternate index (or another proven path-isolated mechanism) so unrelated staged work in the maintainer's default index cannot hitchhike.
- [ ] The transaction never auto-commits, stashes, resets, restores, or cleans unrelated preexisting dirt merely to manufacture a clean baseline.
- [ ] Ordinary implementation commits default to file-by-file granularity.
- [ ] Multi-file implementation commits occur only for explicit CreateStagedGitDiff/source-family/rename/topology exceptions.
- [ ] Files are not grouped merely because they share a feature/refactor/increment.
- [ ] Scaffold project/module-family creation is recognized as an explicit granularity exception.
- [ ] Message generation uses the actual staged diff.
- [ ] Single-file `Create <file>`/`Update <file>` rules are honored before generic validation.
- [ ] No brittle finite verb/past-tense whitelist can reject an otherwise specification-compliant message.
- [ ] Staging is exact in the sense of **no unauthorized staged logical paths**; rename/copy-aware staged status is inspected and staged-record count equality is never a fatal invariant.
- [ ] An empty actual staged diff is handled as a no-op; a rename/copy record may cover both original and destination pathnames as one logical artifact.
- [ ] Generation-time-known rename/move pairs are predeclared and staged directly as atomic authorized work items; no temporary speculative staging/reset cycle is used merely to make Git discover the rename.
- [ ] Every commit captures pre-commit `HEAD`, resolves post-commit `HEAD`, requires it to advance, and reports the actual abbreviated SHA/subject from Git.
- [ ] Internal commit counters are treated as informational only and are never used as proof that Git history changed.
- [ ] The post-capture fixed-point loop can recapture delayed IDE writes without rerunning VCmd.
- [ ] If the artifact is Git-recovery-only, it does not create a new boundary, mutate source/topology/references, suspend/resume ReSharper, invoke VCmd, or run adaptive content-fingerprint waits merely to check in already-existing files.
- [ ] If the artifact is Git-recovery-only, authorized recovery dirt is captured directly and unrelated working-tree/default-index state is left untouched; no clean-index/work-tree requirement exists.
- [ ] If the artifact is Git-recovery-only, post-capture and post-push checks use direct `File.SaveAll` + fresh Git status with bounded direct recapture rounds; content-stability waits appear only if actual active IDE rewriting is observed.

### Visual Studio/Git/source safety

- [ ] The generator audited the current authoritative source and produced complete exact desired-state payloads wherever feasible.
- [ ] Every changed C# payload passed symbol-to-namespace closure: `[Log]`/`[NotLogged]`/PostSharp diagnostic aspects have `using PostSharp.Patterns.Diagnostics;` (unless intentionally fully qualified), `[DebuggerStepThrough]` has `System.Diagnostics`, `DebugUtils` has `xyLOGIX.Core.Debug`, and introduced symbols have their real positive project/assembly dependency closure.
- [ ] Large/complex exact payloads use a transport that preserves exact audited bytes (prefer Base64 + `WriteAllBytes`); every embedded payload was decoded and compared with the generation-time desired bytes before delivery.
- [ ] Authorized target/payload/commit-message maps are internally consistent before delivery.
- [ ] The Solution-level SEM, Solution-level `CONTRIBUTING.md`, and Solution-level `README.md` are absent from transaction-owned target/payload/staging/commit maps even when they are Solution Items; requested updates are separate manual-download artifacts.
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
- [ ] Source-mutating and Git-recovery-only transactions tolerate arbitrary preexisting working-tree/default-index dirt; no unrelated preservation commit, stash, reset, restore, clean, or cleanliness gate is generated.
- [ ] Git status is observed for transaction-owned scope/ownership and optional synchronization feasibility, not as a cleanliness precondition.
- [ ] Unrelated dirt/staging may exist before, during, or after the transaction; it is never staged/reset/committed by the transaction, and remote synchronization is deferred only when reconciliation would disturb it.
- [ ] Pull/rebase/push is keyed to the current branch's configured upstream, not merely to the existence of a remote.
- [ ] For source-mutating transactions that use a boundary, empty-boundary creation/reuse is retry-aware when practical; Git-recovery-only transactions do not create a new boundary.
- [ ] Recognized orphaned empty boundaries from earlier failed iterations are removed only when `HEAD`/empty/ownership checks make cleanup unambiguous, using a soft/ref-only operation that preserves arbitrary work-tree/index state.
- [ ] A successful source-mutating transaction that produces no implementation commit removes its bookkeeping-only boundary without requiring repository cleanliness; Git-recovery-only transactions have no new boundary to remove.
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
- [ ] ReSharper is transitioned for VCmd cleanup only after the full opening pass, using the idempotent Section 6.6 state model; successful transitions are followed by bounded wait/message pumping and any preexisting suspended state temporarily resumed for cleanup is restored afterward.
- [ ] The script derives a separate VCmd processing set rather than equating "changed" or "text/source-editable" with VCmd eligibility.
- [ ] The VCmd processing set contains ordinary changed hand-authored `.cs` files and explicitly includes changed `AssemblyInfo.cs`.
- [ ] The VCmd processing set excludes `Global*.cs`, `*.Designer.cs`, `*.g.cs`, `*.i.cs`, `*.generated.cs`, other known generated/fixed-format C# artifacts, and build-output/intermediate source.
- [ ] The VCmd processing set excludes all non-C# artifacts, including project/Solution/build metadata, resources, configuration/data files, documentation/text files, signing keys, icons, and other binary/scaffold artifacts.
- [ ] Every VCmd-eligible changed C# file is **attempted** before VCmd in the Visual Studio source-code/text editor.
- [ ] Project-owned VCmd-eligible files are preferentially opened through their actual DTE `ProjectItem.Open(...)` relationship; `$dte.ItemOperations.OpenFile(...)` is not used as the default for project-owned source.
- [ ] The `EnvDTE.Window` returned by `ProjectItem.Open(...)` is retained, made visible, activated, followed by `Application.DoEvents()` + a short bounded wait + another pump, and success is not reported until the intended document owns at least one window.
- [ ] Preexisting user-visible documents and the prior active document are snapshotted before the cleanup pass; unrelated preexisting visible C# tabs are temporarily closed for VCmd scope isolation and restored afterward/failure, while unrelated non-C# tabs are left alone.
- [ ] If unrelated visible C# scope cannot be isolated safely, VCmd is skipped rather than knowingly processing unrelated source.
- [ ] The paced opening loop allows bounded project-association settling so files are not unnecessarily relegated to **Miscellaneous Files**.
- [ ] Individual eligible source-editor open failures are warning-only and do not roll back successful source/project mutations.
- [ ] Excluded changed paths remain part of the transaction/Git changed set and are not opened merely for VCmd.
- [ ] Eligible WinForms primary `.cs` files are explicitly opened as source text; the transaction never activates the WinForms Designer for cleanup.
- [ ] VCmd eligibility is determined mechanically from path/classification rules established during generation-time audit; runtime source contents are not parsed to decide eligibility.
- [ ] Immediately before the single VCmd invocation, the script writes `%LOCALAPPDATA%\xyLOGIX, LLC\Visual Commander\Commands\Strip Line Breaks from All Comments\Config\.config.json` using schema `2` with `SuppressPrompts = true`, `EnableGitAwareness = false`, and `AutomaticallyCheckInChangesToGitWhenGitAwarenessIsSuppressed = false`.
- [ ] The script assumes VCmd loads that sidecar once and resets it to defaults at the end of the invocation; every invocation therefore rewrites the exact automation sidecar immediately beforehand.
- [ ] The convergence observation set is pathname-based and retains successfully opened `AssemblyInfo.cs` paths even if VCmd closes those editor windows during its cleanup-only handling.
- [ ] VCmd behavioral assumptions match the supplied source: `AssemblyInfo.cs` uses `ReSharper.ReSharper_SilentCleanupCode`; remaining open documents use `CodeMaid.CleanupOpenCode`, `ReSharper.ReSharper_SilentCleanupOpenFiles`, and `File.SaveAll`.
- [ ] VCmd is never invoked when that sidecar preparation fails; the failure is warning-only, VCmd is skipped, and final `File.SaveAll`/script-owned Git capture continue.
- [ ] `VCmd.CCommandStripLineBreaksFromAllComments` is invoked without command arguments; no `NoPrompt` argument or equivalent is used.
- [ ] The VCmd sidecar disables all VCmd-owned Git behavior so the Change Transaction Script remains solely responsible for synchronization, staging, custom commit-message generation, commits, and push.
- [ ] VCmd is attempted only after the complete VCmd-eligible opening pass has finished and successful sidecar preparation, and normally once for the successfully opened eligible set; VCmd failure is warning-only and final `File.SaveAll` still occurs.
- [ ] After VCmd returns, the script does not begin Git capture immediately; it enters a bounded post-VCmd convergence barrier that pumps the IDE, periodically calls `File.SaveAll`, and repeatedly fingerprints the actual contents of the files successfully opened for VCmd.
- [ ] The convergence barrier accounts for downstream background `ReSharper_SilentCleanupCode` work, resets its quiet timer whenever any VCmd-opened file changes, and treats a fixed short sleep or timestamp-only check as insufficient.
- [ ] The VCmd convergence quiet interval and maximum window are computed adaptively from the exact successfully opened file count using the bounded current square-root policy, and the selected timings are reported through `Write-Host`.
- [ ] After the quiet interval, the script performs a final `File.SaveAll`/message-pump/fingerprint sample and begins Git capture only if that final sample remains unchanged.
- [ ] If post-VCmd convergence cannot be established within the finite maximum wait, the script stops before Git staging/commit while preserving source/project progress.
- [ ] After ordered Git capture, a source-mutating transaction performs adaptive fixed-point stabilization; a Git-recovery-only transaction instead performs direct `File.SaveAll` + fresh status and bounded direct recapture without artificial quiet waits.
- [ ] After push, a source-mutating transaction performs adaptive save/pump/status stabilization; a Git-recovery-only transaction instead performs direct `File.SaveAll` + fresh status and bounded direct recapture/re-push without artificial quiet waits.
- [ ] Final `*** SUCCESS ***` is impossible until Git proves transaction-owned paths are fully captured/stable and final `HEAD` resolves; synchronized repositories require `0/0` ahead/behind, but unrelated work-tree/default-index dirt may remain.
- [ ] `*** INFO *** Synchronizing completed transaction commits...` cannot be emitted before actual commit SHA(s) have been resolved from Git.
- [ ] No source-byte/hash/layout/semantic match is required before overwriting an authorized target or before VCmd.
- [ ] No semantic/textual source verification is performed after successful VCmd cleanup.
- [ ] No build, compilation, or test operation is used as a fatal transaction gate.
- [ ] If an informational build/test was explicitly requested, failure cannot throw, abort, roll back source/project state, or reset transaction-owned Git history.
- [ ] Every modified public WinForms `*.Designer.cs` type part is explicitly declared `public partial class` when its corresponding logical type is public.

---

## 29. Change Transaction Script Delivery Requirements

Use a fresh unique lowercase 32-character hexadecimal GUID-style basename with `.ps1` for every iteration. Deliver the script as a downloadable file intended to be dot-sourced from PMC. Do not create branches/issues/PRs unless explicitly requested.

Freshness also applies **inside generated payloads**: every new transaction attempt must generate fresh values for every transaction-created identity GUID (including new-project `ProjectGuid` and `AssemblyInfo`/COM GUID values). Do not reuse identity GUIDs from an earlier failed/generated transaction merely because that attempt was rolled back. Externally defined fixed API/type/view GUID constants are not generated identities and must remain their documented values.

When the same request also asks for an updated Solution-level xyLOGIX Software Engineering Manifesto, Solution-level `CONTRIBUTING.md`, or Solution-level `README.md`, deliver those documents as **separate downloadable artifacts**. Do not embed them into the `.ps1`, do not make them DTE mutation targets, and do not make them transaction-owned Git work items. The maintainer installs/replaces those files manually.

### Required two-pass pre-delivery audit

Do not stop at reviewing the generator's in-memory representation. **Double-check the exact artifact that will be delivered.** The user should not discover basic PowerShell, payload, or transaction-shape defects by running the script.

#### Pass 1 - transaction/content audit

Audit the planned transaction against the current authoritative workspace and current instructions:

- every authorized target is intentional and repository-correct;
- the Solution-level SEM, Solution-level `CONTRIBUTING.md`, and Solution-level `README.md` are excluded from transaction-owned targets and, when requested, exist only as separate manual-download deliverables;
- every existing-file target is based on the newest authoritative maintainer-authored source available for that path; earlier AI payloads/tarballs are not used to roll back later maintainer edits;
- the generation-time diff from authoritative source to desired payload contains only transaction-owned changes and necessary direct consequences, with unrelated implementation/logging/comments/XML documentation preserved;
- exact full-file desired-state payloads are used wherever feasible after that preservation audit succeeds;
- source/project payloads are generation-time audited for correctness/style/documentation/reference requirements;
- every new transaction-generated identity GUID is freshly generated for the current attempt and checked against the authoritative source/payload set for accidental reuse;
- every changed C# payload is audited for symbol-to-namespace closure, including explicit `using PostSharp.Patterns.Diagnostics;` wherever `[Log]`, `[NotLogged]`, or another PostSharp diagnostic aspect is used without qualification;
- commit messages are scoped to their intended staged work items and obey repository rules;
- boundary/retry/no-op behavior is coherent;
- arbitrary preexisting working-tree/default-index dirt is tolerated in every transaction mode; no unrelated preservation commit, stash, reset, clean, or clean-baseline step is generated;
- `.Constants`/`.Interfaces` dependency exceptions are honored while actual interface inheritance/member-type dependency closure is still satisfied;
- the post-VCmd convergence barrier prevents Git capture until repeated content fingerprints of the exact VCmd-opened file set prove that downstream background `ReSharper_SilentCleanupCode`/IDE rewriting has remained quiet for the **adaptive current interval derived from the actual opened-file count** and through the final save/resample cycle;
- the Git design proves each commit by observing the `HEAD` transition/SHA from Git and contains a bounded post-capture/post-push fixed-point loop that cannot print success while transaction-owned dirt remains;
- if the requested artifact is Git-recovery-only, the design explicitly bypasses source payload mutation, new boundary creation, ReSharper suspension/resumption, VCmd, adaptive content-fingerprint waits, and initial pull/rebase while the authorized recovery work tree is dirty; it uses direct status recapture after `File.SaveAll`, capture, and push instead;
- the complete changed-path set and narrower VCmd-eligible C# set are distinguished correctly; VCmd eligibility includes `AssemblyInfo.cs`, excludes generated/fixed-format/non-C# artifacts, and editor-opening/VCmd cleanup remains best-effort and unable to erase source progress, while the single VCmd invocation is preceded by the exact noninteractive/Git-disabled one-run sidecar so no modal prompt or VCmd-owned Git workflow can occur;
- Git synchronization respects actual upstream state; and
- unrelated dirty/staged paths, regardless of when they appeared, cannot hitchhike or be reset/committed by the transaction;
- visible editor-tab behavior and VCmd scope isolation match the supplied VCmd source: only documents with `Document.Windows.Count > 0` are considered user-visible, open C# documents take scope precedence, and unrelated preexisting visible C# documents are temporarily isolated/restored;
- exact staging is rename/copy-aware and never depends on staged-record count equality; generation-time-known rename/move pairs are predeclared and staged directly without temporary Git rename-discovery staging;
- native zero-output paths are modeled explicitly so a clean Git status is an empty result rather than `$null`; and
- the detailed `%TEMP%` execution log and terminal raw-Git-status finalization requirements are designed into both success and failure flows;
- ReSharper state transitions are idempotent/nonfatal and preserve preexisting user state; and
- every new-project admission uses a bounded `.csproj` readiness barrier, explicit `Solution.AddFromFile($fullProjectPath, $false)`, and semantic pre/post DTE boundary logging.

#### Pass 2 - exact artifact/static audit

After writing the final GUID-named `.ps1` file, reopen **that exact file** and audit it again. At minimum:

1. verify the first PowerShell token/encoding is sane and no stray BOM character is embedded as source text;
2. parse it with a PowerShell 5.1-compatible parser when available (or the closest reliable parser/static check available) and require zero syntax errors before delivery;
3. verify there is no assignment/binding/shadowing/removal of `$dte` in any casing;
4. verify all exact-byte/Base64 payloads decode successfully and exactly match the generation-time audited desired bytes;
5. verify the authorized path set, payload map, commit-message map, and any per-path metadata agree with one another;
6. verify the Solution-level SEM, Solution-level `CONTRIBUTING.md`, and Solution-level `README.md` are absent from transaction-owned payload/staging/implementation-commit maps; if separate updated artifacts were requested, verify those exist independently of the `.ps1`;
7. verify all retries/timeouts are bounded;
8. verify Git stdout/stderr are drained safely and native output is not dumped directly into PMC;
9. verify no cross-repository staging or stale repository-relative state exists;
10. verify no runtime method-body/marker/regex/old-code/hash/source-shape discovery can unnecessarily kill an exact-payload transaction;
11. verify meaningful-mutation tracking and pre-mutation empty-boundary cleanup are present when a boundary is used;
12. verify successful no-op boundary cleanup is present when a boundary is used;
13. verify ReSharper is suspended before any mutation, the suspension state is tracked, and early-failure cleanup can resume it;
14. verify new projects/references/source memberships use DTE/project-system operations rather than hand-authored `.sln`/`ProjectReference` topology, and no junction-canonicalized absolute path is persisted;
15. verify the script maintains a transaction-wide eligible-path registry and performs exactly one paced source-file opening pass after all mutations; verify `AssemblyInfo.cs` is included and `Global*.cs`, `*.Designer.cs`, generated/derived C# source, and all non-C# artifacts are excluded;
16. verify the idempotent ReSharper state transition required for cleanup occurs after that opening pass and before VCmd, with bounded wait/message pumping after successful transitions and restoration of a preexisting suspended state afterward when applicable;
17. verify the single VCmd invocation is immediately preceded by a write to the canonical `.config.json` path with **exactly** schema `2`, `SuppressPrompts: true`, `EnableGitAwareness: false`, and `AutomaticallyCheckInChangesToGitWhenGitAwarenessIsSuppressed: false`;
18. verify the script skips VCmd rather than invoking it when sidecar preparation fails, and verify the VCmd call is argumentless (no `NoPrompt` or other command argument);
19. verify VCmd cannot perform Git synchronization/check-in/push and therefore cannot compete with the script's own custom commit-message/staging workflow;
20. verify eligible-file editor-open failure, sidecar-preparation failure, and VCmd failure are warning-only, excluded files are never opened merely for VCmd, and the final `File.SaveAll` is unconditional;
21. verify the artifact contains no `Assert-CleanIndex` definition/call/equivalent cleanliness gate and no automatic preservation commit/stash/reset/clean step for unrelated Git dirt; verify arbitrary working-tree/default-index dirt is tolerated;
22. verify the post-VCmd convergence barrier explicitly accounts for downstream background `ReSharper_SilentCleanupCode`, retains the exact successfully opened VCmd file set, repeatedly fingerprints those file contents, pumps the message loop, periodically saves, computes the adaptive quiet/max timings from the actual opened-file count, reports those timings, resets the full quiet interval on every detected rewrite, performs a final save/pump/resample after apparent convergence, and prevents Git capture on timeout;
23. verify `.Constants`/`.Interfaces` projects are exempt from blanket `xyLOGIX.Core.Debug`/`xyLOGIX.Core.Extensions*` additions while genuine interface dependency closure is still satisfied;
24. verify no post-VCmd semantic source verification or fatal lint/style/static-analysis/build/compile/test gate exists;
25. verify reference handling is positive-only unless the current prompt explicitly authorizes removal;
26. verify public WinForms `*.Designer.cs` payloads use the required explicit `public partial class` declaration when applicable;
27. verify top-level exception handling reports message/position/stack, performs safe transaction cleanup, preserves meaningful progress, and normally does not rethrow redundantly;
28. verify the script's final success/no-op/error paths all leave the repository and PMC session in a state the maintainer can understand from `Write-Host` output;
29. verify the artifact's payload manifest corresponds to the generation-time desired files that were produced from the newest authoritative maintainer-authored baselines, not from an older AI-generated snapshot; and
30. verify the PowerShell source contains no nested `try`/`catch`/`finally` blocks and that any operation requiring its own exception boundary was extracted into a named helper function; also verify generated C# payloads follow the current xyLOGIX Software Engineering Manifesto rule against nested exception blocks.
31. verify every changed C# payload passed the current symbol-to-namespace closure audit, including `PostSharp.Patterns.Diagnostics` for `[Log]`/`[NotLogged]`/related aspects, `System.Diagnostics` for `[DebuggerStepThrough]`, and `xyLOGIX.Core.Debug` for `DebugUtils` when those symbols are unqualified;
32. verify each Git commit path captures the pre-commit `HEAD`, requires a different post-commit `HEAD`, resolves/reports the actual abbreviated SHA and subject from Git, and never treats an internal counter as commit proof;
33. for a source-mutating artifact, verify the script contains the bounded adaptive post-capture fixed-point loop that waits for late IDE writes to settle and re-enters ordered Git capture for transaction-owned dirt without rerunning VCmd; for a Git-recovery-only artifact, verify the direct-status exception is used instead;
34. for a source-mutating artifact, verify the post-push finalization loop performs adaptive `File.SaveAll`/message-pump/status stabilization and bounded recapture/re-push; for a Git-recovery-only artifact, verify post-push finalization uses direct `File.SaveAll` + fresh status and bounded direct recapture without artificial quiet waits; and
35. verify the final success path obtains fresh Git evidence that all transaction-owned paths are fully captured/stable and final `HEAD` resolves; require `0/0` local/upstream ahead-behind only when synchronization actually occurred, and never require the whole default index/work tree to be clean.
36. if the artifact is Git-recovery-only, verify it performs no source/project/Solution payload mutation, no new empty boundary, no ReSharper suspend/resume, no VCmd opening/sidecar/invocation, and no adaptive content-fingerprint/quiet-period wait merely because the recovery paths are dirty.
37. if the artifact is Git-recovery-only, verify authorized recovery paths are captured directly with transaction-private/path-isolated Git state, initial pull/rebase is not a cleanliness prerequisite, and unrelated dirt/staging remains untouched.
38. if the artifact is Git-recovery-only, verify post-capture and post-push fixed-point checks are direct `File.SaveAll` + fresh Git-status observations with bounded recapture rounds, while the final Git proof requirements remain fully intact.
39. verify every native-process result normalizes absent stdout/stderr to non-null strings and every status parser returns an empty collection for successful zero-output Git status; no `[string]::Join`, `.Trim()`, `.Split()`, or equivalent operation can receive `$null` on that path.
40. verify the exact artifact contains no PowerShell 5.1-invalid trailing comma in array/argument/parameter constructs and no ambiguous expandable-string `$Name:` form; require `${Name}:` or an equivalent unambiguous spelling.
41. verify the transaction initializes a `%TEMP%` log named from the actual GUID script basename plus execution-start `yyyyMMdd_HHmmss_fff` timestamp before the first mutation.
42. verify detailed logging records function entry/exit, inputs/outcomes, important variables/collections/counts, DTE/Git/VCmd operations, warnings/errors/exceptions, while omitting giant Base64/source payload bodies and redacting genuine secrets.
43. verify both success and failure flows perform repository/editor-affecting cleanup/restoration before appending complete raw `git status` output for every affected repository, and verify no substantive file-log content is written after the final status block.
44. verify `ProjectItem.Open(...)` calls use the explicit text/source view kind, retain the returned `EnvDTE.Window`, make it visible, activate it, pump/wait/pump, and count/report success only after the intended document owns at least one window; `ItemOperations.OpenFile(...)` is fallback-only.
45. verify preexisting user-visible C# documents are snapshotted and unrelated ones are temporarily isolated before VCmd; if exact isolation cannot be established, the artifact skips VCmd and later restores the user's editor state instead of processing unrelated source.
46. verify staged-scope validation is rename/copy-aware, allows one staged rename/copy record to cover old/new authorized paths, treats empty actual staged diff as a no-op, and contains **no fatal expected-count-versus-actual-count assertion**.
47. verify every VCmd invocation is immediately preceded by rewriting the exact automation sidecar, and the artifact assumes the command resets that file to defaults; verify post-VCmd observation is pathname-based so a VCmd-closed `AssemblyInfo.cs` remains in the convergence set.
48. verify ReSharper suspend/resume handling is idempotent/nonfatal, distinguishes preexisting state from transaction-owned state changes, and never aborts merely because `ReSharper_Suspend`/`ReSharper_Resume` is unavailable when the requested state may already be active.
49. verify every newly generated identity GUID in the exact payload set is fresh for the current attempt, including new-project `ProjectGuid` and `AssemblyInfo`/COM GUID values; verify fixed external API GUID constants were not randomized.
50. verify each generated new-project `.csproj` admission uses the actual full filesystem path, a finite readiness/readability/stability check, explicit `Solution.AddFromFile($fullProjectPath, $false)`, captured returned `EnvDTE.Project`, and before/after semantic DTE boundary logging with the exact project and Solution paths.
51. verify generation-time-known rename/move work items are staged directly from their predeclared authorized old/new pathsets and that no temporary speculative rename-detection staging cycle exists in the normal source-mutating flow.
52. verify every potentially blocking DTE/COM operation writes a semantic pre-call log record before entry and an immediate outcome record after return, so an externally forced Visual Studio termination leaves an unambiguous last operation even though `catch`/`finally` cannot run.
53. verify the exact artifact contains **no** `Assert-CleanIndex` function/cmdlet, invocation, alias, string-based equivalent, or other fatal default-index-cleanliness gate.
54. verify transaction-owned work-item commits use a transaction-private alternate index (or another explicitly audited path-isolated mechanism) seeded from the current `HEAD`, stage only authorized paths, and inspect the actual private staged diff before commit.
55. verify arbitrary unrelated working-tree/default-index dirt can remain present from script start through completion without being auto-committed, stashed, reset, restored, cleaned, or used to abort source mutation.
56. verify boundary creation/removal and no-op cleanup preserve arbitrary default-index/work-tree state and do not depend on repository cleanliness.
57. verify remote synchronization is opportunistic: dirty local state may defer pull/rebase/reconciliation, but cannot invalidate proven local transaction completion; safe push/fetch operations are not suppressed merely because unrelated dirt exists.

Only after both passes succeed should the artifact be delivered.

---

## 30. Final Standard

> A Change Transaction Script is a **clobbering, progress-first, in-IDE imposition transaction**. It overlays its generation-time-authorized desired state on top of whatever working-tree and default-index state already exists. It does **not care whether the repository is dirty or clean** as a permission condition, does not manufacture a clean baseline, does not auto-commit/stash/reset/restore/clean unrelated local work, and never generates or calls `Assert-CleanIndex` or an equivalent cleanliness gate.
>
> Git isolation is by **transaction ownership**, not repository cleanliness. Prefer a transaction-private alternate index seeded from the current `HEAD`; stage only the generation-time-authorized work-item paths into that index, inspect the actual rename/copy-aware private staged diff, generate the message from that diff, commit through the same private index, and prove the resulting `HEAD` transition/SHA/subject. Unrelated entries in the maintainer's real index may remain staged throughout. Normalize only transaction-owned real-index entries when needed after a commit; never touch unrelated paths.
>
> Generation-time-known renames/moves are predeclared atomic work items, not facts to rediscover by temporary staging. Git may present a known move as `R`/`C` or `D+A`; staged-record count is never a correctness gate. The only staged-scope defect is an unauthorized path inside the transaction-private staged diff.
>
> Use the host-provided `$dte`; never assign or bind to it. Let Visual Studio own Solution/project/reference topology. New project creation uses fresh generated GUID identities on every attempt, a bounded `.csproj` readiness barrier, explicit `Solution.AddFromFile($fullProjectPath, $false)`, captured returned `EnvDTE.Project`, and semantic before/after DTE call-boundary logging.
>
> ReSharper state transitions are idempotent and nonfatal. Command unavailability may simply mean the desired state is already active. Record only state changes positively attributable to the transaction and never blindly invert the maintainer's preexisting state.
>
> Source-mutating transactions perform one final VCmd preparation pass. Snapshot and preserve the user's visible editor state, isolate unrelated visible C# tabs, open each intended C# target through a genuine visible/activated source window, rewrite the exact noninteractive/Git-disabled VCmd sidecar immediately before the one argumentless invocation, and wait for bounded adaptive content convergence of the exact opened pathname set before Git capture.
>
> Treat Windows PowerShell 5.1 as the real runtime. Normalize successful zero-output native results, avoid 5.1 parser/binder traps, log consequential DTE boundaries before entering them, and write a detailed `%TEMP%` transaction log whose complete raw final `git status` blocks are the last substantive file-log content whenever the host survives long enough to finalize.
>
> Remote synchronization is **opportunistic**. A dirty work tree/default index may make pull/rebase reconciliation inappropriate, but that does not invalidate local transaction progress. Fetch/push when mechanically safe; if upstream reconciliation would require disturbing unrelated local state, preserve the transaction commits and report proven local completion rather than manufacturing cleanliness.
>
> Git-recovery-only transactions remain intentionally short: `File.SaveAll`, identify authorized recovery paths, capture them with transaction-private/path-isolated Git state, use direct fresh-status recapture, and prove the resulting commits. They do not mutate source, create new boundaries, change ReSharper state, invoke VCmd, or run artificial content-stability waits merely because the repository is dirty.
>
> Across all modes, preserve maintainer-authored source, use fresh GUID-named downloadable artifacts, never nest exception blocks, leave unrelated local Git state alone, and print `*** SUCCESS ***` only after the transaction-owned end-state proof is observable.