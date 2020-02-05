using System.Collections.Generic;
using Axion.Core.Processing.Errors;
using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Processing.Syntactic.Expressions.Atomic;
using Axion.Core.Processing.Traversal;
using static Axion.Core.Processing.Lexical.Tokens.TokenType;

namespace Axion.Core.Processing.Syntactic.Expressions.TypeNames {
    /// <summary>
    ///     <c>
    ///         type
    ///             : simple_type  | tuple_type
    ///             | generic_type | array_type
    ///             | union_type;
    ///     </c>
    /// </summary>
    public class TypeName : Expr {
        internal TypeName(Expr parent) : base(parent) { }
        protected TypeName() { }

        [NoTraversePath]
        public override TypeName ValueType => this;

        internal TypeName ParseTypeName() {
            // leading
            TypeName leftTypeName;
            // tuple
            if (Stream.PeekIs(OpenParenthesis)) {
                TupleTypeName tuple = new TupleTypeName(this).Parse();
                leftTypeName = tuple.Types.Count == 1
                    ? tuple.Types[0]
                    : tuple;
            }
            // simple
            else if (Stream.PeekIs(Identifier)) {
                leftTypeName = new SimpleTypeName(this).Parse();
            }
            else {
                LangException.ReportUnexpectedSyntax(Identifier, Stream.Peek);
                return new SimpleTypeName("UnknownType");
            }

            // middle
            // generic ('[' followed by not ']')
            if (Stream.PeekIs(OpenBracket) && !Stream.PeekByIs(2, CloseBracket)) {
                leftTypeName = new GenericTypeName(this, leftTypeName).Parse();
            }

            // array
            if (Stream.PeekIs(OpenBracket)) {
                leftTypeName = new ArrayTypeName(this, leftTypeName).Parse();
            }

            // trailing
            // union
            if (Stream.PeekIs(OpBitOr)) {
                leftTypeName = new UnionTypeName(this, leftTypeName).Parse();
            }

            if (Stream.PeekIs(RightArrow)) {
                leftTypeName = new FuncTypeName(this, leftTypeName).Parse();
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
        internal List<(TypeName type, NameExpr label)> ParseNamedTypeArgs() {
            var   typeArgs = new List<(TypeName, NameExpr)>();
            Token start    = Stream.Peek;

            do {
                NameExpr name     = null;
                int      startIdx = Stream.TokenIdx;
                if (Stream.PeekIs(Identifier)) {
                    name = new NameExpr(this).Parse();
                    if (!Stream.MaybeEat(OpAssign)) {
                        Stream.MoveAbsolute(startIdx);
                    }
                }

                typeArgs.Add((ParseTypeName(), name));
            } while (Stream.MaybeEat(Comma));

            if (typeArgs.Count == 0) {
                // redundant parens
                LangException.Report(
                    BlameType.RedundantEmptyListOfTypeArguments,
                    new Span(Source, start.Start, Stream.Token.End)
                );
            }

            return typeArgs;
        }
    }
}