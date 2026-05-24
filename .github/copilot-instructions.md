# Repository instructions for GitHub Copilot

These instructions apply to all Copilot Chat responses, agent-mode changes, code snippets, issue text, pull request text, commit-message assistance, documentation, and generated source files in this repository.

> NOTE: This file is intentionally ASCII-only and uses valid Markdown.

## 0) Priority, scope, and response behavior

1. Priority order is: user prompt > existing repository conventions > these instructions.
2. Use U.S. English for all non-code output.
3. Do not patronize the user or over-explain basics. The user is an experienced C# developer.
4. Produce the smallest complete answer or code change that directly satisfies the prompt. Do not emit skeleton-only code.
5. Work incrementally for long work. Label steps clearly, complete the current step, and stop before beginning the next requested chunk.
6. Do not introduce unrelated tasks while a task is in progress.
7. Never use Comprehensive Commit format for GitHub issue or pull request titles.
8. In issue and pull request titles and bodies, wrap code entities, paths, and file names in backticks liberally.
9. Before changing code, inspect the relevant source files, adjacent partial-class files, project files, documentation, and dependency graph.
10. If a source file is `partial`, inspect nearby files in the same folder that contain the same partial class before changing it.
11. Preserve existing file headers and above-namespace documentation when generating a drop-in source file.
12. Check `CONTRIBUTING.md` for required source-file headers before generating new source files.
13. If a file-read tool fails, retry. If it still fails, do not assume the target file does not exist.
14. Assume required `.csproj` files already exist unless the prompt says otherwise. Do not teach the user how to add files to projects.
15. Generate code in reference order: declarations that do not depend on other generated declarations first, then the code that depends on them.
16. For classes with backing fields, present fields before properties.
17. All generated code must match the surrounding repository style, layout, naming, XML documentation, comments, logging, and language features.
18. For a drop-in replacement of an existing file, keep unchanged code faithful to the original file.
19. Do not regenerate `GlobalAspects.cs` or `AssemblyInfo.cs`.
20. When acting as an agent, remove dead code only after verifying it is truly dead. Public interface members are client-facing even if unused inside the solution.
21. Be date-aware when producing docs, copyright text, or version-related statements.

## 1) User context

The user is Dr. Brian Hart, a U.S.-based C# developer who has programmed since 1994 and has used C# since 2000. He has a Ph.D. in Astrophysics from UC Irvine and served as a U.S. Navy Cyber Warfare Engineer. Write for a professional peer in documentation, READMEs, issues, pull requests, merge commits, and commit messages.

## 2) Target stack and tooling

- Language: C# 7.3.
- Runtime: .NET Framework 4.8.
- UI: Windows Forms 2.0.
- IDE: Visual Studio 2022 Enterprise 17.14.23 or newer unless a repository-specific file states otherwise.
- Installed tooling: CodeMaid, JetBrains ReSharper Ultimate, and GitHub Copilot.
- Test framework: NUnit 4.3.2.
- Documentation generator: Vsxmd 1.4.5.
- File system library: AlphaFS 2.2.6.
- JSON library: Newtonsoft.Json 13.0.3.
- Logging: log4net 3.0.3.
- Aspect/logging framework: PostSharp 2024.1.6.
- NuGet style: prefer `packages.config`; use `PackageReference` only when a project type requires it, such as VSIX projects.
- Project format: prefer legacy MSBuild `.csproj` XML; do not convert to SDK-style projects.
- Solution format: prefer legacy `.sln`; do not introduce `.slnx` unless explicitly requested.

Do not use features newer than C# 7.3, .NET Framework 4.8, or Windows Forms 2.0. Disallowed examples include nullable reference types, records, init-only setters, top-level statements, file-scoped namespaces, global using directives, switch expressions, indices/ranges, `using var`, and default interface members.

## 3) Application families and UI intent

The user commonly builds:

- Legacy Windows Forms 2.0 desktop utilities that augment the local development environment.
- Visual Studio Template Wizard DLLs implementing `Microsoft.VisualStudio.TemplateWizard.IWizard`.
- Standalone tools that automate or scaffold software systems.
- Classic AI, heuristic, or background-service style applications.
- LINQPad scripts that automate maintenance work across large solution sets.
- Integrated desktop applications that mimic Visual Studio, often using WeifenLuo DockPanelSuite.
- Occasional ASP.NET MVC or Blazor applications.

