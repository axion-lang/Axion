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
    public class TypeName : Expression {
        internal TypeName(Expression parent) : base(parent) { }
        protected TypeName() { }
        public override TypeName ValueType => this;

        internal TypeName FromCSharp(TypeSyntax csNode) {
            switch (csNode) {
            case ArrayTypeSyntax a: {
                return new ArrayTypeName(this, a);
            }

            case TupleTypeSyntax t: {
                return new TupleTypeName(this, t);
            }

            case GenericNameSyntax g: {
                return new GenericTypeName(this, g);
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
        internal TypeName ParseTypeName() {
            // leading
            TypeName leftTypeName = null;
            // tuple
            if (Peek.Is(OpenParenthesis)) {
                var tuple = new TupleTypeName(this);
                leftTypeName = tuple.Types.Count == 1
                    ? tuple.Types[0]
                    : tuple;
            }

            // simple
            else if (Peek.Is(Identifier)) {
                leftTypeName = new SimpleTypeName(this);
            }
            else {
                BlameInvalidSyntax(Identifier, Peek);
            }

            if (leftTypeName == null) {
                return new SimpleTypeName("UnknownType");
            }

            // middle
            // generic ('[' followed by not ']')
            if (Peek.Is(OpenBracket)
             && !PeekByIs(2, CloseBracket)) {
                leftTypeName = new GenericTypeName(this, leftTypeName);
            }

            // array
            if (Peek.Is(OpenBracket)) {
                leftTypeName = new ArrayTypeName(this, leftTypeName);
            }

            // trailing
            // union
            if (Peek.Is(OpBitOr)) {
                leftTypeName = new UnionTypeName(this, leftTypeName);
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
        internal List<(TypeName type, SimpleNameExpression label)> ParseNamedTypeArgs() {
            var   typeArgs = new List<(TypeName, SimpleNameExpression)>();
            Token start    = Peek;

            if (!Peek.Is(CloseParenthesis)) {
                do {
                    SimpleNameExpression name     = null;
                    int                  startIdx = Ast.CurrentTokenIndex;
                    if (Peek.Is(Identifier)) {
                        name = new SimpleNameExpression(this);
                        if (!MaybeEat(OpAssign)) {
                            MoveTo(startIdx);
                        }
                    }

                    typeArgs.Add((ParseTypeName(), name));
                } while (MaybeEat(Comma));
            }

            if (typeArgs.Count == 0) {
                // redundant parens
                Unit.Blame(
                    BlameType.RedundantEmptyListOfTypeArguments,
                    start.Span.Start,
                    Token.Span.End
                );
            }

            return typeArgs;
        }
    }
}