using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;

namespace Axion.Core.Processing.Translation {
    /// <summary>
    ///     A wrapper around an <see cref="IndentedTextWriter"/>
    ///     that helps to generate code from Axion expressions
    ///     for multiple target languages.
    /// </summary>
    public class CodeWriter : IDisposable {
        readonly StringWriter baseWriter;
        readonly IndentedTextWriter writer;
        bool lastLineEmpty;

        readonly INodeTranslator translator;
        static readonly INodeTranslator fallbackTranslator =
            Compiler.Translators["axion"];

        public string OutputFileExtension => translator.OutputFileExtension;

        public int IndentLevel {
            get => writer.Indent;
            set => writer.Indent = value;
        }

        public static readonly CodeWriter Default = new(fallbackTranslator);

        public CodeWriter(INodeTranslator translator) {
            baseWriter      = new StringWriter();
            writer          = new IndentedTextWriter(baseWriter);
            this.translator = translator;
        }

        public void Write(params object?[] values) {
            lastLineEmpty = false;
            foreach (var v in values) {
                if (v is ITranslatableNode node) {
                    if (!translator.Translate(this, node)) {
                        // NOTE: Fallback converter
                        fallbackTranslator.Translate(this, node);
                    }
                }
                else {
                    writer.Write(v);
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

        public void AddJoin<T>(
            string   separator,
            IList<T> items,
            bool     indent = false
        ) where T : ITranslatableNode? {
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
