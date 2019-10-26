using System;
using System.Linq;
using Axion.Core.Processing.Errors;
using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Processing.Syntactic.Expressions;
using Axion.Core.Processing.Syntactic.Expressions.Atomic;
using Axion.Core.Processing.Syntactic.Expressions.Definitions;
using Axion.Core.Processing.Syntactic.Expressions.Operations;
using Axion.Core.Processing.Syntactic.Expressions.Postfix;
using Axion.Core.Processing.Syntactic.Expressions.TypeNames;
using Axion.Core.Specification;
using static Axion.Core.Processing.Lexical.Tokens.TokenType;

namespace Axion.Core.Processing.Syntactic {
    public static class Parsing {
        /// <summary>
        ///     <c>
        ///         ['('] %expr {',' %expr} [')'];
        ///     </c>
        ///     Helper for parsing multiple comma-separated
        ///     expressions with optional parenthesis
        ///     (e.g. tuples)
        /// </summary>
        internal static Expr ParseMultiple(
            Expr             parent,
            Func<Expr, Expr> parserFunc = null,
            params Type[]    expectedTypes
        ) {
            TokenStream s = parent.Source.TokenStream;
            parserFunc = parserFunc ?? ParseAny;
            bool parens = s.MaybeEat(OpenParenthesis);
            var list = new NodeList<Expr>(parent) {
                parserFunc(parent)
            };

            // tuple
            if (s.MaybeEat(Comma)) {
                do {
                    list.Add(parserFunc(parent));
                } while (s.MaybeEat(Comma));
            }
            // generator | comprehension
            // TODO HERE 'for' can be after 'newline', but if it's inside (), {} or []
            else if (parens && list[0] is ForComprehension fcomp) {
                s.Eat(CloseParenthesis);
                fcomp.IsGenerator = true;
                return fcomp;
            }

            if (parens) {
                s.Eat(CloseParenthesis);
                if (list.Count == 1) {
                    return new ParenthesizedExpr(list[0]);
                }
            }

            return MaybeTuple(list);
        }

        /// <summary>
        ///     <c>
        ///         var_expr
        ///             : infix_list
        ///             | (['let'] assignable
        ///                [':' type]
        ///                ['=' infix_list]);
        ///     </c>
        /// </summary>
        internal static Expr ParseAny(Expr parent) {
            TokenStream s = parent.Source.TokenStream;

            switch (s.Peek.Type) {
            case KeywordModule: {
                return new ModuleDef(parent).Parse();
            }

            case KeywordClass: {
                return new ClassDef(parent).Parse();
            }

            case KeywordFn: {
                return new FunctionDef(parent).Parse();
            }

            case KeywordIf: {
                return new ConditionalExpr(parent).Parse();
            }

            case KeywordWhile: {
                return new WhileExpr(parent).Parse();
            }

            case At: {
                return new DecoratedExpr(parent).Parse();
            }

            case Semicolon:
            case KeywordPass: {
                return new EmptyExpr(parent).Parse();
            }

            case Indent:
            case OpenBrace:
            case Colon
                when parent.Ast.MacroExpectationType == typeof(BlockExpr): {
                return new BlockExpr(parent).Parse();
            }

            case KeywordBreak: {
                return new BreakExpr(parent).Parse();
            }

            case KeywordContinue: {
                return new ContinueExpr(parent).Parse();
            }

            case KeywordReturn: {
                return new ReturnExpr(parent).Parse();
            }

            case KeywordYield: {
                return new YieldExpr(parent).Parse();
            }
            }

            bool isImmutable = s.MaybeEat(KeywordLet);

            Expr expr = ParseInfix(parent);

            // ['let'] name '=' expr
            if (expr is BinaryExpr bin
             && bin.Left is NameExpr name
             && bin.Operator.Is(OpAssign)
             && !bin.GetParentOfType<BlockExpr>().IsDefined(name.ToString())) {
                return new VarDef(
                    parent,
                    name,
                    null,
                    bin.Right,
                    isImmutable
                );
            }

            // check for ':' - starting block instead of var definition
            if (!isImmutable && !s.MaybeEat(Colon)) {
                return expr;
            }

            if (!(expr is IVarTargetExpr)) {
                LangException.Report(BlameType.RedundantEmptyListOfTypeArguments, expr);
            }

            TypeName type  = new TypeName(parent).ParseTypeName();
            Expr     value = null;
            if (s.MaybeEat(OpAssign)) {
                value = ParseInfix(parent);
            }

            return new VarDef(parent, expr, type, value, isImmutable);
        }

