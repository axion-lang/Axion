using System;
using System.Linq;
using Axion.Core.Processing.Syntactic;
using Axion.Core.Processing.Syntactic.Statements.Definitions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Axion.Core.Processing.CodeGen {
    public class CSharpToAxionWalker : CSharpSyntaxWalker {
        private static int tabs;

        public override void Visit(SyntaxNode node) {
            tabs++;
            var indents = new string('\t', tabs);
            Console.WriteLine(indents + node.Kind());
            base.Visit(node);
            tabs--;
        }
    }

    public static class CSharpToAxion {
        //internal static readonly IndentedTextWriter Result;

        internal static SyntaxTreeNode ConvertNode(SyntaxNode node) {
            if (node is NamespaceDeclarationSyntax n) {
                return new ModuleDefinition(n);
            }

            throw new NotSupportedException();
        }

        internal static NodeList<T> ConvertNodeList<T>(
            SyntaxTreeNode         parent,
            SyntaxList<SyntaxNode> csList
        )
            where T : SyntaxTreeNode? {
            return new NodeList<T>(parent, csList.Select(x => (T) ConvertNode(x)));
        }
    }
}