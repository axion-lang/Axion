using System.Linq;
using Axion.Core.Processing;
using Axion.Core.Processing.Syntactic.Expressions.Binary;
using Axion.Core.Processing.Syntactic.Expressions.TypeNames;
using Axion.Core.Processing.Syntactic.Statements.Small;
using NUnit.Framework;

namespace Axion.Testing.NUnit.Parser {
    public partial class SyntaxTreeNodeTests {
        [Test]
        public void TypeNameValid() {
            SourceUnit unit = MakeSourceFromCode(
                nameof(TypeNameValid),
                @"
type0: A.Qualified.Name
type1: JustAName | ()
type2: A<Generic>
type3: A<Qualified.Generic>
type4: A.Name1<Qualified.Generic1, And.Generic.Number2>
type5: A<Generic> | List< Map<T1, T2> >[]
type6: (A.Name1, A.Name2)
type7: (Name1, Name2)[]
type8: (Type1<Int>[], (Array[] | AnotherType)[])
type9: List< Map<T1, T2> > | (Type1<Int, Type2[]>[], (Array[] | AnotherType)[])
"
            );
            Parse(unit);
            Assert.That(unit.Blames.Count == 0, $"unit.Blames.Count == {unit.Blames.Count}");
            TypeName[] stmts = unit.Ast.Root.Statements.Cast<ExpressionStatement>()
                                   .Select(s => ((VariableDefinitionExpression) s.Expression).Type)
                                   .ToArray();
            Assert.That(stmts.Length == 10);
            Assert.DoesNotThrow(
                () => {
                    //var type0 = (SimpleTypeName) stmts[0];

                    var type1   = (UnionTypeName) stmts[1];
                    //var union1L = (SimpleTypeName) type1.Left;
                    var union1R = (TupleTypeName) type1.Right;
                    Assert.That(union1R.Types.Count == 0);

                    //var type2 = (GenericTypeName) stmts[2];
                    //var genT2 = (SimpleTypeName) type2.Target;
                    //var genG2 = (SimpleTypeName) type2.TypeArguments[0];
                }
            );
        }
    }
}