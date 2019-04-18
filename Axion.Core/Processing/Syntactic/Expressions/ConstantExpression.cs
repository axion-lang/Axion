using System;
using System.Diagnostics;
using Axion.Core.Processing.CodeGen;
using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Processing.Syntactic.Expressions.TypeNames;
using Axion.Core.Specification;

namespace Axion.Core.Processing.Syntactic.Expressions {
    public class ConstantExpression : Expression {
        public readonly   Token    Value;
        internal override TypeName ValueType => throw new NotImplementedException();

        /// <summary>
        ///     Constructor for 'word' constants
        ///     (such as 'true', 'nil', etc.)
        /// </summary>
        internal ConstantExpression(TokenType type) {
            Debug.Assert(Spec.Keywords.ContainsValue(type));
            Value = new WordToken(type);
        }

        internal ConstantExpression(SyntaxTreeNode parent) : base(parent) {
            Value = Token;
            MarkPosition(Value);
        }

        internal override void ToAxionCode(CodeBuilder c) {
            c.Write(Value);
        }

        internal override void ToCSharpCode(CodeBuilder c) {
            c.Write(Value);
        }
    }
}