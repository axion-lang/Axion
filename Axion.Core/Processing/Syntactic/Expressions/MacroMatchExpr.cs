using System.Collections.Generic;
using System.Linq;
using Axion.Core.Processing.Syntactic.Expressions.Common;
using Axion.Core.Processing.Syntactic.Expressions.Definitions;
using Axion.Core.Processing.Syntactic.Expressions.Patterns;
using Magnolia.Trees;

namespace Axion.Core.Processing.Syntactic.Expressions;

/// <summary>
///     Actually, it's not an exact expression,
///     but some piece of code defined as expression
///     by language macros.
/// </summary>
public class MacroMatchExpr : AtomExpr {
    public MacroDef? Macro { get; set; }

    public List<Node> Nodes { get; } = new();

    public MacroMatchExpr(Node parent) : base(parent) { }

    public MacroMatchExpr Parse() {
        var startIdx = Stream.TokenIdx;

        foreach (var macro in Root?.Macros ?? Enumerable.Empty<MacroDef>()) {
            if (macro.Syntax.Patterns.Count > 0
             && macro.Syntax.Patterns[0] is TokenPattern
             && macro.Syntax.Match(this)) {
                Macro = macro;
                break;
            }
            // reset for next macro
            Nodes.Clear();
            Stream.MoveAbsolute(startIdx);
        }
        if (Macro == null) {
            return this;
        }

        Start = Nodes[0].Start;
        End   = Nodes[^1].End;
        return this;
    }

    public new MacroMatchExpr Parse(Node leftExpr) {
        var startIdx = Stream.TokenIdx;

        foreach (var macro in Root?.Macros ?? Enumerable.Empty<MacroDef>()) {
            if (macro.Syntax.Patterns.Count > 1
             && macro.Syntax.Patterns[1] is TokenPattern t
             && t.Match(this)) {
                Macro = macro;
                break;
            }
            // reset for next macro
            Nodes.Clear();
            Stream.MoveAbsolute(startIdx);
        }
        if (Macro == null) {
            return this;
        }

        var restCascade = new CascadePattern(this) {
            Patterns = new NodeList<Pattern, Ast>(
                this,
                Macro.Syntax.Patterns.Skip(2)
            )
        };
        Nodes.Insert(0, leftExpr);
        if (restCascade.Match(this)) {
            Start = Nodes[0].Start;
            End   = Nodes[^1].End;
        }
        return this;
    }
}
