namespace Axion.Core.Tokens {
    /// <summary>
    ///     Represents a &lt;number&gt; <see cref="Token" />.
    /// </summary>
    public class NumberToken : Token {
        internal readonly NumberOptions Options;

        public NumberToken((int, int) startPosition, object value, NumberOptions options)
            : base(TokenType.IntegerLiteral, startPosition, value.ToString()) {
            Options = options;
        }
    }

    public class NumberOptions {
        internal string ValueType;

        internal int Bits;

        internal int  Radix;
        internal bool Floating;
        internal bool Imaginary;
        internal bool Unsigned;
        internal bool Unlimited;

        internal bool HasExponent;
        internal int  Exponent;

        public NumberOptions(
            int  radix     = 10,
            int  bits      = 32,
            bool floating  = false,
            bool imaginary = false,
            bool unsigned  = false,
            bool unlimited = false
        ) {
            Radix     = radix;
            Bits      = bits;
            Floating  = floating;
            Imaginary = imaginary;
            Unsigned  = unsigned;
            Unlimited = unlimited;
        }
    }
}