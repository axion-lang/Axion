using System;
using System.Collections.Generic;
using System.Diagnostics;
using Axion.Core.Processing.Errors;
using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Processing.Syntax.Tree.Expressions;
using Axion.Core.Processing.Syntax.Tree.Statements;
using Axion.Core.Processing.Syntax.Tree.Statements.Small;
using Axion.Core.Specification;
using static Axion.Core.Processing.Lexical.Tokens.TokenType;

namespace Axion.Core.Processing.Syntax.Parser {
    public partial class SyntaxParser {
        /// <summary>
        ///     <c>
        ///         stmt ::=
        ///             if_stmt   | while_stmt | for_stmt    |
        ///             try_stmt  | with_stmt  | import_stmt |
        ///             decorated | module_def | class_def   |
        ///             enum_def  | func_def   | small_stmt
        ///         small_stmt ::=
        ///             assert_stmt | delete_stmt | pass_stmt |
        ///             expr_stmt   | flow_stmt
        ///         flow_stmt ::=
        ///             break_stmt | continue_stmt | return_stmt |
        ///             raise_stmt | yield_stmt
        ///     </c>
        /// </summary>
        private Statement ParseStmt(bool onlyDecorated = false) {
            if (!onlyDecorated) {
                switch (stream.Peek.Type) {
                    case KeywordIf: {
                        return ParseIfStmt();
                    }
                    case KeywordWhile: {
                        return ParseWhileStmt();
                    }
                    case KeywordFor: {
                        return ParseForStmt();
                    }
                    case KeywordTry: {
                        return ParseTryStatement();
                    }
                    case KeywordWith: {
                        return ParseWithStmt();
                    }
                    //case KeywordUse: {
                    //    return ParseImportStmt();
                    //}

                    #region Small statements

                    // assert
                    case KeywordAssert: {
                        return ParseAssertStmt();
                    }
                    // pass
                    case KeywordPass: {
                        return new EmptyStatement(stream.NextToken());
                    }
                    // break
                    case KeywordBreak: {
                        return ParseBreakStmt();
                    }
                    // continue
                    case KeywordContinue: {
                        return ParseContinueStmt();
                    }
                    // delete
                    case KeywordDelete: {
                        return ParseDeleteStmt();
                    }
                    // raise
                    case KeywordRaise: {
                        return ParseRaiseStmt();
                    }
                    // return
                    case KeywordReturn: {
                        return ParseReturnStmt();
                    }
                    // yield
                    case KeywordYield: {
                        return ParseYieldStmt();
                    }

                    #endregion

                    case At: {
                        return ParseDecorated();
                    }
                    // there should be no 'default' case!
                }
            }

            switch (stream.Peek.Type) {
                case KeywordModule: {
                    return ParseModuleDef();
                }
                case KeywordClass: {
                    return ParseClassDef();
                }
                case KeywordEnum: {
                    return ParseEnumDef();
                }
                case KeywordFn: {
                    return ParseFunctionDef();
                }
                default: {
                    if (onlyDecorated) {
                        unit.ReportError("Invalid decorated statement", stream.Peek);
                        return ParseMultipleStmt();
                    }

                    break;
                }
            }

            return new ExpressionStatement(ParseExpression());
        }

        /// <summary>
        ///     <c>
        ///         multiple_stmt ::=
        ///             stmt {';' stmt} [';'] (terminator | NEWLINE)
        ///     </c>
        /// </summary>
        private Statement ParseMultipleStmt(TokenType terminator = None) {
            Statement firstStmt = ParseStmt();
            if (stream.MaybeEat(Semicolon)) {
                var statements = new List<Statement> {
                    firstStmt
                };

                while (stream.Token.Type == Semicolon
                       && !stream.MaybeEatNewline()
                       && !stream.PeekIs(terminator)) {
                    statements.Add(ParseStmt());
                    if (stream.MaybeEat(EndOfCode)) {
                        if (terminator != None) {
                            // force error here (Terminator expected)
                            stream.Eat(terminator);
                        }

                        // else EOC implies a new line
                        break;
                    }

                    if (!stream.MaybeEat(Semicolon)) {
                        stream.EatNewline();
                    }
                }

                return new BlockStatement(statements.ToArray());
            }

            return firstStmt;

        }

