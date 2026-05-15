# Coding Guide

This document serves as the official code standards guide development. Please note that this is a work in progress and may not encapsulate all standards expected of new or existing code. The included `.editorconfig` file can help enforce many of the standards mentioned below.

There are no requirements on a coding environment or IDE in order to contribute. If there are issues with a particular environment, please surface an issue with the project. If there are support files or updates needed to items such as the `.gitignore`, please open an issue or PR to assist the maintainers. Consider using an editor that can take advantage of the `.editorconfig` file to enable automatic enforcement.

## External Code

Generally, the `System` and `SabreTools` libraries are going to be preferred over all external code. If external code is needed, reasonable alternatives need to be explored before it will be included. External code use can also be seen as an opportunity to include that functionality in a `SabreTools` project directly.

External code should first be considered in the form of a Nuget package to allow for easier versioning and tracking. If this is not viable, consult with a maintainer to see if including the code as a Git submodule or having a direct copy of the code included would be reasonable.

## General Coding Style

There are some general coding style preferences that should be adhered to:

- Use 4 spaces for `tab`.

- Curly braces should generally start on the line after but inline with the start of the previous statement, even if multiline.

    ```csharp
    if (flag)
    {
        DoSomething();
    }
    else if (flag2
        && flag3)
    {
        DoSomething2();
    }
    ```

- Multi-line statements need to have following lines indented by one step at minimum.

    ```csharp
    if (flag)
    {
        DoSomething();
    }
    else if (flag2
        && flag3
        && (flag4
            || flag5))
    {
        DoSomething2();
    }
    ```

- Null-coalescing and null-checking operators can be used to make more readable statements and better get across what a statement or string of statements is doing.

    ```csharp
    if (obj?.Parameter is not null) { ... }

    bool value = DoSomething() ?? false;
    ```

- `#region` tags, including nested ones, can be used to both segment methods within a class and statements within a method. Indentation follows the surrounding code. These should be preferred over comments that indicate large blocks of related operations.

    ```csharp
    #region This is the first region

    public static void Method()
    {
        #region This is an in-code region

        DoSomething();

        #endregion

        DoSomething2();
    }

    #endregion
    ```

- Try to avoid use of preprocessor directives other than `#region`, `#endregion`, `#if`, `#elif`, and `#else` unless consulting ahead of time with the maintainers. Exceptions may be made for copied `#pragma` use, especially for known style violations.

## Naming Conventions

With the exception of directly-included external code, the following naming conventions should be observed:

- Methods and classes should use `PascalCase` for naming, even `internal` and `private` ones.

- Properties should use `PascalCase` for naming, even `protected` and `internal` ones.

- Fields should use `camelCase` with a `_` prefix for naming, even `protected`, `internal`, and `private` ones.

- Constants should use `PascalCase` for naming, even `protected`, `internal`, and `private` ones.

- In-method variables should use `camelCase` without a `_` prefix for naming.

- Avoid using `_` in the middle of any names that are included in library code. The exception to this rule are test methods where they are used to separate parts of the test description. e.g. `public void MethodName_NullInput_False() { ... }`

## Access Modifiers

Generally, all classes, methods, properties, and fields should include explicit access modifiers. This applies even if the assigned access modifier would be the same as the default value in C\#. Below are some other guidelines to be aware of around access:

- Avoid making everything `public`; only include the necessary level of access, including accounting for any tests that need to be written.

- Avoid making every method and class instance-based. Use `static` if your method does not need to access instance variables. Use `static` if your class only contains extensions or methods used by other classes.

## `using` Statements

Ordering of `using` statements goes:

```csharp
// System.* namespaces in alphabetical order
using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;

// External namespaces in alphabetical order
using Newtonsoft.Json;
using SabreTools.IO;
using SabreTools.IO.Extensions;
using Xunit;

// Static usings in alphabetical order
using static SabreTools.Data.Models.BFPK.Constants;
using static SabreTools.Data.Models.PKZIP.Constants;

// Renamed usings in alphabetical order of local name
using CompressionMode = System.IO.Compression.CompressionMode;
using IOSeekOrigin = System.IO.SeekOrigin;
```

