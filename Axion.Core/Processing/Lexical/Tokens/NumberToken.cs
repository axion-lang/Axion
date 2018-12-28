using System.Text;
using Axion.Core.Processing.Lexical.Tokens.Interfaces;

namespace Axion.Core.Processing.Lexical.Tokens {
    /// <summary>
    ///     Represents a &lt;number&gt; <see cref="Token" />.
    /// </summary>
    public class NumberToken : Token, ILiteralToken {
        public NumberOptions Options { get; }

        public NumberToken(Position startPosition, object value, NumberOptions options)
            : base(TokenType.Number, startPosition, value.ToString()) {
            Options = options;
        }
    }

    public class NumberOptions {
        internal object Value { get; set; }

        public StringBuilder Number { get; set; } = new StringBuilder();

        public int  Bits      { get; set; }
        public int  Radix     { get; set; }
        public bool Floating  { get; set; }
        public bool Imaginary { get; set; }
        public bool Unsigned  { get; set; }
        public bool Unlimited { get; set; }

        public bool HasExponent { get; set; }
        public int  Exponent    { get; set; }

        public NumberOptions(
            int  radix       = 10,
            int  bits        = 32,
            bool floating    = false,
            bool imaginary   = false,
            bool unsigned    = false,
            bool unlimited   = false,
            bool hasExponent = false,
            int  exponent    = 0
        ) {
            Radix       = radix;
            Bits        = bits;
            Floating    = floating;
            Imaginary   = imaginary;
            Unsigned    = unsigned;
            Unlimited   = unlimited;
            HasExponent = hasExponent;
            Exponent    = exponent;
        }

        public bool TestEquality(NumberOptions other) {
            // don't check value equality
            return Number == other.Number
                && Radix == other.Radix
                && Bits == other.Bits
                && Floating == other.Floating
                && Imaginary == other.Imaginary
                && Unsigned == other.Unsigned
                && Unlimited == other.Unlimited
                && HasExponent == other.HasExponent
                && Exponent == other.Exponent;
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