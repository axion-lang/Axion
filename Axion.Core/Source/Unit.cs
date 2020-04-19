using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Axion.Core.Processing.CodeGen;
using Axion.Core.Processing.Errors;
using Axion.Core.Processing.Lexical;
using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Processing.Syntactic;
using Axion.Core.Processing.Syntactic.Expressions;
using Axion.Core.Processing.Syntactic.Expressions.Definitions;
using Axion.Core.Specification;
using Newtonsoft.Json;

namespace Axion.Core.Source {
    /// <summary>
    ///     Container of Axion source code;
    ///     different kinds of code processing
    ///     are performed with that class.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public sealed class Unit {
        public const string SourceFileExt = ".ax";
        public const string DebugFileExt  = ".dbg.json";

        public bool HasErrors => Blames.Any(b => b.Severity == BlameSeverity.Error);

        private ProcessingOptions options = ProcessingOptions.Debug;

        internal ProcessingOptions Options {
            get => options;
            set {
                options    = value;
                CodeWriter = new CodeWriter(options);
            }
        }

        internal ProcessingMode ProcessingMode = ProcessingMode.Default;

        public List<LangException> Blames      { get; } = new List<LangException>();
        public TextStream          TextStream  { get; private set; }
        public TokenStream         TokenStream { get; }

        [JsonProperty]
        public Ast Ast { get; set; }

        private HashSet<string> CustomKeywords { get; } = new HashSet<string>();

        public CodeWriter CodeWriter { get; private set; }

        private Dictionary<string, Unit> Dependencies { get; set; } =
            new Dictionary<string, Unit>();

        private Dictionary<string, IDefinitionExpr> Definitions { get; set; } =
            new Dictionary<string, IDefinitionExpr>();

        #region File paths

        /// <summary>
        ///     Path to file where source code is located.
        ///     When not specified in constructor,
        ///     then file name assigned to date and time of instance creation.
        /// </summary>
        public FileInfo SourceFilePath { get; }

        private FileInfo? outputFilePath;

        /// <summary>
        ///     Path to file where generated result is located.
        /// </summary>
        public FileInfo OutputFilePath {
            get {
                if (outputFilePath != null) {
                    return outputFilePath;
                }

                // ReSharper disable PossibleNullReferenceException
                outputFilePath = new FileInfo(
                    Path.Combine(
                        SourceFilePath.Directory.FullName,
                        "out",
                        Path.GetFileNameWithoutExtension(SourceFilePath.FullName)
                      + CodeWriter.OutputFileExtension
                    )
                );
                Utilities.ResolvePath(outputFilePath.Directory.FullName);
                // ReSharper restore PossibleNullReferenceException
                return outputFilePath;
            }
        }

        private FileInfo? debugFilePath;

        /// <summary>
        ///     Path to file where processing debug output is located.
        /// </summary>
        public FileInfo DebugFilePath => debugFilePath ?? OutputFilePath;

        #endregion

        private Unit(
            string    code       = "",
            FileInfo? sourcePath = null,
            FileInfo? outputPath = null,
            FileInfo? debugPath  = null,
            bool      noStdLib   = false
        ) {
            if (sourcePath == null) {
                sourcePath = new FileInfo(
                    Path.Combine(
                        Compiler.WorkDir,
                        "temp",
                        "tmp-" + DateTime.Now.ToFileName() + SourceFileExt
                    )
                );
                // ReSharper disable once PossibleNullReferenceException
                Utilities.ResolvePath(sourcePath.Directory.FullName);
            }

            SourceFilePath = sourcePath;

            if (outputPath != null) {
                outputFilePath = outputPath;
            }

            if (debugPath != null) {
                debugFilePath = debugPath;
            }

            TextStream  = new TextStream(code);
            TokenStream = new TokenStream();
            Ast         = new Ast(this);
            CodeWriter  = new CodeWriter(Options);

            if (noStdLib) {
                return;
            }

            // TODO: One AST for multiple files from one package. (definitions in one place)
            // ReSharper disable PossibleNullReferenceException
            // BUG macros.ax is retrieved only from compiler sources (should be replaced with Axion stdlib location)
            var macrosFile = new FileInfo(
                Path.Combine(
                    new DirectoryInfo(Compiler.WorkDir).Parent.Parent.Parent.Parent.FullName,
                    "Axion.Modules",
                    "macros.ax"
                )
            );
            // ReSharper restore PossibleNullReferenceException
            if (SourceFilePath.FullName != macrosFile.FullName) {
                AddDependency(FromFile(macrosFile));
            }
        }

        public void AddDependency(Unit src) {
            Dependencies.Add(src.SourceFilePath.ToString(), src);
            Compiler.Process(src, ProcessingMode.Reduction, ProcessingOptions.Debug);
        }

        public Dictionary<string, IDefinitionExpr> GetAllDefinitions() {
            foreach (Unit dep in Dependencies.Values) {
                foreach (IDefinitionExpr def in dep.Ast.GetScopedDefs()) {
                    if (!Definitions.ContainsKey(def.Name.ToString())) {
                        Definitions.Add(def.Name.ToString(), def);
                    }
                }
            }
            return Definitions;
        }

        public void AddDefinition(IDefinitionExpr def) {
            if (def.Name == null) {
                throw new ArgumentException("Definition name cannot be null", nameof(def));
            }
            var name = def.Name.ToString();
            if (Definitions.ContainsKey(name)) {
                LangException.Report(BlameType.NameIsAlreadyDefined, def.Name);
            }
            else {
                Definitions.Add(name, def);
            }
        }

        public bool IsCustomKeyword(Token id) {
            // TODO: resolve bottleneck
            return CustomKeywords.Contains(id.Content);
        }

        public HashSet<string> GetAllCustomKeywords() {
            foreach (Unit dep in Dependencies.Values) {
                foreach (string kw in dep.CustomKeywords) {
                    CustomKeywords.Add(kw);
                }
            }
            return CustomKeywords;
        }

        public void RegisterCustomKeyword(string keyword) {
            if (!Spec.Keywords.ContainsKey(keyword)
             && !Spec.Operators.ContainsKey(keyword)
             && !Spec.Punctuation.ContainsKey(keyword)) {
                CustomKeywords.Add(keyword);
            }
        }

        public static Unit FromCode(string code, FileInfo? outFilePath = null) {
            return new Unit(code, outputPath: outFilePath);
        }

        public static Unit FromLines(IEnumerable<string> lines, FileInfo? outFilePath = null) {
            return new Unit(string.Join("\n", lines), outputPath: outFilePath);
        }

        public static Unit? FromFile(FileInfo srcFilePath, FileInfo? outFilePath = null) {
            if (!srcFilePath.Exists) {
                Compiler.Logger.Error($"'{srcFilePath.Name}' does not exists.");
                return null;
            }
            if (srcFilePath.Extension != SourceFileExt) {
                Compiler.Logger.Error(
                    $"'{srcFilePath.Name}' file must have {SourceFileExt} extension."
                );
                return null;
            }

            return new Unit(File.ReadAllText(srcFilePath.FullName), srcFilePath, outFilePath);
        }

        public static Unit FromInterpolation(Unit source) {
            return new Unit(noStdLib: true) {
                TextStream = source.TextStream, Definitions = source.Definitions
            };
        }
    }
}
