using System.Collections.Generic;
using Axion.Core.Processing.Syntax.Tree.Statements;
using Newtonsoft.Json;

namespace Axion.Core.Processing.Syntax.Tree {
    public class Ast : Statement {
        private readonly SourceCode Source;

        [JsonProperty]
        internal SuiteStatement Root { get; set; }

        //private readonly Stack<ImportDefinition>   imports = new Stack<ImportDefinition>();
        private readonly Stack<ClassDefinition>    classes   = new Stack<ClassDefinition>();
        private readonly Stack<FunctionDefinition> functions = new Stack<FunctionDefinition>();

        internal Ast(SourceCode sourceCode) {
            Source = sourceCode;
        }

        internal ClassDefinition CurrentClass {
            get {
                if (classes != null && classes.Count > 0) {
                    return classes.Peek();
                }
                return null;
            }
        }

        internal ClassDefinition PopClass() {
            if (classes != null && classes.Count > 0) {
                return classes.Pop();
            }
            return null;
        }

        internal void PushClass(ClassDefinition clazz) {
            classes.Push(clazz);
        }

        internal FunctionDefinition CurrentFunction {
            get {
                if (functions != null && functions.Count > 0) {
                    return functions.Peek();
                }
                return null;
            }
        }

        internal FunctionDefinition PopFunction() {
            if (functions != null && functions.Count > 0) {
                return functions.Pop();
            }
            return null;
        }

        internal void PushFunction(FunctionDefinition function) {
            functions.Push(function);
        }
    }
}