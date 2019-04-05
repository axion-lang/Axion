using Axion.Core.Processing.CodeGen;
using Axion.Core.Processing.Lexical.Tokens;
using JetBrains.Annotations;

namespace Axion.Core.Processing.Syntactic.Expressions.TypeNames {
    /// <summary>
    ///     <c>
    ///         array_type ::=
    ///             type '[' ']'
    ///     </c>
    /// </summary>
    public class ArrayTypeName : TypeName {
        private TypeName elementType;

        public TypeName ElementType {
            get => elementType;
            set => SetNode(ref elementType, value);
        }

        public ArrayTypeName([NotNull] TypeName elementType) {
            ElementType = elementType;
        }

        public ArrayTypeName(SyntaxTreeNode parent, TypeName elementType) : this(elementType) {
            Parent = parent;
            MarkStart(ElementType);
            Eat(TokenType.OpenBracket);
            Eat(TokenType.CloseBracket);
            MarkEnd(Token);
        }

        internal override CodeBuilder ToAxionCode(CodeBuilder c) {
            return c + elementType + "[]";
        }

        internal override CodeBuilder ToCSharpCode(CodeBuilder c) {
            return c + elementType + "[]";
        }
    }
}