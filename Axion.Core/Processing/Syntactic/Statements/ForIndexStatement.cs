using Axion.Core.Processing.CodeGen;
using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Processing.Syntactic.Expressions;
using Axion.Core.Specification;

namespace Axion.Core.Processing.Syntactic.Statements {
    public class ForIndexStatement : LoopStatement {
        private Expression initStmt;

        public Expression InitStmt {
            get => initStmt;
            set => SetNode(ref initStmt, value);
        }

        private Expression condition;

        public Expression Condition {
            get => condition;
            set => SetNode(ref condition, value);
        }

        private Expression iterStmt;

        public Expression IterStmt {
            get => iterStmt;
            set => SetNode(ref iterStmt, value);
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

        internal ForIndexStatement(SyntaxTreeNode parent) {
            Parent = parent;
            StartNode(TokenType.KeywordFor);

            if (!MaybeEat(TokenType.Semicolon)) {
                Condition = Expression.ParseTestExpr(this);
                Eat(TokenType.Semicolon);
            }

            if (!MaybeEat(Spec.NeverTestTypes)) {
                IterStmt = Expression.ParseExpression(this);
            }

            Block = new BlockStatement(this, BlockType.Loop);
            if (MaybeEat(TokenType.KeywordNoBreak)) {
                NoBreakBlock = new BlockStatement(this);
            }

            MarkEnd(Token);
        }

        internal override CodeBuilder ToAxionCode(CodeBuilder c) {
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

        internal override CodeBuilder ToCSharpCode(CodeBuilder c) {
            c = c
                + "for("
                + InitStmt
                + ";"
                + Condition
                + ";"
                + IterStmt
                + ")"
                + Block;

            return c;
        }
    }
}