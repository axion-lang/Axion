using System.CodeDom;
using Axion.Core.Processing.Syntax.Tree.Statements;
using Axion.Core.Processing.Syntax.Tree.Statements.Definitions;
using Newtonsoft.Json;

namespace Axion.Core.Processing.Syntax.Tree {
    public class Ast : SyntaxTreeNode {
        internal readonly SourceUnit Source;
        //private readonly Stack<ImportDefinition> imports = new Stack<ImportDefinition>();

        private BlockStatement root;

        [JsonProperty]
        public BlockStatement Root {
            get => root;
            set {
                if (value != null) {
                    value.Parent = this;
                }
                root = value;
            }
        }

        internal Ast(SourceUnit sourceCode) {
            Source = sourceCode;
        }

        internal override CodeObject ToCSharp() {
            var unit    = new CodeCompileUnit();
            var imports = new CodeNamespaceImportCollection();
            foreach (Statement s in root.Statements) {
//                if (s is ImportStatement) {
//                    imports.Add((CodeNamespaceImport) s.ToCSharp());
//                    continue;
//                }
                if (s is ModuleDefinition) {
                    unit.Namespaces.Add((CodeNamespace) s.ToCSharp());
                }
            }
            return unit;
        }
    }
}