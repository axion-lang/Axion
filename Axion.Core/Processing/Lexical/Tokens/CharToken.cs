using Axion.Core.Hierarchy;
using Axion.Core.Processing.Syntactic.Expressions.TypeNames;
using Axion.Core.Specification;

namespace Axion.Core.Processing.Lexical.Tokens {
    public class CharToken : Token {
        public override TypeName ValueType =>
            new SimpleTypeName(this, Spec.CharType);

        public bool IsUnclosed { get; }

        internal CharToken(
            Unit   unit,
            string value      = "",
            string content    = "",
            bool   isUnclosed = false
        ) : base(
            unit,
            TokenType.Character,
            value,
            content
        ) {
            IsUnclosed = isUnclosed;
        }
    }
}
