namespace Axion.Core.Processing {
    /// <summary>
    ///     (start, end) span
    ///     of code in source.
    /// </summary>
    public struct Span {
        public readonly Position Start;
        public readonly Position End;

        public Span(Position start, Position end) {
            Start = start;
            End   = end;
        }

        public override string ToString() {
            return "start: " + Start + ", end: " + End;
        }

        public override bool Equals(object obj) {
            return base.Equals(obj);
        }

        public bool Equals(Span other) {
            return Start == other.Start && End == other.End;
        }

        public override int GetHashCode() {
            unchecked {
                return (Start.GetHashCode() * 397) ^ End.GetHashCode();
            }
        }

        public static bool operator ==(Span c1, Span c2) {
            return c1.Equals(c2);
        }

        public static bool operator !=(Span c1, Span c2) {
            return !c1.Equals(c2);
        }

        public static implicit operator (Position, Position)(Span span) {
            return (span.Start, span.End);
        }

        public static implicit operator Span((Position, Position) positions) {
            return new Span(positions.Item1, positions.Item2);
        }
    }

    /// <summary>
    ///     (line, column) position of code in source.
    ///     Convertible to (int, int) 2-tuple.
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
            return $"{Line + 1}, {Column + 1}";
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

        public static bool operator >(Position c1, Position c2) {
            return c1.Line > c2.Line
                || c1.Line == c2.Line && c1.Column > c2.Column;
        }

        public static bool operator <(Position c1, Position c2) {
            return c1.Line < c2.Line
                || c1.Line == c2.Line && c1.Column < c2.Column;
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

        public static Position operator +(Position a, Position b) {
            return new Position(a.Line + b.Line, a.Column + b.Column);
        }

        public static Position operator -(Position a, Position b) {
            return new Position(a.Line - b.Line, a.Column - b.Column);
        }
    }
}