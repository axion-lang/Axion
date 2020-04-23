using System.Web;
using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Processing.Syntactic.Expressions;
using Axion.Core.Processing.Syntactic.Expressions.Definitions;
using Axion.Core.Processing.Syntactic.Expressions.Statements;
using Axion.Core.Processing.Syntactic.Expressions.TypeNames;

namespace Axion.Core.Processing.CodeGen {
    public class ConverterToPython : ConverterFromAxion {
        public override string OutputFileExtension => ".py";

        public ConverterToPython(CodeWriter cw) : base(cw) { }

        public override void Convert(CommentToken e) {
            cw.Write("#", e.Content);
        }

        public override void Convert(StringToken e) {
            if (e.HasPrefix("f")) {
                cw.Write("f");
            }

            if (e.HasPrefix("r")) {
                cw.Write("r");
            }

            cw.Write(e.IsMultiline ? "\"\"\"" : "\"");
            cw.Write(HttpUtility.JavaScriptStringEncode(e.Content));
            if (!e.IsUnclosed) {
                cw.Write(e.IsMultiline ? "\"\"\"" : "\"");
            }
        }

        public override void Convert(ClassDef e) {
            cw.Write("class ", e.Name);
            if (e.Bases.Count > 0) {
                cw.Write("(");
                cw.AddJoin(", ", e.Bases);
                cw.Write(")");
            }

            cw.Write(e.Scope);
        }

        public override void Convert(FunctionDef e) {
            cw.Write("def ", e.Name, "(");
            cw.AddJoin(", ", e.Parameters);
            cw.Write(")");
            if (e.ValueType != null) {
                cw.Write(" -> ", e.ValueType);
            }

            cw.Write(e.Scope);
        }

        public override void Convert(ModuleDef e) {
            cw.AddJoin("", e.Scope.Items, true);
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

        public override void Convert(EmptyExpr e) {
            cw.Write("pass");
        }

        public override void Convert(WhileExpr e) {
            cw.Write("while ", e.Condition, e.Scope);
        }

        public override void Convert(ArrayTypeName e) {
            cw.Write("List[", e.ElementType, "]");
        }

        public override void Convert(FuncTypeName e) {
            cw.Write(
                "Callable[[",
                e.ArgsType,
                "], ",
                e.ReturnType,
                "]"
            );
        }

        public override void Convert(DecoratedExpr e) {
            foreach (Expr decorator in e.Decorators) {
                cw.WriteLine("@", decorator);
            }

            cw.Write(e.Target);
        }

        public override void Convert(ScopeExpr e) {
            cw.Write(":");
            cw.IndentLevel++;
            cw.WriteLine();
            cw.AddJoin("", e.Items, true);
            cw.IndentLevel--;
            cw.MaybeWriteLine();
        }
    }
}
