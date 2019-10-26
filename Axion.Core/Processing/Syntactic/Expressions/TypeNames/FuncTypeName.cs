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
            Expr     parent,
            TypeName argsType   = null,
            TypeName returnType = null
        ) : base(parent) {
            ArgsType   = argsType;
            ReturnType = returnType;
        }

        public FuncTypeName Parse() {
            SetSpan(() => {
                if (ArgsType == null) {
                    ArgsType = ParseTypeName();
                }

                Stream.Eat(RightArrow);
                ReturnType = ParseTypeName();
            });
            return this;
        }

        public override void ToAxion(CodeWriter c) {
            c.Write(ArgsType, " -> ", ReturnType);
        }

        public override void ToCSharp(CodeWriter c) {
            c.Write("Func<", ArgsType, ", ", ReturnType, ">");
        }

        public override void ToPython(CodeWriter c) {
            c.Write("Callable[[", ArgsType, "], ", ReturnType, "]");
        }
    }
}