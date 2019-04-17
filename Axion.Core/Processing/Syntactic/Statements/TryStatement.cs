using System.Collections.Generic;
using Axion.Core.Processing.CodeGen;
using Axion.Core.Processing.Errors;
using Axion.Core.Processing.Syntactic.Expressions;
using Axion.Core.Processing.Syntactic.Expressions.TypeNames;
using Axion.Core.Specification;

namespace Axion.Core.Processing.Syntactic.Statements {
    /// <summary>
    ///     <c>
    ///         try_stmt:
    ///             ('try' block
    ///             ((try_handler block)+
    ///             ['else' block]
    ///             ['finally' block] |
    ///             'finally' block))
    ///     </c>
    /// </summary>
    public class TryStatement : Statement {
        #region Properties

        private BlockStatement block;

        public BlockStatement Block {
            get => block;
            set => SetNode(ref block, value);
        }

        private NodeList<TryStatementHandler> handlers;

        public NodeList<TryStatementHandler> Handlers {
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

        #endregion

        #region Constructors

        /// <summary>
        ///     Constructs new <see cref="TryStatement"/> from tokens.
        /// </summary>
        internal TryStatement(SyntaxTreeNode parent) : base(parent) {
            MarkStart(TokenType.KeywordTry);

            // try
            Block = new BlockStatement(this);

            // anyway
            if (MaybeEat(TokenType.KeywordAnyway)) {
                AnywayBlock = new BlockStatement(this, BlockType.Anyway);
                MarkEnd(Token);
                return;
            }

            // catch
            TryStatementHandler defaultHandler = null;
            do {
                var handler = new TryStatementHandler(this);
                Handlers.Add(handler);

                if (defaultHandler != null) {
                    Unit.Blame(BlameType.DefaultCatchMustBeLast, defaultHandler);
                }

                if (handler.ErrorType == null) {
                    defaultHandler = handler;
                }
            } while (PeekIs(TokenType.KeywordCatch));

            // else
            if (MaybeEat(TokenType.KeywordElse)) {
                ElseBlock = new BlockStatement(this);
            }

            // anyway
            if (MaybeEat(TokenType.KeywordAnyway)) {
                AnywayBlock = new BlockStatement(this, BlockType.Anyway);
            }

            MarkEnd(Token);
        }

        /// <summary>
        ///     Constructs plain <see cref="TryStatement"/> without position in source.
        /// </summary>
        public TryStatement(
            BlockStatement                   block,
            IEnumerable<TryStatementHandler> handlers,
            BlockStatement                   elseBlock,
            BlockStatement                   anywayBlock
        ) {
            Block       = block;
            Handlers    = new NodeList<TryStatementHandler>(this, handlers);
            ElseBlock   = elseBlock;
            AnywayBlock = anywayBlock;
        }

        #endregion

        #region Code converters

        internal override void ToAxionCode(CodeBuilder c) {
            c.Write("try ", Block);
            if (Handlers.Count > 0) {
                c.AddJoin(" ", Handlers);
            }

            if (AnywayBlock != null) {
                c.Write(" anyway ", AnywayBlock);
            }
        }

        internal override void ToCSharpCode(CodeBuilder c) {
            c.Write("try ", Block);
            if (Handlers.Count > 0) {
                c.AddJoin(" ", Handlers);
            }

            if (AnywayBlock != null) {
                c.Write(" finally ", AnywayBlock);
            }
        }

        #endregion
    }

    /// <summary>
    ///     <c>
    ///         try_handler:
    ///             'catch' [expr ['as' name]] ['when' test]
    ///     </c>
    /// </summary>
    public class TryStatementHandler : Statement {
        #region Properties

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

        #endregion

        #region Constructors

        /// <summary>
        ///     Constructs new <see cref="TryStatementHandler"/> from tokens.
        /// </summary>
        internal TryStatementHandler(SyntaxTreeNode parent) : base(parent) {
            MarkStart(TokenType.KeywordCatch);

            // If this function has an except block,
            // then it can set the current exception.
            if (Ast.CurrentFunction != null) { }

            if (!PeekIs(Spec.BlockStarters)) {
                ErrorType = TypeName.ParseTypeName(this);
                if (MaybeEat(TokenType.KeywordAs)) {
                    ErrorName = new NameExpression(this, true);
                }
            }

            if (MaybeEat(TokenType.KeywordWhen)) {
                Condition = Expression.ParseTestExpr(this);
            }

            Block = new BlockStatement(this);

            MarkEnd(Token);
        }

        /// <summary>
        ///     Constructs plain <see cref="TryStatementHandler"/> without position in source.
        /// </summary>
        public TryStatementHandler(
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

        #endregion

        #region Code converters

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

        #endregion
    }
}