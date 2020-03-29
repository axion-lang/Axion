using System.Collections.Generic;
using System.Linq;
using Axion.Core.Processing.Syntactic.Expressions.Common;
using Axion.Core.Processing.Syntactic.Expressions.Definitions;
using Axion.Core.Processing.Syntactic.Expressions.MacroPatterns;

namespace Axion.Core.Processing.Syntactic.Expressions {
    /// <summary>
    ///     Actually, it's not an exact expression,
    ///     but some piece of code defined as expression
    ///     by language macros.
    /// </summary>
    public class MacroApplicationExpr : AtomExpr, IDecorableExpr {
        private MacroDef? macroDef;

        public MacroDef? MacroDef {
            get => macroDef;
            private set => macroDef = Bind(value);
        }

        public List<Span> Expressions { get; } = new List<Span>();
        public MacroApplicationExpr(Expr parent) : base(parent) { }

        public MacroApplicationExpr Parse() {
            SetSpan(
                () => {
                    Ast.MacroApplicationParts.Push(this);
                    MacroDef = Ast.Macros.FirstOrDefault(
                        macro => macro.Syntax.Patterns.Length > 0
                              && macro.Syntax.Patterns[0] is TokenPattern
                              && macro.Syntax.Match(Parent)
                    );
                    Ast.MacroApplicationParts.Pop();
                }
            );
            return this;
        }

        public new MacroApplicationExpr Parse(Expr leftExpr) {
            SetSpan(
                () => {
                    Ast.MacroApplicationParts.Push(this);
                    int startIdx = Stream.TokenIdx;

                    MacroDef = Ast.Macros.FirstOrDefault(
                        m => m.Syntax.Patterns.Length > 1
                          && m.Syntax.Patterns[1] is TokenPattern t
                          && t.Value == Stream.Peek.Value
                    );
                    if (MacroDef != null) {
                        var restCascade = new CascadePattern(MacroDef.Syntax.Patterns.Skip(2).ToArray());
                        Expressions.Add(Stream.EatAny());
                        Expressions.Add(leftExpr);
                        if (!restCascade.Match(Parent)) {
                            MacroDef = null;
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