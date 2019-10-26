using Axion.Core.Processing.CodeGen;
using static Axion.Core.Processing.Lexical.Tokens.TokenType;

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

        public UnionTypeName(Expr parent, TypeName left) : base(parent) {
            Left = left;
        }

        public UnionTypeName Parse() {
            SetSpan(() => {
                if (Left == null) {
                    Left = ParseTypeName();
                }

                Stream.Eat(OpBitOr);
                Right = ParseTypeName();
            });
            return this;
        }

        public override void ToAxion(CodeWriter c) {
            c.Write(Left, " | ", Right);
        }
    }
}