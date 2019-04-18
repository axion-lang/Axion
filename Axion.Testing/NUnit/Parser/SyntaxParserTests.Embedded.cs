using System.Linq;
using Axion.Core.Processing;
using Axion.Core.Processing.Syntactic.Expressions;
using Axion.Core.Processing.Syntactic.Expressions.Binary;
using Axion.Core.Processing.Syntactic.Expressions.Multiple;
using Axion.Core.Processing.Syntactic.Expressions.TypeNames;
using Axion.Core.Processing.Syntactic.Statements.Small;
using NUnit.Framework;

namespace Axion.Testing.NUnit.Parser {
    public partial class SyntaxTreeNodeTests {
        [Test]
        public void TypeNameValid() {
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
                unit.Ast.Root.Statements.Cast<ExpressionStatement>()
                    .Select(s => ((VariableDefinitionExpression) s.Expression).Type)
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
        public void ConstantCollectionsValid() {
            SourceUnit unit = MakeSourceFromCode(
                @"
let _map = { 1 : ""one"", 2 : ""two"", 3 : ""three"" }
let _set = { ""one"", ""two"", ""three"" }
let _lst = [1, 2, 3, 4, 5]
let _tup = (1, 2, ""three"", true)
"
            );
            Parse(unit);
            Assert.That(unit.Blames.Count == 0, $"unit.Blames.Count == {unit.Blames.Count}");
            VariableDefinitionExpression[] stmts =
                unit.Ast.Root.Statements.Cast<ExpressionStatement>()
                    .Select(
                        s => (VariableDefinitionExpression) s.Expression
                    )
                    .ToArray();
            Assert.That(stmts.Length == 4);
            Assert.DoesNotThrow(
                () => {
                    // map
                    var map = (HashCollectionExpression) stmts[0].Right;
                    Assert.That(map.Type == HashCollectionType.Map);
                    Assert.That(map.Expressions.Cast<MapItemExpression>().Count() == 3);
                    // set
                    var set = (HashCollectionExpression) stmts[1].Right;
                    Assert.That(set.Type == HashCollectionType.Set);
                    var setValues = new[] { "one", "two", "three" };
                    Assert.That(set.Expressions.Count == setValues.Length);
                    for (var i = 0; i < set.Expressions.Count; i++) {
                        var constant = (ConstantExpression) set.Expressions[i];
                        Assert.That(constant.Value.Value == setValues[i]);
                    }
                    // list
                    var lst = (ListInitializerExpression) stmts[2].Right;
                    var lstValues = new[] { "1", "2", "3", "4", "5" };
                    Assert.That(lst.Expressions.Count == lstValues.Length);
                    for (var i = 0; i < lst.Expressions.Count; i++) {
                        var constant = (ConstantExpression) lst.Expressions[i];
                        Assert.That(constant.Value.Value == lstValues[i]);
                    }
                    //tuple
                    var tup = (TupleExpression) stmts[3].Right;
                    var tupValues = new[] { "1", "2", "three", "true" };
                    Assert.That(tup.Expressions.Count == tupValues.Length);
                    for (var i = 0; i < tup.Expressions.Count; i++) {
                        var constant = (ConstantExpression) tup.Expressions[i];
                        Assert.That(constant.Value.Value == tupValues[i]);
                    }
                }
            );
        }
    }
}