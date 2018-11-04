using System.Collections.Generic;

namespace Axion.Core.Tokens {
    public sealed class InterpolatedStringToken : StringToken {
        internal readonly Interpolation[] Interpolations;

        public InterpolatedStringToken(
            (int, int)           startPosition,
            string               value, char usedQuote,
            StringLiteralOptions literalOptions,
            List<Interpolation>  interpolations
        ) : base(startPosition, value, usedQuote, literalOptions) {
            Interpolations = interpolations.ToArray();
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