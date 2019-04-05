using System.Collections.Generic;
using Axion.Core.Processing.CodeGen;
using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Processing.Syntactic.Expressions;
using Axion.Core.Processing.Syntactic.Statements.Interfaces;
using JetBrains.Annotations;

namespace Axion.Core.Processing.Syntactic.Statements.Definitions {
    /// <summary>
    ///     <c>
    ///         module_def ::=
    ///             'module' name block
    ///     </c>
    /// </summary>
    public class ModuleDefinition : Statement, IDecorated {
        private Expression name;

        [NotNull]
        public Expression Name {
            get => name;
            set => SetNode(ref name, value);
        }

        private BlockStatement block;

        [NotNull]
        public BlockStatement Block {
            get => block;
            set => SetNode(ref block, value);
        }

        public List<Expression> Modifiers { get; set; }

        internal ModuleDefinition([NotNull] Expression name, [NotNull] BlockStatement block) {
            Name  = name;
            Block = block;
            MarkPosition(name, block);
        }

        internal ModuleDefinition(SyntaxTreeNode parent) {
            Parent = parent;
            StartNode(TokenType.KeywordModule);

            Name  = new NameExpression(this);
            Block = new BlockStatement(this, BlockType.Top);

            MarkEnd(Token);
        }

        internal override CodeBuilder ToAxionCode(CodeBuilder c) {
            return c + "module " + Name + " " + Block;
        }

        internal override CodeBuilder ToCSharpCode(CodeBuilder c) {
            return c + "namespace " + Name + Block;
        }
    }
}