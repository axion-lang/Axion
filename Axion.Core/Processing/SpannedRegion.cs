namespace Axion.Core.Processing {
    /// <summary>
    ///     Abstract 'extension-class'
    ///     for tokens, expressions and statements.
    /// </summary>
    public abstract class SpannedRegion {
        public Span Span { get; protected set; }

        // Region

        internal void MarkStart(SpannedRegion mark) {
            Span = new Span(mark.Span.StartPosition, Span.EndPosition);
        }

        internal void MarkEnd(SpannedRegion mark) {
            Span = new Span(Span.StartPosition, mark.Span.EndPosition);
        }

        internal void MarkPosition(SpannedRegion mark) {
            Span = mark.Span;
        }

        internal void MarkPosition(SpannedRegion start, SpannedRegion end) {
            Span = new Span(start.Span.StartPosition, end.Span.EndPosition);
        }

        // Position

        internal void MarkStart(Position position) {
            Span = new Span(position, Span.EndPosition);
        }

        internal void MarkEnd(Position position) {
            Span = new Span(Span.StartPosition, position);
        }

        internal void MarkPosition(Position start, Position end) {
            Span = new Span(start, end);
        }
    }
}