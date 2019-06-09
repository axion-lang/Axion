using System.Collections.Generic;
using Axion.Core.Processing.CodeGen;
using Axion.Core.Processing.Syntactic.Expressions.Definitions;

namespace Axion.Core.Processing.Syntactic.Expressions {
    public class MacroApplicationExpression : Expression {
        private MacroDefinition macroDefinition;

        public MacroDefinition MacroDefinition {
            get => macroDefinition;
            set => SetNode(ref macroDefinition, value);
        }

        public readonly List<SpannedRegion> Expressions;

        internal MacroApplicationExpression(
            AstNode         parent,
            MacroDefinition macroDef
        ) : base(parent) {
            MacroDefinition = macroDef;
            Expressions     = new List<SpannedRegion>(Ast.MacroApplicationParts);
            Ast.MacroApplicationParts.Clear();
        }

        internal MacroApplicationExpression(
            AstNode         parent,
            MacroDefinition macroDef,
            SpannedRegion   infixLeft
        ) : base(parent) {
            MacroDefinition = macroDef;
            Expressions = new List<SpannedRegion> {
                infixLeft
            };
            Expressions.AddRange(Ast.MacroApplicationParts);
            Ast.MacroApplicationParts.Clear();
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