using System;
using System.Numerics;

namespace Axion.Core.Tokens {
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
            string str = value.ToString().ToUpper();
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

        Floating = 0b1000000,
        Unsigned = 0b0100000,

        /// <summary>
        ///     SHOULD NOT BE MIXED WITH OTHER BIT COUNTS!
        /// </summary>
        Bit8 = 0b0010000,

        /// <summary>
        ///     SHOULD NOT BE MIXED WITH OTHER BIT COUNTS!
        /// </summary>
        Bit16 = 0b0001000,

        /// <summary>
        ///     SHOULD NOT BE MIXED WITH OTHER BIT COUNTS!
        /// </summary>
        Bit32 = 0b0000100,

        /// <summary>
        ///     SHOULD NOT BE MIXED WITH OTHER BIT COUNTS!
        /// </summary>
        Bit64 = 0b0000010,

        /// <summary>
        ///     SHOULD NOT BE MIXED WITH OTHER BIT COUNTS!
        /// </summary>
        BitNoLimit = 0b0000001
    }
}