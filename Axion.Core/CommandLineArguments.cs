using System.Collections.Generic;
using Axion.Core.Processing;
using CommandLine;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Axion.Core {
  /// <summary>
  ///     Stores information about last
  ///     user arguments in command line.
  /// </summary>
  internal class CommandLineArguments {
        internal const string HelpText =
            "┌─────────────────────────────┬───────────────────────────────────────────────────────────────┐\r\n"
          + "│        Argument name        │                                                               │\r\n"
          + "├───────┬─────────────────────┤                       Usage description                       │\r\n"
          + "│ short │        full         │                                                               │\r\n"
          + "├───────┼─────────────────────┼───────────────────────────────────────────────────────────────┤\r\n"
          + "│  -i   │ --"
          + nameof(Interactive)
          + "       │ Launch compiler's interactive interpreter mode.               │\r\n"
          + "├───────┼─────────────────────┼───────────────────────────────────────────────────────────────┤\r\n"
          + "│       │ \"<"
          + nameof(Code)
          + ">\"            │ Input code to process.                                        │\r\n"
          + "│  -f   │ --"
          + nameof(Files)
          + " \"<path>\"    │ Input files to process.                                       │\r\n"
          + "│  -p   │ --"
          + nameof(Project)
          + " \"<path>\"  │ Input Axion project to process. [not available yet]           │\r\n"
          + "├───────┼─────────────────────┼───────────────────────────────────────────────────────────────┤\r\n"
          + "│  -m   │ --"
          + nameof(Mode)
          + " <value>      │ Source code processing mode (Default: compile). Available:    ├──┬── not available yet\r\n"
          + "│       │ "
          + nameof(SourceProcessingMode.Interpret)
          + "           │     Interpret source code.                                    │  │\r\n"
          + "│       │ "
          + nameof(SourceProcessingMode.Compile)
          + "             │     Compile source into machine code.                         │  │\r\n"
          + "│       │ "
          + nameof(SourceProcessingMode.ConvertC)
          + "            │     Convert source to 'C' language.                           │  │\r\n"
          + "│       │ "
          + nameof(SourceProcessingMode.ConvertCpp)
          + "          │     Convert source to 'C++' language.                         │  │\r\n"
          + "│       │ "
          + nameof(SourceProcessingMode.ConvertCSharp)
          + "       │     Convert source to 'C#' language.                          │  │\r\n"
          + "│       │ "
          + nameof(SourceProcessingMode.ConvertJavaScript)
          + "   │     Convert source to 'JavaScript' language.                  │  │\r\n"
          + "│       │ "
          + nameof(SourceProcessingMode.ConvertPython)
          + "       │     Convert source to 'Python' language.                      ├──┘\r\n"
          + "│       │ "
          + nameof(SourceProcessingMode.Lex)
          + "                 │     Create tokens (lexemes) list from source.                 │\r\n"
          + "│       │ "
          + nameof(SourceProcessingMode.Parsing)
          + "             │     Create tokens list and Abstract Syntax Tree from source.  │\r\n"
          + "├───────┼─────────────────────┼───────────────────────────────────────────────────────────────┤\r\n"
          + "│  -d   │ --"
          + nameof(Debug)
          + "             │ Save debug information to '<compilerDir>\\output' directory.  │\r\n"
          + "│  -d   │ --"
          + nameof(AstJson)
          + "           │ Show resulting AST in JSON format to the console.             │\r\n"
          + "│  -h   │ --"
          + nameof(Help)
          + "              │ Display this help screen.                                     │\r\n"
          + "│  -v   │ --"
          + nameof(Version)
          + "           │ Display information about compiler version.                   │\r\n"
          + "│  -x   │ --"
          + nameof(Exit)
          + "              │ Exit the compiler.                                            │\r\n"
          + "└───────┴─────────────────────┴───────────────────────────────────────────────────────────────┘\r\n"
          + " (Argument names aren't case-sensitive; you can use '/' or '-' instead of '--'.)\r\n";

        [Value(0)]
        public string Code { get; set; }

        [Option('f', "files", Separator = ';')]
        public IEnumerable<string> Files { get; set; }

        [Option('p', "proj")]
        public string Project { get; set; }

        [Option('m', "mode")]
        public string Mode { get; set; }

        [Option('i', "interactive")]
        public bool Interactive { get; set; }

        [Option('j', "astjson")]
        public bool AstJson { get; set; }

        [Option('d', "debug")]
        public bool Debug { get; set; }

        [Option('x', "exit")]
        public bool Exit { get; set; }

        [Option('h', "help")]
        public bool Help { get; set; }

        [Option('v', "version")]
        public bool Version { get; set; }
    }
}