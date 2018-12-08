namespace Axion.Core.Tokens {
    /// <summary>
    ///     Represents a &lt;number&gt; <see cref="Token" />.
    /// </summary>
    public class NumberToken : Token {
        internal NumberOptions Options { get; }

        public NumberToken((int, int) startPosition, object value, NumberOptions options)
            : base(TokenType.IntegerLiteral, startPosition, value.ToString()) {
            Options = options;
        }
    }

    public class NumberOptions {
        internal string ValueType { get; set; }
        internal object Value     { get; set; }

        internal string Number { get; set; }

        internal int  Bits      { get; set; }
        internal int  Radix     { get; set; }
        internal bool Floating  { get; set; }
        internal bool Imaginary { get; set; }
        internal bool Unsigned  { get; set; }
        internal bool Unlimited { get; set; }

        internal bool HasExponent { get; set; }
        internal int  Exponent    { get; set; }

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

    public enum NumberType {
        Byte,
        Int16,
        Int32,
        Int64,
        IntBig,
        Float32,
        Float64,
        Complex
    }
}