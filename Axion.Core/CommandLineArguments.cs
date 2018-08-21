using System;
using System.IO;
using Axion.Core.Processing;
using Axion.Core.Visual;
using Axion.Core.Visual.ConsoleImpl;
using McMaster.Extensions.CommandLineUtils;
using McMaster.Extensions.CommandLineUtils.HelpText;

namespace Axion.Core {
    /// <summary>
    ///     Stores information about last
    ///     user query arguments in command line.
    /// </summary>
    [Command]
    internal class CommandLineArguments {
        internal static readonly CommandLineApplication App = new CommandLineApplication();
        internal static readonly string                 HelpText;

        static CommandLineArguments() {
            App.HelpTextGenerator = new HelpTextGenerator();

            #region Initializing options

            CommandOption files
                = App.Option("-f|--files", "Files to process by compiler.", CommandOptionType.MultipleValue);
            CommandOption<string> script
                = App.Option<string>("-s|--script", "Axion script to process by compiler.", CommandOptionType.SingleValue);
            CommandOption<string> mode
                = App.Option<string>("-m|--mode", "Compiler source processing mode (e.g. compile or interpret).", CommandOptionType.SingleValue);
            CommandOption<bool> gui
                = App.Option<bool>("-gui", "Determines if compiler should use IDE as default interface.", CommandOptionType.NoValue);
            CommandOption<bool> interactive
                = App.Option<bool>("-i|--interactive", "Determines if compiler should work as interactive interpreter.", CommandOptionType.NoValue);
            CommandOption<bool> debug
                = App.Option<bool>("-d|--debug", "Debug compiler mode enables saving JSON debug info.", CommandOptionType.NoValue);
            CommandOption<bool> exit
                = App.Option<bool>("-x|--exit", "Flag to exit the compiler.", CommandOptionType.NoValue);
            App.HelpOption("-h|-?|--help");
            App.VersionOption("-v|--version", Compiler.Version);

            #endregion

            #region Initializing help text

            HelpText =
                "┌─────────────────────────────┬───────────────────────────────────────────────────────────────┐\r\n" +
                "│        Argument name        │                                                               │\r\n" +
                "├───────┬─────────────────────┤                       Usage description                       │\r\n" +
                "│ short │        full         │                                                               │\r\n" +
                "├───────┼─────────────────────┼───────────────────────────────────────────────────────────────┤\r\n" +
                "│  -i   │ --" + nameof(interactive) + "       │ Launch compiler's interactive interpreter mode.               │\r\n" +
                "├───────┼─────────────────────┼───────────────────────────────────────────────────────────────┤\r\n" +
                "│  -f   │ --" + nameof(files) + "=\"<path>\"    │ Input files to process.                                       │\r\n" +
                "│  -s   │ --" + nameof(script) + "=\"<code>\"   │ Input script to process.                                      │\r\n" +
                "├───────┼─────────────────────┼───────────────────────────────────────────────────────────────┤\r\n" +
                "│  -m   │ --" + nameof(mode) +
                "=<value>      │ Source code processing mode (Default: compile). Available:    ├──┬── not available yet\r\n" +
                "│       │ " + nameof(SourceProcessingMode.Interpret) + "           │     Interpret source code.                                    │  │\r\n" +
                "│       │ " + nameof(SourceProcessingMode.Compile) + "             │     Compile source into machine code.                         │  │\r\n" +
                "│       │ " + nameof(SourceProcessingMode.ConvertC) + "            │     Convert source to 'C' language.                           │  │\r\n" +
                "│       │ " + nameof(SourceProcessingMode.ConvertCpp) + "          │     Convert source to 'C++' language.                         │  │\r\n" +
                "│       │ " + nameof(SourceProcessingMode.ConvertCSharp) + "       │     Convert source to 'C#' language.                          │  │\r\n" +
                "│       │ " + nameof(SourceProcessingMode.ConvertJavaScript) + "   │     Convert source to 'JavaScript' language.                  │  │\r\n" +
                "│       │ " + nameof(SourceProcessingMode.ConvertPython) + "       │     Convert source to 'Python' language.                      ├──┘\r\n" +
                "│       │ " + nameof(SourceProcessingMode.Lex) + "                 │     Create tokens (lexemes) list from source.                 │\r\n" +
                "│       │ " + nameof(SourceProcessingMode.Parsing) + "             │     Create tokens list and Abstract Syntax Tree from source.  │\r\n" +
                "├───────┼─────────────────────┼───────────────────────────────────────────────────────────────┤\r\n" +
                "│  -d   │ --" + nameof(debug) + "             │ Save debug information to '<compilerDir>\\output' directory.   │\r\n" +
                "│  -?   │ --help              │ Display this help screen.                                     │\r\n" +
                "│  -v   │ --version           │ Display information about compiler version.                   │\r\n" +
                "│  -x   │ --" + nameof(exit) + "              │ Exit the compiler.                                            │\r\n" +
                "└───────┴─────────────────────┴───────────────────────────────────────────────────────────────┘\r\n" +
                " (Argument names are not case-sensitive)\r\n";

            #endregion

            App.OnExecute(
                () => {
                    if (exit.HasValue()) {
                        Environment.Exit(0);
                    }
                    // Set debug option
                    bool debugMode = debug.HasValue();
                    // initialize UI
                    if (gui.HasValue()) {
                        throw new NotImplementedException("GUI (IDE) mode not implemented.");
                    }
                    // Interactive mode: jump into interpreter processing loop
                    if (interactive.HasValue()) {
                        ConsoleUI.InteractiveSession();
                        return 0;
                    }
                    // process source
                    {
                        SourceCode source;
                        // get source code
                        if (files.HasValue()) {
                            if (files.Values.Count > 1) {
                                ConsoleLog.Error("Compiler doesn't support multiple files processing yet.");
                                return 1;
                            }
                            Compiler.InputFiles = new FileInfo[files.Values.Count];
                            for (var i = 0; i < files.Values.Count; i++) {
                                Compiler.InputFiles[i] = new FileInfo(files.Values[i]);
                            }
                            source = new SourceCode(Compiler.InputFiles[0]);
                        }
                        else if (script.HasValue()) {
                            source = new SourceCode(script.ParsedValue.Trim('"'));
                        }
                        else {
                            ConsoleLog.Error("Neither script nor path to script file not specified.\n" + Compiler.HelpHint);
                            return 1;
                        }
                        if (!Enum.TryParse(mode.ParsedValue, true, out SourceProcessingMode processingMode)) {
                            processingMode = SourceProcessingMode.Compile;
                        }
                        var processingOptions = SourceProcessingOptions.CheckIndentationConsistency;
                        if (debugMode) {
                            processingOptions |= SourceProcessingOptions.SyntaxAnalysisDebugOutput;
                        }
                        // process source
                        source.Process(processingMode, processingOptions);
                    }
                    return 0;
                }
            );
        }
    }

    internal class HelpTextGenerator : IHelpTextGenerator {
        void IHelpTextGenerator.Generate(CommandLineApplication application, TextWriter output) {
            output.WriteLine(CommandLineArguments.HelpText);
        }
    }
}