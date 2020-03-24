using System;
using System.Collections.Generic;
using Axion.Core.Processing.CodeGen;
using Axion.Core.Processing.Errors;
using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Processing.Syntactic.Expressions.Atomic;
using Axion.Core.Processing.Syntactic.Expressions.MacroPatterns;
using Axion.Core.Processing.Syntactic.Expressions.TypeNames;
using Axion.Core.Specification;
using static Axion.Core.Processing.Lexical.Tokens.TokenType;

namespace Axion.Core.Processing.Syntactic.Expressions.Definitions {
    /// <summary>
    ///     <c>
    ///         macro-def:
    ///             'macro' simple-name syntax-description scope;
    ///     </c>
    /// </summary>
    public class MacroDef : Expr, IDefinitionExpr {
        public CascadePattern Syntax { get; private set; }

        private ScopeExpr scope;

        public ScopeExpr Scope {
            get => scope;
            set => scope = BindNode(value);
        }

        private NameExpr name;

        public NameExpr Name {
            get => name;
            set => name = BindNode(value);
        }

        public readonly Dictionary<string, string> NamedSyntaxParts = new Dictionary<string, string>();

        internal MacroDef(Expr parent) : base(parent) {
            Syntax = new CascadePattern();
        }

        public MacroDef Parse() {
            SetSpan(
                () => {
                    // TODO: find code, that can be replaced with macro by patterns
                    // Example:
                    // ========
                    // macro post-condition-loop ('do', scope: Scope, ('while' | 'until'), condition: Infix)
                    //     if syntax[2] == 'while'
                    //         condition = {{ not $condition }}
                    // 
                    //     return {{
                    //         while true {
                    //             $scope
                    //             if $condition {
                    //                 break
                    //             }
                    //         }
                    //     }}
                    Stream.Eat(KeywordMacro);
                    Name = new NameExpr(this).Parse(true);
                    // EBNF-based syntax definition
                    if (Stream.Eat(OpenParenthesis) != null) {
                        Syntax = ParseSyntaxDescription();
                        Stream.Eat(CloseParenthesis);
                    }
                    Scope = new ScopeExpr(this).Parse();
                }
            );
            return this;
        }

        /// <summary>
        ///     syntax-description:
        ///         ID ':' syntax-description-expr ';';
        ///     syntax-description-expr:
        ///         ID
        ///         | STRING
        ///         | ('[' syntax-description-expr ']')
        ///         | ('{' syntax-description-expr '}')
        ///         | ('(' syntax-description-expr ')')
        ///         | (syntax-description-expr '|' syntax-description-expr)
        ///         | (syntax-description-expr ',' syntax-description-expr);
        /// </summary>
        private CascadePattern ParseSyntaxDescription() {
            var patterns = new List<IPattern>();
            do {
                IPattern pattern = null;
                // syntax group `(x, y)`
                if (Stream.MaybeEat(OpenParenthesis)) {
                    pattern = ParseSyntaxDescription();
                    Stream.Eat(CloseParenthesis);
                }
                // optional pattern `[x]`
                else if (Stream.MaybeEat(OpenBracket)) {
                    pattern = new OptionalPattern(ParseSyntaxDescription());
                    Stream.Eat(CloseBracket);
                }
                // multiple pattern `{x}`
                else if (Stream.MaybeEat(OpenBrace)) {
                    pattern = new MultiplePattern(ParseSyntaxDescription());
                    Stream.Eat(CloseBrace);
                }
                else {
                    // custom keyword
                    if (Stream.MaybeEat(TokenType.String)) {
                        Source.RegisterCustomKeyword(Stream.Token.Content);
                        pattern = new TokenPattern(Stream.Token.Content);
                    }
                    // expr-name `TypeName` 
                    else if (Stream.MaybeEat(Identifier)) {
                        Token id          = Stream.Token;
                        bool  typeDefined = NamedSyntaxParts.ContainsKey(id.Content);
                        if (typeDefined) {
                            pattern = PatternFromTypeName(NamedSyntaxParts[id.Content]);
                        }
                        if (Stream.MaybeEat(Colon)) {
                            TypeName type = TypeName.Parse(this);
                            if (!typeDefined
                             && type is SimpleTypeName exprTypeName
                             && exprTypeName.Name.Qualifiers.Count == 1) {
                                string typeName = exprTypeName.Name.Qualifiers[0].Content;
                                pattern = PatternFromTypeName(typeName);
                                NamedSyntaxParts.Add(id.Content, typeName);
                            }
                            else if (typeDefined) {
                                LangException.Report(BlameType.NameIsAlreadyDefined, id);
                            }
                            else {
                                LangException.Report(BlameType.InvalidMacroParameter, id);
                            }
                        }
                        if (pattern == null) {
                            LangException.Report(BlameType.ImpossibleToInferType, id);
                        }
                    }
                }
                if (pattern == null) {
                    // TODO error
                    continue;
                }
                // or pattern `x | y`
                if (Stream.MaybeEat(OpBitOr)) {
                    patterns.Add(new OrPattern(pattern, ParseSyntaxDescription()));
                }
                else {
                    patterns.Add(pattern);
                }
            } while (Stream.MaybeEat(Comma));

            if (patterns.Count == 1 && !(patterns[0] is CascadePattern)) {
                return new CascadePattern(patterns[0]);
            }
            return new CascadePattern(patterns.ToArray());
        }

        private static ExpressionPattern PatternFromTypeName(string typeName) {
            ExpressionPattern pattern = null;
            if (!typeName.EndsWith("Expr") && !typeName.EndsWith("TypeName")) {
                typeName += "Expr";
            }
            if (Spec.ParsingTypes.TryGetValue(typeName, out Type type)) {
                pattern = new ExpressionPattern(type);
            }
            else if (Spec.ParsingFunctions.TryGetValue(
                typeName,
                out Func<Expr, Expr> fn
            )) {
                pattern = new ExpressionPattern(fn);
            }
            return pattern;
        }

        public override void ToAxion(CodeWriter c) {
            c.Write(
                "macro ", Name, "(", Syntax,
                ")", Scope
            );
        }

        public override void ToCSharp(CodeWriter c) {
            throw new NotSupportedException();
        }
    }
}