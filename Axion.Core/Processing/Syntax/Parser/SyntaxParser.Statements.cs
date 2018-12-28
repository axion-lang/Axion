using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Axion.Core.Processing.Errors;
using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Processing.Syntax.Tree.Expressions;
using Axion.Core.Processing.Syntax.Tree.Statements;
using Axion.Core.Specification;
using static Axion.Core.Processing.Lexical.Tokens.TokenType;

namespace Axion.Core.Processing.Syntax.Parser {
    public partial class SyntaxParser {
        /// <summary>
        /// <c>
        ///     stmt:
        ///         simple_stmt | compound_stmt
        ///     compound_stmt:
        ///         if_stmt  | while_stmt | for_stmt  |
        ///         try_stmt | with_stmt  | class_def |
        ///         func_def | use_stmt   | mod_stmt
        /// </c>
        /// </summary>
        private Statement ParseStmt(TokenType terminator = None) {
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
                case KeywordClass: {
                    return ParseClass();
                }
                // TODO: func def
                //case KeywordUse: {
                //    return ParseImportStmt();
                //}
                default: {
                    if (Spec.Modifiers.Contains(stream.Peek.Type)) {
                        return ParseModifiersStmt();
                    }
                    return ParseSimpleStmt(terminator);
                }
            }
        }

        #region If statement

        /// <summary>
        /// <c>
        ///     if_stmt:
        ///         'if' if_stmt_branch
        ///         ('elif' if_stmt_branch)*
        ///         ['else' suite]
        /// </c>
        /// </summary>
        private IfStatement ParseIfStmt() {
            StartNewStmt(KeywordIf);

            // if start block
            var branches = new List<IfStatementBranch> { ParseIfStmtBranch() };

            // else ifs
            while (stream.MaybeEat(KeywordElseIf)) {
                branches.Add(ParseIfStmtBranch());
            }

            // else
            Statement elseBranch = null;
            if (stream.MaybeEat(KeywordElse)) {
                elseBranch = ParseSuite();
            }

            return new IfStatement(branches, elseBranch);
        }

        /// <summary>
        /// <c>
        ///     if_stmt_branch:
        ///         test suite
        /// </c>
        /// </summary>
        private IfStatementBranch ParseIfStmtBranch() {
            Token      start = stream.Token;
            Expression expr  = ParseTestExpr();
            Statement  suite = ParseSuite();
            return new IfStatementBranch(expr, suite, start);
        }

        #endregion

        #region While statement

        /// <summary>
        /// <c>
        ///     while_stmt:
        ///         'while' test suite ['else' suite]
        /// </c>
        /// </summary>
        private WhileStatement ParseWhileStmt() {
            Token start = StartNewStmt(KeywordWhile);

            Expression condition   = ParseTestExpr();
            Statement  body        = ParseLoopSuite();
            Statement  noBreakBody = null;
            if (stream.MaybeEat(KeywordElse)) {
                noBreakBody = ParseSuite();
            }
            return new WhileStatement(condition, body, noBreakBody, start);
        }

        #endregion

        #region For statement

        /// <summary>
        /// <c>
        ///     for_stmt:
        ///         'for' (exprlist 'in' testlist |
        ///                expr_stmt, expr_stmt, expr_stmt)
        ///         suite ['else' suite]
        /// </c>
        /// </summary>
        private ForStatement ParseForStmt() {
            // TODO: add 'for' expr, expr, expr
            Token start = StartNewStmt(KeywordFor);

            List<Expression> l = ParseTargetListExpr(out bool trailingComma);

            // expr list is something like:
            // ()
            // a
            // a,b
            // a,b,c
            // we either want just () or a or we want (a,b) and (a,b,c)
            // so we can do tupleExpr.EmitSet() or loneExpr.EmitSet()

            Expression lhs = MakeTupleOrExpr(l, trailingComma);
            stream.Eat(KeywordIn);
            Expression list  = ParseTestListAsExpr();
            Statement  body  = ParseLoopSuite();
            Statement  else_ = null;
            if (stream.MaybeEat(KeywordElse)) {
                else_ = ParseSuite();
            }
            return new ForStatement(lhs, list, body, else_, start);
        }

        #endregion

        #region Try statement

