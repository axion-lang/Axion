using System.Diagnostics;
using Axion.Core.Hierarchy;
using Axion.Core.Processing.Syntactic;
using Axion.Core.Processing.Translation;
using Newtonsoft.Json;

namespace Axion.Core.Processing {
    [DebuggerDisplay("{" + nameof(debuggerDisplay) + ",nq}")]
    public class CodeSpan {
        [JsonIgnore]
        public Unit Unit { get; private protected init; }

        [JsonIgnore]
        public TokenStream Stream => Unit.TokenStream;

        /// <summary>
        ///     Start location of this node's code span.
        /// </summary>
        public Location Start { get; private protected set; }

        /// <summary>
        ///     End location of this node's code span.
        /// </summary>
        public Location End { get; private protected set; }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string debuggerDisplay {
            get {
                var cw = CodeWriter.Default;
                cw.Write(this);
                return cw.ToString();
            }
        }

        public CodeSpan(
            Unit     unit,
            Location start = default,
            Location end   = default
        ) {
            Unit  = unit;
            Start = start;
            End   = end;
        }
    }
}
