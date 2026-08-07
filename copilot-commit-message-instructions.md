# Copilot commit-message instructions

These instructions define the required commit-message format for this repository.
They are intended for GitHub Copilot, ChatGPT, and other AI assistants that generate commit messages for the maintainer.

## Required format

A commit message must have this structure:

```text
<topline>

- <body bullet>
- <body bullet>
- <body bullet>
```

The first line is the commit topline.
The second line is blank.
The third line onward is the commit body, written as an outline of bullet points.\n\nThe commit body must contain no fewer than three top-level bullet points. Three top-level bullets is the minimum floor, not a default or a maximum. More than three top-level bullets are always allowed, and should be used whenever the diff contains additional distinct, meaningful changes that deserve separate mention.\n\nIndented sub-bullets may be used beneath a top-level bullet when they make the commit message easier to scan. Sub-bullets do not count toward the minimum requirement of three top-level body bullets.\n\nDo not add bullets merely to reach a certain count. Every top-level bullet and sub-bullet must remain scoped to the diff and describe a meaningful part of the committed change.

## Topline rules

1. The topline must be a single line.
2. The topline must be no more than 72 characters.
3. The topline must start with a verb.
4. The topline verb must be in the present tense.
5. The topline must use sentence case.
6. The topline must not contain backticks.
7. The topline must describe the change directly and specifically.
8. If multiple file(s) are changed, then the topline can be more descriptive of the changes more broadly across the whole difference set.  If only one file is changed, then the topline must follow the single-file rules.

## Single-file commit toplines

Use these exact patterns when the commit affects only one file.

### Single-file addition

If the only change in the commit is the addition of one file, then the topline must be:

```text
Create <file name>
```

Example:

```text
Create CONTRIBUTING.md
```

### Single-file modification

If the only change in the commit is the modification of one existing file, then the topline must be:

```text
Update <file name>
```

Example:

```text
Update xyLOGIX.Net.Framework.Models.sln
```

## Body rules

1. The body must begin on the third line.
2. The body must be written in outline format using bullet points.
3. The body must contain at least three top-level bullet points.
4. More than three top-level bullet points are allowed whenever the diff contains additional distinct, meaningful changes.
5. Sub-bullets do not count toward the minimum of three top-level bullets.
6. Every body bullet must be written in the past tense.
7. Each top-level bullet should describe one meaningful change or one cohesive category of related changes.
8. The body should explain what changed, not merely repeat the topline.
9. The body should stay scoped to the diff being committed.
10. Do not mention unrelated cleanup, future work, or speculative intent.
11. Do not pad the body with redundant bullets merely to increase the bullet count.
12. When a bullet introduces a list of distinct items, place each listed item on its own indented sub-bullet instead of embedding the entire list inline.

## Lists inside body bullets

Use sub-bullets when a body bullet introduces multiple distinct items.

For example, do not write:

```text
- Added reference(s) to the project(s) `Foo`, `Bar`, `Baz`, and `Qux`.
```

Write:

```text
- Added reference(s) to the project(s):
    - `Foo`
    - `Bar`
    - `Baz`
    - `Qux`
```

Apply this rule only when the bullet is actually presenting a list of separate items.

Examples of item lists that should normally use sub-bullets include:

- Multiple project references.
- Multiple file names.
- Multiple source files that were added, removed, renamed, or moved.
- Multiple interfaces, classes, methods, properties, fields, constants, or enum values.
- Multiple package dependencies.
- Multiple configuration values.
- Multiple Solution items.
- Multiple distinct tests or test cases when they are being enumerated as a list.

Do not force sub-bullets for ordinary prose that happens to contain a short compound phrase. Use judgment: the purpose of sub-bullets is to make real lists easier to scan, not to fragment normal sentences unnecessarily.

When a list is expressed with sub-bullets, the parent bullet should describe the overall change and end with a colon. Each sub-bullet should contain one list entry. Sub-bullets should remain concise and should not restate the entire parent bullet.

## Backtick rules for body text

In the body only, wrap technical identifiers in backticks.

Use backticks for:

- File names.
- Path names.
- Code entities.
- Method names.
- Class names.
- Interface names.
- Property names.
- Field names.
- Constants.
- Enum values.
- Command-line switches.
- XML nodes, attributes, and values.
- Data values.
- Database fields.\n- Project names when they refer to technical project identifiers.\n- Package names when they refer to technical dependency identifiers.

Do not use backticks in the topline.

## Tense examples

Correct body bullets use past tense:

```text
- Added `CONTRIBUTING.md` for repository-specific contribution guidance.
- Updated `xyLOGIX.Net.Framework.Models.sln` for Visual Studio 2022.
- Removed stale `UserQuery.ICommandLineInfo` XML documentation references.
```

Incorrect body bullets use present tense or future tense:

```text
- Add `CONTRIBUTING.md` for repository-specific contribution guidance.
- Updates `xyLOGIX.Net.Framework.Models.sln` for Visual Studio 2022.
- Will remove stale `UserQuery.ICommandLineInfo` XML documentation references.
```

## Complete examples

### Single-file addition

```text
Create CONTRIBUTING.md

- Added `CONTRIBUTING.md` for `xyLOGIX.Net.Framework.Models`.
- Documented repository scope, public API, target stack, and dependency boundaries.
- Captured XML documentation, README, project-file, dependency, and verification conventions.
```

### Single-file modification

```text
Update xyLOGIX.Net.Framework.Models.sln

- Updated `xyLOGIX.Net.Framework.Models.sln` for Visual Studio 2022.
- Changed the solution header from `Visual Studio Version 16` to `Visual Studio Version 17`.
- Added the following files to `Solution Items`:
    - `copilot-code-instructions.txt`
    - `CONTRIBUTING.md`
    - `copilot-commit-message-instructions.md`
```

### Multi-file change

```text
Extract Shell COM interfaces

- Moved Shell COM interface declarations out of `Prompt.cs`.
- Added separate source files for the extracted interfaces:
    - `IFileDialog.cs`
    - `IFileOpenDialog.cs`
    - `IShellItem.cs`
- Included the new interop files in `xyLOGIX.Net.Framework.Models.csproj`.
- Removed nested COM interface declarations from `Prompt`.
```

### Multiple project references

```text
Add process validator dependencies

- Added project references for process-name validation:
    - `xyLOGIX.Validators.Names.Processes.Factories`
    - `xyLOGIX.Validators.Names.Processes.Interfaces`
- Added project references for thread-identifier validation:
    - `xyLOGIX.Validators.TIDs.Factories`
    - `xyLOGIX.Validators.TIDs.Interfaces`
- Extended the project dependency graph to support the new validation paths.
```

## Final validation checklist

Before returning a generated commit message, verify all of the following:

1. The topline is one line and no more than 72 characters.
2. The topline starts with a present-tense verb.
3. The topline uses sentence case and contains no backticks.
4. Single-file additions use exactly `Create <file name>`.
5. Single-file modifications use exactly `Update <file name>`.
6. The second line is blank.
7. The body begins on the third line.
8. The body contains at least three top-level bullets.
9. Every top-level body bullet is written in the past tense.
10. Every body bullet remains scoped to the diff.
11. Technical identifiers in the body are wrapped in backticks.
12. Real lists of distinct items are expanded into indented sub-bullets.
13. Sub-bullets are not counted toward the minimum three top-level bullets.
14. Additional meaningful changes receive additional top-level bullets instead of being omitted merely to keep the body short.
15. No bullet was added solely as padding.
