using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Axion.Core.Processing.Errors;
using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Processing.Syntax.Tree;
using Axion.Core.Processing.Syntax.Tree.Comprehensions;
using Axion.Core.Processing.Syntax.Tree.Expressions;
using Axion.Core.Processing.Syntax.Tree.Statements;
using Axion.Core.Specification;

namespace Axion.Core.Processing.Syntax.Parser {
    public partial class SyntaxParser {
        #region Properties

        /// <summary>
        ///     Contains all errors that raised during syntax analysis.
        /// </summary>
        private readonly List<Exception> blames;

        /// <summary>
        ///     Current processing stream.
        /// </summary>
        private readonly TokenStream stream;

        /// <summary>
        ///     Outgoing Abstract Syntax Tree.
        /// </summary>
        private readonly Ast ast;

        /// <summary>
        ///     Start position of current token in stream.
        /// </summary>
        private Position tokenStart => stream.Token.Span.StartPosition;

        /// <summary>
        ///     End position of current token in stream.
        /// </summary>
        private Position tokenEnd => stream.Token.Span.EndPosition;

        private bool inLoop, inFinally, inFinallyLoop;

        #endregion

        internal SyntaxParser(
            string          code,
            List<Token>     tokens,
            Ast             outAst,
            List<Exception> outBlames
        ) {
            stream = new TokenStream(this, tokens, code);
            ast    = outAst ?? throw new ArgumentNullException(nameof(outAst));
            blames = outBlames ?? new List<Exception>();
        }

        internal void Process(bool returnValue) {
            if (stream.Tokens.Count == 0) {
                return;
            }
            stream.MaybeEatNewline();

            var statements = new List<Statement>();

            while (!stream.MaybeEat(TokenType.EndOfStream)) {
                if (stream.MaybeEatNewline()) {
                    continue;
                }
                statements.Add(ParseStmt());
            }

            // wrap last expression in return
            // to allow user to write no 'return' keyword
            if (returnValue && statements.Count > 0) {
                if (statements[statements.Count - 1] is ExpressionStatement exprStmt) {
                    statements[statements.Count - 1] = new ReturnStatement(exprStmt.Expression);
                }
            }
            ast.Root = new BlockStatement(statements.ToArray());
        }

        private Token StartNewStmt(TokenType type) {
            stream.Eat(type);
            return stream.Token;
        }

        /// <summary>
        /// <c>
        ///     primary:
        ///         atom | trailer
        ///     atom:
        ///         ID
        ///         | LITERAL
        ///         | parenthesis_expr
        ///         | list_display
        ///         | map_or_set_display
        /// </c>
        /// </summary>
        private Expression ParsePrimaryExpr() {
            Token token = stream.Peek;
            switch (token.Type) {
                case TokenType.Identifier: {
                    stream.NextToken();
                    return new NameExpression(token);
                }
                case TokenType.LeftParenthesis: {
                    return ParsePrimaryInParenthesis();
                }
                case TokenType.LeftBracket: {
                    return ParsePrimaryInBrackets();
                }
                case TokenType.LeftBrace: {
                    return ParseMapOrSetDisplay();
                }
                default: {
                    if (Spec.ConstantValueTypes.Contains(token.Type)) {
                        stream.NextToken();
                        // TODO add pre-concatenation of literals
                        return new ConstantExpression(token);
                    }
                    ReportError(Spec.ERR_PrimaryExpected, token);
                    return Error();
                }
            }
        }

        #region Primary

