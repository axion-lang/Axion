using System;
using System.CodeDom;
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
                return ((Ast) p).Source;
            }
        }

        internal virtual CodeObject ToCSharp() {
            throw new InvalidOperationException();
        }

        internal virtual CodeObject[] ToCSharpArray() {
            throw new InvalidOperationException();
        }
    }
}