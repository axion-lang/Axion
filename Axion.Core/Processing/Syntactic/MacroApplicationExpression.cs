using System.Collections.Generic;
using System.Linq;
using Axion.Core.Processing.CodeGen;
using Axion.Core.Processing.Syntactic.Definitions;
using Axion.Core.Processing.Syntactic.MacroPatterns;

namespace Axion.Core.Processing.Syntactic {
    /// <summary>
    ///     Actually, it's not an exact expression,
    ///     but some piece of code defined as expression
    ///     by language macros.
    /// </summary>
    public class MacroApplicationExpression : Expression {
        private MacroDefinition macroDefinition;

        public MacroDefinition MacroDefinition {
            get => macroDefinition;
            set => SetNode(ref macroDefinition, value);
        }

        public List<SpannedRegion> Expressions;

        /// <summary>
        ///     Tries to match next piece of source
        ///     with previously defined macro syntax-es.
        ///     This class instance should be deleted immediately if it's
        ///     <see cref="MacroDefinition" /> is null.
        /// </summary>
        internal MacroApplicationExpression(
            Expression parent
        ) {
            Construct(parent, () => {
                MacroDefinition = Ast.Macros.FirstOrDefault(macro => macro.Syntax.Match(parent));
                if (MacroDefinition != null) {
                    Expressions = new List<SpannedRegion>(Ast.MacroApplicationParts);
                    Ast.MacroApplicationParts.Clear();
                }
            });
        }

        /// <summary>
        ///     Infix macros (starts with expression).
        ///     Tries to match next piece of source
        ///     with previously defined macro syntax-es.
        ///     This class instance should be deleted immediately if it's
        ///     <see cref="MacroDefinition" /> is null.
        /// </summary>
        internal MacroApplicationExpression(
            Expression parent,
            Expression leftExpr
        ) {
            Construct(parent, leftExpr, () => {
                MacroDefinition infixMacroDef = Ast.Macros.FirstOrDefault(
                    m => m.Syntax.Patterns.Length > 1
                      && m.Syntax.Patterns[1] is TokenPattern t
                      && t.Value == parent.Peek.Value
                );
                if (infixMacroDef == null) {
                    return;
                }

                int startIdx = Ast.CurrentTokenIndex;
                Eat();
                Ast.MacroApplicationParts.Add(Token);
                var restCascade =
                    new CascadePattern(infixMacroDef.Syntax.Patterns.Skip(2).ToArray());
                if (restCascade.Match(parent)) {
                    MacroDefinition = infixMacroDef;
                    Expressions = new List<SpannedRegion> {
                        leftExpr
                    };
                    Expressions.AddRange(Ast.MacroApplicationParts);
                    Ast.MacroApplicationParts.Clear();
                }
                else {
                    MoveTo(startIdx);
                }
            });
        }

        internal override void ToAxionCode(CodeBuilder c) {
            c.Write("(");
            c.AddJoin(", ", Expressions);
            c.Write(")");
        }

        internal override void ToCSharpCode(CodeBuilder c) {
            c.Write("(");
            c.AddJoin(", ", Expressions);
            c.Write(")");
        }
    }
}