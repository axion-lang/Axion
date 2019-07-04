using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Axion.Core.Processing.Errors;
using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Processing.Source;
using Axion.Core.Processing.Syntactic.Atomic;
using Axion.Core.Processing.Syntactic.Binary;
using Axion.Core.Processing.Syntactic.Definitions;
using Axion.Core.Processing.Syntactic.TypeNames;
using Axion.Core.Specification;
using Newtonsoft.Json;
using static Axion.Core.Specification.TokenType;

namespace Axion.Core.Processing.Syntactic {
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
    public abstract class Expression : SpannedRegion {
        private Ast ast;

        internal Ast Ast {
            get {
                if (ast != null) {
                    return ast;
                }

                Expression p = this;
                while (!(p is Ast)) {
                    p = p.Parent;
                }

                ast = (Ast) p;
                return ast;
            }
        }

        private BlockExpression parentBlock;

        internal BlockExpression ParentBlock {
            get {
                Expression p = this;
                do {
                    p = p.Parent;
                } while (!(p is BlockExpression));

                parentBlock = (BlockExpression) p;
                return parentBlock;
            }
        }

        protected internal Expression Parent;

        [JsonIgnore]
        public virtual TypeName ValueType {
            get => null;
            set => throw new NotSupportedException();
        }

        //[JsonProperty]
        //public string ValueTypeString => ValueType?.ToString() ?? "<Unknown>";

        internal Type MacroExpectationType;

        protected Expression() { }

