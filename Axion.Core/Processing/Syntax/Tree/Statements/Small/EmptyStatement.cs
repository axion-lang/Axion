namespace Axion.Core.Processing.Syntax.Tree.Statements.Small {
    public class EmptyStatement : Statement {
        internal EmptyStatement(SpannedRegion mark) {
            MarkPosition(mark);
        }

        internal override AxionCodeBuilder ToAxionCode(AxionCodeBuilder c) {
            return c + "pass";
        }

        internal override CSharpCodeBuilder ToCSharpCode(CSharpCodeBuilder c) {
            return c + ";";
        }
    }
}