Favor maintainability, documentation, loose coupling, and future re-entry after long pauses. README files, XML documentation, and generated documentation are first-class deliverables.

## 4) Repository, solution, and module organization

1. A module is a group of related C# class libraries.
2. The namespace must match the `.csproj` project name exactly.
3. Do not use Visual Studio solution folders.
4. Projects normally live directly under the folder containing the `.sln` file, unless they are intentionally referenced from another solution.
5. The `.git` folder is normally at the solution root. Git workflows should receive the solution-containing folder as `repoRoot`.
6. Avoid circular dependencies, including transitive project-reference cycles and XML documentation `cref` cycles.
7. Before suggesting a project reference, verify that it is not already present and that adding it will not create a cycle.
8. If a reference would be dodgy or circular, stop and propose a new intermediary module instead.
9. When uncertain about references or NuGet packages, do not modify them. Let the user handle them with Visual Studio and ReSharper.

### 4.1) Standard module suffixes

Use only the suffixes that the design needs:

| Project | Responsibility |
|---|---|
| `MyModule` | Concrete classes and abstract base classes. |
| `MyModule.Actions` | Public static verb-named action classes with fluent method names and functional inputs/outputs. |
| `MyModule.Common` | Public static shared functionality used by other libraries in the same module. |
| `MyModule.Constants` | Public constants and enums only. |
| `MyModule.Displayers` | Public static display entry points, usually `Display`, for secondary windows and dialogs. |
| `MyModule.Events` | Event delegate declarations and matching `EventArgs`-derived classes. |
| `MyModule.Extensions` | Public static extension classes named `<Type>Extensions`; extension methods only. |
| `MyModule.Factories` | Public static factories and singleton accessors. |
| `MyModule.Helpers` | Helper and utility classes shared across the module or exposed to clients. |
| `MyModule.Interfaces` | Public interfaces exposed by the module. |
| `MyModule.Tests` | NUnit test fixtures. |
| `MyModule.Win32` | P/Invoke signatures, `NativeMethods`, structs, and Win32 helpers. |

Stacked suffixes represent support relationships, such as `MyModule.Tests.Actions.Constants` supporting `MyModule.Tests.Actions`.

### 4.2) Action, common, displayer, and factory naming

- Action classes are public static, verb-named, one or two words, PascalCase.
- Method names complete the phrase, such as `Format.FileAsImage(...)`.
- `.Actions` libraries are module front doors.
- `.Common` libraries hold shared module internals that remain public.
- `.Displayers` libraries may be preferred over `.Actions` for sophisticated secondary UI display behavior.
- Factories use fluent names such as `GetXxx.SoleInstance()`, `MakeNewXxx.FromScratch()`, `MakeNewXxx.ForAbc(...)`, `GetStrategy.OfType(...)`, or `MakeNewStrategy.OfType(...)`.
- Constructor-argument factory methods should validate or throw consistently with the constructor.
- Strategy-selector factories inside a `.Factories` project must not directly call a root concrete class's `.Instance` property. Add a dedicated `GetConcreteClass.SoleInstance()` wrapper and call that wrapper.
- Outside a module's own `.Factories` project, prefer that module's factories rather than directly referencing the root concrete project.

### 4.3) Constants and enums

- Constants classes contain only `public const` members.
- Name constants classes by the nominal category of information exposed.
- Enums belong in `.Constants` libraries.
- Enums and enum members must be public and XML documented.
- Enum members must be alphabetized.
- Do not assign explicit enum values except `Unknown = -1`.
- `Unknown` is always the final enum member.
- Do not apply `[Log(AttributeExclude = true)]` to an enum.

### 4.4) Strategy pattern

When implementing Strategy:

1. Define an interface exposing all common events, properties, and methods.
2. Define an enum in `.Constants` listing strategies.
3. Define an abstract base class that implements the interface abstractly and uses Template Method for shared behavior.
4. Use `OnXxx` for the protected template method names, not `DoXxx`.
5. Define one concrete strategy class per enum member.
6. Expose an enum-typed strategy property through the interface, abstract base, and concrete classes.
7. Initialize the concrete strategy property to its matching enum value.
8. Copy XML documentation down the object tree for implemented or overridden events, methods, and properties.
9. Create per-class factory accessors before the higher-level strategy factory.
10. For brand-new strategy patterns, generate in this order: enum, interface, abstract base class, concrete classes one at a time, per-class factories, strategy factory.

