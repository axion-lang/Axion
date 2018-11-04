using System;

namespace Axion.Core.Tokens {
    /// <summary>
    ///     Represents a &lt;number&gt; <see cref="Token" />.
    /// </summary>
    internal class NumberToken : Token {
        internal readonly NumberOptions Options;

        public NumberToken((int, int) startPosition, object value, NumberOptions options)
            : base(TokenType.IntegerLiteral, startPosition, value.ToString()) {
            if (options.Bits < 8 || options.Bits > 256) {
                throw new Exception("Internal error: " + nameof(NumberToken) + " constructor called with bits value of " + options.Bits);
            }
            Options = options;
        }
    }

    internal struct NumberOptions {
        internal int  Bits;
        internal bool Floating;
        internal bool Imaginary;
        internal bool Unsigned;
        internal bool Unlimited;

        public NumberOptions(int bits = 32, bool floating = false, bool imaginary = false, bool unsigned = false, bool unlimited = false) {
            Bits = bits;
            Floating = floating;
            Imaginary = imaginary;
            Unsigned = unsigned;
            Unlimited = unlimited;
        }
    }
}