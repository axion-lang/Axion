using Axion.Core.Processing.Syntactic.Expressions.Common;
using Axion.SourceGenerators;
using static Axion.Specification.TokenType;

namespace Axion.Core.Processing.Syntactic.Expressions.Postfix {
    /// <summary>
    ///     <c>
    ///         func-call-expr:
    ///             atom '(' [multiple-arg | (arg for-comprehension)] ')';
    ///     </c>
    /// </summary>
    [SyntaxExpression]
    public partial class FuncCallExpr : PostfixExpr {
        [LeafSyntaxNode] Node target = null!;
        [LeafSyntaxNode] NodeList<FuncCallArg>? args;

        public FuncCallExpr(Node parent) : base(parent) { }

        public FuncCallExpr Parse(bool allowGenerator = false) {
            Stream.Eat(OpenParenthesis);
            Args = FuncCallArg.ParseArgList(
                this,
                allowGenerator: allowGenerator
            );
            End = Stream.Eat(CloseParenthesis).End;
            return this;
        }
    }
}
