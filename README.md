<img align="center" src="Axion-mini.png" />

<h2 align="center">Welcome to Axion programming language toolset</h2>
<h3 align="center">:construction: Under construction :construction:</h3>

[![Build status](https://ci.appveyor.com/api/projects/status/ij2j74injuejodf2?svg=true)](https://ci.appveyor.com/project/F1uctus/axion)

### :open_file_folder: Distribution

| Directory        | Contents                                                                      | Platform       |
|------------------|-------------------------------------------------------------------------------|----------------|
| [`Axion`](Axion)             | Interface for compiler: CLI, ScriptBench editor, interpreter (C# code runner) | NET Core       |
| [`Axion.Core`](Axion.Core)   | Language core: lexer/parser/transpiler                                        | NET Standard   |
| [`CodeConsole`](https://github.com/F1uctus/CodeConsole) | Sub-repo for ScriptBench - console code editor (still unstable) | NET Core |

### :dart: Objectives

-  **Powerful language-oriented programming & macros system**
-  **Inter-transpiling to other popular languages**
-  **Implementation of common design patterns quickly and simply**
-  **Convenient, simple and easy to read**
-  **Static typed with less annotations**
-  **High-performance**

### :chart: Progress

-  **Console interactive interpreter (based on C# transpiling) and code editor**
-  **Lexical analyzer**
-  **Syntax parser**
-  **C#, Python transpilers**

### :rocket: Launching

You can launch compiler with `dotnet run -vq`
in `Axion.csproj` folder and type `-h` in console to get support
about arguments for compiler CLI interface.

At the moment toolset supports interpretation of the Axion source
with `-i` cli-argument (through console code editor) and
file processing with `-f "<path>.ax" -m <to-output_lang>` arguments.

Interpretation is performed by transpiling Axion to C# and running
it through Roslyn (still incomplete and doesn't support some syntax).

### :scroll: You can take a look at the language syntax in [project wiki](https://github.com/F1uctus/Axion/wiki)
