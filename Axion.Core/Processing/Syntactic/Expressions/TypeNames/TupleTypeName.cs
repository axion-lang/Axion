using Axion.Core.Processing.Lexical.Tokens;
using static Axion.Specification.TokenType;

namespace Axion.Core.Processing.Syntactic.Expressions.TypeNames {
    /// <summary>
    ///     <c>
    ///         tuple-type:
    ///             '(' [type {',' type}] ')';
    ///     </c>
    /// </summary>
    public class TupleTypeName : TypeName {
        private Token? startMark;

        public Token? StartMark {
            get => startMark;
            set => startMark = BindNullable(value);
        }

        private NodeList<TypeName>? types;

        public NodeList<TypeName> Types {
            get => InitIfNull(ref types);
            set => types = Bind(value);
        }

        private Token? endMark;

        public Token? EndMark {
            get => endMark;
            set => endMark = BindNullable(value);
        }

        public TupleTypeName(Node parent) : base(parent) { }

        public TupleTypeName Parse() {
            StartMark = Stream.Eat(OpenParenthesis);
            if (!Stream.PeekIs(CloseParenthesis)) {
                do {
                    Types += Parse(this);
                } while (Stream.MaybeEat(Comma));
            }

            EndMark = Stream.Eat(CloseParenthesis);
            return this;
        }
    }
}
