using System.Collections.Generic;
using Axion.Processing;

namespace Axion {
    /// <summary>
    ///     Stores information about last
    ///     user query arguments in command line.
    ///     <para />
    ///     PROPERTY NAMES SHOULD NOT BE MODIFIED.
    ///     THEY ARE USED AS FULL NAMES OF COMMAND LINE LAUNCH ARGUMENTS.
    /// </summary>
    internal class LaunchArguments {
        // Warnings disabled because 'set' ancestor used when
        // parsing command line arguments.
        // ReSharper disable MemberCanBePrivate.Global
        // ReSharper disable AutoPropertyCanBeMadeGetOnly.Global

        /// <summary>
        ///     Files to process by compiler.
        /// </summary>
        public List<string> Files { get; set; } = null;

        /// <summary>
        ///     Script to process by compiler.
        /// </summary>
        public string Script { get; set; } = null;

        /// <summary>
        ///     Gets or sets what compiler need to do (e.g. interpret or compile source).
        /// </summary>
        public SourceProcessingMode Mode { get; set; } = SourceProcessingMode.Compile;

        /// <summary>
        ///     Determines if compiler should work as interactive interpreter.
        /// </summary>
        public bool Interactive { get; set; } = false;

        /// <summary>
        ///     Debug compiler mode enables saving JSON debug info.
        /// </summary>
        public bool Debug { get; set; } = true;

        /// <summary>
        ///     Flag to invoke <see cref="Compiler.DisplayHelpScreen" />.
        /// </summary>
        public bool Help { get; set; } = false;

        /// <summary>
        ///     Flag to display <see cref="Compiler.version" />.
        /// </summary>
        public bool Version { get; set; } = false;

        /// <summary>
        ///     Flag to exit the compiler.
        /// </summary>
        public bool Exit { get; set; } = false;
    }
}