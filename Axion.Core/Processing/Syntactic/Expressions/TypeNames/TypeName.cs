using System.Collections.Generic;
using Axion.Core.Processing.Errors;
using Axion.Core.Processing.Syntactic.Expressions.Atomic;
using static Axion.Core.Processing.Lexical.Tokens.TokenType;

namespace Axion.Core.Processing.Syntactic.Expressions.TypeNames {
    /// <summary>
    ///     <c>
    ///         type: simple-type  | tuple-type
    ///             | generic-type | array-type
    ///             | union-type   | func-type;
    ///     </c>
    /// </summary>
    public abstract class TypeName : Expr {
        internal TypeName(Node parent) : base(parent) { }

        internal static TypeName Parse(Node parent) {
            var s = parent.Unit.TokenStream;
            // leading
            TypeName leftTypeName;
            // tuple
            if (s.PeekIs(OpenParenthesis)) {
                var tuple = new TupleTypeName(parent).Parse();
                leftTypeName = tuple.Types.Count == 1 ? tuple.Types[0]! : tuple;
            }
            // simple
            else if (s.PeekIs(Identifier)) {
                leftTypeName = new SimpleTypeName(parent).Parse();
            }
            else {
                LangException.ReportUnexpectedSyntax(Identifier, s.Peek);
                return new SimpleTypeName(parent, "UnknownType");
            }

            // middle
            // generic ('[' followed by not ']')
            if (s.PeekIs(OpenBracket) && !s.PeekByIs(2, CloseBracket)) {
                leftTypeName = new GenericTypeName(parent) {
                    Target = leftTypeName
                }.Parse();
            }

            // array
            if (s.PeekIs(OpenBracket)) {
                leftTypeName = new ArrayTypeName(parent) {
                    ElementType = leftTypeName
                }.Parse();
            }

            // trailing
            // union
            if (s.PeekIs(OpBitOr)) {
                leftTypeName = new UnionTypeName(parent) {
                    Left = leftTypeName
                }.Parse();
            }

            if (s.PeekIs(RightArrow)) {
                leftTypeName = new FuncTypeName(parent) {
                    ArgsType = leftTypeName
                }.Parse();
            }

            return leftTypeName;
        }

        /// <summary>
        ///     <c>
        ///         multiple-type:
        ///             [simple-name '='] type {',' [simple-name '='] type};
        ///     </c>
        ///     for class, enum, enum item.
        /// </summary>
        internal static List<(TypeName type, NameExpr? label)>
            ParseNamedTypeArgs(Node parent) {
            var s        = parent.Unit.TokenStream;
            var typeArgs = new List<(TypeName, NameExpr?)>();
            var start    = s.Peek;

            do {
                NameExpr? name     = null;
                var       startIdx = s.TokenIdx;
                if (s.PeekIs(Identifier)) {
                    var typeLabel = new NameExpr(parent).Parse();
                    if (s.MaybeEat(OpAssign)) {
                        name = typeLabel;
                    }
                    else {
                        s.MoveAbsolute(startIdx);
                    }
                }

                typeArgs.Add((Parse(parent), name));
            } while (s.MaybeEat(Comma));

            if (typeArgs.Count == 0) {
                // redundant parens
                LangException.Report(
                    BlameType.RedundantEmptyListOfTypeArguments,
                    start
                );
            }

            return typeArgs;
        }
    }
}
