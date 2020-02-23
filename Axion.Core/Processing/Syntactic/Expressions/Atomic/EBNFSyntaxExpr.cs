using System;
using System.Collections.Generic;
using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Processing.Syntactic.Expressions.Definitions;
using Axion.Core.Processing.Syntactic.Expressions.MacroPatterns;
using Axion.Core.Processing.Syntactic.Expressions.TypeNames;
using Axion.Core.Specification;
using static Axion.Core.Processing.Lexical.Tokens.TokenType;

namespace Axion.Core.Processing.Syntactic.Expressions.Atomic {
    /// <summary>
    ///     ebnf-syntax-expr:
    ///     '$(', {syntax-rule-expr}, ')';
    ///     syntax-rule-expr:
    ///     ID, ':', syntax-description-expr, ';';
    ///     syntax-description-expr:
    ///     ID
    ///     | STRING
    ///     | '[', rhs, ']'
    ///     | '{', rhs, '}'
    ///     | '(', rhs, ')'
    ///     | rhs, '|', rhs
    ///     | rhs, ',', rhs;
    /// </summary>
    public class EBNFSyntaxExpr : Expr {
        public EBNFSyntaxExpr(Expr parent = null) : base(parent) { }
        public CascadePattern Syntax { get; private set; }

        public EBNFSyntaxExpr Parse() {
            var patterns = new List<IPattern>();
            SetSpan(
                () => {
                    Stream.Eat(Dollar);
                    Stream.Eat(OpenParenthesis);
                    if (!Stream.PeekIs(CloseParenthesis)) {
                        patterns.Add(ParseSyntaxDescription());
                    }
                    Stream.Eat(CloseParenthesis);
                }
            );
            if (patterns.Count == 1 && patterns[0] is CascadePattern) {
                Syntax = (CascadePattern) patterns[0];
            }
            else {
                Syntax = new CascadePattern(patterns.ToArray());
            }
            return this;
        }

        public IPattern ParseSyntaxDescription() {
            var patterns = new List<IPattern>();
            do {
                IPattern pattern = null;
                // syntax group: (x, y)
                if (Stream.MaybeEat(OpenParenthesis)) {
                    pattern = ParseSyntaxDescription();
                    Stream.Eat(CloseParenthesis);
                }
                // optional pattern: [x]
                else if (Stream.MaybeEat(OpenBracket)) {
                    pattern = new OptionalPattern(ParseSyntaxDescription());
                    Stream.Eat(CloseBracket);
                }
                // multiple pattern: {x}
                else if (Stream.MaybeEat(OpenBrace)) {
                    pattern = new MultiplePattern(ParseSyntaxDescription());
                    Stream.Eat(CloseBrace);
                }
                else {
                    // custom keyword: 'while'
                    if (Stream.MaybeEat(TokenType.String)) {
                        pattern = Ast.NewTokenPattern(Stream.Token.Content);
                    }
                    // expression, [type defined in macro parameters]: expr-name 
                    else if (Stream.MaybeEat(Identifier)) {
                        IDefinitionExpr def = GetParentOfType<BlockExpr>()
                            .GetDefByName(Stream.Token.Content);
                        if (def is FunctionParameter fnParam
                         && fnParam.ValueType is SimpleTypeName exprTypeName
                         && exprTypeName.Name.Qualifiers.Count      >= 2
                         && exprTypeName.Name.Qualifiers[0].Content == "Syntax") {
                            // Transform "Syntax.Block" => "BlockExpr", etc.
                            // Syntax.<ExprType>
                            //        ~~~~~~~~~~
                            string typeName = exprTypeName.Name.Qualifiers[1].Content;
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
                        }
                    }
                }
                if (pattern == null) {
                    // TODO error
                    continue;
                }
                // or pattern: x | y
                if (Stream.MaybeEat(OpBitOr)) {
                    patterns.Add(new OrPattern(pattern, ParseSyntaxDescription()));
                }
                else {
                    patterns.Add(pattern);
                }
            } while (Stream.MaybeEat(Comma));
            if (patterns.Count == 1) {
                return patterns[0];
            }
            return new CascadePattern(patterns.ToArray());
        }
    }
}