        /// <summary>
        /// <c>
        ///     parenthesis_expr
        ///     | yield_expr
        ///     | (expression (',' expression)* [','] ) # tuple
        ///     | generator_expression
        /// </c>
        /// </summary>
        private Expression ParsePrimaryInParenthesis() {
            Position start = StartNewStmt(TokenType.LeftParenthesis).Span.StartPosition;

            Expression result;
            if (stream.MaybeEat(TokenType.RightParenthesis)) {
                result = new TupleExpression(false, new Expression[0]);
            }
            // yield_expr
            else if (stream.MaybeEat(TokenType.KeywordYield)) {
                result = ParseYield();
            }
            else {
                Expression expr = ParseTestExpr();
                // tuple
                if (stream.MaybeEat(TokenType.Comma)) {
                    // '(' expression ',' ...
                    List<Expression> list = ParseExpressionList(out bool trailingComma);
                    list.Insert(0, expr);
                    result = MakeTupleOrExpr(list, trailingComma);
                }
                // generator_expression
                else if (stream.PeekIs(TokenType.KeywordFor)) {
                    // '(' expression 'for' ...
                    result = ParseGeneratorExpr(expr);
                }
                // parenthesis_expr
                else {
                    // '(' expression ')'
                    result = expr is ParenthesisExpression
                                 ? expr
                                 : new ParenthesisExpression(expr);
                }
            }
            stream.Eat(TokenType.RightParenthesis);
            result.MarkPosition(start, tokenEnd);
            return result;
        }

        /// <summary>
        /// <c>
        ///     list_display | subscription
        ///     list_display:
        ///         '[' expression ( comprehension_iterator | (',' expression)* [','] ) ']'
        /// </c>
        /// </summary>
        private Expression ParsePrimaryInBrackets() {
            Position start = StartNewStmt(TokenType.LeftBracket).Span.StartPosition;

            var expressions = new List<Expression>();
            if (!stream.MaybeEat(TokenType.RightBracket)) {
                expressions.Add(ParseTestExpr());
                if (stream.MaybeEat(TokenType.Comma)) {
                    expressions.AddRange(ParseTestList());
                }
                else if (stream.PeekIs(TokenType.KeywordFor)) {
                    ComprehensionIterator[] iterators = ParseComprehensionIterators();
                    stream.Eat(TokenType.RightBracket);
                    return new ListComprehension(expressions[0], iterators, start, tokenEnd);
                }
                stream.Eat(TokenType.RightBracket);
            }
            return new ListExpression((start, tokenEnd), expressions);
        }

        /// <summary>
        /// <c>
        ///     map_or_set_display:
        ///         '{' [map_or_set_initializer] '}'
        ///     map_or_set_initializer:
        ///         ( (test ':' test (comprehension_for | (',' test ':' test)* [',']))
        ///         | (test (comprehension_for | (',' test)* [','])) )
        /// </c>
        /// </summary>
        private Expression ParseMapOrSetDisplay() {
            Position start = StartNewStmt(TokenType.LeftBrace).Span.StartPosition;

            List<SliceExpression> mapMembers = null;
            List<Expression>      setMembers = null;

            while (!stream.MaybeEat(TokenType.RightBrace)) {
                var        first     = false;
                Expression itemPart1 = ParseTestExpr();

                // map item (expr : expr)
                if (stream.MaybeEat(TokenType.Colon)) {
                    if (setMembers != null) {
                        ReportError("Single expression expected", stream.Token);
                    }
                    else if (mapMembers == null) {
                        mapMembers = new List<SliceExpression>();
                        first      = true;
                    }
                    Expression itemPart2 = ParseTestExpr();
                    // generator: { key: value for (key, value) in iterable }
                    if (stream.PeekIs(TokenType.KeywordFor)) {
                        if (!first) {
                            ReportError("Generator can only be used as single map item", stream.Token);
                        }
                        return FinishMapComprehension(itemPart1, itemPart2, start);
                    }
                    mapMembers?.Add(new SliceExpression(itemPart1, itemPart2, null));
                }
                // set item (expr)
                else {
                    if (mapMembers != null) {
                        ReportError("'Key : Value' expression expected", stream.Token);
                    }
                    else if (setMembers == null) {
                        setMembers = new List<Expression>();
                        first      = true;
                    }
                    // generator: { x * 2 for x in { 1, 2, 3 } }
                    if (stream.PeekIs(TokenType.KeywordFor)) {
                        if (!first) {
                            ReportError("Generator can only be used as single set item", stream.Token);
                        }
                        return FinishSetComprehension(itemPart1, start);
                    }
                    setMembers?.Add(itemPart1);
                }

                if (!stream.MaybeEat(TokenType.Comma)) {
                    stream.Eat(TokenType.RightBrace);
                    break;
                }
            }

            if (mapMembers == null && setMembers != null) {
                return new SetExpression(setMembers.ToArray(), start, tokenEnd);
            }
            return new MapExpression(mapMembers?.ToArray() ?? new SliceExpression[0], start, tokenEnd);
        }

