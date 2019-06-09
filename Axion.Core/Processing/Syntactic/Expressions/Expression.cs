using System;
using System.Collections.Generic;
using System.Linq;
using Axion.Core.Processing.Errors;
using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Processing.Syntactic.Expressions.Atomic;
using Axion.Core.Processing.Syntactic.Expressions.Binary;
using Axion.Core.Processing.Syntactic.Expressions.Definitions;
using Axion.Core.Processing.Syntactic.Expressions.Multiple;
using Axion.Core.Processing.Syntactic.Expressions.TypeNames;
using Axion.Core.Processing.Syntactic.MacroPatterns;
using Axion.Core.Specification;
using static Axion.Core.Specification.TokenType;

namespace Axion.Core.Processing.Syntactic.Expressions {
    /// <summary>
    ///     <c>
    ///         expr_list:
    ///             expr {',' expr}
    ///         infix_list:
    ///             infix_expr {',' infix_expr}
    ///         preglobal_list:
    ///             preglobal_expr {',' preglobal_expr}
    ///         simple_name_list:
    ///             simple_name_expr {',' simple_name_expr}
    ///
    ///         single_stmt:
    ///             cond_stmt | while_stmt | for_stmt    |
    ///             try_stmt  | with_stmt  | import_stmt |
    ///             decorated
    ///         decorated:
    ///             module_def | class_def  | enum_def |
    ///             func_def   | small_stmt
    ///         small_stmt:
    ///             pass_stmt | expr_stmt | flow_stmt
    ///         flow_stmt:
    ///             break_stmt | continue_stmt | return_stmt |
    ///             raise_stmt | yield_stmt
    ///     </c>
    /// </summary>
    public abstract class Expression : AstNode {
        public Expression(AstNode parent) : base(parent) { }
        public Expression() { }

        /// <summary>
        ///     <c>
        ///         atom
        ///             : name
        ///             | await_expr
        ///             | yield_expr
        ///             | type_initializer_expr
        ///             | parenthesis_expr
        ///             | list_expr
        ///             | hash_collection
        ///             | CONSTANT
        ///     </c>
        /// </summary>
        internal static Expression ParseAtomExpr(AstNode parent) {
            Expression value;
            switch (parent.Peek.Type) {
            case Identifier: {
                value = NameExpression.ParseName(parent);
                break;
            }
            case KeywordIf: {
                return new ConditionalExpression(parent);
            }

            case KeywordWhile: {
                return new WhileExpression(parent);
            }

            #region Small statements

            case Semicolon:
            case KeywordPass: {
                return new EmptyExpression(parent);
            }

            case KeywordBreak: {
                return new BreakExpression(parent);
            }

            case KeywordContinue: {
                return new ContinueExpression(parent);
            }

            case KeywordReturn: {
                return new ReturnExpression(parent);
            }

            case KeywordAwait: {
                value = new AwaitExpression(parent);
                break;
            }

            case KeywordYield: {
                value = new YieldExpression(parent);
                break;
            }

            #endregion

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

            case OpenBrace when parent.Ast.MacroExpectationType != typeof(BlockExpression): {
                value = new BraceCollectionExpression(parent);
                break;
            }

            case Indent:
            case OpenBrace:
            case Colon when parent.Ast.MacroExpectationType == typeof(BlockExpression): {
                value = new BlockExpression(parent);
                break;
            }

            case DoubleOpenBrace: {
                value = new CodeQuoteExpression(parent);
                break;
            }

            case OpenParenthesis: {
                Token start = parent.Token;
                parent.Eat(OpenParenthesis);
                // empty tuple
                if (parent.MaybeEat(CloseParenthesis)) {
                    return new TupleExpression(parent, start, parent.Token);
                }

                value = ParseMultiple(parent, parens: true);
                parent.Eat(CloseParenthesis);
                break;
            }

            default: {
                if (Spec.Constants.Contains(parent.Peek.Type)) {
                    parent.GetNext();
                    // TODO add pre-concatenation of literals
                    value = new ConstantExpression(parent);
                }
                else {
                    value = ParseMacroApplication(parent);
                }

                break;
            }
            }

            return value;
        }

        private static Expression ParseMacroApplication(AstNode parent) {
            int startIdx = parent.Ast.Index;
            MacroDefinition matchedMacro =
                parent.Ast.Macros.FirstOrDefault(macro => macro.Syntax.Match(parent));

            if (matchedMacro == null) {
                return new UnknownExpression(parent, startIdx);
            }
            return new MacroApplicationExpression(
                parent,
                matchedMacro
            );
        }

