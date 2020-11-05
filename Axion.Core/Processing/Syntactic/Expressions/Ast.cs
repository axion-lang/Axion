using System.Collections.Generic;
using System.Linq;
using Axion.Core.Hierarchy;
using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Processing.Syntactic.Expressions.Common;
using Axion.Core.Processing.Syntactic.Expressions.Definitions;
using Axion.Core.Processing.Traversal;

namespace Axion.Core.Processing.Syntactic.Expressions {
    /// <summary>
    ///     Abstract Syntax Tree built from source code.
    /// </summary>
    public class Ast : ScopeExpr {
        internal List<MacroDef> Macros =>
            Unit.Module.Definitions.Values.OfType<MacroDef>().ToList();

        internal readonly Stack<MacroApplicationExpr> MacroApplicationParts =
            new Stack<MacroApplicationExpr>();

        internal Ast(Unit src) : base(null!) {
            Unit   = src;
            Parent = this;
            Path = new NodeTreePath(
                this,
                typeof(Unit).GetProperty(nameof(Hierarchy.Unit.Ast))!
            );
        }

        internal new void Parse() {
            SetSpan(
                () => {
                    while (!Stream.MaybeEat(TokenType.End)) {
                        Expr item = AnyExpr.Parse(this);
                        Items += item;
                        if (item is IDefinitionExpr def) {
                            Unit.Module.AddDefinition(def);
                        }
                    }
                }
            );
        }
        
        public override IDefinitionExpr? GetDefByName(string name) { 
            IDefinitionExpr[] defs = GetScopedDefs();
            return defs.FirstOrDefault(def => def.Name?.ToString() == name);
        }
    }
}
