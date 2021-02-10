using System;
using Axion.Core.Processing.Errors;
using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Processing.Syntactic.Expressions.Common;
using Axion.Core.Processing.Syntactic.Expressions.Definitions;
using Axion.Core.Processing.Syntactic.Expressions.TypeNames;
using Axion.Core.Specification;
using static Axion.Core.Processing.Lexical.Tokens.TokenType;

namespace Axion.Core.Processing.Syntactic.Expressions.Patterns {
    /// <summary>
    ///     <c>
    ///         expression-pattern:
    ///             name-expr [':' type-name];
    ///     </c>
    /// </summary>
    public class ExpressionPattern : Pattern {
        private Func<Node, Expr>? parseFunc;
        private Type type = null!;

        public ExpressionPattern(Node parent) : base(parent) { }

        public override bool Match(Node parent) {
            if (parent.Unit.TokenStream.PeekIs(TokenType.End)) {
                return false;
            }

            // leave expression non-starters to next token pattern.
            if (parent.Unit.TokenStream.PeekIs(Spec.NeverExprStartTypes)) {
                return true;
            }

            Expr? e;
            if (parseFunc != null) {
                e = parseFunc(parent);
                parent.Ast.MacroApplicationParts.Peek().Expressions.Add(e);
                return true;
            }

            e = typeof(TypeName).IsAssignableFrom(type)
                ? TypeName.Parse(parent)
                : AnyExpr.Parse(parent);

            if (type.IsInstanceOfType(e)) {
                parent.Ast.MacroApplicationParts.Peek().Expressions.Add(e);
                return true;
            }

            return false;
        }

        public ExpressionPattern Parse() {
            var id          = Stream.Eat(Identifier);
            var namedParts  = GetParent<MacroDef>()!.NamedSyntaxParts;
            var typeDefined = namedParts.ContainsKey(id.Content);
            if (typeDefined) {
                PatternFromTypeName(namedParts[id.Content]);
            }

            if (Stream.MaybeEat(Colon)) {
                var tn = TypeName.Parse(this);
                if (!typeDefined
                 && tn is SimpleTypeName simpleTypeName
                 && simpleTypeName.Name.IsSimple) {
                    var typeName = simpleTypeName.Name.Qualifiers[0].Content;
                    PatternFromTypeName(typeName);
                    namedParts.Add(id.Content, typeName);
                }
                else if (typeDefined) {
                    LanguageReport.To(BlameType.NameIsAlreadyDefined, id);
                }
                else {
                    LanguageReport.To(BlameType.InvalidMacroParameter, id);
                }
            }

            if (type == null && parseFunc == null) {
                LanguageReport.To(BlameType.ImpossibleToInferType, id);
            }

            return this;
        }

        private void PatternFromTypeName(string typeName) {
            if (!typeName.EndsWith("Expr") && !typeName.EndsWith("TypeName")) {
                typeName += "Expr";
            }

            if (Spec.ParsingTypes.TryGetValue(typeName, out var t)) {
                type = t;
            }
            else if (Spec.ParsingFunctions.TryGetValue(
                typeName,
                out var fn
            )) {
                parseFunc = fn;
            }
        }
    }
}
