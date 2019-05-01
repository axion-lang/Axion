using Axion.Core.Processing.CodeGen;
using Axion.Core.Processing.Syntactic.Expressions;
using static Axion.Core.Specification.TokenType;

namespace Axion.Core.Processing.Syntactic.Statements {
    /// <summary>
    ///     <c>
    ///         for_in_stmt:
    ///             'for' simple_name_list 'in' preglobal_list
    ///             block
    ///             ['nobreak' block]
    ///     </c>
    /// </summary>
    public class ForInStatement : LoopStatement {
        private Expression item;

        public Expression Item {
            get => item;
            set => SetNode(ref item, value);
        }

        private Expression iterable;

        public Expression Iterable {
            get => iterable;
            set => SetNode(ref iterable, value);
        }

        /// <summary>
        ///     Constructs from tokens.
        /// </summary>
        internal ForInStatement(SyntaxTreeNode parent) : base(parent) {
            EatStartMark(KeywordFor);

            Item = Expression.ParseMultiple(
                parent,
                Expression.ParsePrimaryExpr,
                expectedTypes: typeof(SimpleNameExpression)
            );
            Eat(OpIn);
            Iterable = Expression.ParseMultiple(
                parent,
                Expression.ParsePreGlobalExpr
            );
            Block = new BlockStatement(this, BlockType.Loop);
            if (MaybeEat(KeywordNoBreak)) {
                NoBreakBlock = new BlockStatement(this);
            }

            MarkEnd(Token);
        }

        /// <summary>
        ///     Constructs without position in source.
        /// </summary>
        public ForInStatement(
            Expression     item,
            Expression     iterable,
            BlockStatement block,
            BlockStatement noBreakBlock
        ) : base(block, noBreakBlock) {
            Item     = item;
            Iterable = iterable;
        }

        internal override void ToAxionCode(CodeBuilder c) {
            c.Write(
                "for ",
                Item,
                " in ",
                Iterable,
                " ",
                Block
            );
            if (NoBreakBlock != null) {
                c.Write(" nobreak ", NoBreakBlock);
            }
        }

        internal override void ToCSharpCode(CodeBuilder c) {
            c.Write(
                "foreach (",
                Item,
                " in ",
                Iterable,
                ") ",
                Block
            );
            if (NoBreakBlock != null) {
                Unit.ReportError("C# doesn't support 'nobreak' block", NoBreakBlock);
            }
        }
    }
}