using Newtonsoft.Json;

namespace Axion.Core.Processing.Syntax.Tree {
    public class TreeNode : SpannedRegion {
        [JsonIgnore]
        protected internal TreeNode Parent;

        internal SourceUnit SourceUnit {
            get {
                TreeNode p = this;
                while (!(p is Ast)) {
                    p = p.Parent;
                }
                return ((Ast) p).Source;
            }
        }
    }
}