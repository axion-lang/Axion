using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Axion.Core.Processing.CodeGen;
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
    ///             expr {',' expr};
    ///         infix_list:
    ///             infix_expr {',' infix_expr};
    ///         preglobal_list:
    ///             preglobal_expr {',' preglobal_expr};
    ///         simple_name_list:
    ///             simple_name_expr {',' simple_name_expr};
    ///         single_expr:
    ///             conditional_expr | while_expr | for_expr    |
    ///             try_expr         | with_expr  | import_expr |
    ///             decorated;
    ///         decorated:
    ///             module_def | class_def  | enum_def |
    ///             func_def   | small_expr;
    ///         small_expr:
    ///             pass_expr | expr_expr | flow_expr;
    ///         flow_expr:
    ///             break_expr | continue_expr | return_expr |
    ///             raise_expr | yield_expr;
    ///     </c>
    /// </summary>
    public class Expression : SpannedRegion {
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

        internal Expression(Expression parent) {
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
        ///             | CONSTANT;
        ///     </c>
        /// </summary>
        internal Expression ParseAtom() {
            switch (Peek.Type) {
            case Identifier: {
                return NameExpression.ParseName(this);
            }

            case KeywordIf: {
                return new ConditionalExpression(this);
            }

            case KeywordWhile: {
                return new WhileExpression(this);
            }

            #region Small statements

            case Semicolon:
            case KeywordPass: {
                return new EmptyExpression(this);
            }

            case KeywordBreak: {
                return new BreakExpression(this);
            }

            case KeywordContinue: {
                return new ContinueExpression(this);
            }

            case KeywordReturn: {
                return new ReturnExpression(this);
            }

            case KeywordAwait: {
                return new AwaitExpression(this);
            }

            case KeywordYield: {
                return new YieldExpression(this);
            }

            #endregion

            case KeywordModule: {
                return new ModuleDefinition(this);
            }

            case KeywordClass: {
                return new ClassDefinition(this);
            }

            case KeywordEnum: {
                return new EnumDefinition(this);
            }

            case KeywordFn: {
                return new FunctionDefinition(this);
            }

            case Indent:
            case OpenBrace:
            case Colon
                when MacroExpectationType == typeof(BlockExpression): {
                return new BlockExpression(this);
            }

            case DoubleOpenBrace: {
                return new CodeQuoteExpression(this);
            }

            case OpenParenthesis: {
                // empty tuple
                if (PeekByIs(2, CloseParenthesis)) {
                    return new TupleExpression(this);
                }
                return ParseMultiple();
            }

            default: {
                if (Spec.Constants.Contains(Peek.Type)) {
                    return new ConstantExpression(this);
                }

                var macro = new MacroApplicationExpression(this);
                if (macro.MacroDefinition != null) {
                    return macro;
                }

                break;
            }
            }

            return new UnknownExpression(this);
        }

        /// <summary>
        ///     <c>
        ///         suffix_expr:
        ///             atom
        ///             {'|>' atom }
        ///             | ({ member | call_expr | index_expr } ['++' | '--']));
        ///     </c>
        /// </summary>
        internal Expression ParseSuffix() {
            Expression value = ParseAtom();
            if (Spec.DefinitionExprs.Contains(value.GetType())) {
                return value;
            }

            if (MaybeEat(RightPipeline)) {
                do {
                    value = new FunctionCallExpression(
                        this,
                        ParseSuffix(ParseAtom()),
                        new CallArgument(this, value)
                    );
                } while (MaybeEat(RightPipeline));

                return value;
            }

            Expression ParseSuffix(Expression result) {
                var loop = true;
                while (loop) {
                    switch (Peek.Type) {
                    case Dot: {
                        result = new MemberAccessExpression(this, result);
                        break;
                    }

                    case OpenParenthesis: {
                        result = new FunctionCallExpression(this, result, true);
                        break;
                    }

                    case OpenBracket: {
                        result = new IndexerExpression(this, result);
                        break;
                    }

                    default: {
                        loop = false;
                        break;
                    }
                    }
                }

                if (MaybeEat(OpIncrement, OpDecrement)) {
                    var op = (OperatorToken) result.Token;
                    op.Properties.InputSide = InputSide.Right;
                    result                  = new UnaryExpression(this, op, result);
                }

                return result;
            }

            return ParseSuffix(value);
        }

        /// <summary>
        ///     <c>
        ///         prefix_expr:
        ///             (PREFIX_OPERATOR prefix_expr) | suffix_expr;
        ///     </c>
        /// </summary>
        internal Expression ParsePrefix() {
            if (MaybeEat(Spec.PrefixOperators)) {
                var op = (OperatorToken) Token;
                op.Properties.InputSide = InputSide.Right;
                return new UnaryExpression(this, op, ParsePrefix());
            }

            return ParseSuffix();
        }

        /// <summary>
        ///     <c>
        ///         infix_expr:
        ///             prefix_expr (ID | SYMBOL) infix_expr;
        ///     </c>
        /// </summary>
        internal Expression ParseInfix() {
            Expression ParseInfix(int precedence) {
                Expression leftExpr = ParsePrefix();
                if (Token.Type == Newline || Spec.DefinitionExprs.Contains(leftExpr.GetType())) {
                    return leftExpr;
                }

                // expr (keyword | expr) expr?
                var macro = new MacroApplicationExpression(this, leftExpr);
                if (macro.MacroDefinition != null) {
                    return macro;
                }

                while (true) {
                    int newPrecedence;
                    if (Peek is OperatorToken opToken) {
                        newPrecedence = opToken.Properties.Precedence;
                    }
                    else if (Token.Type != Newline && Peek.Is(Identifier)) {
                        newPrecedence = 4;
                    }
                    else {
                        break;
                    }

                    if (newPrecedence < precedence) {
                        break;
                    }

                    Eat();
                    leftExpr = new BinaryExpression(
                        this,
                        leftExpr,
                        Token,
                        ParseInfix(newPrecedence + 1)
                    );
                }

                if (Peek.Is(KeywordIf, KeywordUnless)
                 && !Token.Is(Newline, Outdent)) {
                    return new ConditionalInfixExpression(this, leftExpr);
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
        ///                ['=' infix_list]);
        ///     </c>
        /// </summary>
        internal Expression ParseAny() {
            bool isImmutable = MaybeEat(KeywordLet);

            Expression expr = ParseMultiple(ParseInfix);

            // ['let'] name '=' expr
            if (expr is BinaryExpression bin
             && bin.Left is SimpleNameExpression name
             && bin.Operator.Is(OpAssign)
             && !bin.ParentBlock.HasVariable(name)) {
                return new VariableDefinitionExpression(
                    this,
                    bin.Left,
                    null,
                    bin.Right,
                    isImmutable
                );
            }

            // check for ':' - starting block instead of var definition
            if ((Ast.MacroExpectationType?.IsInstanceOfType(typeof(BlockExpression))
              ?? false)
             || !Peek.Is(Colon)) {
                return expr;
            }

            if (!Spec.VariableLeftExprs.Contains(expr.GetType())) {
                Unit.Blame(BlameType.ThisExpressionTargetIsNotAssignable, expr);
            }

            TypeName   type  = null;
            Expression value = null;
            if (MaybeEat(Colon)) {
                type = new TypeName(this).ParseTypeName();
            }

            if (MaybeEat(OpAssign)) {
                value = ParseMultiple(expectedTypes: Spec.InfixExprs);
            }

            return new VariableDefinitionExpression(this, expr, type, value, isImmutable);
        }

        /// <summary>
        ///     <c>
        ///         ['('] %expr {',' %expr} [')'];
        ///     </c>
        ///     Helper for parsing multiple comma-separated
        ///     expressions with optional parenthesis
        ///     (e.g. tuples)
        /// </summary>
        internal Expression ParseMultiple(
            Func<Expression> parserFunc = null,
            params Type[]    expectedTypes
        ) {
            parserFunc = parserFunc ?? ParseAny;
            if (expectedTypes.Length == 0 || parserFunc == ParseAny) {
                expectedTypes = Spec.GlobalExprs;
            }

            var parens = MaybeEat(OpenParenthesis);
            var list = new NodeList<Expression>(this) {
                parserFunc()
            };

            if (parens && MaybeEat(CloseParenthesis)) {
                return list[0];
            }

            // tuple
            if (MaybeEat(Comma)) {
                do {
                    list.Add(parserFunc());
                } while (MaybeEat(Comma));
            }
            // generator | comprehension
            // TODO HERE 'for' can be after 'newline', but if it's inside (), {} or []
            else if (Peek.Is(KeywordFor) && Token.Type != Newline) {
                list[0] = new ForComprehension(this, list[0]);
                if (parens) {
                    list[0] = new GeneratorExpression(this, (ForComprehension) list[0]);
                }
            }

            CheckType(list, expectedTypes);

            if (parens) {
                Eat(CloseParenthesis);
                if (list.Count == 1) {
                    return new ParenthesizedExpression(list[0]);
                }
            }

            return MaybeTuple(list);
        }

        /// <summary>
        ///     <c>
        ///         cascade:
        ///             (expr ((';' [NEWLINE]) | NEWLINE | terminator | END) )+;
        ///     </c>
        /// </summary>
        internal List<Expression> ParseCascade(TokenType terminator = None) {
            var items = new List<Expression> {
                ParseAny()
            };
            if (MaybeEat(Semicolon)) {
                while (Token.Is(Semicolon)
                    && !MaybeEat(Newline)
                    && !Peek.Is(terminator, End)) {
                    items.Add(ParseAny());
                    if (MaybeEat(End)) {
                        if (terminator != None) {
                            Eat(terminator);
                        }
                        break;
                    }

                    if (!MaybeEat(Semicolon)) {
                        Eat(Newline);
                    }
                }
            }

            return items;
        }

        internal Expression MaybeTuple(NodeList<Expression> expressions) {
            if (expressions.Count == 1) {
                return expressions[0];
            }

            return new TupleExpression(this, expressions);
        }

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

        internal override void ToAxionCode(CodeBuilder c) {
            throw new NotSupportedException();
        }

        internal override void ToCSharpCode(CodeBuilder c) {
            throw new NotSupportedException();
        }
    }
}