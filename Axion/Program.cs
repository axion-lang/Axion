using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;
using Axion.Core;
using Axion.Core.Hierarchy;
using Axion.Core.Processing.Errors;
using Axion.Core.Processing.Translation;
using Axion.Specification;
using CodeConsole;
using CodeConsole.ScriptBench;
using CommandLine;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Scripting;
using NLog;
using NLog.Layouts;
using Module = Axion.Core.Hierarchy.Module;

namespace Axion {
    public static class Program {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private static SimpleLayout logLevel {
            get => LogManager.Configuration.Variables["consoleLogLevel"];
            set {
                LogManager.Configuration.Variables["consoleLogLevel"] = value;
                LogManager.Configuration.Variables["fileLogLevel"]    = value;
                LogManager.ReconfigExistingLoggers();
            }
        }

        public static readonly Assembly[] DefaultImports = {
            typeof(Enumerable).Assembly,
            typeof(BigInteger).Assembly
        };

        public static void Main(string[] arguments) {
            var cliParser = new Parser(
                settings => {
                    settings.EnableDashDash = true;
                    settings.CaseSensitive  = false;
                    settings.AutoHelp       = false;
                    settings.AutoVersion    = false;
                    settings.HelpWriter     = null;
                }
            );
            Directory.CreateDirectory(Compiler.OutDir);

            Console.InputEncoding   = Console.OutputEncoding = Encoding.UTF8;
            Console.ForegroundColor = ConsoleColor.White;
            PrintIntro();

            const string namespacePrefix = "Axion.Emitter.";

            var dlls = new DirectoryInfo(Compiler.WorkDir)
                       .EnumerateFiles()
                       .Where(fi => fi.Name.StartsWith(namespacePrefix)
                                 && fi.Extension == ".dll");

            foreach (var fileInfo in dlls) {
                var shortName = Path.GetFileNameWithoutExtension(fileInfo.FullName);
                try {
                    var asmName = new AssemblyName(shortName);
                    var asm = new EmitterLoadContext(fileInfo.FullName)
                        .LoadFromAssemblyName(asmName);
                    shortName = shortName.Replace(namespacePrefix, "");
                    Compiler.AddTranslator(shortName, LoadTranslator(asm));
                }
                catch (Exception e) {
                    logger.Error(
                        $"Failed to load frontend from {shortName}.\n{e}"
                    );
                }
            }

            // main processing loop
            while (true) {
                if (arguments.Length > 0) {
                    var result = cliParser
                        .ParseArguments<CommandLineArguments,
                            CommandLineArguments.ListVerb,
                            CommandLineArguments.HelpVerb,
                            CommandLineArguments.ClearVerb,
                            CommandLineArguments.ExitVerb>(arguments);
                    result
                        .MapResult<CommandLineArguments,
                            CommandLineArguments.ListVerb,
                            CommandLineArguments.HelpVerb,
                            CommandLineArguments.ClearVerb,
                            CommandLineArguments.ExitVerb, int>(
                            args => {
                                if (args.Version) {
                                    return Version();
                                }

                                if (args.Interactive) {
                                    EnterInteractiveMode();
                                    return 0;
                                }

                                logLevel = args.Debug ? "Debug" : "Info";

                                ProcessSources(args);
                                return 0;
                            },
                            args => args.Frontends ? ListFrontends() : 0,
                            args => args.Editor ? EditorHelp() : Help(),
                            args => Clear(),
                            args => Exit(),
                            errors => {
                                foreach (var e in errors) {
                                    logger.Error(e);
                                }

                                return 0;
                            }
                        );
                }

                // wait for next command
                string command;
                do {
                    ConsoleUtils.ClearLine();
                    command = ConsoleUtils.ReadSimple("~> ");
                } while (string.IsNullOrWhiteSpace(command));

                arguments = Utilities.SplitLaunchArguments(command).ToArray();
            }

            static int ListFrontends() {
                Console.WriteLine(string.Join("\n", Compiler.Translators.Keys));
                return 0;
            }

            static int Version() {
                Console.WriteLine(Compiler.Version);
                return 0;
            }

            static int Help() {
                Console.WriteLine(CommandLineArguments.HelpText);
                return 0;
            }

            static int EditorHelp() {
                ScriptBench.DrawHelpBox();
                return 0;
            }

            static int Clear() {
                Console.Clear();
                PrintIntro();
                return 0;
            }

            static int Exit() {
                Environment.Exit(0);
                return 0;
            }

            // This loop breaks only by 'exit' command.
            // ReSharper disable once FunctionNeverReturns
        }