Static, protected, and public constructors on strategy infrastructure should be decorated with `[Log(AttributeExclude = true)]` when they exist only to suppress constructor logging.

### 4.5) Strategy-of-Strategies modules

When a module's sole responsibility is to manufacture objects of a particular domain and host strategy-factory machinery, `.Factories` may appear as an infix in the module name.

Example: `PC.Generators.Paths.Factories.Documentation`.

Use this for nested strategy families where a primary strategy selects a secondary strategy. A typical `DocumentationFile` path generator design uses:

| Project | Contents |
|---|---|
| `PC.Generators.Paths.Documentation.Constants` | Platform enum. |
| `PC.Generators.Paths.Documentation.Interfaces` | Leaf generator interface. |
| `PC.Generators.Paths.Documentation` | Platform generator base and concrete leaf generators. |
| `PC.Generators.Paths.Factories.Documentation.Constants` | Configuration enum. |
| `PC.Generators.Paths.Factories.Documentation.Interfaces` | Configuration factory interface. |
| `PC.Generators.Paths.Factories.Documentation` | Configuration factory base and concrete configuration factories. |
| `PC.Generators.Paths.Factories.Documentation.Factories` | Singleton factory selectors and fluent top-level entry points. |

For Visual Studio `DocumentationFile` values, `AnyCPU` uses `bin\{Configuration}\{ProjectName}.xml`, while every other platform uses `bin\{Platform}\{Configuration}\{ProjectName}.xml`.

## 5) Design principles and code structure

1. Follow SOLID, DRY, Clean Code, loose coupling, encapsulation, and implementation hiding.
2. Code to the highest-level interface or abstract base class that still exposes the needed behavior. Do not use `System.Object` unless the domain requires it, such as COM programming.
3. Prefer built-in .NET Framework APIs when they satisfy the requirement without brittleness.
4. Consult official Microsoft documentation when framework behavior, API availability, or syntax is uncertain.
5. Prefer flexible software over brittle coupling when requirements may evolve.
6. Avoid magic literals. Prefer `.Constants` libraries, enums, or `Resources.resx` only when appropriate.
7. Do not put UI control text or control values in `Resources.resx`.
8. Never use local functions.
9. Never use `#region` or `#endregion`.
10. Never nest `try`/`catch`, `try`/`finally`, `try`/`catch`/`finally`, `if` checks, or locks. Extract helper methods instead.
11. Avoid God Methods. If a large workflow is unavoidable, make it a flat pipeline of single-responsibility helper methods.
12. Use the `TryXxx(..., out var result)` pattern when a method needs to report success and return another value.
13. Use `var`, `out var`, `ref var`, and C# 7.3 pattern matching aggressively when they improve readability.
14. Prefer negated early gates: `if (!condition) return result;` or `if (!condition) continue;`.
15. Do not use `&&` or `||` in eager-returning validation gates. Put each validation in its own `if`.
16. Minimize cyclomatic complexity.
17. Never use `++` or `--`; use `Interlocked.Increment` and `Interlocked.Decrement`.
18. Do not use `GetType().Name` in logging messages when the type name can be inlined in the string literal.
19. Avoid unnecessary `ToList()` and `ToArray()` materialization.
20. Avoid iterating an `IEnumerable<T>` more than once unless it is intentionally materialized.
21. In multithreaded code, avoid LINQ extension methods other than `.AsXxx()` methods; use explicit loops and thread-safe algorithms.
22. Snapshot changing sequences with `ToArray()` before iterating unless the collection is known to be concurrent.
23. Do not call `.AsParallel()` and `.AsSequential()` in the same sequence.

## 6) Defensive programming and validation

Use a shift-left, fault-tolerant style. Do not assume values, indexes, properties, files, folders, return values, or external calls are valid simply because they normally should be.

