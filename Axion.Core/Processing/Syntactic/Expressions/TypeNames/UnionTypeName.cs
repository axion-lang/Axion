using Axion.Core.Processing.CodeGen;
using static Axion.Core.Processing.Lexical.Tokens.TokenType;

namespace Axion.Core.Processing.Syntactic.Expressions.TypeNames {
    /// <summary>
    ///     <c>
    ///         union-type:
    ///             type ('|' type)+;
    ///     </c>
    /// </summary>
    public class UnionTypeName : TypeName {
        private TypeName left;

        public TypeName Left {
            get => left;
            set => left = Bind(value);
        }

        private TypeName right;

        public TypeName Right {
            get => right;
            set => right = Bind(value);
        }

        public UnionTypeName(
            Expr?     parent = null,
            TypeName? left   = null,
            TypeName? right  = null
        ) : base(
            parent
         ?? GetParentFromChildren(left, right)
        ) {
            Left  = left;
            Right = right;
        }

        public UnionTypeName Parse() {
            SetSpan(
                () => {
                    if (Left == null) {
                        Left = Parse(this);
                    }

                    Stream.Eat(OpBitOr);
                    Right = Parse(this);
                }
            );
            return this;
        }

        public override void ToAxion(CodeWriter c) {
            c.Write(Left, " | ", Right);
        }
    }
}