using System.Collections.Generic;
using Axion.Core.Processing.CodeGen;
using Axion.Core.Processing.Errors;
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

        private readonly Stack<IFunctionNode> functions = new Stack<IFunctionNode>();

        public IFunctionNode? CurrentFunction {
            get {
                if (functions != null
                    && functions.Count > 0) {
                    return functions.Peek();
                }

                return null;
            }
        }

        public IFunctionNode? PopFunction() {
            if (functions != null
                && functions.Count > 0) {
                return functions.Pop();
            }

            return null;
        }

        public void PushFunction(IFunctionNode function) {
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

            var b = new CodeBuilder(OutLang.CSharp);

            if (SourceUnit.ProcessingMode == SourceProcessingMode.Interpret) {
                foreach (Statement stmt in Root.Statements) {
                    if (stmt is ModuleDefinition) {
                        Unit.Blame(BlameType.ModulesAreNotSupportedInInterpretationMode, stmt);
                    }
                    else {
                        b.Write(stmt);
                    }

                    b.WriteLine();
                }
            }
            else {
                var rootStmts = new NodeList<Statement>(this);
                foreach (Statement stmt in Root.Statements) {
                    if (stmt is ModuleDefinition) {
                        b.Write(stmt);
                    }
                    else if (stmt is ClassDefinition) {
                        b.WriteLine("namespace _Root_ {");
                        b.Writer.Indent++;
                        b.Write(stmt);
                        b.Writer.Indent--;
                        b.Write("}");
                    }
                    else if (stmt is FunctionDefinition) {
                        b.WriteLine("namespace _Root_ {");
                        b.Writer.Indent++;
                        b.WriteLine("public partial class _RootClass_ {");
                        b.Writer.Indent++;
                        b.Write(stmt);
                        b.Writer.Indent--;
                        b.WriteLine("}");
                        b.Writer.Indent--;
                        b.Write("}");
                    }
                    else {
                        rootStmts.Add(stmt);
                    }
                }

                b.WriteLine("public partial class _RootClass_ {");
                b.WriteLine(
                    new FunctionDefinition(
                        new SimpleNameExpression("Main"),
                        block: new BlockStatement(rootStmts),
                        returnType: new SimpleTypeName("void")
                    )
                );
                b.Write("}");
            }

            SyntaxTree tree = CSharpSyntaxTree.ParseText(b);
            var        unit = (CompilationUnitSyntax) tree.GetRoot();
            unit = unit
                   .AddUsings(
                       UsingDirective(ParseName("System")),
                       UsingDirective(ParseName("System.IO")),
                       UsingDirective(ParseName("System.Linq")),
                       UsingDirective(ParseName("System.Text")),
                       UsingDirective(ParseName("System.Numerics")),
                       UsingDirective(ParseName("System.Threading")),
                       UsingDirective(ParseName("System.Diagnostics")),
                       UsingDirective(ParseName("System.Collections")),
                       UsingDirective(ParseName("System.Collections.Generic"))
                   )
                   .NormalizeWhitespace();
            return unit;
        }

        internal override void ToAxionCode(CodeBuilder c) {
            c.AddJoin("\n", Root.Statements);
        }

        internal override void ToCSharpCode(CodeBuilder c) {
            c.Write(Root);
        }
    }
}