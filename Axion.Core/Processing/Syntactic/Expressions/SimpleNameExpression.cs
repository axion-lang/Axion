using Axion.Core.Processing.CodeGen;
using Axion.Core.Specification;

namespace Axion.Core.Processing.Syntactic.Expressions {
    /// <summary>
    ///     <c>
    ///         name:
    ///             ID {'.' ID}
    ///     </c>
    /// </summary>
    public class SimpleNameExpression : NameExpression {
        public string Name { get; }

        public SimpleNameExpression(string name) {
            Name = name;
        }

        public SimpleNameExpression(SyntaxTreeNode parent, string name) {
            Parent = parent;
            Name   = name;
        }

        internal SimpleNameExpression(SyntaxTreeNode parent) : base(parent) {
            MarkStart(TokenType.Identifier);
            Name = Token.Value;
            MarkEnd(Token);
        }

        public override void ToAxionCode(CodeBuilder c) {
            c.Write(Name);
        }

        public override void ToCSharpCode(CodeBuilder c) {
            if (Name == "self") {
                c.Write("this");
                return;
            }

            c.Write(Name);
        }
    }
}