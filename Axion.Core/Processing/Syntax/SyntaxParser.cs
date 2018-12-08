//using System;
//using System.Collections.Generic;
//using Axion.Core.Processing.Errors;
//using Axion.Core.Processing.Lexical.Tokens;
//using Axion.Core.Processing.Syntax.Tree;
//
//namespace Axion.Core.Processing.Syntax {
//    internal class SyntaxParser {
//        /// <summary>
//        ///     Contains all errors that raised during syntax analysis.
//        /// </summary>
//        private List<SyntaxException> errors { get; }
//
//        /// <summary>
//        ///     Contains all warnings that found during syntax analysis.
//        /// </summary>
//        private List<SyntaxException> warnings { get; }
//
//        private TokenStream stream { get; }
//
//        /// <summary>
//        ///     Outgoing Abstract Syntax Tree.
//        /// </summary>
//        private readonly Ast ast;
//
//        internal SyntaxParser(
//            TokenStream           stream,
//            Ast                   outAst,
//            List<SyntaxException> outErrors,
//            List<SyntaxException> outWarnings
//        ) {
//            this.stream = stream;
//            ast         = outAst ?? throw new ArgumentNullException(nameof(outAst));
//            errors      = outErrors ?? new List<SyntaxException>();
//            warnings    = outWarnings ?? new List<SyntaxException>();
//        }
//
//        internal void Process() {
//            if (stream.Tokens.Count == 0) {
//                return;
//            }
//            var statements = new List<Statement>();
//
//            while (!stream.MaybeEat(TokenType.EndOfStream)) {
//                if (stream.MaybeEat(TokenType.Newline)) continue;
//
//                Statement statement = ParseStmt();
//                statements.Add(statement);
//            }
//        }
//
//        /// <summary>
//        ///     stmt:
//        ///         simple_stmt | compound_stmt
//        ///     compound_stmt:
//        ///         if_stmt | while_stmt | for_stmt |
//        ///         try_stmt | with_stmt | funcdef |
//        ///         classdef | decorated | async_stmt
//        /// </summary>
//        private Statement ParseStmt() {
//            switch (stream.Token.Type) {
//                // if
//                case TokenType.KeywordIf:
//                    return ParseIfStmt();
//                // while
//                case TokenType.KeywordWhile:
//                    return ParseWhileStmt();
//                // for
//                case TokenType.KeywordFor:
//                    return ParseForStmt();
//                // try
//                case TokenType.KeywordTry:
//                    return ParseTryStatement();
//                // use
//                case TokenType.KeywordUse:
//                    return ParseUseStmt();
//                // class
//                case TokenType.KeywordClass:
//                    return ParseClassDef();
//                // async
//                case TokenType.KeywordAsync:
//                    return ParseAsyncStmt();
//                default:
//                    return ParseSimpleStmt();
//            }
//        }
//
//        /// <summary>
//        ///     if_stmt:
//        ///         'if' expression ':'
//        ///         suite
//        ///         ('elif' expression ':' suite)*
//        ///         ['else' ':' suite]
//        /// </summary>
//        private IfStatement ParseIfStmt() {
//            stream.Eat(TokenType.KeywordIf);
//            var branches = new List<IfStatementBranch>();
//
//            do {
//                Expressio condition  = ParseExpression();
//                Statement branchBody = ParseSuite();
//                branches.Add(new IfStatementBranch(condition, branchBody));
//            } while (stream.MaybeEat(TokenType.KeywordElif));
//
//            Statement elseBranch = null;
//            if (stream.MaybeEat(TokenType.KeywordElse)) {
//                elseBranch = ParseSuite();
//            }
//
//            return new IfStatement(branches, elseBranch);
//        }
//
//        /// <summary>
//        /// expression: conditional_expression | lambda_form
//        ///     conditional_expression: or_test ['if' or_test 'else' expression]
//        ///     lambda_form: "lambda" [parameter_list] : expression
//        /// </summary>
//        private Expressio ParseExpression() {
//            if (stream.MaybeEat(TokenType.KeywordLambda)) {
//                return FinishLambdef();
//            }
//
//            Expressio ret = ParseOrTest();
//            if (stream.MaybeEat(TokenType.KeywordIf)) {
//                var start = ret.StartIndex;
//                ret = ParseConditionalTest(ret);
//                ret.SetLoc(_globalParent, start, GetEnd());
//            }
//
//            return ret;
//        }
//
//        /// <summary>
//        /// suite:
//        ///     simple_stmt NEWLINE | Newline INDENT stmt+ DEDENT
//        /// </summary>
//        /// <returns></returns>
//        private Statement ParseSuite() {
//            if (!stream.EatNoEof(TokenType.OpColon)) {
//                // improve error handling...
//                return ErrorStmt();
//            }
//
//            Token cur = stream.Peek;
//            var   l   = new List<Statement>();
//
//            // we only read a real Newline here because we need to adjust error reporting
//            // for the interpreter.
//            if (stream.MaybeEat(TokenType.Newline)) {
//                CheckSuiteEofError(cur);
//
//                cur = nextToken;
//
//                if (!stream.MaybeEat(TokenType.Indent)) {
//                    // no indent?  report the indentation error.
//                    if (cur.Type == TokenType.Outdent) {
//                        ReportSyntaxError(
//                            stream.Peek.Span.Start, nextToken.Span.End, Resources.ExpectedIndentation,
//                            ErrorCodes.IndentationError | ErrorCodes.IncompleteStatement
//                        );
//                    }
//                    else {
//                        ReportSyntaxError(cur, ErrorCodes.IndentationError);
//                    }
//                    return ErrorStmt();
//                }
//
//                while (true) {
//                    Statement s = ParseStmt();
//                    l.Add(s);
//                    if (stream.MaybeEat(TokenType.Outdent)) break;
//
//                    if (stream.PeekIs(TokenType.EndOfStream)) {
//                        ReportSyntaxError("unexpected end of file");
//                        break; // error handling
//                    }
//                }
//
//                if (CurrentClass != null && CurrentClass.Metaclass != null) {
//                    l.Insert(1, new AssignmentStatement(new[] { new NameExpression("__metaclass__") }, CurrentClass.Metaclass));
//                }
//
//                Statement[]    stmts = l.ToArray();
//                SuiteStatement ret   = new SuiteStatement(stmts);
//                ret.SetLoc(_globalParent, stmts[0].StartIndex, stmts[stmts.Length - 1].EndIndex);
//                return ret;
//            }
//            else {
//                //  simple_stmt NEWLINE
//                //  ParseSimpleStmt takes care of the NEWLINE
//                return ParseSimpleStmt();
//            }
//        }
//
//        #region Tokens stream control
//
//
//
//        #endregion
//    }
//}

