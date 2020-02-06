using System.Collections.Generic;
using Axion.Core.Processing.CodeGen;
using static Axion.Core.Processing.Lexical.Tokens.TokenType;

namespace Axion.Core.Processing.Syntactic.Expressions.TypeNames {
    /// <summary>
    ///     <c>
    ///         generic_type:
    ///             type type_args;
    ///         type_args:
    ///             '[' type {',' type} ']';
    ///     </c>
    /// </summary>
    public class GenericTypeName : TypeName {
        private TypeName target;

        public TypeName Target {
            get => target;
            set => SetNode(ref target, value);
        }

        private NodeList<TypeName> typeArguments;

        public NodeList<TypeName> TypeArguments {
            get => typeArguments;
            set => SetNode(ref typeArguments, value);
        }

        public GenericTypeName(
            Expr                  parent,
            TypeName              target   = null,
            IEnumerable<TypeName> typeArgs = null
        ) : base(parent) {
            Target        = target;
            TypeArguments = NodeList<TypeName>.From(this, typeArgs);
        }

        public GenericTypeName Parse() {
            SetSpan(() => {
                if (Target == null) { }

                Stream.Eat(OpenBracket);
                do {
                    TypeArguments.Add(ParseTypeName());
                } while (Stream.MaybeEat(Comma));

                Stream.Eat(CloseBracket);
            });
            return this;
        }

        public override void ToAxion(CodeWriter c) {
            c.Write(Target, "[");
            c.AddJoin(",", TypeArguments);
            c.Write("]");
        }

        public override void ToCSharp(CodeWriter c) {
            c.Write(Target, "<");
            c.AddJoin(",", TypeArguments);
            c.Write(">");
        }

        public override void ToPython(CodeWriter c) {
            c.Write(Target, "[");
            c.AddJoin(",", TypeArguments);
            c.Write("]");
        }
    }
}