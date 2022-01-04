using System;
using System.Collections.Generic;
using Axion.Core.Processing.Errors;
using Axion.Core.Processing.Syntactic.Expressions.Atomic;
using Axion.Core.Processing.Syntactic.Expressions.Common;
using Axion.Core.Processing.Syntactic.Expressions.Definitions;
using Axion.Core.Processing.Syntactic.Expressions.TypeNames;
using Axion.Specification;
using Magnolia.Attributes;
using static Axion.Specification.TokenType;

namespace Axion.Core.Processing.Syntactic.Expressions.Patterns;

/// <summary>
///     <code>
///         expression-pattern:
///             name-expr [':' type-name];
///     </code>
/// </summary>
[Branch]
public partial class ExpressionPattern : Pattern {
    const string exprPostfix = "Expr";

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
    Func<Node, Node>? parseFunc;
    Type? type;

    public ExpressionPattern(Node parent) : base(parent) { }

    public override bool Match(MacroMatchExpr match) {
        if (match.Stream.PeekIs(TokenType.End)) {
            return false;
        }

        // leave expression non-starters to next token pattern.
        if (match.Stream.PeekIs(Spec.NeverExprStartTypes)) {
            return true;
        }

        var startIdx = match.Stream.TokenIdx;

        Node? e = null;
        if (parseFunc != null) {
            e = parseFunc(match);
        }
        else if (type != null) {
            e = typeof(TypeName).IsAssignableFrom(type)
                ? TypeName.Parse(match)
                : AnyExpr.Parse(match);
        }

        if (e != null && (type?.IsInstanceOfType(e) ?? parseFunc != null)) {
            match.Nodes.Add(e);
            return true;
        }
        match.Stream.MoveAbsolute(startIdx);
        return false;
    }

    public ExpressionPattern Parse() {
        var id = Stream.Eat(Identifier);
        var namedParts = GetParent<MacroDef>()!.NamedSyntaxParts;
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

    void PatternFromTypeName(string typeName) {
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
}
