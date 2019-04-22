using System.Collections.Generic;
using Axion.Core.Processing.CodeGen;
using Axion.Core.Processing.Syntactic.Expressions;
using Axion.Core.Processing.Syntactic.Expressions.TypeNames;
using Axion.Core.Processing.Syntactic.Statements;
using Axion.Core.Processing.Syntactic.Statements.Definitions;
using Axion.Core.Specification;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Axion.Core.Processing.Syntactic {
    public class Ast : SyntaxTreeNode {
        internal readonly SourceUnit     SourceUnit;
        internal          bool           InLoop;
        internal          bool           InFinally;
        internal          bool           InFinallyLoop;
        internal          int            Index = -1;
        private           BlockStatement root;

        public BlockStatement Root {
            get => root;
            private set {
                if (value != null) {
                    value.Parent = this;
                }

                root = value;
            }
        }

        private readonly Stack<FunctionDefinition> functions = new Stack<FunctionDefinition>();

        public FunctionDefinition? CurrentFunction {
            get {
                if (functions != null
                    && functions.Count > 0) {
                    return functions.Peek();
                }

                return null;
            }
        }

        public FunctionDefinition? PopFunction() {
            if (functions != null
                && functions.Count > 0) {
                return functions.Pop();
            }

            return null;
        }

        public void PushFunction(FunctionDefinition function) {
            functions.Push(function);
        }

        internal Ast(SourceUnit unit) {
            SourceUnit = unit;
            Root       = new BlockStatement(this);
        }

        internal void Parse() {
            Parent = this;
            while (!MaybeEat(TokenType.End)) {
                Root.Statements.AddRange(Statement.ParseStmt(this));
            }
        }

        internal CSharpSyntaxNode ToCSharp() {
            if (Root.Statements.Count == 0) {
                return CompilationUnit();
            }

            var builder = new CodeBuilder(OutLang.CSharp);

            if (SourceUnit.ProcessingMode == SourceProcessingMode.Interpret) {
                foreach (Statement stmt in Root.Statements) {
                    if (stmt is ModuleDefinition) {
                        Unit.ReportError("Modules are not supported in interpretation mode", stmt);
                    }

                    else if (stmt is ClassDefinition || stmt is FunctionDefinition) {
                        stmt.ToCSharpCode(builder);
                    }
                    else {
                        builder.Write(stmt);
                    }

                    builder.WriteLine();
                }
            }
            else {
                var rootStmts = new NodeList<Statement>(this);
                foreach (Statement stmt in Root.Statements) {
                    if (stmt is ModuleDefinition) {
                        stmt.ToCSharpCode(builder);

                        continue;
                    }

                    if (stmt is ClassDefinition) {
                        builder.WriteLine("namespace _Root_ {");
                        builder.Writer.Indent++;
                        stmt.ToCSharpCode(builder);
                        builder.Writer.Indent--;
                        builder.Write("}");
                        continue;
                    }

                    if (stmt is FunctionDefinition) {
                        builder.WriteLine("namespace _Root_ {");
                        builder.Writer.Indent++;
                        builder.WriteLine("public partial class _RootClass_ {");
                        builder.Writer.Indent++;
                        stmt.ToCSharpCode(builder);
                        builder.Writer.Indent--;
                        builder.WriteLine("}");
                        builder.Writer.Indent--;
                        builder.Write("}");
                        continue;
                    }

                    rootStmts.Add(stmt);
                }

                builder.WriteLine("public partial class _RootClass_ {");
                builder.WriteLine(
                    new FunctionDefinition(
                        new SimpleNameExpression("Main"),
                        block: new BlockStatement(rootStmts),
                        returnType: new SimpleTypeName("void")
                    )
                );
                builder.Write("}");
            }

            SyntaxTree tree = CSharpSyntaxTree.ParseText(builder);

            // add imports
            var unit = (CompilationUnitSyntax) tree.GetRoot();
            unit = unit
                   .AddUsings(
                       UsingDirective(ParseName("System")),
                       UsingDirective(ParseName("System.IO")),
                       UsingDirective(ParseName("System.Linq")),
                       UsingDirective(ParseName("System.Text")),
                       UsingDirective(ParseName("System.Threading")),
                       UsingDirective(ParseName("System.Diagnostics")),
                       UsingDirective(ParseName("System.Collections")),
                       UsingDirective(ParseName("System.Collections.Generic"))
                   )
                   .NormalizeWhitespace();

            return unit;
        }

        public override void ToAxionCode(CodeBuilder c) {
            c.AddJoin("\n", Root.Statements);
        }

        public override void ToCSharpCode(CodeBuilder c) {
            c.Write(Root);
        }
    }
}