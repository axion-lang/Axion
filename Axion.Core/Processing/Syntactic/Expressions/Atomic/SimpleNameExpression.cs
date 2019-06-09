using Axion.Core.Processing.CodeGen;
using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Specification;

namespace Axion.Core.Processing.Syntactic.Expressions.Atomic {
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

        public SimpleNameExpression(AstNode parent, string name) : base(parent) {
            Name = name;
        }

        public SimpleNameExpression(AstNode parent, Token name) : base(parent) {
            Name = name.Value;
            MarkPosition(name);
        }

        internal SimpleNameExpression(AstNode parent) : base(parent) {
            MarkStartAndEat(TokenType.Identifier);
            Name = Token.Value;
            MarkEnd();
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