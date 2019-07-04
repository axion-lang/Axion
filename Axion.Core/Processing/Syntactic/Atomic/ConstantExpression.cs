using System.Diagnostics;
using System.Linq;
using Axion.Core.Processing.CodeGen;
using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Processing.Syntactic.TypeNames;
using Axion.Core.Specification;

namespace Axion.Core.Processing.Syntactic.Atomic {
    public class ConstantExpression : Expression {
        public Token Value;
        public override TypeName ValueType => Value.ValueType;

        /// <summary>
        ///     Constructor for 'word' constants
        ///     (such as 'true', 'nil', etc.)
        /// </summary>
        internal ConstantExpression(TokenType type) {
            Debug.Assert(Spec.Constants.Contains(type));
            Value = new WordToken(type);
        }

        internal ConstantExpression(Expression parent) {
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