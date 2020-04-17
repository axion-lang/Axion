using static Axion.Core.Processing.Lexical.Tokens.TokenType;

namespace Axion.Core.Processing.Syntactic.Expressions.TypeNames {
    /// <summary>
    ///     <c>
    ///         union-type:
    ///             type ('|' type)+;
    ///     </c>
    /// </summary>
    public class UnionTypeName : TypeName {
        private TypeName left = null!;

        public TypeName Left {
            get => left;
            set => left = Bind(value);
        }

        private TypeName right = null!;

        public TypeName Right {
            get => right;
            set => right = Bind(value);
        }

        public UnionTypeName(Expr parent) : base(parent) { }

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
    }
}