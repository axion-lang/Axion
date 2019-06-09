using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;

namespace Axion.Core.Processing.CodeGen {
    public class CodeBuilder : IDisposable {
        private readonly  StringWriter       baseWriter;
        internal readonly IndentedTextWriter Writer;
        private readonly  OutLang            outLang;

        public CodeBuilder(OutLang lang) {
            outLang    = lang;
            baseWriter = new StringWriter();
            Writer     = new IndentedTextWriter(baseWriter);
        }

        public void Write(params object[] values) {
            switch (outLang) {
            case OutLang.Axion: {
                for (var i = 0; i < values.Length; i++) {
                    object val = values[i];
                    if (val is SpannedRegion translatable) {
                        translatable.ToAxionCode(this);
                    }
                    else {
                        Writer.Write(val);
                    }
                }

                break;
            }

            case OutLang.CSharp: {
                for (var i = 0; i < values.Length; i++) {
                    object val = values[i];
                    if (val is SpannedRegion translatable) {
                        translatable.ToCSharpCode(this);
                    }
                    else {
                        Writer.Write(val);
                    }
                }

                break;
            }

            default: {
                throw new NotSupportedException();
            }
            }
        }

        internal void WriteLine(params object[] values) {
            Write(values);
            Writer.WriteLine();
        }

        public static implicit operator string(CodeBuilder sb) {
            return sb.ToString();
        }

        public override string ToString() {
            return baseWriter.ToString();
        }

        public void AddJoin<T>(string separator, IList<T> items, bool indent = false)
            where T : SpannedRegion {
            if (items.Count == 0) {
                return;
            }

            switch (outLang) {
            case OutLang.Axion: {
                for (var i = 0; i < items.Count - 1; i++) {
                    items[i]?.ToAxionCode(this);
                    Write(separator);
                    if (indent) {
                        Writer.WriteLine();
                    }
                }

                items[items.Count - 1]?.ToAxionCode(this);
                return;
            }

            case OutLang.CSharp: {
                for (var i = 0; i < items.Count - 1; i++) {
                    items[i]?.ToCSharpCode(this);
                    Write(separator);
                    if (indent) {
                        Writer.WriteLine();
                    }
                }

                items[items.Count - 1]?.ToCSharpCode(this);
                return;
            }

            default: {
                throw new NotSupportedException();
            }
            }
        }

        protected bool Equals(CodeBuilder other) {
            return Equals(Writer.ToString(), other.Writer.ToString())
                   && outLang == other.outLang;
        }

        public override bool Equals(object obj) {
            return Equals((CodeBuilder) obj);
        }

        public override int GetHashCode() {
            unchecked {
                return ((Writer != null ? Writer.GetHashCode() : 0) * 397) ^ (int) outLang;
            }
        }

        public void Dispose() {
            baseWriter.Dispose();
            Writer.Dispose();
        }
    }

    public enum OutLang {
        Axion,
        CSharp
    }
}