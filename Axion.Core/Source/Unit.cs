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

        public List<LangException> Blames { get; } = new List<LangException>();

        public TextStream TextStream { get; private set; }

        public TokenStream TokenStream { get; }

        [JsonProperty]
        public Ast Ast { get; }

        public HashSet<string> CustomKeywords { get; } = new HashSet<string>();

        private ProcessingOptions options = ProcessingOptions.Debug;

        public ProcessingOptions Options {
            get => options;
            set {
                options    = value;
                CodeWriter = new CodeWriter(options);
            }
        }

        public CodeWriter CodeWriter { get; private set; }

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

        public Module Module { get; set; }

        public Dictionary<string, Module> Imports { get; } = new Dictionary<string, Module>();

        private Unit(
            string    code       = "",
            FileInfo? sourcePath = null,
            FileInfo? outputPath = null,
            FileInfo? debugPath  = null
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
        }

        public void AddDependency(Unit src) {
            Imports.Add(src.SourceFilePath.ToString(), src);
            Compiler.Process(src, ProcessingMode.Reduction, ProcessingOptions.Debug);
        }

        public bool IsCustomKeyword(Token id) {
            // TODO: resolve bottleneck
            return CustomKeywords.Contains(id.Content);
        }

        public HashSet<string> GetAllCustomKeywords() {
            foreach (Unit dep in Imports.Values) {
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

        public static Unit FromFile(FileInfo srcFilePath, FileInfo? outFilePath = null) {
            if (!srcFilePath.Exists) {
                throw new FileNotFoundException($"'{srcFilePath.Name}' does not exists.");
            }
            if (srcFilePath.Extension != SourceFileExt) {
                throw new ArgumentException(
                    $"'{srcFilePath.Name}' file must have {SourceFileExt} extension."
                );
            }

            return new Unit(File.ReadAllText(srcFilePath.FullName), srcFilePath, outFilePath);
        }

        public static Unit FromInterpolation(Unit source) {
            return new Unit {
                TextStream = source.TextStream, Module = source.Module
            };
        }
    }
}
