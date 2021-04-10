using System.Collections.Generic;
using Axion.Core.Hierarchy;
using Axion.Core.Processing.Syntactic;
using Axion.Core.Processing.Syntactic.Expressions.TypeNames;
using Axion.SourceGenerators;
using Axion.Specification;

namespace Axion.Core.Processing.Lexical.Tokens {
    [SyntaxExpression]
    public partial class StringToken : Token {
        public bool IsUnclosed { get; }
        public string Prefixes { get; }
        public string Quote { get; }
        public string EndingQuotes { get; }

        [LeafSyntaxNode] NodeList<StringInterpolation>? interpolations;

        public bool IsMultiline => Quote.Length == Spec.MultilineStringQuotesCount;

        public override TypeName ValueType =>
            new SimpleTypeName(this, Spec.StringType);

        internal StringToken(
            Unit   unit,
            string value      = "",
            string content    = "",
            bool   isUnclosed = false,
            string prefixes   = "",
            string quote      = "\"",
            IEnumerable<StringInterpolation>? interpolations =
                null
        ) : base(
            unit,
            TokenType.String,
            value,
            content
        ) {
            IsUnclosed = isUnclosed;
            Prefixes = prefixes;
            Quote = quote;
            Interpolations = interpolations == null
                ? new NodeList<StringInterpolation>(this)
                : new NodeList<StringInterpolation>(this, interpolations);
            EndingQuotes = "";
        }

        public bool HasPrefix(string prefix) {
            return Prefixes.Contains(prefix.ToLowerInvariant())
                || Prefixes.Contains(prefix.ToUpperInvariant());
        }
    }

    public class StringInterpolation : Node {
        public StringInterpolation(Unit unit) : base(
            Unit.FromInterpolation(unit)
        ) { }
    }
}
