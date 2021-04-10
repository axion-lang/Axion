using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Processing.Syntactic.Expressions.Common;
using Axion.Core.Processing.Syntactic.Expressions.Generic;
using Axion.Core.Processing.Syntactic.Expressions.TypeNames;
using Axion.SourceGenerators;
using Axion.Specification;
using static Axion.Specification.TokenType;

namespace Axion.Core.Processing.Syntactic.Expressions.Statements {
    /// <summary>
    ///     <code>
    ///         return-expr:
    ///             'return' [multiple-expr];
    ///     </code>
    /// </summary>
    [SyntaxExpression]
    public partial class ReturnExpr : Node {
        [LeafSyntaxNode] Token? kwReturn;
        [LeafSyntaxNode] Node? value;

        public override TypeName? ValueType => Value?.ValueType;

        public ReturnExpr(Node parent) : base(parent) { }

        public ReturnExpr Parse() {
            KwReturn = Stream.Eat(KeywordReturn);
            if (!Stream.PeekIs(Spec.NeverExprStartTypes)) {
                Value = Multiple.ParsePermissively<InfixExpr>(this);
            }

            return this;
        }
    }
}
