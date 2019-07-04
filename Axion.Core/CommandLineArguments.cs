using System;
using System.Collections.Generic;
using CommandLine;
using static Axion.Core.Processing.Source.SourceProcessingMode;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global
namespace Axion.Core {
  /// <summary>
  ///     Stores information about last
  ///     user arguments in command line.
  /// </summary>
  internal class CommandLineArguments {
        internal static readonly string HelpText = string.Join(
            Environment.NewLine,
            "┌─────────────────────────────┬───────────────────────────────────────────────────────────────┐",
            "│        Argument name        │                                                               │",
            "├───────┬─────────────────────┤                       Usage description                       │",
            "│ short │        full         │                                                               │",
            "├───────┼─────────────────────┼───────────────────────────────────────────────────────────────┤",
            "│  -i   │ --"
          + nameof(Interactive)
          + "       │ Launch compiler's interactive interpreter mode.               │",
            "├───────┼─────────────────────┼───────────────────────────────────────────────────────────────┤",
            "│       │ \"<"
          + nameof(Code)
          + ">\"            │ Input code to process.                                        │",
            "│  -f   │ --"
          + nameof(Files)
          + " \"<path>\"    │ Input files to process.                                       │",
            "│  -p   │ --"
          + nameof(Project)
          + " \"<path>\"  │ Input Axion project to process. [not available yet]           │",
            "├───────┼─────────────────────┼───────────────────────────────────────────────────────────────┤",
            "│  -m   │ --"
          + nameof(Mode)
          + " <value>      │ Source code processing mode (Default: compile). Available:    │",
            "│       │ "
          + nameof(Lex)
          + "                 │     Create tokens (lexemes) list from source.                 │",
            "│       │ "
          + nameof(Parsing)
          + "             │     Create tokens list and Abstract Syntax Tree from source.  │",
            "│       │ "
          + nameof(Interpret)
          + "           │     Interpret source code.                                    │",
            "│       │ "
          + nameof(ConvertCS)
          + "           │     Convert source to 'C#' language.                          │",
            "│       │ "
          + nameof(Compile)
          + "             │     Compile source into machine code.                         ├──┬── not available yet",
            "│       │ "
          + nameof(ConvertC)
          + "            │     Convert source to 'C' language.                           │  │",
            "│       │ "
          + nameof(ConvertCpp)
          + "          │     Convert source to 'C++' language.                         │  │",
            "│       │ "
          + nameof(ConvertJS)
          + "           │     Convert source to 'JavaScript' language.                  │  │",
            "│       │ "
          + nameof(ConvertPy)
          + "           │     Convert source to 'Python' language.                      ├──┘",
            "├───────┼─────────────────────┼───────────────────────────────────────────────────────────────┤",
            "│  -d   │ --"
          + nameof(Debug)
          + "             │ Save debug information to '<compilerDir>\\output' directory.   │",
            "│  -j   │ --"
          + nameof(AstJson)
          + "           │ Show resulting AST in JSON format in the console.             │",
            "│  -h   │ --"
          + nameof(Help)
          + "              │ Display this help screen.                                     │",
            "│  -v   │ --"
          + nameof(Version)
          + "           │ Display information about compiler version.                   │",
            "│  -x   │ --"
          + nameof(Exit)
          + "              │ Exit the compiler.                                            │",
            "└───────┴─────────────────────┴───────────────────────────────────────────────────────────────┘",
            " (Argument names aren't case-sensitive.)"
        );

        [Value(0)] public string Code { get; set; }

        [Option('f', "files", Separator = ';')]
        public IEnumerable<string> Files { get; set; }

        [Option('p', "proj")] public string Project { get; set; }

        [Option('m', "mode")] public string Mode { get; set; }

        [Option('i', "interactive")] public bool Interactive { get; set; }

        [Option('j', "astjson")] public bool AstJson { get; set; }

        [Option('r', "rewrite")] public bool Rewrite { get; set; }

        [Option('d', "debug")] public bool Debug { get; set; }

        [Option('x', "exit")] public bool Exit { get; set; }

        [Option("cls")] public bool ClearScreen { get; set; }

        [Option('h', "help")] public bool Help { get; set; }

        [Option('v', "verbose")] public bool Verbose { get; set; }

        [Option("version")] public bool Version { get; set; }
    }
}