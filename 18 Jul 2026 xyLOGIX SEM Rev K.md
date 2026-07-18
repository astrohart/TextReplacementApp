# The xyLOGIX Software Engineering Manifesto
Revision: K
Last Updated: 18 July 2026

This document outlines the software-development hills we'll die on, here at xyLOGIX.

By Brian C. Hart, Ph.D.
Copyright © 2026 by xyLOGIX, LLC.  All rights reserved.

## Revision K Scope

Revision K consolidates the architectural manifesto with the operational C# development rules previously maintained in `copilot-code-instructions.txt`.  The original Revision H material is preserved below.  The new implementation manual that follows it resolves the instruction gaps and turns the combined guidance into a single reference for designing, writing, documenting, reviewing, and maintaining xyLOGIX software.

## Why xyLOGIX Must Have SOLID Software

We need to make sure our software is SOLID.  SOLID means following a set of five rules that help us write code that is easy to read, easy to fix, and easy to grow.

Think of writing code like building with Lego blocks. The SOLID rules make sure all our blocks can snap together perfectly, even if we add new ones later.

Here is what each letter stands for, along with a simple example.

### **S - Single Responsibility Principle (SRP)**
**What it means:** A source file should only contain one class, interface, struct, union, enum, or delegate declaration. A class, property, or method should only have one job or one reason to change.

If a class, property, or method tries to do too many things, it becomes confusing and easy to break.

**Bad:** A class that holds user data AND prints it.

```csharp
public class User
{
    public string Name { get; set; }
    
    // This is bad! The User class shouldn't worry about printing.
    public void PrintUserReport()
    {
        Console.WriteLine("User Report: " + Name);
    }
}

```

**Good:** Split the jobs into two classes.

```csharp
public class User
{
    public string Name { get; set; }
}

public class UserPrinter
{
    public void PrintReport(User user)
    {
        Console.WriteLine("User Report: " + user.Name);
    }
}
```

Here, the class, `User`, is known as a "POCO", or "Plain Ol' C# Object," and in the context of software systems and database programming, it is also known as a "Data Transfer Object," or "DTO" for short.  The `UserPrinter` class is a "Service Class," and it accepts instances of `User` POCOs and then does things with them.

---

### **O - Open/Closed Principle (OCP)**

**What it means:** Software should be "open" for adding new things, but "closed" for changing old things. You should be able to add a new feature without having to rewrite your existing, working code.

**Bad:** If we add a new shape, we have to change the calculator class.

```csharp
public class AreaCalculator
{
    public double CalculateArea(object shape)
    {
        if (shape is Rectangle)
            return ((Rectangle)shape).Width * ((Rectangle)shape).Height;
        
        // If we add a Circle, we have to change this class!
        return 0;
    }
}
```

**Good:** Let each shape calculate its own area. Now we can add a Circle without ever touching the calculator.

```csharp
public interface IShape
{
    double GetArea();
}

public class Rectangle : IShape
{
    public double Width { get; set; }
    public double Height { get; set; }
    public double GetArea() => Width * Height;
}

public class AreaCalculator
{
    public double CalculateArea(IShape shape)
    {
        return shape.GetArea();
    }
}
```

---

### **L - Liskov Substitution Principle (LSP)**

**What it means:** If you have a parent class and a child class, you should be able to swap them without breaking the program. A child class must act like its parent.

**Bad:** A penguin is a bird, but if our bird class forces all birds to fly, a penguin will crash the program.

```csharp
public abstract class Bird
{
    public abstract void Fly();
}

public class Penguin : Bird
{
    // We have to override 'Fly' but we really do not want to.
    public override void Fly()
    {
        throw new Exception("Penguins can't fly!");
        // This breaks the program!
    }
}
```

**Good:** Make the parent class more general, and only give flying abilities to birds that actually fly.

```csharp
public abstract class Bird
{
    // General bird things
}

public interface IFlyingBird
{
    void Fly();
}

public class Eagle : Bird, IFlyingBird
{
    public void Fly() { /* Flaps wings */ }
}

public class Penguin : Bird
{
    public void Swim() { /* Swims in water */ }
}
```

Notice how `Bird` is `abstract`.  We do this at xyLOGIX because it really does not make sense to make the base class of a "buncha other clases" have a base class that is, itself, instantiatable; why not just have it be some abstract base class that defines all the things that all the birds can do, and defines abstract property(ies) and method(s) to make sure that all its children define what all birds "have to be capable of doing?"

---

### **I - Interface Segregation Principle (ISP)**

**What it means:** Don't force a class to learn rules it doesn't need. Keep your interfaces (lists of rules) small and specific.

**Bad:** A giant interface that forces a robot to eat.

```csharp
public interface IWorker
{
    void Work();
    void Eat();
}

public class RobotWorker : IWorker
{
    public void Work() { /* Builds cars */ }
    
    public void Eat() 
    {
        // Robots don't eat! This is useless.
        throw new Exception("I only drink oil.");
    }
}
```

**Good:** Break the rules into smaller chunks.

```csharp
public interface IWorkable
{
    void Work();
}

public interface IFeedable
{
    void Eat();
}

public class RobotWorker : IWorkable
{
    public void Work() { /* Builds cars */ }
}

public class HumanWorker : IWorkable, IFeedable
{
    public void Work() { /* Writes code */ }
    public void Eat() { /* Eats a sandwich */ }
}
```

---

### **D - Dependency Inversion Principle (DIP)**

**What it means:** Big, important classes shouldn't rely on small, specific classes. Instead, both should rely on a general set of rules (an interface). This makes it easy to swap parts out later.

**Bad:** A computer that is permanently glued to a specific keyboard.

```csharp
public class MechanicalKeyboard
{
    public void Type() { /* Clack clack */ }
}

public class Computer
{
    private MechanicalKeyboard _keyboard;
    // Stuck with this exact keyboard

    public Computer()
    {
        _keyboard = new MechanicalKeyboard();
    }
}
```

**Good:** The computer just asks for *any* keyboard that follows the rules.

```csharp
public interface IKeyboard
{
    void Type();
}

public class MechanicalKeyboard : IKeyboard
{
    public void Type() { /* Clack clack */ }
}

public class Computer
{
    private IKeyboard _keyboard;
    
    // Now we can plug in ANY keyboard!
    public Computer(IKeyboard keyboard)
    {
        _keyboard = keyboard;
    }
}
```

#### Additional Dependency-Injection and Inversion Notes

At xyLOGIX, we hate passing things to constructors if we can at all avoid it -- pretty much, the only exception consists of `XXXEventArgs` class(es) in our code, who have to be initialized with what value(s) they will then expose to their client(s) via their getter-only property(ies).

