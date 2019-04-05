using System.Text;
using Newtonsoft.Json;

namespace Axion.Core.Processing.Lexical.Tokens {
    /// <summary>
    ///     Represents a 'number' literal.
    /// </summary>
    public class NumberToken : Token {
        public NumberOptions Options { get; }

        public NumberToken(
            string        value,
            NumberOptions options       = null,
            Position      startPosition = default
        ) : base(TokenType.Number, value, startPosition) {
            Options = options ?? new NumberOptions();
        }
    }

    /// <summary>
    ///     Contains information about number properties
    ///     (base, reserved bits count, is it floating, etc.)
    /// </summary>
    [JsonObject(MemberSerialization.Fields)]
    public class NumberOptions {
        [JsonIgnore]
        public StringBuilder Number = new StringBuilder();

        internal object Value;
        public   int    Radix;
        public   int    Bits;
        public   bool   Floating;
        public   bool   Imaginary;
        public   bool   Unsigned;
        public   bool   Unlimited;
        public   bool   HasExponent;
        public   int    Exponent;

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