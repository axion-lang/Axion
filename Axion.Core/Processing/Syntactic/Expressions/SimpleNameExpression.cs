using Axion.Core.Processing.CodeGen;
using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Specification;
using static Axion.Core.Specification.TokenType;

namespace Axion.Core.Processing.Syntactic.Expressions {
    /// <summary>
    ///     <c>
    ///         simple_name: ID
    ///     </c>
    /// </summary>
    public class SimpleNameExpression : NameExpression {
        public override string Name { get; }

        public SimpleNameExpression(string name) {
            Name = name;
        }

        public SimpleNameExpression(SyntaxTreeNode parent, string name) {
            Parent = parent;
            Name   = name;
        }

        public SimpleNameExpression(SyntaxTreeNode parent, Token name) {
            Parent = parent;
            Name   = name.Value;
            MarkPosition(name);
        }

        internal SimpleNameExpression(SyntaxTreeNode parent) : base(parent) {
            EatStartMark(Identifier);
            Name = Token.Value;
            MarkEnd(Token);
        }

        internal override void ToAxionCode(CodeBuilder c) {
            c.Write(Name);
        }

        internal override void ToCSharpCode(CodeBuilder c) {
            if (Spec.CSharp.BuiltInNames.ContainsKey(Name)) {
                c.Write(Spec.CSharp.BuiltInNames[Name]);
                return;
            }

            c.Write(Name);
        }
    }
}