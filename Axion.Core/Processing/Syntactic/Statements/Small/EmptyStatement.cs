using Axion.Core.Processing.CodeGen;

namespace Axion.Core.Processing.Syntactic.Statements.Small {
    public class EmptyStatement : Statement {
        public EmptyStatement(SpannedRegion mark) {
            MarkPosition(mark);
        }

        internal override CodeBuilder ToAxionCode(CodeBuilder c) {
            return c + "pass";
        }

        internal override CodeBuilder ToCSharpCode(CodeBuilder c) {
            return c + ";";
        }
    }
}