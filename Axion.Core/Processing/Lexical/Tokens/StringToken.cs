using System.Collections.Generic;
using Axion.Core.Hierarchy;
using Axion.Core.Processing.Syntactic.Expressions;
using Axion.Core.Processing.Syntactic.Expressions.TypeNames;
using Axion.Specification;
using Magnolia.Attributes;
using Magnolia.Trees;

namespace Axion.Core.Processing.Lexical.Tokens;

[Branch]
public partial class StringToken : Token {
    readonly SimpleTypeName typeName;

    [Leaf] NodeList<StringInterpolation, Ast>? interpolations;

    public bool IsUnclosed { get; }
    public string Prefixes { get; }
    public string Quote { get; }
    public string EndingQuotes { get; }

    public bool IsMultiline => Quote.Length == Spec.MultilineStringQuotesCount;

    public override TypeName InferredType => typeName;

    internal StringToken(
        Unit                              unit,
        string                            value          = "",
        string                            content        = "",
        bool                              isUnclosed     = false,
        string                            prefixes       = "",
        string                            quote          = "\"",
        IEnumerable<StringInterpolation>? interpolations = null
    ) : base(
        unit,
        TokenType.String,
        value,
        content
    ) {
        IsUnclosed = isUnclosed;
        Prefixes   = prefixes;
        Quote      = quote;
        Interpolations = interpolations == null
            ? new NodeList<StringInterpolation, Ast>(this)
            : new NodeList<StringInterpolation, Ast>(this, interpolations);
        EndingQuotes = "";
        typeName     = new SimpleTypeName(this, Spec.StringType);
    }

    public bool HasPrefix(string prefix) {
        return Prefixes.Contains(prefix.ToLowerInvariant())
            || Prefixes.Contains(prefix.ToUpperInvariant());
    }
}

[Branch]
public partial class StringInterpolation : Node {
    public StringInterpolation(Unit unit) : base(
        Unit.FromInterpolation(unit)
    ) { }
}
