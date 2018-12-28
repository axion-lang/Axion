using System.Collections.Generic;

namespace Axion.Core.Processing.Lexical.Tokens {
    public sealed class InterpolatedStringToken : StringToken {
        internal readonly List<Interpolation> Interpolations;

        public InterpolatedStringToken(
            Position             startPosition,
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
        internal readonly List<Token> Tokens = new List<Token>();
        internal readonly int         StartIndex;
        internal          int         EndIndex;

        internal int Length => EndIndex - StartIndex;

        public Interpolation(int startIndex) {
            StartIndex = startIndex;
        }
    }
}