There should be no comments or additional newlines in between `using` statements. The newlines and comments in the above example are to help explain, rather than act as a direct guide.

There may be a need to version-gate some `using` statements if only certain .NET versions require particular code. If this is the case, all namespaces are still listed in the order above, with the preprocessor directives directly contained within. For example:

```csharp
using System;
#if NETCOREAPP
using System.Collections.Concurrent;
#else
using System.Collections.Generic;
#endif
using System.IO;
```

## Classes

Classes are considered to be logical groupings of functionality and should be treated as public-by-default. All non-private classes should be put into their own separate files. This includes typed-overrides, e.g. `Thing` and `Thing<T>`. Below are some other general guidelines for classes:

- Static classes are allowed, but they should not be the default case.

- Partial classes are allowed and are even recommended when there needs to be further logical breakdown of functionality. Common uses of partial classes include separating out interface implementations.

- If multiple interfaces are implemented, they should be listed in alphabetical order. If there are exceptions to this, they will be mentioned in project-specific developer guides.

    ```csharp
    public class Example : IBindable, IComparable, IEquatable
    ```

- Private embedded classes are allowed, but should generally avoided if at all possible. This helps to both avoid discoverability issues as well as better help enforce separation of concern. If an embedded class is desired, talk to a maintainer before doing so.

## Methods

Methods should attempt to limited in size and scope wtihin reason. Methods that do too much are at risk of being untestable or cause maintenence issues. If a method has logical groupings of statements that could act as a standalone method by themselves, then consider doing so. Below are some other general guidelines for methods:

- Use the `<inheritdoc/>` tag when possible to avoid out-of-date information.

    ```csharp
    public interface IInterface
    {
        /// <summary>
        /// Summary to inherit
        /// </summary>
        void DoSomething();
    }

    public class Example : IInterface
    {
        /// <inheritdoc/>
        public void DoSomething() { ... }
    }
    ```

- Try to avoid including too much duplicate code across methods and classes. If you have duplicate code that spans more than ~5 lines, consider writing a helper method.

- Try to use expressive naming. e.g. use names like `PrintSectionTitles` and not `DoTheThing`.

- Try to avoid having too many parameters in a method signature. More parameters means more possible compounded issues.

- Use method overloading if there are multiple, fully-distinct paths within the method. This can help avoid unnecessary complexity in a single method.

    ```csharp
    // Instead of:
    public void Print(string? idString, byte[]? idArray, int? idInt)
    {
        if (idString is not null) { ... }
        else if (idArray is not null) { ... }
        else if (idInt is not null) { ... }
    }

    // You should:
    public void Print(string? id) { ... }

    public void Print(byte[] id) { ... }

    public void Print(int id) { ... }
    ```

- Use optional parameters when the default value is the most common. Method overloads are also acceptable to use here as well, depending on developer and maintainer preference.

    ```csharp
    // Using default parameters
    public void Print(string id, bool toLower = false) { ... }

    // Using method overloads
    public void Print(string id) => Print(id, toLower: false);

    public void Print(string id, bool toLower) { ... }
    ```

## `if-else` and `switch` statement syntax

There are multiple approaches to selections within code. There is often not a "best" solution, but the following guidelines should help guide toward a solution that both developers and maintiners can agree on.

- If all statements in the block are single-line, do not include curly braces.

    ```csharp
    if (flag)
        DoSomething();
    else if (flag2)
        DoSomething2();
    else
        DoSomethingElse();
    ```

- If any of the statements is multi-line _or_ the `if-else` statement is multi-line, include curly braces.

    ```csharp
    if (flag)
    {
        DoSomething();
    }
    else if (flag2
        && flag3
        && flag4)
    {
        DoSomething2();
    }
    else
    {
        DoSomethingElse();
        DoSomethingEvenMore();
    }
    ```

