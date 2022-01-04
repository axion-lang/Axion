using System.Collections.Generic;
using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Processing.Syntactic.Expressions.Atomic;
using Magnolia.Attributes;
using Magnolia.Trees;
using static Axion.Specification.TokenType;

namespace Axion.Core.Processing.Syntactic.Expressions.Definitions;

/// <summary>
///     <code>
///         module-def:
///             'module' name scope;
///     </code>
/// </summary>
[Branch]
public partial class ModuleDef : Node, IDefinitionExpr, IDecorableExpr {
    [Leaf] Token? kwModule;
    [Leaf] NameExpr? name;
    [Leaf] ScopeExpr scope = null!;

    public ModuleDef(Node parent) : base(parent) { }

    public DecoratedExpr WithDecorators(params Node[] items) {
        return new(Parent) {
            Target     = this,
            Decorators = new NodeList<Node, Ast>(this, items)
        };
    }

    public ModuleDef WithScope(params Node[] items) {
        return WithScope((IEnumerable<Node>) items);
    }

    public ModuleDef WithScope(IEnumerable<Node> items) {
        Scope = new ScopeExpr(this).WithItems(items);
        return this;
    }

    public ModuleDef Parse() {
        KwModule = Stream.Eat(KeywordModule);
        Name     = new NameExpr(this).Parse();
        Scope    = new ScopeExpr(this).Parse();
        return this;
    }
}
