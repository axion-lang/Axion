using System.Linq;
using Axion.Core.Processing.CodeGen;
using Axion.Core.Processing.Source;
using Axion.Core.Processing.Syntactic;
using Axion.Core.Processing.Syntactic.Atomic;
using Axion.Core.Processing.Syntactic.Binary;
using Axion.Core.Processing.Syntactic.TypeNames;
using NUnit.Framework;

namespace Axion.Testing.NUnit.Parser {
    public partial class SyntaxParserTests {
        [Test]
        public void IsOK_TypeName() {
            SourceUnit unit = MakeSourceFromCode(
                @"
type0: A.Qualified.Name
type1: JustAName | ()
type2: A[Generic]type3: A[Qualified.Generic]
type4: A.Name1[Qualified.Generic1, And.Generic.Number2]
type5: A[Generic]| List[Map[T1, T2]][]
type6: (A.Name1, A.Name2)
type7: (Name1, Name2)[]
type8: (Type1[Int][], (Array[] | AnotherType)[])
type9: List[Map[T1, T2]]| (Type1[Int, Type2[]][], (Array[] | AnotherType)[])
"
            );
            Parse(unit);
            Assert.That(unit.Blames.Count == 0, $"unit.Blames.Count == {unit.Blames.Count}");
            TypeName[] stmts =
                unit.Ast.Items
                    .Select(s => ((VariableDefinitionExpression) s).ValueType)
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
        public void IsOK_ConstantCollections() {
            SourceUnit unit = MakeSourceFromCode(
                @"
let _map = map (1 to ""one"", 2 to ""two"", 3 to ""three"" )
let _set = set ( ""one"", ""two"", ""three"" )
let _lst = [1, 2, 3, 4, 5]
let _tup = (1, 2, ""three"", true)
"
            );
            Parse(unit);
            Assert.That(unit.Blames.Count == 0, $"unit.Blames.Count == {unit.Blames.Count}");
            VariableDefinitionExpression[] stmts =
                unit.Ast.Items.Cast<VariableDefinitionExpression>()
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
                    var tup = (TupleExpression) stmts[3].Right;
                    var tupValues = new[] {
                        "1", "2", "three", "true"
                    };
                    Assert.That(tup.Expressions.Count == tupValues.Length);
                    for (var i = 0; i < tup.Expressions.Count; i++) {
                        var constant = (ConstantExpression) tup.Expressions[i];
                        Assert.That(constant.Value.Value == tupValues[i]);
                    }
                }
            );
        }

        [Test]
        public void IsOK_PipelineOperator() {
            SourceUnit unit1 = MakeSourceFromCode("person |> parseData |> getAge |> validateAge");
            Parse(unit1);
            Assert.That(unit1.Blames.Count == 0, $"unit1.Blames.Count == {unit1.Blames.Count}");

            SourceUnit unit2 = MakeSourceFromCode("validateAge(getAge(parseData(person)))");
            Parse(unit2);
            Assert.That(unit2.Blames.Count == 0, $"unit2.Blames.Count == {unit2.Blames.Count}");
            var a = new CodeBuilder(OutLang.Axion);
            a.Write(unit2.Ast);
            var b = new CodeBuilder(OutLang.Axion);
            b.Write(unit1.Ast);
            Assert.AreEqual(a.ToString(), b.ToString());
        }

        [Test]
        public void IsOK_ModuleDef() {
            SourceUnit unit1 = MakeSourceFromCode("module ExampleModule: pass");
            Parse(unit1);
            Assert.That(unit1.Blames.Count == 0, $"unit1.Blames.Count == {unit1.Blames.Count}");
        }
    }
}