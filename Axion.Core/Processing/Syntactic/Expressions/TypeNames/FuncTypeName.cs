using Axion.Core.Processing.CodeGen;
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
            set => SetNode(ref argsType, value);
        }

        private TypeName returnType;

        public TypeName ReturnType {
            get => returnType;
            set => SetNode(ref returnType, value);
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
                        ArgsType = TypeName.Parse(this);
                    }

                    Stream.Eat(RightArrow);
                    ReturnType = TypeName.Parse(this);
                }
            );
            return this;
        }

        public override void ToAxion(CodeWriter c) {
            c.Write(ArgsType, " -> ", ReturnType);
        }

        public override void ToCSharp(CodeWriter c) {
            c.Write(
                "Func<", ArgsType, ", ", ReturnType,
                ">"
            );
        }

        public override void ToPython(CodeWriter c) {
            c.Write(
                "Callable[[", ArgsType, "], ", ReturnType,
                "]"
            );
        }
    }
}