# Contributing to %%Solt_Name%%

## XML Documentation Standards

### Referencing Interfaces and Types in `enum` Documentation

When documenting `enum` types that reference interfaces or other types they correspond to, use the appropriate tag based on context:

#### Use `<c>InterfaceName</c>` for General Mentions

When mentioning an `interface` name as a **general concept** (not as a specific navigation target), use `<c>...</c>`:

```csharp
/// <remarks>
/// Each member of this enumeration corresponds to a specific concrete
/// implementation of the <c>IBuildEventReplacer</c> interface.
/// </remarks>
public enum BuildEventReplacerType
{
    // ...
}
```

In general, we tend to favor the usage of `<c>...</c>` to surround type names (other than the `enum` itself, or its members) since `enum` types tend to be at the ends of dependency-tree branches, i.e., they are leaves; furthermore, `class`es, `interface`s, and so forth tend to refer to them, but the library(ies) that contain the `enum` almost never has a need to have a project reference back to the library(ies) containing the `interface`s or `class`es that use the `enum`, so it would be horrible, and would create a circular dependency, if we indeed did put a `<see cref="T:..." />` style cross-reference to an interface in the XML documentation of such an enum, since 9 times out of 10, doing so might otherwise need to create a project reference to the container of that, which, 9 times out of 10, would end up resulting in a circular dependency.  You should always verify whether a circular dependency might otherwise need to be created (and we cannot have those; they are bad, verboten, forbidden, nunca, jamis, nie) when giving code in your responses.  Otherwise, in general, we should actually _favor_ the usage of `<see cref="T:..." />`, `<see cref="E:..." />`, `<see cref="F:..." />`, `<see cref="P:..." />`, and `<see cref="M:..." />` cross-references as much as possible; it's just that, the way we almost always declare and use `enum` types, just spraying such things around willy-nilly is inadvisable.

#### Use `<see cref="T:..." />` for Specific Cross-References

Reserve fully-qualified `<see cref="T:..." />` for situations where you want the reader to be able to navigate directly to the type's documentation:

```csharp
/// <summary>
/// The replacer strategy is selected based on the
/// <see cref="T:PC.Replacers.BuildEvents.Constants.BuildEventReplacerType" />
/// enumeration value.
/// </summary>
public interface IBuildEventReplacer
{
    // ...
}
```

However, one should bear in mind that a project reference will also need to exist for the cross-reference to resolve properly.  This is why one should avoid overusing `<see cref="..." />` in favor of `<c>...</c>` when the intent is simply to mention a type.

#### Key Differences

| Scenario | Tag to Use | Example |
|----------|------------|---------|
| General mention of a type name as a concept | `<c>TypeName</c>` | `<c>IBuildEventReplacer</c>` |
| File names, attributes, or code constructs | `<c>FileName</c>` | `<c>AssemblyInfo</c>`, `<c>PreBuildEvent</c>` |
| Specific cross-reference for navigation | `<see cref="T:Full.Type.Name" />` | `<see cref="T:PC.Replacers.BuildEvents.Interfaces.IBuildEventReplacer" />` |
| Referencing methods, properties, fields | `<see cref="M/P/F:..." />` | `<see cref="M:MyClass.MyMethod(Syste...` |

## Source File Header Standard

Every source file must include the following standard copyright/trademark header block at the very top, verbatim:

```
// Copyright © 2021-2026 xyLOGIX, LLC.  All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
// xyLOGIX and %%Product_Name%% are trademarks of xyLOGIX, LLC.
// Trademark rights are not granted under the MIT License.
```

The line **"Trademark rights are not granted under the MIT License."** must never be changed to any variant such as "Trademark rights are not licensed under the MIT License." The word **"granted"** is mandatory and must never be substituted.

## Result Variable Assignment Pattern in Logic Gates

When implementing logic gates (input validation, condition checking) in methods, follow this pattern:

### Preferred Pattern

