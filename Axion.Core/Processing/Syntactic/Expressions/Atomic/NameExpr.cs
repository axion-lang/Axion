using System.Linq;
using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Processing.Syntactic.Expressions.Common;
using Axion.Core.Processing.Syntactic.Expressions.TypeNames;
using Magnolia.Attributes;
using Magnolia.Trees;
using static Axion.Specification.TokenType;

namespace Axion.Core.Processing.Syntactic.Expressions.Atomic;

/// <summary>
///     <code>
///         name-expr:
///             ID {'.' ID};
///     </code>
/// </summary>
[Branch]
public partial class NameExpr : AtomExpr {
    [Leaf] NodeList<Token, Ast>? tokens;

    public Token[] Qualifiers => Tokens.OfType(Identifier);
    public bool IsSimple => Qualifiers.Length == 1;

    public override TypeName? InferredType =>
        ((Node?) GetParent<ScopeExpr>()?.GetDefByName(this))?.InferredType;

    public NameExpr(Node parent) : base(parent) { }

    public NameExpr(string name) : this(null, name) { }

    public NameExpr(Node? parent, string name) : base(parent) {
        if (name.Contains('.')) {
            var qs = name.Split('.');
            for (var i = 0; i < qs.Length; i++) {
                var q = qs[i];
                Tokens += new Token(Unit, Identifier, q);
                if (i != qs.Length - 1) {
                    Tokens += new Token(Unit, Dot, ".");
                }
            }
        }
        else {
            Tokens += new Token(Unit, Identifier, name);
        }
    }

    public static implicit operator NameExpr(string name) {
        return new(null, name);
    }

    public NameExpr Parse(bool simple = false) {
        Tokens += Stream.Eat(Identifier);
        if (simple) {
            return this;
        }

        while (Stream.PeekIs(Dot)) {
            Tokens += Stream.Eat();
            Tokens += Stream.Eat(Identifier);
        }

        return this;
    }

    public override string ToString() {
        return string.Join(".", Qualifiers.Select(q => q.Content));
    }
}
