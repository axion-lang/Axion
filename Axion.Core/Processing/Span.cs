namespace Axion.Core.Processing {
    public struct Span {
        public readonly Position Start;
        public readonly Position End;

        public Span(Position start, Position end) {
            Start = start;
            End   = end;
        }

        public override string ToString() {
            return "start (" + Start + "), end (" + End + ")";
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
    }

    public struct Position {
        public readonly int Line;
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