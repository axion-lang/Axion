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
        public SourceUnit Source { get; set; }

        public Location Start { get; private set; }
        public Location End   { get; private set; }

        public Span(SourceUnit source, Location start = default, Location end = default) {
            Source = source;
            Start  = start;
            End    = end;
        }

        internal void MarkStart(Location start) {
            Start = start;
        }

        internal void MarkEnd(Location end) {
            End = end;
        }

        internal void MarkStart(Span? mark) {
            Start = mark?.Start ?? Start;
        }

        internal void MarkEnd(Span? mark) {
            End = mark?.End ?? End;
        }

        internal void MarkPosition(Span mark) {
            Start = mark.Start;
            End   = mark.End;
        }

        internal void MarkPosition(Span start, Span end) {
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
}