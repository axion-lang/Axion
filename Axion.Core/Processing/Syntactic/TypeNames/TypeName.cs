using System;
using System.Collections.Generic;
using Axion.Core.Processing.Errors;
using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Processing.Syntactic.Atomic;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Axion.Core.Specification.TokenType;

namespace Axion.Core.Processing.Syntactic.TypeNames {
    /// <summary>
    ///     <c>
    ///         type
    ///             : simple_type  | tuple_type
    ///             | generic_type | array_type
    ///             | union_type;
    ///     </c>
    /// </summary>
    public abstract class TypeName : Expression {
        protected TypeName(Expression parent) : base(parent) { }
        protected TypeName() { }
        public override TypeName ValueType => this;

        internal static TypeName FromCSharp(Expression parent, TypeSyntax csNode) {
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
        ///     Expression is constructed from tokens stream
        ///     that belongs to <see cref="parent"/>'s AST.
        /// </summary>
        internal static TypeName ParseTypeName(Expression parent) {
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
            else if (parent.Peek.Is(Identifier)) {
                leftTypeName = new SimpleTypeName(parent);
            }
            else {
                parent.BlameInvalidSyntax(Identifier, parent.Peek);
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
        ///         [simple_name '='] type {',' [simple_name '='] type};
        ///     </c>
        ///     for class, enum, enum item.
        /// </summary>
        internal static List<(TypeName type, SimpleNameExpression label)> ParseNamedTypeArgs(
            Expression parent
        ) {
            var   typeArgs = new List<(TypeName, SimpleNameExpression)>();
            Token start    = parent.Peek;

            if (!parent.Peek.Is(CloseParenthesis)) {
                do {
                    SimpleNameExpression name     = null;
                    int                  startIdx = parent.Ast.CurrentTokenIndex;
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
                    start.Span.Start,
                    parent.Token.Span.End
                );
            }

            return typeArgs;
        }
    }
}