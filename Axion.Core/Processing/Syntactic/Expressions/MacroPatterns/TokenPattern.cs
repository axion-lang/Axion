namespace Axion.Core.Processing.Syntactic.Expressions.MacroPatterns {
    public class TokenPattern : IPattern {
        internal readonly string Value;

        public TokenPattern(string value) {
            Value = value;
        }

        public bool Match(Expr parent) {
            if (parent.Stream.Peek.Content == Value) {
                parent.Stream.Eat();
                parent.Ast.MacroApplicationParts.Peek().Expressions.Add(parent.Stream.Token);
                return true;
            }

            return false;
        }
    }
}