        #region If statement

        /// <summary>
        ///     <c>
        ///         if_stmt ::=
        ///             'if' test block
        ///             {'elif' test block}
        ///             ['else' block]
        ///     </c>
        /// </summary>
        private IfStatement ParseIfStmt(bool elseIf = false) {
            Position start =
                StartExprOrStmt(elseIf ? KeywordElseIf : KeywordIf).Span.StartPosition;
            Expression condition = ParseTestExpr();
            // if start block
            BlockStatement thenBlock = ParseFullBlock();
            BlockStatement elseBlock = null;

            if (stream.MaybeEat(KeywordElse)) {
                elseBlock = ParseFullBlock();
            }
            else if (stream.MaybeEat(KeywordElseIf)) {
                elseBlock = new BlockStatement(
                    new Statement[] {
                        ParseIfStmt(true)
                    }
                );
            }
            else {
                BlameInvalidSyntax(KeywordElse, stream.Peek);
            }

            var result = new IfStatement(condition, thenBlock, elseBlock);
            result.MarkPosition(start, tokenEnd);
            return result;
        }

        #endregion

        #region While statement

        /// <summary>
        ///     <c>
        ///         while_stmt ::=
        ///             'while' test block ['else' block]
        ///     </c>
        /// </summary>
        private WhileStatement ParseWhileStmt() {
            Token start = StartExprOrStmt(KeywordWhile);

            Expression     condition    = ParseTestExpr();
            BlockStatement block        = ParseLoopBlock();
            BlockStatement noBreakBlock = null;
            if (stream.MaybeEat(KeywordElse)) {
                noBreakBlock = ParseFullBlock();
            }

            return new WhileStatement(condition, block, noBreakBlock, start);
        }

        #endregion

        #region For statement

        /// <summary>
        ///     <c>
        ///         for_stmt ::=
        ///             'for'
        ///                 (exprlist 'in' testlist) |
        ///                 (expr_stmt ';' expr_stmt ';' expr_stmt)
        ///             block ['else' block]
        ///     </c>
        /// </summary>
        private ForStatement ParseForStmt() {
            Token start    = StartExprOrStmt(KeywordFor);
            int   startIdx = stream.Index;

            Expression initStmt = ParseExpression();
            if (stream.MaybeEat(Semicolon)) {
                Expression condition = ParseTestExpr();
                stream.Eat(Semicolon);
                Expression     iterStmt  = ParseExpression();
                BlockStatement block     = ParseLoopBlock();
                BlockStatement elseBlock = null;
                if (stream.MaybeEat(KeywordElse)) {
                    elseBlock = ParseFullBlock();
                }

                return new ForIndexStatement(
                    initStmt,
                    condition,
                    iterStmt,
                    block,
                    elseBlock,
                    start
                );
            }

            stream.MoveTo(startIdx);

            Expression expressions = MakeTupleOrExpr(
                ParseTargetList(out bool trailingComma),
                trailingComma
            );
            if (stream.MaybeEat(KeywordIn)) {
                Expression     inList    = ParseTestList();
                BlockStatement block     = ParseLoopBlock();
                BlockStatement elseBlock = null;
                if (stream.MaybeEat(KeywordElse)) {
                    elseBlock = ParseFullBlock();
                }

                return new ForInStatement(
                    expressions,
                    inList,
                    block,
                    elseBlock,
                    start
                );
            }

            unit.ReportError(
                "Expected 'for x in y' or 'for init_statement, condition, update_statement'.",
                start
            );
            return null;
        }

        #endregion

