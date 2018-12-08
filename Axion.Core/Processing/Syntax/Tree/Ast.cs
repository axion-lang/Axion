namespace Axion.Core.Processing.Syntax.Tree {
    public class Ast {
        internal readonly SourceCode Source;

//        private readonly Stack<ImportDefinition>   imports   = new Stack<ImportDefinition>();
//        private readonly Stack<ClassDefinition>    classes   = new Stack<ClassDefinition>();
//        private readonly Stack<FunctionDefinition> functions = new Stack<FunctionDefinition>();

        internal Ast(SourceCode sourceCode) {
            Source = sourceCode;
        }
    }
}