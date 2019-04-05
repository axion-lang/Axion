using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Processing.Syntactic.Expressions.Binary;
using JetBrains.Annotations;

namespace Axion.Core.Processing.Syntactic.Expressions.Multiple {
    /// <summary>
    ///     <c>
    ///         hash_collection ::=
    ///             '{' [comprehension | hash_initializer] '}'
    ///         hash_initializer ::=
    ///             ({test ':' test ','} | {test ','}) [',']
    ///     </c>
    /// </summary>
    public class HashCollectionExpression : MultipleExpression<Expression> {
        public HashCollectionType Type { get; } = HashCollectionType.Map;

        internal HashCollectionExpression(SyntaxTreeNode parent) {
            Parent      = parent;
            Expressions = new NodeList<Expression>(this);
            StartNode(TokenType.OpenBrace);

            while (!MaybeEat(TokenType.CloseBrace)) {
                Expression itemPart1 = ParseTestExpr(this);
                // map item (expr : expr)
                if (MaybeEat(TokenType.Colon)) {
                    if (Type == HashCollectionType.Set) {
                        Unit.ReportError("Single expression expected", Token);
                    }

                    Type = HashCollectionType.Map;
                    var pair = new PairExpression(itemPart1, ParseTestExpr(this));
                    if (PeekIs(TokenType.KeywordFor)) {
                        // { key : value for (key, value) in iterable }
                        Expressions.Add(new ForComprehension(this));
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
                        Expressions.Add(new ForComprehension(this));
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
            }

            MarkEnd(Token);
        }
    }

    public class PairExpression : LeftRightExpression {
        public PairExpression([NotNull] Expression left, [NotNull] Expression right) {
            Left  = left;
            Right = right;
        }
    }

    public enum HashCollectionType {
        Map,
        Set
    }
}