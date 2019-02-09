namespace Axion.Core.Processing.Lexical.Tokens {
    /// <summary>
    ///     Represents a 'number' literal.
    /// </summary>
    public class NumberToken : Token {
        public NumberToken(Position startPosition, object value, NumberOptions options = null) : base(
            TokenType.Number,
            startPosition,
            value.ToString()
        ) {
            Options = options ?? new NumberOptions();
        }

        public NumberOptions Options { get; }

        public override string ToString() {
            return ToAxionCode();
        }
    }

    /// <summary>
    ///     Contains information about number properties
    ///     (base, reserved bits count, is it floating, etc.)
    /// </summary>
    public class NumberOptions {
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

        public int  Bits      { get; set; }
        public int  Radix     { get; set; }
        public bool Floating  { get; set; }
        public bool Imaginary { get; set; }
        public bool Unsigned  { get; set; }
        public bool Unlimited { get; set; }

        public   bool   HasExponent { get; set; }
        public   int    Exponent    { get; set; }
        internal object Value       { get; set; }
    }
}