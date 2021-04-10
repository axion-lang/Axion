using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Syntax.Bindings {
    /// <summary>
    ///     Created on demand before each generation pass
    /// </summary>
    internal class SyntaxReceiver : ISyntaxReceiver {
        public IList<ClassDeclarationSyntax> CandidateClasses { get; } =
            new List<ClassDeclarationSyntax>();

        /// <summary>
        ///     Called for every syntax node in the compilation,
        ///     we can inspect the nodes and save any information useful for generation.
        /// </summary>
        public void OnVisitSyntaxNode(SyntaxNode node) {
            // any field with at least one attribute is a candidate for being cloneable
            if (node is ClassDeclarationSyntax { AttributeLists: { Count: > 0 } } cds) {
                CandidateClasses.Add(cds);
            }
        }
    }
}
