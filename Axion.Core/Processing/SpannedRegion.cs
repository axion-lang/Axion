using System;

namespace Axion.Core.Processing {
    /// <summary>
    ///     Abstract 'extension-class'
    ///     for tokens, expressions and statements.
    /// </summary>
    public abstract class SpannedRegion {
        public Span Span { get; protected set; }

        // Region

        internal dynamic MarkStart(SpannedRegion mark) {
            Span = new Span(mark.Span.StartPosition, Span.EndPosition);
            return this;
        }

        internal dynamic MarkEnd(SpannedRegion mark) {
            Span = new Span(Span.StartPosition, mark.Span.EndPosition);
            return this;
        }

        internal dynamic MarkPosition(SpannedRegion mark) {
            Span = mark.Span;
            return this;
        }

        internal dynamic MarkPosition(SpannedRegion start, SpannedRegion end) {
            Span = new Span(start.Span.StartPosition, end.Span.EndPosition);
            return this;
        }

        internal virtual AxionCodeBuilder ToAxionCode(AxionCodeBuilder c) {
            throw new InvalidOperationException();
        }

        internal virtual CSharpCodeBuilder ToCSharpCode(CSharpCodeBuilder c) {
            throw new InvalidOperationException();
        }
    }
}