using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Processing.Syntactic.Expressions;
using Axion.Core.Processing.Syntactic.Expressions.Atomic;
using Axion.Core.Processing.Syntactic.Expressions.Definitions;
using Axion.Core.Processing.Syntactic.Expressions.Operations;
using Axion.Core.Processing.Syntactic.Expressions.Postfix;
using static Axion.Specification.TokenType;

namespace Axion.Core.Processing.Traversal.Reducers;

public class TupleUnwrappingReducer : Reducer<BinaryExpr> {
    protected override void Reduce(BinaryExpr node) {
        if (node.Operator is not { Type: EqualsSign }
         || node.Left is not TupleExpr tpl) {
            return;
        }

        // (x, y) = GetCoordinates()
        // <=======================>
        // wrapped = GetCoordinates()
        // x = wrapped
        // y = wrapped
        var scope = node.GetParent<ScopeExpr>();
        var (_, deconstructionIdx) = scope!.IndexOf(node);
        var deconstructionVar = new VarDef(
            scope,
            new Token(node.Unit, KeywordLet)
        ) {
            Name = new NameExpr(
                node,
                scope.CreateUniqueId("wrapped{0}")
            ),
            Value = node.Right
        };

        scope.Items[deconstructionIdx] = deconstructionVar;
        for (var i = 0; i < tpl.Expressions.Count; i++) {
            scope.Items.Insert(
                deconstructionIdx + i + 1,
                new VarDef(scope) {
                    Name = (NameExpr) tpl.Expressions[i],
                    Value = new MemberAccessExpr(scope) {
                        Target = deconstructionVar.Name,
                        Member = tpl.Expressions[i]
                    }
                }
            );
        }

        node.Path.Node = node;
    }
}
