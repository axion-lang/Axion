namespace Axion.Core.Processing.Syntax.Tree.Statements {
    public class Statement : SpannedRegion {
        protected Statement() {
        }

        protected Statement(SpannedRegion mark) {
            MarkPosition(mark);
        }
    }
}