namespace Axion.Core {
    /// <summary>
    ///     Defines some settings for source code processing.
    /// </summary>
    public class ProcessingOptions {
        public static readonly ProcessingOptions Default = new(Mode.Default);

        public Mode ProcessingMode { get; }

        /// <summary>
        ///     Target source to compile Axion code into.
        /// </summary>
        public string TargetLanguage { get; }

        /// <summary>
        ///     Check code against inconsistent
        ///     indentation (mixed spaced and tabs).
        /// </summary>
        public bool CheckIndentationConsistency { get; set; }

        public bool Debug { get; set; }

        public ProcessingOptions(Mode processingMode) {
            ProcessingMode = processingMode;
            TargetLanguage = "axion";
        }

        public ProcessingOptions(string targetLanguage) {
            ProcessingMode = Mode.Translation;
            TargetLanguage = targetLanguage.Trim().ToLower();
        }
    }
}