1. Validate inputs eagerly.
2. Bounds-check indexes and length/size values.
3. Gate against physically or logically impossible values, such as non-positive lengths used for area calculations.
4. Validate called method return values before using them.
5. Check file and folder existence before using or searching paths.
6. After critical file operations, verify the expected result.
7. Prefer validator objects and silent validators when the codebase provides them.
8. For pathnames, use the relevant pathname validator's silent method rather than first checking `string.IsNullOrWhiteSpace`.
9. `Does.FileExist` and `Does.FolderExist` from `xyLOGIX.Core.Files` already handle null or whitespace paths; do not duplicate that check immediately before calling them.
10. Validate complex context objects before asynchronous or pipeline work.
11. Validate parameters before async work begins.
12. Log before checks, log failures, log successes, and log `result` before every early return when surrounding code uses detailed logging.

When logging a validation gate, use this structure:

```csharp
DebugUtils.WriteLine(
    DebugLevel.Info,
    "ClassName.MethodName: *** INFO *** Checking whether the method parameter, 'data', has a null reference for a value..."
);

// Check whether the method parameter, 'data', is set to a null reference.
// If this is the case, then write an error message to the log file, and
// then terminate the execution of this method, returning the default return value.
if (data == null)
{
    // The method parameter, 'data', is set to a null reference. This is not desirable.
    DebugUtils.WriteLine(
        DebugLevel.Error,
        "ClassName.MethodName: *** ERROR *** A null reference was passed for the method parameter, 'data'. Stopping..."
    );

    DebugUtils.WriteLine(
        DebugLevel.Debug,
        $"*** ClassName.MethodName: Result = {result}"
    );

    // stop.
    return result;
}

DebugUtils.WriteLine(
    DebugLevel.Info,
    "ClassName.MethodName: *** SUCCESS *** We have been passed a valid object reference for the method parameter, 'data'. Proceeding..."
);
```

Inside an error branch, the explanatory comment immediately above the first error log line must end with `This is not desirable.`.

## 7) Exception handling and return-value pattern

Prefer methods that return default/failure values over methods that throw, while still allowing framework or third-party exceptions to be caught and logged.

### 7.1) Required method pattern

- Wrap method bodies in `try`/`catch` when applicable.
- Non-void methods declare a `result` variable near the top.
- Initialize `result` to the semantic failure/default value unless there are no gates and assignment is guaranteed before return.
- Logic gates return `result`, never literal `true`, `false`, or `null`.
- In `catch`, log the exception and reset `result` to the default failure value.
- Put `using xyLOGIX.Core.Debug;` at the top of files that use `DebugUtils`.
- Put this exact comment immediately before every `DebugUtils.LogException(ex);` call:

```csharp
// dump all the exception info to the log
DebugUtils.LogException(ex);
```

Do not explicitly pass the second parameter to `DebugUtils.LogException`.

### 7.2) Default return values

- `bool`: `false` means failure unless the method explicitly documents different semantics.
- `int`: `-1` means failure or unable to compute; `0` means zero count.
- `string`: `string.Empty`, not `null` and not `default`.
- Reference/interface/class/delegate/collection returns: `default` unless an empty collection is the documented failure value.
- Return collections through collection interfaces such as `IList<T>`, `ICollection<T>`, or `IDictionary<TKey,TValue>`.

### 7.3) Success comment

Before setting `result = true;` because the operation reached the success point, use this multi-line C-style block comment:

```csharp
/*
 * If we made it this far with no Exception(s) getting caught, then
 * assume that the operation(s) succeeded.
 */

result = true;
```

Do not collapse this into a single-line block comment.

If a method's success path temporarily needs a `true` default, set `result = true;` before the gate, return `result` from the gate, and then restore `result = false;` once that temporary success default no longer applies. If the default applies to the end, do not redundantly set it back.

### 7.4) Void and async methods

- `void` methods still validate inputs and use `try`/`catch`, but do not invent a meaningless `result` variable.
- A `void` method must not end with `return;`.
- Async methods must return `Task` or `Task<T>`, never `async void` except event handlers.
- Async methods use the same `result` pattern and validate synchronous failure paths before async work.
- When returning synchronously from an async `Task<T>` method, use `return await Task.FromResult(result);` when that matches existing repository style.
- Validate the results of awaited calls.
- Use `ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync()` before Visual Studio SDK or DTE operations that require the UI thread.
- Do not nest async `try`/`catch`; extract helper methods.

### 7.5) Retry loops

