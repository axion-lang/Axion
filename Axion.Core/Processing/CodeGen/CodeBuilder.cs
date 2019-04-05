using System;
using System.Collections.Generic;
using System.Text;

namespace Axion.Core.Processing.CodeGen {
    /// <summary>
    ///     This wraps the .NET <c>StringBuilder</c> in a slightly more easy to use format.
    /// </summary>
    internal class CodeBuilder {
        private readonly StringBuilder builder;
        private readonly OutLang       outLang;

        public CodeBuilder(OutLang lang) {
            outLang = lang;
            builder = new StringBuilder();
        }

        private CodeBuilder Append(string s) {
            builder.Append(s);
            return this;
        }

        public static CodeBuilder operator +(CodeBuilder sb, string s) {
            return sb.Append(s);
        }

        public static implicit operator string(CodeBuilder sb) {
            return sb.ToString();
        }

        public override string ToString() {
            return builder.ToString();
        }

        public static CodeBuilder operator +(CodeBuilder b, SpannedRegion node) {
            switch (b.outLang) {
                case OutLang.Axion: {
                    return node.ToAxionCode(b);
                }

                case OutLang.CSharp: {
                    return node.ToCSharpCode(b);
                }

                default: {
                    throw new NotSupportedException();
                }
            }
        }

        public CodeBuilder AppendJoin<T>(string separator, IList<T> items)
            where T : SpannedRegion {
            if (items.Count == 0) {
                return this;
            }

            switch (outLang) {
                case OutLang.Axion: {
                    for (var i = 0; i < items.Count - 1; i++) {
                        items[i].ToAxionCode(this).Append(separator);
                    }

                    items[items.Count - 1].ToAxionCode(this);
                    return this;
                }

                case OutLang.CSharp: {
                    for (var i = 0; i < items.Count - 1; i++) {
                        items[i].ToCSharpCode(this).Append(separator);
                    }

                    items[items.Count - 1].ToCSharpCode(this);
                    return this;
                }

                default: {
                    throw new NotSupportedException();
                }
            }
        }
    }

    internal enum OutLang {
        Axion,
        CSharp
    }
}