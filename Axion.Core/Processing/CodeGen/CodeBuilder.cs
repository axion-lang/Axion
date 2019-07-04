using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Axion.Core.Processing.Syntactic;
using Axion.Core.Processing.Syntactic.Atomic;
using Axion.Core.Specification;

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

        internal bool WriteDecorators(NodeList<Expression> decorators) {
            var haveAccessMod = false;
            for (var i = 0; i < decorators?.Count; i++) {
                Expression modifier = decorators[i];
                if (modifier is NameExpression n && Spec.CSharp.AccessModifiers.Contains(n.Name)) {
                    haveAccessMod = true;
                }

                Write(modifier, " ");
                if (i == decorators.Count - 1) {
                    Write(" ");
                }
                else {
                    Write(", ");
                }
            }

            return haveAccessMod;
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

        public static implicit operator string(CodeBuilder sb) {
            return sb.ToString();
        }

        public override string ToString() {
            return baseWriter.ToString();
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