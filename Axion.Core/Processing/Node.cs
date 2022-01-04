using Axion.Core.Hierarchy;
using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Processing.Syntactic;
using Axion.Core.Processing.Syntactic.Expressions;
using Axion.Core.Processing.Syntactic.Expressions.TypeNames;
using Axion.Core.Processing.Translation;
using Magnolia.Attributes;
using Magnolia.Interface;
using Magnolia.Trees;

namespace Axion.Core.Processing;

/// <summary>
///     Span of source code / Tree leaf with parent and children nodes.
///     <code>
///         multiple-expr:
///             expr {',' expr};
///         multiple-infix:
///             infix-expr {',' infix-expr};
///         simple-multiple-name:
///             simple-name-expr {',' simple-name-expr};
///         single-expr:
///             conditional-expr | while-expr | for-expr    |
///             try-expr         | with-expr  | import-expr |
///             decorated;
///         decorated:
///             module-def | class-def  | enum-def |
///             func-def   | small-expr;
///         small-expr:
///             pass-expr | expr-expr | flow-expr;
///         flow-expr:
///             break-expr | continue-expr | return-expr |
///             raise-expr | yield-expr;
///     </code>
/// </summary>
[Branch]
public abstract partial class Node : TreeNode<Ast>, ICodeSpan, ITranslatableNode {
    bool firstSpanMarking = true;

    /// <summary>
    ///     Language type-name of this node that can be inferred from context.
    /// </summary>
    [Leaf(Virtual = true), NotTraversable] TypeName? inferredType;

    protected Unit unit;

    [NotTraversable]
    public new Node? Parent {
        get => (Node?) base.Parent;
        protected internal set => base.Parent = value;
    }

    public TokenStream Stream => Unit.TokenStream;

    /// <summary>
    ///     Constructor for <see cref="Token" />s.
    /// </summary>
    protected Node(
        Unit     unit,
        Location start = default,
        Location end   = default
    ) {
        this.unit = unit;
        Start     = start;
        End       = end;
    }

    /// <summary>
    ///     Constructor for expressions.
    /// </summary>
    protected Node(Node? parent) {
        this.parent = parent;
        if (parent == null) {
            return;
        }

        unit  = parent.Unit;
        Start = parent.Start;
        End   = parent.End;
    }

    public Unit Unit {
        get => unit;
        set {
            unit = value;
            Traverse<Node>(
                child => {
                    child.unit = value;
                }
            );
        }
    }

    public Location Start { get; set; }
    public Location End { get; set; }

    /// <summary>
    ///     Extends this span of code
    ///     if provided mark is out of existing span.
    /// </summary>
    public override void AfterPropertyBinding<T>(T? value) where T : class {
        if (value is not Node n) {
            return;
        }

        // if span is marked first time, set span equal to starting one
        // to prevent new node spanning from (1,1) to end.
        if (firstSpanMarking) {
            Start            = n.Start;
            End              = n.End;
            firstSpanMarking = false;
            return;
        }

        if (n.Start < Start) {
            Start = n.Start;
        }

        if (n.End > End) {
            End = n.End;
        }

        // fix negative span
        if (End < Start) {
            End = Start;
        }
    }

    /// <summary>
    ///     Extends this span of code
    ///     if any of provided marks is out of existing span.
    /// </summary>
    public override void AfterPropertyBinding<T>(INodeList<T, Ast> value) {
        if (value.IsEmpty
         || value[0] is not Node a
         || value[^1] is not Node b) {
            return;
        }

        // if span is marked first time, select least span of a & b.
        // to prevent new node spanning from (1,1) to end.
        if (firstSpanMarking) {
            Start            = Location.Max(a.Start, b.Start);
            End              = Location.Min(a.End, b.End);
            firstSpanMarking = false;
            return;
        }

        if (a.Start < Start) {
            Start = a.Start;
        }
        else if (b.Start < Start) {
            Start = b.Start;
        }

        if (b.End > End) {
            End = b.End;
        }
        else if (a.End > End) {
            End = a.End;
        }

        // fix negative span
        if (End < Start) {
            End = Start;
        }
    }

    public override string ToString() {
        return "from " + Start + " to " + End;
    }
}
