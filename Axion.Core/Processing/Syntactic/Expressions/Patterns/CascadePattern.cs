using System.Linq;
using Axion.Core.Processing.Errors;
using static Axion.Specification.TokenType;

namespace Axion.Core.Processing.Syntactic.Expressions.Patterns {
    /// <summary>
    ///     <c>
    ///         cascade-pattern:
    ///             syntax-pattern {',' syntax-pattern};
    ///     </c>
    /// </summary>
    public class CascadePattern : Pattern {
        private NodeList<Pattern>? patterns;

        internal NodeList<Pattern> Patterns {
            get => InitIfNull(ref patterns);
            set => patterns = Bind(value);
        }

        public CascadePattern(Node parent) : base(parent) { }

        public override bool Match(MacroMatchExpr parent) {
            var startIdx = parent.Stream.TokenIdx;
            var nodesLen = parent.Nodes.Count;
            if (Patterns.All(pattern => pattern.Match(parent))) {
                return true;
            }
            parent.Stream.MoveAbsolute(startIdx);
            parent.Nodes.RemoveRange(nodesLen, parent.Nodes.Count - nodesLen);
            return false;
        }

        public CascadePattern Parse() {
            do {
                Pattern pattern;
                // syntax group `(x, y)`
                if (Stream.PeekIs(OpenParenthesis)) {
                    pattern = new GroupPattern(this).Parse();
                }
                // optional pattern `[x]`
                else if (Stream.PeekIs(OpenBracket)) {
                    pattern = new OptionalPattern(this).Parse();
                }
                // multiple pattern `{x}`
                else if (Stream.PeekIs(OpenBrace)) {
                    pattern = new MultiplePattern(this).Parse();
                }
                // custom keyword
                else if (Stream.PeekIs(String)) {
                    pattern = new TokenPattern(this).Parse();
                }
                // expr-name `TypeName`
                else if (Stream.PeekIs(Identifier)) {
                    pattern = new ExpressionPattern(this).Parse();
                }
                else {
                    // TODO error
                    LanguageReport.To(BlameType.InvalidSyntax, Stream.Peek);
                    continue;
                }

                // or pattern `x | y`
                if (Stream.PeekIs(Pipe)) {
                    Patterns += new OrPattern(this) {
                        Left = pattern
                    }.Parse();
                }
                else {
                    Patterns += pattern;
                }
            } while (Stream.MaybeEat(Comma));

            return this;
        }
    }
}