        #endregion

        /// <summary>
        /// <c>
        ///     trailer:
        ///         call
        ///         | '[' subscription_list ']'
        ///         | member
        ///     call:
        ///         '(' [args_list | comprehension_iterator] ')'
        ///     member:
        ///         '.' ID
        /// </c>
        /// </summary>
        private Expression ParseTrailingExpr(Expression result, bool allowGeneratorExpression) {
            while (true) {
                if (stream.PeekIs(TokenType.LeftParenthesis)) {
                    if (!allowGeneratorExpression) {
                        return result;
                    }

                    stream.NextToken();
                    Arg[] args = FinishGeneratorOrArgList();
                    result = args != null
                                 ? FinishCallExpr(result, args)
                                 : new CallExpression(result, new Arg[0], tokenEnd);
                }
                else if (stream.PeekIs(TokenType.LeftBracket)) {
                    result = new IndexExpression(result, ParseSubscriptList());
                }
                else if (stream.MaybeEat(TokenType.Dot)) {
                    result = new MemberExpression(result, ParseName());
                }
                else if (stream.PeekIs(Spec.ConstantValueTypes)) {
                    // abc 1, abc "", abc 1L, abc 0j
                    ReportError(Spec.ERR_InvalidStatement, stream.Peek);
                    return Error();
                }
                else {
                    return result;
                }
            }
        }

        private CallExpression FinishCallExpr(Expression target, params Arg[] args) {
//            var hasArgsTuple  = false;
//            var hasKeywordMap = false;
//            var keywordCount  = 0;
//
//            foreach (Arg arg in args) {
//                if (arg.Name.Name == null) {
//                    if (hasArgsTuple || hasKeywordMap || keywordCount > 0) {
//                        ReportSyntaxError(IronPython.Resources.NonKeywordAfterKeywordArg);
//                    }
//                }
//                else if (arg.Name.Name == "*") {
//                    if (hasArgsTuple || hasKeywordMap) {
//                        ReportSyntaxError(IronPython.Resources.OneListArgOnly);
//                    }
//                    hasArgsTuple = true;
//                    extraArgs++;
//                }
//                else if (arg.Name.Name == "**") {
//                    if (hasKeywordMap) {
//                        ReportSyntaxError(IronPython.Resources.OneKeywordArgOnly);
//                    }
//                    hasKeywordMap = true;
//                    extraArgs++;
//                }
//                else {
//                    if (hasKeywordMap) {
//                        ReportSyntaxError(IronPython.Resources.KeywordOutOfSequence);
//                    }
//                    keywordCount++;
//                }
//            }

            return new CallExpression(target, args, tokenEnd);
        }

