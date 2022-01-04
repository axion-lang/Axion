using Axion.Core.Processing.Syntactic.Expressions.TypeNames;

namespace Axion.Core.Processing.Traversal.Reducers;

public class UnionTypeReducer : Reducer<UnionTypeName> {
    protected override void Reduce(UnionTypeName union) {
        if (union is not { Left: { }, Right: { } }) {
            return;
        }

        union.Path.Node = new GenericTypeName(union.Parent) {
            Target = new SimpleTypeName(union, "Union"),
            TypeArgs = {
                union.Left,
                union.Right
            }
        };
    }
}
