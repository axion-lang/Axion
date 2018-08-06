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

    [Flags]
    public enum NumberOptions {
        None = 0,

        Floating   = 0x0_100,
        Unsigned   = 0x0_010,
        Bit8       = 0x0_001, // bit count
        Bit16      = 0x0_002,
        Bit32      = 0x0_003,
        Bit64      = 0x0_004,
        BitNoLimit = 0x0_005,
    }
}