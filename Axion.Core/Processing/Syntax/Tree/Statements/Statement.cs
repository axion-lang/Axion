namespace Axion.Core.Processing.Syntax.Tree.Statements {
    public class Statement : TreeNode {
        protected Statement() {
        }

        protected Statement(SpannedRegion mark) {
            MarkPosition(mark);
        }
    }
}