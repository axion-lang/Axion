using Axion.Core.Source;
using Axion.Core.Specification;

namespace Axion.Core.Processing.Lexical.Tokens {
    public class NumberToken : Token {
        //public override TypeName ValueType => Spec.NumberType();

        public NumberToken(
            SourceUnit source,
            string     value = "",
            Location   start = default
        ) : base(source, TokenType.Number, value, start: start) { }

        public override Token Read() {
            while (AppendNext(true, "0")) { }

            while (AppendNext(expected: Spec.NumbersDec)) {
                Content += Stream.C;
            }

            return this;
        }
    }
}