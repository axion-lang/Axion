using System;

namespace Axion.Core.Tokens {
    /// <summary>
    ///     Represents a &lt;string literal&gt; <see cref="Token" />.
    /// </summary>
    public class StringToken : Token {
        private readonly bool                 multiline;
        private readonly char                 quote;
        private readonly StringLiteralOptions options;

        public StringToken((int line, int column) location,
                           string                 value,
                           char                   usedQuote,
                           StringLiteralOptions   literalOptions,
                           bool                   multipleLines)
            : base(TokenType.StringLiteral, location, value) {
            multiline = multipleLines;
            quote     = usedQuote;
            options   = literalOptions;
            int linesCount = value.Split(Spec.Newlines, StringSplitOptions.None).Length;
            // addition of quotes length:
            // compute count of quotes on token end line:
            // Multiline:  6 quotes on 1  line,  (3 * 2);
            //             3 quotes on 2+ lines, (3 * 1);
            // One-line:   2 quotes on 1  line,  (1 * 2);
            //             1 quote  on 2+ lines, (1 * 1).
            int multiplier = linesCount == 1 ? 2 : 1;
            if (multiline) { // """ ```
                EndColumnPos += 3 * multiplier;
            }
            else {
                EndColumnPos += 1 * multiplier; // " " | ` `
            }
            // if 1 line: add 1 for each prefix letter
            if (multiplier == 2) {
                EndColumnPos += Utilities.GetSetBitCount((long) options);
            }
        }

        public override string ToAxionCode() {
            var result = "";
            if (options.HasFlag(StringLiteralOptions.Format)) {
                result += "f";
            }
            if (options.HasFlag(StringLiteralOptions.Raw)) {
                result += "r";
            }
            if (multiline) {
                result += $"{quote}{quote}{quote}{Value}{quote}{quote}{quote}";
            }
            else {
                result += quote + Value + quote;
            }
            return result;
        }
    }

    [Flags]
    public enum StringLiteralOptions {
        None   = 0,
        Raw    = 1 << 0,
        Format = 1 << 1
    }
}