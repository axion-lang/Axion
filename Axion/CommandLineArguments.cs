using System;
using System.Collections.Generic;
using Axion.Core;
using CommandLine;
using static Axion.Core.Mode;

namespace Axion {
    /// <summary>
    ///     Stores information about last
    ///     user arguments in command line.
    /// </summary>
    [Verb("axion", true)]
    public class CommandLineArguments {
        // @formatter:off
        public static readonly string HelpText = string.Join(
            Environment.NewLine,
            "┌─────┬──────────────────┬───────────────────────────────────────────────────────────────┐",
            "│     │ \"2 + 2\"          │ Input code to process.                                        │",
            "│ -i  │ --interactive    │ Start interactive interpreter mode.                           │",
            "├─────┼──────────────────┼───────────────────────────────────────────────────────────────┤",
            "│ -f  │ --files \"file\"   │ Input files to process (separated by ';').                    │",
            "│ -p  │ --project \"file\" │ Input project to process.                                     │",
            "│ -s  │ --stdlib \"file\"  │ Path to standard library.                                     │",
            "├─────┼──────────────────┼───────────────────────────────────────────────────────────────┤",
            "│ -m  │ --mode <value>   │ Source code processing mode (Default: compile). Available:    │",
            "│     │ " + nameof(Lexing) + "           │ | Generate tokens (lexemes) list.                             │",
            "│     │ " + nameof(Parsing) + "          │ | Generate syntax tree.                                       │",
            "│     │ " + nameof(Reduction) + "        │ | Generate syntax tree and reduce it.                         │",
            "│     │ " + nameof(Translation) + "      │ | Translate Axion code into target language.                  │",
            "│     │                  │ | | Available translation targets:                            │",
            "│     │                  │ | | " + string.Join(", ", Compiler.Converters) + ".                                    │",
            "├─────┼──────────────────┼───────────────────────────────────────────────────────────────┤",
            "│ -d  │ --debug          │ Save debug information to compiler output directory.          │",
            "│ -v  │ --version        │ Display information about compiler version.                   │",
            "├─────┼──────────────────┼───────────────────────────────────────────────────────────────┤",
            "│  h  │ help             │ Display this help screen.                                     │",
            "│ -e  │   --editor       │   Display interactive code editor's help screen.              │",
            "├─────┼──────────────────┼───────────────────────────────────────────────────────────────┤",
            "│  l  │ list             │                                                               │",
            "│ -f  │   --frontends    │   List available compiler frontends.                          │",
            "├─────┼──────────────────┼───────────────────────────────────────────────────────────────┤",
            "│ cls │ clear            │ Clear program screen.                                         │",
            "│  x  │ exit             │ Exit the compiler.                                            │",
            "└─────┴──────────────────┴───────────────────────────────────────────────────────────────┘"
        );
        
        // @formatter:on

        [Option('i', "interactive")]
        public bool Interactive { get; set; }

        [Value(0)]
        public string? Code { get; set; }

        [Option('f', "files", Separator = ';')]
        public IEnumerable<string> Files { get; set; } = null!;

        [Option('p', "project")]
        public string? Project { get; set; }

        [Option('m', "mode")]
        public string? Mode { get; set; }

        [Option('s', "stdlib")]
        public string? StdLib { get; set; }

        [Option('d', "debug")]
        public bool Debug { get; set; }

        [Option('v', "version")]
        public bool Version { get; set; }

        [Verb("list", aliases: new[] { "l" })]
        public class ListVerb {
            [Option('f', "frontends")]
            public bool Frontends { get; set; }
        }

        [Verb(
            "help",
            aliases: new[] {
                "h",
                "?"
            }
        )]
        public class HelpVerb {
            [Option('e', "editor")]
            public bool Editor { get; set; }
        }

        [Verb(
            "clear",
            aliases: new[] {
                "c",
                "cls"
            }
        )]
        public class ClearVerb { }

        [Verb("exit", aliases: new[] { "x" })]
        public class ExitVerb { }
    }
}