        private static Expression ParseInfixMacroApplication(
            AstNode         parent,
            MacroDefinition matchedInfixMacro,
            Expression      infixLeft = null
        ) {
            int startIdx = parent.Ast.Index;
            parent.GetNext();
            parent.Ast.MacroApplicationParts.Add(parent.Token);
            var restCascade =
                new CascadePattern(matchedInfixMacro.Syntax.Patterns.Skip(2).ToArray());
            if (restCascade.Match(parent)) {
                return new MacroApplicationExpression(
                    parent,
                    matchedInfixMacro,
                    infixLeft
                );
            }
            return new UnknownExpression(parent, startIdx);
        }

        /// <summary>
        ///     <c>
        ///         suffix_expr:
        ///             (pipeline | { member | call_expr | index_expr })
        ///             ['++' | '--']
        ///         pipeline:
        ///             atom {'|>' atom }
        ///     </c>
        /// </summary>
        internal static Expression ParseSuffixExpr(AstNode parent) {
            Expression value = ParseAtomExpr(parent);
            if (parent.MaybeEat(RightPipeline)) {
                do {
                    value = new FunctionCallExpression(
                        parent,
                        ParseSuffix(ParseAtomExpr(parent)),
                        new CallArgument(parent, value)
                    );
                } while (parent.MaybeEat(RightPipeline));

                return value;
            }

            Expression ParseSuffix(Expression result) {
                var loop = true;
                while (loop) {
                    switch (parent.Peek.Type) {
                    case Dot: {
                        result = new MemberAccessExpression(parent, result);
                        break;
                    }
                    case OpenParenthesis: {
                        result = new FunctionCallExpression(parent, result, true);
                        break;
                    }
                    case OpenBracket: {
                        result = new IndexerExpression(parent, result);
                        break;
                    }
                    default: {
                        loop = false;
                        break;
                    }
                    }
                }

                if (parent.MaybeEat(OpIncrement, OpDecrement)) {
                    var op = (OperatorToken) result.Token;
                    op.Properties.InputSide = InputSide.Right;
                    result                  = new UnaryExpression(parent, op, result);
                }

                return result;
            }

            return ParseSuffix(value);
        }

        /// <summary>
        ///     <c>
        ///         prefix_expr:
        ///             (PREFIX_OPERATOR prefix_expr) | suffix_expr
        ///     </c>
        /// </summary>
        internal static Expression ParsePrefixExpr(AstNode parent) {
            if (parent.MaybeEat(Spec.PrefixOperators)) {
                var op = (OperatorToken) parent.Token;
                op.Properties.InputSide = InputSide.Right;
                return new UnaryExpression(parent, op, ParsePrefixExpr(parent));
            }

            return ParseSuffixExpr(parent);
        }

        /// <summary>
        ///     <c>
        ///         infix_expr:
        ///             prefix_expr (ID | SYMBOL) infix_expr
        ///     </c>
        /// </summary>
        internal static Expression ParseInfixExpr(AstNode parent) {
            Expression ParseInfix(int precedence) {
                Expression expr = ParsePrefixExpr(parent);
                if (parent.Token.Type == Newline) {
                    return expr;
                }
                MacroDefinition infixMacro = parent.Ast.Macros.FirstOrDefault(
                    m => m.Syntax.Patterns.Length > 1
                         && m.Syntax.Patterns[1] is TokenPattern
                             t
                         && t.Value == parent.Peek.Value
                );
                // infix macro check
                // expr (keyword | expr) expr?
                if (infixMacro != null) {
                    return ParseInfixMacroApplication(parent, infixMacro, expr);
                }
                while (parent.Peek is OperatorToken || parent.Peek.Is(Identifier)) {
                    parent.GetNext();
                    if (parent.Token.Is(Identifier)) {
                        expr = new BinaryExpression(
                            parent,
                            expr,
                            parent.Token,
                            ParseInfix(4)
                        );
                        continue;
                    }

                    var op = (OperatorToken) parent.Token;
                    if (op.Properties.Precedence < precedence) {
                        break;
                    }

                    expr = new BinaryExpression(
                        parent,
                        expr,
                        op,
                        ParseInfix(op.Properties.Precedence + 1)
                    );
                }

                if (parent.Peek.Is(KeywordIf, KeywordUnless)
                    && !parent.Token.Is(Newline, Outdent)) {
                    return new ConditionalInfixExpression(parent, expr);
                }
                return expr;
            }

            return ParseInfix(0);
        }

