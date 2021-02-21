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
    public class MacroMatchExpr : AtomExpr {
        public MacroDef? Macro { get; set; }

        public List<Node> Nodes { get; } = new();

        public MacroMatchExpr(Node parent) : base(parent) { }

        public MacroMatchExpr Parse() {
            Ast.MatchedMacros.Push(this);
            Macro = Ast.Macros.FirstOrDefault(
                m => m.Syntax.Patterns.Count > 0
                  && m.Syntax.Patterns[0] is TokenPattern
                  && m.Syntax.Match(this)
            );
            if (Macro == default) {
                Nodes.Clear();
            }
            else {
                Start = Nodes[0].Start;
                End   = Nodes[^1].End;
            }
            Ast.MatchedMacros.Pop();
            return this;
        }

        public MacroMatchExpr Parse(Expr leftExpr) {
            Ast.MatchedMacros.Push(this);
            var startIdx = Stream.TokenIdx;

            Macro = Ast.Macros.FirstOrDefault(
                m => m.Syntax.Patterns.Count > 1
                  && m.Syntax.Patterns[1] is TokenPattern t
                  && t.Match(this)
            );
            if (Macro != null) {
                var restCascade = new CascadePattern(this) {
                    Patterns = new NodeList<Pattern>(
                        this,
                        Macro.Syntax.Patterns.Skip(2)
                    )
                };
                Nodes.Insert(0, leftExpr);
                if (restCascade.Match(this)) {
                    Start = Nodes[0].Start;
                    End   = Nodes[^1].End;
                }
                else {
                    Macro = null;
                    Stream.MoveAbsolute(startIdx);
                    Nodes.Clear();
                }
            }

            Ast.MatchedMacros.Pop();
            return this;
        }
    }
}