        /// <summary>
        /// <c>
        ///     try_stmt:
        ///         ('try' suite
        ///         ((except_clause suite)+
        ///         ['else' suite]
        ///         ['finally' suite] |
        ///         'finally' suite))
        /// </c>
        /// </summary>
        private Statement ParseTryStatement() {
            Token start = StartNewStmt(KeywordTry);

            // try
            Statement body         = ParseSuite();
            Statement finallySuite = null;

            // anyway
            if (stream.MaybeEat(KeywordAnyway)) {
                finallySuite = ParseFinallySuite();
                return new TryStatement(body, null, null, finallySuite, start);
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
                if (handler.Test == null) {
                    defaultHandler = handler;
                }
            } while (stream.PeekIs(KeywordCatch));
            // else
            Statement elseSuite = null;
            if (stream.MaybeEat(KeywordElse)) {
                elseSuite = ParseSuite();
            }
            // anyway
            if (stream.MaybeEat(KeywordAnyway)) {
                // If this function has an except block, then it can set the current exception.
                finallySuite = ParseFinallySuite();
            }
            return new TryStatement(body, handlers, elseSuite, finallySuite, start);
        }

        // catch_clause: 'catch' [expression ['as' identifier]]
        private TryStatementHandler ParseTryStmtHandler() {
            stream.Eat(KeywordCatch);

            // If this function has an except block,
            // then it can set the current exception.
            if (ast.CurrentFunction != null) {
                ast.CurrentFunction.CanSetSysExcInfo = true;
            }

            Position   start = tokenStart;
            Expression test  = null, target = null;
            if (!stream.PeekIs(Colon)) {
                test = ParseTestExpr();
                if (stream.MaybeEat(KeywordAs)) {
                    target = ReadName();
                }
            }
            Statement body = ParseSuite();
            return new TryStatementHandler(test, target, body, start);
        }

        private Statement ParseFinallySuite() {
            if (ast.CurrentFunction != null) {
                ast.CurrentFunction.ContainsTryFinally = true;
            }
            Statement finallySuite;
            bool isInFinally     = inFinally,
                 isInFinallyLoop = inFinallyLoop;
            try {
                inFinally     = true;
                inFinallyLoop = false;
                finallySuite  = ParseSuite();
            }
            finally {
                inFinally     = isInFinally;
                inFinallyLoop = isInFinallyLoop;
            }
            return finallySuite;
        }

        #endregion

        #region With statement

        /// <summary>
        ///     with_stmt:
        ///     'with' with_item (',' with_item)* ':' suite
        ///     with_item:
        ///     test ['as' expr]
        /// </summary>
        private WithStatement ParseWithStmt() {
            Token start = StartNewStmt(KeywordWith);

            WithStatementItem       withItem = ParseWithItem();
            List<WithStatementItem> items    = null;
            while (stream.MaybeEat(Comma)) {
                if (items == null) {
                    items = new List<WithStatementItem>();
                }

                items.Add(ParseWithItem());
            }

            Statement body = ParseSuite();
            if (items != null) {
                for (int i = items.Count - 1; i >= 0; i--) {
                    body = new WithStatement(items[i], body, start);
                }
            }

            return new WithStatement(withItem, body, start);
        }

        private WithStatementItem ParseWithItem() {
            Position   start          = tokenStart;
            Expression contextManager = ParseTestExpr();
            Expression variable       = null;
            if (stream.MaybeEat(KeywordAs)) {
                variable = ParseTestExpr();
            }

            return new WithStatementItem(start, contextManager, variable);
        }

        #endregion

        #region Class statement

        /// <summary>
        ///     class_def:
        ///         'class' ID ['(' args_list ')'] ':' suite
        /// </summary>
        private ClassDefinition ParseClass() {
            Token start = StartNewStmt(KeywordClass);

            NameExpression name = ReadName();
            if (name == null) {
                // no name, assume there's no class.
                return new ClassDefinition(null, new Expression[0], new Expression[0], ErrorStmt());
            }

            Expression metaClass = null;
            var        bases     = new List<Expression>();
            var        keywords  = new List<Expression>();
            if (stream.MaybeEat(LeftParenthesis)) {
                foreach (Arg arg in ParseArgumentsList()) {
                    ArgumentKind argKind = arg.GetArgumentInfo();
                    if (argKind == ArgumentKind.Simple) {
                        bases.Add(arg.Value);
                    }
                    else if (argKind == ArgumentKind.Named) {
                        keywords.Add(arg.Value);
                        if (arg.Name.Name == "metaclass") {
                            metaClass = arg.Value;
                        }
                    }
                }
            }
            var ret = new ClassDefinition(name, bases.ToArray(), keywords.ToArray(), metaClass: metaClass);
            ast.PushClass(ret);

            // Parse class body
            Statement body = ParseClassOrFuncBody();

            ClassDefinition ret2 = ast.PopClass();
            Debug.Assert(ret == ret2);

            ret.Body = body;
            ret.MarkPosition(start.Span.Start, tokenEnd);
            return ret;
        }

