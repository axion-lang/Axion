using System.Text;
using Newtonsoft.Json;

namespace Axion.Core.Processing.Lexical.Tokens {
    /// <summary>
    ///     Represents a 'number' literal.
    /// </summary>
    public class NumberToken : Token {
        public NumberOptions Options { get; }

        public NumberToken(
            object        value,
            NumberOptions options       = null,
            Position      startPosition = default
        ) :
            base(TokenType.Number, value.ToString(), startPosition) {
            Options = options ?? new NumberOptions();
        }
        
        internal override AxionCodeBuilder ToAxionCode(AxionCodeBuilder c) {
            return c + Options.Number.ToString();
        }

        internal override CSharpCodeBuilder ToCSharpCode(CSharpCodeBuilder c) {
            return c + Options.Number.ToString();
        }
    }

    /// <summary>
    ///     Contains information about number properties
    ///     (base, reserved bits count, is it floating, etc.)
    /// </summary>
    public class NumberOptions {
        [JsonIgnore]
        public StringBuilder Number { get; set; } = new StringBuilder();

        internal object Value       { get; set; }
        public   int    Radix       { get; set; }
        public   int    Bits        { get; set; }
        public   bool   Floating    { get; set; }
        public   bool   Imaginary   { get; set; }
        public   bool   Unsigned    { get; set; }
        public   bool   Unlimited   { get; set; }
        public   bool   HasExponent { get; set; }
        public   int    Exponent    { get; set; }

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
    }
}