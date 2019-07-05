using Axion.Core.Processing.CodeGen;
using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Specification;
using static Axion.Core.Specification.TokenType;

namespace Axion.Core.Processing.Syntactic.Atomic {
    /// <summary>
    ///     <c>
    ///         simple_name:
    ///             ID;
    ///     </c>
    /// </summary>
    public class SimpleNameExpression : NameExpression {
        public sealed override string Name { get; set; }

        /// <summary>
        ///     Expression is constructed from tokens stream
        ///     that belongs to <see cref="parent"/>'s AST.
        /// </summary>
        internal SimpleNameExpression(Expression parent) {
            Construct(parent, () => {
                Eat(Identifier);
                Name = Token.Value;
            });
        }

        public SimpleNameExpression(string name) {
            Name = name;
        }

        public SimpleNameExpression(Expression parent, string name) : base(parent) {
            Name = name;
        }

        public SimpleNameExpression(Expression parent, Token name) : base(parent) {
            Name = name.Value;
            MarkPosition(name);
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