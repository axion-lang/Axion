using Axion.Core.Processing.CodeGen;
using Axion.Core.Processing.Errors;
using Axion.Core.Processing.Syntactic.Expressions.Binary;
using Axion.Core.Processing.Syntactic.Expressions.TypeNames;
using Axion.Core.Specification;
using static Axion.Core.Specification.TokenType;

namespace Axion.Core.Processing.Syntactic.Expressions.Multiple {
    /// <summary>
    ///     <c>
    ///         brace_collection_expr:
    ///             '{'
    ///                 (expr | map_item)
    ///                 [comprehension | (',' (expr | map_item))+]
    ///                 [',']
    ///             '}';
    ///     </c>
    /// </summary>
    public class BraceCollectionExpression : MultipleExpression<Expression> {
        public BraceCollectionType Type { get; }

        public override TypeName ValueType {
            get {
                if (Expressions.Count == 0) {
                    return Spec.UnknownBracesCollectionType;
                }

                switch (Type) {
                    case BraceCollectionType.Map:
                        return Spec.MapType(Expressions[0].ValueType);
                    case BraceCollectionType.Set:
                        return Spec.SetType(Expressions[0].ValueType);
                    default:
                        return Spec.UnknownBracesCollectionType;
                }
            }
        }

        internal BraceCollectionExpression(
            SyntaxTreeNode      parent,
            BraceCollectionType type = BraceCollectionType.Unknown
        ) : base(parent) {
            Expressions = new NodeList<Expression>(this);
            Type        = type;
            EatStartMark(OpenBrace);

            if (!Peek.Is(CloseBrace)) {
                var comprehension = false;
                while (true) {
                    Expression item = ParsePreGlobalExpr(this);
                    if (comprehension) {
                        Unit.Blame(
                            BlameType.CollectionInitializerCannotContainItemsAfterComprehension,
                            item
                        );
                    }

                    // map item (expr ':' expr)
                    if (MaybeEat(Colon)) {
                        if (Type == BraceCollectionType.Set) {
                            Unit.ReportError("Single expression expected", Token);
                        }

                        Type = BraceCollectionType.Map;
                        item = new MapItemExpression(
                            this,
                            item,
                            ParsePreGlobalExpr(this)
                        );
                    }
                    // set item (expr)
                    else {
                        if (Type == BraceCollectionType.Map) {
                            Unit.ReportError("'Key : Value' expression expected", Token);
                        }

                        Type = BraceCollectionType.Set;
                    }

                    if (Peek.Is(KeywordFor)) {
                        item          = new ForComprehension(this, item);
                        comprehension = true;
                    }

                    Expressions.Add(item);

                    if (Peek.Is(CloseBrace)) {
                        break;
                    }

                    Eat(Comma);
                }
            }

            EatEndMark(CloseBrace);

            if (Expressions.Count == 0) {
                Unit.Blame(BlameType.EmptyCollectionLiteralNotSupported, this);
            }
        }

        internal override void ToAxionCode(CodeBuilder c) {
            c.WriteLine("{");
            c.AddJoin("", Expressions, true);
            c.WriteLine();
            c.Write("}");
        }

        internal override void ToCSharpCode(CodeBuilder c) {
            c.WriteLine("{");
            c.AddJoin("", Expressions, true);
            c.WriteLine();
            c.Write("}");
        }
    }

    /// <summary>
    ///     <c>
    ///         map_item_expr:
    ///             test ':' test
    ///     </c>
    /// </summary>
    public class MapItemExpression : LeftRightExpression {
        public MapItemExpression(SyntaxTreeNode parent, Expression left, Expression right) : base(
            parent
        ) {
            Left  = left;
            Right = right;
        }

        internal override void ToAxionCode(CodeBuilder c) {
            c.Write(Left, " : ", Right);
        }

        internal override void ToCSharpCode(CodeBuilder c) {
            c.Write("{ ", Left, ", ", Right, " }");
        }
    }

    public enum BraceCollectionType {
        Unknown,
        Map,
        Set
    }
}