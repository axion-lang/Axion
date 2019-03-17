using System.Collections.Generic;
using Axion.Core.Processing.Syntax.Tree.Expressions.TypeNames;
using Axion.Core.Processing.Syntax.Tree.Statements;
using Axion.Core.Processing.Syntax.Tree.Statements.Definitions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Axion.Core.Processing.Syntax.Tree {
    public class Ast : SyntaxTreeNode {
        internal readonly SourceUnit Unit;
        //private readonly Stack<ImportDefinition> imports = new Stack<ImportDefinition>();

        private BlockStatement root;

        public BlockStatement Root {
            get => root;
            set {
                if (value != null) {
                    value.Parent = this;
                }

                root = value;
            }
        }

        internal Ast(SourceUnit unit) {
            Unit = unit;
        }

        internal CSharpSyntaxNode ToCSharp() {
            if (Root.Statements.Length == 0) {
                return CompilationUnit();
            }

            if (Unit.ProcessingMode == SourceProcessingMode.Interpret) {
                return ToCSharpX();
            }

            var builder   = new CSharpCodeBuilder();
            var rootStmts = new List<Statement>();

            foreach (Statement stmt in Root.Statements) {
                if (stmt is ModuleDefinition m) {
                    m.ToCSharpCode(builder);
                    continue;
                }

                if (stmt is ClassDefinition c) {
                    builder += "namespace _Root_ {";
                    c.ToCSharpCode(builder);
                    builder += "}";
                    continue;
                }

                if (stmt is FunctionDefinition f) {
                    builder += "namespace _Root_ {"
                               + "public partial class _RootClass_ {";
                    f.ToCSharpCode(builder);
                    builder += "}}";
                    continue;
                }

                rootStmts.Add(stmt);
            }

            var rootFn = new FunctionDefinition(
                "Main",
                block: new BlockStatement(rootStmts.ToArray()),
                returnType: new SimpleTypeName("void")
            );

            builder += "public partial class _RootClass_ {";
            builder += rootFn;
            builder += "}";

            SyntaxTree tree = CSharpSyntaxTree.ParseText(builder);

            // add imports
            var unit = (CompilationUnitSyntax) tree.GetRoot();
            unit = unit.AddUsings(
                UsingDirective(ParseName("System")),
                UsingDirective(ParseName("System.Diagnostics")),
                UsingDirective(ParseName("System.Collections"))
            );

            // format
            var ws = new AdhocWorkspace();
            unit = (CompilationUnitSyntax) Formatter.Format(unit, ws);
            return unit;
        }

        private CompilationUnitSyntax ToCSharpX() {
            var builder = new CSharpCodeBuilder();

            foreach (Statement stmt in Root.Statements) {
                if (stmt is ModuleDefinition) {
                    continue;
                }

                if (stmt is ClassDefinition c) {
                    c.ToCSharpCode(builder);
                    continue;
                }

                if (stmt is FunctionDefinition f) {
                    f.ToCSharpCode(builder);
                    continue;
                }

                builder += stmt;
            }

            SyntaxTree tree = CSharpSyntaxTree.ParseText(builder);

            var unit = (CompilationUnitSyntax) tree.GetRoot();
            unit = unit.AddUsings(
                UsingDirective(ParseName("System")),
                UsingDirective(ParseName("System.Diagnostics")),
                UsingDirective(ParseName("System.Collections"))
            );

            // format
            var ws = new AdhocWorkspace();
            unit = (CompilationUnitSyntax) Formatter.Format(unit, ws);
            return unit;
        }

        internal override AxionCodeBuilder ToAxionCode(AxionCodeBuilder c) {
            return c + Root;
        }

        internal override CSharpCodeBuilder ToCSharpCode(CSharpCodeBuilder c) {
            return c + Root;
        }
    }
}