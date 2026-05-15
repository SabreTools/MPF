# Coding Guide

This document serves as the official code standards guide development. Please note that this is a work in progress and may not encapsulate all standards expected of new or existing code. The included `.editorconfig` file can help enforce some of the standards mentioned below.

## General Code Guidelines

This section contains information on code standards regardless of which part of the project you are working in.

### Style and Naming

- Prefer `System` namespaces for supporting operations before external ones.

- Ordering of `using` statements goes:
  - `using System.*`
  - `using <Alphabetical>`
  - `using static <Alphabetical>`
  - `using X <Alphabetical> = Y`

- Use 4 spaces for `tab`.

- Curly braces should generally start on the line after but inline with the start of the previous statement, even if multiline.

    ```c#
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

    ```c#
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

- Methods and classes should use `PascalCase` for naming, even `internal` and `private` ones.

- Class properties should use `PascalCase` for naming, even `protected` and `internal` ones.

- Instance variables should use `camelCase` with a `_` prefix for naming, even `protected`, `internal`, and `private` ones.

- In-method variables should use `camelCase` without a `_` prefix for naming.

- Include explicit access modifiers for all class-level properties, variables, and methods.

- Avoid making everything `public`; only include the necessary level of access.

- Avoid making every method and class instance-based. Use `static` if your method does not need to access instance variables. Use `static` if your class only contains extensions or methods used by other classes.

- Null-coalescing and null-checking operators can be used to make more readable statements and better get across what a statement or string of statements is doing.

    ```c#
    if (obj?.Parameter is not null) { ... }

    bool value = DoSomething() ?? false;
    ```

- `#region` tags, including nested ones, can be used to both segment methods within a class and statements within a method. Indentation follows the surrounding code.

    ```c#
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

- Try to avoid use of other preprocessor directives unless consulting ahead of time with the maintainers.

- Interfaces should be listed in alphabetical order

    ```c#
    public class Example : IBindable, IComparable, IEquatable
    ```

- Use the `<inheritdoc/>` tag when possible to avoid out-of-date information.

    ```c#
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

### Methods

- Try to avoid including too much duplicate code across methods and classes. If you have duplicate code that spans more than ~5 lines, consider writing a helper method.

- Try to use expressive naming. e.g. use names like `PrintSectionTitles` and not `DoTheThing`.

- Try to avoid having too many parameters in a method signature. More parameters means more things interacting.

- Use method overloading to avoid unnecessary complexity in a single method.

    ```c#
    Instead of:

    Print(string idString, byte[] idArray, int idInt) { ... }

    You should:

    Print(string id) { ... }

    Print(byte[] id) { ... }

    Print(int id) { ... }
    ```

- Use optional parameters when the default value is the most common.

    ```c#
    Print(string id, bool toLower = false) { ... }
    ```

### `if-else` and `switch` statement syntax

- If all statements in the block are single-line, do not include curly braces.

    ```c#
    if (flag)
        DoSomething();
    else if (flag2)
        DoSomething2();
    else
        DoSomethingElse();
    ```

- If any of the statements is multi-line _or_ the `if-else` statement is multi-line, include curly braces.

    ```c#
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

    ```c#
    As an if-else statement:
    
    if (value == 1)
        DoValue1();
    else if (value == 2)
        DoValue2();
    else if (value == 3)
        DoValue3();
    else
        DoValueDefault();

    As a switch statement:

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

- If comparing against values for assignment, try to use a `switch` expression instead.

    ```c#
    As an if-else statement:
    
    int x;
    if (value == 1)
        x = 0;
    else if (value == 2)
        x = 1;
    else if (value == 3)
        x = 2;
    else
        x = -1;

    As a switch statement:

    int x = value switch
    {
        1 => 0,
        2 => 1,
        3 => 2,
        _ => -1,
    }
    ```

- When using a `switch` statement, if all switch cases are single-expression, they can be written in-line.  You can also add newlines between cases for segmentation or clarity.If the expressions are too complex, they should not be.

    ```c#
    switch (value)
    {
        case 1: DoValue1(); break;
        case 2: DoValue2(); break;
        case 3: DoValue3(); break;

        default: DoValueDefault(); break;
    }
    ```

- When using a `switch` expression, cases that lead to the same value can be combined using `or`. This is not required, especially if readability would be sacrificed.

    ```c#
    int x = value switch
    {
        1 or 2 => 0,
        3 or 4 => 1,
        5 or 6 => 2,
        _ => -1,
    }
    ```

- If any of the switch cases are multi-expression, write all on separate lines. You can also add newlines between cases for segmentation or clarity.

    ```c#
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

### Commenting

- All classes and methods should contain a `summary` block at bare minimum to explain the purpose. For methods, it is highly recommended to also include `param` tags for each parameter and a `return` tag if the method returns a value. Do not hesitate to use `remarks` as well to include additional information.

    ```c#
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

    ```c#
    // This code block does something important
    var x = SetXFromInputs(y, z);

    // This code block does something really,
    // really, really, really important and
    // I need multiple lines to say so
    var w = SetWFromInputs(x, y, z);
    ```

- Comments should be expressive and fully explain what is being described. Try to avoid using slang, "pointed comments" such as "you should" or "we do".

- Comments should avoid the use of first-person writing, such as "I think" or "We found".

- If comments include links, they can either be included as-is or using the `<see href="value"/>` tag

    ```c#
    // This information can be found from the following site:
    // <see href="www.regex101.com"/>
    ```

- Try to avoid using multiple, distinct comment blocks next to each other.

    ```c#
    // We want to try to avoid this situation where
    // we have multiple things to say.

    // Here, the statements are not inherently linked
    // but still need to go in the same area.
    //
    // But here the statements are logically linked but
    // needed additional formatting
    ```
