using System.Collections.Generic;
using System.Linq;
using Axion.Core.Processing.CodeGen;
using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Processing.Syntactic.Expressions.TypeNames;
using Axion.Core.Processing.Syntactic.Statements;
using Axion.Core.Processing.Syntactic.Statements.Definitions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Axion.Core.Processing.Syntactic {
    public class Ast : SyntaxTreeNode {
        internal readonly SourceUnit     SourceUnit;
        internal          bool           InLoop;
        internal          bool           InFinally;
        internal          bool           InFinallyLoop;
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

        public FunctionDefinition currentFunction {
            get {
                if (functions != null
                    && functions.Count > 0) {
                    return functions.Peek();
                }

                return null;
            }
        }

        public FunctionDefinition PopFunction() {
            if (functions != null
                && functions.Count > 0) {
                return functions.Pop();
            }

            return null;
        }

        public void PushFunction(FunctionDefinition function) {
            functions.Push(function);
        }

        internal new int _Index = -1;

        internal new Token Token =>
            _Index > -1
            && _Index < Unit.Tokens.Count
                ? Unit.Tokens[_Index]
                : Unit.Tokens.Last();

        internal new Token Peek =>
            _Index + 1 < Unit.Tokens.Count
                ? Unit.Tokens[_Index + 1]
                : Unit.Tokens.Last();

        internal Ast(SourceUnit unit) {
            SourceUnit = unit;
        }

        internal void Parse() {
            Parent = this;
            var statements = new List<Statement>();
            while (!MaybeEat(TokenType.End)) {
                statements.Add(Statement.ParseStmt(this));
            }

            Root = new BlockStatement(statements);
        }

        internal CSharpSyntaxNode ToCSharp() {
            if (Root.Statements.Count == 0) {
                return CompilationUnit();
            }

            var builder = new CodeBuilder(OutLang.CSharp);

            if (SourceUnit.ProcessingMode == SourceProcessingMode.Interpret) {
                foreach (Statement stmt in Root.Statements) {
                    if (stmt is ModuleDefinition) {
                        // no modules in interactive c#
                        continue;
                    }

                    if (stmt is ClassDefinition || stmt is FunctionDefinition) {
                        stmt.ToCSharpCode(builder);
                        continue;
                    }

                    builder += stmt;
                }
            }
            else {
                var rootStmts = new List<Statement>();
                foreach (Statement stmt in Root.Statements) {
                    if (stmt is ModuleDefinition) {
                        stmt.ToCSharpCode(builder);

                        continue;
                    }

                    if (stmt is ClassDefinition) {
                        builder += "namespace _Root_ {";
                        stmt.ToCSharpCode(builder);
                        builder += "}";
                        continue;
                    }

                    if (stmt is FunctionDefinition) {
                        builder += "namespace _Root_ {";
                        builder += "public partial class _RootClass_ {";
                        stmt.ToCSharpCode(builder);
                        builder += "}}";
                        continue;
                    }

                    rootStmts.Add(stmt);
                }

                builder += "public partial class _RootClass_ {";
                builder += new FunctionDefinition(
                    "Main",
                    block: new BlockStatement(rootStmts.ToArray()),
                    returnType: new SimpleTypeName("void")
                );
                builder += "}";
            }

            SyntaxTree tree = CSharpSyntaxTree.ParseText(builder);

            // add imports
            var unit = (CompilationUnitSyntax) tree.GetRoot();
            unit = unit
                   .AddUsings(
                       UsingDirective(ParseName("System")),
                       UsingDirective(ParseName("System.IO")),
                       UsingDirective(ParseName("System.Linq")),
                       UsingDirective(ParseName("System.Diagnostics")),
                       UsingDirective(ParseName("System.Collections"))
                   )
                   .NormalizeWhitespace();

            // format
            var ws = new AdhocWorkspace();
            unit = (CompilationUnitSyntax) Formatter.Format(unit, ws);
            return unit;
        }

        internal override CodeBuilder ToAxionCode(CodeBuilder c) {
            return c + Root;
        }

        internal override CodeBuilder ToCSharpCode(CodeBuilder c) {
            return c + Root;
        }
    }
}