- Use `do`/`while` for retry loops, not `while`.
- Increment retry counters with `Interlocked.Increment`.
- Put a single-attempt operation in its own helper, usually named `TryXxxSingleAttempt`.
- Retry loops should log attempts, failures, exhaustion, and delays.
- If retrying file operations, verify each attempt's expected file-system result.

## 8) PostSharp logging and `[NotLogged]`

### 8.1) Primitive definition

For these logging rules, a primitive means a type that would have been primitive in C++:

- `bool`, `char`.
- `byte`, `sbyte`.
- `short`, `ushort`.
- `int`, `uint`.
- `long`, `ulong`.
- `float`, `double`.
- Enums.

Complex/non-primitive types include `string`, `object`, `decimal`, nullable value types, all structs such as `Guid`, `DateTime`, and `Rectangle`, all classes, all interfaces, delegates, and collections.

### 8.2) Parameters and return values

- Every method parameter whose type is complex/non-primitive must be decorated with `[NotLogged]`.
- `string` parameters are always `[NotLogged]`.
- `object` parameters are always `[NotLogged]`.
- Struct parameters such as `Guid`, `DateTime`, and `Rectangle` are `[NotLogged]`.
- Methods returning complex/non-primitive types must be decorated with `[return: NotLogged]`.
- Methods returning `string` or `object` must be decorated with `[return: NotLogged]`.
- Methods returning enums must not be decorated with `[return: NotLogged]`.
- Do not apply `[return: NotLogged]` to properties. `GlobalAspects.cs` already excludes property getters/setters and event add/remove methods.
- Event-handler parameters such as `object sender` and `EventArgs e` must be individually `[NotLogged]`.

### 8.3) Constructors and static classes

- Static classes should declare a static constructor decorated with `[Log(AttributeExclude = true)]` to suppress `.cctor` logging.
- Constants classes, action classes, common classes, extension classes, displayer classes, and factory classes follow this rule.
- Abstract base classes should decorate static and protected constructors with `[Log(AttributeExclude = true)]` when the constructors exist for normal initialization and logging suppression.
- Concrete strategy classes should decorate static and public constructors with `[Log(AttributeExclude = true)]`.
- If a static constructor performs meaningful work that should be logged, do not suppress it solely for convention.

## 9) XML documentation

### 9.1) Coverage and preservation

- Every class, struct, interface, enum, enum member, delegate, method, event, field, property, constant, parameter, and meaningful return value must have XML documentation regardless of access modifier.
- Do not change existing XML documentation unless explicitly asked. Add only missing documentation.
- For generated snippets, prefer each XML documentation element for a code entity on a single line when practical; ReSharper may reformat later.
- Method documentation must include `<remarks>` describing caller-relevant behavior, alternate code paths, and invalid-input behavior.
- Documentation for interface implementations, overrides, and corresponding interface members must be reusable; do not mention private helper methods in such docs.

### 9.2) Required wording

- Interface summaries must begin: `Defines the publicly-exposed events, methods and properties of ...`.
- Every `<param>` body starts with `(Required.)` or `(Optional.)`.
- Use `If ..., then ...` phrasing in XML documentation.
- Adjacent sentences are separated with `<para />`, not whitespace.
- Use `<see langword="null" />`, `<see langword="true" />`, and `<see langword="false" />` when specifically referencing C# keywords.
- Use `<paramref name="..." />` only when specifically referring to the method parameter.
- Use parenthetical plurals carefully in docs, comments, log messages, UI text, and message boxes: `attribute(s)`, `box(es)`, `GUID(s)`, `journey(s)`. Do not apply them where grammar does not call for them.
- When documenting value types or primitive keyword types, do not say `Reference to`. Prefer `A <see cref="T:System.Guid" /> value` or `An <see cref="T:System.Int32" /> value`.
- When documenting reference types, prefer `Reference to an instance of <see cref="T:Namespace.Type" />` or `reference to an instance of an object that implements the <see cref="T:Namespace.IType" /> interface`.
- Do not write `an IInterface instance`; interfaces are implemented by object instances.

### 9.3) `cref` and inline code

Use fully-qualified XML documentation references whenever semantically valid:

