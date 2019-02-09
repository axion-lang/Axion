using System;
using System.Collections.Generic;
using System.Diagnostics;
using Axion.Core.Processing.Errors;
using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Processing.Syntax.Tree.Expressions;
using Axion.Core.Processing.Syntax.Tree.Statements;
using Axion.Core.Specification;
using static Axion.Core.Processing.Lexical.Tokens.TokenType;

namespace Axion.Core.Processing.Syntax.Parser {
    public partial class SyntaxParser {
        /// <summary>
        ///     <c>
        ///         stmt:
        ///         simple_stmt | compound_stmt
        ///         compound_stmt:
        ///         if_stmt   | while_stmt | for_stmt |
        ///         try_stmt  | with_stmt  | use_stmt |
        ///         decorated | class_def  | enum_def |
        ///         func_def  | small_stmt
        ///         small_stmt:
        ///         assert_stmt | delete_stmt |
        ///         pass_stmt   | expr_stmt   |
        ///         flow_stmt
        ///         flow_stmt:
        ///         break_stmt  | continue_stmt |
        ///         return_stmt | raise_stmt    | yield_stmt
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
                    // delete
                    case KeywordDelete: {
                        return ParseDeleteStmt();
                    }
                    // pass
                    case KeywordPass: {
                        stream.NextToken();
                        return new EmptyStatement(stream.Token);
                    }
                    // break
                    case KeywordBreak: {
                        return ParseBreakStmt();
                    }
                    // continue
                    case KeywordContinue: {
                        return ParseContinueStmt();
                    }
                    // return
                    case KeywordReturn: {
                        return ParseReturnStmt();
                    }
                    // raise
                    case KeywordRaise: {
                        return ParseRaiseStmt();
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
                        ReportError("Invalid decorated statement", stream.Peek);
                        return ParseMultipleStmt();
                    }
                    break;
                }
            }
            return new ExpressionStatement(ParseExpression());
        }

        #region While statement

        /// <summary>
        ///     <c>
        ///         while_stmt:
        ///         'while' test block ['else' block]
        ///     </c>
        /// </summary>
        private WhileStatement ParseWhileStmt() {
            Token start = StartExprOrStmt(KeywordWhile);

            Expression condition    = ParseTestExpr();
            Statement  block        = ParseLoopBlock();
            Statement  noBreakBlock = null;
            if (stream.MaybeEat(KeywordElse)) {
                noBreakBlock = ParseFullBlock();
            }
            return new WhileStatement(condition, block, noBreakBlock, start);
        }

        #endregion

        #region For statement

        /// <summary>
        ///     <c>
        ///         for_stmt:
        ///         'for' (exprlist 'in' testlist |
        ///         expr_stmt, expr_stmt, expr_stmt)
        ///         block ['else' block]
        ///     </c>
        /// </summary>
        private ForStatement ParseForStmt() {
            // TODO: add 'for' expr, expr, expr
            Token start = StartExprOrStmt(KeywordFor);

            List<Expression> l = ParseTargetList(out bool trailingComma);

            // expr list is something like:
            // ()
            // a
            // a,b
            // a,b,c
            // we either want just () or a or we want (a,b) and (a,b,c)
            // so we can do tupleExpr.EmitSet() or loneExpr.EmitSet()

            Expression lhs = MakeTupleOrExpr(l, trailingComma);
            stream.Eat(KeywordIn);
            Expression list  = ParseTestList();
            Statement  block = ParseLoopBlock();
            Statement  @else = null;
            if (stream.MaybeEat(KeywordElse)) {
                @else = ParseFullBlock();
            }
            return new ForStatement(lhs, list, block, @else, start);
        }

        #endregion

        /// <summary>
        ///     <c>
        ///         multiple_stmt:
        ///         multiple_stmt: stmt (';' stmt)* [';'] (terminator | NEWLINE)
        ///     </c>
        /// </summary>
        private Statement ParseMultipleStmt(TokenType terminator = None) {
            Statement statement = ParseStmt();
            if (stream.MaybeEat(Semicolon)) {
                var statements = new List<Statement> { statement };
                while (stream.Token.Type == Semicolon && !stream.MaybeEatNewline() && !stream.PeekIs(terminator)) {
                    statements.Add(ParseStmt());
                    if (stream.MaybeEat(EndOfCode)) {
                        if (terminator != None) {
                            // force error here (No terminator)
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
            return statement;
        }

        #region If statement

        /// <summary>
        ///     <c>
        ///         if_stmt:
        ///         'if' if_stmt_branch
        ///         ('elif' if_stmt_branch)*
        ///         ['else' block]
        ///     </c>
        /// </summary>
        private IfStatement ParseIfStmt() {
            StartExprOrStmt(KeywordIf);

            // if start block
            var branches = new List<IfStatementBranch> { ParseIfStmtBranch() };

            // else ifs
            while (stream.MaybeEat(KeywordElseIf)) {
                branches.Add(ParseIfStmtBranch());
            }

            // else
            Statement elseBranch = null;
            if (stream.MaybeEat(KeywordElse)) {
                elseBranch = ParseFullBlock();
            }

            return new IfStatement(branches, elseBranch);
        }

        /// <summary>
        ///     <c>
        ///         if_stmt_branch:
        ///         test block
        ///     </c>
        /// </summary>
        private IfStatementBranch ParseIfStmtBranch() {
            Token      start = stream.Token;
            Expression expr  = ParseTestExpr();
            Statement  block = ParseFullBlock();
            return new IfStatementBranch(expr, block, start);
        }

        #endregion

        #region Try statement

        /// <summary>
        ///     <c>
        ///         try_stmt:
        ///         ('try' block
        ///         ((try_handler block)+
        ///         ['else' block]
        ///         ['finally' block] |
        ///         'finally' block))
        ///     </c>
        /// </summary>
        private Statement ParseTryStatement() {
            Token start = StartExprOrStmt(KeywordTry);

            // try
            Statement block        = ParseFullBlock();
            Statement finallyBlock = null;

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
                    Blame(BlameType.DefaultCatchMustBeLast, defaultHandler);
                }
                if (handler.ErrorType == null) {
                    defaultHandler = handler;
                }
            } while (stream.PeekIs(KeywordCatch));
            // else
            Statement elseBlock = null;
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
        ///         try_handler:
        ///         'catch' [expr ['as' ID]]
        ///     </c>
        /// </summary>
        private TryStatementHandler ParseTryStmtHandler() {
            stream.Eat(KeywordCatch);

            // If this function has an except block,
            // then it can set the current exception.
            if (ast.CurrentFunction != null) {
                ast.CurrentFunction.CanSetSysExcInfo = true;
            }

            Position   start     = tokenStart;
            Expression errorType = null, name = null;
            if (!stream.PeekIs(Spec.BlockStarters)) {
                errorType = ParseTestExpr();
                if (stream.MaybeEat(KeywordAs)) {
                    name = ParseName();
                }
            }
            Statement block = ParseFullBlock();
            return new TryStatementHandler(errorType, name, block, start);
        }

        private Statement ParseFinallyBlock() {
            if (ast.CurrentFunction != null) {
                ast.CurrentFunction.ContainsTryFinally = true;
            }
            Statement finallyBlock;
            bool      isInFinally = inFinally, isInFinallyLoop = inFinallyLoop;
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
        ///         with_stmt:
        ///         'with' with_item (',' with_item)* ':' block
        ///     </c>
        /// </summary>
        private WithStatement ParseWithStmt() {
            Token start = StartExprOrStmt(KeywordWith);

            WithStatementItem       withItem = ParseWithItem();
            List<WithStatementItem> items    = null;
            while (stream.MaybeEat(Comma)) {
                if (items == null) {
                    items = new List<WithStatementItem>();
                }

                items.Add(ParseWithItem());
            }

            Statement block = ParseFullBlock();
            if (items != null) {
                for (int i = items.Count - 1; i >= 0; i--) {
                    block = new WithStatement(items[i], block, start);
                }
            }

            return new WithStatement(withItem, block, start);
        }

        /// <summary>
        ///     <c>
        ///         with_item:
        ///         test ['as' ID]
        ///     </c>
        /// </summary>
        private WithStatementItem ParseWithItem() {
            Position   start          = tokenStart;
            Expression contextManager = ParseTestExpr();
            Expression name           = null;
            if (stream.MaybeEat(KeywordAs)) {
                name = ParseName();
            }

            return new WithStatementItem(start, contextManager, name);
        }

        #endregion

        #region Use statement

//        /// <summary>
//        ///     use_stmt: 'import' module ['as' ID] (',' module ['as' ID])*
//        /// </summary>
//        private ImportStatement ParseImportStmt() {
//            stream.Eat(TokenType.KeywordImport);
//            Position start = tokenStart;
//
//            List<ModuleName> l   = new List<ModuleName>();
//            var              las = new List<string>();
//            l.Add(ParseModuleName());
//            las.Add(MaybeParseAsName());
//            while (stream.MaybeEat(TokenType.OpComma)) {
//                l.Add(ParseModuleName());
//                las.Add(MaybeParseAsName());
//            }
//            ModuleName[] names   = l.ToArray();
//            string[]     asNames = las.ToArray();
//
//            return new ImportStatement(names, asNames, start, tokenEnd);
//        }
//
//        /// <summary>
//        ///     module: (identifier '.')* identifier
//        /// </summary>
//        private ModuleName ParseModuleName() {
//            Position start = tokenStart;
//            return new ModuleName(ReadNames(), start, tokenEnd);
//        }
//
//        // relative_module: "."* module | "."+
//        private ModuleName ParseRelativeModuleName() {
//            Token start = stream.Token;
//
//            var dotCount = 0;
//            while (stream.MaybeEat(TokenType.OpDot)) {
//                dotCount++;
//            }
//
//            var names = new Token[0];
//            if (stream.Peek is IdentifierToken) {
//                names = ReadNames();
//            }
//
//            ModuleName ret;
//            if (dotCount > 0) {
//                ret = new RelativeModuleName(names, dotCount);
//            }
//            else {
//                if (names.Length == 0) {
//                    BlameInvalidSyntax(start);
//                }
//                ret = new ModuleName(names);
//            }
//
//            ret.MarkPosition(start.Span.Start, tokenEnd);
//            return ret;
//        }
//
//        // 'from' relative_module 'import' name ['as' ID] (',' name ['as' ID]) *
//        // 'from' relative_module 'import' '(' name ['as' ID] (',' name ['as' ID])* [','] ')'        
//        // 'from' module 'import' "*"                                        
//        private FromImportStatement ParseFromImportStmt() {
//            stream.Eat(TokenType.KeywordFrom);
//            Position   start = tokenStart;
//            ModuleName dname = ParseRelativeModuleName();
//
//            stream.Eat(TokenType.KeywordImport);
//
//            bool ateParen = stream.MaybeEat(TokenType.OpLeftParenthesis);
//
//            string[] names;
//            string[] asNames;
//
//            if (stream.MaybeEat(TokenType.OpMultiply)) {
//                names   = (string[]) FromImportStatement.Star;
//                asNames = null;
//            }
//            else {
//                var l   = new List<string>();
//                var las = new List<string>();
//
//                if (stream.MaybeEat(TokenType.OpLeftParenthesis)) {
//                    ParseAsNameList(l, las);
//                    stream.Eat(TokenType.OpRightParenthesis);
//                }
//                else {
//                    ParseAsNameList(l, las);
//                }
//                names   = l.ToArray();
//                asNames = las.ToArray();
//            }
//
//            if (ateParen) {
//                stream.Eat(TokenType.OpRightParenthesis);
//            }
//
//            return new FromImportStatement(dname, names, asNames, start, tokenEnd);
//        }
//
//        // import_as_name (',' import_as_name)*
//        private void ParseAsNameList(List<string> l, List<string> las) {
//            l.Add(ReadName());
//            las.Add(MaybeParseAsName());
//            while (stream.MaybeEat(TokenType.OpComma)) {
//                if (stream.PeekIs(TokenType.OpRightParenthesis)) {
//                    return; // the list is allowed to end with a ,
//                }
//
//                l.Add(ReadName());
//                las.Add(MaybeParseAsName());
//            }
//        }
//
//        //import_as_name: NAME [NAME NAME]
//        //dotted_as_name: dotted_name [NAME NAME]
//        private string MaybeParseAsName() {
//            if (stream.MaybeEat(TokenType.KeywordAs)) {
//                return ReadName();
//            }
//            return null;
//        }

        #endregion

        #region Decorators

        /// <summary>
        ///     <c>
        ///         decorators:
        ///         ('@' decorator (',' decorator)* NEWLINE)+
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
                    ReportError("Decorators list must be placed on one line.", stream.Token);
                }
            }

            return decorators;
        }

        /// <summary>
        ///     '
        ///     <c>
        ///         decorator:
        ///         name ['(' [argument_list [',']] ')']
        ///     </c>
        /// </summary>
        private Expression ParseDecorator() {
            // on '@'
            Position   start     = tokenStart;
            Expression decorator = ParseName(true);
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
        ///         assert_stmt:
        ///         'assert' test [',' test]
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
        ///         delete_stmt:
        ///         'delete' expr_list
        ///     </c>
        ///     <para />
        ///     For error reporting reasons we allow any
        ///     expr and then report the bad delete node when it fails.
        ///     This is the reason we don't call ParseTargetList.
        /// </summary>
        private Statement ParseDeleteStmt() {
            Token start = StartExprOrStmt(KeywordDelete);

            List<Expression> expressions = ParseExpressionList(out bool _);
            foreach (Expression expr in expressions) {
                if (expr.CannotDeleteReason != null) {
                    Blame(BlameType.InvalidExpressionToDelete, expr);
                }
            }

            return new DeleteStatement(expressions.ToArray(), start);
        }

        /// <summary>
        ///     <c>
        ///         break_stmt:
        ///         'break' [ID]
        ///     </c>
        /// </summary>
        private Statement ParseBreakStmt() {
            // TODO: add ['break' loop_name]
            stream.Eat(KeywordBreak);
            if (!inLoop) {
                Blame(BlameType.BreakIsOutsideLoop, stream.Token);
            }
            return new BreakStatement(stream.Token);
        }

        /// <summary>
        ///     <c>
        ///         continue_stmt:
        ///         'continue' [ID]
        ///     </c>
        /// </summary>
        private Statement ParseContinueStmt() {
            // TODO: add ['continue' loop_name]
            stream.Eat(KeywordContinue);
            if (!inLoop) {
                Blame(BlameType.ContinueIsOutsideLoop, stream.Token);
            }
            else if (inFinally && !inFinallyLoop) {
                Blame(BlameType.ContinueNotSupportedInsideFinally, stream.Token);
            }
            return new ContinueStatement(stream.Token);
        }

        /// <summary>
        ///     <c>
        ///         return_stmt:
        ///         'return' [test_list]
        ///     </c>
        /// </summary>
        private Statement ParseReturnStmt() {
            Token start = StartExprOrStmt(KeywordReturn);

            if (ast.CurrentFunction == null) {
                Blame(BlameType.MisplacedReturn, stream.Token);
            }

            Expression expr = null;
            if (!stream.PeekIs(Spec.NeverTestTypes)) {
                expr = ParseTestList();
            }

            return new ReturnStatement(expr, start);
        }

        /// <summary>
        ///     <c>
        ///         raise_stmt:
        ///         'raise' [test ['from' test]]
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
        ///         yield_stmt: yield_expr
        ///     </c>
        /// </summary>
        private Statement ParseYieldStmt() {
            // For yield statements, continue to enforce that it's currently in a function. 
            // This gives us better syntax error reporting for yield-statements than for yield-expressions.
            if (ast.CurrentFunction == null) {
                Blame(BlameType.MisplacedYield, stream.Token);
            }

            Expression yieldExpr = ParseYield();
            Debug.Assert(yieldExpr != null); // caller already verified we have a yield.

            return new ExpressionStatement(yieldExpr);
        }

        #endregion

        #region Block statement

        private Statement ParseTopLevelBlock() {
            Statement block;
            bool      isInLoop = inLoop, isInFinally = inFinally, isInFinallyLoop = inFinallyLoop;
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

        private Statement ParseLoopBlock() {
            Statement block;
            bool      wasInLoop = inLoop, wasInFinallyLoop = inFinallyLoop;
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
        private (TokenType terminator, bool oneLine, bool error) ParseBlockStart(bool usesColon = true) {
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
                    Blame(BlameType.ExpectedBlockDeclaration, stream.Peek);
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
                Blame(BlameType.RedundantColonWithBraces, blockStart);
            }

            stream.MaybeEatNewline();
            return (terminator, false, false);
        }

        /// <summary>
        ///     <c>
        ///         block:
        ///         (':' simple_stmt) |
        ///         ([':'] '{' stmt+ '}') |
        ///         (':' NEWLINE INDENT stmt+ OUTDENT)
        ///     </c>
        /// </summary>
        private Statement ParseFullBlock(bool usesColon = true) {
            var statements = new List<Statement>();
            (TokenType terminator, bool oneLine, bool error) = ParseBlockStart(usesColon);

            Position start = tokenStart;
            if (oneLine) {
                statements.Add(ParseMultipleStmt());
            }
            else if (!error && !stream.MaybeEat(terminator)) {
                while (true) {
                    statements.Add(ParseMultipleStmt(terminator));
                    if (stream.MaybeEat(terminator)) {
                        break;
                    }
                    if (CheckUnexpectedEOC()) {
                        break;
                    }
                }
            }

            return new BlockStatement(statements.ToArray(), start, tokenEnd);
        }

        #endregion
    }
}