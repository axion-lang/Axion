using Axion.Core.Processing.CodeGen;
using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Processing.Syntactic.TypeNames;

namespace Axion.Core.Processing.Syntactic {
    /// <summary>
    ///     <c>
    ///         tuple_expr:
    ///             tuple_paren_expr | (expr_list [',']);
    ///         tuple_paren_expr:
    ///             '(' expr_list [','] ')';
    ///     </c>
    /// </summary>
    public class TupleExpression : Expression {
        private NodeList<Expression> expressions;

        public NodeList<Expression> Expressions {
            get => expressions;
            set => SetNode(ref expressions, value);
        }

        public override TypeName ValueType => new TupleTypeName(this, Expressions);

        /// <summary>
        ///     Constructor for empty tuple.
        /// </summary>
        internal TupleExpression(
            Expression parent,
            Token      start,
            Token      end
        ) : base(parent) {
            Expressions = new NodeList<Expression>(this);
            MarkPosition(start, end);
        }

        internal TupleExpression(
            Expression           parent,
            NodeList<Expression> expressions
        ) : base(parent) {
            Expressions = expressions;
            if (Expressions.Count > 0) {
                MarkPosition(
                    Expressions[0],
                    Expressions[Expressions.Count - 1]
                );
            }
        }

        internal override void ToAxionCode(CodeBuilder c) {
            c.Write("(");
            c.AddJoin(", ", Expressions);
            c.Write(")");
        }

        internal override void ToCSharpCode(CodeBuilder c) {
            c.Write("(");
            c.AddJoin(", ", Expressions);
            c.Write(")");
        }
    }
}