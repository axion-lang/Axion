using System;
using Axion.Core.Processing.CodeGen;
using static Axion.Core.Specification.TokenType;

namespace Axion.Core.Processing.Syntactic.TypeNames {
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
        ///     Expression is constructed from tokens stream
        ///     that belongs to <see cref="parent"/>'s AST.
        /// </summary>
        public UnionTypeName(Expression parent, TypeName left) {
            Construct(parent, left, () => {
                Left = left;
                Eat(OpBitOr);
                Right = ParseTypeName();
            });
        }

        internal override void ToAxionCode(CodeBuilder c) {
            c.Write(Left, " | ", Right);
        }

        internal override void ToCSharpCode(CodeBuilder c) {
            throw new NotSupportedException();
        }
    }
}