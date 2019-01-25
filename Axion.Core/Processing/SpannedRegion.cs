namespace Axion.Core.Processing {
    public abstract class SpannedRegion {
        public Span Span { get; protected set; }

        // Region

        internal void MarkStart(SpannedRegion mark) {
            Span = new Span(mark.Span.Start, Span.End);
        }

        internal void MarkEnd(SpannedRegion mark) {
            Span = new Span(Span.Start, mark.Span.End);
        }

        internal void MarkPosition(SpannedRegion mark) {
            Span = mark.Span;
        }

        internal void MarkPosition(SpannedRegion start, SpannedRegion end) {
            Span = new Span(start.Span.Start, end.Span.End);
        }

        // Position

        internal void MarkStart(Position position) {
            Span = new Span(position, Span.End);
        }

        internal void MarkEnd(Position position) {
            Span = new Span(Span.Start, position);
        }

        internal void MarkPosition(Position start, Position end) {
            Span = new Span(start, end);
        }
    }
}