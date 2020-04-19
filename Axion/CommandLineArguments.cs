using System;
using System.Collections.Generic;
using CommandLine;
using static Axion.Core.Source.ProcessingMode;
using static Axion.Core.Source.ProcessingOptions;

namespace Axion {
    /// <summary>
    ///     Stores information about last
    ///     user arguments in command line.
    /// </summary>
    public class CommandLineArguments {
        // @formatter:off
        public static readonly string HelpText = string.Join(
            Environment.NewLine,
            "┌─────────────────────────────┬───────────────────────────────────────────────────────────────┐",
            "│        Argument name        │                                                               │",
            "├───────┬─────────────────────┤                       Usage description                       │",
            "│ short │        full         │                                                               │",
            "├───────┼─────────────────────┼───────────────────────────────────────────────────────────────┤",
            "│  -i   │ --" + nameof(Interactive) + "       │ Launch compiler's interactive interpreter mode.               │",
            "├───────┼─────────────────────┼───────────────────────────────────────────────────────────────┤",
            "│       │ \"<" + nameof(Code) + ">\"            │ Input code to process.                                        │",
            "│  -f   │ --" + nameof(Files) + " \"<path>\"    │ Input files to process.                                       │",
            "├───────┼─────────────────────┼───────────────────────────────────────────────────────────────┤",
            "│  -m   │ --" + nameof(Mode) + " <value>      │ Source code processing mode (Default: compile). Available:    │",
            "│       │ " + nameof(Lexing) + "              │     Generate tokens (lexemes) list.                           │",
            "│       │ " + nameof(Parsing) + "             │     Generate syntax tree.                                     │",
            "│       │ " + nameof(Reduction) + "           │     Generate syntax tree and reduce it.                       │",
            "│       │ ToCSharp            │     Convert source to 'C#' language.                          │",
            "│       │ ToPython            │     Convert source to 'Python' language.                      │",
            "├───────┼─────────────────────┼───────────────────────────────────────────────────────────────┤",
            "│  -d   │ --" + nameof(Debug) + "             │ Save debug information to '<compilerDir>\\output' directory.   │",
            "│  -h   │ --" + nameof(Help) + "              │ Display this help screen.                                     │",
            "│  -h   │ --" + nameof(EditorHelp) + "        │ Display interactive code editor's help screen.                │",
            "│       │ --" + nameof(Cls) + "               │ Clear program screen.                                         │",
            "│  -v   │ --" + nameof(Version) + "           │ Display information about compiler version.                   │",
            "│  -x   │ --" + nameof(Exit) + "              │ Exit the compiler.                                            │",
            "└───────┴─────────────────────┴───────────────────────────────────────────────────────────────┘",
            " (Argument names aren't case-sensitive.)"
        );
        // @formatter:on

        [Option('i', "interactive")]
        public bool Interactive { get; set; }

        [Value(0)]
        public string Code { get; set; }

        [Option('f', "files", Separator = ';')]
        public IEnumerable<string> Files { get; set; }

        [Option('m', "mode", Default = "")]
        public string Mode { get; set; }

        [Option('d', "debug")]
        public bool Debug { get; set; }

        [Option("cls")]
        public bool Cls { get; set; }

        [Option('h', "help")]
        public bool Help { get; set; }

        [Option("editorhelp")]
        public bool EditorHelp { get; set; }

        [Option('v', "version")]
        public bool Version { get; set; }

        [Option('x', "exit")]
        public bool Exit { get; set; }
    }
}
