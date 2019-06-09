using Axion.Core.Processing.CodeGen;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Axion.Core.Specification.TokenType;

namespace Axion.Core.Processing.Syntactic.Expressions.TypeNames {
    /// <summary>
    ///     <c>
    ///         array_type:
    ///             type '[' ']';
    ///     </c>
    /// </summary>
    public class ArrayTypeName : TypeName {
        private TypeName elementType;

        public TypeName ElementType {
            get => elementType;
            set => SetNode(ref elementType, value);
        }

        /// <summary>
        ///     Constructs expression from Axion tokens.
        /// </summary>
        public ArrayTypeName(AstNode parent, TypeName elementType) : base(parent) {
            MarkStart(ElementType = elementType);
            Eat(OpenBracket);
            MarkEndAndEat(CloseBracket);
        }

        /// <summary>
        ///     Constructs expression from C# syntax.
        /// </summary>
        public ArrayTypeName(AstNode parent, ArrayTypeSyntax csNode) : base(parent) {
            ElementType = FromCSharp(this, csNode.ElementType);
        }

        /// <summary>
        ///     Constructs expression without position in source.
        /// </summary>
        public ArrayTypeName(TypeName elementType) {
            ElementType = elementType;
        }

        internal override void ToAxionCode(CodeBuilder c) {
            c.Write(elementType, "[]");
        }

        internal override void ToCSharpCode(CodeBuilder c) {
            c.Write(elementType, "[]");
        }
    }
}