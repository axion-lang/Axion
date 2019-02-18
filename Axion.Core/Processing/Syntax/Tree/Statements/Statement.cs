namespace Axion.Core.Processing.Syntax.Tree.Statements {
    public class Statement : SyntaxTreeNode {
        protected Statement() {
        }

        protected Statement(SpannedRegion mark) {
            MarkPosition(mark);
        }
    }
}