We express our adoption of DIP in the use of "Singleton Services" (see the section on Uncle Bob's Clean Code, below).

We also need to make sure our software is DRY. DRY means "Don't Repeat Yourself."

If you find yourself copying and pasting the exact same lines of code into different parts of your program, you are creating a trap. Why is this bad? Imagine you find a bug in that copied code later. If you pasted it in five different places, you have to remember to track down and fix all five places. If you forget even one, your software is still broken. It also makes your code much longer and harder to read.

Instead of copying and pasting, the DRY rule tells us to take that matching code and put it into a single "box" (like a method or a function). Then, you just call that box whenever you need it. This way, if you ever need to change how the code works, you only have to change it in one single place.

Here is what it looks like in C#.

##### **The Bad Way (Not DRY)**

In this example, the math for adding tax and printing the receipt is written out twice. If the tax rate changes from 8% to 10%, we have to remember to change it in both methods.

```csharp
public class Store
{
    public void BuyApple()
    {
        double price = 1.00;
        double tax = price * 0.08; // Calculating tax
        double total = price + tax;
        Console.WriteLine("Your total is: $" + total);
    }

    public void BuyBanana()
    {
        double price = 0.50;
        double tax = price * 0.08; // Calculating tax again!
        double total = price + tax;
        Console.WriteLine("Your total is: $" + total);
    }
}
```

##### **The Good Way (DRY)**

Here, we moved the math and the printing step into a single helper method called `BuyItem`. Now, the tax logic is only written once. If the tax rate changes, we only have to update one line of code!

```csharp
public class Store
{
    // We created one method that handles the math for ANY item.
    public void BuyItem(string itemName, double price)
    {
        double tax = price * 0.08;
        // Tax is only calculated here
        double total = price + tax;
        Console.WriteLine("Your total for the " + itemName + " is: $" + total);
    }

    public void Checkout()
    {
        // Now we just reuse the method!
        BuyItem("Apple", 1.00);
        BuyItem("Banana", 0.50);
        BuyItem("Mango", 2.00); // It is very easy to add new items now.
    }
}
```

## Applying DRY to Avoid Doing the Same Thing in Multiple Places in the Code

This is the primary application of DRY.  DRY means "Don't Repeat Yourself."  This also goes for methods or components.  If we're doing exactly the same, or similar, logic gates, `if` checks, algorithms, operations, etc. in multiple different places, then it sometimes is the case that a good appraoch is to encapsulate those centrally in a Singleton component and then depend upon that Singleton component in multiple different places.  If the repitition is limited to the scope of just a single source file, then creating a method that is called by both locations is the better way to go.  Singletons are really only necessary when we're centralizing functionality that is utilized by disparate components across the software system, writ large.

---

## A Synopsis of *Clean Code*

Robert C. Martin’s (affectionately known as "Uncle Bob") book *Clean Code: A Handbook of Agile Software Craftsmanship* is a foundational text in modern software engineering.

Its central thesis is simple but profound: **code is read far more often than it is written.** Therefore, code should be treated like well-crafted prose. It should be intuitive, express its intent clearly, and hide unnecessary details, allowing the reader to understand the system without mental gymnastics.

Uncle Bob argues that "bad code" might work in the short term, but it acts as a swamp that slows down future development (often referred to as "technical debt"). Clean code, on the other hand, is elegant, straightforward, and easy to test.

### Core Themes of Clean Code

* **Meaningful Names:** Variables, functions, and classes should reveal their intent immediately.
* **Small Functions:** Functions should do exactly one thing, and they should do it well. They should be short enough to read in a single glance.
* **Self-Documenting Code over Comments:** Comments are often lies waiting to happen because they aren't maintained when code changes. Clean code expresses itself clearly enough that comments are rarely needed.
* **Error Handling:** Exceptions are preferred over returning error codes, and error handling should not obscure the main logic.

---

## Clean Code in the Purview of SOLID and DRY

Uncle Bob is one of the primary architects of the **SOLID** principles, and he heavily emphasizes **DRY** (Don't Repeat Yourself) throughout his teachings. In the *Clean Code* philosophy, following these rules is what transforms a tangled script into a professional architecture.

Here is how *Clean Code* views good and bad practices through the lens of SOLID and DRY.

### 1. The "God Method" vs. SRP and DRY

In *Clean Code*, Uncle Bob warns against large functions that try to orchestrate too many things, or large classes, say, with a few `public` methods and a huge number of "private helper methods."

Uncle Bob says, when you find yourself with a function that tries to orchestrate too many things, or when you have a class with a few `public` methods and a jillion `private`, `protected`, or `internal` ones, such a function or class violates the **Single Responsibility Principle (SRP)**, and it usually forces developers to violate **DRY** because they can't reuse the tangled pieces of logic.

**The Bad: Tangled Responsibilities (Violating SRP and DRY)**

In this example, the method calculates pay, formats an email, and sends it. It does three jobs. If another part of the system needs to calculate pay, the developer will likely copy and paste the math logic, violating DRY.

```csharp
public class PayrollManager
{
    public void ProcessEmployeePay(string employeeName, double hours, double rate)
    {
        // 1. Calculate Pay (Math)
        double totalPay = hours * rate;
        if (hours > 40) 
        {
            totalPay += (hours - 40) * (rate * 0.5);
            // overtime
        }

        // 2. Format Message (Presentation)
        string message = $"Hello {employeeName}, your pay this week is ${totalPay}.";
        
        // 3. Send Email (Infrastructure)
        Console.WriteLine("Sending email: " + message);
    }
}

```

**The Good: Small, Focused Classes (Following SRP and DRY)**

Clean Code dictates extracting these behaviors into small, focused modules. Now, the math can be reused anywhere (DRY), and each class has only one reason to change (SRP).

```csharp
public interface IPayCalculator
{
    double CalculateTotalPay(double hours, double rate);
}

public class PayCalculator : IPayCalculator
{
    public static IPayCalculator Instance { [DebuggerStepThrough] get; } = new PayCalculator();
    
    public double CalculateTotalPay(double hours, double rate)
    {
        double total = hours * rate;
        if (hours > 40) total += (hours - 40) * (rate * 0.5);
        return total;
    }
}

public static class GetPayCalculator
{
    [DebuggerStepThrough]
    [return: NotLogged]
    public static IPayCalculator SoleInstance() => PayCalculator.Instance;
}

public interface IEmailNotifier
{
    void SendPayNotification(string employeeName, double totalPay)
}

public class EmailNotifier : IEmailNotifier
{
    public static IEmailNotifier Instance { [DebuggerStepThrough] get; } = new EmailNotifier();

    public void SendPayNotification(string employeeName, double totalPay)
    {
        string message = $"Hello {employeeName}, your pay this week is ${totalPay}.";
        Console.WriteLine("Sending email: " + message);
    }
}

public static class GetEmailNotifier
{
    [DebuggerStepThrough]
    [return: NotLogged]
    public static IEmailNotifier SoleInstance() => EmailNotifier.Instance;
}
```

Then, the `PayrollManager` calls upon the `PayCalculator` and `EmailNotifier` as "singleton services" (at xyLOGIX, we support using Dependency Injection and Inversion of Control, but we think using dependency graphs full of classes passing jillions of things into each other's constructors reduces malleability and makes code changes unnecessarily "viral," i.e., when you change a small piece of code, way down in the bowels of the software system, we are of the mindset that typical dependency-injection methodologies and frameworks cause such a change to then make it necessary to change other things all over the rest of the codebase. So, instead of doing (as is often preached):

```csharp
public class PayrollManager
{
    /* ... */
    
    private IPayCalculator _payCalculator;
    
    private IEmailNotifier _emailNotifier;
    
    public PayrollManager(IPayCalculator payCalculator, IEmailNotifier emailNotifier)
    {
        _payCalculator = payCalculator;
        _emailNotifier = emailNotifier;
    }
    
    /* ... */
}
```

; instead, we prefer to use "singleton dependency properties" i.e.,

```csharp
public class PayrollManager
{
    /* ... */
    
    private static IPayCalculator PayCalculator
    {
        [DebuggerStepThrough]
        get;
    } = GetPayCalculator.SoleInstance();
    
    private static IEmailNotifier EmailNotifier
    {
        [DebuggerStepThrough]
        get;
    } = GetEmailNotifier.SoleInstance();
    
    /* ... */
}
```

so, then, the `ProcessEmployeePay` method becomes:

```csharp
public class PayrollManager
{
    /* ... */
    
    private static IPayCalculator PayCalculator
    {
        [DebuggerStepThrough]
        get;
    } = GetPayCalculator.SoleInstance();
    
    private static IEmailNotifier EmailNotifier
    {
        [DebuggerStepThrough]
        get;
    } = GetEmailNotifier.SoleInstance();
    
    /* ... */

    public void ProcessEmployeePay(string employeeName, double hours, double rate)
    {
        // 1. Calculate Pay (Math)
        double totalPay = PayCalculator.CalculateTotalPay(hours, rate);
        
        // 2. Format and send Email message (Presentation)
        EmailNotifier.SendPayNotification(employeeName, totalPay);
    }
}
```

Of course, if you really wanted to get fanatical (and we do!), you would have also written a `EmailMessageFormatter` service-singleton class to produce the string, `$"Hello {employeeName}, your pay this week is ${totalPay}."`.

This is what is known as "Separation of Concerns": NONE of the pieces of the workflow know, or even care, what the other is doing --- they are just calling each other in a prescribed manner.

We'll talk more about "Separation of Concerns" later. But first, let's see how Uncle Bob's Clean Code applies in view of the Open/Closed Principle ("OCP").

Our other favorite thing about this "inside-out dependency inversion", or "dependency eversion", as we call it, is that singleton services allow us to automatically follow DRY because everyone who needs to calculate employee pay can simply create a property,

```csharp
private static IPayCalculator PayCalculator
{
    [DebuggerStepThrough]
    get;
} = GetPayCalculator.SoleInstance();
```

And then utilize the functionality of the `PayCalculator` class -- no matter where in the Software System we are.

This enables the `PayCalculator` class to be a central repository of functionality. And then we do not have to worry whose constructor(s) we have to pass it in to.

Now, why bother with the `GetXXX.SoleInstance()` method when the class, itself, already exposes a `public static IXXX Instance` property?

Because of intra-project reference tangling in large Visual Studio `.sln` file(s) --- the more degrees of separation, IMHO, we can put between a thing and its client, then the less chance of incurring circular dependencies (which are not allowed for intra-project references in a Visual Studio Solution.)

---

### 2. The "Switch Statement of Doom" vs. OCP

*Clean Code* dedicates specific attention to `switch` statements (or long `if-else` chains). Uncle Bob notes that switch statements inherently violate the **Open/Closed Principle (OCP)** because every time you add a new type (like a new type of employee), you have to open up existing functions and modify them.

**The Bad: Hardcoded Types (Violating OCP)**

If we add a `Contractor` to this system, we have to find every `switch` statement in the codebase and update it. This is a fragile, messy design.

```csharp
public class EmployeeManager
{
    public double CalculateBonus(string employeeType, double baseSalary)
    {
        switch (employeeType)
        {
            case "Manager":
                return baseSalary * 0.20;
            case "Developer":
                return baseSalary * 0.10;
            default:
                return 0;
        }
    }
}
```

**The Good: Polymorphism (Following OCP)**

Clean code replaces type-checking switch statements with polymorphic interfaces. Now, the system is *open* to extension (we can add a `Contractor` class) but *closed* to modification (we never have to touch the `BonusCalculator` again).

```csharp
public interface IEmployee
{
    double CalculateBonus();
}

public class Manager : IEmployee
{
    private double _baseSalary;
    public Manager(double salary) => _baseSalary = salary;
    
    public double CalculateBonus() => _baseSalary * 0.20;
}

public class Developer : IEmployee
{
    private double _baseSalary;
    public Developer(double salary) => _baseSalary = salary;
    
    public double CalculateBonus() => _baseSalary * 0.10;
}

public class BonusCalculator
{
    // This code never has to change, no matter how many employee types we add!
    public double GetBonusFor(IEmployee employee)
    {
        return employee.CalculateBonus();
    }
}
```

So, at xyLOGIX, we, in principle, see nothing wrong with the above, but we definitely need `switch` statement(s) when we are implementing the "Strategy Factory", or its related, "Pipeline," "Chain," and "Playbook" patterns.

The selector method MUST have a `switch` statement. We do not mind slightly violating OCP in order to, e.g., add new concrete strategy(ies) later.

---

### 3. The "Liar Subclass" vs. LSP (Liskov Substitution Principle)

In *Clean Code*, Uncle Bob stresses that a subclass must act like its parent. If a subclass overrides a method and throws a `NotImplementedException`, or fundamentally changes the nature of the state in an unexpected way, it's lying to the client that consumes it.

**The xyLOGIX Way:** We hate surprises in our code. If an object implements an interface or inherits from an abstract base class, it must fulfill the *entire* contract. If it cannot fulfill the contract, it has no business inheriting from it. We achieve strict adherence to LSP by keeping our interfaces exceptionally small (which naturally leads us directly to ISP). We would rather build a new interface than force an object to lie about its capabilities.

---

### 4. The "Fat Interface" vs. ISP (Interface Segregation Principle)

Uncle Bob teaches that classes shouldn't be forced to depend on methods they do not use. "Fat interfaces" create unnecessary coupling and force developers to implement dummy methods (which causes the LSP violations mentioned above).

**The xyLOGIX Way:** This is why we advocate for small, highly cohesive interfaces for our "Singleton Services." If a class only needs to calculate pay, it shouldn't have to implement or know about a `SendEmail` method. We segregate our interfaces so that our classes are laser-focused. This ensures that our "Dependency Eversion" properties are unambiguous, and a developer reading the code immediately understands exactly what a service is capable of doing without wading through a swamp of unrelated methods.

---

### 5. "Dependency Eversion" vs. DIP (Dependency Inversion Principle)

Uncle Bob states that high-level modules should not depend on low-level modules; both should depend on abstractions. Typically, the software industry solves this with massive Dependency Injection (DI) containers and constructors bloated with interfaces.

**The xyLOGIX Way (Our Hill to Die On):** We fully support DIP—we depend entirely on abstractions and interfaces (`IPayCalculator`, `IEmailNotifier`). However, we fundamentally reject the industry dogma of utilizing constructor injection for absolutely everything. As detailed earlier, constructor injection makes architectural changes "viral."

Instead, we use our **Dependency Eversion** paradigm. We use `public static IXXX Instance` properties and a separate `GetXXX.SoleInstance()` factory method, accessed via `private static` properties on the consuming class. This achieves the exact same decoupling and testability (we can always swap the instance behind the getter during testing) but completely eliminates constructor bloat and the viral nature of architectural changes.

---

### Summary of Good vs. Bad in Clean Architecture

| Principle Focus | Bad Code (The "Swamp") | Good Code (Clean Architecture) |
| --- | --- | --- |
| **Naming** | `int d; // elapsed time in days` | `int elapsedTimeInDays;` |
| **Duplication (DRY)** | Copying/pasting logic across modules. | Abstracting shared logic into reusable utility methods or classes. |
| **Modifications (OCP)** | Modifying existing methods to add features. | Creating new classes that implement existing interfaces. |
| **Dependencies (DIP)** | High-level business rules relying on specific SQL database classes. | High-level rules relying on a generic `IRepository` interface. |

---

## Fluent Programming

At xyLOGIX, we are big believers in Fluent Programming. Fluent interfaces utilize method chaining (returning `this` or a related builder interface) to make the code read like well-crafted English sentences. It encapsulates the sequential steps of an operation perfectly.

However, we do not just "spray it everywhere." We reserve Fluent Programming for builders, configuration objects, and setup pipelines where it makes semantic sense.

**Example:**

```csharp
// Good use of Fluent Programming
var report = MakeNewReport.FromScratch()
                          .WithTitle("Monthly Financials")
                          .ForDateRange(startDate, endDate)
                          .IncludeConfidentialData()
                          .Build();
```

This reads beautifully, hides the complex instantiation logic, and adheres strictly to our naming conventions (like using `MakeNewReport.FromScratch()`).

Sometimes, when there is such an obvious need to do so, such as there is a genuine case to be made for a class' constructor to "swallow" something else (because that class wraps/transforms it), we'll ditch the `FromScratch` method.  As we said previously, "Fluent interfaces utilize method chaining (returning `this` or a related builder interface) to make the code read like well-crafted English sentences."  But there is no point in chaining methods that do not need to even be there.  For instance, look at this interface:

```csharp
// Copyright © 2020-2026 xyLOGIX, LLC.  All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
// xyLOGIX and GUI Window Wrapper Module are trademarks of xyLOGIX, LLC.
// Trademark rights are not granted under the MIT License.

using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace xyLOGIX.GUI.Windows.Wrappers.Interfaces
{
    /// <summary>
    /// Defines the publicly-exposed events, methods and properties of a
    /// window-wrapper object.
    /// </summary>
    /// <remarks>
    /// The object(s) that implement this interface have, as their purpose, the
    /// repackaging of Win32 API <c>HWND</c> handle(s) as
    /// <see cref="T:System.Windows.Forms.IWin32Window" /> instance(s), so that they
    /// can then be used to identify, e.g., the owner or parent window(s) of various
    /// form(s).
    /// <para />
    /// This interface extends the
    /// <see cref="T:System.Windows.Forms.IWin32Window" /> interface by re-declaring
    /// the
    /// <see cref="P:xyLOGIX.GUI.Windows.Wrappers.Interfaces.IWindowWrapper.Handle" />
    /// property with a <c>[DebuggerStepThrough]</c> attribute on its getter.
    /// </remarks>
    public interface IWindowWrapper : IWin32Window
    {
        /// <summary>
        /// Gets a <see cref="T:System.IntPtr" /> value that represents the handle to the
        /// window represented by the implementer.
        /// </summary>
        /// <remarks>
        /// This property hides the
        /// <see cref="P:System.Windows.Forms.IWin32Window.Handle" /> property declared
        /// by the base interface in order to apply the
        /// <c>[DebuggerStepThrough]</c> attribute to its getter.
        /// <para />
        /// If no window handle has been assigned, this property returns
        /// <see cref="F:System.IntPtr.Zero" />.
        /// </remarks>
        /// <returns>
        /// A <see cref="T:System.IntPtr" /> value containing the handle to the window
        /// represented by the implementer.
        /// </returns>
        new IntPtr Handle { [DebuggerStepThrough] get; }
    }
}
```

This interface should be implemented by a Window Wrapper object:

```csharp
// Copyright © 2020-2026 xyLOGIX, LLC.  All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
//
// xyLOGIX and GUI Window Wrapper Module are trademarks of xyLOGIX, LLC.
// Trademark rights are not granted under the MIT License.

using PostSharp.Patterns.Diagnostics;
using System;
using System.Diagnostics;
using xyLOGIX.GUI.Windows.Wrappers.Interfaces;

namespace xyLOGIX.GUI.Windows.Wrappers
{
    /// <summary>
    /// Wraps an <see cref="T:System.IntPtr" /> value, ostensibly representing a
    /// <c>HWND</c> window handle, with a C# object,
    /// <see cref="T:xyLOGIX.GUI.Windows.Wrappers.WindowWrapper" />, that implements
    /// the <see cref="T:System.Windows.Forms.IWin32Window" /> interface.
    /// </summary>
    /// <remarks>
    /// This class exists for the purpose of repackaging Win32 API
    /// <c>HWND</c> handle(s) as <see cref="T:System.Windows.Forms.IWin32Window" />
    /// instance(s), so that they can then be used to identify, e.g., the owner or
    /// parent window(s) of various form(s).
    /// <para />
    /// This class implements the
    /// <see cref="T:xyLOGIX.GUI.Windows.Wrappers.Interfaces.IWindowWrapper" />
    /// interface.
    /// </remarks>
    public class WindowWrapper : IWindowWrapper
    {
        /// <summary>
        /// Initializes <see langword="static" /> data or performs actions that need to be
        /// performed once only for the
        /// <see cref="T:xyLOGIX.GUI.Windows.Wrappers.WindowWrapper" /> class.
        /// </summary>
        /// <remarks>
        /// This constructor is called automatically prior to the first instance being
        /// created or before any <see langword="static" /> members are referenced.
        /// <para />
        /// We've decorated this constructor with the <c>[Log(AttributeExclude = true)]</c>
        /// attribute in order to simplify the logging output.
        /// </remarks>
        [Log(AttributeExclude = true)]
        static WindowWrapper() { }

        /// <summary>
        /// Constructs a new instance of
        /// <see cref="T:xyLOGIX.GUI.Windows.Wrappers.WindowWrapper" /> and returns a
        /// reference to it.
        /// </summary>
        /// <remarks>
        /// This parameterless constructor initializes the
        /// <see cref="P:xyLOGIX.GUI.Windows.Wrappers.WindowWrapper.Handle" /> property
        /// to <see cref="F:System.IntPtr.Zero" />, indicating that no window handle has
        /// been assigned.
        /// <para />
        /// We've decorated this constructor with the <c>[Log(AttributeExclude = true)]</c>
        /// attribute in order to simplify the logging output.
        /// </remarks>
        [Log(AttributeExclude = true)]
        public WindowWrapper()
            => Handle = IntPtr.Zero;

        /// <summary>
        /// Constructs a new instance of
        /// <see cref="T:xyLOGIX.GUI.Windows.Wrappers.WindowWrapper" /> and returns a
        /// reference to it.
        /// </summary>
        /// <param name="hWnd">
        /// (Required.) A <see cref="T:System.IntPtr" /> value that
        /// provides the window handle, i.e., the <c>HWND</c>, to be wrapped by this
        /// object.
        /// </param>
        /// <remarks>
        /// This constructor assigns the specified <paramref name="hWnd" /> value
        /// directly to the
        /// <see cref="P:xyLOGIX.GUI.Windows.Wrappers.WindowWrapper.Handle" /> property
        /// without performing any validation.
        /// <para />
        /// Callers who require validation that the specified handle identifies an existing
        /// window should use the <c>MakeNewWindowWrapper.ForWindowHandle</c> factory
        /// method instead.
        /// <para />
        /// We've decorated this constructor with the <c>[Log(AttributeExclude = true)]</c>
        /// attribute in order to simplify the logging output.
        /// </remarks>
        [Log(AttributeExclude = true)]
        public WindowWrapper([NotLogged] IntPtr hWnd)
            => Handle = hWnd;

        /// <summary>
        /// Gets a <see cref="T:System.IntPtr" /> value that represents the handle to the
        /// window represented by this object.
        /// </summary>
        /// <remarks>
        /// The setter for this property is <see langword="private" />, meaning that the
        /// window handle can only be assigned during construction.
        /// <para />
        /// If no window handle has been assigned, this property returns
        /// <see cref="F:System.IntPtr.Zero" />.
        /// </remarks>
        /// <returns>
        /// A <see cref="T:System.IntPtr" /> value containing the handle to the window
        /// represented by this object.
        /// </returns>
        public IntPtr Handle
        {
            [DebuggerStepThrough] get;
            [DebuggerStepThrough] private set;
        }
    }
}
```

As you can see, it is very obvious here, that we MUST pass an HWND to the constructor.  This allows us to then implement the `IWin32Window` interface.

What is the use of such a class?  Well, as the XML docs state, sometimes we want to show a dialog box (made with WinForms 2.0) and we do not have an immediately accessible parent window for the dialog box; but we can use the Win32 EnumWindows API to find such a suitable window's HWND -- it's "window handle" so to speak -- and then we need to tell WinForms who the parent of the dialog box is.  Say, the dialog box is being implemented in a Visual Studio IDE extension.  We want to ensure that it is parented to the Visual Studio IDE's main window so that it behaves like a modal dialog box should -- blocking user input until the user deals with the setting(s) in the dialog box and dismisses it.  But, the WinForm `ShowDialog` method only accepts a reference to an instance of an object that implements the `IWin32Window` interface (way to go, clean code!) as its `ownerWindow` parameter, not a naked HWND (represented in the .NET Framework bby the `System.IntPtr` class).  So, we have our `WindowWrapper` class to wrap that HWND for us, and then we can throw a `WindowWrapper` instnce down into the `ShowDialog` method.

We also have a `MakeNewWindowWrapper` factory (as the XML docs from above explain):

```class
// Copyright © 2020-2026 xyLOGIX, LLC.  All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
// xyLOGIX and GUI Window Wrapper Module are trademarks of xyLOGIX, LLC.
// Trademark rights are not granted under the MIT License.

using PostSharp.Patterns.Diagnostics;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using xyLOGIX.Core.Debug;
using xyLOGIX.GUI.Windows.Wrappers.Interfaces;

namespace xyLOGIX.GUI.Windows.Wrappers.Factories
{
    /// <summary>
    /// Creates new instances of objects that implement the
    /// <see cref="T:xyLOGIX.GUI.Windows.Wrappers.Interfaces.IWindowWrapper" />
    /// interface, and returns references to them.
    /// </summary>
    public static class MakeNewWindowWrapper
    {
        /// <summary>
        /// Initializes <see langword="static" /> data or performs actions that need to be
        /// performed once only for the
        /// <see cref="T:xyLOGIX.GUI.Windows.Wrappers.Factories.MakeNewWindowWrapper" />
        /// class.
        /// </summary>
        /// <remarks>
        /// This constructor is called automatically prior to the first instance being
        /// created or before any <see langword="static" /> members are referenced.
        /// <para />
        /// We've decorated this constructor with the <c>[Log(AttributeExclude = true)]</c>
        /// attribute in order to simplify the logging output.
        /// </remarks>
        [Log(AttributeExclude = true)]
        static MakeNewWindowWrapper() { }

        /// <summary>
        /// Creates a new instance of an object that implements the
        /// <see cref="T:xyLOGIX.GUI.Windows.Wrappers.Interfaces.IWindowWrapper" />
        /// interface for the specified window handle, and returns a reference to it.
        /// </summary>
        /// <param name="hWnd">
        /// (Required.) A <see cref="T:System.IntPtr" /> value that specifies the handle
        /// of the window, i.e., its <c>HWND</c>, that is to be wrapped.
        /// </param>
        /// <remarks>
        /// This method calls the <c>IsWindow</c> Win32 API function on the
        /// specified <paramref name="hWnd" /> to verify that it identifies an existing
        /// window before constructing the wrapper.
        /// <para />
        /// If the specified <paramref name="hWnd" /> is equal to
        /// <see cref="F:System.IntPtr.Zero" />, or if it does not identify an existing
        /// window, then this method returns a <see langword="null" /> reference.
        /// </remarks>
        /// <returns>
        /// Reference to an instance of an object that implements the
        /// <see cref="T:xyLOGIX.GUI.Windows.Wrappers.Interfaces.IWindowWrapper" />
        /// interface that wraps the specified <paramref name="hWnd" />; otherwise, a
        /// <see langword="null" /> reference is returned if the specified
        /// <paramref name="hWnd" /> is <see cref="F:System.IntPtr.Zero" /> or does not
        /// identify an existing window.
        /// </returns>
        [return: NotLogged]
        [DebuggerStepThrough]
        public static IWindowWrapper ForWindowHandle([NotLogged] IntPtr hWnd)
        {
            IWindowWrapper result = default;

            try
            {
                if (IntPtr.Zero.Equals(hWnd)) return result;
                if (!IsWindow(hWnd)) return result;

                result = new WindowWrapper(hWnd);
            }
            catch (Exception ex)
            {
                // dump all the exception info to the log
                DebugUtils.LogException(ex);

                result = default;
            }

            return result;
        }

        /// <summary>
        /// Determines whether the specified window handle identifies an existing window.
        /// </summary>
        /// <param name="hWnd">
        /// (Required.) A <see cref="T:System.IntPtr" /> value containing a handle to the
        /// window to be tested.
        /// </param>
        /// <remarks>
        /// This method is a P/Invoke wrapper around the <c>IsWindow</c> function exported
        /// by <c>user32.dll</c>.
        /// </remarks>
        /// <returns>
        /// <see langword="true" /> if the specified <paramref name="hWnd" /> identifies an
        /// existing window; <see langword="false" /> otherwise.
        /// </returns>
        [DllImport("user32.dll"), Log(AttributeExclude = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool IsWindow(IntPtr hWnd);
    }
}
```

This class is very fluent.  You call it by saying:

```csharp
dialog.ShowDialog(
    MakeNewWindowWrapper.ForWindowHandle(
        myWindowHandle
    )
);
```

Now, never mind my line wrapping; that was just to make it easier to read the code snippet.  The point is, it's pointless to put a `.FromScratch()` method here, to make us have to say, `MakeNewWindowWrapper.FromScratch().ForWindowHandle(myWindowHandle)` which would just be silly.  So that's what I mean, when I say that we do not have to ALWAYS have a `FromScratch`. method.

Now, let's also talk about the school of so-called "Functional Programming" and why we love it.

## Functional Programming

We also strongly embrace Functional Programming (FP) paradigms, though strictly within the boundaries of C# 7.3 and our object-oriented architecture.

Functional Programming emphasizes pure functions (methods that have no side effects and always return the same output for the same input), immutability, and eager evaluation.

**The xyLOGIX Way:** We express FP predominantly through our "Action Classes"—`static` classes named after verbs (e.g., `Format`, `Display`, `Calculate`). These classes contain `static` methods that take specific inputs and return specific outputs without mutating global state.

We combine this with our "Shift-Left" approach:

1. **Eager Returns:** Validate inputs immediately upon method entry and return eagerly if they fail.
2. **No Unnecessary State:** Use `var` and `out var`, and avoid reassigning variables where possible.
3. **Pattern Matching & Exclusions:** Use C# features to write concise, predictable logic, focusing on gating logic that EXCLUDES cases we DO NOT want, rather than deeply nesting `if` statements for the cases we DO want.
4. **The Discard is Cool:** In C# 7.3, the `_` character is the Discard Character.  If you do not need a return value of a method, or you do not need the value of an `out` parameter, then just discard it! Do `_ = Foo(x, y, z);` or `TryGetValue(theFolderPathname, out _)` if all your're interesed in is (a) whether something fails or not, or (b) whether a `TryXXX` gate method gates or not, but you just wanna otherwise throw away what it gives you.  At xyLOGIX, we *love* the Discard character.

By marrying these FP concepts with our SOLID foundations and Dependency Eversion, we create a codebase that is robust, fault-tolerant, and incredibly resistant to the rotting effects of age.

### Sidebar: Using Functional Programming in Algorithms

One of the primary uses of FP is when we're implementing the "Strategy Factory" pattern, or one of its derivative pattern(s).  Actually, it comes into play more when we're *using* the pattern.  Typically, an `enum` is written to identify the strategies; one of these is then fed to a strategy selector method:

```csharp
public enum BalloonColor
{
    Red,
    Orange,
    Yellow,
    Green,
    Blue,
    Indigo,
    Violet,
    Unknown = -1
}

public static MakeNewBalloon
{
    public static IBalloon HavingColor(BalloonColor color)
    {
        IBalloon result = default;
    
        switch(color)
        {
            case BalloonColor.Red:
                result = MakeNewRedBalloon.FromScratch();
                break;
                
            case BalloonColor.Orange:
                result = MakeNewOrangeBalloon.FromScratch();
                break;
                
            case BalloonColor.Yellow:
                result = MakeNewYellowBalloon.FromScratch();
                break;

            /* ... */
                
        }
        
        return result;
    }
}
```

Now, let's say "Krusty the Clown" hands out balloons at the state fair (with all respect to "The Simpsons," by Matt Groening).  What if his manager says that children over age 13 get blue, children aged 8-12 get green, children aged 5-7 get red, children under age 5 get orange, and everyone else gets yellow.  There clearly is a requirement to implement some sort of algorithm to picking the correct balloon color.  The client of the strategy factory has to work out what is the correct `BalloonColor` `enum` value to then pass to the `MakeNewBalloon.HavingColor` method.  Now, we COULD just implement such an algorithm right in the client of the Strategy Factory:

**Bad**

```csharp
public class KrustyTheClown
{
    public void GiveThatKidABalloon(int theKidsAge)
    {
        var color = BalloonColor.Yellow;
        
        if (age <= 0)
            color = BalloonColor.Unknown;
        else if (age == 13)
            color = BalloonColor.Blue;
        else if (age >= 8 && age < 13)
        {
            color = BalloonColor.Green;
        }
        else if (age >= 5 && age < 8)
        {
            color = BalloonColor.Red;
        }
        else if (age < 5)
        {
            color = BalloonColor.Orange;
        }
        
        if (BalloonColor.Unknown.Equals(color))
            return;
            
        var balloon = MakeNewBalloon.HavingColor(color);
        
        /* yay! I have a balloon, how fun! */
    }
}
```

The above is horrible code.  (BTW, I do not know what color to give a person who is zero or negative years old.)  It is wrong on so many levels.  Not to mention hard to read and maintain.   And it's also very brittle.

So, how do we make it better?  Separate out the determination of the correct color from the making of the balloon!

It's more functional to instead, create a `WorkOut` action class (can also be named `Determine`, `Ascertain`, `Obtain`, `Derive`, `Resolve` etc.), with a `TheCorrectBalloonColorForChildsAge` method:

```csharp
public static WorkOut
{
    public static BalloonColor TheCorrectBalloonColorForChildsAge(int age)
    {
        var result = BalloonColor.Yellow;
        
        if (age <= 0)
            return BalloonColor.Unknown;
        
        if (age == 13)
            result = BalloonColor.Blue;
        else if (age >= 8 && age < 13)
        {
            result = BalloonColor.Green;
        }
        else if (age >= 5 && age < 8)
        {
            result = BalloonColor.Red;
        }
        else if (age < 5)
        {
            result = BalloonColor.Orange;
        }
        
        return result;
    }
}
```

Then, in the client, we call:

```csharp
public class KrustyTheClown
{
    public void GiveThatKidABalloon(int theKidsAge)
    {
        var color = WorkOut.TheCorrectBalloonColorForChildsAge(theKidsAge);
        
        if (BalloonColor.Unknown.Equals(color))
            return;
            
        var balloon = MakeNewBalloon.HavingColor(color);
        
        /* yay! I have a balloon, how fun! */
    }
}
```

All Krusty needs to do is "work out the correct balloon color given the child's age" and then "make a new balloon having that color." Given how Krusty has been portrayed on "The Simpsons," it's probably the deepest level of thought he is capable of, ha ha ha!

This way, the `WorkOut.TheCorrectBalloonColorForChildsAge` method is THE central decision maker about who gets what color of balloon.  Note my input-validation in two places.  Since `int`, in C#, can be negative, zero, or positive, and since I do not know how to represent a human being who is zero or negative years old, the `WorkOut.TheCorrectBalloonColorForChildsAge` method tests `if (age <= 0)` and, if that is the case, returns `BalloonColor.Unknown` since I do not know what color of balloon to give that person.  And of course, the `KrustyTheClown.GiveThatKidABalloon` method traps the `if (BalloonColor.Unknown.Equals(color))` case and just stops, since it does not know what to do with such a value.

At xyLOGIX, BTW, we like to say, `if (BalloonColor.Unknown.Equals(color))` or `if (!BalloonColor.Unknown.Equals(color))` not `if (color == BalloonColor.Unknown)` / `if (color != BalloonColor.Unknown)`.

BTW, you see now, where we say "thank you very much Uncle Bob" to him about the "switch statement of doom."  We used `switch` statement(s) just now.  You may ask, why?  We think we're on okay ground here, since this is for code that is supposed to rarely, if ever change.

---

## Always Access Property Getters Directly (No Local Aliasing)

At xyLOGIX, we always want to utilize the values returned by property getters directly within our methods and algorithms, rather than assigning them to intermediate local variables.

A common anti-pattern generated by AI chatbots and amateur developers alike is "local aliasing"—assigning the value of a property (such as `CurrentClientSession`) to a local variable (such as `var currentClientSession = CurrentClientSession;`) before performing null-checks, logging, or evaluating conditional gating logic. We strictly reject this practice as a default rule.

### Why This is a Hill to Die On

* **Eliminates Unnecessary State:** Our Functional Programming standards explicitly mandate that we avoid unnecessary state and avoid reassigning variables where possible. Creating a local variable solely to hold an already-accessible property value introduces redundant state and visual clutter.
* **Prevents Stale Reads:** Properties—especially those utilizing our Dependency Eversion paradigm or lazy evaluation—are designed to return the correct, current state upon invocation. Caching a property in a local variable creates an isolated snapshot that can become stale if the underlying state shifts during method execution.
* **Zero Performance Penalty:** In C# 7.3 and .NET Framework 4.8, simple property getters (especially those decorated with the `[DebuggerStepThrough]` attribute) are aggressively inlined by the JIT compiler. There is zero performance advantage to caching a simple property getter in a local variable.

### The Bad Way: Aliasing to a Local Variable

In this bad example, assigning `CurrentClientSession` to the local variable `currentClientSession` adds useless boilerplate, clutters the scope with redundant identifiers, and violates our minimalist state principles:

```csharp
var currentClientSession = CurrentClientSession;

System.Diagnostics.Debug.WriteLine(
    "Determine.TheCorrectSessionLoggerFetchApproachToUse: Checking whether the variable, 'currentClientSession', has a null reference for a value..."
);

// Check whether a current logging-client session is available. If this is not the
// case, then use the legacy logger-fetching approach.
if (currentClientSession == null)
{
    // There is no current logging-client session. The default behavior of this
    // library must therefore prevail.
    System.Diagnostics.Debug.WriteLine(
        "Determine.TheCorrectSessionLoggerFetchApproachToUse: *** WARNING *** The variable, 'currentClientSession', has a null reference for a value.  Using the legacy logging infrastructure..."
    );

    result = SessionLoggerFetchApproach.FetchLegacyLogger;

    System.Diagnostics.Debug.WriteLine(
        $"Determine.TheCorrectSessionLoggerFetchApproachToUse: Result = '{result}'"
    );

    // stop.
    return result;
}

```

### The xyLOGIX Way: Direct Property Evaluation

We must evaluate the property getter directly for every method and algorithm everywhere in our code. Notice how directly evaluating `CurrentClientSession` keeps the gating logic clean, unambiguous, and free of redundant local state:

```csharp
System.Diagnostics.Debug.WriteLine(
    "Determine.TheCorrectSessionLoggerFetchApproachToUse: Checking whether the property, 'CurrentClientSession', has a null reference for a value..."
);

// Check whether a current logging-client session is available. If this is not the
// case, then use the legacy logger-fetching approach.
if (CurrentClientSession == null)
{
    // There is no current logging-client session. The default behavior of this
    // library must therefore prevail.
    System.Diagnostics.Debug.WriteLine(
        "Determine.TheCorrectSessionLoggerFetchApproachToUse: *** WARNING *** The property, 'CurrentClientSession', has a null reference for a value.  Using the legacy logging infrastructure..."
    );

    result = SessionLoggerFetchApproach.FetchLegacyLogger;

    System.Diagnostics.Debug.WriteLine(
        $"Determine.TheCorrectSessionLoggerFetchApproachToUse: Result = '{result}'"
    );

    // stop.
    return result;
}

```

### The Explicit Exception: Volatile and Heavy Computed Properties

While direct property evaluation is our immutable baseline, pragmatic engineering requires recognizing where strict adherence introduces logical race conditions or performance penalties. Local aliasing is explicitly permitted—and required—under two specific circumstances:

* **Volatile or Non-Deterministic State:** If a property's underlying value is non-deterministic or subject to mutation during the execution of a method (e.g., cross-thread background modifications, hardware polling, time-sensitive calculations, or UI control properties accessed during asynchronous state changes), repeatedly evaluating the getter can lead to inconsistent evaluation (torn state). You must capture a single, immutable snapshot in a local variable to ensure your algorithm operates against a consistent state from start to finish.
* **Heavy Computed Getters Without Memoization:** While we actively discourage designing properties that execute expensive operations without internal memoization or lazy caching, you may encounter legacy getters or complex mathematical calculations where every read incurs a massive CPU or memory allocation penalty. In these cases, caching the result locally prevents compounding performance degradation.

**The Exception Rule of Engagement:** Whenever you invoke this exception and alias a property to a local variable, you must treat that variable as immutable (`readonly` where applicable) and include a clear, concise comment explaining *why* the snapshot is required. This prevents automated refactoring tools and well-meaning future maintainers from "cleaning up" your local variable back into a direct property read and reintroducing bugs.

## Always Code to an Interface

One of the most critical hills we will die on at xyLOGIX is this: **Always code to an interface, not a concrete implementation.**

Why is this so important? When a class depends directly on another concrete class, they become tightly coupled. They are welded together. If you need to change how the dependency works, or if you need to swap it out for testing (like using a mock object), you are forced to rip the weld apart and change the consuming code. This makes the system brittle.

When you code to an interface, you are establishing a "contract." The consuming class says, "I don't care *how* you do this job, I only care that you *can* do it." This allows us to plug in any class that signs the contract, completely shielding the consumer from implementation details.

**The Bad: Tightly Coupled Concrete Types**

```csharp
public class ReportGenerator
{
    // BAD: We are welded to the concrete LocalDiskWriter class.
    private LocalDiskWriter _writer = new LocalDiskWriter();

    public void GenerateAndSave()
    {
        var data = "Confidential Financial Data";
        _writer.SaveText(data); 
        // If we suddenly need to save to Azure Blob Storage, we have to rewrite this entire class!
    }
}

```

**The xyLOGIX Way (Good): Coding to an Interface**

We declare an `IReportWriter` interface, and the `ReportGenerator` relies strictly on that. Notice how we use our Dependency Eversion pattern here:

```csharp
public interface IReportWriter
{
    void SaveText(string text);
}

public class LocalDiskWriter : IReportWriter
{
    public static IReportWriter Instance { [DebuggerStepThrough] get; } = new LocalDiskWriter();

    public void SaveText(string text)
    {
        /* saves to C:\ drive */
    }
}

public static class GetReportWriter
{
    [DebuggerStepThrough]
    [return: NotLogged]
    public static IReportWriter SoleInstance() => LocalDiskWriter.Instance;
}

public class ReportGenerator
{
    // GOOD: We rely ONLY on the interface contract.
    private static IReportWriter Writer
    {
        [DebuggerStepThrough]
        get;
    } = GetReportWriter.SoleInstance();

    public void GenerateAndSave()
    {
        var data = "Confidential Financial Data";
        Writer.SaveText(data); 
        // We can swap the implementation behind GetReportWriter at any time, 
        // and this class never has to change.
    }
}

```

By keeping our interfaces small and focused, and strictly communicating through them, we ensure our codebase remains highly flexible, infinitely testable, and strictly decoupled.

---

## The Single Responsibility Principle (SRP) at Every Level

While many developers understand SRP at the class level ("a class should do one thing"), we take it much further. To prevent the rot of technical debt, SRP must be ruthlessly applied at three distinct levels: The File, The Class, and The Method/Property.

### 1. The File Level

**The Rule:** A single `.cs` file must contain exactly one class, interface, struct, enum, or delegate declaration.

When developers cram multiple classes or interfaces into a single file, it becomes a nightmare to navigate. It destroys the utility of the Solution Explorer, bloats the file size, and guarantees massive merge conflicts in source control when multiple developers are working in the same module.

* **Bad:** Putting `IWindowFinder`, `WindowFinder`, and `WindowEnumCallbackDelegate` all in `WindowFinder.cs`.
* **Good:** `IWindowFinder.cs` contains only the interface. `WindowFinder.cs` contains only the concrete class. `WindowEnumCallbackDelegate.cs` contains only the delegate.

### 2. The Class Level

**The Rule:** A class should have only one reason to change. It should represent a single cohesive concept or responsibility.

If a class has the word "And" in its description (e.g., "This class retrieves the user data *and* formats it for the screen"), it is violating SRP. When classes violate SRP, they turn into "God Objects"—massive, monolithic files with thousands of lines of code, hundreds of `private` helper methods, and tightly tangled state.

* **Bad:** A `MainForm` that handles button clicks, directly queries the SQL database, and parses JSON strings.
* **Good:** A `MainForm` that handles UI layout, a `GetUserData` action class that queries the database, and a `ParseJson` action class that handles the strings. The form delegates all non-UI work to dedicated singleton services and action classes.

### 3. The Method and Property Level

**The Rule:** A method should perform one discrete action. A property should expose one discrete piece of state.

If a method is doing three or four things in sequence (e.g., validating input, opening a file, reading the lines, filtering the lines, and saving a new file), it is too long. Extract those distinct steps into smaller, strictly-named helper methods.

We heavily utilize our "Shift-Left" approach and eager returns to keep methods strictly focused on their happy-path responsibility, rather than getting bogged down in nested `if` statements.

**Bad (Method doing too much):**

```csharp
public bool ProcessData(string input)
{
    if (!string.IsNullOrWhiteSpace(input))
    {
        var sanitized = input.Trim().ToLower();
        if (sanitized.Length > 5)
        {
            // Do processing...
            return true;
        }
        else
        {
            return false;
        }
    }
    return false;
}
```

**Good (Shift-Left, Eager Returns, Single Responsibility):**

```csharp
[return: NotLogged]
public bool ProcessData([NotLogged] string input)
{
    var result = false;

    try
    {
        if (string.IsNullOrWhiteSpace(input)) return result;

        var sanitized = SanitizeInput(input);
        if (sanitized.Length <= 5) return result;

        /*
         * Base our report of success (returning TRUE) on the success
         * of the call to ExecuteProcessing, below.
         */

        result = ExecuteProcessing(sanitized);
    }
    catch (Exception ex)
    {
        // dump all the exception info to the log
        DebugUtils.LogException(ex);
        result = false;
    }

    return result;
}

[return: NotLogged]
private string SanitizeInput([NotLogged] string input)
    => input.Trim().ToLowerInvariant();
```

By enforcing SRP at the method level, we eliminate deep nesting, make our logic infinitely easier to read, and ensure that if the sanitization logic needs to change, it changes in exactly one place (DRY), without risking the core processing logic.

---

## Loose Coupling and Separation of Concerns (SoC)

At xyLOGIX, we build systems that are meant to last. To do that, we must strictly enforce two related ideas: **Loose Coupling** and **Separation of Concerns (SoC)**.

Think of a professional restaurant. The waiter takes your order and brings you your food. The chef stays in the kitchen, chops the vegetables, and cooks the meal. The waiter doesn't cook, and the chef doesn't talk to the customers. They have a **Separation of Concerns**—they each have a specific, separate job. 

Because they have separate jobs, they are **Loosely Coupled**. If the restaurant hires a new chef, the waiter doesn't have to learn a new way to talk to the customers. They just hand the ticket to the new chef. 

In software, if your user interface (the Windows Form) talks directly to your SQL database, your waiter is cooking the food. If you change your database, your UI breaks. This is a nightmare to test and maintain.

### The Solution: Model-View-Presenter (MVP)

To achieve Loose Coupling and SoC in Windows Forms, we use the **Model-View-Presenter (MVP)** pattern. 

1. **The Model:** The data and the database rules (the Chef).
2. **The View:** The Windows Form itself. It only handles displaying things and clicking buttons (the Waiter).
3. **The Presenter:** The middleman. It listens to the View, talks to the Model, and tells the View exactly what to show (the Kitchen Expediter).

The Golden Rule of MVP: **The View never talks to the Database, and the Presenter never touches WinForms UI controls.**

### The Bad Way: The "Code-Behind" Swamp

Here is what 99% of amateur Windows Forms code looks like. The form does everything. It knows about the UI, the database connection strings, and the SQL queries. It is completely tangled (Tightly Coupled).

```csharp
public partial class MainWindow : Form
{
    // BAD: The form is talking directly to the database!
    private void searchButton_Click(object sender, EventArgs e)
    {
        string term = searchTextBox.Text;
        using (var conn = new SqlConnection("MyConnectionString"))
        {
            var cmd = new SqlCommand($"SELECT * FROM Users WHERE Name LIKE '%{term}%'", conn);
            // ... reading data and filling the grid ...
        }
    }
}
```

### The xyLOGIX Way: Clean MVP with Dependency Eversion

First, we define an interface for our View. The Presenter will *only* talk to this interface, never to the physical form controls.

```csharp
public interface IMainWindow
{
    // The View promises to provide the search term
    string SearchTerm { [DebuggerStepThrough] get; }

    // The View promises to display a list of users when asked
    void DisplayUsers(IEnumerable<User> users);

    // The View promises to tell us when the user wants to search
    event EventHandler SearchRequested;
}
```

Next, we create the **Presenter**. Notice how we use our xyLOGIX "Dependency Eversion" pattern to bring in the database service (`IUserRepository`) without bloating a constructor!

```csharp
public class MainWindowPresenter
{
    // The Presenter only knows about the View's interface
    private readonly IMainWindow _view;

    // We get our Database service using Dependency Eversion
    private static IUserRepository UserRepository
    {
        [DebuggerStepThrough]
        get;
    } = GetUserRepository.SoleInstance();

    // The Presenter is created by passing in the View interface
    public MainWindowPresenter(IMainWindow view)
    {
        _view = view;
        
        // Listen for the View's events
        _view.SearchRequested += OnSearchRequested;
    }

    private void OnSearchRequested(object sender, EventArgs e)
    {
        // 1. Get the term from the view
        var term = _view.SearchTerm;

        // 2. Ask the Model (Database) for the data
        var users = UserRepository.GetUsers(term);

        // 3. Tell the View to display the data
        _view.DisplayUsers(users);
    }
}
```

Finally, the **View** (the actual Windows Form). At xyLOGIX, for every form that uses MVP, we place an `InitializePresenter` method in a partial class file (e.g., `MainWindow.Presenter.cs`) to hook it up. The Form handles the UI and *nothing else*.

```csharp
public partial class MainWindow : DarkForm, IMainWindow
{
    // The property holding the Presenter
    private MainWindowPresenter Presenter { get; set; }

    public MainWindow()
    {
        InitializeComponent();
        InitializePresenter(); // Hook up the Presenter
    }

    private void InitializePresenter()
    {
        Presenter = new MainWindowPresenter(this);
    }

    // --- Implementing the IMainWindow Interface ---

    public string SearchTerm 
    { 
        [DebuggerStepThrough] 
        get => searchTextBox.Text; 
    }

    public void DisplayUsers(IEnumerable<User> users)
    {
        usersDataGridView.DataSource = users;
    }

    public event EventHandler SearchRequested;

    // When the real button is clicked, just raise the interface event!
    private void searchButton_Click(object sender, EventArgs e)
    {
        SearchRequested?.Invoke(this, EventArgs.Empty);
    }
}
```

### Why this is a Hill to Die On

Look at how cleanly those concerns are separated!

* **The Form** has zero idea what SQL is, what a database is, or where the data comes from.
* **The Presenter** has zero idea what a `TextBox` or a `DataGridView` is. It only knows about `SearchTerm` and `DisplayUsers`.
* **The Model (UserRepository)** just fetches POCOs and returns them.

If we want to change from a SQL database to a cloud API, we only update the `UserRepository`. The Form and Presenter are untouched. If we want to unit test the `MainWindowPresenter`, we can easily pass in a fake `IMainWindow` interface and a fake `IUserRepository` to test our logic without ever opening a real Windows Form or connecting to a real database.

This is the ultimate expression of SOLID, DRY, and Loose Coupling. It takes slightly more setup, but it ensures your software can grow indefinitely without collapsing under its own weight.

---

## Deep Dive: The Presenter and The Model

To fully grasp the power of the Model-View-Presenter (MVP) pattern and how it enforces Loose Coupling, we must examine the other two legs of the tripod: The Presenter and The Model. 

If the View (the Windows Form) is the Waiter, the Presenter is the Kitchen Expediter, and the Model is the Chef cooking the meal from the raw ingredients (the database).

### The Presenter: The Ultimate Orchestrator

The Presenter's entire reason for existence is to keep the View and the Model from ever meeting. 

**The Golden Rule of the Presenter:** A Presenter class must **never** contain `using System.Windows.Forms;`. It must never know about a `TextBox`, a `Button`, or a `DataGridView`. 

If a Presenter knows about UI controls, you have failed to separate your concerns. The Presenter speaks to the View strictly through the interface contract (e.g., `IMainWindow`). It listens to the View's events, requests data from the Model, and then passes POCOs (Plain Ol' C# Objects) back to the View to be displayed.

Here is what a pristine, xyLOGIX-compliant Presenter looks like:

```csharp
using System;
using System.Collections.Generic;
using System.Diagnostics;
using xyLOGIX.Core.Debug;

namespace xyLOGIX.App.Presenters
{
    public class MainWindowPresenter
    {
        private readonly IMainWindow _view;

        // Dependency Eversion: We ask for the interface, never the concrete SQL class.
        private static IUserRepository UserRepository
        {
            [DebuggerStepThrough]
            get;
        } = GetUserRepository.SoleInstance();

        public MainWindowPresenter(IMainWindow view)
        {
            _view = view ?? throw new ArgumentNullException(nameof(view));
            
            // Subscribe to the View's events
            _view.SearchRequested += OnSearchRequested;
        }

        private void OnSearchRequested(object sender, EventArgs e)
        {
            try
            {
                // 1. Get the search term from the View
                var term = _view.SearchTerm;

                if (string.IsNullOrWhiteSpace(term))
                {
                    _view.DisplayUsers(new List<User>());
                    return;
                }

                // 2. Ask the Model for the data
                var users = UserRepository.GetUsers(term);

                // 3. Hand the data back to the View
                _view.DisplayUsers(users);
            }
            catch (Exception ex)
            {
                // dump all the exception info to the log
                DebugUtils.LogException(ex);
            }
        }
    }
}
```

Because this class has zero ties to WinForms, you can instantiate it in an NUnit test fixture, pass it a mock `IMainWindow`, and thoroughly test your application's logic in milliseconds.

### The Model: The Data Access Layer

The Model is where the actual business rules and data retrieval happen. In a standard enterprise application, this usually means talking to a SQL Server database.

**The Golden Rule of the Model:** The Model must never know about the UI. It receives raw inputs (like strings or integers) and returns POCOs. It handles its own exceptions, manages its own connections, and cleans up its own resources.

Here is how we implement a trivial SQL Server database connection using our Shift-Left, eager-returning, exception-logging standards.

First, the POCO (Data Transfer Object) and the Interface:

```csharp
namespace xyLOGIX.App.Models
{
    // The POCO
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    // The Interface Contract
    public interface IUserRepository
    {
        IEnumerable<User> GetUsers([NotLogged] string term);
    }
}
```

Next, the Concrete Implementation. Notice how we use `using` blocks to ensure the `SqlConnection` and `SqlCommand` are properly disposed of, preventing memory leaks and dangling database connections. We also utilize parameterized queries (`@term`) to prevent SQL Injection attacks—another hill we will absolutely die on.

```csharp
using PostSharp.Patterns.Diagnostics;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using xyLOGIX.Core.Debug;

namespace xyLOGIX.App.Models
{
    public class SqlUserRepository : IUserRepository
    {
        public static IUserRepository Instance { [DebuggerStepThrough] get; } = new SqlUserRepository();

        private SqlUserRepository() { }

        [return: NotLogged]
        public IEnumerable<User> GetUsers([NotLogged] string term)
        {
            var result = Enumerable.Empty<User>();

            try
            {
                // Shift-Left: Eager return on invalid input.
                if (string.IsNullOrWhiteSpace(term)) return result;

                var users = new List<User>();

                // NOTE: Assume 'DbConfig.ConnectionString' is a property providing our SQL connection string.
                using (var conn = new SqlConnection(DbConfig.ConnectionString))
                {
                    conn.Open();

                    var sql = "SELECT Id, Name FROM Users WHERE Name LIKE @term;";
                    
                    using (var cmd = new SqlCommand(sql, conn))
                    {
                        // ALWAYS use parameters to prevent SQL injection!
                        cmd.Parameters.AddWithValue("@term", $"%{term}%");

                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                users.Add(new User
                                {
                                    Id = reader.GetInt32(0),
                                    Name = reader.GetString(1)
                                });
                            }
                        }
                    }
                }


                /* 
                 *If we made it this far with no Exception(s) getting caught, 
                 * then assume that the
                 * operation(s) succeeded.
                 */
                 
                result = users;
            }
            catch (Exception ex)
            {
                // dump all the exception info to the log
                DebugUtils.LogException(ex);
                
                result = Enumerable.Empty<User>();
            }

            return result;
        }
    }
}
```

Finally, our Dependency Eversion factory:

```csharp
namespace xyLOGIX.App.Models.Factories
{
    public static class GetUserRepository
    {
        [DebuggerStepThrough]
        [return: NotLogged]
        public static IUserRepository SoleInstance() => SqlUserRepository.Instance;
    }
}

```

### The Beauty of the Architecture

Look at what we have achieved:

1. **The User** types a name into a `TextBox` and clicks a button on the Form.
2. **The Form (View)** says, "Hey Presenter, the user wants to search for this string!"
3. **The Presenter** says, "Thanks, Form. Hey UserRepository, get me the users matching this string."
4. **The Model (UserRepository)** connects to SQL Server safely, grabs the data, turns it into a list of `User` objects, and hands it back to the Presenter.
5. **The Presenter** says, "Hey Form, here is a list of User objects. Display them."
6. **The Form** binds the list to the `DataGridView`.

Nobody stepped on anyone else's toes. The code is SOLID, DRY, Loosely Coupled, thoroughly protected with `try/catch` blocks, and completely isolated from viral dependency changes thanks to Dependency Eversion. This is the xyLOGIX way.

---

## Loose Coupling and Separation of Concerns: Everywhere, Not Just UI

You’ve seen how we use MVP to separate the User Interface from the Business Logic and the Data Access Layer. But **Separation of Concerns (SoC)** is not just a UI pattern; it is a fundamental engineering philosophy.

If you find yourself writing code that does two different things—like calculating a user's tax *and* logging the result to a file—you have coupled those two things together. This is "spaghetti architecture." If the logging format changes, your tax calculation code shouldn't have to change.

Here are two non-UI examples of how we apply this at xyLOGIX to keep our system loosely coupled.

## Example 1: The Logging Concern

**The Problem:** Hard-coding a logging destination (e.g., a file) is a violation of the Open/Closed Principle. If the business requirements shift to require logging to Splunk or an event viewer, modifying your `FileLogger` class is a maintenance nightmare.

**The xyLOGIX Way:** We treat logging as a **Strategy Pattern**. We define a common interface (`ILogger`), implement specific strategies (`FileLogger`, `SplunkLogger`), and use a `Strategy Factory` to select the appropriate logger.  We also implement an abstract base class, `LoggerBase`, that (a) provides common functionality and shared singleton services (declaring them `protected static` not `private static`, so that child classes can access them) and (b) declares abstract properties and methods that children *must* override/implement.  It is also the only class that actually implements the `ILogger` interface directly --- the children all implement the `LoggerBase` class.

### 1. The Strategy Definitions

First, we define our strategies and the `enum` that acts as the "selector."

```csharp
public enum LoggingDestination { File, Splunk }
```

Then, we define the C# interface that every single strategy has in common:

```csharp
public interface ILogger
{
    // Mark this logger with the strategy it follows.
    LoggingDestination Destination { get; }

    void LogInfo(string message);
    void LogError(string message);
}
```

Then, we define the common, abstract base class that everyone has to inherit:

```csharp
public abstract class LoggerBase : ILogger
{
    public abstract LoggingDestination Destination { get; }
    
    public abstract void LogInfo(string message);
    public abstract void LogError(string error);
}
```

Then, we define the concrete children strategy class(es):

```csharp
public class FileLogger : LoggerBase
{
    public override LoggingDestination Destination { get; } = LoggingDestination.File;

    public static ILogger Instance { [DebuggerStepThrough] get; } = new FileLogger();
    private FileLogger() { }
    
    public override void LogInfo(string message) => /* Logic for File Append */;
    public override void LogError(string message) => /* Logic for File Append */;
}

public class SplunkLogger : LoggerBase
{
    public override LoggingDestination Destination { get; } = LoggingDestination.Splunk;

    public static ILogger Instance { [DebuggerStepThrough] get; } = new SplunkLogger();
    private SplunkLogger() { }
    
    public override void LogInfo(string message) => /* Logic for Splunk API */;
    public override void LogError(string message) => /* Logic for Splunk API */;
}
```

Then, we define factory(ies) for each concrete class:

```csharp
public static class GetFileLogger
{
    [DebuggerStepThrough]
    [return: NotLogged]
    public static ILogger SoleInstance() => FileLogger.Instance;
}

public static class GetSplunkLogger
{
    [DebuggerStepThrough]
    [return: NotLogged]
    public static ILogger SoleInstance() => SplunkLogger.Instance;
}
```

### 2. The Strategy Factory

This is the heart of the design. The factory is the only place in the system that knows about the concrete implementations. It uses the `switch` statement of doom (which is perfectly acceptable and required here!) to return the requested strategy.

```csharp
public static class GetLogger
{
    [DebuggerStepThrough]
    public static ILogger ForDestination(LoggingDestination destination)
    {
        switch (destination)
        {
            case LoggingDestination.File:
                return GetFileLogger.SoleInstance();
            case LoggingDestination.Splunk:
                return GetSplunkLogger.SoleInstance();
            default:
                throw new ArgumentOutOfRangeException(nameof(destination), destination, $"There is no logger for the destination, '{destination}'.);
        }
    }
}
```

### 3. Usage

The consuming business logic does not know *how* to log, nor does it care about the specific implementation. It simply requests the strategy it needs from the factory.

```csharp
public class PayrollProcessor
{
    // The Processor doesn't know about FileLogger or SplunkLogger.
    // It only knows about the ILogger interface.  It picks the 
    // destination to where to send its logs.
    private static ILogger Logger 
    { 
        [DebuggerStepThrough] 
        get; 
    } = GetLogger.ForDestination(LoggingDestination.File); // Or inject from config

    public void Process()
    {
        Logger.LogInfo("Processing started.");
        /* ... logic ... */
    }
}
```

### Why this is a Hill to Die On

By using the **Strategy Factory**:

* **Extensibility:** When we need to add `DatabaseLogger` or `CloudWatchLogger`, we create a new class, update the enum, and add one `case` statement to the factory. We **never** touch the `PayrollProcessor` or any other business logic.
* **Separation of Concerns:** The Business Logic is concerned with *when* to log. The Factory is concerned with *how* to resolve the logging dependency. The Strategies are concerned with *how* to communicate with their specific target.

This design ensures that our system is loosely coupled, highly extensible, and adheres strictly to the Open/Closed Principle.

### Example 2: The Configuration Concern

**The Problem:** Business logic often needs settings (like a timeout limit, a connection string, or a feature flag). Developers usually hardcode `ConfigurationManager.AppSettings["Timeout"]` directly into their classes. This is a nightmare. What if you want to move settings from a `.config` file to an encrypted database or a web-based secret manager? You'd have to find-and-replace that code in a dozen files.

**The xyLOGIX Way:** We encapsulate configuration behind an interface. The business logic asks for what it *needs*, not *where* it comes from.

```csharp
// 1. The Interface
public interface IAppSettings
{
    int ConnectionTimeout { get; }
    bool EnableFeatureX { get; }
}

// 2. The Implementation
public class AppSettings : IAppSettings
{
    public static IAppSettings Instance { [DebuggerStepThrough] get; } = new AppSettings();
    
    // We encapsulate the reading logic here, and here ONLY.
    public int ConnectionTimeout => Properties.Settings.Default.Timeout;
    public bool EnableFeatureX => Properties.Settings.Default.EnableFeatureX;
}

// 3. Factory
public static class GetAppSettings
{
    [DebuggerStepThrough]
    [return: NotLogged]
    public static IAppSettings SoleInstance() => AppSettings.Instance;
}
```

Now, your complex service class doesn't know about `Properties.Settings` or `app.config`.

```csharp
public class DatabaseService
{
    private static IAppSettings Config 
    { 
        [DebuggerStepThrough] 
        get; 
    } = GetAppSettings.SoleInstance();

    public void Connect()
    {
        int timeout = Config.ConnectionTimeout; // Clean, simple, decoupled.
        /* ... connect logic ... */
    }
}
```

### Why this is a Hill to Die On

When you decouple these "random" parts—Logging, Configuration, I/O, Printing, Notification—you create a system that is essentially a collection of **Black Boxes**.

* **The Business Logic** is a Black Box: It takes inputs and produces outputs. It shouldn't care if the logger prints to a screen or sends a smoke signal.
* **The Logger** is a Black Box: It just takes strings and writes them somewhere. It shouldn't care if the string came from a math calculation or a user error.
* **The Config** is a Black Box: It just holds values. It shouldn't care if it loaded them from an XML file or a registry key.

By ensuring these parts don't know about each other, you remove the "viral" nature of changes. You can rip out the Logger, the Config, or the UI, and the rest of the application remains completely indifferent. That is the true meaning of Loose Coupling.

## Some Exceptions/Twists on SRP

### SRP on the `Module` Level

As defined by the `copilot-code-instructions.txt` file, a `Module` identifies a group of related class library project(s) within the broader Visual Studio Solution, writ large.  Applying SRP on the `Module` level is important.  SRP is an important principle and it definitely should apply on the `Module` level but, at xyLOGIX, we are okay with grouping related functionality classes together in the same `Module` if it means that the containing `Module`, as a whole, can justify the set of those related components as still fulling the same or similar tasks, overall.

If there is a way to realistically group related classes, each of which have related responsibilities that all share the same general "thrust" of what the particular containing `Module` is being designed to expose as its core responsibility, then by all means.

### SRP on the Class Level

What SRP on the C# class level looks like, to us, is not necessary "only has one public method."  More, "only does one job."  If overload(s) of methods are necessary to do that job, or if there is more than one manner in which the job can be handled, the class can expose multiple public methods if they all serve the same overall purpose, writ large.  Such as an event bus -- it needs to be connected and disconnected and assist clients in managing the events; that's its "single responsibility."  It should be obvious, to even the most casual of observers, that to do that requires an interface that exposes more than just a single method.  Take validators, also, for example.  A validator object typically exposes an `IsValid` method.  Such methods generally, at xylOGIX, are intended to write messages to the logs as they run rules and validations on the data that they are tasked with accessing.  But, sometimes callers may not want extraneous logging; perhaps they need to call the validation frequently.  At xyLOGIX, we also write an `IsValidSilent` method to go along with the corresponding `IsValid` method; this has the same signature and similar XML documentation.  The exception is that, for an `IsValidSilent` method, neither the call of the method itself, nor anything it does (including trapping of `System.Exception`) writes info to the log file.  However, conceptually, the Single Responsibility Principle is still, in our mindset, being upheld.  This is due to the fact that both the `IsValid` and `IsValidSilent` method(s) carry out the same task, but one is logged and the other is not.  It is also acceptable for validators to have multiple `IsValidXYZ()` methods, where such methods are called for a target data object, such as a pipeline context, that is being filled in to a greater and greater extent, as a pipeline, chain, or playbook proceeds.

## Pipelines, Chains, Playbooks, Links, Steps, and Algorithms, Oh My!

We think there is merit to discussing the concepts of different architecture patterns for doing a sequence of computational steps.  To that end, we introduce the `Pipeline`, `Chain`, `Playbook`, `Step`, and `Algorithm` concepts.  `Pipeline`s, `Chain`s, and `Playbook`s are patterns that, conceptually, "inherit" from the `Strategy Factory` pattern, in that their steps are defined by an `enum`, and they have a common interface, a common, shared abstract base class across the concerete class(es), and concrete classes that define what occurs at each state; these are known, collectively, as `Link`(s), `Step`(s), or `Algorithm`(s).  There is also, apart from this, a top-level `Pipeline`, `Chain`, or `Playbook` object that actually executes the `Link`(s), `Step`(s), or `Algorithm`(s) in a correct order.  (FYI, backtick-delimited name(s) in this section, and its subsection(s), refer to the suffixes for the name(s) of the corresonding C# object(s).)  

There are architectural differences between a `Pipeline`, a `Playbook`, and a `Chain`.  Let's start with the `Chain`.

### A Chain of Responsibility -- `Chain` and `FallbackChain`

At xyLOGIX, sometimes we will write workflows whose successful completion are not guaranteed.  They operate a sequence of `Link`(s) in a specific order.  This order is a hard-coded array of the `enum` member(s) that identify each `Link`, ordered by the order in which the `Link`(s) are supposed to be evaluated.  These are to use the so-called `Chain of Responsibility` pattern.  The C# object to write for this (interface, common abstract base class, etc) will have a suffix of `Chain`.  Such a sequence of `Link`(s) (as the concrete workflow unit-of-work-defining object(s) are to be known and suffixed, like links in a chain, get it?) executes until one `Link` fails; if this happens, then the entire `Chain` aborts, and reports failure.  Right?  We mean to say, in the "real world," a chain is only as good as its weakest link.  If every `Link` reports success, then the chain reports success.

A slight variant of this is the following.  Sometimes, a `Chain` will stop before all `Link`(s) have been executed, not because one of them failed, but because we're doing a fallback, and we stop the `Chain` and report success because we accomplished what the chain was designed to accomplish; we just had to do trial-and-error by executing several `Link`(s) in sequence before either (a) we achieved our objectives, or (b) none of them worked. In such a `Chain`, we will use the suffix, `FallbackChain` and each `Link` will instead be called an `Approach`.

### Pipelines

Pipelines are what you think they are: workflows.  They operate a sequence of `Step`(s) in a specific order.  This order is a hard-coded array of the `enum` member(s) that identify each step, ordered by the order in which the `Step`(s) are supposed to occur.  

A `Pipeline` is a collection of two or more `Step`(s).  The only way a `Pipeline` comes to a stop prematurely, if it something is blocking the entire workflow from proceeding.  `Pipeline`(s) differ, conceptually, from `Chain`(s), `FallbackChain`(s), and `Playbook`(s) in that a `Pipeline` typically either (a) runs an inductive, task-based user interface to prompt the user for more and more information while guiding the user through a series of, e.g., wizard screens and this type of `Pipeline` stops prematurely, say, if the user clicks the **Cancel** button; or (b) runs a workflow but does so by accessing enabler(s) -- the file system, the network, REST APIs, databases, etc -- i.e., something the software system really has little to no control over whether it will work or not -- and then manipulating the data at hand based on what it retrieves.

### Playbooks

A `Playbook`, made up of a collection of `Algorithm`(s), is very similar to a `Pipeline`, but it also, conceptually, overlaps with `Chain` and `FallbackChain`.  As discussed earlier, conceptually, a `Playbook` is highly similar to a `Chain` or `FallbackChain`.  A single `Playbook` consists of two or more `Algorithm`(s) as its component and individual action-step/operation concrete objects, each of which is identified by an `enum` member in a manner not unlike a `Strategy Factory`.  In fact, `Playbook`(s) resemble `Chain`(s) or `FallbackChain`(s) more than anything else, but conceptually, they are different since they are meant to interpret the data in a specified `Context` object and come up with an answer (such as calculating a mathemtical formula or something).  `Playbook`(s) resemble `Chain`(s) and `FallbackChain`(s) in that they are potentially going to come to a stopping point before all `Algorithm`(s) have been evaluated (for whatever reason).  

### General Principles

BTW -- if any of a `Pipeline`, `Playbook`, `Chain`, or `FallbackChain` is going to otherwise be designed to only execute a single action step, then don't even bother.  Just create a single, dedicated component object that handles the task.  It's only when the sequence of operations numbers two or more, that one of these construct(s) will be created.

## Depend on the Narrowest Possible Contract

At xyLOGIX, we keep dependencies as narrow as possible.

A method, class, or module should depend only on the facts it truly needs.

If a method only needs an `enum` value and two `int` values, then pass the `enum` value and the two `int` values. Do not pass an entire dialog box, presenter, context object, or service object just because those objects happen to contain the values.

This keeps coupling low.

It also makes the code easier to test, easier to reuse, and easier to move later.

### Bad: Depending on a Whole Object

This is too much coupling:

```csharp
bool CanApplySelectedScope(
    IAssemblyProcessingContext context,
    IExecuteDialogBox dialogBox
);
````

The policy does not really care about the dialog box.

It also may not care about the whole context.

It only cares about the selected scope, the number of assembly files found, and the number of assembly files requested.

By passing the whole dialog box, we make the policy know about UI concerns.

That is a Separation of Concerns violation.

The dialog box is a source of user input. It is not the business rule.

### Good: Depending Only on Required Facts

This is better:

```csharp
bool CanApplySelectedScope(
    AssemblyProcessingScope scope,
    int assemblyCount,
    int assemblyCountToProcess
);
```

Now the policy depends only on the values it must inspect.

It does not know whether the values came from a dialog box, a command-line option, a config file, a test fixture, or another workflow step.

That is the point.

The policy should answer the business question.

The caller should gather the facts.

The applicator should mutate the target object only after the policy approves the choice.

### The Rule

Do not pass a big object when a small value will do.

Do not pass a UI object into business policy code when the policy only needs the values selected by the user.

Do not pass a workflow context into a policy method when the policy only needs a few scalar values from the context.

Ask this question before adding a dependency:

> What is the smallest thing this code must know in order to do its job?

Then depend on that.

### Why This Matters

Narrow dependencies reduce ripple effects.

If a dialog interface changes, a pure policy should not have to change.

If a context object changes, a rule that only needs three values should not have to change.

If we later replace the WinForms dialog with a command-line prompt, JSON config file, or automated test input, the policy should still work.

That is loose coupling.

That is SRP.

That is Dependency Inversion done carefully.

### Example: Clean Responsibility Split

For assembly-processing scope selection, the clean split is:

```plaintext
ExecuteDialogBox
    Collects user choices.

Presenter
    Reads the selected values from the dialog box.

Policy
    Decides whether the selected values are valid.

Applicator
    Applies the approved values to the assembly-processing context.

Pipeline / Orchestrator
    Coordinates the workflow.
```

Each part has one job.

The dialog box does not own business rules.

The policy does not know about UI.

The applicator does not decide whether the selection is valid.

The presenter does not duplicate policy logic.

This keeps the system easier to change without breaking unrelated code.

## Debugging Logging Statement(s) that Use the `?:` Operator

Hey, FYI, for logging such as the following:

```csharp
DebugUtils.WriteLine(
    result ? DebugLevel.Info : DebugLevel.Warning,
    result
        ? $"AssemblyProcessingScopePromptPolicy.ShouldProcessAllAssembliesWithoutPrompt: *** SUCCESS *** The count of assembly(ies) found in the scan directory, {assemblyCount}, is less than the maximum count that may be processed without prompting the user, {maximumUnpromptedAssemblyFileCount}.  The user does not need to be prompted."
        : $"AssemblyProcessingScopePromptPolicy.ShouldProcessAllAssembliesWithoutPrompt: *** WARNING *** The count of assembly(ies) found in the scan directory, {assemblyCount}, is greater than, or equal to, the maximum count that may be processed without prompting the user, {maximumUnpromptedAssemblyFileCount}.  The user must be prompted."
);
```

I am referring, in general, to logging that uses the `?:` operator in one or both of its argument(s).  The name of the containing class and the name of the containing method should NOT be included in the logging string, as a convention, for these type(s) of statement(s).

## About `WriteExtractionSummary` methods

I really wish you'd stop creating `WriteExtractionSummary` methods all over the place.  I know SRP, but writing a method simply to write one logging statement is trivial, stupid, and overenginering.  Never write a method whose job is to write a single logging messages.  The operation of writing a logging message never counts as a "reponsibility" for SRP.  The "R" of responsibility, to us at xyLOGIX, means, "an operation the software performs to benefit the user, besides writing logging messages."

## About Extracting Helpers

Reserve helper extraction for logic that is actually meaningful, reused, or large enough to justify its own name.  However, I hate classes that only expose a few `public` methods and then have (and I am exaggerating here) a billion-jillion `protected` or `private` helper methods.  That is, IMHO, insane, since that makes the class be bloated and points to an opportunity to break up the bloated class into several little classes/objects/singleton services/etc., which is really the better thing to do.
# The xyLOGIX C# Implementation Manual

The architectural principles above describe why xyLOGIX software is designed the way it is.  This part of the manifesto defines how those principles are applied in day-to-day implementation.  It consolidates the operational rules that had previously lived mainly in coding-assistant instructions and turns them into a human-readable engineering standard.

These rules are defaults for xyLOGIX software systems.  A repository-specific `CONTRIBUTING.md`, current source pattern, explicit product requirement, or direct maintainer instruction may specialize them.  When two rules conflict, use the most recent and most specific instruction.  Preserve established project behavior unless a requested change requires otherwise.

## Supported Toolchain and Compatibility Baseline

Unless a repository explicitly establishes a different baseline, xyLOGIX desktop software is written for:

- C# 7.3.
- .NET Framework 4.8.
- Windows Forms 2.0 when Windows Forms is involved.
- Windows as the supported operating system when the product is Windows-specific.
- Visual Studio 2022 Enterprise Edition.
- Legacy, non-SDK-style `.csproj` projects.
- `packages.config` where the existing solution uses that package-management model.
- NUnit 4.3.2 for unit tests.
- AlphaFS 2.2.6 for file-system work when AlphaFS is already part of the solution.
- Newtonsoft.Json 13.0.3 for JSON processing when JSON.NET is already part of the solution.
- log4net 3.0.3 for logging.
- PostSharp 2024.1.6 and its compatible pattern libraries when aspects are part of the design.
- Vsxmd 1.4.5 or xyLOGIX DocForge for transforming XML documentation into Markdown, as applicable to the repository.

Do not introduce language features or framework APIs that require a newer C# compiler, .NET Core, modern .NET, or a newer target framework unless the product is deliberately being retargeted.  SDK-style project syntax and modern target frameworks are not interchangeable concepts: SDK-style is a project format, while a target framework is a runtime/API contract.  Nevertheless, the standard xyLOGIX legacy solution should not be converted to SDK style merely for convenience.

Do not regenerate `GlobalAspects.cs` or `AssemblyInfo.cs` unless explicitly requested.  Do not create solution folders or project folders merely to organize source.  The namespace of a source file normally matches the exact name of the project containing it.

Before rolling a custom implementation of behavior that the .NET Framework may already provide, consult the official Microsoft documentation.  Use the framework implementation when it satisfies the required flexibility, fault tolerance, and compatibility.  Prefer a custom abstraction when direct framework coupling would make the system brittle or prevent the design from adapting to changing requirements.

## Source Files, Declaration Order, and Reference Order

A source file should normally contain one top-level class, interface, struct, enum, or delegate declaration.  Partial classes are the primary exception.  When reviewing a partial class, inspect the adjacent source files that declare its other parts before drawing conclusions about missing members, presenters, event handlers, or initialization logic.

Write source in reference order.  Define the code that has the fewest dependencies first, followed by code that depends on it.  Within a class, fields that back properties appear before those properties.  Supporting constants and fields appear before the members that consume them.  This keeps paste-in and incremental compilation workflows understandable and prevents avoidable errors caused by encountering a reference before its declaration has been supplied.

For a new Strategy Pattern implementation, generate and review the parts in this order:

1. The strategy enum.
2. The strategy interface.
3. The abstract base class that supplies shared Template Method services.
4. The concrete strategy classes.
5. The factory for each concrete singleton or concrete instance creator.
6. The strategy factory that selects the implementation.
7. The client or facade that determines the strategy and invokes it.

Preserve the current file's legitimate header comments exactly when modifying an existing file.  Header comments are the comments that appear before the first `using` directive.  Do not normalize, rewrite, reflow, or replace them unless the repository expressly requires a header change.  When creating a new file, use the repository-specific header from `CONTRIBUTING.md` verbatim.  A required repository header is an exception to the general preference for ASCII-only comments if the required header itself contains a non-ASCII character.

Do not add decorative file banners, `File:` banners, `Namespace:` banners, or separator regions that are not already part of the file.  Do not use `#region` or `#endregion`.

Do not use local functions.  When nested `try`/`catch` blocks or a substantial local algorithm suggest extraction, create a private method or, when the containing class is becoming unfocused, move the responsibility to a separate class or singleton service.  Do not extract a helper whose only purpose is to write one log message.

## Formatting and Code-Cleanup Boundaries

CodeMaid and JetBrains ReSharper are responsible for final wrapping, indentation, member layout, and cleanup.  Generated or handoff code should be syntactically correct and structurally faithful before cleanup; the cleanup tools may then reflow it according to the maintainer's configured rules.

Formatting differences are not substantive code changes.  Line wrapping, indentation, brace placement, expression wrapping, documentation wrapping, and member wrapping do not, by themselves, represent changes to behavior, architecture, algorithms, public APIs, data flow, validation, exception handling, or dependencies.

Except for preserved source-file headers and XML documentation, ordinary comments should occupy one physical line.  This rule applies to `//` comments and C-style `/* ... */` comments, including explanatory comments.  Newly written non-header comments should use ASCII characters only.

For XML documentation, place each opening and closing non-self-closing tag on its own line, with the documentation text between those tag lines.  Keep self-closing tags, such as `<para />`, `<see ... />`, and `<paramref ... />`, intact.  This is the pre-cleanup source shape; ReSharper may subsequently reflow it.

## Module and Project Taxonomy

A module is a family of related class-library projects.  The root project name identifies the capability, and optional suffix projects separate concerns.  Not every module requires every suffix.

| Project suffix | Responsibility |
| --- | --- |
| No suffix | Concrete classes and abstract base classes. |
| `.Actions` | Static action classes, normally named with short verbs.  Method names complete a readable phrase, such as `Format.FileAsImage(...)`. |
| `.Constants` | Public constants and enum declarations. |
| `.Displayers` | Static display actions for secondary Windows Forms windows and dialog boxes, commonly through a `Display` class. |
| `.Events` | Event-handler delegate declarations and their corresponding `EventArgs`-derived data classes. |
| `.Extensions` | Public static extension classes named `<Type>Extensions`. |
| `.Factories` | Static fluent factories, singleton accessors, and strategy selectors. |
| `.Helpers` | Cohesive helper and utility services shared by the module. |
| `.Interfaces` | Interfaces exposed by the module. |
| `.Tests` | NUnit fixtures and supporting test infrastructure. |

Action and factory classes should read fluently at the call site.  Examples include `Determine.TheModeToUse(...)`, `GetClassInfoValidator.SoleInstance()`, `MakeNewModel.FromScratch()`, and `GetRenderer.ForRenderingMode(mode)`.

Do not create a project simply to hold unrelated leftovers.  Each project and each class must have a cohesive responsibility.  Do not create circular project references.  Remember that an XML documentation `cref` can also create pressure to add a project reference; avoid a cross-reference that would force a circular dependency.  Use `<c>TypeName</c>` for a conceptual mention when a navigable cross-reference would introduce undesirable coupling.

## Magic Values, Constants, Resources, and User-Visible Text

Avoid unexplained literals.  Put reusable constant values in a suitable `.Constants` project or in an existing cohesive constants class.  Name constants according to the nominal category of information they expose.

Use an existing `Resources.resx` entry when the project already centralizes a diagnostic message there and the entry is appropriate.  Do not automatically move control captions, user-visible values, or dialog text into `Resources.resx`; follow the current repository's UI-text convention.  Do not create a resource entry merely to avoid a single, local, self-explanatory literal.

An enum belongs in the appropriate `.Constants` project.  A file name, attribute name, source-code construct, command-line switch, or other code-like term that cannot sensibly be cross-referenced in XML documentation should be enclosed in `<c>...</c>`.

## Coding to Abstractions and Narrow Contracts

Use the highest-level interface or abstract base class that still exposes the functionality required by the member.  This applies to fields, properties, method parameters, and return types.  Do not expose or depend on a concrete collection type when `IList<T>`, `ICollection<T>`, `IEnumerable<T>`, `IDictionary<TKey, TValue>`, or another narrower abstraction is sufficient.

Do not pass a large context, form, model, or workflow object to a policy that only needs a few values.  Gather the required facts at the orchestration boundary and pass the narrowest possible contract.  This reduces ripple effects, improves testability, and keeps business rules independent from UI and infrastructure.

Avoid constructor injection as a universal default.  xyLOGIX usually expresses Dependency Inversion through singleton services:

- A concrete singleton exposes a public static `Instance` property typed as its interface.
- A separate static factory exposes `GetXxx.SoleInstance()`.
- A client stores the dependency in a private static property typed as the interface.
- The property accessor is decorated with `[DebuggerStepThrough]`.

Constructor arguments are appropriate when an object's identity or immutable state genuinely depends on the supplied values, as with many `EventArgs` classes.  They should not be used merely to push a large dependency graph through every constructor in a system.

## Method Design: Result Variables, Fault Tolerance, and Early Gates

A method should have a clear default result.  Declare a local variable named `result` at the beginning of a non-void method and initialize it to the method's safe failure value.  Typical defaults include:

- `false` for a Boolean operation.
- `string.Empty` for a text-producing operation.
- `default` or `null` for an interface reference when no useful object can be returned.
- `Enumerable.Empty<T>()` or an empty array for a collection-producing operation.
- The enum's `Unknown` member for a selector or determination method.
- The input value for an intentionally idempotent transformation.

Wrap method bodies in `try`/`catch` blocks when called code can throw.  Catch `Exception` at the appropriate service boundary, log it, restore `result` to its documented default, and return that value.  Avoid throwing from ordinary validation paths.  Throw only when the contract specifically requires an exception or when continuing would hide a programming error that the selected strategy explicitly promises to reject.

Every call result must be treated as untrusted until validated.  Do not assume that a method succeeded merely because it returned.  Validate returned references, strings, collections, counts, enum values, and status flags before using them.  When an intermediate result is unusable, stop the operation or select a documented fallback.

Validate inputs eagerly and one condition at a time.  Do not combine independent eager-return validation gates with `&&` or `||`.  Each invalid condition should have its own log message, comment, and return statement.  This improves traceability and makes the exact rejected condition visible in the log.

Prefer negated, fail-fast gates:

```csharp
if (!condition)
    return result;

ContinueTheOperation();
```

This is preferred to deeply nesting the successful path inside an `if` block.  Keep cyclomatic complexity low and terminate invalid paths as early as possible.

Bounds-check values before use.  Validate indexes, lengths, dimensions, counts, enum values, numeric ranges, file paths, and other quantities even when an earlier caller was expected to validate them.  This shift-left practice protects against malformed inputs, stale assumptions, decompiled callers, memory corruption, and Single-Event Upsets.

Check that files and folders exist before opening, deleting, enumerating, or searching them.  Use AlphaFS when it is already the solution's file-system abstraction.  Validate that a path is of the required kind and, when required, that it is absolute.

### Boolean-result pattern

Do not initialize a Boolean `result` from a complex Boolean expression.  Perform the logged gates first, then affirmatively set `result` to `true` when the operation has succeeded.  Use the standard comment:

```csharp
/* If we made it this far with no Exception(s) getting caught, then assume that the operation(s) succeeded. */
result = true;
```

When a gate begins with a provisional successful value, set `result = true` before the test, perform the test, and restore `result = false` after the gate when execution continues into an unsuccessful path.  Match the established Live Template pattern in the repository.

### Incrementing and decrementing

Do not use `++` or `--`.  Use `Interlocked.Increment(ref value)` and `Interlocked.Decrement(ref value)` for counters.  This expresses the intent to make counter mutation atomic and prepares the code for later concurrent execution.

### Helper extraction

Extract a helper when the logic is meaningful, reusable, independently nameable, or large enough to obscure the caller.  Do not extract a one-line logging wrapper.  A class with a few public methods and a very large number of private helpers is a signal that responsibilities should be split into separate services, actions, strategies, pipeline steps, or policy objects.

## Logged Methods and Silent Methods

A normal logged method uses verbose, explicit gates.  Before each gate, write an informational message stating what is being checked.  On failure, write an error or warning message, log the result when the repository pattern requires it, and return the default result.  On success, write a success message and proceed.  Leave a blank source line after each `DebugUtils.WriteLine(...)` call in pre-cleanup generated code.

A method intentionally named with a `Silent` suffix is different.  It should perform the same validation gates without the verbose diagnostic commentary and routine `DebugUtils.WriteLine(...)` calls.  Do not make a silent method noisy in the name of consistency.

Do not create a helper whose entire responsibility is to write a single logging statement.  Logging supports a user-benefiting responsibility; it is not, by itself, a responsibility that justifies another method or class.

## PostSharp, Logging, and Diagnostic Conventions

Use PostSharp logging attributes consistently with the existing project.

- Apply `[Log(AttributeExclude = true)]` to constructors whose invocation would add noise.
- Static classes should define an explicit static constructor decorated with `[Log(AttributeExclude = true)]` so that `.cctor` calls are not logged.
- Never apply `[Log(AttributeExclude = true)]` to an enum.
- Global aspects already suppress logging of property getters and setters and event adders and removers; do not add `[return: NotLogged]` to properties merely because their type is complex.
- Apply `[NotLogged]` to method parameters that contain strings, objects, structs, collection references, XML nodes, framework objects, and other complex or non-scalar data.
- Treat `object` as complex.
- Apply `[return: NotLogged]` to methods that return a string, object, interface, collection, struct, or other non-primitive/complex value.

Use `DebugUtils.WriteLine(...)` for ordinary informational, success, warning, error, and debug messages.  Except for established exceptions, the message begins with the literal containing type and method name, such as `ClassInfoValidator.IsValid:`.  Do not use `GetType().Name` to construct the prefix.

A conditional logging statement that selects both its level and message with the `?:` operator is an established exception: omit the containing type and method prefix from the two conditional message strings when that is the current project convention.

Immediately before every `DebugUtils.LogException(ex);` call, place this exact ordinary comment:

```csharp
// dump all the exception info to the log
```

When XML documentation cross-references this method, use the full signature `M:xyLOGIX.Core.Debug.DebugUtils.LogException(System.Exception,System.Boolean)`.

## XML Documentation Standard

Every code entity must be documented, regardless of accessibility.  This includes public, protected, internal, and private classes, structs, interfaces, enums, enum members, delegates, events, fields, constants, properties, methods, constructors, and parameters.

XML documentation is part of the product.  It is transformed into Markdown and must read like professional Microsoft Learn-style documentation.

### Tag order

For a method, use applicable tags in this order:

1. `<summary>`
2. `<param>` tags in parameter order
3. `<remarks>`
4. `<returns>`
5. `<exception>` tags, when the method intentionally throws documented exceptions

Do not add tags that do not apply.  Do not use `<example>` sections.  Put illustrative material in `<remarks>` and separate distinct paragraphs with self-closing `<para />` tags.  Do not place `<para />` at the beginning or end of a `<remarks>` section.

### Cross-references

Use fully qualified `cref` values when doing so is semantically correct and does not create undesirable project coupling:

- `T:` for classes, interfaces, structs, delegates, and enums.
- `F:` for fields, constants, enum members, `System.String.Empty`, and `System.Guid.Empty`.
- `P:` for properties.
- `M:` for methods.
- `E:` for events.

Use `<paramref name="..." />` only when referring specifically to the method parameter.  A coincidentally identical word in ordinary prose does not need `<paramref>`.

Use `<c>...</c>` for file names, source constructs, attribute names, comments, command-line switches, literal examples, and conceptual type mentions that should not or cannot become navigable cross-references.  Enum projects are commonly dependency leaves, so an enum's documentation should often use `<c>InterfaceName</c>` for the strategy interface rather than creating a reverse project reference solely for a `cref`.

### Language keywords and type names

Use `<see langword="null" />`, `<see langword="true" />`, and `<see langword="false" />` when referring to those C# language keywords.  Do not use `<c>null</c>`, `<c>true</c>`, or `<c>false</c>`.  Use `<see langword="..." />` only when the prose actually refers to the language keyword.

Do not write the bare word `string` when the documentation refers to the .NET type.  Use `<see cref="T:System.String" />`.  Apply the same fully qualified approach to other framework types.

### Parameter documentation

Every `<param>` body begins with `(Required.)` or `(Optional.)`.

Describe a reference-type parameter as a reference to an instance of the relevant type.  Describe value types and primitive values as values, not references.  In xyLOGIX documentation, `System.String` is described as `A <see cref="T:System.String" /> ...`, not as a reference.  Use language that accurately distinguishes an object reference from a scalar or value.

When a sentence begins with a conditional, use the form `If ..., then ...`.

### Remarks and failure behavior

Method remarks should explain information that a caller needs to know, including alternate paths and behavior for invalid inputs.  State what happens when a reference is `null`, when text is blank or `System.String.Empty`, when a count or index is out of range, when a collection is empty, or when an exception is caught.

Use concise fault-tolerance wording.  For example:

> The method returns the `<default>` value in the event that it catches an `<Exception>`.  All such exception(s) are sent to the log file.

Do not write a long explanation of defensive programming when this concise statement communicates the contract.

### Field documentation

Document every field.  When a field backs or caches a property, the field summary should explain the property's purpose and the field's role.  Its remarks should include a note in this form:

```xml
<b>NOTE:</b> The purpose of this field is to cache the value of the <see cref="P:Fully.Qualified.Type.Property" /> property.
```

### Interface documentation

An interface summary begins with:

> Defines the publicly-exposed events, methods and properties of ...

Keep interface member documentation and implementation member documentation as close as possible.  Do not mention a private helper in documentation that must also describe the corresponding interface member.

### Pluralization and US English

Use American English.  When prose genuinely needs to allow singular and plural, use the xyLOGIX parenthetical plural style, such as `parameter(s)`, `box(es)`, and `discovery(ies)`.  Do not insert parenthetical plurals where grammar or meaning does not require them.

## Fields, Properties, and Accessors

Prefer auto-properties.  Introduce a backing field only when:

- Updating the property must raise an event.
- A read-only property caches an expensive or stable value.
- The current design requires additional state that cannot be expressed cleanly by an auto-property.

Place a backing field before the property it supports.  Prefix private fields with an underscore when that is the repository convention.

Decorate every getter and setter at accessor level with `[DebuggerStepThrough]`.  Add `using System.Diagnostics;` when required.  Do not add `[return: NotLogged]` to property getters; PostSharp global aspects already suppress accessor logging.

Do not alias a simple property value into a local variable merely for convenience.  Access the property directly so that the code clearly expresses the source of the value.  A local snapshot is appropriate when the property is volatile, computationally expensive, stateful, intentionally read once, or must remain stable throughout an operation.

## Events and Event Invocation

Do not declare an event that is never raised.  Every event must represent an actual observable state transition or action in the implementation.

Each event has a corresponding protected virtual invocator named `On<EventName>`.  The invocator invokes the event through the null-conditional operator.  Call the invocator rather than invoking the event directly elsewhere.

```csharp
protected virtual void OnChanged(EventArgs e)
    => Changed?.Invoke(this, e);
```

An event invocator is one of the narrow cases where a concise expression body may be retained when it matches current project style.  A class that exposes a protected virtual invocator cannot remain `sealed`.

For a property-sheet Apply operation, `DoApplyChanges` is an accepted special-case name when the method communicates an action rather than a conventional event notification.

Event delegates and `EventArgs`-derived classes belong in the module's `.Events` project.  Event-handler delegate names normally end with `EventHandler`.

## Enum and Validator Conventions

Put enum declarations in `.Constants` projects.  Document the enum and every member.

Unless protocol compatibility or persisted numeric values require otherwise:

- Arrange ordinary members alphabetically.
- Do not assign explicit numeric values to ordinary members.
- Place `Unknown` last.
- Assign `Unknown = -1` explicitly.

Do not decorate an enum with PostSharp logging attributes.

Validate enum inputs through the module's validator service rather than relying only on a cast or assuming that the value is defined.  A value can be outside the declared set.  A validator should normally reject `Unknown` for operations that require a concrete strategy.

## Collections, Enumeration, LINQ, and Concurrency

Do not enumerate an `IEnumerable<T>` repeatedly without understanding its source.  Multiple enumeration can repeat expensive work, advance a state machine, or observe different state.  Materialize when a stable snapshot or repeated access is required.

In code being prepared for multithreading or parallel processing, create a snapshot with `ToArray()` before iterating a mutable non-concurrent collection.  Iterate the snapshot rather than the live collection.  PostSharp `AdvisableCollection<T>` and related collection types may be used where their behavior is part of the design, but they do not remove the need to reason about concurrent mutation.

LINQ is acceptable in single-threaded, clear, non-critical code.  When optimizing a path for concurrency or predictable performance, replace LINQ chains with explicit `for` or `foreach` loops whose materialization and mutation behavior is apparent.  Do not assume that ordinary LINQ-to-Objects operations are thread-safe.

Avoid materialization that provides no benefit.  `ToArray()` and `ToList()` themselves enumerate and allocate.  Use them when a snapshot, stable count, index-based access, or protection against concurrent mutation justifies the cost.

Validate collection references, counts, and individual elements.  Skip unusable elements when the operation can produce a meaningful partial result.  If no useful elements remain, return the documented empty/default value.

## Strategy, Template Method, Chain, Pipeline, and Playbook Patterns

Use a strategy when behavior varies by a discrete mode and the client should not contain a growing set of behavioral branches.  A strategy family normally contains:

- An enum that names the available strategies.
- A focused interface.
- An abstract base class that provides common services through the Template Method pattern.
- One concrete class per strategy.
- One fluent singleton factory per concrete class.
- A strategy factory with a `switch` statement that maps the enum to the implementation.
- A determination action that selects the enum value from input data.

The strategy factory is the appropriate place for the selector `switch`.  Adding a strategy may require modifying this factory; that deliberate, centralized exception to strict OCP is preferable to duplicating selection branches throughout the codebase.

Name overridable Template Method hooks with an `On` prefix rather than `Do`, except for established action-oriented special cases such as `DoApplyChanges`.

Use a Chain of Responsibility when a request should pass through handlers until one can process it.  Use a fallback chain when later handlers are alternatives after earlier handlers decline or fail.  Use a pipeline when ordered stages transform or validate a value.  Use a playbook when the system selects a named sequence of operations for a scenario.  Keep each link, step, stage, or play focused on one responsibility.

Avoid a deeply nested set of more than one behavioral `if` branch when the branches represent stable, named strategies.  Prefer a strategy family and factory so that each behavior remains independently testable and replaceable.

## Singletons and Factory Naming

A singleton concrete class exposes an interface-typed `Instance` property.  Its constructor is non-public and decorated to suppress unnecessary constructor logging where appropriate.

A fluent accessor factory is a public static class named `GetXxx`.  Its sole public method is normally:

```csharp
[DebuggerStepThrough]
[return: NotLogged]
public static IXxx SoleInstance()
    => Xxx.Instance;
```

A factory that creates new objects is commonly named `MakeNewXxx` and uses a fluent method such as `FromScratch()` or `From(...)`.

A singleton strategy factory commonly uses `GetXxx.ForType(...)`, `ForMode(...)`, or `ForFormattingMethod(...)`.  A factory that creates new strategy instances uses `MakeNewXxx.OfType(...)` or similar wording.

## Windows Forms and UI/UX Conventions

xyLOGIX desktop UI follows classic Windows conventions, with the 1995 Microsoft Windows User Interface Guidelines and Windows 3.1 behavior serving as important references.

### Form base types and MVP

Windows Forms and dialog boxes should derive from `xyLOGIX.UI.Dark.Forms.DarkForm` in dark-theme projects.  Interfaces implemented by forms should inherit the xyLOGIX `IForm` abstraction when that is the module's established pattern, leaving the form-specific interface focused on application behavior.

Use Model-View-Presenter when a form contains meaningful orchestration or business interaction.  The form is the view, the presenter coordinates the workflow, and the model owns data access or domain interaction.  Business policy does not belong in control event handlers.

A form that uses MVP commonly has a `MyForm.Presenter.cs` partial-class file containing its `Presenter` property and `InitializePresenter` method.  Inspect all partial files before deciding that presenter behavior is missing.

### Fixed dialogs

Do not add a `MenuStrip`, `ToolStrip`, or `StatusStrip` to a form whose `FormBorderStyle` is `FixedDialog`.

### Control naming

Name designer fields in camelCase with a mandatory control-type suffix, such as `saveButton`, `projectComboBox`, `optionsTabControl`, `generalTabPage`, and `cloneNameTextBox`.  Button names include both the action and object; avoid vague names such as `addButton` when `addTextEditorButton` is possible.

### Button sizing and text

Standard push buttons are normally `87 x 27` pixels.  Forms use Segoe UI 9-point and `AutoScaleMode.Dpi` unless the existing product deliberately establishes another standard.

Use the established captions and mnemonic placement for OK, Cancel, Apply, Next, Browse, Add, Edit, Remove, Remove All, and Close buttons.  OK and Cancel buttons normally rely on `DialogResult` and should not receive redundant Click handlers.  Property-sheet Apply buttons and operational buttons do receive handlers.

### Tab order

Tab order follows visual reading order from upper-left to lower-right and top to bottom.  A label with a mnemonic receives the tab index immediately before the control that it labels.  Controls inside a container have their own local tab order.

### Control exposure and separation of concerns

Expose controls to a presenter only when the presenter must interact with them, and place those exposure properties in the form's main source file rather than `.Designer.cs`.  Keep generated designer code focused on control construction and layout.

Do not place business rules in the form.  The presenter gathers user choices, a policy validates them, an applicator updates the context, and an orchestrator coordinates the workflow.

## Testing Standard

Use NUnit for unit testing.  A concrete fixture normally corresponds to a concrete production class.  Share common test setup and behavior through abstract test base classes when multiple fixtures genuinely need the same service.

When the project uses the `xyLOGIX.Tests.Logging` module, derive fixtures from `LoggingTestBase` so PostSharp and log4net diagnostics are available during tests.

xyLOGIX uses a hybrid testing approach.  Write tests when the behavior is critical, subtle, regression-prone, security-sensitive, or difficult to verify manually.  Do not create low-value tests merely to achieve a coverage number.  Tests themselves must follow the same documentation, naming, fault-tolerance, and dependency standards where applicable.

## Dead Code and Repository Hygiene

Do not retain dead classes, interfaces, methods, fields, events, resources, or factories solely because they once existed.  Source control preserves history.  Remove obsolete components after confirming that the new architecture no longer calls them.

Do not remove apparently unused event members, partial-class members, reflection targets, serialization members, aspect targets, or designer-wired handlers without checking the complete solution and generated wiring.

Avoid circular project references, stale package references, duplicate constants, and abandoned strategies.  Keep dependency direction intentional.

## Git, Issues, Pull Requests, and Commit Communication

Issue and pull-request titles are not commit messages and should not use Comprehensive Commit formatting.  Make them concise and descriptive.  Technical identifiers and file names may be enclosed in backticks when that improves clarity.

Commit messages follow the repository's dedicated commit-message instructions.  The standard xyLOGIX commit message contains a present-tense verb-led topline, a blank line, and concise past-tense body bullets.  A single-file addition uses `Create <file name>`; a single-file modification uses `Update <file name>`.

Keep commits scoped to the actual diff.  Do not claim unperformed cleanup, future work, or unrelated changes.

## AI-Assisted Development and Code Handoff

AI assistance is a coding accelerator, not an authority over the repository.  The current source, repository guidance, maintainer instructions, and compiler remain authoritative.

When an AI assistant supplies C# source changes:

- Provide paste-in or paste-over code inline in a fenced block labeled `csharp`, unless the maintainer explicitly requests a downloadable artifact.
- Do not replace an inline-code request with an archive, patch, or generated-file link.
- Do not explain how to add files, references, or imports unless explicitly asked; Visual Studio, CodeMaid, and ReSharper provide those workflows.
- Preserve existing file headers and unchanged source faithfully.
- Generate the smallest amount of code required to solve the current problem while keeping each supplied snippet complete and buildable.
- Supply code in reference order.
- Do not treat cleanup-generated wrapping or indentation as a substantive change.
- Concentrate on behavior, APIs, logic, data flow, validation, exceptions, documentation meaning, and dependency direction.
- When a large change is naturally incremental, deliver it in coherent reference-ordered steps without abandoning completion of the current task.

The assistant should consult repository-level `CONTRIBUTING.md`, code instructions, current source, and ReSharper Live Template guidance before generating logged gates, validators, factories, result-variable methods, or exception paths.

## ReSharper Live Templates and Pre-Cleanup Code Shape

When repository Live Template guidance exists, treat it as the canonical shape for generated logged methods, validators, result variables, guard clauses, collection checks, console output, Debug output, and exception paths.

Generate source as though the appropriate Live Template had just been expanded before ReSharper cleanup.  This commonly means:

- Verbose pre-gate informational logging.
- Comprehensive one-line comments describing the gate and failure path.
- Error or warning logging on failure.
- Result logging before an early return when the template requires it.
- A blank line after each `DebugUtils.WriteLine(...)` call.
- Distinct patterns for logged and `Silent` methods.

Do not shorten these patterns merely because a compact equivalent would compile.  Consistency with the established template improves maintainability and diagnostic usefulness.

## Engineering Judgment and Narrowly Tailored Exceptions

The rules in this Manifesto establish the default standards for designing, implementing, documenting, reviewing, and maintaining xyLOGIX software. They are intended to promote consistency, reliability, maintainability, fault tolerance, and architectural clarity. They are not intended to override sound engineering judgment when the requirements of a particular piece of code demand a different approach.

A rule in this Manifesto may be bent or, when genuinely necessary, broken when strict compliance would conflict with a specific technical requirement such as thread safety, atomicity, reentrancy, memory visibility, correctness under concurrent access, security, interoperability, platform behavior, resource lifetime, data integrity, or another demonstrable engineering concern.

Any such exception must be evaluated on a case-by-case basis. The existence of an exception in one component does not establish a general exception for other components, even when the code appears superficially similar.

A deviation is warranted only when the normal xyLOGIX convention would be incorrect, unsafe, misleading, or materially less reliable for the particular operation being implemented. Personal preference, convenience, novelty, fashion, or a desire to use a newer technique are not sufficient reasons to disregard the Manifesto.

When a deviation is necessary, it must be limited to the smallest degree and narrowest scope required to satisfy the applicable technical requirement. Every unaffected xyLOGIX convention must remain in force. A localized exception must not be used as justification for abandoning unrelated documentation, logging, validation, architectural, naming, fault-tolerance, or code-organization standards.

Thread safety must be evaluated according to the actual behavior of the data structures, execution paths, and operations involved. Calling `ToArray()` or otherwise creating a collection snapshot may reduce exposure to changes made during later processing, but a snapshot does not automatically make access to the source collection thread-safe. Snapshot creation itself may be unsafe if another thread can mutate a collection that does not support concurrent enumeration.

When code is expressly required to be thread-safe, the implementation must use the mechanism that correctly addresses the actual concurrency problem. Depending on the circumstances, this may require an appropriate concurrent collection, immutable state, copy-on-write behavior, atomic operations through `Interlocked`, synchronization primitives, controlled ownership, message passing, thread confinement, or another narrowly selected mechanism. The mechanism should be no broader or more intrusive than necessary.

Likewise, use of `Interlocked` does not automatically make an entire method, object, or collection thread-safe. `Interlocked` protects only the specific atomic operation to which it is applied. Compound operations, check-then-act sequences, enumeration, updates involving multiple fields, and invariants spanning multiple values may require a different design.

Fault tolerance must not be implemented in a manner that conceals data corruption, violates an invariant, produces a misleading result, or allows an unsafe operation to continue. Returning a default value is preferred when doing so is meaningful and safe. When continuing would be more dangerous than failing, the implementation may reject the operation, surface the failure through an appropriate contract, or use another behavior justified by the circumstances.

Security requirements take precedence over stylistic conventions. Input validation, bounds checking, resource limits, path validation, type validation, authorization checks, cryptographic requirements, and protection against malformed or hostile input must be implemented according to the actual threat model. A convention must not be followed mechanically when doing so would create or preserve a security weakness.

Performance concerns may justify a deviation only when the relevant cost is material, measurable, or clearly unavoidable in the execution path under consideration. Premature optimization is not sufficient justification. When performance and a general convention conflict, correctness, safety, and maintainability remain the primary concerns unless the software's explicit requirements establish otherwise.

A non-obvious deviation should be documented close to the affected code when future maintainers would otherwise be likely to "correct" the implementation back into an unsafe or incorrect form. Such documentation should explain the technical reason for the exception, not merely state that the code is unusual.

Tests should verify the technical requirement that justified the deviation whenever practical. For thread-safe code, this may include tests for concurrent access, repeated execution, race-sensitive state transitions, idempotency, or preservation of invariants. A test that exercises only the single-threaded path is not sufficient evidence that a concurrency-specific implementation is correct.

If the technical condition that required an exception later ceases to exist, the code should be reconsidered and, when appropriate, brought back into alignment with the standard xyLOGIX convention.

The governing principle is therefore:

Apply the Manifesto by default. Deviate only when the particular code requires it. Make the smallest deviation necessary. Preserve every unaffected rule. Prefer correctness, safety, security, and demonstrable reliability over mechanical conformity.

## Pre-Commit Engineering Checklist

Before considering a source change complete, verify the following:

1. The code is compatible with C# 7.3 and .NET Framework 4.8.
2. The existing header and namespace are preserved.
3. No circular project dependency has been introduced.
4. Code appears in reference order.
5. Every code entity has appropriate XML documentation.
6. XML documentation uses fully qualified, semantically correct cross-references without forcing circular dependencies.
7. All applicable parameters and method return values use `[NotLogged]` or `[return: NotLogged]` correctly.
8. Every property accessor has `[DebuggerStepThrough]`.
9. Method inputs are validated eagerly and independently.
10. Indexes, lengths, counts, enum values, paths, and intermediate results are checked before use.
11. Methods return documented default values when validation or exception handling fails.
12. Every exception log call has the required preceding comment.
13. Logged gates follow the appropriate ReSharper Live Template pattern.
14. No local function, region, needless one-line logging helper, or dead code was introduced.
15. Collection enumeration and materialization are deliberate.
16. Events are raised through protected virtual invocators and are not declared without use.
17. Enum members follow the ordering and `Unknown = -1` convention unless compatibility requires another layout.
18. UI code respects MVP and established Windows dialog conventions.
19. Tests cover critical or regression-prone behavior where necessary.
20. ReSharper and CodeMaid formatting changes are not mistaken for behavioral changes.

## Revision K Consolidation Summary

Revision K preserves the architectural principles and examples from Revision H and adds the implementation rules that were previously distributed through coding-assistant instructions.  The principal additions are:

- The supported toolchain and compatibility baseline.
- Source-file ordering, header preservation, comment layout, and cleanup boundaries.
- Module/project suffix responsibilities.
- Result-variable, eager-validation, bounds-checking, and fault-tolerance patterns.
- PostSharp, `[NotLogged]`, `[DebuggerStepThrough]`, and diagnostic logging conventions.
- Comprehensive XML documentation rules.
- Field, property, event, and enum conventions.
- Collection snapshot, LINQ, and concurrency guidance.
- Strategy, Template Method, factory, chain, pipeline, and playbook implementation order.
- Windows Forms and MVP implementation standards.
- Testing, dead-code, Git communication, and AI-assisted handoff practices.
- A consolidated pre-commit engineering checklist.
- Incorporated guidance as to when to exercise engineering judgment to bend, break, and/or otherwise deviate from the SEM on a case by case basis.