        private Statement ParseClassOrFuncBody() {
            Statement body;
            bool isInLoop        = inLoop,
                 isInFinally     = inFinally,
                 isInFinallyLoop = inFinallyLoop;
            try {
                inLoop        = false;
                inFinally     = false;
                inFinallyLoop = false;
                body          = ParseSuite();
            }
            finally {
                inLoop        = isInLoop;
                inFinally     = isInFinally;
                inFinallyLoop = isInFinallyLoop;
            }
            return body;
        }

        #endregion

        #region Use statement

//        /// <summary>
//        ///     use_stmt: 'import' module ['as' name] (',' module ['as' name])*        
//        ///     name: identifier
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
//        // 'from' relative_module 'import' identifier ['as' name] (',' identifier ['as' name]) *
//        // 'from' relative_module 'import' '(' identifier ['as' name] (',' identifier ['as' name])* [','] ')'        
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

        /// <summary>
        /// <c>
        ///     simple_stmt:
        ///         simple_stmt: small_stmt (';' small_stmt)* [';'] (terminator | NEWLINE)
        /// </c>
        /// </summary>
        private Statement ParseSimpleStmt(TokenType terminator = None) {
            Statement statement = ParseSmallStmt();
            if (stream.MaybeEat(Semicolon)) {
                var statements = new List<Statement> { statement };
                while (
                    stream.Token.Type == Semicolon
                 && !stream.MaybeEatNewline()
                 && !stream.PeekIs(terminator)
                ) {
                    statements.Add(ParseSmallStmt());
                    if (stream.MaybeEat(EndOfStream)) {
                        if (terminator != None) {
                            // force error here (No terminator)
                            stream.Eat(terminator);
                        }
                        // else EOF implies a new line
                        break;
                    }
                    if (!stream.MaybeEat(Semicolon)) {
                        stream.EatNewline();
                    }
                }
                return new SuiteStatement(statements);
            }
            if (!stream.MaybeEat(EndOfStream) && !stream.EatNewline()) {
                // error handling, make sure we're making forward progress
                stream.NextToken();
            }
            return statement;
        }

        /// <summary>
        ///     small_stmt:
        ///         assert_stmt | delete_stmt |
        ///         pass_stmt | flow_stmt | expr_stmt
        ///     flow_stmt:
        ///         break_stmt | continue_stmt |
        ///         return_stmt | raise_stmt | yield_stmt
        /// </summary>
        private Statement ParseSmallStmt() {
            switch (stream.Peek.Type) {
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
                default: {
                    return ParseExpressionStmt();
                }
            }
        }

        #region Small statements

        /// <summary>
        /// <c>
        ///     assert_stmt:
        ///         'assert' test [',' test]
        /// </c>
        /// </summary>
        private AssertStatement ParseAssertStmt() {
            Token start = StartNewStmt(KeywordAssert);

            Expression condition       = ParseTestExpr();
            Expression falseExpression = null;
            if (stream.MaybeEat(Comma)) {
                falseExpression = ParseTestExpr();
            }
            return new AssertStatement(condition, falseExpression, start);
        }

        /// <summary>
        /// <c>
        ///     delete_stmt:
        ///         'delete' expr_list
        /// </c>
        /// <para />
        ///     For error reporting reasons we allow any
        ///     expression and then report the bad delete node when it fails.
        ///     This is the reason we don't call ParseTargetList.
        /// </summary>
        private Statement ParseDeleteStmt() {
            Token start = StartNewStmt(KeywordDelete);

            List<Expression> expressions = ParseExprList();
            foreach (Expression expr in expressions) {
                if (expr.CannotDeleteReason != null) {
                    Blame(BlameType.InvalidExpressionToDelete, expr);
                }
            }

            return new DeleteStatement(expressions, start);
        }

        /// <summary>
        /// <c>
        ///     break_stmt:
        ///         'break' [ID]
        /// </c>
        /// </summary>
        private Statement ParseBreakStmt() {
            // TODO: add ['break' loop_name]
            stream.NextToken();
            if (!inLoop) {
                Blame(BlameType.BreakIsOutsideLoop, stream.Token);
            }
            return new BreakStatement(stream.Token);
        }

