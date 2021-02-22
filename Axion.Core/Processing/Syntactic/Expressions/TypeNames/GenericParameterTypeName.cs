using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Processing.Syntactic.Expressions.Atomic;
using static Axion.Specification.TokenType;

namespace Axion.Core.Processing.Syntactic.Expressions.TypeNames {
    public class GenericParameterTypeName : TypeName, ITypeParameter {
        private NameExpr name = null!;

        public NameExpr Name {
            get => name;
            set => name = Bind(value);
        }

        private Token? typeConstraintsStartMark;

        public Token? TypeConstraintsStartMark {
            get => typeConstraintsStartMark;
            set => typeConstraintsStartMark = BindNullable(value);
        }

        private NodeList<TypeName>? typeConstraints;

        public NodeList<TypeName> TypeConstraints {
            get => InitIfNull(ref typeConstraints);
            set => typeConstraints = Bind(value);
        }

        public GenericParameterTypeName(Node parent) : base(parent) { }

        public GenericParameterTypeName Parse() {
            TypeConstraintsStartMark = Stream.Eat(Colon);
            do {
                TypeConstraints += Parse(this);
            } while (Stream.MaybeEat(Comma));
            return this;
        }
    }
}
