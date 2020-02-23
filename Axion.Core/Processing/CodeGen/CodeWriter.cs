using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using Axion.Core.Source;

namespace Axion.Core.Processing.CodeGen {
    /// <summary>
    ///     A wrapper around an <see cref="IndentedTextWriter"/>
    ///     that helps to generate code from Axion expressions
    ///     for multiple target languages.
    /// </summary>
    public class CodeWriter : IDisposable {
        private readonly ProcessingOptions  options;
        private readonly StringWriter       baseWriter;
        private readonly IndentedTextWriter writer;
        private          bool               lastLineEmpty;

        public int Indent {
            get => writer.Indent;
            set => writer.Indent = value;
        }

        public CodeWriter(ProcessingOptions options) {
            this.options = options;
            baseWriter   = new StringWriter();
            writer       = new IndentedTextWriter(baseWriter);
        }

        public void Write(params object[] values) {
            lastLineEmpty = false;
            if (options.HasFlag(ProcessingOptions.ToAxion)) {
                foreach (object val in values) {
                    if (val is Span translatable) {
                        translatable.ToAxion(this);
                    }
                    else {
                        writer.Write(val);
                    }
                }
            }
            else if (options.HasFlag(ProcessingOptions.ToCSharp)) {
                foreach (object val in values) {
                    if (val is Span translatable) {
                        translatable.ToCSharp(this);
                    }
                    else {
                        writer.Write(val);
                    }
                }
            }
            else if (options.HasFlag(ProcessingOptions.ToPython)) {
                foreach (object val in values) {
                    if (val is Span translatable) {
                        translatable.ToPython(this);
                    }
                    else {
                        writer.Write(val);
                    }
                }
            }
            else if (options.HasFlag(ProcessingOptions.ToPascal)) {
                foreach (object val in values) {
                    if (val is Span translatable) {
                        translatable.ToPascal(this);
                    }
                    else {
                        writer.Write(val);
                    }
                }
            }
            else {
                throw new NotSupportedException($"Code building for '{options:G}' mode is not supported.");
            }
        }

        public void WriteLine(params object[] values) {
            Write(values);
            writer.WriteLine();
            lastLineEmpty = true;
        }

        public void MaybeWriteLine() {
            if (!lastLineEmpty) {
                WriteLine();
            }
        }

        public void AddJoin<T>(string separator, IList<T> items, bool indent = false)
            where T : Span {
            if (items.Count == 0) {
                return;
            }

            for (var i = 0; i < items.Count - 1; i++) {
                Write(items[i], separator);
                if (indent) {
                    writer.WriteLine();
                }
            }

            Write(items[^1]);
        }

        public override string ToString() {
            return baseWriter.ToString();
        }

        public void Dispose() {
            baseWriter.Dispose();
            writer.Dispose();
        }
    }
}