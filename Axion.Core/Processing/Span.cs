namespace Axion.Core.Processing {
    /// <summary>
    ///     Represents a (start, end) span
    ///     of some piece of source.
    /// </summary>
    public struct Span {
        public readonly Position StartPosition;
        public readonly Position EndPosition;

        public Span(Position start, Position end) {
            StartPosition = start;
            EndPosition   = end;
        }

        public override string ToString() {
            return "start (" + StartPosition + "), end (" + EndPosition + ")";
        }

        public override bool Equals(object obj) {
            return base.Equals(obj);
        }

        public bool Equals(Span other) {
            return StartPosition == other.StartPosition && EndPosition == other.EndPosition;
        }

        public override int GetHashCode() {
            unchecked {
                return (StartPosition.GetHashCode() * 397) ^ EndPosition.GetHashCode();
            }
        }

        public static bool operator ==(Span c1, Span c2) {
            return c1.Equals(c2);
        }

        public static bool operator !=(Span c1, Span c2) {
            return !c1.Equals(c2);
        }

        public static implicit operator (Position, Position)(Span span) {
            return (span.StartPosition, span.EndPosition);
        }

        public static implicit operator Span((Position, Position) positions) {
            return new Span(positions.Item1, positions.Item2);
        }
    }

    /// <summary>
    ///     Represents (line, column) position
    ///     in source code.
    /// </summary>
    public struct Position {
        /// <summary>
        ///     0-based.
        /// </summary>
        public readonly int Line;
        
        /// <summary>
        ///     0-based.
        /// </summary>
        public readonly int Column;

        internal Position(int line, int column) {
            Line   = line;
            Column = column;
        }

        public override string ToString() {
            return "line " + (Line + 1) + ", column " + (Column + 1);
        }

        public override bool Equals(object obj) {
            return base.Equals(obj);
        }

        public bool Equals(Position other) {
            return Line == other.Line && Column == other.Column;
        }

        public override int GetHashCode() {
            unchecked {
                return (Line * 397) ^ Column;
            }
        }

        public static bool operator ==(Position c1, Position c2) {
            return c1.Equals(c2);
        }

        public static bool operator !=(Position c1, Position c2) {
            return !c1.Equals(c2);
        }

        public static implicit operator (int, int)(Position position) {
            return (position.Line, position.Column);
        }

        public static implicit operator Position((int, int) position) {
            return new Position(position.Item1, position.Item2);
        }
    }
}