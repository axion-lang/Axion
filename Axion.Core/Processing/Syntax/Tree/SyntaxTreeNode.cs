using Newtonsoft.Json;

namespace Axion.Core.Processing.Syntax.Tree {
    public class SyntaxTreeNode : SpannedRegion {
        [JsonIgnore]
        protected internal SyntaxTreeNode Parent;

        internal SourceUnit SourceUnit {
            get {
                SyntaxTreeNode p = this;
                while (!(p is Ast)) {
                    p = p.Parent;
                }

                return ((Ast) p).Unit;
            }
        }
    }
}