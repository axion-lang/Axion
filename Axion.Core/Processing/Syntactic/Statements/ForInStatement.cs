using Axion.Core.Processing.CodeGen;
using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Processing.Syntactic.Expressions;
using JetBrains.Annotations;

namespace Axion.Core.Processing.Syntactic.Statements {
    public class ForInStatement : LoopStatement {
        private Expression item;

        [NotNull]
        public Expression Item {
            get => item;
            set => SetNode(ref item, value);
        }

        private Expression iterable;

        [NotNull]
        public Expression Iterable {
            get => iterable;
            set => SetNode(ref iterable, value);
        }

        public ForInStatement(
            Token                startToken,
            [NotNull] Expression item,
            [NotNull] Expression iterable,
            BlockStatement       block,
            BlockStatement       noBreakBlock
        ) : base(startToken, block, noBreakBlock) {
            Item     = item;
            Iterable = iterable;
        }

        internal ForInStatement(SyntaxTreeNode parent) {
            Parent = parent;
            StartNode(TokenType.KeywordFor);

            Item = Expression.MaybeTuple(
                Expression.TargetList(this, out bool trailingComma),
                trailingComma
            );
            if (MaybeEat(TokenType.OpIn)) {
                Iterable = Expression.SingleOrTuple(this);
                Block    = new BlockStatement(this, BlockType.Loop);
                if (MaybeEat(TokenType.KeywordElse)) {
                    NoBreakBlock = new BlockStatement(this);
                }
            }

            MarkEnd(Token);
        }

        internal override CodeBuilder ToAxionCode(CodeBuilder c) {
            c = c
                + "for "
                + Item
                + " in "
                + Iterable
                + " "
                + Block;
            if (NoBreakBlock != null) {
                c = c + " nobreak " + NoBreakBlock;
            }

            return c;
        }
    }
}