        /// <summary>
        ///     <c>
        ///         var_expr
        ///                 : infix_list
        ///                 | (['let'] assignable
        ///                    [':' type]
        ///                    ['=' infix_list])
        ///     </c>
        /// </summary>
        internal static Expression ParseVarExpr(AstNode parent) {
            bool isImmutable = parent.MaybeEat(KeywordLet);

            Expression expr = ParseMultiple(parent, node => ParseInfixExpr(node));

            // ['let'] name '=' expr
            if (expr is BinaryExpression bin
                && bin.Left is SimpleNameExpression name
                && bin.Operator.Is(OpAssign)
                && !bin.ParentBlock.HasVariable(name)) {
                return new VariableDefinitionExpression(
                    parent,
                    bin.Left,
                    null,
                    bin.Right,
                    isImmutable
                );
            }

            // check for ':' - starting block instead of var definition
            if ((parent.Ast.MacroExpectationType?.IsInstanceOfType(typeof(BlockExpression))
                 ?? false)
                || !parent.Peek.Is(Colon)) {
                return expr;
            }

            if (!Spec.VariableLeftExprs.Contains(expr.GetType())) {
                parent.Unit.Blame(BlameType.ThisExpressionTargetIsNotAssignable, expr);
            }

            TypeName   type  = null;
            Expression value = null;
            if (parent.MaybeEat(Colon)) {
                type = TypeName.ParseTypeName(parent);
            }

            if (parent.MaybeEat(OpAssign)) {
                value = ParseMultiple(parent, expectedTypes: Spec.InfixExprs);
            }

            return new VariableDefinitionExpression(parent, expr, type, value, isImmutable);
        }

        /// <summary>
        ///     <c>
        ///         ['('] %expr {',' %expr} [')']
        ///     </c>
        ///     Helper for parsing multiple comma-separated
        ///     expressions with optional parenthesis
        ///     (e.g. tuples)
        /// </summary>
        internal static Expression ParseMultiple(
            AstNode                   parent,
            Func<AstNode, Expression> parserFunc = null,
            bool                      parens     = false,
            params Type[]             expectedTypes
        ) {
            parserFunc = parserFunc ?? ParseVarExpr;
            if (expectedTypes.Length == 0 || parserFunc == ParseVarExpr) {
                expectedTypes = Spec.GlobalExprs;
            }
            var list = new NodeList<Expression>(parent) {
                parserFunc(parent)
            };

            if (parens && parent.Peek.Is(CloseParenthesis)) {
                return list[0];
            }

            // tuple
            if (parent.MaybeEat(Comma)) {
                do {
                    list.Add(parserFunc(parent));
                } while (parent.MaybeEat(Comma));
            }
            // generator | comprehension
            else if (parent.Peek.Is(KeywordFor) && parent.Token.Type != Newline) {
                list[0] = new ForComprehension(parent, list[0]);
                if (parens) {
                    list[0] = new GeneratorExpression(parent, (ForComprehension) list[0]);
                }
            }

            CheckType(list, expectedTypes);

            if (parens && list.Count == 1) {
                return new ParenthesizedExpression(list[0]);
            }

            return MaybeTuple(parent, list);
        }

        /// <summary>
        ///     <c>
        ///         cascade:
        ///             expr [{';' expr} ';'] [terminator | NEWLINE]
        ///     </c>
        /// </summary>
        internal static NodeList<Expression> ParseCascade(
            AstNode   parent,
            TokenType terminator = None
        ) {
            var items = new NodeList<Expression>(parent) {
                ParseVarExpr(parent)
            };
            if (parent.MaybeEat(Semicolon)) {
                while (parent.Token.Is(Semicolon)
                       && !parent.MaybeEatNewline()
                       && !parent.Peek.Is(terminator, End)) {
                    items.Add(ParseVarExpr(parent));
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

            return items;
        }

        internal static Expression MaybeTuple(
            AstNode              parent,
            NodeList<Expression> expressions
        ) {
            if (expressions.Count == 1) {
                return expressions[0];
            }

            return new TupleExpression(parent, expressions);
        }

        /// <summary>
        ///     Checks if every expression in collection
        ///     belong to any of <paramref name="expectedTypes"/>.
        /// </summary>
        internal static void CheckType(
            IEnumerable<Expression> expressions,
            params Type[]           expectedTypes
        ) {
            if (expectedTypes.Length == 0) {
                return;
            }

            foreach (Expression e in expressions) {
                CheckType(e);
            }
        }

        /// <summary>
        ///     Checks if expression
        ///     belongs to any of <paramref name="expectedTypes"/>.
        /// </summary>
        internal static void CheckType(
            Expression    expr,
            params Type[] expectedTypes
        ) {
            if (expectedTypes.Length == 0) {
                return;
            }

            Type itemType = expr.GetType();
            if (expectedTypes.Contains(itemType)) {
                return;
            }

            if (Spec.ExprGroupNames.ContainsKey(expectedTypes)) {
                expr.Unit.ReportError(
                    "Expected "
                    + Spec.ExprGroupNames[expectedTypes]
                    + ", got "
                    + Utilities.GetExprFriendlyName(itemType.Name),
                    expr
                );
            }
            else {
                expr.Unit.ReportError(
                    "Expected "
                    + Utilities.GetExprFriendlyName(expectedTypes[0].Name)
                    + ", got "
                    + Utilities.GetExprFriendlyName(itemType.Name),
                    expr
                );
            }
        }
    }
}