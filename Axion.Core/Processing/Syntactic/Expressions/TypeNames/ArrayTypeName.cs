using Axion.Core.Processing.CodeGen;
using Axion.Core.Specification;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Axion.Core.Processing.Syntactic.Expressions.TypeNames {
    /// <summary>
    ///     <c>
    ///         array_type:
    ///             type '[' ']'
    ///     </c>
    /// </summary>
    public class ArrayTypeName : TypeName {
        private TypeName elementType;

        public TypeName ElementType {
            get => elementType;
            set => SetNode(ref elementType, value);
        }

        #region Constructors

        /// <summary>
        ///     Constructs new <see cref="ArrayTypeName"/> from Axion tokens.
        /// </summary>
        public ArrayTypeName(SyntaxTreeNode parent, TypeName elementType) {
            Parent      = parent;
            ElementType = elementType;

            MarkStart(ElementType);
            Eat(TokenType.OpenBracket);
            Eat(TokenType.CloseBracket);
            MarkEnd(Token);
        }

        /// <summary>
        ///     Constructs new <see cref="ArrayTypeName"/> from C# syntax.
        /// </summary>
        public ArrayTypeName(SyntaxTreeNode parent, ArrayTypeSyntax csNode) {
            Parent      = parent;
            ElementType = FromCSharp(this, csNode.ElementType);
        }

        /// <summary>
        ///     Constructs plain <see cref="ArrayTypeName"/> without position in source.
        /// </summary>
        public ArrayTypeName(TypeName elementType) {
            ElementType = elementType;
        }

        #endregion

        #region Code converters

        internal override void ToAxionCode(CodeBuilder c) {
            c.Write(elementType, "[]");
        }

        internal override void ToCSharpCode(CodeBuilder c) {
            c.Write(elementType, "[]");
        }

        #endregion
    }
}