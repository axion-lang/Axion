using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Processing.Syntactic.Expressions.Operations;
using static Axion.Specification.TokenType;

namespace Axion.Core.Processing.Traversal.Reducers;

public class IsNotReducer : Reducer<BinaryExpr> {
    protected override void Reduce(BinaryExpr bin) {
        if (bin is not {
                Operator.Type: Is,
                Right: UnaryExpr { Operator.Type: Not } un
            }) {
            return;
        }

        bin.Path.Node = new UnaryExpr(bin.Parent) {
            Operator = new OperatorToken(
                bin.Unit,
                tokenType: Not
            ),
            Value = new BinaryExpr(bin) {
                Left = bin.Left,
                Operator = new OperatorToken(
                    bin.Unit,
                    tokenType: Is
                ),
                Right = un.Value
            }
        };
    }
}
