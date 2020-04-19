namespace Axion.Core.Source {
    /// <summary>
    ///     Defines some settings for source code processing.
    /// </summary>
    public class ProcessingOptions {
        /// <summary>
        ///     Target source to compile Axion code into.
        ///     (e.g. 'Python', 'C#', etc.)
        /// </summary>
        public string TargetType { get; }

        /// <summary>
        ///     Check code against inconsistent
        ///     indentation (mixed spaced and tabs).
        /// </summary>
        public bool CheckIndentationConsistency { get; }

        public static ProcessingOptions Debug { get; } = new ProcessingOptions("axion");

        public ProcessingOptions(string targetType, bool checkIndentationConsistency = true) {
            TargetType                  = targetType.Trim().ToLower();
            CheckIndentationConsistency = checkIndentationConsistency;
        }
    }
}