        #region Try statement

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
        private Statement ParseTryStatement() {
            Token start = StartExprOrStmt(KeywordTry);

            // try
            BlockStatement block        = ParseFullBlock();
            BlockStatement finallyBlock = null;

            // anyway
            if (stream.MaybeEat(KeywordAnyway)) {
                finallyBlock = ParseFinallyBlock();
                return new TryStatement(block, null, null, finallyBlock, start);
            }

            // catch
            var                 handlers       = new List<TryStatementHandler>();
            TryStatementHandler defaultHandler = null;
            do {
                TryStatementHandler handler = ParseTryStmtHandler();
                handlers.Add(handler);

                if (defaultHandler != null) {
                    unit.Blame(BlameType.DefaultCatchMustBeLast, defaultHandler);
                }

                if (handler.ErrorType == null) {
                    defaultHandler = handler;
                }
            } while (stream.PeekIs(KeywordCatch));

            // else
            BlockStatement elseBlock = null;
            if (stream.MaybeEat(KeywordElse)) {
                elseBlock = ParseFullBlock();
            }

            // anyway
            if (stream.MaybeEat(KeywordAnyway)) {
                // If this function has an except block, then it can set the current exception.
                finallyBlock = ParseFinallyBlock();
            }

            return new TryStatement(block, handlers.ToArray(), elseBlock, finallyBlock, start);
        }

        /// <summary>
        ///     <c>
        ///         try_handler ::=
        ///             'catch' [expr ['as' name]]
        ///     </c>
        /// </summary>
        private TryStatementHandler ParseTryStmtHandler() {
            stream.Eat(KeywordCatch);

            // If this function has an except block,
            // then it can set the current exception.
            if (currentFunction != null) {
                currentFunction.CanSetSysExcInfo = true;
            }

            Position   start     = tokenStart;
            Expression errorType = null, name = null;
            if (!stream.PeekIs(Spec.BlockStarters)) {
                errorType = ParseTestExpr();
                if (stream.MaybeEat(KeywordAs)) {
                    name = ParseName();
                }
            }

            BlockStatement block = ParseFullBlock();
            return new TryStatementHandler(errorType, name, block, start);
        }

        private BlockStatement ParseFinallyBlock() {
            if (currentFunction != null) {
                currentFunction.ContainsTryFinally = true;
            }

            BlockStatement finallyBlock;
            bool           isInFinally = inFinally, isInFinallyLoop = inFinallyLoop;
            try {
                inFinally     = true;
                inFinallyLoop = false;
                finallyBlock  = ParseFullBlock();
            }
            finally {
                inFinally     = isInFinally;
                inFinallyLoop = isInFinallyLoop;
            }

            return finallyBlock;
        }

        #endregion

        #region With statement

        /// <summary>
        ///     <c>
        ///         with_stmt ::=
        ///             'with' with_item {',' with_item} block
        ///         with_item ::=
        ///             test ['as' name]
        ///     </c>
        /// </summary>
        private WithStatement ParseWithStmt() {
            Token start = StartExprOrStmt(KeywordWith);
            // TODO: add 'with' expression like in Kotlin
            var items = new List<WithStatementItem>();
            do {
                Position   itemStart      = tokenStart;
                Expression contextManager = ParseTestExpr();
                Expression name           = null;
                if (stream.MaybeEat(KeywordAs)) {
                    name = ParseName();
                }

                items.Add(new WithStatementItem(itemStart, contextManager, name));
            } while (stream.MaybeEat(Comma));

            Statement block = ParseFullBlock();
            if (items.Count > 1) {
                for (int i = items.Count - 1; i > 0; i--) {
                    block = new WithStatement(items[i], block, start);
                }
            }

            return new WithStatement(items[0], block, start);
        }

        #endregion

        #region Import statement

//        /// <summary>
//        ///     import_stmt ::=
//        ///         'use' module ['as' name]
//        /// </summary>
//        private ImportStatement ParseImportStmt() {
//            stream.Eat(TokenType.KeywordUse);
//            Position start = tokenStart;
//
//            return new ImportStatement();
//        }

        #endregion

        #region Decorators

        /// <summary>
        ///     <c>
        ///         decorators:
        ///             ('@' decorator {',' decorator} NEWLINE)+
        ///     </c>
        /// </summary>
        private List<Expression> ParseDecorators() {
            var decorators = new List<Expression>();

            while (stream.MaybeEat(At)) {
                try {
                    stream.EnsureOneLine(
                        () => {
                            do {
                                decorators.Add(ParseDecorator());
                            } while (stream.MaybeEat(Comma));

                            stream.EatNewline();
                        }
                    );
                }
                catch (NotSupportedException) {
                    unit.ReportError("Decorators list must be placed on one line.", stream.Token);
                }
            }

            return decorators;
        }

