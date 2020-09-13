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
        private Type              type = null!;

        public ExpressionPattern(Node parent) : base(parent) { }

        public override bool Match(Node parent) {
            // leave expression non-starters to next token pattern.
            if (parent.Source.TokenStream.PeekIs(Spec.NeverExprStartTypes)) {
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
            Token id          = Stream.Eat(Identifier);
            var   namedParts  = GetParent<MacroDef>()!.NamedSyntaxParts;
            bool  typeDefined = namedParts.ContainsKey(id.Content);
            if (typeDefined) {
                PatternFromTypeName(namedParts[id.Content]);
            }
            if (Stream.MaybeEat(Colon)) {
                TypeName? tn = TypeName.Parse(this);
                if (!typeDefined
                 && tn is SimpleTypeName simpleTypeName
                 && simpleTypeName.Name.IsSimple) {
                    string typeName = simpleTypeName.Name.Qualifiers[0].Content;
                    PatternFromTypeName(typeName);
                    namedParts.Add(id.Content, typeName);
                }
                else if (typeDefined) {
                    LangException.Report(BlameType.NameIsAlreadyDefined, id);
                }
                else {
                    LangException.Report(BlameType.InvalidMacroParameter, id);
                }
            }
            if (type == null && parseFunc == null) {
                LangException.Report(BlameType.ImpossibleToInferType, id);
            }
            return this;
        }

        private void PatternFromTypeName(string typeName) {
            if (!typeName.EndsWith("Expr") && !typeName.EndsWith("TypeName")) {
                typeName += "Expr";
            }
            if (Spec.ParsingTypes.TryGetValue(typeName, out Type t)) {
                type = t;
            }
            else if (Spec.ParsingFunctions.TryGetValue(typeName, out Func<Node, Expr> fn)) {
                parseFunc = fn;
            }
        }
    }
}
