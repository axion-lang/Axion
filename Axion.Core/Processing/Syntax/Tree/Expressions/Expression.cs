using Axion.Core.Processing.Lexical.Tokens;

namespace Axion.Core.Processing.Syntax.Tree.Expressions {
    public abstract class Expression : SyntaxTreeNode {
        protected Expression() { }

        protected Expression(Token start, Token end) {
            MarkPosition(start, end);
        }

        internal virtual string CannotDeleteReason => null;
        internal virtual string CannotAssignReason => null;
    }
}