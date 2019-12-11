using System;
using Axion.Core.Processing.CodeGen;
using Axion.Core.Source;
using Newtonsoft.Json;

namespace Axion.Core.Processing {
    /// <summary>
    ///     (start, end) span of code in source.
    /// </summary>
    public class Span {
        [JsonIgnore]
        public SourceUnit Source;

        public Location Start { get; private set; }
        public Location End { get; private set; }

        public Span(SourceUnit source, Location start = default, Location end = default) {
            Source = source;
            Start  = start;
            End    = end;
        }

        protected void MarkStart(Location start) {
            Start = start;
        }

        protected void MarkEnd(Location end) {
            End = end;
        }

        protected void MarkStart(Span mark) {
            Start = mark.Start;
        }

        protected void MarkEnd(Span mark) {
            End = mark.End;
        }

        protected void MarkPosition(Span mark) {
            Start = mark.Start;
            End   = mark.End;
        }

        protected void MarkPosition(Span start, Span end) {
            Start = start.Start;
            End   = end.End;
        }

        /// <summary>
        ///     Converts this code span
        ///     to it's string representation in Axion language.
        /// </summary>
        public virtual void ToAxion(CodeWriter c) {
            throw new NotSupportedException($"{GetType().FullName}");
        }

        /// <summary>
        ///     Converts this code span
        ///     to it's string representation in C# language.
        /// </summary>
        public virtual void ToCSharp(CodeWriter c) {
            throw new NotSupportedException($"{GetType().FullName}");
        }

        /// <summary>
        ///     Converts this code span
        ///     to it's string representation in Python language.
        /// </summary>
        public virtual void ToPython(CodeWriter c) {
            throw new NotSupportedException($"{GetType().FullName}");
        }

        /// <summary>
        ///     Converts this code span
        ///     to it's string representation in Pascal language.
        /// </summary>
        public virtual void ToPascal(CodeWriter c) {
            throw new NotSupportedException($"{GetType().FullName}");
        }

        public override string ToString() {
            return "from " + Start + " to " + End;
        }
    }

    /// <summary>
    ///     (line, column) position of code in source (0-based).
    ///     Convertible to (int, int) 2-tuple.
    /// </summary>
    public struct Location {
        public readonly int Line;
        public readonly int Column;

        internal Location(int line, int column) {
            Line   = line;
            Column = column;
        }

        public override string ToString() {
            return $"{Line + 1}, {Column + 1}";
        }

        public static implicit operator Location((int, int) position) {
            (int line, int column) = position;
            return new Location(line, column);
        }
    }
}