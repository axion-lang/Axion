using System.Collections.Generic;
using System.Linq;
using Axion.Core.Hierarchy;
using Axion.Core.Processing.Syntactic.Expressions.Common;
using Axion.Core.Processing.Syntactic.Expressions.Definitions;
using Axion.Specification;
using Magnolia.Trees;
using Magnolia.Trees.Paths;

namespace Axion.Core.Processing.Syntactic.Expressions;

/// <summary>
///     Abstract Syntax Tree built from source code.
/// </summary>
public class Ast : ScopeExpr {
    internal List<MacroDef> Macros =>
        (Unit.Module?.Definitions.OfType<MacroDef>() ?? Enumerable.Empty<MacroDef>())
        .ToList();

    public Ast(Unit unit) : base(null) {
        this.unit = unit;
        parent    = this;
        Path = new NodeTreePath<TreeNode<Ast>, Ast>(
            this,
            typeof(Unit).GetProperty(nameof(Hierarchy.Unit.Ast))!
        );
    }

    internal new void Parse() {
        Start = Stream[0].Start;
        End   = Stream[^1].End;

        while (!Stream.MaybeEat(TokenType.End)) {
            var item = AnyExpr.Parse(this);
            Items += item;
            if (item is IDefinitionExpr def) {
                Unit.Module?.AddDefinition(def);
            }
        }
    }

    public override IDefinitionExpr? GetDefByName(string name) {
        var defs = GetScopedDefs();
        return defs.FirstOrDefault(def => def.Name?.ToString() == name);
    }
}
