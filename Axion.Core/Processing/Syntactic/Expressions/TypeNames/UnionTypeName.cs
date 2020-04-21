using Axion.Core.Processing.Lexical.Tokens;
using static Axion.Core.Processing.Lexical.Tokens.TokenType;

namespace Axion.Core.Processing.Syntactic.Expressions.TypeNames {
    /// <summary>
    ///     <c>
    ///         union-type:
    ///             type '|' type;
    ///     </c>
    /// </summary>
    public class UnionTypeName : TypeName {
        private TypeName left = null!;

        public TypeName Left {
            get => left;
            set => left = Bind(value);
        }

        private Token? joiningMark;

        public Token? JoiningMark {
            get => joiningMark;
            set => joiningMark = BindNullable(value);
        }

        private TypeName right = null!;

        public TypeName Right {
            get => right;
            set => right = Bind(value);
        }

        public UnionTypeName(Node parent) : base(parent) { }

        public UnionTypeName Parse() {
            Left ??= Parse(this);

            JoiningMark = Stream.Eat(OpBitOr);
            Right       = Parse(this);
            return this;
        }
    }
}
