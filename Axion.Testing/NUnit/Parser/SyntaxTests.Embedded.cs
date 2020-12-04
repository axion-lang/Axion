using System;
using System.Linq;
using Axion.Core;
using Axion.Core.Processing.Syntactic.Expressions.Definitions;
using Axion.Core.Processing.Syntactic.Expressions.TypeNames;
using Axion.Core.Processing.Translation;
using NUnit.Framework;

namespace Axion.Testing.NUnit.Parser {
    public partial class SyntaxParserTests {
        [Test]
        public void TestPipelineOperator() {
            var (mainUnit1, _) = TestUtils.ModuleFromCode(
                "person |> parseData |> getAge |> validateAge"
            );
            var cw1 = (CodeWriter?) Compiler.Process(
                mainUnit1,
                new ProcessingOptions("Axion") {
                    Debug = true
                }
            );

            var (mainUnit2, _) = TestUtils.ModuleFromCode(
                "validateAge(getAge(parseData(person)))"
            );
            var cw2 = (CodeWriter?) Compiler.Process(
                mainUnit2,
                new ProcessingOptions("Axion") {
                    Debug = true
                }
            );

            Assert.AreEqual(cw1?.ToString(), cw2?.ToString());
        }

        [Test]
        public void TestTypeNames() {
            var (mainUnit, module) = TestUtils.ModuleFromCode(
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
            Compiler.Process(module, new ProcessingOptions(Mode.Parsing));
            Assert.AreEqual(0, module.Blames.Count);
            TypeName?[] stmts = mainUnit.Ast.Items
                                        .Select(s => ((VarDef) s).ValueType)
                                        .ToArray();
            Assert.AreEqual(10, stmts.Length);
        }
    }
}