        /// <summary>
        /// <c>
        ///     subscriptions_list:
        ///         subscription (',' subscription)* [',']
        ///     subscription:
        ///         expression | slice
        ///     slice:
        ///         [expression] ":" [expression] [ ":" [expression] ]
        ///     ellipsis:
        ///         '...'
        /// </c>
        /// </summary>
        private Expression ParseSubscriptList() {
            stream.Eat(TokenType.LeftBracket);
            bool trailingComma;

            var expressions = new List<Expression>();
            while (true) {
                Expression expr;
                // TODO do something with ellipsis
//                if (stream.MaybeEat(TokenType.OpDot)) {
//                    Position start = tokenStart;
//                    stream.Eat(TokenType.OpDot);
//                    stream.Eat(TokenType.OpDot);
//                    e = new ConstantExpression(Ellipsis.Name);
//                    e.MarkPosition(start, tokenEnd);
//                }
//                else
                if (stream.MaybeEat(TokenType.Colon)) {
                    expr = FinishSlice(null);
                }
                else {
                    expr = ParseTestExpr();
                    if (stream.MaybeEat(TokenType.Colon)) {
                        expr = FinishSlice(expr);
                    }
                }

                expressions.Add(expr);

                if (stream.MaybeEat(TokenType.Comma)) {
                    trailingComma = true;
                    if (stream.MaybeEat(TokenType.RightBracket)) {
                        break;
                    }
                }
                else {
                    stream.Eat(TokenType.RightBracket);
                    trailingComma = false;
                    break;
                }
            }

            Expression FinishSlice(Expression startExpr) {
                Expression stop = null;
                Expression step = null;

                switch (stream.Peek.Type) {
                    case TokenType.Comma:
                    case TokenType.RightBracket:
                        break;
                    case TokenType.Colon:
                        // x[?::?]
                        stream.NextToken();
                        ParseSliceStep();
                        break;
                    default:
                        // x[?:val:?]
                        stop = ParseTestExpr();
                        if (stream.MaybeEat(TokenType.Colon)) {
                            ParseSliceStep();
                        }
                        break;
                }

                void ParseSliceStep() {
                    if (!stream.PeekIs(TokenType.Comma, TokenType.RightBracket)) {
                        step = ParseTestExpr();
                    }
                }

                return new SliceExpression(startExpr, stop, step);
            }

            return MakeTupleOrExpr(expressions, trailingComma, true);
        }

        /// <summary>
        /// <c>
        ///     generator_expression:
        ///         '(' expression comp_for ')'
        /// </c>
        /// "for" has NOT been stream.Eaten before entering this method
        /// </summary>
        private Expression ParseGeneratorExpr(Expression expr) {
            var comprehensions = new List<ComprehensionIterator> { ParseComprehensionFor() };
            while (true) {
                if (stream.PeekIs(TokenType.KeywordFor)) {
                    comprehensions.Add(ParseComprehensionFor());
                }
                else if (stream.PeekIs(TokenType.KeywordIf)) {
                    comprehensions.Add(ParseComprehensionIf());
                }
                else {
                    break;
                }
            }
            return new GeneratorExpression(expr, comprehensions.ToArray());
        }

        #region Arguments list

        /// <summary>
        /// <c>
        ///     arg_list:
        ///         (expression
        ///          | expression '=' expression
        ///          | expression 'for')*
        /// </c>
        /// </summary>
        private Arg[] FinishGeneratorOrArgList() {
            if (stream.PeekIs(TokenType.RightParenthesis, TokenType.OpMultiply, TokenType.OpPower)) {
                return ParseArgumentsList();
            }
            Position   start          = tokenStart;
            Expression argNameOrValue = ParseTestExpr();
            if (argNameOrValue is ErrorExpression) {
                return new Arg[0];
            }
            var generator = false;
            Arg arg;
            if (stream.MaybeEat(TokenType.Assign)) {
                // Keyword argument
                arg = FinishKeywordArgument(argNameOrValue);
            }
            else if (stream.PeekIs(TokenType.KeywordFor)) {
                // Generator expression
                arg       = new Arg(ParseGeneratorExpr(argNameOrValue));
                generator = true;
            }
            else {
                arg = new Arg(argNameOrValue);
            }

            // Was this all?
            if (!generator && stream.MaybeEat(TokenType.Comma)) {
                return ParseArgumentsList(arg);
            }

            stream.Eat(TokenType.RightParenthesis);
            arg.MarkPosition(start, tokenEnd);
            return new[] { arg };
        }

