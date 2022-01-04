namespace Axion.Core;

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

    public bool Debug { get; set; }

    public ProcessingOptions(Mode processingMode) {
        ProcessingMode = processingMode;
        TargetLanguage = "axion";
    }

    public ProcessingOptions(string targetLanguage) {
        ProcessingMode = Mode.Translation;
        TargetLanguage = targetLanguage.Trim().ToLowerInvariant();
    }
}
