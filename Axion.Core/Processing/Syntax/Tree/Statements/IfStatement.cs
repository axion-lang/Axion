using System;
using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Processing.Syntax.Tree.Expressions;

namespace Axion.Core.Processing.Syntax.Tree.Statements {
    public class IfStatement : Statement {
        private Expression condition;

        public Expression Condition {
            get => condition;
            set {
                value.Parent = this;
                condition    = value;
            }
        }

        private BlockStatement thenBlock;

        public BlockStatement ThenBlock {
            get => thenBlock;
            set {
                value.Parent = this;
                thenBlock    = value;
            }
        }

        private BlockStatement elseBlock;

        public BlockStatement ElseBlock {
            get => elseBlock;
            set {
                if (value != null) {
                    value.Parent = this;
                }

                elseBlock = value;
            }
        }

        public IfStatement(
            Token          startToken,
            Expression     condition,
            BlockStatement thenBlock,
            BlockStatement elseBlock
        ) : base(startToken) {
            Condition = condition ?? throw new ArgumentNullException(nameof(condition));
            ThenBlock = thenBlock ?? throw new ArgumentNullException(nameof(thenBlock));
            ElseBlock = elseBlock;

            MarkEnd(ElseBlock ?? ThenBlock);
        }

        internal override AxionCodeBuilder ToAxionCode(AxionCodeBuilder c) {
            c = c + "if " + Condition + " " + ThenBlock + " else ";
            if (ElseBlock != null) {
                c = c + ElseBlock;
            }
            else {
                c = c + "{ }";
            }

            return c;
        }
    }
}