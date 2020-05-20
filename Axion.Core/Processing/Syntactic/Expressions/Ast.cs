using System.Collections.Generic;
using System.Linq;
using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Processing.Syntactic.Expressions.Common;
using Axion.Core.Processing.Syntactic.Expressions.Definitions;
using Axion.Core.Processing.Traversal;
using Axion.Core.Source;

namespace Axion.Core.Processing.Syntactic.Expressions {
    /// <summary>
    ///     Abstract Syntax Tree built from source code.
    /// </summary>
    public class Ast : ScopeExpr {
        internal List<MacroDef> Macros =>
            Source.Module.GetDefinitions().Values.OfType<MacroDef>().ToList();

        internal readonly Stack<MacroApplicationExpr> MacroApplicationParts =
            new Stack<MacroApplicationExpr>();

        internal Ast(Unit src) {
            Source = src;
            Parent = this;
            Path   = new NodeTreePath(this, typeof(Unit).GetProperty(nameof(Unit.Ast)));
        }

        internal void Parse() {
            SetSpan(
                () => {
                    while (!Stream.MaybeEat(TokenType.End)) {
                        Expr item = AnyExpr.Parse(this);
                        Items.Add(item);
                        if (item is IDefinitionExpr def) {
                            Source.Module.AddDefinition(def);
                        }
                    }
                }
            );
        }
    }
}
