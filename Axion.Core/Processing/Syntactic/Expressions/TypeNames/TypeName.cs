using System.Collections.Generic;
using Axion.Core.Processing.Errors;
using Axion.Core.Processing.Lexical.Tokens;

namespace Axion.Core.Processing.Syntactic.Expressions.TypeNames {
    public class TypeName : Expression {
        /// <summary>
        ///     <c>
        ///         type ::=
        ///             simple_type  | tuple_type |
        ///             generic_type | array_type | union_type
        ///     </c>
        /// </summary>
        internal static TypeName Parse(SyntaxTreeNode parent) {
            // leading
            TypeName leftTypeName = null;
            // simple
            if (parent.PeekIs(TokenType.Identifier)) {
                leftTypeName = new SimpleTypeName(new NameExpression(parent));
            }
            // tuple
            else if (parent.PeekIs(TokenType.OpenParenthesis)) {
                var tuple = new TupleTypeName(parent);
                leftTypeName = tuple.Types.Count == 1
                    ? tuple.Types[0]
                    : tuple;
            }

            if (leftTypeName == null) {
                return null;
            }

            // middle
            // generic
            if (parent.PeekIs(TokenType.OpLess)) {
                leftTypeName = new GenericTypeName(parent, leftTypeName);
            }

            // array
            if (parent.PeekIs(TokenType.OpenBracket)) {
                leftTypeName = new ArrayTypeName(parent, leftTypeName);
            }

            // trailing
            // union
            if (parent.PeekIs(TokenType.OpBitOr)) {
                leftTypeName = new UnionTypeName(parent, leftTypeName);
            }

            return leftTypeName;
        }

        /// <summary>
        ///     for class, enum, enum item.
        /// </summary>
        internal static List<(NameExpression, TypeName)> ParseTypeArgs(
            SyntaxTreeNode parent,
            bool           allowNamed
        ) {
            var   typeArgs = new List<(NameExpression, TypeName)>();
            Token start    = parent.Peek;
            // '(' type (',' type)* ')'
            if (parent.MaybeEat(TokenType.OpenParenthesis)) {
                if (parent.MaybeEat(TokenType.CloseParenthesis)) {
                    // redundant parens
                    parent.Unit.Blame(
                        BlameType.RedundantEmptyListOfTypeArguments,
                        start.Span.StartPosition,
                        parent.Token.Span.EndPosition
                    );
                }
                else {
                    do {
                        NameExpression name     = null;
                        int            startIdx = parent._Index;
                        if (parent.PeekIs(TokenType.Identifier)) {
                            name = new NameExpression(parent);
                            if (parent.MaybeEat(TokenType.OpAssign)) {
                                if (!allowNamed) {
                                    parent.Unit.ReportError(
                                        "Base of this type cannot be named.",
                                        name
                                    );
                                }
                            }
                            else {
                                parent.MoveTo(startIdx);
                            }
                        }

                        typeArgs.Add((name, Parse(parent)));
                    } while (parent.MaybeEat(TokenType.Comma));

                    parent.Eat(TokenType.CloseParenthesis);
                }
            }

            return typeArgs;
        }
    }
}