<img align="center" src="https://github.com/F1uctus/Axion/blob/csharp/Other/Graphics/Axion_Mini.png" />

<h2 align="center">Welcome to Axion programming language toolset</h2>
<h3 align="center">:\:\:\:\:\:\:\:\:\: Under construction :/:/:/:/:/:/:/:/:/:</h3>

### Objectives:

- **Language union (provide inter-transpiling to other popular languages).**
- **Implementation of common design patterns quickly and simply.**
- **Convenient, simple and easy to read**
-  **Static typed with less annotations**
- **High-performance**

### Progress:

- **Console interactive interpreter (based on C# transpiling) and code editor**
- **Lexical analyzer**
- **Syntax parser**
- **Transpiling to C/C++ or Rust is planned**

### Launching:

Now compiler supports interpretation of Axion source
with `-i` option (through embedded code editor) and
also file processing with `-f "<path>.ax" -m interpret` options.

Interpretation is performed by transpiling Axion to C# and running
it through Roslyn (still incomplete and doesn't support some syntax).

You can launch compiler with `axion.py` file
and type `-h` in console to get support
about arguments for compiler CLI interface.

### You can take a look at syntax in [project wiki](https://github.com/F1uctus/Axion/wiki)