        /// <summary>
        ///     <c>
        ///         decorator:
        ///             name ['(' [argument_list [',']] ')']
        ///     </c>
        /// </summary>
        private Expression ParseDecorator() {
            // on '@'
            Position   start     = tokenStart;
            Expression decorator = ParseExpression();
            if (stream.MaybeEat(LeftParenthesis)) {
                Arg[] args = FinishGeneratorOrArgList();
                decorator = new CallExpression(decorator, args, tokenEnd);
            }

            decorator.MarkPosition(start, tokenEnd);
            return decorator;
        }

        #endregion

        #region Small statements

        /// <summary>
        ///     <c>
        ///         assert_stmt ::=
        ///             'assert' test [',' test]
        ///     </c>
        /// </summary>
        private AssertStatement ParseAssertStmt() {
            Token start = StartExprOrStmt(KeywordAssert);

            Expression condition       = ParseTestExpr();
            Expression falseExpression = null;
            if (stream.MaybeEat(Comma)) {
                falseExpression = ParseTestExpr();
            }

            return new AssertStatement(condition, falseExpression, start);
        }

        /// <summary>
        ///     <c>
        ///         break_stmt ::=
        ///             'break' [name]
        ///     </c>
        /// </summary>
        private Statement ParseBreakStmt() {
            // TODO: add ['break' loop_name]
            stream.Eat(KeywordBreak);
            if (!inLoop) {
                unit.Blame(BlameType.BreakIsOutsideLoop, stream.Token);
            }

            return new BreakStatement(stream.Token);
        }

        /// <summary>
        ///     <c>
        ///         continue_stmt ::=
        ///             'continue' [name]
        ///     </c>
        /// </summary>
        private Statement ParseContinueStmt() {
            // TODO: add ['continue' loop_name]
            stream.Eat(KeywordContinue);
            if (!inLoop) {
                unit.Blame(BlameType.ContinueIsOutsideLoop, stream.Token);
            }
            else if (inFinally && !inFinallyLoop) {
                unit.Blame(BlameType.ContinueNotSupportedInsideFinally, stream.Token);
            }

            return new ContinueStatement(stream.Token);
        }

        /// <summary>
        ///     <c>
        ///         delete_stmt ::=
        ///             'delete' expr_list
        ///     </c>
        ///     <para />
        ///     For error reporting reasons we allow any
        ///     expr and then report the bad delete node when it fails.
        ///     This is the reason we don't call ParseTargetList.
        /// </summary>
        private Statement ParseDeleteStmt() {
            Token start = StartExprOrStmt(KeywordDelete);

            List<Expression> expressions = ParseTestList(out bool _);
            foreach (Expression expr in expressions) {
                if (expr.CannotDeleteReason != null) {
                    unit.Blame(BlameType.InvalidExpressionToDelete, expr);
                }
            }

            return new DeleteStatement(expressions.ToArray(), start);
        }

        /// <summary>
        ///     <c>
        ///         raise_stmt ::=
        ///             'raise' [test ['from' test]]
        ///     </c>
        /// </summary>
        private RaiseStatement ParseRaiseStmt() {
            Token start = StartExprOrStmt(KeywordRaise);

            Expression expr = null, cause = null;
            if (!stream.PeekIs(Spec.NeverTestTypes)) {
                expr = ParseTestExpr();
                if (stream.MaybeEat(KeywordFrom)) {
                    cause = ParseTestExpr();
                }
            }

            return new RaiseStatement(expr, cause, start);
        }

        /// <summary>
        ///     <c>
        ///         return_stmt ::=
        ///             'return' [test_list]
        ///     </c>
        /// </summary>
        private Statement ParseReturnStmt() {
            Token start = StartExprOrStmt(KeywordReturn);

            if (currentFunction == null) {
                unit.Blame(BlameType.MisplacedReturn, stream.Token);
            }

            Expression expr = null;
            if (!stream.PeekIs(Spec.NeverTestTypes)) {
                expr = ParseTestList();
            }

            return new ReturnStatement(expr, start);
        }

