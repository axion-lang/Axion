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
        private readonly StringWriter       baseWriter;
        private readonly IndentedTextWriter writer;
        private          bool               lastLineEmpty;

        private readonly ConverterFromAxion converter;

        public int IndentLevel {
            get => writer.Indent;
            set => writer.Indent = value;
        }

        public CodeWriter(ProcessingOptions options) {
            baseWriter = new StringWriter();
            writer     = new IndentedTextWriter(baseWriter);
            if (options.HasFlag(ProcessingOptions.ToAxion)) {
                converter = new AxionToAxionConverter(this);
            }
            else if (options.HasFlag(ProcessingOptions.ToCSharp)) {
                converter = new AxionToCSharpConverter(this);
            }
            else if (options.HasFlag(ProcessingOptions.ToPython)) {
                converter = new AxionToPythonConverter(this);
            }
            else if (options.HasFlag(ProcessingOptions.ToPascal)) {
                converter = new AxionToPascalConverter(this);
            }
            else {
                throw new NotSupportedException(
                    $"Code building for '{options:G}' mode is not supported."
                );
            }
        }

        public void Write(params object[] values) {
            lastLineEmpty = false;
            foreach (object val in values) {
                if (val is Span translatable) {
                    try {
                        converter.Convert((dynamic) translatable);
                    }
                    catch {
                        Write(translatable.ToString());
                    }
                }
                else {
                    writer.Write(val);
                }
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
