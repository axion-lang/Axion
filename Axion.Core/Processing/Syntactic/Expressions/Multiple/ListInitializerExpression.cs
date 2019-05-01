using Axion.Core.Processing.CodeGen;
using Axion.Core.Processing.Errors;
using Axion.Core.Processing.Syntactic.Expressions.TypeNames;
using Axion.Core.Specification;
using static Axion.Core.Specification.TokenType;

namespace Axion.Core.Processing.Syntactic.Expressions.Multiple {
    /// <summary>
    ///     <c>
    ///         list_initializer_expr:
    ///             '['
    ///                 expr
    ///                 [comprehension | (',' expr)+]
    ///                 [',']
    ///             ']';
    ///     </c>
    /// </summary>
    public class ListInitializerExpression : MultipleExpression<Expression> {
        public override TypeName ValueType {
            get {
                if (Expressions.Count == 0) {
                    return Spec.UnknownListType;
                }

                return Spec.ListType(Expressions[0].ValueType);
            }
        }

        internal ListInitializerExpression(SyntaxTreeNode parent) : base(parent) {
            Expressions = new NodeList<Expression>(this);
            EatStartMark(OpenBracket);
            if (!Peek.Is(CloseBracket)) {
                while (true) {
                    Expressions.Add(ParsePreGlobalExpr(this));
                    if (Peek.Is(CloseBracket)) {
                        break;
                    }

                    Eat(Comma);
                }
            }

            EatEndMark(CloseBracket);
            if (Expressions.Count == 0) {
                Unit.Blame(BlameType.EmptyCollectionLiteralNotSupported, this);
            }
        }

        public ListInitializerExpression(NodeList<Expression> expressions) {
            Expressions = expressions ?? new NodeList<Expression>(this);
        }

        internal override void ToAxionCode(CodeBuilder c) {
            c.WriteLine("[");
            c.AddJoin("", Expressions, true);
            c.WriteLine();
            c.Write("]");
        }

        internal override void ToCSharpCode(CodeBuilder c) {
            c.WriteLine("{");
            c.AddJoin("", Expressions, true);
            c.WriteLine();
            c.Write("}");
        }
    }
}