        /// <summary>
        ///     <c>
        ///         infix_expr:
        ///             prefix_expr (ID | SYMBOL) infix_expr;
        ///     </c>
        /// </summary>
        internal static Expr ParseInfix(Expr parent) {
            TokenStream s = parent.Source.TokenStream;

            Expr ParseInfix(int precedence) {
                Expr leftExpr = ParsePrefix(parent);
                if (leftExpr is IDefinitionExpr) {
                    return leftExpr;
                }

                // expr (keyword | expr) expr?
                MacroApplicationExpr macro = new MacroApplicationExpr(parent).Parse(leftExpr);
                if (macro.MacroDef != null) {
                    return macro;
                }

                while (true) {
                    int newPrecedence;
                    if (s.Peek is OperatorToken opToken) {
                        newPrecedence = opToken.Precedence;
                    }
                    else if (!s.Token.Is(Newline, Outdent) && s.PeekIs(Identifier)) {
                        newPrecedence = 4;
                    }
                    else {
                        break;
                    }

                    if (newPrecedence < precedence) {
                        break;
                    }

                    s.EatAny();
                    leftExpr = new BinaryExpr(
                        parent,
                        leftExpr,
                        s.Token,
                        ParseInfix(newPrecedence + 1)
                    );
                }

                if (!s.Token.Is(Newline, Outdent)) {
                    if (s.PeekIs(KeywordFor)) {
                        leftExpr = new ForComprehension(parent, leftExpr).Parse();
                    }

                    if (s.PeekIs(KeywordIf, KeywordUnless)) {
                        return new TernaryExpr(parent, trueExpr: leftExpr).Parse();
                    }
                }

                return leftExpr;
            }

            return ParseInfix(0);
        }

        /// <summary>
        ///     <c>
        ///         prefix_expr:
        ///             (PREFIX_OPERATOR prefix_expr) | suffix_expr;
        ///     </c>
        /// </summary>
        internal static Expr ParsePrefix(Expr parent) {
            TokenStream s = parent.Source.TokenStream;

            if (s.MaybeEat(Spec.PrefixOperators)) {
                var op = (OperatorToken) s.Token;
                op.Side = InputSide.Right;
                return new UnaryExpr(parent, op, ParsePrefix(parent));
            }

            return ParsePostfix(parent);
        }

        /// <summary>
        ///     <c>
        ///         suffix_expr:
        ///             atom
        ///             {'|>' atom }
        ///             | ({ member | call_expr | index_expr } ['++' | '--']));
        ///     </c>
        /// </summary>
        internal static Expr ParsePostfix(Expr parent) {
            TokenStream s = parent.Source.TokenStream;

            bool unquoted = s.MaybeEat(Dollar);
            Expr value    = ParseAtom(parent);
            if (value is IDefinitionExpr) {
                return value;
            }

            var loop = true;
            while (loop) {
                switch (s.Peek.Type) {
                case OpDot: {
                    value = new MemberAccessExpr(parent, value).Parse();
                    break;
                }

                case OpenParenthesis when !(value is ConstantExpr): {
                    value = new FuncCallExpr(parent, value).Parse(true);
                    break;
                }

                case OpenBracket: {
                    value = new IndexerExpr(parent, value).Parse();
                    break;
                }

                default: {
                    loop = false;
                    break;
                }
                }
            }

            if (s.MaybeEat(OpIncrement, OpDecrement)) {
                var op = (OperatorToken) s.Token;
                op.Side = InputSide.Right;
                value   = new UnaryExpr(parent, op, value);
            }

            if (unquoted) {
                value = new CodeUnquotedExpr(parent, value);
            }

            return value;
        }

        /// <summary>
        ///     <c>
        ///         atom
        ///             : name
        ///             | await_expr
        ///             | parenthesis_expr
        ///             | CONSTANT;
        ///     </c>
        /// </summary>
        internal static Expr ParseAtom(Expr parent) {
            TokenStream s = parent.Source.TokenStream;

            switch (s.Peek.Type) {
            case Identifier: {
                return new NameExpr(parent).Parse(true);
            }

            case KeywordAwait: {
                return new AwaitExpr(parent).Parse();
            }

            case KeywordFn: {
                return new FunctionDef(parent).Parse(true);
            }

            case DoubleOpenBrace: {
                return new CodeQuoteExpr(parent).Parse();
            }

            case OpenParenthesis: {
                // empty tuple
                if (s.PeekByIs(2, CloseParenthesis)) {
                    return new TupleExpr(parent).ParseEmpty();
                }

                return ParseMultiple(parent);
            }

            default: {
                if (Spec.Constants.Contains(s.Peek.Type)) {
                    return new ConstantExpr(parent).Parse();
                }

                MacroApplicationExpr macro = new MacroApplicationExpr(parent).Parse();
                if (macro.MacroDef != null) {
                    return macro;
                }

                break;
            }
            }

            return new UnknownExpr(parent).Parse();
        }

        internal static Expr MaybeTuple(NodeList<Expr> expressions) {
            if (expressions.Count == 1) {
                return expressions[0];
            }

            return new TupleExpr(expressions.Parent, expressions);
        }
    }
}