using System;
using System.Collections.Generic;
using Axion.Core.Processing.Errors;
using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Specification;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Axion.Core.Processing.Syntactic.Expressions.TypeNames {
    public abstract class TypeName : Expression {
        internal static TypeName FromCSharp(SyntaxTreeNode parent, TypeSyntax csNode) {
            switch (csNode) {
                case ArrayTypeSyntax a: {
                    return new ArrayTypeName(parent, a);
                }

                case TupleTypeSyntax t: {
                    return new TupleTypeName(parent, t);
                }

                case GenericNameSyntax g: {
                    return new GenericTypeName(parent, g);
                }

                case PredefinedTypeSyntax p: {
                    return new SimpleTypeName(p.Keyword.Text);
                }
            }

            throw new NotSupportedException();
        }

        /// <summary>
        ///     <c>
        ///         type:
        ///             simple_type  | tuple_type |
        ///             generic_type | array_type | union_type
        ///     </c>
        /// </summary>
        internal static TypeName? ParseTypeName(SyntaxTreeNode parent) {
            // leading
            TypeName? leftTypeName = null;
            // simple
            if (parent.Peek.Is(TokenType.OpenParenthesis)) {
                var tuple = new TupleTypeName(parent);
                leftTypeName = tuple.Types.Count == 1
                    ? tuple.Types[0]
                    : tuple;
            }

            // tuple
            else if (parent.EnsureNext(TokenType.Identifier)) {
                leftTypeName = new SimpleTypeName(new NameExpression(parent));
            }

            if (leftTypeName == null) {
                return null;
            }

            // middle
            // generic ('[' followed by not ']')
            if (parent.Peek.Is(TokenType.OpenBracket)
                && !parent.PeekByIs(2, TokenType.CloseBracket)) {
                leftTypeName = new GenericTypeName(parent, leftTypeName);
            }

            // array
            if (parent.Peek.Is(TokenType.OpenBracket)) {
                leftTypeName = new ArrayTypeName(parent, leftTypeName);
            }

            // trailing
            // union
            if (parent.Peek.Is(TokenType.OpBitOr)) {
                leftTypeName = new UnionTypeName(parent, leftTypeName);
            }

            return leftTypeName;
        }

        /// <summary>
        ///     for class, enum, enum item.
        /// </summary>
        internal static List<(TypeName?, NameExpression?)>
            ParseNamedTypeArgs(SyntaxTreeNode parent) {
            var   typeArgs = new List<(TypeName?, NameExpression?)>();
            Token start    = parent.Peek;
            if (parent.MaybeEat(TokenType.OpenParenthesis)) {
                if (!parent.Peek.Is(TokenType.CloseParenthesis)) {
                    do {
                        NameExpression? name     = null;
                        int             startIdx = parent.Ast.Index;
                        if (parent.Peek.Is(TokenType.Identifier)) {
                            name = new NameExpression(parent);
                            if (!parent.MaybeEat(TokenType.OpAssign)) {
                                parent.MoveTo(startIdx);
                            }
                        }

                        typeArgs.Add((ParseTypeName(parent), name));
                    } while (parent.MaybeEat(TokenType.Comma));
                }

                parent.Eat(TokenType.CloseParenthesis);

                if (typeArgs.Count == 0) {
                    // redundant parens
                    parent.Unit.Blame(
                        BlameType.RedundantEmptyListOfTypeArguments,
                        start.Span.StartPosition,
                        parent.Token.Span.EndPosition
                    );
                }
            }

            return typeArgs;
        }

        /// <summary>
        ///     <c>
        ///         '(' type {',' type} ')'
        ///     </c>
        ///     for class, enum, enum item.
        /// </summary>
        internal static NodeList<TypeName?> ParseTypeArgs(SyntaxTreeNode parent) {
            var   typeArgs = new NodeList<TypeName?>(parent);
            Token start    = parent.Peek;
            if (parent.MaybeEat(TokenType.OpenParenthesis)) {
                if (!parent.Peek.Is(TokenType.CloseParenthesis)) {
                    do {
                        typeArgs.Add(ParseTypeName(parent));
                    } while (parent.MaybeEat(TokenType.Comma));
                }

                parent.Eat(TokenType.CloseParenthesis);

                if (typeArgs.Count == 0) {
                    // redundant parens
                    parent.Unit.Blame(
                        BlameType.RedundantEmptyListOfTypeArguments,
                        start.Span.StartPosition,
                        parent.Token.Span.EndPosition
                    );
                }
            }

            return typeArgs;
        }
    }
}