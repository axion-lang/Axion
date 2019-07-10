using Axion.Core.Processing.CodeGen;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Axion.Core.Specification.TokenType;

namespace Axion.Core.Processing.Syntactic.TypeNames {
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
        ///     Expression is constructed from tokens stream
        ///     that belongs to <see cref="parent"/>'s AST.
        /// </summary>
        public ArrayTypeName(Expression parent, TypeName elementType) {
            Construct(parent, elementType, () => {
                ElementType = elementType;
                Eat(OpenBracket);
                Eat(CloseBracket);
            });
        }

        /// <summary>
        ///     Constructs expression from C# syntax.
        /// </summary>
        public ArrayTypeName(Expression parent, ArrayTypeSyntax csNode) : base(parent) {
            ElementType = FromCSharp(csNode.ElementType);
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