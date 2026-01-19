# Repository instructions for GitHub Copilot (C# 7.3 / .NET Framework 4.8 / WinForms 2.0)

These instructions apply to all Copilot Chat responses and code changes in this repository.

> NOTE: This file is intentionally ASCII-only (ANSI-safe) and uses valid Markdown.

## 0) Priority and behavior (read first)

1. **User prompt > existing codebase conventions > these instructions.**
2. **Do not change existing XML documentation.** Only add missing docs. Never "rewrite" docs that already exist.
3. Output should be **as small as possible while still complete**:
   - Avoid unnecessary scaffolding, boilerplate, or speculative code (unless requested).
   - Do not produce "skeleton-only" code; produce code that compiles and is usable.
4. If a response would be long, **work incrementally**:
   - Clearly label **Step 1**, **Step 2**, etc.
   - Stop after each step and ask if the user is ready for the next step.
   - Do not introduce new tasks until the current task is done.
5. Before starting work on a task:
   - Read the relevant parts of the codebase and any related project files.
   - Analyze the dependency graph of the #solution.
   - Make sure you understand how the code works before changing it.
   - Mimic the surrounding format, tone, style, language features, comments, and logging.
6. All code should be as production-ready as possible.
7. Note what year it currently is. Quick bio of me, your user:
   - Name: Dr. Brian Hart (male)
   - Country: United States of America
   - Born: 1980
   - Programming since: 1994 (C# since 2000)
   - Education: Ph.D. in Astrophysics, University of California, Irvine
     - X-ray astrophysicist; expert on clusters of galaxies
   - Service: U.S. Navy, Cyber Warfare Engineer, commissioned officer (2018-2021)
   - Expectation: Do not patronize me or over-explain unless I expressly ask.
   - However: In docs/README/issues/PRs/merge commits/commit messages, write for professional peers.
8. All output other than code should be in U.S. English.
9. When running as an agent, double-check that you are indeed correct, before making a code change.
   - I am willing to wait for quality code as opposed to getting shitty, poorly-written code right away.
10. Separate concerns and loosely couple software components whenever possible.
11. I like Uncle Bob's Clean Code.  Write Uncle Bob's Clean Code at all times.
12. Never nest `try`/`catch`, `try`/`finally`, or `try`/`catch`/`finally` blocks.
   - If you find yourself needing nesting, extract the nested work into an entirely separate method.
13. Never use local functions.
14. If you have to write a God Method, make it a flat pipeline of helper methods.
   - Each following the Single Responsibility Principle (SRP).  
   - What is to be passed to them is to be checked for validity.
   - What each returns should be logic-gated against and checked for validity.
15. Never nest `if` checks. 
   - Invert `if` statements.
   - Eagerly return.
   - Extract helper methods wherever you can.
16. If you have to know if a method succeeds or not but you also need something else returned from it, use the `private bool TryDoThis(string input, out IMyInterface otherResult)` pattern (like, for example, `Int32.TryParse()`).
16. Match the surrounding code: if nearby code is heavily commented and logs a lot, follow suit; if it is intentionally sparse, follow suit as well. Always mirror the layout, style, tone, format, and language features of the surrounding code.

### 0.1) The kinds of application(s) and software system(s) I tend to develop

1. Legacy Windows Forms 2.0 application(s)
    - More often than not, such application(s) are meant to augment the software that is already on my computer.
    - It is desired for this software to look and feel as much like Microsoft-produced software as possible.
    - A key emphasis is placed on these tools' UI and UX being laid out in as strict an adherance to the book,
      "The Windows User Interface Guidelines for Software Design" by Microsoft Press, published in 1995, and 
      on Windows 95 style UI design patterns as much as possible.
        - EXCEPTION: All push buttons must be sized (87,27) as opposed to (75,23) as called for by the Guidelines
        - EXCEPTION: All forms must have Segoe UI, 9pt for their font and NOT MS Sans Serif, 8.25pt.
    - The Window style/chrome of a form must vary depending on the type of form that it is.
        - If the main window of an application that has a menu bar, one or more toolbar(s), a status bar, and 
          displays MDI child windows or tabbed documents in which the user enters, views, updates, or deletes 
          data and loads or saves that data to/from document file(s) that the user opens and saves with the application,
          such as Word or Excel, then the form must have an "overlapped window" style.  Specifically, certain of its properties must have the following value(s) (note -- if a property is not covered in the following list, leave its default value alone): 
            - `AutoScaleMode` = `Dpi`
            - `ControlBox` = `true`
            - `Font` = `Segoe UI, 9pt`
            - `FormBorderStyle` = `Sizable`,
            - `IsMdiContainer` ask me
            - `MainMenuStrip` -> Set to main menu `MenuStrip`
                - Such a control should be called `mainMenu` by default
            - `MaximizeBox` = `true`
            - `MinimizeBox` = `true`
            - `ShowIcon` = `true`
            - `ShowInTaskbar` = `true`
            - `Size` = 1024x768
            - `SizeGripStyle` = `Show`
            - `StartPosition` = `CenterScreen`,
            - `Text` should be equal to the application product name
            - `WindowState` = `Maximized`
        - If the main window of an application is that of a "dialog-based app" i.e., an app that perhaps alters data values, but  that does not necessarily open MDI child windows and/or deal with document files per se, then this is an app that   
          generally serves to be more of a "utility" style app.  Its main window must have the following styles/properties:
            - `AutoScaleMode` = `Dpi`
            - `ControlBox` = `true`
            - `Font` = `Segoe UI, 9pt`
            - `FormBorderStyle` = `FixedSingle`,
            - `IsMdiContainer` = `false`
            - `MainMenuStrip` -> Set to main menu `MenuStrip`
                - Such a control should be called `mainMenu` by default
            - `MaximizeBox` = `false`
            - `MinimizeBox` = `true`
            - `ShowIcon` = `true`
            - `ShowInTaskbar` = `true`
            - `Size` = 470x561
            - `SizeGripStyle` = `Show`
            - `StartPosition` = `CenterScreen`,
            - `Text` should be equal to the application product name
            - `WindowState` = `Normal`     
        - For a dialog box within either of the two type(s) of application(s) shown above, the properties/style(s) should be:
            - `AutoScaleMode` = `Dpi`
            - `ControlBox` = `true`
            - `Font` = `Segoe UI, 9pt`
            - `FormBorderStyle` = `FixedDialog`,
            - `IsMdiContainer` = `false`
            - `MainMenuStrip` -> no menu
            - `MaximizeBox` = `false`
            - `MinimizeBox` = `false`
            - `ShowIcon` = `false`
            - `ShowInTaskbar` = `false`
            - `Size` = 470x561
            - `SizeGripStyle` = `Show`
            - `StartPosition` = `CenterParent`,
            - `Text` should be equal to the application product name
            - `WindowState` = `Normal`       
        - For a dialog box, it is essential to parent it -- meaning, find out what window is to be its owner and pass that to the call to `ShowDialog` that shows it.  Otherwise, during the method that displays it, if the owner window cannot be identified, or, say, it is being shown by a Windows Service, then set its `StartPosition` property to `CenterScreen`, but do that at runtime, not at design time.
        - For a dialog box, there are **OK** and **Cancel** buttons, neither of which have an underlined letter.  If such a button has any word other than those, then it can have an underlined letter.  The **OK** button has a `DialogResult` of `OK`, and the **Cancel** button has a `DialogResult` of `Cancel` and its `CausesValidation` property is also set to `false`.  
        - The dialog itself must have the **OK** button (I prefer to set is `Name` property to `okayButton`) set as its `AcceptButton` button, and the **Cancel** button (I prefer to set its `Name` property to `cancelButton`) should be set as the `CancelButton` property of the dialog box's form.  The **OK** button must always be the second-to-last entry in the dialog's tab order, followed by the **Cancel** button.  FYI: Sometimes, a dialog box does not care whether the user wishes to continue.  Case in point: an "About" box, whose single goal in life is merely to display static information.  Such a dialog can have just a single **Close** button, with a caption of "&Close" and name of `closeButton` which is both the `AcceptButton` and `CancelButton`. A **Close** button must have its `DialogResult` property set to `Cancel` and its `CausesValidation` property set to `false`.  It is not necessary to attach a `Click` event handler to a **OK**, **Cancel**, or **Close** button, since merely setting the `DialogResult` property of such a button is enough to tell Windows Forms that that button, when clicked by the user, sets the `DialogResult` property of the form to the same thing and then closes the form.  We can use the `OnFormClosing` override to trap the `DialogResult` of `OK` and invoke validation logic.
        - Never attach event handlers to the event(s) a `Form` itself will raise.  Just override the appropriate `OnXXX()` `protected` `virtual` methods that `System.Windows.Forms.Form` defines, which raise the corresponding event(s); just
        make sure to always allow the base-class version to run first.
        - When a method has a `object sender` and/or an `EventArgs e` (or `XYZEventArgs e`) parameter(s), each of these should be individually decorated with the PostSharp `[NotLogged]` attribute.
    - Lately, I've taken a keen interest in designing Windows Forms apps that can serve as standalone tools I can
      launch and use to enhance my own experience as a user of Microsoft Visual Studio.
    - The interest in in producing them as Dark-theme application(s) if I am using them as a developer or if they
      are intended for use by sophisticated professionals, such as myself.
        - Otherwise, they can be Light-themed (i.e., use the default Windows system colors).
    - I also like creating Windows Forms tools that I can potentially then market as freeware and/or software tools for enterprises, developers, and/or the general public
    
2. Template Wizard DLLs
     - I.e., that host implementations of the `Microsoft.VisualStudio.TemplateWizard.IWizard` interface
     - These can be integrated with `.vstemplate`-based project template(s) to quickly design software systems and components that I can then integrate into new and/or existing software systems.
     - Typically, I like to build the UI/UX of such tools in Windows Forms 2.0, but with a look that matches the look and feel of Visual Studio IDE features and functionality as much as possible, for a clean, consistent, and well-defined user experience that matches the user experience that already ships with Visual Studio, as much as is possible.
     
3. Autonomous/heuristic classic AI systems
     - For instance, I might be interested in creating a Windows Service style software system that can look at data as it streams by, onto which I can then impress my own ways of approaching mission-critical problems and scenarios and/or my expertise in math, sciecne, and statistics that I have gained from both my education as a Ph.D. Physicist and my own experience in the world.
     - Kind of a "Lt Cmdr Data" of software that androidally puts my own brain in the computer but for a specific data application such as performing management of investment portfolio(s) or such.
     - Or I might wish to write a "always keep the various software application(s) and tool(s) installed on my computer updated with the latest version(s)" sort of background windows service that, say, would run on a schedule overnight to go out to the web and scrape websites and download the latest version of, e.g., Notepad++, Zoom for Windows client, and such.
     
4. LINQPad scripts
     - For some of my more larger software systems, even a simple task such as managing NuGet packages might be time consuming.  Thus, it would be imperative to write, e.g., a script to iterate over my Solution(s) that are all in a certain directory and then generate a `.ps1` script, say, that can then be invoked using DTE automation in the Package Manager Console, once per Solution, in order to manage my NuGet packages for me while I, say, go pet my dog, eat dinner, sleep, go to the mall and etc.
     
5. Fully integrated environment applications that mimic the look and feel of the Visual Studio Development Environment
    - My favorite framework(s) to utilize for this sort of application are .NET Framework 4.8, C# 7.3, and Windows Forms 2.0.  I like to also make use of the `WeifenLuo.DockPanelSuite` series of NuGet package(s) so I can basically create my own "IDEs" but for different functionality and productivity application(s), such as a replacement for BareTail or klogg but one whose look and feel strongly resembles that of Visual Stuio 2019 or 2022 or 2026 etc.
    - Or perhaps a consulting client of mine might wish to emulate the look and feel of Microsoft Management Console or something because, for security reasons they cannot have a Web-based software application but they need something that runs on users' desktops in-house.  Perhaps they manage lots and lots of confidential customer data for use in a call center.
    - Perhaps there might be an application like this that I would write to manage my own freelancing LLC business where I try to make use of 3rd-party vendors (with clients' permission) and employees to scale the business but still oversee everything as the chief technologist.  Some sort of integrated CRM + ERP thing but that is built around a consulting and professional services business as opposed to a products business.
    
6. Web-Based Applications such as using ASP.NET MVC and/or Blazor
    - This is my least-developed software system type and also the one with which I have the least experience.  But I have been running, off and on, a field-service tutoring business of my own where I make appointments to visit folks in their homes and such to tutor their children in their homework one on one and there is of course, customer service that has to be handled surrounding such appointments.  It might be handy to write a website and mobile app that are intergrated together (I am not saying I've done this, only that I'd like to) so I can advertise online about my tutoring services, have clients sign up online and then, when I go tutor them either at their home or Starbucks, they can sign my phone and pay right there on my phone or tablet for the session and get an email receipt.  This is a future idea.
    
### 0.2) Special emphasis on helping "future me" and others

1. The idea is that, whatever code I may write, I'd like to make the software system as easily maintainable and well-documented as I can, so that I do not end up having to try and remember what I wrote or why I wrote it down the road.   `README.md` and other `.md` files, XML documentation, using tools that create XML documentation and then translate it into other documentation formats, are key.  This way, if I have to pause working on a particular software system for months or years, I can come back to it and easily pick up where I left off.

## 1) Target stack, constraints, and tooling

### Hard constraints
- **Language:** C# **7.3**
- **Runtime:** **.NET Framework 4.8**
- **UI:** **Windows Forms 2.0**
- **NuGet:** Prefer `packages.config` versus `PackageReference` ALWAYS.
    - ONE exception: say, if the project itself is a VSIX extension project or other project type that absolutely requires `PackageReference`.
- **.csproj files:** Prefer legacy MSBuild XML versus newer project formats (SDK-style).
- **.sln files:** Prefer legacy, non-`.slnx` formats for updating/modifying/creating solution files.
- Do not use language/runtime/UI features outside these versions and constraints.

### C# 7.3 guardrails (do not use newer language features)
- Do NOT use language features introduced after C# 7.3.
- Examples of disallowed syntax/features: 
    - "using var" declarations;
    - Switch expressions;
    - Indices/ranges;
    - Nullable reference types;
    - Default interface members;
    - Records;
    - Init-only setters;
    - Top-level statements;
    - File-scoped namespaces;
    - Global `using` directives etc.
- If a prompt seems to require a newer feature, provide an equivalent C# 7.3 / .NET Framework 4.8 approach instead.

### Libraries and versions (assume these are available)
- NUnit **4.3.2**
- Vsxmd **1.4.5**
- AlphaFS **2.2.6**
- Newtonsoft.Json **13.0.3**
- log4net **3.0.3**
- PostSharp **2024.1.6**

### IDE context
- Visual Studio 2022 Enterprise **17.14.23** and up.
- CodeMaid + JetBrains ReSharper Ultimate installed
- GitHub Copilot assists with code + commit messages

## 2) Architectural & project organization rules ("modules")

### Module concept
A **module** is a group of related C# class libraries. Project names determine namespaces.

- **No project folders.** Meaning, no grouping projects into "solution folders" in the Solution.  "Solution Folders" are anathema.  Of course, each project and its member(s) should be in their own folder on the file system, always directly under the folder that contains the `.sln` file of which it is a member (unless it's a project that is being included from a different Solution entirely).
- Assume `namespace` **exactly matches** the `.csproj` project name.

### Project naming convention within a module
Projects are named with a root and optional suffix from:
- `.Actions`
- `.Constants`
- `.Common`
- `.Displayers`
- `.Events`
- `.Extensions`
- `.Factories`
- `.Helpers`
- `.Interfaces`
- `.Tests`
- `.Win32`

Not every module must contain every suffix; include only what is required.

### Responsibilites by project suffix
- `MyModule`  
  Concrete classes and abstract base classes ONLY.
- `MyModule.Actions`  
  Static "action" classes, **verb-named**, 1-2 words, PascalCase. Method names complete the phrase.  
  Example: `Format.FileAsImage(...)`.  Think: verb-subject. 
  Prefer functional-programming style with clear inputs/outputs.  Always code to an interface.
  All "action" classes are always to be declared `public static` and shall always declare the
  `static` constructor decorated with a `[Log(AttributeExclude = true)]` attribute when using
  PostSharp.  Include detailed XML documentation for such a constructor that explains what it 
  does and why it's being declared.  The whole point is to stop the static constructor call from
  being emitted to log files.  This requirement is nullified if the static constructor actually
  does something inside itself --- then, of course, we wish to log it.
- `MyModule.Constants`  
  Only `public const <type>` members and `enum` declarations. 
  All `enum`s and other type(s) must always be declared `public` and have plenty of comprehensive
  XML documentation in my preferred style (see below).
  Enum conventions:
    - Enum members should be in alphabetical order.
    - Do not assign explicit values, except: the final member must be `Unknown = -1`.
    - The enum member list must end with `Unknown`.
    - Each enum and each enum member must have XML documentation.
  Name constants classes by the **nominal level/category** of info they expose.
  Constants classes are always to be declared `public static` and shall always declare the
  `static` constructor decorated with a `[Log(AttributeExclude = true)]` attribute when using
  PostSharp.  Include detailed XML documentation for such a constructor that explains what it 
  does and why it's being declared.  The whole point is to stop the static constructor call from
  being emitted to log files.  This requirement is nullified if the static constructor actually
  does something inside itself --- then, of course, we wish to log it.
- `MyModule.Common`
  Static "action" classes, **verb-named**, 1-2 words, PascalCase. Method names complete the phrase.  
  Example: `Format.FileAsImage(...)`.  Think: verb-subject. 
  Prefer functional-programming style with clear inputs/outputs.  Always code to an interface.
  The `MyModule.Common` library is different from the `MyModule.Actions` library in that the
  `MyModule.Actions` libraries are intended to be the "front door" of the module to clients; whereas
  `MyModule.Common` class libraries are intended to group together that functionality that is
  commonly used by all other class library(ies) in the same module.  All classes must still be
  `public`.
- `MyModule.Displayers`  
  Typically a single `public static class Display` class that shows secondary windows/forms/dialogs,
  in the same "action-class" fluent style.  Such a library is a "front door" for a module that 
  implements sophisticated functionality for secondary windows/forms/dialogs.  In this case, such a
  library should be preferred over `MyModule.Actions`.
- `MyModule.Extensions`  
  Only extension classes: `public static class <Type>Extensions` containing extension methods only.
- `MyModule.Events`  
  Only `delegate` declarations (e.g., `XYZEventHandler`) and corresponding `EventArgs`-derived classes.
- `MyModule.Factories`  
  Static factory classes. Common patterns:
  - `GetXYZClass.SoleInstance()` to alias `.Instance` of a singleton.
  - `MakeNewXYZClass.FromScratch()` to create new objects.
  - `MakeNewXYZClass.ForABC([NotLogged] ABC abc, ...)` when the class to be created's constructor takes
    argument(s).  Such a method should handle the argument(s) / throw the same exception(s) as the
    constructor would, so as to fail early.
  - Strategy factories:
    - `GetHairDryer.OfType(HairDryerType type)` for singleton strategies and `MakeNewHairDryer.OfType(HairDryerType type)` for multi-instance strategies; the factory method should be named in a fluent, "verb subject" style and should vary with the name of the strategy `enum`.  For example, `MakeNewHairDryer.OfType(HairDryerType type)` or `MakeNewBalloon.HavingColor(BalloonColor color)` so we have a nice, self-documenting codebase.
    The parameter name should, more often than not, just be the last word of the initial-caps `enum`
    name, but all lowercase.  If something else is more descriptive, then go with that.
- `MyModule.Helpers`  
  Helper/utility classes shared across the module.  Different from the `MyModule.Common` class library in that these can be more "utilities," shared across the module and used by clients of the module.
- `MyModule.Interfaces`  
  ONLY C# Interfaces exposed by the module.
- `MyModule.Tests`  
  ONLY NUnit test fixtures.
- `MyModule.Win32`
  ONLY P/Invoke signatures, helper static methods, `NativeMethods` class(es), and Windows struct(s), etc. to support the P/Invoke --- for the sole purpose of assisting our code in accessing the Win32 API.
  
Modules and their component class library(ies) can repeat the naming conventions a la Matroyshka dolls; for instance, `MyModule.Tests.Actions.Constants` are constants and `enum` that supports an "action class" class library, `MyModule.Tests.Actions`, that in turn, supports a unit test library, `MyModule.Tests` --- as an example.  The stacked suffixes -- if you will -- form a "supports" relationship with the class library that does not have the last suffix in the chain.

### Strategy Pattern preference
When implementing Strategy:
- Define an interface exposing common events/properties/methods.
- Provide an abstract base class using the "Template Method" Gang of Four Pattern to share services common to all the concrete type(s).
- It, itself, directly implements the interface `abstract`.
- Provide one concrete class per strategy, each inheriting the abstract base class.
- Include an enum (in `.Constants`) listing strategies.
- Each strategy class exposes a property of that enum type and initializes it to the corresponding value.  The property is named semantically according to the name of the enum, such as `HairDryerType Type { [DebuggerStepThrough] get; }` or `BalloonColor Color { [DebuggerStepThrough] get; }`.
- The interface exposes that property; the abstract base implements it abstractly.
- Concrete class(es) implement the `enum`-typed property as an `override` and they use the static initializer to set its value; i.e., `public override BalloonColor Color { [DebuggerStepThrough] get; } = BalloonColor.Red` is how the property would be declared in the concrete class, `RedBalloon`.  
- All XML documentation must be copied down the object tree from base -> child for all events, methods, and properties that are implemented and/or overriden.

An abstract base class must declare, and suppress logging for, the `static` and `protected` constructors, like the example shown:

```csharp
using System;
using System.Diagnostics;
using xyLOGIX.Core.Debug;

namespace MyNamespace
{
    /// <summary>
    /// Provides a base implementation of the
    /// <see cref="T:MyModule.Interfaces.IHairDryer" /> interface.
    /// </summary>
    /// <remarks>
    /// This class uses the Template Method pattern to provide common functionality
    /// to all hair dryer strategy implementations.
    /// </remarks>
    public abstract class HairDryerBase : IHairDryer
    {
        /// <summary>Initializes <see langword="static" /> data or performs actions that need to be performed once only for the <see cref="T:.HairDryerBase"/> class.</summary><remarks>This constructor is called automatically prior to the first instance being created or before any <see langword="static" /> members are referenced.<para />We've decorated this constructor with the <c>[Log(AttributeExclude = true)]</c> attribute in order to simplify the logging output.</remarks>
        [Log(AttributeExclude = true)]
        static HairDryerBase() { }

        /// <summary>Initializes a new instance of <see cref="T:.HairDryerBase" /> and returns a reference to it.</summary><remarks><strong>NOTE:</strong> This constructor is marked <see langword="protected" /> due to the fact that this class is marked <see langword="abstract" />.<para />We've decorated this constructor with the <c>[Log(AttributeExclude = true)]</c> attribute in order to simplify the logging output.</remarks>
        [Log(AttributeExclude = true)]
        protected HairDryerBase() { }

        /// <summary>
        /// Gets the type of hair dryer.
        /// </summary>
        public abstract HairDryerType Type { [DebuggerStepThrough] get; }

        /// <summary>
        /// Dries the hair.
        /// </summary>
        public void DryHair()
        {
            try
            {
                OnBeforeDryHair();
                DoDryHair();
                OnAfterDryHair();
            }
            catch (Exception ex)
            {
                // dump all the exception info to the log
                DebugUtils.LogException(ex);
            }
        }

        /// <summary>
        /// Called before the hair drying operation begins.
        /// </summary>
        /// <remarks>
        /// Derived classes can override this method to perform setup tasks.
        /// </remarks>
        protected virtual void OnBeforeDryHair()
        {
            // Default implementation does nothing
        }

        /// <summary>
        /// Performs the actual hair drying operation.
        /// </summary>
        /// <remarks>
        /// Derived classes must implement this method to provide strategy-specific
        /// hair drying logic.
        /// </remarks>
        protected abstract void DoDryHair();

        /// <summary>
        /// Called after the hair drying operation completes.
        /// </summary>
        /// <remarks>
        /// Derived classes can override this method to perform cleanup tasks.
        /// </remarks>
        protected virtual void OnAfterDryHair()
        {
            // Default implementation does nothing
        }
    }

}
```

Likewise for each concrete class, although the non-`static` constructor is `public`.

This is to make sure PostSharp refrains from logging these constructors.

### Circular dependencies are forbidden
- No circular dependencies between class libraries/projects.
- This also applies to XML documentation `<see cref="..."/>` usage: do not introduce circular refs via documentation.
- If avoiding a circular dependency requires creating a new module/library, say so.
- Generally, if the #solution contains projects whose names do not begin with `Core` or `xyLOGIX`, those projects are higher in the call stack / dependency graph hierarchy than those whose names do begin with `Core` or `xyLOGIX`.  Also, as a rule of thumb, the shorter a project's name, or the fewer period-separated parts its name has, the higher up the call stack / dependency graph it is.  If you are not sure where to add references, then just don't.  Let me handle that interactively with the help of Roslyn and the ReSharper Ultimate functionality (shows me red lightbulbs prompt me where to add references and `using` statements).
- Remember, references can be transitive (i.e., a "reference to a reference") and thus become circular through the transitive chain.  Watch out for that.
- If you get tempted to create a reference to project Y from project X, and then a reference to project X from project Y, stop and double-check, read the codebase, and determine if that is really necessary.
- Check if a reference you want to add is already present before trying to add it again.

Generally, more often than not, before I prompt you, I will have already set up the reference and dependency graph of the Solution the way I want it.  If you're having trouble with references, or it begins to strike you that it may be dodgy, just do not mess with the references or NuGet packages, especially in "Agent" mode...let me just follow along behind you putting those in place as need be.

## 3) Design principles and coding style

### SOLID / DRY / loose coupling
- Follow **SOLID** and **DRY** strictly.
- Prefer **loose coupling** over tight coupling where feasible.
- Prefer the **highest-level interface or abstract base class** that still provides required functionality:
  - Applies to fields, properties, parameters, and return types (especially for "complex" types).

### Avoid magic literals
- Avoid "magic values" (literals) whenever feasible.
- Prefer:
  - `MyModule.Constants` `public const` members (preferred for most things)
  - `enum`s in `MyModule.Constants`
  - `Resources.resx` only when appropriate for the project
- **Do not put UI control text or control values in `Resources.resx`.**

### Prefer `var`, early-gating, and low cyclomatic complexity
- Use `var` aggressively.
- Use `out var` and `ref var` whenever supported and sensible.
- Use pattern matching (C# 7.3) when it improves clarity and reduces casting/branching.
- Prefer early returns/continues by **negating `if` conditions**:
  - In loops: `if (!condition) continue;`
  - In methods: `if (!condition) return result;`
  - Apply De Morgan's laws to come up with logic gates.
     - Meaning: instead of writing an `if` branch: Suppose we desire `if (theSky.IsRed && 
       iHate.Suzy)` to evaluate to `true` in order to run some method's code.  We
       know that De Morgan's laws would say that the negation of that is: `!theSky.IsRed || !iHate.Suzy` so as to logic gate and return early -- but instead of even combining them,
       just put them into separate `if (!theSky.IsRed) return;` and  `if (!iHate.Suzy) return;`
       statement(s).  Basically, since we're ORing them anyway, the first time one of them evaluates
       to `true`, short-circuit the enclosing method or loop that way.
     - Think about what we desire to be true, first, and then apply De Morgan's laws to then
       NEGATE that, so we are now EXCLUDING what we WISH NOT TO BE SO.
- Gate out invalid/unwanted cases as early as possible.
- Watch your assumptions.  For instance, the method:

```csharp
int calcAreaOfRug(int width, int length) => width * length;
```

in my mindset, is very poorly written.  This is because `int` can have a negative or a positive value.  However, I have never seen a rug with zero or negative width or length; have you?  So we also have to be mindful of what value(s) variable(s) and method input(s) are CAPABLE of having but are not DESIRED to have, or which would not be LOGICAL, given the use case, for them to have; and then gate against those cases accordingly.  That is, the ideal version of the `calcAreaOfRug` method would be:

```csharp
int calcAreaOfRug(int width, int length)
{
    if (width <= 0) return -1;      /* or some nonsense value, which should be documented */
    if (length <= 0) return -1;
    
    return width * length;
}
```

In this version, we have a guarantee, by the time we do `return width * length`, that both `width` and `length` are greater than zero; as should be the case for all rugs, in our example.  We have to think about the real-world and physically-viable possibilities of our use cases and applications when gating.  Think to yourself: what makes sense, and what doesn't make sense?  Gate against what doesn't make sense, so that what makes sense is what is executed.  Watch your assumptions, and gate against them.

- Minimize cyclomatic complexity.

### No regions
- Do not use `#region` / `#endregion`.  EVER.  Or I shall be very cross with you.

### Mathematical programming

For a mathematical equation, formula, or algorithm, consider the properties (I mean, the mathematical properties) of the things you are working with, to include linear-algebraical and abstract-algebraical properties, such as the properties of matrices, tensors, operators, operations, functions, maps, relations, linear transformations, groups, rings, fields, vector spaces, sets, measures, and so on.  It also goes without saying, that if the things we're working with are part of a division ring (as in a set that is not a field, but where the operation of multiplication has the concept that you can multiply two things and get zero as the answer), then we need to gate against zero divisors.  It goes without saying, that all fields (such as the set of real numbers) are division rings.  It also goes without saying that all vector spaces have to be defined over a field; meaning, the vector components' values are themselves elements of the field.

## 4) Defensive programming ("shift-left"), validation, and fault tolerance

### Shift-left mindset (SEU/hardening)
Do not assume values are valid:
- Validate inputs eagerly and bounds-check indices.
- Validate properties/fields/variables before use (e.g., length must be > 0 where meaningful).
- Validate the return values of called methods. If a callee result is not useful, the caller should give up early.  The documentation and XML documentation of methods should be very clear about what they return on failure, if anything.  Read this documentation for methods that you call.
- Check existence of files/folders before use/search.
    - When using `Does.FileExist` and `Does.FolderExist` from the `xyLOGIX.Core.Files` module, bear in mind that these methods always check whether `string.IsNullOrWhiteSpace` on their arguments; thus, it is completely unnecessary to gate against `string.IsNullOrWhiteSpace` just before a call to any of the methods in the `xyLOGIX.Core.Files.Does` class, period.
- Validating and gating is especially important for multithreading.

SEUs are single-event upsets; it's well-known to the astrophysics community that, for instance, a Beryllium nucleus can (and do, very frequently) come screaming in from beyond the Triangulum galaxy and intersect with the micro-circuitry of a computing machine, and sometimes this can alter the values of pointers, variables, CPU registers, and such in a very random and unpredictable way; thus, we should never make assumptions as to what values our variables, method parameters, and such have.  Although, strike a balance between code readability/maintainability and guarding/gating against SEUs.

### Input validation rules
- Any parameter that can be `null` must be checked for `null` -- well, except for value types other than `string`.  `string` is always checked using `string.IsNullOrWhiteSpace`.
- **Do NOT use `||` or `&&` inside eager-returning input-validation `if` statements.**
  - Each validation must be its own `if` with its own early return.

Example pattern:
```csharp
if (someReference == null)
    return result;

if (index < 0)
    return result;

if (index >= list.Count)
    return result;
```


### Exception handling preference (robust, returns defaults)

* Prefer methods that return **default/failure values** over throwing.
* Every method body should be wrapped in a `try`/`catch` per the template below (where applicable).
* On exception:

  * Log it
  * Set the return value to default again
  * Return safely

#### Required logging call style

* Before calling `DebugUtils.LogException(ex);`, insert this comment **on the line above**:

  * `// dump all the exception info to the log`
  
* Never call `DebugUtils.LogException` with its second parameter explicitly specified; it's defined to have a Boolean parameter as the second parameter, but that should be reserved to always be set to its default value, implicitly.  When I am debugging, I will sometimes go in and then change its explicit value but only on a case-by-case basis; leave that to me.

#### Required using

* Add `using xyLOGIX.Core.Debug;` to the **very top** of the file (where available/valid for the project).
* Ensure all necessary `using` statements exist for referenced types.

### Required return-value pattern: `result`

* For non-`void` methods: define a `result` variable at the top, initialized to a default `invalid/failure` value.
* If a catch occurs, reset `result` to default and return it.

Canonical method template:

```csharp
using xyLOGIX.Core.Debug;

public int DoSomething(int value)
{
    var result = -1;        /* or zero, depending on context */

    try
    {
        if (value < 0)
            return result;

        // ...work...

        result = value + 1;
    }
    catch (Exception ex)
    {
        // dump all the exception info to the log
        DebugUtils.LogException(ex);
        
        result = -1;
    }

    return result;
}
```

* Return `-1` from a method declaring an `int` return type to mean "failure" or "I couldn't compute it."  
* Return `0` from a method returning an `int` return type to mean "there is a zero count of items."

* Only initialize a default-return-value, `result`, with the `default` keyword if it is a reference type such as a `struct`, `class`, `union`, `interface`, `delegate` or collection.  More often than not, the default return value of methods that return a collection will be initialized to the empty version of that collection.  Always return collections typed via the corresponding interface; i.e., `IList<T>`, `ICollection<T>`, `IDictionary<K, V>`, etc.

* If a method has no logic gates, simply declare the `result` variable above the `try` block but do not initialize it.

* All logic gates must always `return result;` or `return;` for a void function; do not return scalar values.

* Never explicitly say, `return true;`, `return false;` or `return null;`.  If you need to be explicit about the return value of a logic gate and that gate's return value is something other than the default value of `result`, initialize result BEFORE the gate, and then gate, and then restore the value of `result` following the gate.

> DO NOT do:

```csharp
bool Foo(int myParam1, double myParam2)
{
    var result = false;
    
    try
    {
        if (myParam1 == 3)
        {
            result = true;      // NO!!!
            return result;
        }
    
        /* ... */
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

Instead, DO:

```csharp
bool Foo(int myParam1, double myParam2)
{
    var result = false;
    
    try
    {
        result = true;
        
        if (myParam1 == 3)
        {
            return result;
        }
        
        result = false;
    
        /* ... */
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

> If a method must be `void` due to an override/interface/event-handler signature, still validate inputs and wrap the body in `try/catch`, but do not invent a meaningless `result` variable.  Simply `return;` from logic gates.

> Methods that are `void` must NEVER end with a `return;` statement.

### 4.1) Refined Return Value Pattern by Type:

#### **For `string` Returns:**
```csharp
[return: NotLogged]
public string GetName()
{
    var result = string.Empty;  // NOT default, NOT null
    
    try
    {
        // ...work...
    }
    catch (Exception ex)
    {
        // dump all the exception info to the log
        DebugUtils.LogException(ex);
        
        result = string.Empty;  // NOT default, NOT null
    }
    
    return result;
}
```

#### **For Interface/Object/Reference Type Returns:**
```csharp
[return: NotLogged]
public IMyInterface GetInstance()
{
    var result = default;
    
    try
    {
        // ...work...
    }
    catch (Exception ex)
    {
        // dump all the exception info to the log
        DebugUtils.LogException(ex);
        
        result = default;
    }
    
    return result;
}
```

and

```csharp
[return: NotLogged]
public MyClass Foo()
{
    var result = default;
    
    try
    {
        // ...work...
    }
    catch (Exception ex)
    {
        // dump all the exception info to the log
        DebugUtils.LogException(ex);
        
        result = default;
    }
    
    return result;
}
```

#### **For Primitive Returns (`int`, `bool`, etc.):**
```csharp
public int GetCount()
{
    var result = -1;  // or 0, depending on semantics
    
    try
    {
        // ...work...
    }
    catch (Exception ex)
    {
        // dump all the exception info to the log
        DebugUtils.LogException(ex);
        
        result = -1;  // or 0
    }
    
    return result;
}
```

For primitive returns, we'll explicitly set the `result` variable to some nonsensical value that would indicate failure, empty list, zero count, or some other 'bad' condition that we do not desire.

## 5) PostSharp logging exclusions and `[NotLogged]` rules

### Important correction about `[return: NotLogged]`

* **Do NOT apply `[return: NotLogged]` to properties.**
  A `GlobalAspects.cs` exists in each project excluding logging on property getters/setters and event add/remove methods.
* **Apply `[return: NotLogged]` to methods only** when the return type is not a C++-primitive (definition below), and also for `string` returns.  `enum` return values are logged.

### Definition: "primitive" for logging rules

When these instructions say "primitive," it means "would have been primitive in C++."

Treat these as primitives (do NOT require `[NotLogged]` by default):

* `bool`, `char`
* `byte`, `sbyte`
* `short`, `ushort`
* `int`, `uint`
* `long`, `ulong`
* `float`, `double`
* `enum`

Treat as **non-primitive/complex** (DO require `[NotLogged]` on parameters and/or `[return: NotLogged]` on method returns):

* `string` (explicitly required even though it has a C# keyword)
* `object`
* Any `struct` (e.g., `Guid`, `DateTime`, `Rectangle`, `decimal`, `Nullable<T>`, etc.)
* Any class/interface type
* Any type not used as a scalar in the abstract-algebra sense
* Any collection

### Method parameter rules

* Any method parameter whose type is **not** a C++-primitive must be annotated:
  * `([NotLogged] TypeName parameter)`
* `string` parameters MUST be `[NotLogged]`.
* Value types like `Rectangle`, `Guid`, etc. MUST be `[NotLogged]`.
* Any parameter of type `object` MUST be `[NotLogged]`.

Example:

```csharp
public void Save([NotLogged] string path, [NotLogged] Guid id, int retryCount)
{
    // ...
}
```

### Method return rules

* If a method returns anything other than a C++-primitive, decorate the method with:

  * `[return: NotLogged]`
* `string` return values MUST use `[return: NotLogged]`.
* `object` return values MUST use `[return: NotLogged]`.
* `enum` return values MUST NOT use `[return: NotLogged]`

Example:

```csharp
[return: NotLogged]
public string GetName()
{
    var result = default(string);
    // ...
    return result;
}
```

### Static constructors

* Every `static` class should define a `static` constructor with:

  * `[Log(AttributeExclude = true)]`
  * Purpose: avoid `.cctor` calls being logged.

## 6) XML documentation (100% coverage) - rules and templates

### Coverage requirements

Every one of these must have XML documentation, regardless of access modifier:

* `class`, `struct`, `interface`, `enum`
* methods
* events
* fields
* properties
* constants
* parameters
* return values (where meaningful)

No code entity should exist without XML documentation.

### Do not modify existing docs

If XML documentation already exists, **do not change it**.

### Method docs MUST include `<remarks>`

Method XML docs should include `<remarks>` describing:

* Caller-relevant behavior
* Alternate code paths
* What happens if invalid values are provided (out-of-bounds index, `null`, blank/empty strings, invalid references, etc.)

### Sentence separation rule

Adjacent sentences in XML docs must not be separated by whitespace.
Use self-closing `<para />` tags between adjacent sentences.

Example:

```xml
/// <summary>Does X.<para />Returns Y.</summary>
```

### `langword` usage for C# keywords

* Use `<see langword="null" />`, `<see langword="true" />`, `<see langword="false" />`; **not** `<c>null</c>`, etc.
* If XML docs refer to a C# language keyword specifically, wrap it with `<see langword="..." />`.
* If the word is used in plain English and not referring to the keyword, do not use `langword`.

### Cross-reference rules (`<see cref="..."/>`)

* Always use **fully-qualified** names in `cref`.
* Use the correct prefix:

  * Types: `<see cref="T:Namespace.TypeName" />`
  * Methods: `<see cref="M:Namespace.TypeName.MethodName(ParamType,ParamType)" />`
  * Properties: `<see cref="P:Namespace.TypeName.PropertyName" />`
  * Events: `<see cref="E:Namespace.TypeName.EventName" />`
  * Fields/constants/enum members: `<see cref="F:Namespace.TypeName.FieldName" />`
* Use `<see cref="F:System.String.Empty" />` and `<see cref="F:System.Guid.Empty" />` when referencing those values.
* Use `<paramref name="paramName" />` only when referring to the parameter (not when using the same word generically).

### When NOT to use `<see cref="..."/>`

If a term looks like a code entity but cannot/should not be referenced (e.g., file names, source-level attributes like `AssemblyTitle`, etc.), wrap it in `<c>...</c>`.

Examples:

* `<c>AssemblyInfo</c>`
* `<c>AssemblyTitle</c>`

### Required `(Required.)` / `(Optional.)` parameter doc prefix

Every `<param name="...">` body must start with:

* `(Required.)` or `(Optional.)`

Example:

```xml
/// <param name="e">(Required.) A <see cref="T:System.Windows.Forms.MouseEventArgs" /> that contains the event data.</param>
```

### Reference vs value semantics in wording

* Do **not** say "Reference to an instance of Xxx" for value types and "primitive keyword types" (including `string`, `Guid`, `DateTime`, `int`, etc.). Prefer:

  * A Xxx value as in, `A <see cref="T:System.Guid" />` value
  * `An <see cref="T:System.Int32" /> value that `
  * `A <see cref="T:System.String" /> that `
* For non-primitive reference types (typical classes/interfaces), prefer:

  * `Reference to an instance of <see cref="T:Fully.Qualified.Type" />`

### Inline code in XML docs

When reproducing code or comments inline, always wrap with `<c>...</c>`.

Example:

* `<c>// dump all exception info to the log</c>`

### Generic type documentation preference

When documenting generic types, prefer **backtick + arity** notation and keeping type references fully-qualified (e.g., ``System.Collections.Generic.IList`1``) over curly-brace notation, where the "arity" refers to the number of generic type parameter(s) in its definition.

### Backing-field documentation requirement

If a field backs a property, field docs must:

* In `<summary>`: explain the purpose of the property and the field.
* In `<remarks>`: include:

  * `<b>NOTE:</b> The purpose of this field is to cache the value of the XYZ property`
  * Where `XYZ` is a `<see cref="P:Fully.Qualified.Property" />` reference.

### Interface summary requirement

For interfaces, the `<summary>` tag MUST begin with:

* `Defines the publicly-exposed events, methods and properties of ___`

### `If _, then` phrasing

In XML docs, prefer:

* `If ___, then ___.`
  Not:
* `If ___, ___.`

### Pluralization style in docs/comments/UI text

When a word may be plural, pluralize by parenthesizing only the plural part:

* discovery(ies), journey(s), attribute(s), box(es)
  Do not overuse; keep correct U.S. English grammar.
* Do not add parenthetical plurals where they do not belong (e.g., "continues", not "continue(s)"). Use judgment.

### Vsxmd output awareness

XML documentation is converted into `README.md` via Vsxmd, and fully-qualified `<see cref="..."/>` links should help produce Microsoft Learn-style hyperlinks. Take every reasonable opportunity to provide correct cross-references.

### Special `DebugUtils.LogException` cref signature

When referencing the overload in XML documentation, use the fully-qualified signature:

* `xyLOGIX.Core.Debug.DebugUtils.LogException(System.Exception,System.Boolean)`
  Example:

```xml
/// <see cref="M:xyLOGIX.Core.Debug.DebugUtils.LogException(System.Exception,System.Boolean)" />
```

## 7) Properties, fields, and accessors

### Accessor body rules

* Do NOT use expression-bodied properties or expression-bodied accessors.
* If you write explicit accessor bodies, they must be statement-bodied (no expression-bodied accessors).
* Every getter and setter must be annotated at the accessor level with `[DebuggerStepThrough]` (if not already).
* Ensure `using System.Diagnostics;` exists when using `[DebuggerStepThrough]`.

### Auto-properties

* Auto-properties are preferred when feasible. Use accessor attributes like:

```csharp
public int Count
{
    [DebuggerStepThrough] get;
    [DebuggerStepThrough] set;
}
```

* Always decorate accessors with [DebuggerStepThrough] and make sure `using System.Diagnostics` is near the top of the file.
* Favor the use of getter-only auto properties (vs. auto-properties with a protected or private setter) when they are only initialized at construction time or static construction time.
* Never use the auto-property accessor `init;`.  

### Backing fields

Only use backing fields when:

* The property update must raise an event, or
* A read-only property value must be cached
   - This is a best practice when a property should be computed once but is going to be read
     frequently.

## 8) Events

### No dead events

Never declare/expose an event that is never invoked.

### Required `OnXxx` pattern

For each event `Xxx`, provide:

* `protected virtual void OnXxx(...)` which fires the event:

  * `Xxx?.Invoke(this, e);` (or appropriate signature)
* Call `OnXxx` wherever the event should fire.

### Custom `EventArgs`-derived class template

The following C# source code listing is how a custom `EventArgs`-derived class should be declared:

```csharp
using PostSharp.Patterns.Diagnostics;
using PostSharp.Patterns.Threading;
using System;
using System.Diagnostics;

namespace MyModule.Events
{
    /// <summary>
    /// Provides data for the
    /// <see cref="E:MyModule.Interfaces.IHairDryer.DryingCompleted" /> event.
    /// </summary>
    [ExplicitlySynchronized, Log(AttributeExclude = true)]
    public class DryingCompletedEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes <see langword="static" /> data or performs actions that need to be
        /// performed once only for the
        /// <see cref="T:MyModule.Events.DryingCompletedEventArgs" /> class.
        /// </summary>
        /// <remarks>
        /// This constructor is called automatically prior to the first instance being
        /// created or before any <see langword="static" /> members are referenced.
        /// <para />
        /// We've decorated this constructor with the <c>[Log(AttributeExclude = true)]</c>
        /// attribute in order to simplify the logging output.
        /// </remarks>
        [Log(AttributeExclude = true)]
        static DryingCompletedEventArgs() { }

        /// <summary>
        /// Constructs a new instance of
        /// <see cref="T:MyModule.Events.DryingCompletedEventArgs" /> and returns a
        /// reference to it.
        /// </summary>
        /// <param name="success">
        /// (Required.) Value indicating whether the hair was dried successfully.
        /// </param>
        [Log(AttributeExclude = true)]
        public DryingCompletedEventArgs(bool success)
        {
            Success = success;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the drying operation was successful.
        /// </summary>
        public bool Success
        {
            [DebuggerStepThrough] get;
        }
    }
}
```

We always initialize getter-only properties from the ctor.  We never use the member initializer list when calling the `OnDryingCompleted` event-invocator method (as it would be named); we pass the value through the ctor.

### Sealed classes

If you add new `virtual` methods to a `sealed` class, remove the `sealed` modifier.

## 9) WinForms/UI and form and dialog control-layout conventions (Win32-minded, classic UX)

* Follow "The Windows User Interface Guidelines for Software Design" (Microsoft Press, 1995) where possible.
* Prefer Windows 95 UI/UX conventions where they conflict with later guidance.
* Never add `MenuStrip`, `ToolStrip`, or `StatusStrip` to a form with `FormBorderStyle = FixedDialog`.

### Base form and form interfaces

* In the event that class library(ies) beginning with `xyLOGIX.UI.Dark` are included in the #solution, then all Windows Forms and dialog boxes must derive from:

  * `xyLOGIX.UI.Dark.Forms.DarkForm`
  
otherwise they can derive from `System.Windows.Forms.Form` as per usual.
  
* In the event that class library(ies) beginning with `xyLOGIX.UI.Dark` are included in the #solution, then form's interface should inherit:

  * `xyLOGIX.UI.Dark.Forms.IDarkForm`
  * (So the interface can expose only app-specific functionality.)
  
otherwise, they can inherit `xyLOGIX.Core.Extensions.IForm` which exposes all the usual methods and properties that a Windows Form has (`xyLOGIX.UI.Dark.Forms.IDarkForm` already inherits `xyLOGIX.Core.Extensions.IForm`, FYI).
  
The `xyLOGIX.UI.Dark.Forms.IDarkForm` interface implements `xyLOGIX.Core.Extensions.IForm` which exposes all the usual methods and properties that a Windows Form has.   All form controls have `DarkXxx` counterparts in the `xyLOGIX.UI.Dark.Controls` library, such as `DarkCheckBox` instead of `CheckBox` and so on.  If a `DarkXxx` counterpart doesn't exist, then use the normal `System.Windows.Forms.Xxx` version of it.  If dark controls are used, the `xyLOGIX.UI.Dark.Controls`,  `xyLOGIX.UI.Dark.Controls.Interfaces`, and `xyLOGIX.UI.Dark.Controls.Themes.Interfaces` class libraries and namespaces must be references.

If no class library(ies) beginning with `xyLOGIX.UI.Dark` are included in the #solution, then forms and dialog boxes can inherit from `System.Windows.Forms.Form` as per usual.  

### Control exposure rule

If a form exposes controls to external clients:

* Expose them via properties in the form's main `.cs` file, not in `.Designer.cs`.

If UI elements are referenced in XML documentation, then the name of the UI element must be in Title Case (without any occurrence of the '&' mnemonic character) and enclosed within a `<b>...</b>` tag -- mimicking the style of the Microsoft docs.  The title of a form, if known at design/documentation time, must always be in Title Case inside a `<b>...</b>` tag when referred to in XML documentation.  Pretend you're authoring Microsoft docs, the way Microsoft would write them, when writing XML documentation.

## 10) Testing conventions (NUnit)

* Use NUnit 4.3.2 for unit tests.
* Prefer one test fixture per concrete class.
* It's OK to build abstract base test fixtures to share behavior across multiple test fixtures using the Template Method pattern.
* In test projects, prefer using `xyLOGIX.Tests.Logging` and derive fixtures from:

  * `LoggingTestBase` (enables PostSharp/log4net logging to file automatically)
  
* Adorn ALL test fixture classes with the `TestFixture` and `ExplicitlySynchronized` attributes.
     - The `TestFixture` attribute comes from NUnit.
     - The `ExplicitlySynchronized` attribute comes from `PostSharp.Patterns.Threading`.
     
* Only if MULTIPLE tests display / interact with a GUI, then add the following attributes to the test fixture at the class level:
    - The `NonParallelizable` attribute, which comes from NUnit; and
    - The `Apartment(ApartmentState.STA)` attribute, which also comes from NUnit.
    
* Apply the `Apartment(ApartmentState.STA)` attribute to the class level if MORE THAN ONE test shows a GUI; if only one test shows a GUI, then drop the `NonParallelizable` attribute and apply the `[Apartment(ApartmentState.STA)]` attribute to the specific test that shows the GUI ONLY.

* If even ONE test shows a Windows Form, then override `LoggingTestBase.OneTimeSetUp` and implement it thus:

```csharp
public override void OneTimeSetUp()
{
    base.OneTimeSetUp();
    
    Application.EnableVisualStyles();
    Application.SetCompatibleTextRenderingDefault(false);
}
```
This will ensure that Windows Forms have the correct look and feel.

It is not necessary to decorate the override with the `[OneTimeSetUp]` attribute as that is done by the base class, `LoggingTestsBase`.  Do NOT add a `// ??? NO [OneTimeSetUp] attribute` comment to the file, nor add a `// ??? Method-level attribute` or any other such comment to the file.

## 11) Performance, threading, LINQ, and enumeration

* Avoid unnecessary materialization (`ToList()`, `ToArray()`) unless it improves performance or correctness.
* Remember: materialization (`ToList()`, `ToArray()`) enumerates the source sequence once.
* Avoid iterating an `IEnumerable<T>` more than once.
* In multithreaded/concurrent scenarios:

  * Avoid LINQ extension methods.  In the .NET Framework 4.8, they are all known by the community to be non-thread-safe and non-concurrent, and their usage must be avoided at all times.
  * Prefer `for`/`foreach` loops and thread-safe algorithms.
  * If enumerating a potentially-changing sequence, snapshot it first (e.g., `ToArray()`), then iterate the snapshot unless the collection is known to be concurrent.
  * Be sure appropriate use of locks are used.  Make sure to avoid nesting locks -- even implicitly nesting them through method calls (i.e., lock, then call a method which itself enters a lock etc.).
  * You may always use `.AsXxx()` LINQ extension methods (i.e., `.AsQueryable()`, `.AsParallel()` etc)
  * Favor the use of `.AsParallel()` or `.AsSequential()` over `Parallel.ForEach` -- choose which based on correctness and performance.
  * NEVER call `.AsParallel()` then `.AsSequential()` in the same sequence (or vice-versa).  Use one OR the other.

## 12) Git/repo conventions and agent behaviors

### Repo root convention

* `.git` folders are typically at the **solution root** (folder containing the `.sln` / `.slnx`).
* Git workflows should be given the solution-containing folder as `repoRoot`.

### Agent mode housekeeping

If operating as an agent:

* Delete dead code when it becomes unused due to architecture changes:

  * Methods, events, fields, properties, interfaces, classes, delegates, and `Resources.resx` entries.
* Do not regenerate:

  * `GlobalAspects.cs`
  * `AssemblyInfo.cs`
  
* We use Git all the time, so it's not a big issue if something is deleted by mistake.  We can always roll the file back.  HOWEVER, don't be stupid about it!  Double-check that you are correct before you delete something.  If you remove code, and then the software breaks or suddenly refuses to build, try putting the deleted code back.  It's possible you broke something by accidentally deleting live code.  I am willing to wait a little extra time for you to double-check yourself.

* Bear in mind that 99.99999% of my software components are libraries, so of course they will have TONS of methods that are "never called" but you are correct, if they are exposed by an interface then they are meant to be called BY CLIENTS.  OK, now let's do the final section, Section 13.

## 13) Framework-first vs flexibility

Before reinventing the wheel:

* Consider whether .NET Framework already provides the needed capability.
* When unsure, consult Microsoft Docs (or other primary/official documentation) before rolling your own solution.
* Consider whether the needed capability already exists elsewhere in the codebase of the current #solution.
* Prefer built-in APIs when they satisfy requirements without brittleness.
* However, overall preference is **flexibility** and **loose coupling** over brittle software coupling when requirements may evolve (which, in my mindset, they are prone to doing often).
* Always code to an interface.  This means, always use an interface for the type of a method parameter, return value, property, or field, instead of a concrete type.  When an interface isn't available, code to the nearest abstract base class.  If that is not available, always code to the highest object in the IMMEDIATE inheritance graph (except do not code to `System.Object` unless you have to, say, when doing COM programming, because everything in the .NET Framework inherits `System.Object` but we do not want code with `System.Object` being the type of every method parameter, field, return value, and property; that's dumb.  Use common sense).
* Never produce XML documentation like the following:

```csharp
/// <summary>
/// Initializes the manager for the main application tab control.
/// </summary>
/// <remarks>
/// This method creates an
/// <see
///     cref="T:ControlTestApp.Windows.TabControlManagers.Interfaces.ITabControlManager" />
/// instance for the
/// <see cref="P:ControlTestApp.Windows.MainWindow.DarkTabControl" />.
/// <para />
/// If creation of the manager fails, then the corresponding field remains set to
/// <see langword="null" />.
/// </remarks>
```

The correct version is:

```csharp
/// <summary>
/// Initializes the manager for the main application tab control.
/// </summary>
/// <remarks>
/// This method creates a reference to an instance of an object that implements the
/// <see
///     cref="T:ControlTestApp.Windows.TabControlManagers.Interfaces.ITabControlManager" />
/// interface for the
/// <see cref="P:ControlTestApp.Windows.MainWindow.DarkTabControl" />.
/// <para />
/// If creation of the manager fails, then the corresponding field remains set to
/// <see langword="null" />.
/// </remarks>
```

Notice the `<remarks>` section.  See how it refers to `a reference to an instance of an object that implements the IMyInterface interface` and not `an IMyInterface instance`? I never talk about interfaces as if they are object instances; it's always, "we have a reference to an instance of an object that implements the interface," not "an IMyInterface instance;" that makes no sense in view of how C# objects actually work.

## 14) Proper way to check things with logging

When you're running an `if` check and it has surrounding logging:

```csharp
                DebugUtils.WriteLine(
                    DebugLevel.Info,
                    "AssemblyInfoProvider.Save: *** INFO *** Checking whether the method parameter, 'data', has a null reference for a value..."
                );

                if (data == null)
                {
                    DebugUtils.WriteLine(
                        DebugLevel.Error,
                        "AssemblyInfoProvider.Save: *** ERROR *** A null reference was passed for the method parameter, 'data'. Stopping..."
                    );

                    DebugUtils.WriteLine(
                        DebugLevel.Debug,
                        $"AssemblyInfoProvider.Save: Result = {result}"
                    );

                    return result;
                }

                DebugUtils.WriteLine(
                    DebugLevel.Info,
                    "AssemblyInfoProvider.Save: *** SUCCESS *** We have been passed a valid object reference for the method parameter, 'data'. Proceeding..."
                );

```

There should be mucho comments here.  The version above is incorrect.  Too few comments.  The correct version is:

```csharp
                DebugUtils.WriteLine(
                    DebugLevel.Info,
                    "AssemblyInfoProvider.Save: *** INFO *** Checking whether the method parameter, 'data', has a null reference for a value..."
                );

                // Check whether the method parameter, 'data', is set to a null reference.
                // If this is the case, then write an error message to the log file, and
                // then terminate the execution of this method, returning the default 
                // return value.
                if (data == null)
                {
                    // The method parameter, 'data', is set to a null reference.  This is not desirable.
                    DebugUtils.WriteLine(
                        DebugLevel.Error,
                        "AssemblyInfoProvider.Save: *** ERROR *** A null reference was passed for the method parameter, 'data'. Stopping..."
                    );

                    DebugUtils.WriteLine(
                        DebugLevel.Debug,
                        $"AssemblyInfoProvider.Save: Result = {result}"
                    );

                    // stop.
                    return result;
                }

                DebugUtils.WriteLine(
                    DebugLevel.Info,
                    "AssemblyInfoProvider.Save: *** SUCCESS *** We have been passed a valid object reference for the method parameter, 'data'. Proceeding..."
                );

```

Analyze the differences between these two listings and tell me what you see.  Now, do this (or something similar) every time we're logging in a method and also running `if` checks.

Inside the `if` branch, above the first logging line, the comment above it should always end with a second sentence, `This is not desirable.` if the logging line logs an *** ERROR *** message.  Also, there is not enough logging and comments in the `DetectEncodingWithBomCheck` method.  Do this no matter the purpose nor the modifier of any method, only if the method is decorated with the `[Log(AttributeExclude = false)]` attribute, then you refrain from all logging and of course, it would also not be necessary to heavily comment in this case.