- If comparing against values, try to use a `switch` statement instead.

    ```csharp
    // As an if-else statement:
    if (value == 1)
        DoValue1();
    else if (value == 2)
        DoValue2();
    else if (value == 3)
        DoValue3();
    else
        DoValueDefault();

    // As a switch statement:
    switch (value)
    {
        case 1:
            DoValue1();
            break;
        case 2:
            DoValue2();
            break;
        case 3:
            DoValue3();
            break;
        default:
            DoValueDefault();
            break;
    }
    ```

- If comparing against values for assignment, try to use a `switch` expression instead. Please note that this may not always be possible depending on the types that are being compared.

    ```csharp
    // As an if-else statement:
    int x;
    if (value == 1)
        x = 0;
    else if (value == 2)
        x = 1;
    else if (value == 3)
        x = 2;
    else
        x = -1;

    // As a switch statement:
    int x = value switch
    {
        1 => 0,
        2 => 1,
        3 => 2,
        _ => -1,
    }
    ```

- When using a `switch` statement, if all switch cases are single-expression, they can be written in-line.  You can also add newlines between cases for segmentation or clarity. If a single case ends up overly-complex, consider separating it out into a new method and then calling that method instead.

    ```csharp
    switch (value)
    {
        case 1: DoValue1(); break;
        case 2: DoValue2(); break;
        case 3: DoValue3(); break;

        default: DoValueDefault(); break;
    }
    ```

- When using a `switch` expression, cases that lead to the same value can be combined using `or`. This is not required, especially if readability would be sacrificed.

    ```csharp
    int x = value switch
    {
        1 or 2 => 0,
        3 or 4 => 1,
        5 or 6 => 2,
        _ => -1,
    }
    ```

- If any of the switch cases are multi-expression, write all on separate lines. You can also add newlines between cases for segmentation or clarity.

    ```csharp
    switch (value)
    {
        case 1:
            DoValue1();
            break;
        case 2:
            DoValue2();
            break;
        case 3:
            DoValue3();
            break;

        default:
            DoValueDefault();
            DoValueAsWell();
            break;
    }
    ```

## Commenting

Comments can be essential to the understanding of a given block of code. They can also be very helpful for figuring out appropriate use of the code and even sometimes help track down issues. Below are some other general guidelines for commenting:

- All classes and methods should contain a `summary` block at bare minimum to explain the purpose. For methods, it is highly recommended to also include `param` tags for each parameter and a `return` tag if the method returns a value. Do not hesitate to use `remarks` as well to include additional information.

    ```csharp
    /// <summary>
    /// This class is an example
    /// </summary>
    /// <remarks>
    /// This class does nothing but it is useful to demonstrate
    /// coding standards.
    /// </remarks>
    public class Example
    {
        /// <summmary>
        /// This property is the name of the thing
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// This method is an example method
        /// </summary>
        /// <param name="shouldPrint">Indicates if the value should be printed</param>
        /// <returns>A value between 1 and 10, or null on error</returns>
        public static int? PrintAndReturn(bool shouldPrint)
        {
            ...
        }
    }
    ```

- In-code comments should use the `//` syntax and not the `/* */` syntax, even for multiple lines.

    ```csharp
    // This code block does something important
    var x = SetXFromInputs(y, z);

    // This code block does something really,
    // really, really, really important and
    // I need multiple lines to say so
    var w = SetWFromInputs(x, y, z);
    ```

- Comments should be expressive and fully explain what is being described. Try to avoid using slang or statements that may have double meanings. If you ware unsure if a comment will be read incorrectly, please consult with a maintainer.

- Comments should avoid the use of first-person writing and "pointed comments", such as "I think", "We found", or "You should".

- If comments include links, they can either be included as-is or using the `<see href="value"/>` tag. If the link is being included in a class- or method-level comment, prefer using the latter option.

    ```csharp
    // This information can be found from the following site:
    // <see href="www.regex101.com"/>
    ```

- Try to avoid using multiple, distinct comment blocks next to each other.

    ```csharp
    // We want to try to avoid this situation where
    // we have multiple things to say.

    // Here, the statements are not inherently linked
    // but still need to go in the same area.
    //
    // But here the statements are logically linked but
    // needed additional formatting
    ```
