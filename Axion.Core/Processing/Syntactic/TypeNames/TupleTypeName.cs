using System.Collections.Generic;
using System.Linq;
using Axion.Core.Processing.CodeGen;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Axion.Core.Specification.TokenType;

namespace Axion.Core.Processing.Syntactic.TypeNames {
    /// <summary>
    ///     <c>
    ///         tuple_type:
    ///             tuple_paren_expr
    ///     </c>
    /// </summary>
    public class TupleTypeName : TypeName {
        private NodeList<TypeName> types;

        public NodeList<TypeName> Types {
            get => types;
            set => SetNode(ref types, value);
        }

        /// <summary>
        ///     Constructs expression from Axion tokens.
        /// </summary>
        public TupleTypeName(Expression parent) {
            Construct(parent, () => {
                Types = new NodeList<TypeName>(this);
                Eat(OpenParenthesis);

                if (!Peek.Is(CloseParenthesis)) {
                    do {
                        Types.Add(ParseTypeName(parent));
                    } while (MaybeEat(Comma));
                }

                Eat(CloseParenthesis);
            });
        }

        /// <summary>
        ///     Constructs expression from C# syntax.
        /// </summary>
        public TupleTypeName(Expression parent, TupleTypeSyntax csNode) : base(parent) {
            // TODO: add names for tuple items
            Types = new NodeList<TypeName>(
                this,
                csNode.Elements.Select(i => FromCSharp(this, i.Type))
            );
        }

        /// <summary>
        ///     Constructs expression without position in source.
        /// </summary>
        public TupleTypeName(Expression parent, IEnumerable<Expression> exprs) : base(parent) {
            Types = new NodeList<TypeName>(this, exprs.Select(e => e.ValueType));
        }

        internal override void ToAxionCode(CodeBuilder c) {
            c.Write("(");
            c.AddJoin(", ", types);
            c.Write(")");
        }

        internal override void ToCSharpCode(CodeBuilder c) {
            c.Write("(");
            c.AddJoin(", ", types);
            c.Write(")");
        }
    }
}