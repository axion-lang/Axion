<p align="center">
  <img src="misc/logo/Axion-mini.png" />
</p>

<h2 align="center">Welcome to Axion programming language toolset</h2>
<h3 align="center">:construction: Under construction :construction:</h3>

<p align="center">
  <a href="https://app.codacy.com/manual/f1uctus/Axion?utm_source=github.com&utm_medium=referral&utm_content=F1uctus/Axion&utm_campaign=Badge_Grade_Dashboard">
    <img src="https://api.codacy.com/project/badge/Grade/5c60bd255f884ed88bb9248155f8abac"
       alt="Codacy Badge">
  </a>
  <a href="https://ci.appveyor.com/project/F1uctus/axion">
    <img src="https://ci.appveyor.com/api/projects/status/ij2j74injuejodf2?svg=true"
       alt="Build Status">
  </a>
  <a href="https://gitmoji.carloscuesta.me">
    <img src="https://img.shields.io/badge/gitmoji-%20😜%20😍-FFDD67.svg?style=flat-square"
       alt="Gitmoji">
  </a>
</p>

<h3 align="center">:open_file_folder: Distribution</h3>

| Directory                                               | Contents                                                                | Platform         |
|---------------------------------------------------------|-------------------------------------------------------------------------|------------------|
| [`Axion`](Axion)                                        | User interface for compiler: CLI, ScriptBench editor, interpreter       | .NET             |
| [`Axion.Core`](Axion.Core)                              | Language core, lexer, parser, frontend interface, compiler API          | .NET Standard    |
| [`Axion.Frontend.Axion`](Axion.Frontend.Axion)          | Extension to translate Axion syntax tree back into Axion source code    | .NET Standard    |
| [`Axion.Frontend.CSharp`](Axion.Frontend.CSharp)        | Extension to translate Axion AST into C# source code                    | .NET Standard    |
| [`Axion.Frontend.Python`](Axion.Frontend.Python)        | Extension to translate Axion AST into Python source code                | .NET Standard    |
| [`Axion.Testing`](Axion.Testing)                        | NUnit-based unit tests for the toolset                                  | .NET             |
| [`CodeConsole`](https://github.com/F1uctus/CodeConsole) | Sub-repo for ScriptBench - console code editor (still unstable)         | .NET             |
| [`misc`](misc)                                          | Code examples, arts, etc.                                               |                  |

<h3 align="center">:dart: Key features</h3>

- **Expressiveness**

  Maintaining the "perfect balance" (© Thanos) between usage of expressive symbols and readable names.
  <br>
  Support for language-oriented programming (LOP) and macros system allows you to:

  - Add new language syntax of almost any complexity
  - Implement common design patterns quickly and simply

- **Inter-transpiling to other popular languages**

  At the moment Axon targets C# and Python code output. More languages are planned!
  
- **Static typing with less annotations**

  Support for full type soundness and type inferring (in design).
  
- **High-performance**

  Compile-time computations, macros expansion, and transpiling to fastest target language's code.

<h3 align="center">:chart: Progress</h3>

- **Lexical analyzer**
- **Syntax parser**
- **Interpreter (based on C# transpiling)**
- **Console code editor with syntax highlighting & error reporting**
- **C#, Python transpilers**

<h3 align="center">:rocket: Launching</h3>

You can launch compiler with `dotnet run` in `Axion.csproj` folder
and type `-h` in console to get support about arguments for compiler interface.

At the moment toolset supports interpretation of the Axion source
with `-i` CLI argument (through console code editor) and
file processing with `-f "<path>.ax" -m <to-output_lang>` arguments.

Interpretation is performed by transpiling Axion to C# and running
it through Roslyn (still incomplete and doesn't support some syntax).

<h3 align="center">:scroll: You can take a look at the language syntax in <a href="https://github.com/F1uctus/Axion/wiki">project wiki</a></h3>
