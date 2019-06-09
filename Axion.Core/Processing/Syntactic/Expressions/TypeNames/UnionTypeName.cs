using System;
using Axion.Core.Processing.CodeGen;
using static Axion.Core.Specification.TokenType;

namespace Axion.Core.Processing.Syntactic.Expressions.TypeNames {
    /// <summary>
    ///     <c>
    ///         union_type:
    ///             type ('|' type)+;
    ///     </c>
    /// </summary>
    public class UnionTypeName : TypeName {
        private TypeName left;

        public TypeName Left {
            get => left;
            set => SetNode(ref left, value);
        }

        private TypeName right;

        public TypeName Right {
            get => right;
            set => SetNode(ref right, value);
        }

        /// <summary>
        ///     Constructs expression from Axion tokens.
        /// </summary>
        public UnionTypeName(AstNode parent, TypeName left) : base(parent) {
            MarkStart(Left = left);
            Eat(OpBitOr);
            MarkEnd(Right = ParseTypeName(this));
        }

        /// <summary>
        ///     Constructs expression without position in source.
        /// </summary>
        public UnionTypeName(TypeName left, TypeName right) {
            MarkStart(Left = left);
            MarkEnd(Right  = right);
        }

        internal override void ToAxionCode(CodeBuilder c) {
            c.Write(Left, " | ", Right);
        }

        internal override void ToCSharpCode(CodeBuilder c) {
            throw new NotSupportedException();
        }
    }
}