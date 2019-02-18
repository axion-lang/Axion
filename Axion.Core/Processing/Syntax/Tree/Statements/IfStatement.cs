using System;
using System.CodeDom;
using Axion.Core.Processing.Syntax.Tree.Expressions;
using Newtonsoft.Json;

namespace Axion.Core.Processing.Syntax.Tree.Statements {
    public class IfStatement : Statement {
        private Expression condition;

        [JsonProperty]
        internal Expression Condition {
            get => condition;
            set {
                value.Parent = this;
                condition    = value;
            }
        }

        private BlockStatement thenBlock;

        [JsonProperty]
        internal BlockStatement ThenBlock {
            get => thenBlock;
            set {
                value.Parent = this;
                thenBlock    = value;
            }
        }

        private BlockStatement elseBlock;

        [JsonProperty]
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
            Expression     condition,
            BlockStatement thenBlock,
            BlockStatement elseBlock
        ) {
            Condition = condition ?? throw new ArgumentNullException(nameof(condition));
            ThenBlock = thenBlock ?? throw new ArgumentNullException(nameof(thenBlock));
            ElseBlock = elseBlock;
            MarkPosition(ThenBlock, ElseBlock ?? ThenBlock);
        }

        internal override CodeObject ToCSharp() {
            return new CodeConditionStatement(
                (CodeExpression) Condition.ToCSharp(),
                (CodeStatement[]) ThenBlock.ToCSharpArray(),
                (CodeStatement[]) ElseBlock.ToCSharpArray()
            );
        }
    }
}