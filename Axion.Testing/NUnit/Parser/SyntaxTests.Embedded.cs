using System;
using System.Linq;
using Axion.Core.Processing.CodeGen;
using Axion.Core.Processing.Syntactic.Expressions;
using Axion.Core.Processing.Syntactic.Expressions.Atomic;
using Axion.Core.Processing.Syntactic.Expressions.Definitions;
using Axion.Core.Processing.Syntactic.Expressions.TypeNames;
using Axion.Core.Source;
using NUnit.Framework;

namespace Axion.Testing.NUnit.Parser {
    public partial class SyntaxParserTests {
        [Test]
        public void TestTypeNames() {
            SourceUnit src = MakeSourceFromCode(
                string.Join(
                    Environment.NewLine,
                    "type0: A.Qualified.Name",
                    "type1: JustAName | ()",
                    "type2: A[Generic]type3: A[Qualified.Generic]",
                    "type3: A.Name1[Qualified.Generic1, And.Generic.Number2]",
                    "type4: A[Generic]| List[Map[T1, T2]][]",
                    "type5: (A.Name1, A.Name2)",
                    "type6: (Name1, Name2)[]",
                    "type7: (Type1[Int][], (Array[] | AnotherType)[])",
                    "type8: List[Map[T1, T2]]| (Type1[Int, Type2[]][], (Array[] | AnotherType)[])"
                )
            );
            Parse(src);
            Assert.That(src.Blames.Count == 0, $"Blames.Count == {src.Blames.Count}");
            TypeName[] stmts =
                src.Ast.Items
                   .Select(s => ((VarDef) s).ValueType)
                   .ToArray();
            Assert.That(stmts.Length == 10);
            Assert.DoesNotThrow(
                () => {
                    var type1   = (UnionTypeName) stmts[1];
                    var union1R = (TupleTypeName) type1.Right;
                    Assert.That(union1R.Types.Count == 0);
                }
            );
        }

        [Test]
        public void TestConstantCollections() {
            SourceUnit src = MakeSourceFromCode(
                string.Join(
                    Environment.NewLine,
                    "let _map = map (1 to \"one\", 2 to \"two\", 3 to \"three\" )",
                    "let _set = set ( \"one\", \"two\", \"three\" )",
                    "let _lst = [1, 2, 3, 4, 5]",
                    "let _tup = (1, 2, \"three\", true)"
                )
            );
            Parse(src);
            Assert.That(src.Blames.Count == 0, $"Blames.Count == {src.Blames.Count}");
            VarDef[] stmts =
                src.Ast.Items.Cast<VarDef>()
                   .ToArray();
            Assert.That(stmts.Length == 4);
            Assert.DoesNotThrow(
                () => {
                    // // map
                    // var map = (BraceCollectionExpression) stmts[0].Right;
                    // Assert.That(map.Type == BraceCollectionType.Map);
                    // Assert.That(map.Expressions.Cast<MapItemExpression>().Count() == 3);
                    // // set
                    // var set = (BraceCollectionExpression) stmts[1].Right;
                    // Assert.That(set.Type == BraceCollectionType.Set);
                    // var setValues = new[] {
                    //     "one", "two", "three"
                    // };
                    // Assert.That(set.Expressions.Count == setValues.Length);
                    // for (var i = 0; i < set.Expressions.Count; i++) {
                    //     var constant = (ConstantExpression) set.Expressions[i];
                    //     Assert.That(constant.Value.Value == setValues[i]);
                    // }

                    // list
                    //                    var lst = (ListInitializerExpression) stmts[2].Right;
                    //                    var lstValues = new[] {
                    //                        "1", "2", "3", "4", "5"
                    //                    };
                    //                    Assert.That(lst.Expressions.Count == lstValues.Length);
                    //                    for (var i = 0; i < lst.Expressions.Count; i++) {
                    //                        var constant = (ConstantExpression) lst.Expressions[i];
                    //                        Assert.That(constant.Value.Value == lstValues[i]);
                    //                    }

                    //tuple
                    var tup = (TupleExpr) stmts[3].Value;
                    var tupValues = new[] {
                        "1", "2", "three", "true"
                    };
                    Assert.That(tup.Expressions.Count == tupValues.Length);
                    for (var i = 0; i < tup.Expressions.Count; i++) {
                        var constant = (ConstantExpr) tup.Expressions[i];
                        Assert.That(constant.Literal.Content == tupValues[i]);
                    }
                }
            );
        }

        [Test]
        public void TestPipelineOperator() {
            SourceUnit src1 = MakeSourceFromCode("person |> parseData |> getAge |> validateAge");
            Parse(src1);
            Assert.That(src1.Blames.Count == 0, $"unit1.Blames.Count == {src1.Blames.Count}");

            SourceUnit src2 = MakeSourceFromCode("validateAge(getAge(parseData(person)))");
            Parse(src2);
            Assert.That(src2.Blames.Count == 0, $"unit2.Blames.Count == {src2.Blames.Count}");
            var a = new CodeWriter(ProcessingMode.ConvertAxion);
            a.Write(src2.Ast);
            var b = new CodeWriter(ProcessingMode.ConvertAxion);
            b.Write(src1.Ast);
            //Assert.AreEqual(a.ToString(), b.ToString());
        }
    }
}