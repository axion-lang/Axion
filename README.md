<img align="center" src="Other/Graphics/Axion-mini.png" />

<h2 align="center">Welcome to Axion programming language toolset</h2>
<h3 align="center">:construction: Under construction :construction:</h3>

[![Codacy Badge](https://api.codacy.com/project/badge/Grade/5c60bd255f884ed88bb9248155f8abac)](https://app.codacy.com/manual/f1uctus/Axion?utm_source=github.com&utm_medium=referral&utm_content=F1uctus/Axion&utm_campaign=Badge_Grade_Dashboard)
[![Build status](https://ci.appveyor.com/api/projects/status/ij2j74injuejodf2?svg=true)](https://ci.appveyor.com/project/F1uctus/axion)

### :open_file_folder: Distribution

| Directory                                               | Contents                                                                      | Platform         |
|---------------------------------------------------------|-------------------------------------------------------------------------------|------------------|
| [`Axion`](Axion)                                        | Interface for compiler: CLI, ScriptBench editor, interpreter (C# code runner) | NET Core         |
| [`Axion.Modules`](Modules)                              | Built-in modules for language, written in Axion iteslf                        | Axion            |
| [`Axion.Core`](Axion.Core)                              | Language core: lexer/parser/transpiler                                        | NET Standard     |
| [`CodeConsole`](https://github.com/F1uctus/CodeConsole) | Sub-repo for ScriptBench - console code editor (still unstable)               | NET Core         |
| [`Other`](Other)                                        | Code examples, arts, etc.                                                     |                  |

### :dart: Objectives

-   **Powerful language-oriented programming & macros system**
-   **Inter-transpiling to other popular languages**
-   **Implementation of common design patterns quickly and simply**
-   **Convenient, simple and easy to read**
-   **Static typing with less annotations**
-   **High-performance**

### :chart: Progress

-   **Lexical analyzer**
-   **Syntax parser**
-   **Interpreter (based on C# transpiling)**
-   **Console code editor with syntax highlighting & error reporting**
-   **C#, Python transpilers**

### :rocket: Launching

You can launch compiler with `dotnet run -vq`
in `Axion.csproj` folder and type `-h` in console to get support
about arguments for compiler interface.

At the moment toolset supports interpretation of the Axion source
with `-i` CLI argument (through console code editor) and
file processing with `-f "<path>.ax" -m <to-output_lang>` arguments.

Interpretation is performed by transpiling Axion to C# and running
it through Roslyn (still incomplete and doesn't support some syntax).

### :scroll: You can take a look at the language syntax in [project wiki](https://github.com/F1uctus/Axion/wiki)
