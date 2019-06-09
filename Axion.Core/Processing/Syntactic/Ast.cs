using System;
using System.Collections.Generic;
using Axion.Core.Processing.CodeGen;
using Axion.Core.Processing.Errors;
using Axion.Core.Processing.Syntactic.Expressions;
using Axion.Core.Processing.Syntactic.Expressions.Atomic;
using Axion.Core.Processing.Syntactic.Expressions.Definitions;
using Axion.Core.Processing.Syntactic.Expressions.TypeNames;
using Axion.Core.Processing.Syntactic.MacroPatterns;
using Axion.Core.Specification;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Axion.Core.Processing.Syntactic {
    public class Ast : BlockExpression {
        internal readonly SourceUnit            SourceUnit;
        internal          bool                  InLoop;
        internal          bool                  InFinally;
        internal          bool                  InFinallyLoop;
        internal          int                   Index = -1;
        internal readonly List<MacroDefinition> Macros;
        internal readonly List<SpannedRegion>   MacroApplicationParts = new List<SpannedRegion>();
        internal          Type                  MacroExpectationType;

        /// <summary>
        ///     Constructor for root AST block.
        /// </summary>
        internal Ast(SourceUnit unit) {
            SourceUnit = unit;
            Parent     = this;
            Items      = new NodeList<Expression>(this);
            Macros = new List<MacroDefinition> {
                // do..while/until
                new MacroDefinition(
                    new TokenPattern("do"),
                    new ExpressionPattern(typeof(BlockExpression)),
                    new OrPattern(new TokenPattern("while"), new TokenPattern("until")),
                    new ExpressionPattern(ParseInfixExpr)
                ),
                // until..
                new MacroDefinition(
                    new TokenPattern("until"),
                    new ExpressionPattern(ParseInfixExpr),
                    new ExpressionPattern(typeof(BlockExpression))
                ),
                new MacroDefinition(
                    new TokenPattern("for"),
                    new ExpressionPattern(ParseAtomExpr),
                    new TokenPattern("in"),
                    new ExpressionPattern(ParseInfixExpr),
                    new ExpressionPattern(typeof(BlockExpression))
                ),
                // unless..elif..else
                new MacroDefinition(
                    new TokenPattern("unless"),
                    new ExpressionPattern(ParseInfixExpr),
                    new ExpressionPattern(typeof(BlockExpression)),
                    new OptionalPattern(
                        new OptionalPattern(
                            new MultiplePattern(
                                new TokenPattern("elif"),
                                new ExpressionPattern(ParseInfixExpr),
                                new ExpressionPattern(typeof(BlockExpression))
                            )
                        ),
                        new CascadePattern(
                            new TokenPattern("else"),
                            new ExpressionPattern(typeof(BlockExpression))
                        )
                    )
                ),
                // list initializer
                new MacroDefinition(
                    new TokenPattern("["),
                    new OptionalPattern(new ExpressionPattern(typeof(Expression))),
                    new TokenPattern("]")
                ),
                // type_initializer_expr:
                //     'new' type ['(' arg_list ')'] ['{' '}']
                new MacroDefinition(
                    new TokenPattern("new"),
                    new ExpressionPattern(typeof(TypeName)),
                    new OptionalPattern(
                        new TokenPattern("("),
                        new ExpressionPattern(typeof(Expression)),
                        new TokenPattern(")")
                    ),
                    new OptionalPattern(
                        new TokenPattern("{"),
                        new OptionalPattern(
                            new ExpressionPattern(typeof(Expression)),
                            new OptionalPattern(
                                new TokenPattern(","),
                                new MultiplePattern(new ExpressionPattern(typeof(Expression)))
                            ),
                            new ExpressionPattern(typeof(Expression))
                        ),
                        new TokenPattern("}")
                    )
                ),
                new MacroDefinition(
                    new ExpressionPattern(typeof(Expression)),
                    new TokenPattern("match"),
                    new MultiplePattern(
                        new TokenPattern("@"),
                        new ExpressionPattern(typeof(Expression)),
                        new TokenPattern("=>"),
                        new ExpressionPattern(typeof(Expression))
                    )
                )
            };
        }

        internal void Parse() {
            while (!MaybeEat(TokenType.End)) {
                Items.AddRange(ParseCascade(this));
            }
        }

        internal CSharpSyntaxNode ToCSharp() {
            if (Items.Count == 0) {
                return CompilationUnit();
            }

            var b = new CodeBuilder(OutLang.CSharp);

            if (SourceUnit.ProcessingMode == SourceProcessingMode.Interpret) {
                foreach (Expression stmt in Items) {
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
                var rootItems = new NodeList<Expression>(this);
                foreach (Expression stmt in Items) {
                    if (stmt is ModuleDefinition) {
                        b.Write(stmt);
                    }
                    else if (stmt is ClassDefinition) {
                        b.WriteLine("namespace _ {");
                        b.Writer.Indent++;
                        b.Write(stmt);
                        b.Writer.Indent--;
                        b.Write("}");
                    }
                    else if (stmt is FunctionDefinition) {
                        b.WriteLine("namespace _ {");
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
                        rootItems.Add(stmt);
                    }
                }

                b.WriteLine("public partial class _RootClass_ {");
                b.WriteLine(
                    new FunctionDefinition(
                        new SimpleNameExpression("Main"),
                        block: new BlockExpression(rootItems),
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
    }
}