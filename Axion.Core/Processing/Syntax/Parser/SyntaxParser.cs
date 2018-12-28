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
        private Position tokenStart => stream.Token.Span.Start;

        /// <summary>
        ///     End position of current token in stream.
        /// </summary>
        private Position tokenEnd => stream.Token.Span.End;

        private bool allowIncomplete;

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
            ast.Root = new SuiteStatement(statements);
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
        ///         ID | LITERAL
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
                    ReportError("Expected an identifier, list, map or other primary expression.", token);
                    return new ErrorExpression(token);
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
            Position start = StartNewStmt(TokenType.LeftParenthesis).Span.Start;

            Expression result;
            if (stream.MaybeEat(TokenType.RightParenthesis)) {
                result = new TupleExpression(false, new List<Expression>());
            }
            // yield_expr
            else if (stream.MaybeEat(TokenType.KeywordYield)) {
                result = ParseYield();
                stream.Eat(TokenType.RightParenthesis);
            }
            else {
                bool prevAllow = allowIncomplete;
                allowIncomplete = true;
                try {
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
                finally {
                    allowIncomplete = prevAllow;
                }
            }

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
            Position start = StartNewStmt(TokenType.LeftBracket).Span.Start;

            var expressions = new List<Expression>();
            if (!stream.MaybeEat(TokenType.RightBracket)) {
                bool prevAllow = allowIncomplete;
                allowIncomplete = true;
                try {
                    expressions.Add(ParseTestExpr());
                    if (stream.MaybeEat(TokenType.Comma)) {
                        expressions.AddRange(ParseTestList());
                    }
                    else if (stream.PeekIs(TokenType.KeywordFor)) {
                        ComprehensionIterator[] iterators = ParseComprehensionIterators();
                        stream.Eat(TokenType.RightBracket);
                        return new ListComprehension(expressions[0], iterators);
                    }
                    stream.Eat(TokenType.RightBracket);
                }
                finally {
                    allowIncomplete = prevAllow;
                }
            }
            return new ListExpression(new Span(start, tokenEnd), expressions);
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
            Position start = StartNewStmt(TokenType.LeftBrace).Span.Start;

            List<SliceExpression> mapMembers = null;
            List<Expression>      setMembers = null;

            bool prevAllow = allowIncomplete;
            allowIncomplete = true;
            try {
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
                                ReportError("Generator can only be used as first map item", stream.Token);
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
                                ReportError("Generator can only be used as first set item", stream.Token);
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
            }
            finally {
                allowIncomplete = prevAllow;
            }

            if (mapMembers == null && setMembers != null) {
                return new SetExpression(setMembers, start, tokenEnd);
            }
            return new MapExpression(mapMembers ?? new List<SliceExpression>(), start, tokenEnd);
        }

        #endregion

        /// <summary>
        /// <c>
        ///     trailer:
        ///         call
        ///         | '[' subscription_list ']'
        ///         | attribute_ref
        ///     call:
        ///         '(' [argument_list | comprehension_iterator] ')'
        ///     attribute_ref:
        ///         '.' ID
        /// </c>
        /// </summary>
        private Expression ParseTrailingExpr(Expression result, bool allowGeneratorExpression) {
            bool prevAllow = allowIncomplete;
            allowIncomplete = true;
            try {
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
                    else if (stream.PeekIs(TokenType.Dot)) {
                        stream.NextToken();
                        result = new MemberExpression(result, ReadName());
                    }
                    else if (Spec.ConstantValueTypes.Contains(stream.Peek.Type)) {
                        // abc.1, abc"", abc 1L, abc 0j
                        ReportError(Spec.ERR_InvalidStatement, stream.Peek);
                        return Error();
                    }
                    else {
                        return result;
                    }
                }
            }
            finally {
                allowIncomplete = prevAllow;
            }
        }

        private CallExpression FinishCallExpr(Expression target, params Arg[] args) {
//            var hasArgsTuple   = false;
//            var hasKeywordMap = false;
//            var keywordCount   = 0;
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
            Position start = StartNewStmt(TokenType.LeftBracket).Span.Start;
            bool     trailingComma;

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

                return new SliceExpression(startExpr, stop, step, start, tokenEnd);
            }

            Expression ret = MakeTupleOrExpr(expressions, trailingComma, true);
            ret.MarkPosition(start, tokenEnd);
            return ret;
        }

        /// <summary>
        ///     generator_expression:
        ///         '(' expression comp_for ')'
        ///     <para />
        ///     "for" has NOT been stream.Eaten before entering this method
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

        #region Functions