```csharp
public bool MyMethod(string input)
{
    var result = false;

    try
    {
        // Set result to the desired value BEFORE the logic gate
        result = true;

        DebugUtils.WriteLine(
            DebugLevel.Info,
            "MyMethod: Checking whether the input is valid..."
        );

        // Check to see whether the input is valid.
        // If this is not the case, then write an error message to the log file,
        // and then terminate the execution of this method.
        if (!IsValidInput(input))
        {
            // The input is NOT valid.  This is not desirable.
            DebugUtils.WriteLine(
                DebugLevel.Error,
                "MyMethod: *** ERROR *** The input is NOT valid.  Stopping..."
            );

            // Set result back to its default value AFTER the gate
            result = false;

            DebugUtils.WriteLine(
                DebugLevel.Debug,
                $"MyMethod: Result = {result}"
            );

            // stop.
            return result;
        }

        DebugUtils.WriteLine(
            DebugLevel.Info,
            "MyMethod: *** SUCCESS *** The input is valid.  Proceeding..."
        );

        // Continue with method logic...
    }
    catch (Exception ex)
    {
        // dump all the exception info to the log
        DebugUtils.LogException(ex);

        result = false;
    }

    return result;
}
```

### Key Points

1. **Set `result` to the desired value BEFORE the `if` check**
2. **Perform the logic gate check**
3. **Set `result` back to its default value AFTER the gate (if the gate doesn't execute)**
4. **Avoid assignment of `result` inside the `if` block itself**

This pattern provides:
- Better traceability of the code flow
- Clearer intent of what the method is checking
- Improved debug logging visibility
- Consistent structure across all validation gates

### What to Avoid

❌ **Do NOT assign `result` inside the `if` block:**

```csharp
if (!IsValidInput(input))
{
    result = false;  // DON'T DO THIS
    return result;
}
```

✅ **Instead, set it before and after the gate:**

```csharp
result = true;  // Set desired value

if (!IsValidInput(input))
{
    result = false;  // Reset to default
    return result;
}
```

## Windows Forms / Dialog UI/UX Conventions

These rules apply to every Windows Form and dialog box in this codebase.  They are distilled from observation of `OptionsDialogBox`, `AddEditPathRegexDialogBox`, `GeneralInfoCloneWizardStepDialog`, `AssemblyInfoCloneWizardStepDialog`, `MarqueeProgressDialogBox`, and `SingleProgressBarDialog`.

### Section 1 — Designer field naming

Every control field in a `.Designer.cs` file is named in `camelCase` and reflects both its role and control type.  The suffixes are mandatory:

| Control type | Field name suffix | Example |
|---|---|---|
| `DarkButton` / `Button` | `Button` | `addTextEditorButton`, `editRegexToAlwaysCloneButton` |
| `DarkCheckBox` / `CheckBox` | `CheckBox` | `cloneGlobalSourceFilesCheckBox`, `usePreferredEditorCheckBox` |
| `DarkComboBox` / `ComboBox` | `ComboBox` | `preferredEditorComboBox` |
| `DarkGroupBox` / `GroupBox` | `GroupBox` | `preferredEditorGroupBox` |
| `DarkListBox` / `ListBox` | `ListBox` | `textEditorsListBox`, `pathRegexesAlwaysCloneListBox` |
| `DarkLabel` / `Label` | `Label` or `Divider` | `textEditorsLabel`, `pathRegexexAlwaysCloneDivider` |
| `DarkTextBox` / `TextBox` | `TextBox` | `cloneNameTextBox` |
| `DarkTabControl` / `TabControl` | `TabControl` | `optionsTabControl` |
| `TabPage` | `TabPage` | `generalTabPage`, `textEditorsTabPage` |
| Standard dialogs | Descriptive noun | `browseDialog` |

Button field names spell out the verb and the noun completely.  There are no bare `addButton`, `editButton`, `removeButton` shortcuts — always `addTextEditorButton`, `editRegexToAlwaysCloneButton`, etc.

### Section 2 — Control sizing and font

- **All push buttons** must be sized `(87, 27)`.  Never `(75, 23)`.
- **All forms** use `Font = Segoe UI, 9pt` and `AutoScaleMode = Dpi`.
- **Dark-theme projects** use `DarkButton`, `DarkCheckBox`, `DarkComboBox`, `DarkGroupBox`, `DarkLabel`, `DarkListBox`, `DarkTabControl`, `DarkTextBox` from `xyLOGIX.UI.Dark.Controls` in place of all `System.Windows.Forms` counterparts.  `DarkLabel` replaces `Label` everywhere, even for static text.
- `DarkButton` fields always set `IsDarkTheme = true`, `Padding = new Padding(5)`, and `PressedBackColor = Color.Empty`.

### Section 3 — Button caption and wiring conventions

| Role | Caption | `DialogResult` set? | Click handler wired in Designer? |
|---|---|---|---|
| **OK** | `"OK"` (no mnemonic) | `DialogResult.OK` | No — `OnFormClosing` handles the OK path |
| **Cancel** | `"Cancel"` (no mnemonic) | `DialogResult.Cancel` | No |
| **Apply** | `"A&pply"` (mnemonic on `p`) | None | Yes — `OnClickApplyButton` |
| **Next** (wizard) | `"&Next"` | `DialogResult.OK` | No |
| **Browse...** | `"&Browse..."` or `"Bro&wse..."` | None | Yes |
| **Add...** | `"&Add..."` or `"&Add <noun>"` | None | Yes |
| **Edit...** | `"&Edit..."` or `"E&dit"` | None | Yes |
| **Remove** | `"&Remove"` | None | Yes |
| **Remove All** | `"Remove &All"` | None | Yes |
| **Close** (About/progress) | `"&Close"` | `DialogResult.Cancel` | No |

The **OK** and **Cancel** buttons (and the **Next** / **Cancel** pair in wizards) are never given `Click` handlers — setting `DialogResult` alone is sufficient and `OnFormClosing` intercepts the OK / Next path for validation.

### Section 4 — Tab order

Tab indices always progress in reading order: upper-left → lower-right, top → bottom, group-by-group.

- When a `DarkLabel` has an access-key mnemonic (e.g., `"Pro&ject to Clone:"`), it is assigned a tab index **one before** the control it labels (e.g., its associated combo box or text box).  Labels never receive keyboard focus themselves; the mnemonic fires and focus lands on the next control in tab order — which is the associated input control.
- In a property-sheet (Options-style dialog), the dialog-level commit buttons receive the **last** tab indices on the form so the tab ring visits all content controls first:
  - `TabIndex = 1` → **OK** button
  - `TabIndex = 2` → **Cancel** button
  - `TabIndex = 3` → **Apply** button (property sheets only)
- Controls inside `TabPage`s or `GroupBox`es are numbered independently within their container, starting from `TabIndex = 0`.

### Section 5 — Button exposure rules (MVP vs. self-contained dialogs)

**For dialogs that use MVP (Model-View-Presenter):** every button that is not **OK**, **Cancel**, **Apply**, or **Close** gets a full triad of public properties on the form and three corresponding public events, all exposed through the form's `interface` so the Presenter has complete remote control:

```csharp
// Control reference (read-only)
public DarkButton AddTextEditorButton { [DebuggerStepThrough] get => addTextEditorButton; }

// Enabled wrapper — setter raises OnAddTextEditorButtonEnabledChanged()
public bool AddTextEditorButtonEnabled
{
    [DebuggerStepThrough] get => addTextEditorButton.Enabled;
    [DebuggerStepThrough]
    set { var changed = addTextEditorButton.Enabled != value; addTextEditorButton.Enabled = value; if (changed) OnAddTextEditorButtonEnabledChanged(); }
}

// Text wrapper — setter raises OnAddTextEditorButtonTextChanged()
public string AddTextEditorButtonText
{
    [DebuggerStepThrough] get => addTextEditorButton.Text;
    [DebuggerStepThrough]
    set { var changed = addTextEditorButton.Text != value; addTextEditorButton.Text = value; if (changed) OnAddTextEditorButtonTextChanged(); }
}

// Events
public event EventHandler AddTextEditorButtonClicked;
public event EventHandler AddTextEditorButtonEnabledChanged;
public event EventHandler AddTextEditorButtonTextChanged;

// Invocators
protected virtual void OnAddTextEditorButtonClicked()
    => AddTextEditorButtonClicked?.Invoke(this, EventArgs.Empty);

protected virtual void OnAddTextEditorButtonEnabledChanged()
    => AddTextEditorButtonEnabledChanged?.Invoke(this, EventArgs.Empty);

protected virtual void OnAddTextEditorButtonTextChanged()
    => AddTextEditorButtonTextChanged?.Invoke(this, EventArgs.Empty);
```

This pattern applies to **every** non-OK/Cancel/Apply button in an MVP-backed form, not just list-management buttons.  The same triad and event set applies to the six regex-group buttons and all four Text Editors tab buttons in `OptionsDialogBox`.

**For self-contained dialogs with no Presenter** (e.g., `AddEditPathRegexDialogBox`): a button that triggers inline logic needs only a single read-only control-reference property and a `private` `Click` handler wired in the Designer.  No events are exposed because there is no `interface` consuming them.

### Section 6 — The `applyButton` (property-sheet pattern)

The `applyButton` is exclusive to property-sheet (Options-style) dialogs:

- Starts `Enabled = false` in the Designer.
- Any user change to any control sets `private bool _hasChanges = true` **and** enables the button.
- Its `Click` handler calls `DoApplyChanges()`, which gates on `_hasChanges` and raises `ApplyChanges?.Invoke(this, EventArgs.Empty)`.
- `OnFormClosing` for `DialogResult.OK` also calls `DoApplyChanges()` so that pending changes are committed if the user clicks **OK** without having clicked **Apply** first.
- The `ApplyChanges` event is subscribed to by the Presenter, which calls `SaveToConfiguration()` in response.

### Section 7 — `CheckedChanged` handler conventions

Every `DarkCheckBox` wires a `CheckedChanged` handler that is:

- `private`
- Decorated with `[Log(AttributeExclude = true)]`
- Parameters: `[NotLogged] object sender, [NotLogged] EventArgs e`

The handler:
1. Marks `_hasChanges = true` (in property-sheet dialogs) and enables the **Apply** button.
2. Triggers any dependent enable/disable logic (e.g., checking **Ignore Source Files When Cloning** disables **Clone Global Source Files**).

### Section 8 — Presenter-backed vs. self-contained dialogs

**Property sheets with a Presenter (`OptionsDialogBox` pattern):**
- The form's `interface` (`IOptionsDialogBox`) exposes every control's value, state, and events that the Presenter needs.
- The Presenter subscribes to events on the interface; the form raises them via `OnXxx()` invocators.
- The form never calls business logic directly — it raises events and exposes property wrappers only.
- `LoadFromConfiguration()` and `SaveToConfiguration()` live in the Presenter, not the form.

**Simple data-entry dialogs (`AddEditPathRegexDialogBox` pattern):**
- No Presenter; no dedicated `interface` beyond the form's own class.
- `OnFormClosing` handles all validation inline via private `TryValidateXxx` helpers.
- No `ApplyChanges` event; no `_hasChanges` field.
- Only data fields get public properties — no button triads, no button events exposed.

### Section 9 — Progress dialogs

`MarqueeProgressDialogBox` / `SingleProgressBarDialog` pattern:

- The form's **Cancel** and **Close** buttons are internal — they are not exposed as properties.
- `Status` (`string`) is the only user-visible data property.
- For `SingleProgressBarDialog`, `Operation` (`Func<IProgress<IProgressUpdate>, Task>`) is injected by the caller before `ShowDialog` is called.
- For `MarqueeProgressDialogBox`, `ActionToDo` (`Action`) is injected via the fluent `AndActionToDo(Action)` method.

---

## MVP Wiring Conventions

### Presenter construction and View event subscription

The Presenter is always responsible for subscribing to View events.  The View never calls the Presenter directly.

- **View construction:** The View (Form) is created first, then passed to the Presenter's constructor.
- **Event subscription:** The Presenter's constructor calls a private `InitializeView()` method, which:
  1. Guards against a `null` `View` reference.
  2. Subscribes the Presenter's event handler(s) to the View's event(s) (e.g., `View.Load += OnViewLoad`).
- `InitializeView()` is always `private`, always wrapped in `try/catch`, and always logs its progress.
- The two-argument (View-accepting) constructor is **never** decorated with `[Log(AttributeExclude = true)]` — only the parameterless constructor receives that decoration.

### `View.Load` — combo box population

Populating a combo box with `enum` values is **Presenter logic**, not View logic.  The correct flow is:

1. Presenter subscribes `OnViewLoad` to `View.Load` inside `InitializeView()`.
2. When `Load` fires, `OnViewLoad` calls `PopulateMatchModeComboBox()` then `SetMatchMode(modeToSelect)`.
3. `PopulateMatchModeComboBox()` also subscribes `OnMatchModeComboBoxSelectedIndexChanged` to `View.MatchModeComboBox.SelectedIndexChanged` (idempotent: `-=` then `+=`).
4. `OnMatchModeComboBoxSelectedIndexChanged` resolves the selected display string back to the `enum` value and writes it to `View.MatchMode`.

**Never place `TryPopulateXxxComboBox` or any combo-population logic inside an `OnLoad` or `OnShown` override in the View.**  The View is a dumb data surface.

### `enum` display strings — `DescriptionAttribute` as single source of truth

Friendly display strings for `enum` members are declared **on the `enum` member itself** using `System.ComponentModel.DescriptionAttribute`.  There is no separate dictionary in the Presenter.

```csharp
public enum PathRegexMatchMode
{
    [Description("File name only")]
    FileNameOnly,

    [Description("Folder name only")]
    FolderNameOnly,

    [Description("Full pathname")]
    FullPathname,

    Unknown = -1
}
```

The Presenter reads the attribute via a `private static string GetDescription(TEnum mode)` helper that:
1. Validates the `enum` value using the corresponding `*Validator` class before doing any reflection.
2. Calls `typeof(TEnum).GetMember(mode.ToString())`, guards against null/empty result.
3. Calls `memberInfo[0].GetCustomAttribute<DescriptionAttribute>()`, guards against null.
4. Returns `attribute.Description ?? string.Empty`.

### Presenter static validator property pattern

Every Presenter that validates an `enum` value uses a `private static` validator property, initialized inline from the corresponding factory:

```csharp
private static IPathRegexMatchModeValidator PathRegexMatchModeValidator
{
    [DebuggerStepThrough] get;
} = GetPathRegexMatchModeValidator.SoleInstance();
```

This keeps validator acquisition out of method bodies and avoids repeated factory calls.

---

## `enum` Declaration Conventions

- Members are in **alphabetical order**.
- No explicit numeric value is assigned to any member except `Unknown = -1`.
- `Unknown = -1` is always the **last** member.
- Every `enum` and every member has XML documentation.
- Friendly display strings are declared on each non-`Unknown` member with `[System.ComponentModel.Description("...")]`.
- The `[Log(AttributeExclude = true)]` attribute is **never** applied to an `enum` or its members.
- The form closes itself via `RequestClose(DialogResult.OK)` on success or `RequestClose(DialogResult.Cancel)` on cancellation/exception.  The caller never manually closes it.
- `OnShown` (not `OnLoad`) is the entry point for kicking off the async or synchronous work.
- `ControlBox = false` and `ShowInTaskbar = false` are set in the Designer.

### Section 10 — Wizard step dialogs

**`VisualStudioProjectWizardForm`-derived steps (`GeneralInfoCloneWizardStepDialog` pattern):**

- Each step form derives from `CloneWizardStepDialogBase`, which derives from `VisualStudioProjectWizardForm` → `VisualStudioDialogForm` → `DarkForm` → `Form`.
- Navigation buttons are named `okayButton` (**Next**, caption `"&Next"`, `DialogResult.OK`) and `cancelButton` (**Cancel**, `DialogResult.Cancel`).  They function identically to **OK**/**Cancel** in a standard dialog — `DialogResult` closes the form; no `Click` handlers are needed.
- A left-aligned contextual button (e.g., `optionsButton`, caption `"&Options..."`) may appear at the bottom-left of the form opposite the navigation buttons.  It gets a `Click` handler wired in the Designer.
- `AttachCloneContext(ICloneContext)` injects shared cloning state; it throws `ArgumentNullException` immediately if passed `null`.
- `LoadFromContext` and `SaveToContext` are `protected override` in each concrete step; they are the only data-flow methods.

**Wizard97-style paged wizards (Cristi Prolog's `WizardControl`):**

- When using a `WizardControl`-hosted multi-page wizard, the page control itself owns all navigation.  Individual pages do not declare navigation buttons.
- The wizard host form sets `AcceptButton` / `CancelButton` at the host level only.

### Section 11 — Controls that never get public properties

`DarkLabel` (and `Label`) controls are **never** given public properties, events, or any form-level exposure.  They are purely presentational and must not be referenced outside the form class.

---

## Collection-Validation Loop Patterns

These rules are distilled from observation of `CloneContextValidator.ValidatePathRegexRuleCollection`.

### Rule 1 — Delegate per-element validation to a dedicated `*Validator` class

Never hand-roll per-field checks (blank name, blank regex, unknown match mode, etc.) inside a loop body.  Instead, call the dedicated `*Validator.IsValidSilent(element)` method for the element type:

```csharp
if (PathRegexRuleValidator.IsValidSilent(rule))
{
    // valid — proceed to the next element
    continue;
}

// Invalid element found — stop and return false.
result = false;
break;
```

The validator is always surfaced as a `private static` property initialized from its factory, identical to all other validator properties in the class:

```csharp
private static IPathRegexRuleValidator PathRegexRuleValidator
{
    [DebuggerStepThrough] get;
} = GetPathRegexRuleValidator.SoleInstance();
```

### Rule 2 — Set `result = true` *before* the loop, not after it

The optimistic success value is assigned immediately after the pre-loop null/empty short-circuits, before the `foreach` begins.  This is the "all rules are valid unless the loop proves otherwise" invariant.  The loop only needs to correct `result` to `false` and `break` when it discovers an invalid element:

```csharp
// All pre-loop gates passed — assume every rule is valid
// unless the loop below finds otherwise.
result = true;

foreach (var rule in rules)
{
    if (PathRegexRuleValidator.IsValidSilent(rule))
        continue;   // valid → next element

    result = false; // invalid → abort
    break;
}
```

❌ **Do NOT** write `result = true;` after the loop or inside a `/* If we made it this far... */` block at the end of the method when a validation loop is involved.  The success-before-loop pattern is the correct one here.

### Rule 3 — `null` elements inside a collection are silently skipped, not hard failures

A `null` element encountered during iteration is treated as a non-fatal anomaly.  Log a warning and `continue`; do not set `result = false` and do not `break`:

```csharp
if (rule == null)
{
    DebugUtils.WriteLine(
        DebugLevel.Warning,
        $"CloneContextValidator.ValidatePathRegexRuleCollection: *** WARNING *** A null rule was found in the '{label}' collection.  Skipping..."
    );

    // skip this rule.
    continue;
}
```

### Rule 4 — Two distinct uses of `continue` in the same loop

The loop body contains **two separate `continue` statements** with completely different semantics — do not conflate them:

| `continue` site | Condition | Meaning |
|---|---|---|
| Null-element guard | `rule == null` | Non-fatal anomaly — skip with a **warning** log |
| Validator pass | `IsValidSilent(rule) == true` | Element is **valid** — advance to the next element normally |

Only the `IsValidSilent == false` path (no `continue`) leads to `result = false; break`.

### Rule 5 — A `null` or empty collection is valid (zero elements is acceptable)

Guard against `null` and `Count <= 0` at the top of the method and return `true` immediately in both cases.  Zero rules is an acceptable configuration, not an error:

```csharp
if (rules == null)
{
    result = true;
    return result;
}

if (rules.Count <= 0)
{
    result = true;
    return result;
}
```

### Rule 6 — The `collectionName` parameter drives all log messages

A `collectionName` parameter (or analogous label string) is always accepted so that log messages unambiguously identify *which* collection is being validated.  If the caller passes `null` or blank, substitute a generic placeholder:

```csharp
var label = string.IsNullOrWhiteSpace(collectionName)
    ? "(unnamed collection)"
    : collectionName;
```

All subsequent log messages use `label` rather than a hard-coded collection name.

### Complete canonical example

```csharp
private static bool ValidatePathRegexRuleCollection(
    [NotLogged] IList<IPathRegexRule> rules,
    [NotLogged] string collectionName
)
{
    var result = false;

    try
    {
        var label = string.IsNullOrWhiteSpace(collectionName)
            ? "(unnamed collection)"
            : collectionName;

        if (rules == null)
        {
            result = true;

            DebugUtils.WriteLine(
                DebugLevel.Debug,
                $"CloneContextValidator.ValidatePathRegexRuleCollection: Result = {result}"
            );

            return result;
        }

        if (rules.Count <= 0)
        {
            result = true;

            DebugUtils.WriteLine(
                DebugLevel.Debug,
                $"CloneContextValidator.ValidatePathRegexRuleCollection: Result = {result}"
            );

            return result;
        }

        // Assume all rules are valid; the loop below corrects this if it finds otherwise.
        result = true;

        foreach (var rule in rules)
        {
            if (rule == null)
            {
                DebugUtils.WriteLine(
                    DebugLevel.Warning,
                    $"*** WARNING *** A null rule was found in the '{label}' collection.  Skipping..."
                );

                continue; // null → skip (non-fatal)
            }

            if (PathRegexRuleValidator.IsValidSilent(rule))
                continue; // valid → next element

            // Invalid element found — abort immediately.
            result = false;
            break;
        }
    }
    catch (Exception ex)
    {
        // dump all the exception info to the log
        DebugUtils.LogException(ex);

        result = false;
    }

    DebugUtils.WriteLine(
        DebugLevel.Debug,
        $"CloneContextValidator.ValidatePathRegexRuleCollection: Result = {result}"
    );

    return result;
}