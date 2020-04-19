using Axion.Core.Processing.Syntactic.Expressions;

namespace Axion.Core.Processing.CodeGen {
    public class AxionToPascalConverter : ConverterFromAxion {
        public override string OutputFileExtension => ".pas";

        public AxionToPascalConverter(CodeWriter cw) : base(cw) { }

        public override void Convert(Ast e) {
            cw.WriteLine("program PascalFromAxion;");
            cw.WriteLine("var x, y: integer;");
            cw.WriteLine("begin");
            cw.IndentLevel++;
            cw.AddJoin("", e.Items, true);
            cw.IndentLevel--;
            cw.WriteLine("end.");
        }

        public override void Convert(IfExpr e) {
            cw.Write(
                "if ",
                e.Condition,
                " then",
                e.ThenScope
            );
            if (e.ElseScope != null) {
                cw.Write("else", e.ElseScope);
            }
        }
    }
}
