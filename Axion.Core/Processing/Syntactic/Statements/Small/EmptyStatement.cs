using Axion.Core.Processing.CodeGen;

namespace Axion.Core.Processing.Syntactic.Statements.Small {
    /// <summary>
    ///     empty_stmt:
    ///         (';' | 'pass')
    /// </summary>
    public class EmptyStatement : Statement {
        /// <summary>
        ///     Constructs new <see cref="EmptyStatement"/> from tokens.
        /// </summary>
        public EmptyStatement(SyntaxTreeNode parent) : base(parent) {
            Move();
            MarkPosition(Token);
        }

        public override void ToAxionCode(CodeBuilder c) {
            c.Write("pass");
        }

        public override void ToCSharpCode(CodeBuilder c) {
            c.Write(";");
        }
    }
}