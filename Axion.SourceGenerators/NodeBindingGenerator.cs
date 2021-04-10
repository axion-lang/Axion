using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Syntax.Bindings;
using S = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Axion.SourceGenerators {
    [Generator]
    public class NodeBindingGenerator : ISourceGenerator {
        const string syntaxBindingsNamespace = "Axion.SourceGenerators";
        const string syntaxExpressionAttrName = "SyntaxExpressionAttribute";
        const string leafSyntaxNodeAttrName = "LeafSyntaxNodeAttribute";

        // @formatter:off

        const string syntaxExpressionTemplate = @"using System;

namespace " + syntaxBindingsNamespace + @"
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = true, AllowMultiple = false)]
    public sealed class " + syntaxExpressionAttrName + @" : Attribute
    {
        public " + syntaxExpressionAttrName + @"()
        {
        }
    }
}
";

        const string leafSyntaxNodeTemplate = @"using System;

namespace " + syntaxBindingsNamespace + @"
{
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public sealed class " + leafSyntaxNodeAttrName + @" : Attribute
    {
        public " + leafSyntaxNodeAttrName + @"()
        {
        }
    }
}
";

        // @formatter:on

        static readonly string nl = Environment.NewLine;

        INamedTypeSymbol syntaxExpressionAttribute = null!;
        INamedTypeSymbol leafSyntaxNodeAttribute = null!;

        public void Initialize(GeneratorInitializationContext context) {
            context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
        }

        static void InjectAttribute(
            GeneratorExecutionContext context,
            string                    name,
            string                    code
        ) {
            context.AddSource(name, SourceText.From(code, Encoding.UTF8));
        }

        public void Execute(GeneratorExecutionContext context) {
            InjectAttribute(context, leafSyntaxNodeAttrName,   leafSyntaxNodeTemplate);
            InjectAttribute(context, syntaxExpressionAttrName, syntaxExpressionTemplate);

            GenerateProperties(context);
        }

        void GenerateProperties(GeneratorExecutionContext context) {
            if (context.SyntaxReceiver is not SyntaxReceiver receiver)
                return;

            Compilation compilation = GetCompilation(context);

            syntaxExpressionAttribute = compilation.GetTypeByMetadataName(
                $"{syntaxBindingsNamespace}.{syntaxExpressionAttrName}"
            )!;
            leafSyntaxNodeAttribute = compilation.GetTypeByMetadataName(
                $"{syntaxBindingsNamespace}.{leafSyntaxNodeAttrName}"
            )!;

            var symbols = GetClassSymbols(compilation, receiver);
            foreach (var symbol in symbols) {
                if (!symbol.HasAttribute(syntaxExpressionAttribute))
                    continue;

                context.AddSource(
                    $"{symbol.Name}.g.cs",
                    SourceText.From(CreatePropertiesCode(symbol), Encoding.UTF8)
                );
            }
        }

        static Compilation GetCompilation(GeneratorExecutionContext context) {
            var options = context.Compilation.SyntaxTrees.First().Options
                as CSharpParseOptions;

            return context.Compilation
                .AddSyntaxTrees(CSharpSyntaxTree.ParseText(
                    SourceText.From(leafSyntaxNodeTemplate, Encoding.UTF8),
                    options
                ))
                .AddSyntaxTrees(CSharpSyntaxTree.ParseText(
                    SourceText.From(syntaxExpressionTemplate, Encoding.UTF8),
                    options
                ));
        }

        string CreatePropertiesCode(INamedTypeSymbol classSymbol) {
            string namespaceName = classSymbol.ContainingNamespace.ToDisplayString();
            var (usings, props) = GeneratePropertiesCode(classSymbol);
            var namespaces = usings.Distinct()
                .Select(uc => uc.ToString().ToUsing());

            return $@"#nullable enable
{string.Join(nl, namespaces)}

namespace {namespaceName}
{{
    {GetAccessModifier(classSymbol)} partial class {classSymbol.Name}
    {{
{string.Join(nl + nl, props)}
    }}
}}";
        }

        (List<INamespaceSymbol> usings, string[] props) GeneratePropertiesCode(
            ITypeSymbol classSymbol
        ) {
            var usings = new List<INamespaceSymbol> { classSymbol.ContainingNamespace };
            var props = GetBoundFields(classSymbol)
                .Where(f => f.HasAttribute(leafSyntaxNodeAttribute))
                .Select(f => {
                    usings.Add(f.Type.ContainingNamespace);
                    return GeneratePropertyCode(f);
                })
                .ToArray();
            return (usings, props);
        }

        string GeneratePropertyCode(IFieldSymbol field) {
            string propertyName = char.ToUpper(field.Name[0]) + field.Name.Substring(1);
            var isList = field.Type.Name.StartsWith("NodeList");
            string[] attributes = field.GetAttributes()
                .Where(a => !a.AttributeClass!.DefaultEquals(leafSyntaxNodeAttribute))
                .Select(a => "[" + a + "]")
                .ToArray();
            string type =
                field.Type.WithNullableAnnotation(
                        isList
                            ? NullableAnnotation.NotAnnotated
                            : field.Type.NullableAnnotation
                    )
                    .ToString();
            string bindMethodName =
                field.Type.NullableAnnotation == NullableAnnotation.Annotated && !isList
                    ? "BindNullable"
                    : "Bind";
            string refFieldName =
                field.Name == "value"
                    ? "this.value"
                    : SyntaxFacts.GetKeywordKind(field.Name) == SyntaxKind.None
                   && SyntaxFacts.GetContextualKeywordKind(field.Name) == SyntaxKind.None
                        ? field.Name
                        : "@" + field.Name;
            string getPattern =
                isList
                    ? "InitIfNull(ref {0})"
                    : "{0}";
            return $@"{string.Join(nl, attributes)}
public {type} {propertyName} {{
    get => {string.Format(getPattern, refFieldName)};
    set => {refFieldName} = {bindMethodName}(value);
}}".Indent(8);
        }

        static string GetAccessModifier(INamedTypeSymbol classSymbol) {
            return classSymbol.DeclaredAccessibility.ToString().ToLowerInvariant();
        }

        static IEnumerable<IFieldSymbol> GetBoundFields(ITypeSymbol classSymbol) {
            var targetSymbolMembers = classSymbol
                .GetMembers()
                .OfType<IFieldSymbol>()
                .Where(x => x.CanBeReferencedByName);
            return targetSymbolMembers;
        }

        static IEnumerable<INamedTypeSymbol> GetClassSymbols(
            Compilation    compilation,
            SyntaxReceiver receiver
        ) {
            return receiver.CandidateClasses.Select(c => GetClassSymbol(compilation, c));
        }

        static INamedTypeSymbol GetClassSymbol(
            Compilation            compilation,
            ClassDeclarationSyntax clazz
        ) {
            var model = compilation.GetSemanticModel(clazz.SyntaxTree);
            var classSymbol = ModelExtensions.GetDeclaredSymbol(model, clazz)!;
            return (INamedTypeSymbol) classSymbol;
        }
    }
}
