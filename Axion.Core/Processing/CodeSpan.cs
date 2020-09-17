using Axion.Core.Hierarchy;
using Newtonsoft.Json;

namespace Axion.Core.Processing {
    public class CodeSpan {
        [JsonIgnore]
        public Unit Unit { get; private protected set; }

        /// <summary>
        ///     Start location of this node's code span.
        /// </summary>
        public Location Start { get; private protected set; }

        /// <summary>
        ///     End location of this node's code span.
        /// </summary>
        public Location End { get; private protected set; }

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
