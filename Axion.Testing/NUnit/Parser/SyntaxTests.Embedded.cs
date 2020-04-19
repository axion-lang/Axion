using System;
using System.Linq;
using Axion.Core;
using Axion.Core.Processing.Syntactic.Expressions.Definitions;
using Axion.Core.Processing.Syntactic.Expressions.TypeNames;
using Axion.Core.Source;
using NUnit.Framework;

namespace Axion.Testing.NUnit.Parser {
    public partial class SyntaxParserTests {
        [Test]
        public void TestPipelineOperator() {
            Unit src1 = MakeSourceFromCode("person |> parseData |> getAge |> validateAge");
            Parse(src1);
            Assert.That(src1.Blames.Count == 0, $"unit1.Blames.Count == {src1.Blames.Count}");

            Unit src2 = MakeSourceFromCode("validateAge(getAge(parseData(person)))");
            Parse(src2);
            Assert.That(src2.Blames.Count == 0, $"unit2.Blames.Count == {src2.Blames.Count}");

            Compiler.Process(src1, ProcessingMode.Transpilation, ProcessingOptions.Debug);
            Compiler.Process(src2, ProcessingMode.Transpilation, ProcessingOptions.Debug);

            Assert.AreEqual(src1.CodeWriter.ToString(), src2.CodeWriter.ToString());
        }

        [Test]
        public void TestTypeNames() {
            Unit src = MakeSourceFromCode(
                string.Join(
                    Environment.NewLine,
                    "type0: A.Qualified.Name",
                    "type1: JustAName | ()",
                    "type2: A[Generic]",
                    "type3: A[Qualified.Generic]",
                    "type4: A.Name1[Qualified.Generic1, And.Generic.Number2]",
                    "type5: A[Generic]| List[Map[T1, T2]][]",
                    "type6: (A.Name1, A.Name2)",
                    "type7: (Name1, Name2)[]",
                    "type8: (Type1[Int][], (Array[] | AnotherType)[])",
                    "type9: List[Map[T1, T2]]| (Type1[Int, Type2[]][], (Array[] | AnotherType)[])"
                )
            );
            Parse(src);
            Assert.That(src.Blames.Count == 0, $"Blames.Count == {src.Blames.Count}");
            TypeName[] stmts = src.Ast.Items.Select(s => ((VarDef) s).ValueType).ToArray();
            Assert.That(stmts.Length == 10);
        }
    }
}
