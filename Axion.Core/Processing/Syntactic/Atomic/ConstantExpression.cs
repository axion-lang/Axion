using Axion.Core.Processing.CodeGen;
using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Processing.Syntactic.TypeNames;

namespace Axion.Core.Processing.Syntactic.Atomic {
    /// <summary>
    ///     <c>
    ///         const_expr:
    ///             CONST_TOKEN | STRING+;
    ///     </c>
    /// </summary>
    public class ConstantExpression : Expression {
        public Token Value;
        public override TypeName ValueType => Value.ValueType;

        /// <summary>
        ///     Expression is constructed from tokens stream
        ///     that belongs to <see cref="parent"/>'s AST.
        /// </summary>
        internal ConstantExpression(Expression parent) {
            // TODO add concatenation of string literals
            Construct(parent, () => {
                Value = Peek;
                Eat();
            });
        }

        internal override void ToAxionCode(CodeBuilder c) {
            c.Write(Value);
        }

        internal override void ToCSharpCode(CodeBuilder c) {
            c.Write(Value);
        }
    }
}