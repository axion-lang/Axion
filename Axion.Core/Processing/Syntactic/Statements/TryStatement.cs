using System.Collections.Generic;
using Axion.Core.Processing.CodeGen;
using Axion.Core.Processing.Errors;
using Axion.Core.Processing.Syntactic.Expressions;
using Axion.Core.Processing.Syntactic.Expressions.TypeNames;
using Axion.Core.Specification;
using static Axion.Core.Specification.TokenType;

namespace Axion.Core.Processing.Syntactic.Statements {
    /// <summary>
    ///     <c>
    ///         try_stmt:
    ///             'try' block
    ///             ('anyway' block)
    ///           | (try_handler+
    ///             ['else' block]
    ///             ['anyway' block])
    ///     </c>
    /// </summary>
    public class TryStatement : Statement {
        private BlockStatement block;

        public BlockStatement Block {
            get => block;
            set => SetNode(ref block, value);
        }

        private NodeList<TryHandlerStatement> handlers;

        public NodeList<TryHandlerStatement> Handlers {
            get => handlers;
            set => SetNode(ref handlers, value);
        }

        private BlockStatement elseBlock;

        public BlockStatement ElseBlock {
            get => elseBlock;
            set => SetNode(ref elseBlock, value);
        }

        private BlockStatement anywayBlock;

        public BlockStatement AnywayBlock {
            get => anywayBlock;
            set => SetNode(ref anywayBlock, value);
        }

        /// <summary>
        ///     Constructs from tokens.
        /// </summary>
        internal TryStatement(SyntaxTreeNode parent) : base(parent) {
            EatStartMark(KeywordTry);

            // try
            Block = new BlockStatement(this);

            // anyway
            if (MaybeEat(KeywordAnyway)) {
                AnywayBlock = new BlockStatement(this, BlockType.Anyway);
                MarkEnd(Token);
                return;
            }

            // catch
            Handlers = new NodeList<TryHandlerStatement>(this);
            TryHandlerStatement defaultHandler = null;
            do {
                var handler = new TryHandlerStatement(this);
                Handlers.Add(handler);

                if (defaultHandler != null) {
                    Unit.Blame(BlameType.DefaultCatchMustBeLast, defaultHandler);
                }

                if (handler.ErrorType == null) {
                    defaultHandler = handler;
                }
            } while (Peek.Is(KeywordCatch));

            // else
            if (MaybeEat(KeywordElse)) {
                ElseBlock = new BlockStatement(this);
            }

            // anyway
            if (MaybeEat(KeywordAnyway)) {
                AnywayBlock = new BlockStatement(this, BlockType.Anyway);
            }

            MarkEnd(Token);
        }

        /// <summary>
        ///     Constructs without position in source.
        /// </summary>
        public TryStatement(
            BlockStatement                   block,
            IEnumerable<TryHandlerStatement> handlers,
            BlockStatement                   elseBlock,
            BlockStatement                   anywayBlock
        ) {
            Block       = block;
            Handlers    = new NodeList<TryHandlerStatement>(this, handlers);
            ElseBlock   = elseBlock;
            AnywayBlock = anywayBlock;
        }

        internal override void ToAxionCode(CodeBuilder c) {
            c.Write("try ", Block);
            if (Handlers.Count > 0) {
                c.AddJoin("", Handlers);
            }

            if (AnywayBlock != null) {
                c.Write("else ", ElseBlock);
            }

            if (AnywayBlock != null) {
                c.Write("anyway ", AnywayBlock);
            }
        }

        internal override void ToCSharpCode(CodeBuilder c) {
            c.Write("try ", Block);
            if (Handlers.Count > 0) {
                c.AddJoin("", Handlers);
            }

            if (AnywayBlock != null) {
                c.Write("finally ", AnywayBlock);
            }
        }
    }

    /// <summary>
    ///     <c>
    ///         try_handler:
    ///             'catch' [type ['as' name]] ['when' preglobal_expr]
    ///     </c>
    /// </summary>
    public class TryHandlerStatement : Statement {
        private TypeName? errorType;

        public TypeName? ErrorType {
            get => errorType;
            set => SetNode(ref errorType, value);
        }

        private Expression? errorName;

        public Expression? ErrorName {
            get => errorName;
            set => SetNode(ref errorName, value);
        }

        private Expression? condition;

        public Expression? Condition {
            get => condition;
            set => SetNode(ref condition, value);
        }

        private BlockStatement block;

        public BlockStatement Block {
            get => block;
            set => SetNode(ref block, value);
        }

        /// <summary>
        ///     Constructs from tokens.
        /// </summary>
        internal TryHandlerStatement(SyntaxTreeNode parent) : base(parent) {
            EatStartMark(KeywordCatch);

            // If this function has an except block,
            // then it can set the current exception.
            if (Ast.CurrentFunction != null) { }

            if (!Peek.Is(Spec.BlockStarters)) {
                ErrorType = TypeName.ParseTypeName(this);
                if (MaybeEat(OpAs)) {
                    ErrorName = new SimpleNameExpression(this);
                }
            }

            if (MaybeEat(KeywordWhen)) {
                Condition = Expression.ParsePreGlobalExpr(this);
            }

            Block = new BlockStatement(this);

            MarkEnd(Token);
        }

        /// <summary>
        ///     Constructs without position in source.
        /// </summary>
        public TryHandlerStatement(
            TypeName?      errorType,
            Expression?    errorName,
            Expression?    condition,
            BlockStatement block
        ) {
            ErrorType = errorType;
            ErrorName = errorName;
            Condition = condition;
            Block     = block;
        }

        internal override void ToAxionCode(CodeBuilder c) {
            c.Write("catch");
            if (ErrorType != null) {
                c.Write(" ", ErrorType);
            }

            if (ErrorName != null) {
                c.Write(" as ", ErrorName);
            }

            if (Condition != null) {
                c.Write(" when ", Condition);
            }

            c.Write(Block);
        }

        internal override void ToCSharpCode(CodeBuilder c) {
            c.Write("catch (");

            if (ErrorName != null) {
                c.Write(ErrorName, " ");
            }

            if (ErrorType != null) {
                c.Write(ErrorType);
            }

            c.Write(")");

            if (Condition != null) {
                c.Write(" when (", Condition, ")");
            }

            c.Write(Block);
        }
    }
}