        protected Expression(Expression parent) {
            Parent = parent;
        }

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
        internal static Expression ParseAtomExpr(Expression parent) {
            switch (parent.Peek.Type) {
            case Identifier: {
                return NameExpression.ParseName(parent);
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
                return new AwaitExpression(parent);
            }

            case KeywordYield: {
                return new YieldExpression(parent);
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

            case Indent:
            case OpenBrace:
            case Colon
                when parent.MacroExpectationType == typeof(BlockExpression): {
                return new BlockExpression(parent);
            }

            case DoubleOpenBrace: {
                return new CodeQuoteExpression(parent);
            }

            case OpenParenthesis: {
                Token start = parent.Token;
                parent.Eat(OpenParenthesis);
                // empty tuple
                if (parent.MaybeEat(CloseParenthesis)) {
                    return new TupleExpression(parent, start, parent.Token);
                }

                Expression result = ParseMultiple(parent, parens: true);
                parent.Eat(CloseParenthesis);
                return result;
            }

            default: {
                if (Spec.Constants.Contains(parent.Peek.Type)) {
                    // TODO add pre-concatenation of literals
                    return new ConstantExpression(parent);
                }

                var macro = new MacroApplicationExpression(parent);
                if (macro.MacroDefinition != null) {
                    return macro;
                }

                break;
            }
            }

            return new UnknownExpression(parent);
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
        internal static Expression ParseSuffixExpr(Expression parent) {
            Expression value = ParseAtomExpr(parent);
            if (Spec.DefinitionExprs.Contains(value.GetType())) {
                return value;
            }

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
        internal static Expression ParsePrefixExpr(Expression parent) {
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
        internal static Expression ParseInfixExpr(Expression parent) {
            Expression ParseInfix(int precedence) {
                Expression leftExpr = ParsePrefixExpr(parent);
                if (parent.Token.Type == Newline || Spec.DefinitionExprs.Contains(leftExpr.GetType())) {
                    return leftExpr;
                }

                // expr (keyword | expr) expr?
                var macro = new MacroApplicationExpression(parent, leftExpr);
                if (macro.MacroDefinition != null) {
                    return macro;
                }

                while (true) {
                    int newPrecedence;
                    if (parent.Peek is OperatorToken operatorToken) {
                        newPrecedence = operatorToken.Properties.Precedence;
                    }
                    else if (parent.Token.Type != Newline && parent.Peek.Is(Identifier)) {
                        newPrecedence = 4;
                    }
                    else {
                        break;
                    }

                    if (newPrecedence < precedence) {
                        break;
                    }

                    parent.Eat();
                    leftExpr = new BinaryExpression(
                        parent,
                        leftExpr,
                        parent.Token,
                        ParseInfix(newPrecedence + 1)
                    );
                }

                if (parent.Peek.Is(KeywordIf, KeywordUnless)
                 && !parent.Token.Is(Newline, Outdent)) {
                    return new ConditionalInfixExpression(parent, leftExpr);
                }

                return leftExpr;
            }

            return ParseInfix(0);
        }

        /// <summary>
        ///     <c>
        ///         var_expr
        ///             : infix_list
        ///             | (['let'] assignable
        ///                [':' type]
        ///                ['=' infix_list])
        ///     </c>
        /// </summary>
        internal static Expression ParseVarExpr(Expression parent) {
            bool isImmutable = parent.MaybeEat(KeywordLet);

            Expression expr = ParseMultiple(parent, ParseInfixExpr);

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
            Expression                   parent,
            Func<Expression, Expression> parserFunc = null,
            bool                         parens     = false,
            params Type[]                expectedTypes
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
            // TODO HERE 'for' can be after 'newline', but if it's inside (), {} or []
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
            Expression parent,
            TokenType  terminator = None
        ) {
            var items = new NodeList<Expression>(parent) {
                ParseVarExpr(parent)
            };
            if (parent.MaybeEat(Semicolon)) {
                while (parent.Token.Is(Semicolon)
                    && !parent.MaybeEat(Newline)
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
            Expression           parent,
            NodeList<Expression> expressions
        ) {
            if (expressions.Count == 1) {
                return expressions[0];
            }

            return new TupleExpression(parent, expressions);
        }

        #region Expression type checkers

        /// <summary>
        ///     Checks if every expression in collection
        ///     belong to any of <paramref name="expectedTypes" />.
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
        ///     belongs to any of <paramref name="expectedTypes" />.
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
                  + GetFriendlyName(itemType.Name),
                    expr
                );
            }
            else {
                expr.Unit.ReportError(
                    "Expected "
                  + GetFriendlyName(expectedTypes[0].Name)
                  + ", got "
                  + GetFriendlyName(itemType.Name),
                    expr
                );
            }
        }

        #endregion

        /// <summary>
        ///     In:  SampleExpression
        ///     Out: 'sample' expression
        /// </summary>
        internal static string GetFriendlyName(string expressionTypeName) {
            string exprOriginalName = expressionTypeName.Replace("Expression", "");
            var    result           = new StringBuilder();
            result.Append("'" + char.ToLower(exprOriginalName[0]));

            exprOriginalName = exprOriginalName.Remove(0, 1);
            foreach (char c in exprOriginalName) {
                if (char.IsUpper(c)) {
                    result.Append(" ").Append(char.ToLower(c));
                }
                else {
                    result.Append(c);
                }
            }

            result.Append("' expression");
            return result.ToString();
        }

        /// <summary>
        ///     Helper for constructing expressions that
        ///     start with token.
        ///     (marks start/end of expr, sets it's parent)
        /// </summary>
        protected void Construct(Expression parent, Action constructor) {
            Parent = parent;
            MarkStart(Peek);
            constructor();
            MarkEnd();
        }

        /// <summary>
        ///     Helper for constructing expressions that
        ///     start with token.
        ///     (marks start/end of expr, sets it's parent)
        /// </summary>
        protected void Construct(Expression parent, Expression firstExpr, Action constructor) {
            Parent = parent;
            MarkStart(firstExpr);
            constructor();
            MarkEnd();
        }

        #region AST properties short links

        internal SourceUnit Unit => Ast.SourceUnit;

        internal Token Token =>
            Ast.CurrentTokenIndex > -1
         && Ast.CurrentTokenIndex < Unit.Tokens.Count
                ? Unit.Tokens[Ast.CurrentTokenIndex]
                : Unit.Tokens[Unit.Tokens.Count - 1];

        private Token exactPeek =>
            Ast.CurrentTokenIndex + 1 < Unit.Tokens.Count
                ? Unit.Tokens[Ast.CurrentTokenIndex + 1]
                : Unit.Tokens[Unit.Tokens.Count     - 1];

        internal Token Peek {
            get {
                SkipTrivial();
                return exactPeek;
            }
        }

        #endregion

        #region Token stream controllers

        internal void MarkStart() {
            MarkStart(Token);
        }

        internal void MarkStart(int tokenIdx) {
            MarkStart(Unit.Tokens[tokenIdx]);
        }

        internal void MarkEnd() {
            MarkEnd(Token);
        }

        internal void MarkEnd(int tokenIdx) {
            MarkEnd(Unit.Tokens[tokenIdx]);
        }

        internal bool PeekByIs(int peekBy, params TokenType[] expected) {
            SkipTrivial(expected);
            if (Ast.CurrentTokenIndex + peekBy < Unit.Tokens.Count) {
                Token peekN = Unit.Tokens[Ast.CurrentTokenIndex + peekBy];
                for (var i = 0; i < expected.Length; i++) {
                    if (peekN.Is(expected[i])) {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        ///     Adjusts [<see cref="Ast" />.Index] by [<paramref name="pos" />]
        ///     and returns token by current index.
        /// </summary>
        internal void Eat(int pos = 1) {
            if (Ast.CurrentTokenIndex + pos >= 0
             && Ast.CurrentTokenIndex + pos < Unit.Tokens.Count) {
                Ast.CurrentTokenIndex += pos;
            }
        }

        /// <summary>
        ///     Skips new line token, failing,
        ///     if the next token type is not
        ///     the same as passed in parameter.
        /// </summary>
        internal void Eat(params TokenType[] types) {
            Debug.Assert(types.Length > 0);
            if (Peek.Is(types)) {
                Eat();
                return;
            }

            // TODO types[*]
            BlameInvalidSyntax(types[0], exactPeek);
        }

        /// <summary>
        ///     Skips token of specified type,
        ///     returns: was token skipped or not.
        /// </summary>
        internal bool MaybeEat(params TokenType[] types) {
            SkipTrivial(types);
            for (var i = 0; i < types.Length; i++) {
                if (exactPeek.Is(types[i])) {
                    Eat();
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        ///     Moves current token index by absolute value.
        /// </summary>
        internal void MoveTo(int tokenIndex) {
            Debug.Assert(tokenIndex >= -1 && tokenIndex < Unit.Tokens.Count);
            Ast.CurrentTokenIndex = tokenIndex;
        }

        private void SkipTrivial(params TokenType[] wantedTypes) {
            while (true) {
                if (exactPeek.Is(Comment)) {
                    Eat();
                }
                // if we got newline before wanted type, just skip it
                // (except we WANT to get newline)
                else if (exactPeek.Is(Newline)
                      && !wantedTypes.Contains(Newline)) {
                    Eat();
                }
                else {
                    break;
                }
            }
        }

        #endregion

        #region Syntax node's property setter helpers

        protected void SetNode<T>(ref T field, T value)
            where T : Expression {
            if (field == value) {
                return;
            }

            if (value != null) {
                value.Parent = this;
            }

            field = value;
        }

        protected void SetNode<T>(ref NodeList<T> field, NodeList<T> value)
            where T : Expression {
            if (field == value) {
                return;
            }

            if (value != null) {
                foreach (T expr in value) {
                    if (expr != null) {
                        expr.Parent = this;
                    }
                }
            }
            else {
                value = new NodeList<T>(this);
            }

            field = value;
        }

        #endregion

        #region Errors reporting

        internal void BlameInvalidSyntax(TokenType expectedType, SpannedRegion mark) {
            Unit.ReportError(
                "Invalid syntax, expected '"
              + expectedType.GetValue()
              + "', got '"
              + exactPeek.Type.GetValue()
              + "'.",
                mark
            );
        }

        #endregion
    }
}