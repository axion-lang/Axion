using Axion.Core.Processing.Lexical.Tokens;

namespace Axion.Core.Processing.Syntactic.Expressions.MacroPatterns {
    /// <summary>
    ///     <c>
    ///         token-pattern:
    ///             STRING;
    ///     </c>
    /// </summary>
    public class TokenPattern : Pattern {
        internal Token Value;

        public TokenPattern(Expr parent) : base(parent) { }

        public override bool Match(Expr parent) {
            if (parent.Stream.Peek.Content != Value.Content) {
                return false;
            }
            parent.Stream.Eat();
            parent.Ast.MacroApplicationParts.Peek().Expressions.Add(parent.Stream.Token);
            return true;
        }

        public TokenPattern Parse() {
            Value = Stream.Eat()!;
            Source.RegisterCustomKeyword(Value.Content);
            return this;
        }
    }
}