//        // decorators ::=
//        // decorator+
//        // decorator ::=
//        // "@" dotted_name ["(" [argument_list [","]] ")"] NEWLINE
//        private List<Expression> ParseDecorators() {
//            var decorators = new List<Expression>();
//
//            while (stream.MaybeEat(TokenType.OpAt)) {
//                Position   start     = tokenStart;
//                Expression decorator = new NameExpression(ReadName());
//                decorator.MarkPosition(start, tokenEnd);
//                while (stream.MaybeEat(TokenType.OpDot)) {
//                    Token name = ReadName();
//                    decorator = new MemberExpression(decorator, name);
//                    decorator.MarkPosition(tokenStart, tokenEnd);
//                }
//                decorator.MarkPosition(start, tokenEnd);
//
//                if (stream.MaybeEat(TokenType.OpLeftParenthesis)) {
//                    Arg[] args = FinishArgumentList(null);
//                    decorator = FinishCallExpr(decorator, args);
//                }
//                decorator.MarkPosition(start, tokenEnd);
//                stream.EatNewline();
//
//                decorators.Add(decorator);
//            }
//
//            return decorators;
//        }
//
//        // 'def' NAME parameters ['->' test] ':' suite
//
//        // decorated: decorators(class | funcdef | async_funcdef)
//        // this gets called with "@" look-ahead
//        private Statement ParseDecorated() {
//            List<Expression> decorators = ParseDecorators();
//
//            Statement res;
//
//            if (stream.Peek == Tokens.KeywordDefToken) {
//                FunctionDefinition fnc = ParseFuncDef();
//                fnc.Decorators = decorators.ToArray();
//                res            = fnc;
//            }
//            else if (stream.PeekIs(TokenType.KeywordClass)) {
//                ClassDefinition cls = ParseClassDef();
//                cls.Decorators = decorators.ToArray();
//                res            = cls;
//            }
//            else if (stream.PeekIs(TokenType.KeywordAsync)) {
//                stream.NextToken();
//                FunctionDefinition fnc = ParseFuncDef(true);
//                fnc.Decorators = decorators.ToArray();
//                res            = fnc;
//            }
//            else {
//                res = new EmptyStatement(stream.Peek);
//                ReportSyntaxError(stream.Peek);
//            }
//
//            return res;
//        }
//
//        // func_def: [decorators] 'def' NAME parameters ':' suite
//        // parameters: '(' [typed_args_list] ')'
//        // this gets called with "def" as the look-ahead
//        private FunctionDefinition ParseFuncDef(bool isAsync = false) {
//            stream.Eat(TokenType.KeywordDef);
//            Position start = tokenStart;
//            string   name  = ReadName().Value;
//
//            stream.Eat(TokenType.OpLeftParenthesis);
//
//            Position lEnd = tokenEnd;
//
//            Parameter[] parameters = ParseParameterList(TokenType.OpRightParenthesis, true);
//            if (parameters == null) {
//                // error in parameters
//                return new FunctionDefinition(name, new Parameter[0], start/*, lEnd*/);
//            }
//
//            Expression annotation = null;
//            if (stream.MaybeEat(TokenType.ReturnAnnotation)) {
//                annotation = ParseExpression();
//            }
//
//            var ret = new FunctionDefinition(name, parameters, start);
//            ast.PushFunction(ret);
//
//            Statement          body = ParseClassOrFuncBody();
//            FunctionDefinition ret2 = ast.PopFunction();
//            Debug.Assert(ret == ret2);
//
//            ret.Body             = body;
//            ret.ReturnAnnotation = annotation;
//            ret.MarkPosition(start, body.Span.End);
//
//            return ret;
//        }

        #endregion

        #region Parameters

        private void CheckUniqueParameter(HashSet<string> names, Token name) {
            if (names.Contains(name.Value)) {
                Blame(BlameType.DuplicatedArgumentInFunctionDefinition, name);
            }
            names.Add(name.Value);
        }

        /// <summary>
        /// <c>
        ///     parameter_list:
        ///         (named_parameter ",")*
        ///         ( "*" [parameter] ("," named_parameter)* ["," "**" parameter]
        ///         | "**" parameter
        ///         | named_parameter[","] )
        /// </c>
        /// </summary>
        private Parameter[] ParseFunctionParameterList(TokenType terminator, bool allowAnnotations) {
            var pl                      = new List<Parameter>();
            var names                   = new HashSet<string>(StringComparer.Ordinal);
            var needDefault             = false;
            var readMultiply            = false;
            var hasKeywordOnlyParameter = false;
            // we want these to be the last two parameters
            Parameter listParameter = null;
            Parameter mapParameter  = null;
            while (true) {
                if (stream.MaybeEat(terminator)) {
                    break;
                }

                if (stream.MaybeEat(TokenType.OpPower)) {
                    mapParameter = ParseParameter(names, ParameterKind.Map, allowAnnotations);
                    if (mapParameter == null) {
                        // no parameter name, syntax error
                        return new Parameter[0];
                    }
                    stream.Eat(terminator);
                    break;
                }

                if (stream.MaybeEat(TokenType.OpMultiply)) {
                    if (readMultiply) {
                        ReportError("Invalid syntax (ALE-1, description_not_implemented)", stream.Peek);
                        return new Parameter[0];
                    }

                    if (stream.PeekIs(TokenType.Comma)) {
                        // "*"
                    }
                    else {
                        listParameter = ParseParameter(names, ParameterKind.List, allowAnnotations);
                        if (listParameter == null) {
                            // no parameter name, syntax error
                            return new Parameter[0];
                        }
                    }

                    readMultiply = true;
                }
                else {
                    // If a parameter has a default value, all following parameters up until the "*" must also have a default value
                    Parameter parameter;
                    if (readMultiply) {
                        var dontCare = false;
                        parameter               = ParseNamedParameter(names, ParameterKind.KeywordOnly, allowAnnotations, ref dontCare);
                        hasKeywordOnlyParameter = true;
                    }
                    else {
                        parameter = ParseNamedParameter(names, ParameterKind.Normal, allowAnnotations, ref needDefault);
                    }
                    if (parameter == null) {
                        // no parameter, syntax error
                        return new Parameter[0];
                    }

                    pl.Add(parameter);
                }

                if (!stream.MaybeEat(TokenType.Comma)) {
                    stream.Eat(terminator);
                    break;
                }
            }

            if (readMultiply
             && listParameter == null
             && mapParameter != null
             && !hasKeywordOnlyParameter) {
                // TODO: this should not throw right away
                ReportError("named arguments must follow bare *", stream.Token);
            }

            if (listParameter != null) {
                pl.Add(listParameter);
            }
            if (mapParameter != null) {
                pl.Add(mapParameter);
            }

            return pl.ToArray();
        }

        // named_parameter:
        //     parameter ["=" expression]
        private Parameter ParseNamedParameter(HashSet<string> names, ParameterKind parameterKind, bool allowAnnotations, ref bool needDefault) {
            Parameter parameter = ParseParameter(names, parameterKind, allowAnnotations);
            if (parameter != null) {
                if (stream.MaybeEat(TokenType.Assign)) {
                    needDefault            = true;
                    parameter.DefaultValue = ParseTestExpr();
                }
                else if (needDefault) {
                    Blame(BlameType.ExpectedDefaultParameterValue, stream.Token);
                }
            }
            return parameter;
        }

        // parameter:
        //     ID [":" expression]
        private Parameter ParseParameter(HashSet<string> names, ParameterKind parameterKind, bool allowAnnotations) {
            if (!stream.PeekIs(TokenType.Identifier)) {
                BlameInvalidSyntax(TokenType.Identifier, stream.Peek);
                return null;
            }

            Token t         = stream.NextToken();
            var   parameter = new Parameter(t, parameterKind);
            CompleteParameterName(parameter, t, names);

            // expression
            if (allowAnnotations && stream.MaybeEat(TokenType.Colon)) {
                parameter.Annotation = ParseTestExpr();
            }

            return parameter;
        }

        private void CompleteParameterName(SpannedRegion node, Token name, HashSet<string> names) {
            CheckUniqueParameter(names, name);
            node.MarkPosition(tokenStart, tokenEnd);
        }

        #endregion

        #region Arguments list

        /// <summary>
        ///     arg_list:
        ///     expression                     rest_of_arguments
        ///     expression "="   expression    rest_of_arguments
        ///     expression "for" gen_expr_rest
        /// </summary>
        private Arg[] FinishGeneratorOrArgList() {
            if (stream.PeekIs(TokenType.RightParenthesis, TokenType.OpMultiply, TokenType.OpPower)) {
                return ParseArgumentsList();
            }
            Position   start = tokenStart;
            Expression e     = ParseTestExpr();
            if (e is ErrorExpression) {
                return new Arg[0];
            }
            Arg a;
            if (stream.MaybeEat(TokenType.Assign)) {
                // Keyword argument
                a = FinishKeywordArgument(e);
            }
            else if (stream.PeekIs(TokenType.KeywordFor)) {
                // Generator expression
                a = new Arg(ParseGeneratorExpr(e));
                stream.Eat(TokenType.RightParenthesis);
                a.MarkPosition(start, tokenEnd);
                return new[] { a }; // Generator expression is the argument
            }
            else {
                a = new Arg(e);
            }

            // Was this all?
            if (stream.MaybeEat(TokenType.Comma)) {
                return ParseArgumentsList(a);
            }

            stream.Eat(TokenType.RightParenthesis);
            a.MarkPosition(start, tokenEnd);
            return new[] { a };
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
        ///     arg_list: (argument ',')* (argument [',']| '*' expression [',' '**' expression] | '**' expression)
        ///     argument: [expression '='] expression
        ///     # Really [keyword '='] expression
        /// </summary>
        private Arg[] ParseArgumentsList(Arg first = null) {
            const TokenType terminator = TokenType.RightParenthesis;

            var arguments = new List<Arg>();

            if (first != null) {
                arguments.Add(first);
            }

            // parse remaining arguments
            while (true) {
                if (stream.MaybeEat(terminator)) {
                    break;
                }
                var name = new NameExpression(stream.Peek);
                Arg arg;

                if (stream.MaybeEat(TokenType.OpMultiply)) {
                    Expression t = ParseTestExpr();
                    arg = new Arg(name, t);
                }
                else {
                    Expression e = ParseTestExpr();
                    if (stream.MaybeEat(TokenType.Assign)) {
                        arg = FinishKeywordArgument(e);
                        CheckUniqueArgument(arguments, arg);
                    }
                    else {
                        arg = new Arg(e);
                    }
                }
                arguments.Add(arg);
                if (stream.MaybeEat(TokenType.Comma)) {
                    continue;
                }
                stream.Eat(terminator);
                break;
            }
            return arguments.ToArray();
        }

        #endregion

        private NameExpression ReadName() {
            if (!stream.PeekIs(TokenType.Identifier)) {
                BlameInvalidSyntax(TokenType.Identifier, stream.Peek);
                return null;
            }
            stream.NextToken();
            return new NameExpression(stream.Token);
        }

        private NameExpression[] ReadNames() {
            var l = new List<NameExpression> { ReadName() };
            while (stream.MaybeEat(TokenType.Dot)) {
                l.Add(ReadName());
            }
            return l.ToArray();
        }

        private ErrorExpression Error() {
            return new ErrorExpression(tokenStart, tokenEnd);
        }

        private ExpressionStatement ErrorStmt() {
            return new ExpressionStatement(Error());
        }

        #region Errors reporting

        internal void BlameInvalidSyntax(TokenType expectedType, SpannedRegion mark) {
            ReportError("Invalid syntax, '" + expectedType.GetValue() + "' expected.", mark);
        }

        internal void ReportError(string message, SpannedRegion mark) {
            blames.Add(new LanguageException(new Blame(message, BlameSeverity.Error, mark), stream.Source));
        }

        internal void ReportWarning(string message, SpannedRegion mark) {
            blames.Add(new LanguageException(new Blame(message, BlameSeverity.Warning, mark), stream.Source));
        }

        internal void Blame(BlameType type, SpannedRegion mark) {
            Blame(type, mark.Span.Start, mark.Span.End);
        }

        internal void Blame(BlameType type, Position start, Position end) {
            Debug.Assert(type != BlameType.None);

            blames.Add(new LanguageException(new Blame(type, Spec.Blames[type], start, end), stream.Source));
        }

        #endregion
    }
}