using Axion.Core.Processing.Syntactic.Expressions.Common;
using Axion.SourceGenerators;
using static Axion.Specification.TokenType;

namespace Axion.Core.Processing.Syntactic.Expressions.Postfix {
    /// <summary>
    ///     <code>
    ///         func-call-expr:
    ///             atom '(' [multiple-arg | (arg for-comprehension)] ')';
    ///     </code>
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
