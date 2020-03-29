using Axion.Core.Processing.Syntactic.Expressions.TypeNames;
using Axion.Core.Source;
using Axion.Core.Specification;

namespace Axion.Core.Processing.Lexical.Tokens {
    public class CharToken : Token {
        public override TypeName ValueType  => Spec.CharType;
        public          bool     IsUnclosed { get; }

        internal CharToken(
            SourceUnit source,
            string     value      = "",
            string     content    = "",
            bool       isUnclosed = false
        ) : base(source, TokenType.Character, value, content) {
            IsUnclosed = isUnclosed;
        }
    }
}