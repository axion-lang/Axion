using System.Linq;
using Axion.Core.Processing;
using Axion.Core.Processing.Syntax.Tree.Expressions;
using Axion.Core.Processing.Syntax.Tree.Expressions.Binary;
using Axion.Core.Processing.Syntax.Tree.Expressions.TypeNames;
using Axion.Core.Processing.Syntax.Tree.Statements.Small;
using NUnit.Framework;

namespace Axion.Testing.NUnit.Parser {
    public partial class SyntaxParserTests {
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
            unit.Process(
                SourceProcessingMode.Parsing,
                SourceProcessingOptions.SyntaxAnalysisDebugOutput
            );
            TypeName[] stmts = unit.SyntaxTree.Root.Statements.Cast<ExpressionStatement>()
                                   .Select(s => ((VarDefinitionExpression) s.Expression).Type)
                                   .ToArray();
            Assert.That(stmts.Length == 10);
            Assert.DoesNotThrow(
                () => {
                    var type0 = (SimpleTypeName) stmts[0];
                    Assert.That(!(type0.Name is NameExpression));

                    var type1   = (UnionTypeName) stmts[1];
                    var union1L = (SimpleTypeName) type1.Left;
                    Assert.That(union1L.Name is NameExpression);
                    var union1R = (TupleTypeName) type1.Right;
                    Assert.That(union1R.Types.Length == 0);

                    var type2 = (GenericTypeName) stmts[2];
                    var genT2 = (SimpleTypeName) type2.Target;
                    Assert.That(genT2.Name is NameExpression);
                    var genG2 = (SimpleTypeName) type2.Generics[0];
                    Assert.That(genG2.Name is NameExpression);
                }
            );
        }
    }
}