using Axion.Core.Processing.Errors;
using Axion.Core.Processing.Syntactic.Expressions;
using Axion.Core.Processing.Syntactic.Statements.Definitions;
using Axion.Core.Processing.Syntactic.Statements.Interfaces;
using Axion.Core.Processing.Syntactic.Statements.Small;
using Axion.Core.Specification;
using static Axion.Core.Specification.TokenType;

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
        private static Statement ParseSingleStmt(SyntaxTreeNode parent) {
            switch (parent.Peek.Type) {
                case KeywordIf:
                case KeywordUnless: {
                    return new ConditionalStatement(parent);
                }

                case KeywordDo: {
                    return new WhileStatement(parent, true);
                }

                case KeywordWhile: {
                    return new WhileStatement(parent);
                }

                case KeywordFor: {
                    return LoopStatement.ParseFor(parent);
                }

                case KeywordTry: {
                    return new TryStatement(parent);
                }

                case KeywordWith: {
                    return new WithStatement(parent);
                }

                #region Small statements

                case KeywordAssert: {
                    return new AssertStatement(parent);
                }

                case Semicolon:
                case KeywordPass: {
                    return new EmptyStatement(parent);
                }

                case KeywordBreak: {
                    return new BreakStatement(parent);
                }

                case KeywordContinue: {
                    return new ContinueStatement(parent);
                }

                case KeywordDelete: {
                    return new DeleteStatement(parent);
                }

                case KeywordRaise: {
                    return new RaiseStatement(parent);
                }

                case KeywordReturn: {
                    return new ReturnStatement(parent);
                }

                case KeywordYield: {
                    // For yield statements, continue to enforce that it's currently in a function. 
                    // This gives us better syntax error reporting for yield-statements than for yield-expressions.
                    if (parent.Ast.CurrentFunction == null) {
                        parent.Unit.Blame(BlameType.MisplacedYield, parent.Token);
                    }

                    return new ExpressionStatement(new YieldExpression(parent));
                }

                #endregion

                case OpenBracket: {
                    NodeList<Expression> decorators = ParseDecorators(parent);

                    Statement stmt = ParseSingleStmt(parent);
                    if (stmt is IDecorated def) {
                        def.Modifiers = decorators;
                    }
                    else {
                        parent.Unit.Blame(BlameType.InvalidDecoratorPlacement, stmt);
                    }

                    return stmt;
                }

                case KeywordModule: {
                    return new ModuleDefinition(parent);
                }

                case KeywordClass: {
                    return new ClassDefinition(parent);
                }

                case KeywordEnum: {
                    return new EnumDefinition(parent);
                }

                case KeywordFn: {
                    return new FunctionDefinition(parent);
                }
            }

            return new ExpressionStatement(Expression.ParseGlobalExpr(parent));
        }

        /// <summary>
        ///     <c>
        ///         stmt:
        ///             single_stmt {';' single_stmt} ';' [ (terminator | NEWLINE)
        ///     </c>
        /// </summary>
        internal static NodeList<Statement> ParseStmt(
            SyntaxTreeNode parent,
            TokenType      terminator = None
        ) {
            var statements = new NodeList<Statement>(parent) {
                ParseSingleStmt(parent)
            };
            if (parent.MaybeEat(Semicolon)) {
                while (parent.Token.Is(Semicolon)
                       && !parent.MaybeEatNewline()
                       && !parent.Peek.Is(terminator, End)) {
                    statements.Add(ParseSingleStmt(parent));
                    if (parent.MaybeEat(End)) {
                        if (terminator != None) {
                            parent.Eat(terminator);
                        }

                        // else EOC implies a new line
                        break;
                    }

                    if (!parent.MaybeEat(Semicolon)) {
                        parent.Eat(Newline);
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

            while (parent.MaybeEat(OpenBracket)) {
                do {
                    decorators.Add(Expression.ParsePreGlobalExpr(parent));
                } while (parent.MaybeEat(Comma));

                parent.Eat(CloseBracket);
            }

            Expression.CheckType(decorators, Spec.DecoratorExprs);

            return decorators;
        }
    }
}