        /// <summary>
        /// <c>
        ///     continue_stmt:
        ///         'continue' [ID]
        /// </c>
        /// </summary>
        private Statement ParseContinueStmt() {
            // TODO: add ['continue' loop_name]
            stream.NextToken();
            if (!inLoop) {
                Blame(BlameType.ContinueIsOutsideLoop, stream.Token);
            }
            else if (inFinally && !inFinallyLoop) {
                Blame(BlameType.ContinueNotSupportedInsideFinally, stream.Token);
            }
            return new ContinueStatement(stream.Token);
        }

        /// <summary>
        /// <c>
        ///     return_stmt:
        ///         'return' [test_list]
        /// </c>
        /// </summary>
        private Statement ParseReturnStmt() {
            Token start = StartNewStmt(KeywordReturn);

            if (ast.CurrentFunction == null) {
                Blame(BlameType.MisplacedReturn, stream.Token);
            }

            Expression expr = null;
            if (!Spec.NeverTestTypes.Contains(stream.Peek.Type)) {
                expr = ParseTestListAsExpr();
            }

            return new ReturnStatement(expr, start);
        }

        /// <summary>
        /// <c>
        ///     raise_stmt:
        ///         'raise' [test ['from' test]]
        /// </c>
        /// </summary>
        private RaiseStatement ParseRaiseStmt() {
            Token start = StartNewStmt(KeywordRaise);

            Expression expr = null, cause = null;
            if (!Spec.NeverTestTypes.Contains(stream.Peek.Type)) {
                expr = ParseTestExpr();
                if (stream.MaybeEat(KeywordFrom)) {
                    cause = ParseTestExpr();
                }
            }
            return new RaiseStatement(expr, cause, start);
        }

        /// <summary>
        /// <c>
        ///     yield_stmt: yield_expr
        /// </c>
        /// </summary>
        private Statement ParseYieldStmt() {
            stream.Eat(KeywordYield);

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

        private Statement ParseLoopSuite() {
            Statement body;
            bool wasInLoop        = inLoop,
                 wasInFinallyLoop = inFinallyLoop;
            try {
                inLoop        = true;
                inFinallyLoop = inFinally;
                body          = ParseSuite();
            }
            finally {
                inLoop        = wasInLoop;
                inFinallyLoop = wasInFinallyLoop;
            }
            return body;
        }

        /// <summary>
        /// <c>
        ///     suite:
        ///         (':' simple_stmt) |
        ///         ([':'] [NEWLINE] '{' [NEWLINE] stmt+ '}') |
        ///         (':' NEWLINE INDENT stmt+ OUTDENT)
        /// </c>
        /// </summary>
        private Statement ParseSuite() {
            var statements = new List<Statement>();

            // 1) colon
            bool  hasColon = stream.MaybeEat(Colon);
            Token colon    = stream.Token;

            // single-line suite (not a NEWLINE, not a '{')
            if (!stream.MaybeEatNewline() && !stream.PeekIs(LeftBrace)) {
                if (!hasColon) {
                    // must have a colon
                    BlameInvalidSyntax(Colon, stream.Peek);
                }
                return ParseSimpleStmt();
            }

            // multiline suite
            TokenType bodyTerminator;
            // 2) '{'
            if (stream.MaybeEat(LeftBrace)) {
                bodyTerminator = RightBrace;
                stream.MaybeEatNewline();
            }
            // 2) INDENT
            else if (stream.MaybeEat(Indent)) {
                bodyTerminator = Outdent;
            }
            else {
                // no indent or brace? - report suite error.
                Blame(BlameType.ExpectedBlockDeclaration, stream.Peek);
                return ErrorStmt();
            }

            // ':' followed by '{'
            if (hasColon && bodyTerminator == RightBrace) {
                Blame(BlameType.ColonIsNotNeededWithBraces, colon);
            }

            // statements
            while (true) {
                statements.Add(ParseStmt(bodyTerminator));
                if (stream.MaybeEat(bodyTerminator)) {
                    break;
                }
                if (stream.PeekIs(EndOfStream)) {
                    Blame(BlameType.UnexpectedEndOfCode, stream.Token);
                    break;
                }
            }

            return new SuiteStatement(statements);
        }
    }
}