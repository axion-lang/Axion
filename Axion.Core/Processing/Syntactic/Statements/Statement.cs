using Axion.Core.Processing.Errors;
using Axion.Core.Processing.Syntactic.Expressions;
using Axion.Core.Processing.Syntactic.Statements.Definitions;
using Axion.Core.Processing.Syntactic.Statements.Interfaces;
using Axion.Core.Processing.Syntactic.Statements.Small;
using Axion.Core.Specification;

namespace Axion.Core.Processing.Syntactic.Statements {
    public abstract class Statement : SyntaxTreeNode {
        protected Statement(SyntaxTreeNode parent) : base(parent) { }
        protected Statement() { }

        /// <summary>
        ///     <c>
        ///         single_stmt:
        ///             cond_stmt | while_stmt | for_stmt    |
        ///             try_stmt  | with_stmt  | import_stmt |
        ///             decorated
        ///         decorated:
        ///             module_def | class_def  | enum_def |
        ///             func_def   | small_stmt
        ///         small_stmt:
        ///             assert_stmt | delete_stmt | pass_stmt |
        ///             expr_stmt   | flow_stmt
        ///         flow_stmt:
        ///             break_stmt | continue_stmt | return_stmt |
        ///             raise_stmt | yield_stmt
        ///     </c>
        /// </summary>
        private static Statement ParseSingleStmt(
            SyntaxTreeNode parent,
            bool           onlyDecorated = false
        ) {
            if (!onlyDecorated) {
                switch (parent.Peek.Type) {
                    case TokenType.KeywordIf:
                    case TokenType.KeywordUnless: {
                        return new ConditionalStatement(parent);
                    }

                    case TokenType.KeywordWhile: {
                        return new WhileStatement(parent);
                    }

                    case TokenType.KeywordFor: {
                        return LoopStatement.ParseFor(parent);
                    }

                    case TokenType.KeywordTry: {
                        return new TryStatement(parent);
                    }

                    case TokenType.KeywordWith: {
                        return new WithStatement(parent);
                    }

                    #region Small statements

                    case TokenType.KeywordAssert: {
                        return new AssertStatement(parent);
                    }

                    case TokenType.Semicolon:
                    case TokenType.KeywordPass: {
                        return new EmptyStatement(parent);
                    }

                    case TokenType.KeywordBreak: {
                        return new BreakStatement(parent);
                    }

                    case TokenType.KeywordContinue: {
                        return new ContinueStatement(parent);
                    }

                    case TokenType.KeywordDelete: {
                        return new DeleteStatement(parent);
                    }

                    case TokenType.KeywordRaise: {
                        return new RaiseStatement(parent);
                    }

                    case TokenType.KeywordReturn: {
                        return new ReturnStatement(parent);
                    }

                    case TokenType.KeywordYield: {
                        // For yield statements, continue to enforce that it's currently in a function. 
                        // This gives us better syntax error reporting for yield-statements than for yield-expressions.
                        if (parent.Ast.CurrentFunction == null) {
                            parent.Unit.Blame(BlameType.MisplacedYield, parent.Token);
                        }

                        return new ExpressionStatement(new YieldExpression(parent));
                    }

                    #endregion

                    case TokenType.CloseBracket: {
                        NodeList<Expression> decorators = ParseDecorators(parent);

                        Statement stmt = ParseSingleStmt(parent, true);
                        if (stmt is IDecorated def) {
                            def.Modifiers = decorators;
                        }
                        else {
                            parent.Unit.ReportError(Spec.ERR_InvalidDecoratorPosition, stmt);
                        }

                        return stmt;
                    }

                    // there should be no 'default' case!
                }
            }

            switch (parent.Peek.Type) {
                case TokenType.KeywordModule: {
                    return new ModuleDefinition(parent);
                }

                case TokenType.KeywordClass: {
                    return new ClassDefinition(parent);
                }

                case TokenType.KeywordEnum: {
                    return new EnumDefinition(parent);
                }

                case TokenType.KeywordFn: {
                    return new FunctionDefinition(parent);
                }

                default: {
                    if (onlyDecorated) {
                        parent.Unit.ReportError("Invalid decorated statement", parent.Peek);
                        return new BlockStatement(ParseStmt(parent));
                    }

                    break;
                }
            }

            return new ExpressionStatement(Expression.ParseSingleExpr(parent));
        }

        /// <summary>
        ///     <c>
        ///         stmt:
        ///             single_stmt {';' single_stmt} ';' [ (terminator | NEWLINE)
        ///     </c>
        /// </summary>
        internal static NodeList<Statement> ParseStmt(
            SyntaxTreeNode parent,
            TokenType      terminator = TokenType.None
        ) {
            var statements = new NodeList<Statement>(parent) {
                ParseSingleStmt(parent)
            };
            if (parent.MaybeEat(TokenType.Semicolon)) {
                while (parent.Token.Is(TokenType.Semicolon)
                       && !parent.MaybeEatNewline()
                       && !parent.Peek.Is(terminator)) {
                    statements.Add(ParseSingleStmt(parent));
                    if (parent.MaybeEat(TokenType.End)) {
                        if (terminator != TokenType.None) {
                            parent.Eat(terminator);
                        }

                        // else EOC implies a new line
                        break;
                    }

                    if (!parent.MaybeEat(TokenType.Semicolon)) {
                        parent.Eat(TokenType.Newline);
                    }
                }
            }

            return statements;
        }

        /// <summary>
        ///     <c>
        ///         decorators:
        ///             ('[' decorator {',' decorator} ']')+
        ///         decorator:
        ///             name ['(' [arg_list [',']] ')']
        ///     </c>
        /// </summary>
        private static NodeList<Expression> ParseDecorators(SyntaxTreeNode parent) {
            var decorators = new NodeList<Expression>(parent);

            while (parent.MaybeEat(TokenType.OpenBracket)) {
                do {
                    // on '['
                    Expression decorator = Expression.ParseSingleExpr(parent);
                    decorators.Add(decorator);
                } while (parent.MaybeEat(TokenType.Comma));

                parent.Eat(TokenType.CloseBracket);
            }

            return decorators;
        }
    }
}