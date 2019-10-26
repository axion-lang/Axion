using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Axion.Core.Processing.CodeGen;
using Axion.Core.Processing.Errors;
using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Processing.Syntactic.Expressions.Definitions;
using Axion.Core.Specification;
using static Axion.Core.Processing.Lexical.Tokens.TokenType;

namespace Axion.Core.Processing.Syntactic.Expressions {
    /// <summary>
    ///     <c>
    ///         block:
    ///             (':' expr)
    ///             | ([':'] '{' expr* '}')
    ///             | ([':'] NEWLINE INDENT expr+ OUTDENT);
    ///     </c>
    /// </summary>
    public class BlockExpr : Expr {
        private NodeList<Expr> items;

        public NodeList<Expr> Items {
            get => items;
            protected set => SetNode(ref items, value);
        }

        internal BlockExpr(
            Expr          parent = null,
            params Expr[] items
        ) : base(parent) {
            Items = new NodeList<Expr>(this, items);

            if (Items.Count != 0) {
                MarkPosition(Items.First, Items.Last);
            }
        }

        protected BlockExpr() { }

        public string CreateUniqueId(string formattedId) {
            var    i  = 0;
            string id = string.Format(formattedId, i);
            while (IsDefined(id)) {
                i++;
                id = string.Format(formattedId, i);
            }

            return id;
        }

        public bool IsDefined(string name) {
            return GetDefByName(name) != null;
        }

        public IDefinitionExpr GetDefByName(string name) {
            if (!(this is Ast)) {
                IDefinitionExpr e = GetParentOfType<BlockExpr>().GetDefByName(name);
                if (e != null) {
                    return e;
                }
            }

            IDefinitionExpr[] defs = GetAllDefs();
            return defs.FirstOrDefault(def => def.Name.ToString() == name);
        }

        public IDefinitionExpr[] GetAllDefs() {
            List<IDefinitionExpr> defs = Items.OfType<IDefinitionExpr>().ToList();

            // Add parameters of function if inside it.
            var parentFn = GetParentOfType<FunctionDef>();
            if (parentFn != null) {
                defs.AddRange(parentFn.Parameters);
            }

            return defs.ToArray();
        }

        public List<(T item, BlockExpr itemParentBlock, int itemIndex)> FindItemsOfType<T>(
            List<(T item, BlockExpr itemParentBlock, int itemIndex)> _outs = null
        ) where T : Expr {
            _outs = _outs ?? new List<(T item, BlockExpr itemParentBlock, int itemIndex)>();
            for (var i = 0; i < Items.Count; i++) {
                Expr item = Items[i];
                if (item is T expr) {
                    _outs.Add((expr, this, i));
                }
                else {
                    IEnumerable<PropertyInfo> childBlockProps = item.GetType().GetProperties().Where(
                        p => p.PropertyType == typeof(BlockExpr)
                          && p.Name         != nameof(Parent)
                    );
                    foreach (PropertyInfo blockProp in childBlockProps) {
                        var b = (BlockExpr) blockProp.GetValue(item);
                        b?.FindItemsOfType(_outs);
                    }
                }
            }

            return _outs;
        }

        public (BlockExpr itemParentBlock, int itemIndex) IndexOf<T>(T expression) where T : Expr {
            for (var i = 0; i < Items.Count; i++) {
                Expr item = Items[i];
                if (item == expression) {
                    return (this, i);
                }

                IEnumerable<PropertyInfo> childBlockProps = item.GetType().GetProperties().Where(
                    p => p.PropertyType == typeof(BlockExpr)
                      && p.Name         != nameof(Parent)
                );
                foreach (PropertyInfo blockProp in childBlockProps) {
                    var                                         b   = (BlockExpr) blockProp.GetValue(item);
                    (BlockExpr itemParentBlock, int itemIndex)? idx = b?.IndexOf(expression);
                    if (idx != null && idx != (null, -1)) {
                        return ((BlockExpr itemParentBlock, int itemIndex)) idx;
                    }
                }
            }

            return (null, -1);
        }

        public BlockExpr Parse(BlockType blockType = BlockType.Default) {
            if (!Stream.PeekIs(Spec.BlockStartMarks)) {
                return this;
            }

            SetSpan(() => {
                TokenType terminator = ParseStart(this);

                if (terminator == Outdent && blockType.HasFlag(BlockType.Lambda)) {
                    LangException.Report(BlameType.IndentationBasedBlockNotAllowed, this);
                }

                if (terminator == Newline) {
                    Items.Add(Parsing.ParseAny(this));
                }
                else {
                    while (!Stream.MaybeEat(terminator)
                        && !Stream.PeekIs(TokenType.End)
                        && !(terminator == Newline && Stream.Token.Is(Newline))) {
                        Items.Add(Parsing.ParseAny(this));
                    }
                }
            });
            return this;
        }

        /// <summary>
        ///     Starts parsing the statement's block,
        ///     returns terminator what can be used to parse block end.
        /// </summary>
        internal static TokenType ParseStart(Expr parent) {
            // colon
            bool  hasColon   = parent.Stream.MaybeEat(Colon);
            Token blockStart = parent.Stream.Token;

            // newline
            bool hasNewline = hasColon
                ? parent.Stream.MaybeEat(Newline)
                : blockStart.Is(Newline);

            // '{'
            if (parent.Stream.MaybeEat(OpenBrace)) {
                if (hasColon) { // ':' '{'
                    LangException.Report(BlameType.RedundantColonWithBraces, blockStart);
                }

                return CloseBrace;
            }

            // indent
            if (parent.Stream.MaybeEat(Indent)) {
                return Outdent;
            }

            if (hasNewline) {
                // newline followed by not indent or '{'
                LangException.Report(BlameType.ExpectedBlockDeclaration, parent.Stream.Peek);
            }
            // exactly a 1-line block
            else if (!hasColon) {
                // one line block must have a colon
                LangException.ReportUnexpectedSyntax(Colon, parent.Stream.Peek);
            }

            return Newline;
        }

        public override void ToAxion(CodeWriter c) {
            bool inAnonFn = Parent is FunctionDef fn && fn.Name == null;
            if (inAnonFn) {
                c.WriteLine(" {");
            }

            c.Indent++;
            c.MaybeWriteLine();
            c.AddJoin("", Items, true);
            c.MaybeWriteLine();
            c.Indent--;
            
            if (inAnonFn) {
                c.Write("}");
            }
        }

        public override void ToCSharp(CodeWriter c) {
            c.WriteLine();
            c.WriteLine("{");
            c.Indent++;
            foreach (Expr item in Items) {
                c.Write(item);
                if (!(item is IDefinitionExpr
                   || item is ConditionalExpr
                   || item is WhileExpr) || item is VarDef) {
                    c.Write(";");
                }

                c.MaybeWriteLine();
            }

            c.Indent--;
            c.WriteLine("}");
        }

        public override void ToPython(CodeWriter c) {
            c.Write(":");
            c.Indent++;
            c.WriteLine();
            c.AddJoin("", Items, true);
            c.Indent--;
            c.MaybeWriteLine();
        }
    }

    [Flags]
    public enum BlockType {
        Default,
        Named,
        Loop,
        Lambda
    }
}