using Axion.Core.Processing.Lexical.Tokens;

namespace Axion.Core.Processing.Syntactic.Expressions.Patterns {
    /// <summary>
    ///     <c>
    ///         token-pattern:
    ///             STRING;
    ///     </c>
    /// </summary>
    public class TokenPattern : Pattern {
        internal Token? Value;

        public TokenPattern(Node parent) : base(parent) { }

        public override bool Match(Expr parent) {
            var s = parent.Unit.TokenStream;
            if (s.Peek.Content != Value?.Content) {
                return false;
            }

            parent.Ast.MatchedMacros.Peek().Nodes.Add(s.Eat());
            return true;
        }

        public TokenPattern Parse() {
            Value = Stream.Eat();
            Unit.Module.RegisterCustomKeyword(Value.Content);
            return this;
        }
    }
}
