using Axion.Core.Processing.Syntactic.Expressions.Definitions;

namespace Axion.Core.Processing.Traversal.Reducers;

public class DataMembersReducer : Reducer<ClassDef> {
    protected override void Reduce(ClassDef cls) {
        // class Point (x: int, y: int)
        //     fn print
        //         print(x, y)
        // <============================>
        // class Point
        //     x: int
        //     y: int
        //     fn print
        //         print(x, y)
        foreach (var dataMember in cls.DataMembers) {
            cls.Scope.Items.Insert(0, dataMember);
        }

        cls.DataMembers.Clear();

        cls.Path.Node = cls;
    }
}