        private Arg FinishKeywordArgument(Expression expr) {
            if (expr is NameExpression name) {
                Expression value = ParseTestExpr();
                return new Arg(name, value);
            }
            BlameInvalidSyntax(TokenType.Identifier, expr);
            return new Arg(expr);
        }

        private void CheckUniqueArgument(List<Arg> arguments, Arg arg) {
            if (arg?.Name.Name == null) {
                return;
            }
            for (var i = 0; i < arguments.Count; i++) {
                if (arguments[i].Name.Name == arg.Name.Name) {
                    Blame(BlameType.DuplicatedKeywordArgument, arg);
                }
            }
        }

        /// <summary>
        /// <c>
        ///     arg_list:
        ///         (argument ',')* (argument [',']| '*' expression [',' '**' expression] | '**' expression)
        ///     argument:
        ///         [expression '='] expression
        /// </c>
        /// Really [keyword '='] expression
        /// </summary>
        private Arg[] ParseArgumentsList(Arg first = null) {
            var arguments = new List<Arg>();

            if (first != null) {
                arguments.Add(first);
            }

            while (!stream.MaybeEat(TokenType.RightParenthesis)) {
                NameExpression name = ParseName();
                stream.NextToken();
                Arg arg;

                if (stream.MaybeEat(TokenType.OpMultiply)) {
                    arg = new Arg(name, ParseTestExpr());
                }
                else {
                    if (stream.MaybeEat(TokenType.Assign)) {
                        arg = FinishKeywordArgument(name);
                        CheckUniqueArgument(arguments, arg);
                    }
                    else {
                        arg = new Arg(name);
                    }
                }
                arguments.Add(arg);
                if (!stream.MaybeEat(TokenType.Comma)) {
                    stream.Eat(TokenType.RightParenthesis);
                    break;
                }
            }

            return arguments.ToArray();
        }

        #endregion

        private NameExpression ParseName(bool allowQualified = false) {
            var nameParts = new List<Token>();
            do {
                stream.Eat(TokenType.Identifier);
                nameParts.Add(stream.Token);
            } while (allowQualified && stream.MaybeEat(TokenType.Dot));
            return new NameExpression(nameParts.ToArray());
        }

        #region Errors reporting

        private ErrorExpression Error() {
            return new ErrorExpression(tokenStart, tokenEnd);
        }

        private ExpressionStatement ErrorStmt() {
            return new ExpressionStatement(Error());
        }

        internal bool CheckUnexpectedEOS() {
            if (stream.PeekIs(TokenType.EndOfStream)) {
                Blame(BlameType.UnexpectedEndOfCode, stream.Token);
                return true;
            }
            return false;
        }

        internal void BlameInvalidSyntax(TokenType expectedType, SpannedRegion mark) {
            ReportError("Invalid syntax, '" + expectedType.GetValue() + "' expected.", mark);
        }

        internal void ReportError(string message, SpannedRegion mark) {
            blames.Add(new LanguageException(new Blame(message, BlameSeverity.Error, mark.Span), stream.Source));
        }

        internal void ReportWarning(string message, SpannedRegion mark) {
            blames.Add(new LanguageException(new Blame(message, BlameSeverity.Warning, mark.Span), stream.Source));
        }

        internal void Blame(BlameType type, SpannedRegion region) {
            Blame(type, region.Span.StartPosition, region.Span.EndPosition);
        }

        internal void Blame(BlameType type, Span span) {
            Blame(type, span.StartPosition, span.EndPosition);
        }

        internal void Blame(BlameType type, Position start, Position end) {
            Debug.Assert(type != BlameType.None);

            blames.Add(new LanguageException(new Blame(type, Spec.Blames[type], start, end), stream.Source));
        }

        #endregion
    }
}