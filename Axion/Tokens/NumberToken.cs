using System;
using System.Numerics;

namespace Axion.Tokens {
    /// <summary>
    ///     Represents a &lt;number&gt; <see cref="Token" />.
    /// </summary>
    internal class NumberToken : Token {
        public NumberToken((int line, int column) location, object value, NumberOptions options)
            : base(TokenType.Unknown, location, value.ToString()) {
            if (!(value is long) &&
                !(value is BigInteger)) {
                throw new Exception($"Internal exception: {nameof(NumberToken)} constructor got invalid {nameof(value)}: {value}");
            }
            var str = value.ToString().ToUpper();
            if (str.Contains(".")) {
                Type = TokenType.FloatLiteral;
            }
            if (str.Contains("J") ||
                str.Contains("E")) {
                Type = TokenType.RealLiteral;
            }
        }
    }

    /// <summary>
    ///     USE '&amp;' AND '&amp;=' OPERATORS TO DEFINE AND APPEND 'Bit#' FLAGS!
    /// </summary>
    [Flags]
    public enum NumberOptions {
        None = 0,

        Floating   = 0b1000000,
        Unsigned   = 0b0100000,
        Bit8       = 0b0010000, // bit count
        Bit16      = 0b0001000,
        Bit32      = 0b0000100,
        Bit64      = 0b0000010,
        BitNoLimit = 0b0000001,
    }
}