        private static void PrintIntro() {
            const string header = "Axion programming language compiler toolset";
            Console.Title = header;
            ConsoleUtils.WriteLine(
                (header + " v. ", ConsoleColor.White),
                (Compiler.Version, ConsoleColor.DarkYellow)
            );
            ConsoleUtils.WriteLine(
                ("Working in ", ConsoleColor.White),
                (Compiler.WorkDir, ConsoleColor.DarkYellow)
            );
            ConsoleUtils.WriteLine(
                "Type 'h', or 'help' to get documentation about launch arguments.\n"
            );
        }

        private static void PrintError(LanguageReport e) {
            var codeLines = e.TargetUnit.TextStream.Text.Split(
                new[] { "\n" },
                StringSplitOptions.None
            );

            var lines = new List<string>();
            // limit code piece by 5 lines
            for (var i = e.ErrorSpan.Start.Line;
                 i < codeLines.Length && lines.Count < 4;
                 i++) {
                lines.Add(codeLines[i].TrimEnd('\n', '\r', Spec.EndOfCode));
            }

            if (lines.Count > codeLines.Length - e.ErrorSpan.Start.Line) {
                lines.Add("...");
            }

            // first line
            // <line number>| <code line>
            var pointerTailLength = 8 + e.ErrorSpan.Start.Column;
            int errorTokenLength;
            if (e.ErrorSpan.End.Line > e.ErrorSpan.Start.Line) {
                errorTokenLength = lines[0].Length - e.ErrorSpan.Start.Column;
            }
            else {
                errorTokenLength =
                    e.ErrorSpan.End.Column - e.ErrorSpan.Start.Column;
            }

            // underline, red-colored
            var pointer = new string(' ', pointerTailLength)
                        + new string('~', Math.Abs(errorTokenLength));

            //=========Error template=========
            //
            // Error: mismatching parenthesis.
            // --> C:\path\to\file.ax
            //
            //     1 │ func("string",
            //             ~
            //     2 │      'c',
            //     3 │      123
            // ...
            //
            var color = e.Severity == BlameSeverity.Error
                ? ConsoleColor.Red
                : ConsoleColor.DarkYellow;

            // <severity>: <message>.
            ConsoleUtils.WriteLine(
                (e.Severity.ToString("G") + ": " + e.Message, color)
            );
            // file name
            ConsoleUtils.WriteLine(
                $"--> {e.TargetUnit.SourceFile}:"
              + $"{e.ErrorSpan.Start.Line + 1},{e.ErrorSpan.Start.Column + 1}"
            );
            Console.WriteLine();
            // line with error
            PrintLineNumber(e.ErrorSpan.Start.Line + 1);
            ConsoleUtils.WriteLine(lines[0]);
            // error pointer
            ConsoleUtils.WriteLine((pointer, color));
            // next lines
            for (var i = e.ErrorSpan.Start.Line + 1; i < lines.Count; i++) {
                PrintLineNumber(i + 1);
                ConsoleUtils.WriteLine(lines[i]);
            }
        }

        private static void PrintLineNumber(int lineNumber) {
            var strNum = lineNumber.ToString();
            var width  = Math.Max(strNum.Length, 4);
            var view   = strNum.PadLeft(width + 1).PadRight(width + 2) + "│ ";
            Console.Write(view);
        }

