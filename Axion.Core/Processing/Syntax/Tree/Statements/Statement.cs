using Axion.Core.Processing.Lexical.Tokens;

namespace Axion.Core.Processing.Syntax.Tree.Statements {
    public class Statement : SyntaxTreeNode {
        protected Statement() { }

        protected Statement(Token startToken) {
            MarkStart(startToken);
        }
    }
}