using System;
using System.Collections.Generic;
using Axion.Core.Processing.Errors;
using Axion.Core.Processing.Lexical.Tokens;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Axion.Core.Specification.TokenType;

namespace Axion.Core.Processing.Syntactic.Expressions.TypeNames {
    /// <summary>
    ///     <c>
    ///         type
    ///             : simple_type  | tuple_type
    ///             | generic_type | array_type
    ///             | union_type;
    ///     </c>
    /// </summary>
    public abstract class TypeName : Expression {
        public override TypeName ValueType => this;

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

        internal static TypeName ParseTypeName(SyntaxTreeNode parent) {
            // leading
            TypeName leftTypeName = null;
            // tuple
            if (parent.Peek.Is(OpenParenthesis)) {
                var tuple = new TupleTypeName(parent);
                leftTypeName = tuple.Types.Count == 1
                    ? tuple.Types[0]
                    : tuple;
            }

            // simple
            else if (parent.EnsureNext(Identifier)) {
                leftTypeName = new SimpleTypeName(NameExpression.ParseName(parent));
            }

            if (leftTypeName == null) {
                return new SimpleTypeName("UnknownType");
            }

            // middle
            // generic ('[' followed by not ']')
            if (parent.Peek.Is(OpenBracket)
                && !parent.PeekByIs(2, CloseBracket)) {
                leftTypeName = new GenericTypeName(parent, leftTypeName);
            }

            // array
            if (parent.Peek.Is(OpenBracket)) {
                leftTypeName = new ArrayTypeName(parent, leftTypeName);
            }

            // trailing
            // union
            if (parent.Peek.Is(OpBitOr)) {
                leftTypeName = new UnionTypeName(parent, leftTypeName);
            }

            return leftTypeName;
        }

        /// <summary>
        ///     <c>
        ///         type_list:
        ///             [simple_name '='] type {',' [simple_name '='] type};
        ///     </c>
        ///     for class, enum, enum item.
        /// </summary>
        internal static List<(TypeName type, SimpleNameExpression? label)> ParseNamedTypeArgs(
            SyntaxTreeNode parent
        ) {
            var   typeArgs = new List<(TypeName, SimpleNameExpression?)>();
            Token start    = parent.Peek;

            if (!parent.Peek.Is(CloseParenthesis)) {
                do {
                    SimpleNameExpression? name     = null;
                    int                   startIdx = parent.Ast.Index;
                    if (parent.Peek.Is(Identifier)) {
                        name = new SimpleNameExpression(parent);
                        if (!parent.MaybeEat(OpAssign)) {
                            parent.MoveTo(startIdx);
                        }
                    }

                    typeArgs.Add((ParseTypeName(parent), name));
                } while (parent.MaybeEat(Comma));
            }

            if (typeArgs.Count == 0) {
                // redundant parens
                parent.Unit.Blame(
                    BlameType.RedundantEmptyListOfTypeArguments,
                    start.Span.StartPosition,
                    parent.Token.Span.EndPosition
                );
            }

            return typeArgs;
        }
    }
}