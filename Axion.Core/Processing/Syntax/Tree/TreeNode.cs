using Newtonsoft.Json;

namespace Axion.Core.Processing.Syntax.Tree {
    public class TreeNode : SpannedRegion {
        [JsonIgnore] protected internal TreeNode Parent;
    }
}