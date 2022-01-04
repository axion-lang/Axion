using Axion.Core.Processing.Syntactic.Expressions.Operations;
using Axion.Core.Processing.Syntactic.Expressions.Postfix;
using static Axion.Specification.TokenType;

namespace Axion.Core.Processing.Traversal.Reducers;

public class FunctionPipeReducer : Reducer<BinaryExpr> {
    protected override void Reduce(BinaryExpr pipe) {
        if (pipe is not {
                Operator.Type: PipeRightAngle,
                Right: { },
                Left: { }
            }) {
            return;
        }

        pipe.Path.Node = new FuncCallExpr(pipe.Parent) {
            Target = pipe.Right,
            Args = {
                new FuncCallArg {
                    Value = pipe.Left
                }
            }
        };
    }
}
