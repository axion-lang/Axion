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
    ///             (':' expr)
    ///             | ([':'] '{' expr* '}')
    ///             | ([':'] NEWLINE INDENT expr+ OUTDENT);
    ///     </c>
    /// </summary>
    public class ScopeExpr : Expr {
        private NodeList<Expr> items = null!;

        public NodeList<Expr> Items {
            get => items;
            set => items = Bind(value);
        }

        internal ScopeExpr(Node parent, IEnumerable<Expr>? items = null) : base(parent) {
            Items = NodeList<Expr>.From(this, items);

            if (Items.Count != 0) {
                MarkPosition(Items.First, Items.Last);
            }
        }

        internal ScopeExpr(Node? parent, params Expr[] items) : base(
            parent ?? GetParentFromChildren(items)
        ) {
            Items = NodeList<Expr>.From(this, items);

            if (Items.Count != 0) {
                MarkPosition(Items.First, Items.Last);
            }
        }

        internal static ScopeExpr FromItems(params Expr[] items) {
            return new ScopeExpr(null, items);
        }

        internal static ScopeExpr FromItems(IEnumerable<Expr> items) {
            Expr[] scopeItems = items.ToArray();
            Node   parent     = GetParentFromChildren(scopeItems);
            return new ScopeExpr(parent, scopeItems);
        }

        protected ScopeExpr() { }

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
                IDefinitionExpr e = GetParent<ScopeExpr>().GetDefByName(name);
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

        public ScopeExpr Parse(ScopeType type = ScopeType.Default) {
            if (!Stream.PeekIs(Spec.ScopeStartMarks)) {
                return this;
            }

            SetSpan(
                () => {
                    TokenType terminator = ParseStart(this);

                    if (terminator == Outdent && type.HasFlag(ScopeType.Lambda)) {
                        LangException.Report(BlameType.IndentationBasedScopeNotAllowed, this);
                    }

                    if (terminator == Newline) {
                        Items.Add(AnyExpr.Parse(this));
                    }
                    else {
                        while (!Stream.MaybeEat(terminator)
                            && !Stream.PeekIs(TokenType.End)
                            && !(terminator == Newline && Stream.Token.Is(Newline))) {
                            Items.Add(AnyExpr.Parse(this));
                        }
                    }
                }
            );
            return this;
        }

        /// <summary>
        ///     Starts parsing the statement's scope,
        ///     returns terminator what can be used to parse scope end.
        /// </summary>
        private static TokenType ParseStart(Expr parent) {
            // colon
            bool  hasColon   = parent.Stream.MaybeEat(Colon);
            Token scopeStart = parent.Stream.Token;

            // newline
            bool hasNewline = hasColon ? parent.Stream.MaybeEat(Newline) : scopeStart.Is(Newline);

            // '{'
            if (parent.Stream.MaybeEat(OpenBrace)) {
                if (hasColon) {
                    // ':' '{'
                    LangException.Report(BlameType.RedundantColonWithBraces, scopeStart);
                }

                return CloseBrace;
            }

            // indent
            if (parent.Stream.MaybeEat(Indent)) {
                return Outdent;
            }

            if (hasNewline) {
                // newline followed by not indent or '{'
                LangException.Report(BlameType.ExpectedScopeDeclaration, parent.Stream.Peek);
            }
            // exactly a 1-line scope
            else if (!hasColon) {
                // one line scope must have a colon
                LangException.ReportUnexpectedSyntax(Colon, parent.Stream.Peek);
            }

            return Newline;
        }
    }

    [Flags]
    public enum ScopeType {
        Default,
        Lambda
    }
}
