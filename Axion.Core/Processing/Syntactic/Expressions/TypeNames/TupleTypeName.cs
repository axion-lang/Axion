using System.Collections.Generic;
using Axion.Core.Processing.CodeGen;
using static Axion.Core.Processing.Lexical.Tokens.TokenType;

namespace Axion.Core.Processing.Syntactic.Expressions.TypeNames {
    /// <summary>
    ///     <c>
    ///         tuple_type:
    ///             tuple_paren_expr;
    ///     </c>
    /// TODO fix syntax for tuple type name
    /// </summary>
    public class TupleTypeName : TypeName {
        private NodeList<TypeName> types;

        public NodeList<TypeName> Types {
            get => types;
            set => SetNode(ref types, value);
        }

        public TupleTypeName(Expr parent, IEnumerable<TypeName> types = null) : base(parent) {
            Types = NodeList<TypeName>.From(this, types);
        }

        public TupleTypeName Parse() {
            SetSpan(
                () => {
                    Stream.Eat(OpenParenthesis);

                    if (!Stream.PeekIs(CloseParenthesis)) {
                        do {
                            Types.Add(ParseTypeName());
                        } while (Stream.MaybeEat(Comma));
                    }

                    Stream.Eat(CloseParenthesis);
                }
            );
            return this;
        }

        public override void ToAxion(CodeWriter c) {
            c.Write("(");
            c.AddJoin(", ", types);
            c.Write(")");
        }

        public override void ToCSharp(CodeWriter c) {
            c.Write("(");
            c.AddJoin(", ", types);
            c.Write(")");
        }

        public override void ToPython(CodeWriter c) {
            c.Write("(");
            c.AddJoin(", ", types);
            c.Write(")");
        }
    }
}