using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Processing.Syntactic.Expressions;
using Axion.Core.Processing.Syntactic.Expressions.Atomic;
using Axion.Core.Processing.Syntactic.Expressions.Definitions;
using Axion.Core.Processing.Syntactic.Expressions.Postfix;
using Axion.Core.Processing.Syntactic.Expressions.Statements;
using Axion.Core.Processing.Syntactic.Expressions.TypeNames;

namespace Axion.Core.Processing.Emitting {
    public class ConverterToAxion : ConverterFromAxion {
        public override string OutputFileExtension => ".ax";

        public ConverterToAxion(CodeWriter cw) : base(cw) { }

        public override void Convert(CharToken e) {
            cw.Write(e.Value);
        }

        public override void Convert(CommentToken e) {
            cw.Write(e.Value);
        }

        public override void Convert(StringToken e) {
            cw.Write(e.Value);
        }

        public override void Convert(NameExpr e) {
            // Preserve original code formatting
            cw.AddJoin(".", e.Qualifiers);
        }

        public override void Convert(FunctionDef e) {
            cw.Write("fn ");
            if (e.Name != null) {
                cw.Write(e.Name, " ");
            }

            if (e.Parameters.Count > 0) {
                cw.Write("(");
                cw.AddJoin(", ", e.Parameters);
                cw.Write(") ");
            }

            if (e.ValueType != null) {
                cw.Write("-> ", e.ValueType);
            }

            cw.Write(e.Scope);
        }

        public override void Convert(MacroDef e) {
            cw.Write(
                "macro ",
                e.Name,
                "(",
                e.Syntax,
                ")",
                e.Scope
            );
        }

        public override void Convert(ModuleDef e) {
            cw.Write("module ", e.Name, e.Scope);
        }

        public override void Convert(NameDef e) {
            cw.Write(e.Name);
            if (e.ValueType != null) {
                cw.Write(": ", e.ValueType);
            }

            if (e.Value != null) {
                cw.Write(" = ", e.Value);
            }
        }

        public override void Convert(VarDef e) {
            if (e.IsImmutable) {
                cw.Write("let ");
            }

            cw.Write(e.Name);
            if (e.ValueType != null) {
                cw.Write(": ", e.ValueType);
            }

            if (e.Value != null) {
                cw.Write(" = ", e.Value);
            }
        }

        public override void Convert(CodeUnquotedExpr e) {
            cw.Write("$", e.Value);
        }

        public override void Convert(BreakExpr e) {
            cw.Write("break");
            if (e.LoopName != null) {
                cw.Write(" ", e.LoopName);
            }
        }

        public override void Convert(ContinueExpr e) {
            cw.Write("continue");
            if (e.LoopName != null) {
                cw.Write(" ", e.LoopName);
            }
        }

        public override void Convert(FuncTypeName e) {
            cw.Write(e.ArgsType, " -> ", e.ReturnType);
        }

        public override void Convert(UnionTypeName e) {
            cw.Write(e.Left, " | ", e.Right);
        }

        public override void Convert(DecoratedExpr e) {
            if (e.Decorators.Count == 1) {
                cw.Write("@", e.Decorators[0], e.Target);
            }
            else {
                cw.Write("@[");
                cw.AddJoin(", ", e.Decorators);
                cw.WriteLine("]");
                cw.Write(e.Target);
            }
        }

        public override void Convert(ScopeExpr e) {
            bool inAnonFn = e.Parent is FunctionDef fn && fn.Name == null;
            if (inAnonFn) {
                cw.WriteLine(" {");
            }

            cw.IndentLevel++;
            cw.MaybeWriteLine();
            cw.AddJoin("", e.Items, true);
            cw.MaybeWriteLine();
            cw.IndentLevel--;

            if (inAnonFn) {
                cw.Write("}");
            }
        }
    }
}
