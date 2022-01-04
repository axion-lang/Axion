using Axion.Core.Processing.Syntactic.Expressions.Atomic;
using Magnolia.Attributes;
using Magnolia.Trees;

namespace Axion.Core.Processing.Syntactic.Expressions.Statements;

[Branch]
public partial class ImportEntry : Node {
    public NameExpr FullName => Parent is NameExpr pn
        ? new NameExpr(this, pn.ToString())
        : Name;

    public NameExpr Name { get; }

    public NodeList<ImportEntry, Ast> Children { get; }

    public NodeList<NameExpr, Ast> Exceptions { get; }

    public NameExpr? Alias { get; init; }

    public ImportEntry(
        Node?                      parent,
        NameExpr                   name,
        NodeList<ImportEntry, Ast> children,
        NodeList<NameExpr, Ast>    exceptions
    ) : base(parent) {
        Name       = name;
        Children   = children;
        Exceptions = exceptions;
    }
}