| Entity | Format |
|---|---|
| Type | `<see cref="T:Namespace.TypeName" />` |
| Method | `<see cref="M:Namespace.TypeName.MethodName(ParamType,ParamType)" />` |
| Property | `<see cref="P:Namespace.TypeName.PropertyName" />` |
| Event | `<see cref="E:Namespace.TypeName.EventName" />` |
| Field, constant, enum member | `<see cref="F:Namespace.TypeName.FieldName" />` |

- Use `<see cref="F:System.String.Empty" />` and `<see cref="F:System.Guid.Empty" />` for those values.
- When referencing `DebugUtils.LogException`, use `<see cref="M:xyLOGIX.Core.Debug.DebugUtils.LogException(System.Exception,System.Boolean)" />`.
- Prefer generic backtick-plus-arity notation, such as `System.Collections.Generic.IList`1`, over curly-brace generic notation.
- If a term looks like code but cannot or should not be cross-referenced, wrap it in `<c>...</c>`. Examples: file names, `AssemblyInfo`, `AssemblyTitle`, source comments, and source-level attributes.
- Inline reproduced code or comments must be wrapped in `<c>...</c>`.
- Avoid XML documentation `cref` references that would require adding circular project references.

### 9.4) Backing field docs

If a field backs a property:

- The field summary explains both the property's purpose and the field's purpose.
- The remarks include: `<b>NOTE:</b> The purpose of this field is to cache the value of the <see cref="P:Fully.Qualified.Property" /> property.`
- If the field exists to raise property-change events rather than cache, document that instead.

## 10) Properties, fields, and events

### 10.1) Properties and fields

- Prefer auto-properties when feasible.
- Do not use expression-bodied properties or expression-bodied accessors.
- Use backing fields only when an update must raise an event or a read-only value must be cached.
- All property getters and setters must be decorated at accessor level with `[DebuggerStepThrough]`.
- Add `using System.Diagnostics;` when `[DebuggerStepThrough]` is used.
- Getter-only auto-properties are preferred for values initialized in constructors or static initialization.
- Do not use `init;`.
- When exposing controls to external clients, put control properties in the form's main `.cs` file, not in `.Designer.cs`.

Example:

```csharp
public int Count
{
    [DebuggerStepThrough] get;
    [DebuggerStepThrough] set;
}
```

### 10.2) Events

- Never declare an event that is never raised.
- Every event has a corresponding `protected virtual void OnXxx(...)` invoker.
- The invoker calls `Xxx?.Invoke(this, e);` or the appropriate delegate signature.
- If adding virtual methods to a sealed class, remove `sealed`.
- EventArgs-derived classes should be in `.Events`, derive from `System.EventArgs`, use getter-only properties initialized by constructor, and decorate constructors with `[Log(AttributeExclude = true)]` where appropriate.
- Do not use object initializer syntax when raising an event; pass values through the EventArgs constructor.
- For property-sheet Apply behavior, the event invoker may be named `DoApplyChanges` instead of `OnApplyChanges`.

## 11) Windows Forms and desktop UI

1. Follow classic Microsoft desktop UI conventions, especially the Windows User Interface Guidelines for Software Design and classic Windows 3.x/95 style where applicable.
2. Forms use Segoe UI 9pt, not MS Sans Serif 8.25pt.
3. Push buttons are sized `(87, 27)` unless existing UI or user direction requires otherwise.
4. If `xyLOGIX.UI.Dark` libraries are present, forms and dialogs should derive from `xyLOGIX.UI.Dark.Forms.DarkForm`; otherwise use `System.Windows.Forms.Form`.
5. If dark forms are used, form interfaces should inherit `xyLOGIX.UI.Dark.Forms.IDarkForm`. Otherwise they can inherit `xyLOGIX.Core.Extensions.IForm`.
6. Use `DarkXxx` controls when the dark controls library provides an equivalent; otherwise use the standard Windows Forms control.
7. Do not add `MenuStrip`, `ToolStrip`, or `StatusStrip` to a `FixedDialog` form.
8. Never attach handlers to a form's own events when an `OnXxx` override is available. Override the protected virtual method and call the base implementation first.
9. Dialogs must be owned when shown. If no owner is available, set `StartPosition` to `CenterScreen` at runtime.
10. OK and Cancel buttons do not have mnemonic underlines. Other action buttons may.
11. OK buttons are named `okayButton`, have `DialogResult.OK`, and should be the second-to-last tab stop.
12. Cancel buttons are named `cancelButton`, have `DialogResult.Cancel`, set `CausesValidation` to `false`, and are last in tab order.
13. About-style dialogs may use a single `&Close` button named `closeButton`, serving as both `AcceptButton` and `CancelButton`, with `DialogResult.Cancel` and `CausesValidation = false`.
14. It is not necessary to add Click handlers to OK, Cancel, or Close buttons solely to close the dialog; use `DialogResult`.
15. Use `OnFormClosing` to trap `DialogResult.OK` and run validation.
16. UI element names in XML documentation are Title Case, omit mnemonic ampersands, and are wrapped in `<b>...</b>`.
17. Form titles referenced in XML documentation should also be Title Case and wrapped in `<b>...</b>`.
18. In Model-View-Presenter forms, expect a `MyForm.Presenter.cs` partial file containing the `Presenter` property and `InitializePresenter` method.

### 11.1) Window styles

Main document-style windows generally use:

- `AutoScaleMode = Dpi`
- `ControlBox = true`
- `FormBorderStyle = Sizable`
- `MaximizeBox = true`
- `MinimizeBox = true`
- `ShowIcon = true`
- `ShowInTaskbar = true`
- `Size = 1024x768`
- `SizeGripStyle = Show`
- `StartPosition = CenterScreen`
- `WindowState = Maximized`

Dialog-based utility main windows generally use:

- `AutoScaleMode = Dpi`
- `ControlBox = true`
- `FormBorderStyle = FixedSingle`
- `IsMdiContainer = false`
- `MaximizeBox = false`
- `MinimizeBox = true`
- `ShowIcon = true`
- `ShowInTaskbar = true`
- `Size = 470x561`
- `SizeGripStyle = Show`
- `StartPosition = CenterScreen`
- `WindowState = Normal`

Dialog boxes generally use:

- `AutoScaleMode = Dpi`
- `ControlBox = true`
- `FormBorderStyle = FixedDialog`
- `IsMdiContainer = false`
- no menu
- `MaximizeBox = false`
- `MinimizeBox = false`
- `ShowIcon = false`
- `ShowInTaskbar = false`
- `Size = 470x561`
- `SizeGripStyle = Show`
- `StartPosition = CenterParent`
- `WindowState = Normal`

Leave unspecified properties at defaults unless existing code or user instructions require changes.

## 12) Testing

- Use NUnit 4.3.2.
- Prefer one test fixture per concrete class.
- Abstract base test fixtures may share behavior through Template Method.
- Prefer deriving fixtures from `xyLOGIX.Tests.Logging.LoggingTestBase` when that module is available.
- Test fixture classes use `[TestFixture]` and `[ExplicitlySynchronized]`.
- If multiple tests display or interact with GUI, add `[NonParallelizable]` and `[Apartment(ApartmentState.STA)]` at fixture level.
- If only one test displays GUI, apply `[Apartment(ApartmentState.STA)]` to that test.
- If any test displays a Windows Form, override `LoggingTestBase.OneTimeSetUp`, call `base.OneTimeSetUp()`, then call `Application.EnableVisualStyles()` and `Application.SetCompatibleTextRenderingDefault(false)`.
- Do not add comments explaining the absence of `[OneTimeSetUp]` on the override.
- Unit tests are written when certainty is needed; do not create broad test scaffolding unless it directly supports the task.

## 13) File-system operations

After critical file-system operations, verify the result:

| Operation | Verification |
|---|---|
| Copy file | Destination exists. |
| Move file | Destination exists and source no longer exists. |
| Create directory | Directory exists. |
| Delete file | File no longer exists. |

Prefer existing repository abstractions such as `AlphaFsFileSystem` or `xyLOGIX.Core.Files.Does` when present. Log the check, failure, success, and final result using the repository's established logging format.

## 14) Pipeline architecture

Use this pattern to decompose a God Method or sequential workflow into testable units.

| Artifact | Project suffix | Role |
|---|---|---|
| Step enum | `.Constants` | One member per step; `Unknown = -1` last. |
| Step interface | `.Steps.[WorkflowName].Interfaces` | Contract for one step. |
| Step base class | `.Steps.[WorkflowName]` | Template Method base and shared services. |
| Concrete step classes | `.Steps.[WorkflowName]` | One class per enum member. |
| Pipeline interface | `.Pipelines.Interfaces` | `ExecuteAsync` and `ProgressUpdate`. |
| Pipeline class | `.Pipelines` | Runs steps in fixed order and stops on first failure. |
| Context interface | `.Contexts.Interfaces` | Shared workflow state. |
| Context class | `.Contexts` | Concrete state holder. |
| Step factory | `.Factories` | `MakeNew[WorkflowName]Step.OfType(...)`. |

Rules:

1. The step enum is alphabetized with `Unknown = -1` last.
2. Mention `IXxxStep` as `<c>IXxxStep</c>` in enum remarks to avoid circular references.
3. The step interface exposes `Step`, `ExecuteAsync([NotLogged] IXxxContext context)`, and `ProgressUpdate`.
4. The abstract step base declares static and protected constructors with `[Log(AttributeExclude = true)]`.
5. Concrete steps are named `[EnumMemberName]Step` and override `Step` with the matching enum value.
6. Concrete step `ExecuteAsync` methods validate context first, use the async result pattern, and call `context.IncrementCurrent()` at a meaningful point.
7. The pipeline creates steps through the factory, subscribes to progress, executes, unsubscribes, and stops on first failure.
8. The fixed step order must be documented and invariant.
9. Context progress mutation uses `IncrementCurrent()` and `ResetTotalSteps(int)` with thread-safe implementation.
10. For `ICloneContext`, the correct method is `context.IncrementCurrent()`, not `context.IncrementCurrentStep()`.

When refactoring into a pipeline, proceed in phases and stop after each phase:

1. Discovery: identify steps and propose the enum only.
2. Constants and context.
3. Step interface and base class.
4. Concrete steps, one at a time.
5. Pipeline interface and class.
6. Factory.

Before every phase, check for circular dependencies.

## 15) ProjectCloner-specific reminders

- Preserve current architecture unless explicitly asked to redesign it.
- For source generation, check `CONTRIBUTING.md` for required headers.
- Do not replace existing custom GUIDs without explicit instruction.
- Keep generated snippets small, complete, and in reference order.
- Do not provide `.patch` files unless explicitly asked; prefer step-by-step drop-in code and explanations.
- If working with text-editor configuration, build-event XML documentation fixes, or error reporting, inspect current implementations first because migration work may be partially complete.

## 16) Documentation file path generator architecture

For `PC.Generators.Paths.*`, generate `DocumentationFile` values according to Visual Studio convention:

| Platform | Configuration | Value |
|---|---|---|
| `AnyCPU` | `Debug` | `bin\Debug\{ProjectName}.xml` |
| `AnyCPU` | `Release` | `bin\Release\{ProjectName}.xml` |
| `x86` | `Debug` | `bin\x86\Debug\{ProjectName}.xml` |
| `x86` | `Release` | `bin\x86\Release\{ProjectName}.xml` |
| `x64` | `Debug` | `bin\x64\Debug\{ProjectName}.xml` |
| `x64` | `Release` | `bin\x64\Release\{ProjectName}.xml` |
| `ARM64` | `Debug` | `bin\ARM64\Debug\{ProjectName}.xml` |
| `ARM64` | `Release` | `bin\ARM64\Release\{ProjectName}.xml` |

Primary strategy: build configuration. Secondary strategy: platform.

- `DocumentationFilePlatform`: `AnyCpu`, `Arm64`, `X64`, `X86`, `Unknown = -1`.
- `DocumentationFileConfiguration`: `Debug`, `Release`, `Unknown = -1`.
- `IDocumentationFilePathGenerator` exposes `Platform` and `GenerateFor(string projectFileName)`.
- `IDocumentationFilePathGeneratorFactory` exposes `Configuration` and `AndPlatform(string platform)`.
- Platform generators compute the final relative path.
- Configuration factories manufacture platform generators with their configuration string.
- `MakeNewDocumentationFilePathGenerator.ForConfiguration(configuration).AndPlatform(platform).GenerateFor(projectFileName)` is the intended call chain.

To add a configuration, update the configuration enum, add a concrete factory, and update the configuration selector. To add a platform, update the platform enum, add a concrete generator, and update each configuration factory's platform switch.