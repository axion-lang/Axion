using Axion.Core.Processing.Lexical.Tokens;
using static Axion.Core.Processing.Lexical.Tokens.TokenType;

namespace Axion.Core.Processing.Syntactic.Expressions.TypeNames {
    /// <summary>
    ///     <c>
    ///         func-type:
    ///             type '->' type
    ///     </c>
    /// </summary>
    public class FuncTypeName : TypeName {
        private TypeName argsType = null!;

        public TypeName ArgsType {
            get => argsType;
            set => argsType = Bind(value);
        }

        private Token? joiningMark;

        public Token? JoiningMark {
            get => joiningMark;
            set => joiningMark = BindNullable(value);
        }

        private TypeName returnType = null!;

        public TypeName ReturnType {
            get => returnType;
            set => returnType = Bind(value);
        }

        public FuncTypeName(Node parent) : base(parent) { }

        public FuncTypeName Parse() {
            ArgsType ??= Parse(this);

            JoiningMark = Stream.Eat(RightArrow);
            ReturnType  = Parse(this);
            return this;
        }
    }
}
