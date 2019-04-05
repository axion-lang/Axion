using System.Collections.Generic;
using Axion.Core.Processing.CodeGen;
using Axion.Core.Processing.Errors;
using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Processing.Syntactic.Expressions;
using Axion.Core.Processing.Syntactic.Expressions.TypeNames;
using Axion.Core.Specification;
using JetBrains.Annotations;

namespace Axion.Core.Processing.Syntactic.Statements {
    /// <summary>
    ///     <c>
    ///         try_stmt ::=
    ///             ('try' block
    ///             ((try_handler block)+
    ///             ['else' block]
    ///             ['finally' block] |
    ///             'finally' block))
    ///     </c>
    /// </summary>
    public class TryStatement : Statement {
        private BlockStatement block;

        [NotNull]
        public BlockStatement Block {
            get => block;
            set => SetNode(ref block, value);
        }

        private NodeList<TryStatementHandler> handlers;

        [NotNull]
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

        public TryStatement(
            [NotNull] BlockStatement                   block,
            [NotNull] IEnumerable<TryStatementHandler> handlers,
            BlockStatement                             elseBlock,
            BlockStatement                             anywayBlock
        ) {
            Block    = block;
            Handlers = new NodeList<TryStatementHandler>(this, handlers);

            ElseBlock   = elseBlock;
            AnywayBlock = anywayBlock;

            MarkEnd(
                AnywayBlock
                ?? ElseBlock
                ?? (Handlers.Count > 0
                    ? Handlers[Handlers.Count - 1]
                    : (SyntaxTreeNode) block)
            );
        }

        internal TryStatement(SyntaxTreeNode parent) {
            Parent = parent;
            StartNode(TokenType.KeywordTry);

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

        internal override CodeBuilder ToAxionCode(CodeBuilder c) {
            c = c + "try " + Block;
            if (Handlers.Count > 0) {
                c.AppendJoin(" ", Handlers);
            }

            if (AnywayBlock != null) {
                c = c + " anyway " + AnywayBlock;
            }

            return c;
        }
    }

    /// <summary>
    ///     <c>
    ///         try_handler ::=
    ///             'catch' [expr ['as' name]] ['when' test]
    ///     </c>
    /// </summary>
    public class TryStatementHandler : Statement {
        private TypeName errorType;

        public TypeName ErrorType {
            get => errorType;
            set => SetNode(ref errorType, value);
        }

        private Expression errorName;

        public Expression ErrorName {
            get => errorName;
            set => SetNode(ref errorName, value);
        }

        private Expression condition;

        public Expression Condition {
            get => condition;
            set => SetNode(ref condition, value);
        }

        private BlockStatement block;

        [NotNull]
        public BlockStatement Block {
            get => block;
            set => SetNode(ref block, value);
        }

        public TryStatementHandler(
            TypeName                 errorType,
            Expression               errorName,
            Expression               condition,
            [NotNull] BlockStatement block
        ) {
            ErrorType = errorType;
            ErrorName = errorName;
            Condition = condition;
            Block     = block;

            MarkEnd(Block);
        }

        internal TryStatementHandler(SyntaxTreeNode parent) {
            Parent = parent;
            StartNode(TokenType.KeywordCatch);

            // If this function has an except block,
            // then it can set the current exception.
            if (Ast.CurrentFunction != null) {
                Ast.CurrentFunction.CanSetSysExcInfo = true;
            }

            if (!PeekIs(Spec.BlockStarters)) {
                ErrorType = TypeName.Parse(this);
                if (MaybeEat(TokenType.OpAs)) {
                    ErrorName = new NameExpression(this, true);
                }
            }

            if (MaybeEat(TokenType.KeywordWhen)) {
                Condition = Expression.ParseTestExpr(this);
            }

            Block = new BlockStatement(this);

            MarkEnd(Token);
        }

        internal override CodeBuilder ToAxionCode(CodeBuilder c) {
            c += "catch";
            if (ErrorType != null) {
                c = c + " " + ErrorType;
            }

            if (ErrorName != null) {
                c = c + " as " + ErrorName;
            }

            if (Condition != null) {
                c = c + " when " + Condition;
            }

            return c + Block;
        }
    }
}