        private static void EnterInteractiveMode() {
            logger.Info(
                "Axion code editor & interpreter mode.\n"
              + "Type 'exit' or 'quit' to exit mode, 'cls' to clear screen."
            );
            while (true) {
                // code editor header
                var rawInput = ConsoleUtils.Read("i> ");
                var input    = rawInput.Trim().ToUpper();

                switch (input) {
                case "":
                    // skip empty commands
                    continue;
                case "EXIT":
                case "QUIT":
                    // exit from interpreter to main loop
                    logger.Info("\nInteractive interpreter closed.");
                    return;
                default:
                    // Disable logging while editing
                    var ll = logLevel.Text;
                    logLevel = "Fatal";

                    // initialize editor
                    var editor = new ScriptBench(
                        firstCodeLine: rawInput,
                        highlighter: new AxionSyntaxHighlighter()
                    );
                    var codeLines = editor.Run();

                    // Re-enable logging
                    logLevel = ll;

                    if (string.IsNullOrWhiteSpace(string.Join("", codeLines))) {
                        continue;
                    }

                    // interpret as source code and output result
                    var module = Module.RawFrom(
                        new FileInfo(Compiler.GetTempSourceFilePath()).Directory!
                    );
                    module.Bind(Unit.FromLines(codeLines));
                    var result = Compiler.Process(
                        module,
                        new ProcessingOptions("CSharp")
                    );
                    if (module.HasErrors || result is not CodeWriter codeWriter) {
                        break;
                    }

                    try {
                        logger.Info("Interpretation:");
                        ExecuteCSharp(codeWriter.ToString());
                    }
                    catch (CompilationErrorException e) {
                        logger.Error(
                            string.Join(Environment.NewLine, e.Diagnostics)
                        );
                    }

                    break;
                }
            }
        }

        private static void ProcessSources(CommandLineArguments args) {
            ProcessingOptions? pOptions;
            if (string.IsNullOrWhiteSpace(args.Mode)) {
                pOptions = new ProcessingOptions(Mode.Reduction);
            }
            else if (Enum.TryParse(args.Mode, true, out Mode mode)) {
                pOptions = new ProcessingOptions(mode);
            }
            else {
                pOptions = new ProcessingOptions(args.Mode);
            }

            Module module;
            if (args.Files.Any()) {
                var filesCount = args.Files.Count();
                if (filesCount > 1) {
                    logger.Error(
                        "Compiler doesn't support multiple files processing yet."
                    );
                    return;
                }

                var inputFiles = new FileInfo[filesCount];
                for (var i = 0; i < filesCount; i++) {
                    inputFiles[i] = new FileInfo(
                        Utilities.TrimMatchingChars(args.Files.ElementAt(i), '"')
                    );
                }

                module = Module.RawFrom(inputFiles[0].Directory);
                module.Bind(Unit.FromFile(inputFiles[0]));
                Compiler.Process(module, pOptions);
            }
            else if (!string.IsNullOrWhiteSpace(args.Project)) {
                var proj = new Project(args.Project);
                var ll   = logLevel.Text;
                logLevel = "Fatal";
                Compiler.Process(proj, pOptions);
                logLevel = ll;
                module   = proj.MainModule;
            }
            else if (!string.IsNullOrWhiteSpace(args.Code)) {
                var tempDir =
                    new FileInfo(Compiler.GetTempSourceFilePath()).Directory!;
                var unit = Unit.FromCode(
                    Utilities.TrimMatchingChars(args.Code, '"')
                );
                module = Module.RawFrom(tempDir);
                module.Bind(unit);
                Compiler.Process(module, pOptions);
            }
            else {
                logger.Error(
                    "Neither code nor path to source/project file not specified."
                );
                return;
            }
            foreach (var e in module.Blames) {
                PrintError(e);
            }
        }