        /// <summary>
        ///     <c>
        ///         yield_stmt ::=
        ///             yield_expr
        ///     </c>
        /// </summary>
        private Statement ParseYieldStmt() {
            // For yield statements, continue to enforce that it's currently in a function. 
            // This gives us better syntax error reporting for yield-statements than for yield-expressions.
            if (currentFunction == null) {
                unit.Blame(BlameType.MisplacedYield, stream.Token);
            }

            Expression yieldExpr = ParseYield();
            Debug.Assert(yieldExpr != null); // caller already verified we have a yield.

            return new ExpressionStatement(yieldExpr);
        }

        #endregion

        #region Block statement

        private BlockStatement ParseTopLevelBlock() {
            BlockStatement block;
            bool isInLoop        = inLoop,
                 isInFinally     = inFinally,
                 isInFinallyLoop = inFinallyLoop;
            try {
                inLoop        = false;
                inFinally     = false;
                inFinallyLoop = false;
                block         = ParseFullBlock();
            }
            finally {
                inLoop        = isInLoop;
                inFinally     = isInFinally;
                inFinallyLoop = isInFinallyLoop;
            }

            return block;
        }

        private BlockStatement ParseLoopBlock() {
            BlockStatement block;
            bool           wasInLoop = inLoop, wasInFinallyLoop = inFinallyLoop;
            try {
                inLoop        = true;
                inFinallyLoop = inFinally;
                block         = ParseFullBlock();
            }
            finally {
                inLoop        = wasInLoop;
                inFinallyLoop = wasInFinallyLoop;
            }

            return block;
        }

        /// <summary>
        ///     Starts parsing the statement's block,
        ///     returns terminator what can be used to parse block end.
        /// </summary>
        private (TokenType terminator, bool oneLine, bool error) ParseBlockStart(
            bool usesColon = true
        ) {
            // 1) colon
            bool  hasColon   = stream.MaybeEat(Colon);
            Token blockStart = stream.Token;

            // 1-2) newline
            bool hasNewline;
            if (hasColon) {
                // ':' NL
                hasNewline = stream.MaybeEatNewline();
            }
            else {
                // NL
                hasNewline = blockStart.Type == Newline;
            }

            TokenType terminator;
            // 1-3) '{'
            if (stream.MaybeEat(LeftBrace)) {
                terminator = RightBrace;
            }
            // 3) INDENT
            else if (stream.MaybeEat(Indent)) {
                terminator = Outdent;
            }
            else {
                // no indent or brace? - report block error
                if (hasNewline) {
                    // newline but with invalid follower
                    unit.Blame(BlameType.ExpectedBlockDeclaration, stream.Peek);
                    return (None, false, true);
                }

                // no newline
                if (!hasColon && usesColon) {
                    // must have a colon
                    BlameInvalidSyntax(Colon, stream.Peek);
                    return (None, true, true);
                }

                return (Newline, true, false);
            }

            // ':' followed by '{'
            if (hasColon && terminator == RightBrace) {
                unit.Blame(BlameType.RedundantColonWithBraces, blockStart);
            }

            stream.MaybeEatNewline();
            return (terminator, false, false);
        }

        /// <summary>
        ///     <c>
        ///         block ::=
        ///             (':' simple_stmt) |
        ///             ([':'] '{' stmt+ '}') |
        ///             (':' NEWLINE INDENT stmt+ OUTDENT)
        ///     </c>
        /// </summary>
        private BlockStatement ParseFullBlock(bool usesColon = true) {
            var statements = new List<Statement>();
            (TokenType terminator, bool oneLine, bool error) = ParseBlockStart(usesColon);

            if (oneLine) {
                statements.Add(ParseMultipleStmt());
            }
            else if (!error
                     && !stream.MaybeEat(terminator)) {
                while (true) {
                    statements.Add(ParseMultipleStmt(terminator));
                    if (stream.MaybeEat(terminator)) {
                        break;
                    }

                    if (CheckUnexpectedEoc()) {
                        break;
                    }
                }
            }

            // wrap last expr in return
            // to allow user to write no 'return' keyword
//            if (statements.Count > 0) {
//                if (statements[statements.Count - 1] is ExpressionStatement exprStmt) {
//                    statements[statements.Count - 1] = new ReturnStatement(exprStmt.Expression);
//                }
//            }

            return new BlockStatement(statements.ToArray());
        }

        #endregion
    }
}