using System;
using Axion.Core.Specification;

namespace Axion.Core {
    /// <summary>
    ///     Defines some settings for source code processing.
    /// </summary>
    public class ProcessingOptions {
        public static readonly ProcessingOptions Default =
            new ProcessingOptions(Mode.Default);

        public Mode ProcessingMode { get; }

        /// <summary>
        ///     Target source to compile Axion code into.
        /// </summary>
        public Language TargetLanguage { get; }

        /// <summary>
        ///     Check code against inconsistent
        ///     indentation (mixed spaced and tabs).
        /// </summary>
        public bool CheckIndentationConsistency { get; set; }

        public bool Debug { get; set; }

        public ProcessingOptions(Mode processingMode) {
            ProcessingMode = processingMode;
            TargetLanguage = Language.Axion;
        }

        public ProcessingOptions(Language targetLanguage) {
            ProcessingMode = Mode.Transpilation;
            TargetLanguage = targetLanguage;
        }

        public ProcessingOptions(string targetLanguage) {
            ProcessingMode = Mode.Transpilation;
            TargetLanguage = targetLanguage.Trim().ToUpper() switch {
                "AXION"  => Language.Axion,
                "C#"     => Language.CSharp,
                "CSHARP" => Language.CSharp,
                "PYTHON" => Language.Python,
                "PASCAL" => Language.Pascal,
                _ => throw new ArgumentException(
                    $"Invalid target language: {targetLanguage}."
                ),
            };
        }
    }
}
