using Axion.Core.Processing.Lexical.Tokens;
using static Axion.Core.Processing.Lexical.Tokens.TokenType;

namespace Axion.Core.Processing.Syntactic.Expressions.TypeNames {
    /// <summary>
    ///     <c>
    ///         generic-type:
    ///             type '[' type {',' type} ']';
    ///     </c>
    /// </summary>
    public class GenericTypeName : TypeName {
        private TypeName? target;

        public TypeName? Target {
            get => target;
            set => target = BindNullable(value);
        }

        private Token? typeArgsStartMark;

        public Token? TypeArgsStartMark {
            get => typeArgsStartMark;
            set => typeArgsStartMark = BindNullable(value);
        }

        private NodeList<TypeName>? typeArgs;

        public NodeList<TypeName> TypeArgs {
            get => InitIfNull(ref typeArgs);
            set => typeArgs = Bind(value);
        }

        private Token? typeArgsEndMark;

        public Token? TypeArgsEndMark {
            get => typeArgsEndMark;
            set => typeArgsEndMark = BindNullable(value);
        }

        public GenericTypeName(Node parent) : base(parent) { }

        public GenericTypeName Parse() {
            Target ??= Parse(this);

            TypeArgsStartMark = Stream.Eat(OpenBracket);
            do {
                TypeArgs += Parse(this);
            } while (Stream.MaybeEat(Comma));

            TypeArgsEndMark = Stream.Eat(CloseBracket);
            return this;
        }
    }
}
