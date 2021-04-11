using Axion.Core.Hierarchy;
using Axion.Core.Processing.Syntactic.Expressions.TypeNames;
using Axion.Specification;

namespace Axion.Core.Processing.Lexical.Tokens {
    public class CharToken : Token {
        readonly SimpleTypeName typeName;

        public override TypeName ValueType => typeName;

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
            typeName   = new SimpleTypeName(this, Spec.CharType);
        }
    }
}
