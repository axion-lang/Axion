using System.Collections.Generic;
using System.Linq;
using Axion.Core.Processing.Errors;
using Axion.Core.Processing.Syntactic.Expressions.Atomic;
using Axion.Core.Processing.Syntactic.Expressions.Common;
using Axion.Core.Processing.Syntactic.Expressions.Definitions;
using Axion.SourceGenerators;
using Axion.Specification;
using static Axion.Specification.TokenType;

namespace Axion.Core.Processing.Syntactic.Expressions {
    /// <summary>
    ///     <code>
    ///         scope:
    ///             expr
    ///             | ('{' expr* '}')
    ///             | (NEWLINE INDENT expr+ OUTDENT);
    ///     </code>
    /// </summary>
    [SyntaxExpression]
    public partial class ScopeExpr : Node {
        [LeafSyntaxNode] NodeList<Node>? items;

        public ScopeExpr(Node parent) : base(parent) { }

        public ScopeExpr WithItems(IEnumerable<Node> list) {
            Items = new NodeList<Node>(this, list);
            return this;
        }

        public string CreateUniqueId(string formattedId) {
            var i = 0;
            string id;
            do {
                id = string.Format(formattedId, i);
                i++;
            } while (IsDefined(id));

            return id;
        }

        public bool IsDefined(NameExpr name) {
            return GetDefByName(name) != null;
        }

        public bool IsDefined(string name) {
            return GetDefByName(name) != null;
        }

        public IDefinitionExpr? GetDefByName(NameExpr name) {
            return GetDefByName(name.ToString());
        }

        public virtual IDefinitionExpr? GetDefByName(string name) {
            var e = GetParent<ScopeExpr>()?.GetDefByName(name);
            if (e != null) {
                return e;
            }

            var defs = GetScopedDefs();
            return defs.FirstOrDefault(def => def.Name?.ToString() == name);
        }

        public IDefinitionExpr[] GetScopedDefs() {
            var defs =
                Items.OfType<IDefinitionExpr>().ToList();

            // Add parameters of function if inside it.
            var parentFn = GetParent<FunctionDef>();
            if (parentFn != null) {
                defs.AddRange(parentFn.Parameters);
            }

            return defs.ToArray();
        }

        public List<(T item, ScopeExpr itemParentScope, int itemIndex)>
            FindItemsOfType<T>(
                List<(T item, ScopeExpr itemParentScope, int itemIndex)>? _outs
                    = null
            ) {
            _outs ??=
                new List<(T item, ScopeExpr itemParentScope, int itemIndex)>();
            for (var i = 0; i < Items.Count; i++) {
                var item = Items[i];
                if (item is T expr) {
                    _outs.Add((expr, this, i));
                }
                else {
                    var childProps = item.GetType()
                        .GetProperties()
                        .Where(
                            p => p.PropertyType
                              == typeof(ScopeExpr)
                              && p.Name != nameof(Parent)
                        );
                    foreach (var prop in childProps) {
                        var b = (ScopeExpr?) prop.GetValue(item);
                        b?.FindItemsOfType(_outs);
                    }
                }
            }

            return _outs;
        }

        public (ScopeExpr? itemParentScope, int itemIndex) IndexOf<T>(
            T expression
        ) where T : Node {
            for (var i = 0; i < Items.Count; i++) {
                var item = Items[i];
                if (item == expression) {
                    return (this, i);
                }

                var childProps =
                    item.GetType()
                        .GetProperties()
                        .Where(
                            p => p.PropertyType
                              == typeof(ScopeExpr)
                              && p.Name != nameof(Parent)
                        );
                foreach (var prop in childProps) {
                    var b = (ScopeExpr?) prop.GetValue(item);
                    var idx =
                        b?.IndexOf(expression) ?? (null, -1);
                    if (idx != (null, -1)) {
                        return idx;
                    }
                }
            }

            return (null, -1);
        }

        public ScopeExpr Parse() {
            if (!Stream.PeekIs(Spec.ScopeStartMarks)) {
                return this;
            }

            Start = Stream.Peek.Start;
            var hasNewline = Stream.MaybeEat(Newline);
            if (Stream.MaybeEat(OpenBrace)) {
                while (!Stream.MaybeEat(CloseBrace, TokenType.End)) {
                    Items += AnyExpr.Parse(this);
                }
            }
            else if (Stream.MaybeEat(Indent)) {
                while (!Stream.MaybeEat(Outdent, TokenType.End)) {
                    Items += AnyExpr.Parse(this);
                }
            }
            else if (hasNewline) {
                // newline followed by not indent or '{'
                LanguageReport.To(
                    BlameType.ExpectedScopeDeclaration,
                    Stream.Peek
                );
            }
            else {
                Items += AnyExpr.Parse(this);
            }
            return this;
        }
    }
}
