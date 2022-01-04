using System.Collections.Generic;
using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Processing.Syntactic.Expressions.Atomic;
using Axion.Core.Processing.Syntactic.Expressions.Patterns;
using Magnolia.Attributes;
using static Axion.Specification.TokenType;

namespace Axion.Core.Processing.Syntactic.Expressions.Definitions;

/// <summary>
///     <code>
///         macro-def:
///             'macro' simple-name syntax-description scope;
///     </code>
/// </summary>
[Branch]
public partial class MacroDef : Node, IDefinitionExpr {
    [Leaf] Token? kwMacro;
    [Leaf] NameExpr? name;
    [Leaf] ScopeExpr scope = null!;
    [Leaf] CascadePattern syntax = null!;

    public Dictionary<string, string> NamedSyntaxParts { get; } = new();

    internal MacroDef(Node parent) : base(parent) { }

    public MacroDef Parse() {
        // TODO: find code, that can be replaced with macro by patterns
        // Example:
        // ========
        // macro post-condition-loop (
        //   'do',
        //   scope: Scope,
        //   ('while' | 'until'),
        //   condition: Infix
        // )
        //   if syntax[2] == 'while'
        //     condition = {{ not $condition }}
        // 
        //   return {{
        //     while true {
        //       $scope
        //       if $condition {
        //         break
        //       }
        //     }
        //   }}
        KwMacro = Stream.Eat(KeywordMacro);
        Name    = new NameExpr(this).Parse(true);
        Syntax  = new CascadePattern(this);
        // EBNF-based syntax definition
        if (Stream.MaybeEat(OpenParenthesis)) {
            Syntax.Parse();
            Stream.Eat(CloseParenthesis);
        }

        Scope = new ScopeExpr(this).Parse();
        return this;
    }
}
