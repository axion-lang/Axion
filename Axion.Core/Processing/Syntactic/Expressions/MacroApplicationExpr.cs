using System.Collections.Generic;
using System.Linq;
using Axion.Core.Processing.Syntactic.Expressions.Common;
using Axion.Core.Processing.Syntactic.Expressions.Definitions;
using Axion.Core.Processing.Syntactic.Expressions.Patterns;

namespace Axion.Core.Processing.Syntactic.Expressions {
    /// <summary>
    ///     Actually, it's not an exact expression,
    ///     but some piece of code defined as expression
    ///     by language macros.
    /// </summary>
    public class MacroApplicationExpr : AtomExpr {
        public MacroDef? Macro { get; set; }

        public List<Node> Expressions { get; } = new List<Node>();

        public MacroApplicationExpr(Node parent) : base(parent) { }

        public MacroApplicationExpr Parse() {
            SetSpan(
                () => {
                    Ast.MacroApplicationParts.Push(this);
                    Macro = Ast.Macros.FirstOrDefault(
                        m => m.Syntax.Patterns.Count > 0
                          && m.Syntax.Patterns[0] is TokenPattern
                          && m.Syntax.Match(this)
                    );
                    Ast.MacroApplicationParts.Pop();
                }
            );
            return this;
        }

        public MacroApplicationExpr Parse(Expr leftExpr) {
            SetSpan(
                () => {
                    Ast.MacroApplicationParts.Push(this);
                    int startIdx = Stream.TokenIdx;

                    Macro = Ast.Macros.FirstOrDefault(
                        m => m.Syntax.Patterns.Count > 1
                          && m.Syntax.Patterns[1] is TokenPattern t
                          && t.Match(this)
                    );
                    if (Macro != null) {
                        var restCascade = new CascadePattern(this) {
                            Patterns = new NodeList<Pattern>(this, Macro.Syntax.Patterns.Skip(2))
                        };
                        Expressions.Add(Stream.EatAny());
                        Expressions.Add(leftExpr);
                        if (!restCascade.Match(this)) {
                            Macro = null;
                            Stream.MoveAbsolute(startIdx);
                            Expressions.Clear();
                        }
                    }

                    Ast.MacroApplicationParts.Pop();
                }
            );
            return this;
        }
    }
}
