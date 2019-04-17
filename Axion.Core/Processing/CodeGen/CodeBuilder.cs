using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using Axion.Core.Processing.Syntactic;

namespace Axion.Core.Processing.CodeGen {
    internal class CodeBuilder : IDisposable {
        private readonly  StringWriter       baseWriter;
        internal readonly IndentedTextWriter Writer;
        private readonly  OutLang            outLang;

        public CodeBuilder(OutLang lang) {
            outLang    = lang;
            baseWriter = new StringWriter();
            Writer     = new IndentedTextWriter(baseWriter);
        }

        internal void Write(params object[] values) {
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
            for (var i = 0; i < values.Length; i++) {
                object val = values[i];
                if (val is SyntaxTreeNode n) {
                    n.ToAxionCode(this);
                }
                else {
                    Write(val);
                }
            }

            Writer.WriteLine();
        }

//        public static CodeBuilder operator +(CodeBuilder sb, string s) {
//            sb.Write(s);
//            return sb;
//        }

        public static implicit operator string(CodeBuilder sb) {
            return sb.ToString();
        }

        public override string ToString() {
            return baseWriter.ToString();
        }

//        public static CodeBuilder operator +(CodeBuilder b, SpannedRegion node) {
//            switch (b.outLang) {
//                case OutLang.Axion: {
//                    node?.ToAxionCode(b);
//                    return b;
//                }
//
//                case OutLang.CSharp: {
//                    node?.ToCSharpCode(b);
//                    return b;
//                }
//
//                default: {
//                    throw new NotSupportedException();
//                }
//            }
//        }

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

        public void Dispose() {
            baseWriter.Dispose();
            Writer.Dispose();
        }
    }

    internal enum OutLang {
        Axion,
        CSharp
    }
}