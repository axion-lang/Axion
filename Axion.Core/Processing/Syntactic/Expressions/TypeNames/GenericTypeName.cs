using static Axion.Core.Processing.Lexical.Tokens.TokenType;

namespace Axion.Core.Processing.Syntactic.Expressions.TypeNames {
    /// <summary>
    ///     <c>
    ///         generic-type:
    ///             type type-args;
    ///         type-args:
    ///             '[' type {',' type} ']';
    ///     </c>
    /// </summary>
    public class GenericTypeName : TypeName {
        private TypeName target = null!;

        public TypeName Target {
            get => target;
            set => target = Bind(value);
        }

        private NodeList<TypeName> typeArguments = null!;

        public NodeList<TypeName> TypeArguments {
            get => typeArguments;
            set => typeArguments = Bind(value);
        }

        public GenericTypeName(Node parent) : base(parent) { }

        public GenericTypeName Parse() {
            TypeArguments ??= new NodeList<TypeName>(this);
            SetSpan(
                () => {
                    if (Target == null) {
                        Target = Parse(this);
                    }

                    Stream.Eat(OpenBracket);
                    do {
                        TypeArguments.Add(Parse(this));
                    } while (Stream.MaybeEat(Comma));

                    Stream.Eat(CloseBracket);
                }
            );
            return this;
        }
    }
}