        private static void ExecuteCSharp(string csCode) {
            if (string.IsNullOrWhiteSpace(csCode)) {
                return;
            }

            var refs = new List<MetadataReference>(
                DefaultImports.Select(
                    asm => MetadataReference.CreateFromFile(asm.Location)
                )
            );

            // Location of the .NET assemblies
            var assemblyPath =
                Path.GetDirectoryName(typeof(object).Assembly.Location)!;

            // Adding some necessary .NET assemblies
            // These assemblies couldn't be loaded correctly via the same construction as above.
            refs.Add(
                MetadataReference.CreateFromFile(
                    Path.Join(assemblyPath, "mscorlib.dll")
                )
            );
            refs.Add(
                MetadataReference.CreateFromFile(
                    Path.Join(assemblyPath, "System.dll")
                )
            );
            refs.Add(
                MetadataReference.CreateFromFile(
                    Path.Join(assemblyPath, "System.Core.dll")
                )
            );
            refs.Add(
                MetadataReference.CreateFromFile(
                    Path.Join(assemblyPath, "System.Runtime.dll")
                )
            );
            refs.Add(
                MetadataReference.CreateFromFile(
                    typeof(Console).Assembly.Location
                )
            );
            refs.Add(
                MetadataReference.CreateFromFile(
                    Path.Join(assemblyPath, "System.Private.CoreLib.dll")
                )
            );

            var assemblyName = Path.GetRandomFileName();
            var syntaxTree   = CSharpSyntaxTree.ParseText(csCode);

            var compilation = CSharpCompilation.Create(
                assemblyName,
                new[] { syntaxTree },
                refs,
                new CSharpCompilationOptions(
                    OutputKind.DynamicallyLinkedLibrary
                )
            );

            using var ms     = new MemoryStream();
            var       result = compilation.Emit(ms);

            if (result.Success) {
                ms.Seek(0, SeekOrigin.Begin);
                var assembly = Assembly.Load(ms.ToArray());

                var type = assembly.GetType("__RootModule__.__RootClass__");
                var main = type!.GetMethod("Main");

                // Let's assume that compiler anyway 'd create Main method for us :)
                main!.Invoke(null, new object[] { new string[0] });
            }
            else {
                var failures = result.Diagnostics.Where(
                    diagnostic => diagnostic.IsWarningAsError
                               || diagnostic.Severity
                               == DiagnosticSeverity.Error
                );

                foreach (var diagnostic in failures) {
                    logger.Error($"{diagnostic.Id}: {diagnostic.GetMessage()}");
                }
            }
        }

        private static INodeTranslator LoadTranslator(Assembly assembly) {
            foreach (var type in assembly.GetTypes()) {
                if (typeof(INodeTranslator).IsAssignableFrom(type)
                 && Activator.CreateInstance(type) is INodeTranslator ncv)
                    return ncv;
            }

            var availableTypes = string.Join(
                ", ",
                assembly.GetTypes().Select(t => t.FullName)
            );
            throw new ApplicationException(
                $"Can't find any type which implements {nameof(INodeTranslator)}"
              + $"in {assembly} from {assembly.Location}.\n"
              + $"Available types: {availableTypes}"
            );
        }
    }

    public class EmitterLoadContext : AssemblyLoadContext {
        private readonly AssemblyDependencyResolver resolver;

        public EmitterLoadContext(string pluginPath) {
            resolver = new AssemblyDependencyResolver(pluginPath);
        }

        protected override Assembly? Load(AssemblyName assemblyName) {
            var assemblyPath = resolver.ResolveAssemblyToPath(assemblyName);
            return assemblyPath != null
                ? LoadFromAssemblyPath(assemblyPath)
                : null;
        }

        protected override IntPtr LoadUnmanagedDll(string unmanagedDllName) {
            var libraryPath = resolver.ResolveUnmanagedDllToPath(unmanagedDllName);
            return libraryPath != null
                ? LoadUnmanagedDllFromPath(libraryPath)
                : IntPtr.Zero;
        }
    }
}
