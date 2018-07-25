using System;

namespace Axion.Tokens {
    /// <summary>
    ///     Represents a &lt;string literal&gt; <see cref="Token" />.
    /// </summary>
    public class StringToken : Token {
        public StringToken((int line, int column) location,
                           string                 value,
                           StringLiteralOptions   options,
                           bool                   multiline)
            : base(TokenType.StringLiteral, location, value) {
            int linesCount = value.Split(Spec.Newlines, StringSplitOptions.None).Length;
            // addition of quotes length:
            // compute count of quotes on token end line:
            // Multiline:  6 quotes on 1  line,  (3 * 2);
            //             3 quotes on 2+ lines, (3 * 1);
            // One-line:   2 quotes on 1  line,  (1 * 2);
            //             1 quote  on 2+ lines, (1 * 1).
            int multiplier = linesCount == 1 ? 2 : 1;
            if (multiline) { // """ ```
                EndClPos += 3 * multiplier;
            }
            else {
                EndClPos += 1 * multiplier; // " " | ` `
            }
            // if 1 line: add 1 for each prefix letter
            if (multiplier == 2) {
                EndClPos += Utilities.GetSetBitCount((long) options);
            }
        }
    }

    [Flags]
    public enum StringLiteralOptions {
        None   = 0,
        Raw    = 1 << 0,
        Format = 1 << 1
    }
}