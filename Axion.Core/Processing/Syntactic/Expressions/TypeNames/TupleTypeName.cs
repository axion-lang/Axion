using System.Collections.Generic;
using Axion.Core.Processing.CodeGen;
using static Axion.Core.Processing.Lexical.Tokens.TokenType;

namespace Axion.Core.Processing.Syntactic.Expressions.TypeNames {
    /// <summary>
    ///     <c>
    ///         tuple-type:
    ///             tuple-paren-expr;
    ///     </c>
    /// TODO fix syntax for tuple type name
    /// </summary>
    public class TupleTypeName : TypeName {
        private NodeList<TypeName> types;

        public NodeList<TypeName> Types {
            get => types;
            set => types = Bind(value);
        }

        public TupleTypeName(
            Expr                   parent,
            IEnumerable<TypeName>? types = null
        ) : base(parent) {
            Types = NodeList<TypeName>.From(this, types);
        }

        public TupleTypeName Parse() {
            SetSpan(
                () => {
                    Stream.Eat(OpenParenthesis);

                    if (!Stream.PeekIs(CloseParenthesis)) {
                        do {
                            Types.Add(Parse(this));
                        } while (Stream.MaybeEat(Comma));
                    }

                    Stream.Eat(CloseParenthesis);
                }
            );
            return this;
        }

        public override void ToDefault(CodeWriter c) {
            c.Write("(");
            c.AddJoin(", ", types);
            c.Write(")");
        }
    }
}