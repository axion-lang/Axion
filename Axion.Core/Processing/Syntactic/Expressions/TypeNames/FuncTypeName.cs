using static Axion.Core.Processing.Lexical.Tokens.TokenType;

namespace Axion.Core.Processing.Syntactic.Expressions.TypeNames {
    /// <summary>
    ///     <c>
    ///     </c>
    /// </summary>
    public class FuncTypeName : TypeName {
        private TypeName argsType;

        public TypeName ArgsType {
            get => argsType;
            set => argsType = Bind(value);
        }

        private TypeName returnType;

        public TypeName ReturnType {
            get => returnType;
            set => returnType = Bind(value);
        }

        public FuncTypeName(
            Expr?     parent     = null,
            TypeName? argsType   = null,
            TypeName? returnType = null
        ) : base(
            parent
         ?? GetParentFromChildren(argsType, returnType)
        ) {
            ArgsType   = argsType;
            ReturnType = returnType;
        }

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