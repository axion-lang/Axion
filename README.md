<img align="center" src="Other/Graphics/Axion_Mini.png" />

<h2 align="center">Welcome to Axion programming language toolset</h2>
<h3 align="center">:\:\:\:\:\:\:\:\:\: Under construction :/:/:/:/:/:/:/:/:/:</h3>

[![Build status](https://ci.appveyor.com/api/projects/status/ij2j74injuejodf2?svg=true)](https://ci.appveyor.com/project/F1uctus/axion)

### :open_file_folder: Repository consists of 3 parts:

- **Language core (lexer, parser, transpiler, etc.) (.NET Standard)**
- **A .NET Core wrapper around language core (mainly to provide unit-testing and launching)**
- **[CodeConsole](https://github.com/F1uctus/CodeConsole) project (console code editor with syntax highlighting) (a bit unstable)**

### :dart: Objectives:

- **Language union (provide inter-transpiling to other popular languages).**
- **Implementation of common design patterns quickly and simply.**
- **Convenient, simple and easy to read**
-  **Static typed with less annotations**
- **High-performance**

### :chart: Progress:

- **Console interactive interpreter (based on C# transpiling) and code editor**
- **Lexical analyzer**
- **Syntax parser**
- **C#, Python transpilers**
- **Transpiling to C/C++ or Rust is planned**

### :rocket: Launching:

You can launch compiler with `dotnet run -vq`
in `Axion.csproj` folder and type `-h` in console to get support
about arguments for compiler CLI interface.

Now compiler supports interpretation of Axion source
with `-i` option (through embedded code editor) and
also file processing with `-f "<path>.ax" -m <output_lang>` options.

Interpretation is performed by transpiling Axion to C# and running
it through Roslyn (still incomplete and doesn't support some syntax).

### :scroll: You can take a look at syntax in [project wiki](https://github.com/F1uctus/Axion/wiki)
