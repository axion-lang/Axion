//using System.Collections.Generic;
//using Axion.Core.Tokens;
//using Axion.Core.Tokens.Ast;
//
//namespace Axion.Core.Processing {
//    internal class SyntaxParser {
//        /// <summary>
//        ///     Reference to processing <see cref="LinkedList{T}" /> of tokens.
//        /// </summary>
//        private readonly LinkedList<Token> tokens;
//
//        /// <summary>
//        ///     Contains all errors that raised during syntax analysis.
//        /// </summary>
//        private readonly List<SyntaxException> errors = new List<SyntaxException>();
//
//        /// <summary>
//        ///     Contains all warnings that found during syntax analysis.
//        /// </summary>
//        private readonly List<SyntaxException> warnings = new List<SyntaxException>();
//
//        /// <summary>
//        ///     Outgoing Abstract Syntax Tree.
//        /// </summary>
//        private readonly Ast ast;
//
//        private LinkedListNode<Token> tokenNode;
//
//        private LinkedListNode<Token> nextTokenNode => tokenNode.Next;
//
//        private Token nextToken => tokenNode.Next?.Value;
//
//        //private readonly Stack<ImportDefinition>   imports   = new Stack<ImportDefinition>();
//        //private readonly Stack<ClassDefinition>    classes   = new Stack<ClassDefinition>();
//        //private readonly Stack<FunctionDefinition> functions = new Stack<FunctionDefinition>();
//
//        internal SyntaxParser(
//            LinkedList<Token> tokens,
//            out Ast           ast
//        ) {
//            this.tokens = tokens;
//            ast         = this.ast;
//        }
//
//        internal void Process() {
//            if (tokens.Count == 0) {
//                return;
//            }
//            tokenNode = tokens.First;
//            var statements = new List<Statement>();
//
//            while (true) {
//                if (MaybeEat(TokenType.EndOfStream)) break;
//
//                if (MaybeEat(TokenType.Newline)) continue;
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
//            switch (tokenNode.Value.Type) {
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
//            Eat(TokenType.KeywordIf);
//            var branches = new List<IfStatementBranch>();
//
//            do {
//                Expressio condition  = ParseExpression();
//                Statement branchBody = ParseSuite();
//                branches.Add(new IfStatementBranch(condition, branchBody));
//            } while (MaybeEat(TokenType.KeywordElif));
//
//            Statement elseBranch = null;
//            if (MaybeEat(TokenType.KeywordElse)) {
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
//            if (MaybeEat(TokenKind.KeywordLambda)) {
//                return FinishLambdef();
//            }
//
//            Expressio ret = ParseOrTest();
//            if (MaybeEat(TokenKind.KeywordIf)) {
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
//            if (!EatNoEof(TokenType.Colon)) {
//                // improve error handling...
//                return ErrorStmt();
//            }
//
//            Token cur = nextToken;
//            var   l   = new List<Statement>();
//
//            // we only read a real Newline here because we need to adjust error reporting
//            // for the interpreter.
//            if (MaybeEat(TokenType.Newline)) {
//                CheckSuiteEofError(cur);
//
//                cur = nextToken;
//
//                if (!MaybeEat(TokenType.Indent)) {
//                    // no indent?  report the indentation error.
//                    if (cur.Type == TokenType.Outdent) {
//                        ReportSyntaxError(
//                            nextToken.Span.Start, nextToken.Span.End, Resources.ExpectedIndentation,
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
//                    if (MaybeEat(TokenType.Dedent)) break;
//
//                    if (PeekToken(TokenType.EndOfStream)) {
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
//        private Token Move() {
//            tokenNode = nextTokenNode;
//            return tokenNode.Value;
//        }
//
//        private bool PeekToken(TokenType type) {
//            return nextToken.Type == type;
//        }
//
//        private bool PeekToken(Token expected) {
//            return Equals(nextToken, expected);
//        }
//
//        private bool MaybeEat(TokenType type) {
//            if (nextToken.Type == type) {
//                Move();
//                return true;
//            }
//            return false;
//        }
//
//        private bool Eat(TokenType type) {
//            if (nextToken.Type == type) {
//                Move();
//                return true;
//            }
//            ReportSyntaxError(nextToken);
//            return false;
//        }
//
//        private bool EatNonEof(TokenType type) {
//            if (nextToken.Type == type) {
//                Move();
//                return true;
//            }
//            ReportSyntaxError(nextToken, ErrorCodes.SyntaxError, false);
//            return false;
//        }
//
//        #endregion
//    }
//}