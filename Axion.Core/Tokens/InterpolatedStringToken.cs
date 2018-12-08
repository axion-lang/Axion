using System.Collections.Generic;

namespace Axion.Core.Tokens {
    public sealed class InterpolatedStringToken : StringToken {
        internal readonly List<Interpolation> Interpolations;

        public InterpolatedStringToken(
            (int, int)           startPosition,
            StringLiteralOptions options,
            List<Interpolation>  interpolations,
            string               value,
            string               unescapedValue,
            bool                 isUnclosed = false
        ) : base(
            startPosition,
            options,
            value,
            unescapedValue,
            isUnclosed
        ) {
            Interpolations = interpolations;
        }
    }

    public sealed class Interpolation {
        internal readonly LinkedList<Token> Tokens = new LinkedList<Token>();
        internal readonly int               StartIndex;
        internal          int               EndIndex;

        internal int Length => EndIndex - StartIndex;

        public Interpolation(int startIndex) {
            StartIndex = startIndex;
        }
    }
}