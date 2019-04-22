using System;
using Axion.Core.Processing.CodeGen;

namespace Axion.Core.Processing {
    /// <summary>
    ///     Abstract 'extension-class'
    ///     for tokens, expressions and statements.
    /// </summary>
    public abstract class SpannedRegion {
        public Span Span { get; protected set; }

        public bool ShouldSerializeSpan() {
            return !Compiler.Options.HasFlag(SourceProcessingOptions.ShowAstJson);
        }

        // Region
        protected void MarkStart(SpannedRegion mark) {
            Span = new Span(mark.Span.StartPosition, Span.EndPosition);
        }

        protected void MarkEnd(SpannedRegion mark) {
            Span = new Span(Span.StartPosition, mark.Span.EndPosition);
        }

        protected void MarkPosition(SpannedRegion mark) {
            Span = mark.Span;
        }

        protected void MarkPosition(SpannedRegion start, SpannedRegion end) {
            Span = new Span(start.Span.StartPosition, end.Span.EndPosition);
        }

        public virtual void ToOriginalAxionCode(CodeBuilder c) {
            throw new NotSupportedException();
        }

        public abstract void ToAxionCode(CodeBuilder  c);
        public abstract void ToCSharpCode(CodeBuilder c);
    }
}