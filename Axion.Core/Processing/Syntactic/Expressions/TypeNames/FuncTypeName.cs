using static Axion.Core.Processing.Lexical.Tokens.TokenType;

namespace Axion.Core.Processing.Syntactic.Expressions.TypeNames {
    /// <summary>
    ///     <c>
    ///     </c>
    /// </summary>
    public class FuncTypeName : TypeName {
        private TypeName argsType = null!;

        public TypeName ArgsType {
            get => argsType;
            set => argsType = Bind(value);
        }

        private TypeName returnType = null!;

        public TypeName ReturnType {
            get => returnType;
            set => returnType = Bind(value);
        }

        public FuncTypeName(Node parent) : base(parent) { }

        public FuncTypeName Parse() {
            SetSpan(
                () => {
                    if (ArgsType == null) {
                        ArgsType = Parse(this);
                    }

                    Stream.Eat(RightArrow);
                    ReturnType = Parse(this);
                }
            );
            return this;
        }
    }
}
