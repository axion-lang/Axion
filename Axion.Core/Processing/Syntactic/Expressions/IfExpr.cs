﻿using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Processing.Syntactic.Expressions.Common;
using Magnolia.Attributes;
using static Axion.Specification.TokenType;

namespace Axion.Core.Processing.Syntactic.Expressions;

/// <summary>
///     <code>
///         conditional-expr:
///             'if' infix-expr scope
///             {'elif' infix-expr scope}
///             ['else' scope];
///     </code>
/// </summary>
[Branch]
public partial class IfExpr : Node {
    [Leaf] Token? branchKw;
    [Leaf] Node? condition;
    [Leaf] ScopeExpr? elseScope;
    [Leaf] ScopeExpr? thenScope;

    internal IfExpr(Node parent) : base(parent) { }

    public IfExpr Parse(bool elseIf = false) {
        if (!elseIf) {
            BranchKw = Stream.Eat(KeywordIf);
        }

        Condition = InfixExpr.Parse(this);
        ThenScope = new ScopeExpr(this).Parse();

        if (Stream.MaybeEat(KeywordElse)) {
            ElseScope = new ScopeExpr(this).Parse();
        }
        else if (Stream.MaybeEat(KeywordElif)) {
            ElseScope = new ScopeExpr(this) {
                Items = {
                    new IfExpr(this) {
                        BranchKw = Stream.Token
                    }.Parse(true)
                }
            };
        }

        return this;
    }
}
