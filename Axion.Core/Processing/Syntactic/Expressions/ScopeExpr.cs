using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Axion.Core.Processing.Errors;
using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Processing.Syntactic.Expressions.Atomic;
using Axion.Core.Processing.Syntactic.Expressions.Common;
using Axion.Core.Processing.Syntactic.Expressions.Definitions;
using Axion.Core.Specification;
using static Axion.Core.Processing.Lexical.Tokens.TokenType;

namespace Axion.Core.Processing.Syntactic.Expressions {
    /// <summary>
    ///     <c>
    ///         scope:
    ///             expr
    ///             | ('{' expr* '}')
    ///             | (NEWLINE INDENT expr+ OUTDENT);
    ///     </c>
    /// </summary>
    public class ScopeExpr : Expr {
        private NodeList<Expr> items = null!;

        public NodeList<Expr> Items {
            get => InitIfNull(ref items);
            set => items = Bind(value);
        }

        internal ScopeExpr(Node parent) : base(parent) { }

        protected ScopeExpr() { }

        public ScopeExpr WithItems(IEnumerable<Expr> list) {
            Items = new NodeList<Expr>(this, list);
            return this;
        }

        public string CreateUniqueId(string formattedId) {
            var    i  = 0;
            string id = string.Format(formattedId, i);
            while (IsDefined(id)) {
                i++;
                id = string.Format(formattedId, i);
            }

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

        public IDefinitionExpr? GetDefByName(string name) {
            if (!(this is Ast)) {
                IDefinitionExpr? e = GetParent<ScopeExpr>()?.GetDefByName(name);
                if (e != null) {
                    return e;
                }
            }

            IDefinitionExpr[] defs = GetScopedDefs();
            return defs.FirstOrDefault(def => def.Name?.ToString() == name);
        }

        public IDefinitionExpr[] GetScopedDefs() {
            List<IDefinitionExpr> defs = Items.OfType<IDefinitionExpr>().ToList();

            // Add parameters of function if inside it.
            var parentFn = GetParent<FunctionDef>();
            if (parentFn != null) {
                defs.AddRange(parentFn.Parameters);
            }

            return defs.ToArray();
        }

        public List<(T item, ScopeExpr itemParentScope, int itemIndex)> FindItemsOfType<T>(
            List<(T item, ScopeExpr itemParentScope, int itemIndex)>? _outs = null
        ) {
            _outs ??= new List<(T item, ScopeExpr itemParentScope, int itemIndex)>();
            for (var i = 0; i < Items.Count; i++) {
                Expr item = Items[i];
                if (item is T expr) {
                    _outs.Add((expr, this, i));
                }
                else {
                    IEnumerable<PropertyInfo> childProps = item
                                                           .GetType()
                                                           .GetProperties()
                                                           .Where(
                                                               p => p.PropertyType
                                                                 == typeof(ScopeExpr)
                                                                 && p.Name != nameof(Parent)
                                                           );
                    foreach (PropertyInfo prop in childProps) {
                        var b = (ScopeExpr) prop.GetValue(item);
                        b?.FindItemsOfType(_outs);
                    }
                }
            }

            return _outs;
        }

        public (ScopeExpr? itemParentScope, int itemIndex) IndexOf<T>(T expression)
            where T : Expr {
            for (var i = 0; i < Items.Count; i++) {
                Expr item = Items[i];
                if (item == expression) {
                    return (this, i);
                }

                IEnumerable<PropertyInfo> childProps = item
                                                       .GetType()
                                                       .GetProperties()
                                                       .Where(
                                                           p => p.PropertyType == typeof(ScopeExpr)
                                                             && p.Name         != nameof(Parent)
                                                       );
                foreach (PropertyInfo prop in childProps) {
                    var                                         b = (ScopeExpr) prop.GetValue(item);
                    (ScopeExpr itemParentScope, int itemIndex)? idx = b?.IndexOf(expression);
                    if (idx != null && idx != (null, -1)) {
                        return ((ScopeExpr itemParentScope, int itemIndex)) idx;
                    }
                }
            }

            return (null, -1);
        }

        public ScopeExpr Parse(bool allowIndent = true) {
            if (!Stream.PeekIs(Spec.ScopeStartMarks)) {
                return this;
            }

            SetSpan(
                () => {
                    ScopeType scopeType = ParseStart(this);

                    if (scopeType == ScopeType.Single) {
                        Items.Add(AnyExpr.Parse(this));
                    }
                    else if (scopeType == ScopeType.Indented) {
                        if (allowIndent) {
                            while (!Stream.MaybeEat(Outdent, TokenType.End)) {
                                Items.Add(AnyExpr.Parse(this));
                            }
                        }
                        else {
                            LangException.Report(BlameType.IndentationBasedScopeNotAllowed, this);
                        }
                    }
                    else if (scopeType == ScopeType.Embraced) {
                        while (!Stream.MaybeEat(CloseBrace, TokenType.End)) {
                            Items.Add(AnyExpr.Parse(this));
                        }
                    }
                    else {
                        throw new NotSupportedException("Invalid scope type.");
                    }
                }
            );
            return this;
        }

        /// <summary>
        ///     Starts parsing the statement's scope,
        ///     returns terminator what can be used to parse scope end.
        /// </summary>
        private static ScopeType ParseStart(Node parent) {
            var s = parent.Source.TokenStream;
            // newline
            bool hasNewline = s.MaybeEat(Newline);

            // '{'
            if (s.MaybeEat(OpenBrace)) {
                return ScopeType.Embraced;
            }

            // indent
            if (s.MaybeEat(Indent)) {
                return ScopeType.Indented;
            }

            if (hasNewline) {
                // newline followed by not indent or '{'
                LangException.Report(BlameType.ExpectedScopeDeclaration, s.Peek);
            }
            // exactly a 1-line scope
            return ScopeType.Single;
        }
    }

    [Flags]
    public enum ScopeType {
        Indented,
        Embraced,
        Single
    }
}
