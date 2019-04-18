using Axion.Core.Processing.CodeGen;
using Axion.Core.Processing.Syntactic.Expressions.Binary;
using Axion.Core.Processing.Syntactic.Expressions.TypeNames;
using Axion.Core.Specification;

namespace Axion.Core.Processing.Syntactic.Expressions.Multiple {
    /// <summary>
    ///     <c>
    ///         hash_collection:
    ///             '{' [comprehension | hash_initializer] '}'
    ///         hash_initializer:
    ///             ({test ':' test ','} | {test ','}) [',']
    ///     </c>
    /// </summary>
    public class HashCollectionExpression : MultipleExpression<Expression> {
        public HashCollectionType Type { get; } = HashCollectionType.Unknown;

        internal override TypeName ValueType =>
            Type switch {
                HashCollectionType.Map => Spec.MapType(),
                HashCollectionType.Set => Spec.SetType()
                };

        internal HashCollectionExpression(SyntaxTreeNode parent) : base(parent) {
            Expressions = new NodeList<Expression>(this);
            MarkStart(TokenType.OpenBrace);

            do {
                Expression itemPart1 = ParseTestExpr(this);
                // map item (expr : expr)
                if (MaybeEat(TokenType.Colon)) {
                    if (Type == HashCollectionType.Set) {
                        Unit.ReportError("Single expression expected", Token);
                    }

                    Type = HashCollectionType.Map;
                    var pair = new MapItemExpression(itemPart1, ParseTestExpr(this));
                    if (PeekIs(TokenType.KeywordFor)) {
                        // { key : value for (key, value) in iterable }
                        Expressions.Add(new ForComprehension(this, pair));
                        Eat(TokenType.CloseBrace);
                        break;
                    }

                    Expressions.Add(pair);
                }
                // set item (expr)
                else {
                    if (Type == HashCollectionType.Map) {
                        Unit.ReportError("'Key : Value' expression expected", Token);
                    }

                    Type = HashCollectionType.Set;
                    if (PeekIs(TokenType.KeywordFor)) {
                        // { x * 2 for x in { 1, 2, 3 } }
                        Expressions.Add(new ForComprehension(this, itemPart1));
                        Eat(TokenType.CloseBrace);
                        break;
                    }

                    Expressions.Add(itemPart1);
                }

                if (MaybeEat(TokenType.Comma)) {
                    continue;
                }

                Eat(TokenType.CloseBrace);
                break;
            } while (!MaybeEat(TokenType.CloseBrace));

            MarkEnd(Token);
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

    public class MapItemExpression : LeftRightExpression {
        public MapItemExpression(Expression left, Expression right) {
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

    public enum HashCollectionType {
        Unknown,
        Map,
        Set
    }
}