using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Processing.Syntax.Tree.Expressions;

namespace Axion.Core.Processing.Syntax.Tree.Statements {
    public class ForIndexStatement : LoopStatement {
        private Expression initStmt;

        public Expression InitStmt {
            get => initStmt;
            set {
                if (value != null) {
                    value.Parent = this;
                }

                initStmt = value;
            }
        }

        private Expression condition;

        public Expression Condition {
            get => condition;
            set {
                if (value != null) {
                    value.Parent = this;
                }

                condition = value;
            }
        }

        private Expression iterStmt;

        public Expression IterStmt {
            get => iterStmt;
            set {
                if (value != null) {
                    value.Parent = this;
                }

                iterStmt = value;
            }
        }

        public ForIndexStatement(
            Token          startToken,
            Expression     initStmt,
            Expression     condition,
            Expression     iterStmt,
            BlockStatement block,
            BlockStatement noBreakBlock
        ) : base(startToken, block, noBreakBlock) {
            InitStmt  = initStmt;
            Condition = condition;
            IterStmt  = iterStmt;
        }

        internal override AxionCodeBuilder ToAxionCode(AxionCodeBuilder c) {
            c = c
                + "for "
                + InitStmt
                + "; "
                + Condition
                + "; "
                + IterStmt
                + " "
                + Block;
            if (NoBreakBlock != null) {
                c = c + " nobreak " + NoBreakBlock;
            }

            return c;
        }
    }
}