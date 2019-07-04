using System.Globalization;
using System.Numerics;
using System.Text;
using Axion.Core.Processing.CodeGen;
using Axion.Core.Processing.Syntactic.TypeNames;
using Axion.Core.Specification;
using Newtonsoft.Json;

namespace Axion.Core.Processing.Lexical.Tokens {
    /// <summary>
    ///     Represents a 'number' literal.
    /// </summary>
    public class NumberToken : Token {
        public NumberOptions Options { get; }
        public override TypeName ValueType => Spec.NumberType(Options);

        public NumberToken(
            string        value,
            NumberOptions options,
            Position      startPosition = default
        ) : base(TokenType.Number, value, startPosition) {
            Options = options ?? new NumberOptions(value);
        }

        internal override void ToCSharpCode(CodeBuilder c) {
            if (Options.Value is Complex complex) {
                c.Write("new Complex(", complex.Real, ", ", complex.Imaginary, ")");
            }
            else {
                c.Write(Options.Value);
            }
        }
    }

    /// <summary>
    ///     Contains information about number properties
    ///     (base, reserved bits count, is it floating, etc.)
    /// </summary>
    [JsonObject(MemberSerialization.Fields)]
    public class NumberOptions {
        public static bool operator ==(NumberOptions left, NumberOptions right) {
            return Equals(left, right);
        }

        public static bool operator !=(NumberOptions left, NumberOptions right) {
            return !Equals(left, right);
        }

        [JsonIgnore] public readonly StringBuilder ClearNumber = new StringBuilder();

        private static NumberFormatInfo numFormat = new NumberFormatInfo {
            NumberDecimalSeparator = "."
        };

        internal object Value {
            get {
                const NumberStyles numStyles = NumberStyles.AllowExponent;
                string             n         = ClearNumber.ToString();
                if (Floating || Imaginary) {
                    double dbl = double.Parse(n, numStyles | NumberStyles.Float, numFormat);
                    return Imaginary ? new Complex(0.0, dbl) : dbl;
                }

                if (Radix <= 10) {
                    return Utilities.RadixLess10ToBigInt(n, Radix);
                }

                if (Radix == 16) {
                    return BigInteger.Parse(n, NumberStyles.HexNumber);
                }

                return BigInteger.Parse(n, numStyles);
            }
        }

        public readonly int  Radix;
        public          int  Bits;
        public          bool Floating;
        public          bool Imaginary;
        public          bool Unsigned;
        public          bool Unlimited;
        public          bool HasExponent;
        public          int  Exponent;

        internal NumberOptions(
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

        public NumberOptions(
            string clearValue,
            int    radix       = 10,
            int    bits        = 32,
            bool   floating    = false,
            bool   imaginary   = false,
            bool   unsigned    = false,
            bool   unlimited   = false,
            bool   hasExponent = false,
            int    exponent    = 0
        ) : this(
            radix,
            bits,
            floating,
            imaginary,
            unsigned,
            unlimited,
            hasExponent,
            exponent
        ) {
            ClearNumber.Append(clearValue);
        }

        protected bool Equals(NumberOptions other) {
            return ClearNumber.ToString() == other.ClearNumber.ToString()
                && Radix                  == other.Radix
                && Bits                   == other.Bits
                && Floating               == other.Floating
                && Imaginary              == other.Imaginary
                && Unsigned               == other.Unsigned
                && Unlimited              == other.Unlimited
                && HasExponent            == other.HasExponent
                && Exponent               == other.Exponent;
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) {
                return false;
            }

            if (ReferenceEquals(this, obj)) {
                return true;
            }

            if (obj.GetType() != GetType()) {
                return false;
            }

            return Equals((NumberOptions) obj);
        }

        public override int GetHashCode() {
            unchecked {
                int hashCode = ClearNumber != null ? ClearNumber.GetHashCode() : 0;
                // ReSharper disable NonReadonlyMemberInGetHashCode
                hashCode = (hashCode * 397) ^ Radix;
                hashCode = (hashCode * 397) ^ Bits;
                hashCode = (hashCode * 397) ^ Floating.GetHashCode();
                hashCode = (hashCode * 397) ^ Imaginary.GetHashCode();
                hashCode = (hashCode * 397) ^ Unsigned.GetHashCode();
                hashCode = (hashCode * 397) ^ Unlimited.GetHashCode();
                hashCode = (hashCode * 397) ^ HasExponent.GetHashCode();
                hashCode = (hashCode * 397) ^ Exponent;
                return hashCode;
            }
        }

        public override string ToString() {
            return
                $"{nameof(ClearNumber)}: {ClearNumber}, \n"
              + $"{nameof(Value)}: {Value},\n"
              + $"{nameof(Radix)}: {Radix},\n"
              + $"{nameof(Bits)}: {Bits},\n"
              + $"{nameof(Floating)}: {Floating},\n"
              + $"{nameof(Imaginary)}: {Imaginary},\n"
              + $"{nameof(Unsigned)}: {Unsigned},\n"
              + $"{nameof(Unlimited)}: {Unlimited},\n"
              + $"{nameof(HasExponent)}: {HasExponent},\n"
              + $"{nameof(Exponent)}: {Exponent}";
        }
    }
}