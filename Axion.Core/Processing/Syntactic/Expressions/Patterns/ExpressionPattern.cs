using System;
using System.Collections.Generic;
using Axion.Core.Processing.Errors;
using Axion.Core.Processing.Syntactic.Expressions.Atomic;
using Axion.Core.Processing.Syntactic.Expressions.Common;
using Axion.Core.Processing.Syntactic.Expressions.Definitions;
using Axion.Core.Processing.Syntactic.Expressions.TypeNames;
using Axion.Specification;
using static Axion.Specification.TokenType;

namespace Axion.Core.Processing.Syntactic.Expressions.Patterns {
    /// <summary>
    ///     <c>
    ///         expression-pattern:
    ///             name-expr [':' type-name];
    ///     </c>
    /// </summary>
    public class ExpressionPattern : Pattern {
        private Func<Node, Node>? parseFunc;
        private Type? type;

        public ExpressionPattern(Node parent) : base(parent) { }

        public override bool Match(MacroMatchExpr parent) {
            if (parent.Stream.PeekIs(TokenType.End)) {
                return false;
            }

            // leave expression non-starters to next token pattern.
            if (parent.Stream.PeekIs(Spec.NeverExprStartTypes)) {
                return true;
            }

            var startIdx = parent.Stream.TokenIdx;

            Node? e = null;
            if (parseFunc != null) {
                e = parseFunc(parent);
            }
            else if (type != null) {
                e = typeof(TypeName).IsAssignableFrom(type)
                    ? TypeName.Parse(parent)
                    : AnyExpr.Parse(parent);
            }

            if (e != null && (type?.IsInstanceOfType(e) ?? parseFunc != null)) {
                parent.Nodes.Add(e);
                return true;
            }
            parent.Stream.MoveAbsolute(startIdx);
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

        private const string exprPostfix = "Expr";

        private void PatternFromTypeName(string typeName) {
            if (!typeName.EndsWith(exprPostfix) && !typeName.EndsWith("TypeName")) {
                typeName += exprPostfix;
            }

            if (ParsingTypes.TryGetValue(typeName, out var t)) {
                type = t;
            }
            else if (ParsingFunctions.TryGetValue(
                typeName,
                out var fn
            )) {
                parseFunc = fn;
            }
        }

        public static readonly Dictionary<string, Func<Node, Node>>
            ParsingFunctions = new() {
                { exprPostfix, AnyExpr.Parse },
                { nameof(AnyExpr), AnyExpr.Parse },
                { nameof(InfixExpr), InfixExpr.Parse },
                { nameof(PrefixExpr), PrefixExpr.Parse },
                { nameof(PostfixExpr), PostfixExpr.Parse },
                { nameof(AtomExpr), AtomExpr.Parse },
                { nameof(ConstantExpr), ConstantExpr.ParseNew }
            };

        public static readonly Dictionary<string, Type> ParsingTypes = new() {
            { nameof(ScopeExpr), typeof(ScopeExpr) },
            { nameof(TypeName), typeof(TypeName) }
        };
    }
}
