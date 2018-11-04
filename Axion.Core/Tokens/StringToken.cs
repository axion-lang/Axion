using System;

namespace Axion.Core.Tokens {
    /// <summary>
    ///     Represents a &lt;string literal&gt; <see cref="Token" />.
    /// </summary>
    public class StringToken : Token {
        internal readonly bool                 IsUnclosed;
        internal readonly char                 Quote;
        internal readonly StringLiteralOptions Options;

        public StringToken(
            (int, int)           startPosition,
            string               value,
            char                 usedQuote,
            StringLiteralOptions literalOptions,
            bool                 isUnclosed = false
        ) : base(TokenType.StringLiteral, startPosition, value) {
            Quote      = usedQuote;
            Options    = literalOptions;
            IsUnclosed = isUnclosed;
            int linesCount = value.Split(Spec.Newlines, StringSplitOptions.None).Length;
            // addition of quotes length:
            // compute count of quotes on token end line:
            // Multiline:  6 quotes on 1  line,  (3 * 2);
            //             3 quotes on 2+ lines, (3 * 1);
            // One-line:   2 quotes on 1  line,  (1 * 2);
            //             1 quote  on 2+ lines, (1 * 1).
            int quotesCount = Options.HasFlag(StringLiteralOptions.Multiline)
                                  ? 3
                                  : 1;
            if (linesCount == 1) {
                // if 1 line: add 1 for each prefix letter
                EndColumn += Utilities.GetSetBitCount((long) Options);
                if (isUnclosed) {
                    EndColumn += quotesCount;
                }
                else {
                    EndColumn += quotesCount * 2;
                }
            }
            else if (!isUnclosed) {
                EndColumn += quotesCount;
            }
        }

        public override string ToAxionCode() {
            var result = "";
            if (Options.HasFlag(StringLiteralOptions.Format)) {
                result += "f";
            }
            if (Options.HasFlag(StringLiteralOptions.Raw)) {
                result += "r";
            }
            int quotesCount = Options.HasFlag(StringLiteralOptions.Multiline)
                                  ? 3
                                  : 1;
            result += new string(Quote, quotesCount) + Value;
            if (!IsUnclosed) {
                result += new string(Quote, quotesCount);
            }
            return result;
        }
    }

    [Flags]
    public enum StringLiteralOptions {
        None = 0,

        // prefixes
        Raw    = 1 << 0,
        Format = 1 << 1,

        // other
        Multiline            = 1 << 5,
        NormalizeLineEndings = 1 << 6
    }
}