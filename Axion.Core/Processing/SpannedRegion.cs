using System;
using Axion.Core.Processing.CodeGen;
using Axion.Core.Processing.Syntactic.Expressions.TypeNames;
using Newtonsoft.Json;

namespace Axion.Core.Processing {
    /// <summary>
    ///     Abstract 'extension-class'
    ///     for tokens, expressions and statements.
    /// </summary>
    public abstract class SpannedRegion {
        public Span Span { get; protected set; }

        public bool ShouldSerializeSpan() {
            return false;
        }

        // Region
        protected void MarkStart(SpannedRegion mark) {
            Span = new Span(mark.Span.Start, Span.End);
        }

        protected void MarkEnd(SpannedRegion mark) {
            Span = new Span(Span.Start, mark.Span.End);
        }

        protected void MarkPosition(SpannedRegion mark) {
            Span = mark.Span;
        }

        protected void MarkPosition(SpannedRegion start, SpannedRegion end) {
            Span = new Span(start.Span.Start, end.Span.End);
        }

        [JsonIgnore]
        public virtual TypeName ValueType {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }

        [JsonProperty]
        public string ValueTypeString {
            get {
                try {
                    return ValueType.ToString();
                }
                catch {
                    return null;
                }
            }
        }

        /// <summary>
        ///     Converts this code span
        ///     to it's string representation in Axion language.
        /// </summary>
        internal abstract void ToAxionCode(CodeBuilder c);

        /// <summary>
        ///     Converts this code span
        ///     to it's string representation in C# language.
        /// </summary>
        internal abstract void ToCSharpCode(CodeBuilder c);
    }
}