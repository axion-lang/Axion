namespace Axion.Core.Processing.Syntactic.MacroPatterns {
    public class TokenPattern : IPattern {
        internal readonly string Value;

        public TokenPattern(string value) {
            Value = value;
        }

        public bool Match(AstNode parent) {
            if (parent.Peek.Value == Value) {
                parent.GetNext();
                parent.Ast.MacroApplicationParts.Add(parent.Token);
                return true;
            }
            return false;
        }
    }
}