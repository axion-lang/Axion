using System;
using System.Collections.Generic;
using System.Linq;
using Axion.Core.Processing.CodeGen;
using Axion.Core.Processing.Errors;
using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Processing.Source;
using Axion.Core.Processing.Syntactic.Atomic;
using Axion.Core.Processing.Syntactic.Definitions;
using Axion.Core.Processing.Syntactic.MacroPatterns;
using Axion.Core.Processing.Syntactic.TypeNames;
using Axion.Core.Specification;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Axion.Core.Specification.TokenType;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Axion.Core.Processing.Syntactic {
    /// <summary>
    ///     Abstract Syntax Tree of
    ///     file with Axion source code.
    /// </summary>
    public class Ast : BlockExpression {
        internal readonly SourceUnit            SourceUnit;
        internal          int                   CurrentTokenIndex     = -1;
        internal readonly List<MacroDefinition> Macros                = new List<MacroDefinition>();
        internal readonly List<SpannedRegion>   MacroApplicationParts = new List<SpannedRegion>();

        /// <summary>
        ///     Constructor for root AST block.
        /// </summary>
        internal Ast(SourceUnit unit) {
            SourceUnit = unit;
            Parent     = this;
            Items      = new NodeList<Expression>(this);
        }

        internal TokenPattern NewTokenPattern(string keyword) {
            if (keyword.All(Spec.IsIdPart)) {
                for (int i = Math.Max(0, CurrentTokenIndex); i < Unit.Tokens.Count; i++) {
                    Token token = Unit.Tokens[i];
                    if (token.Value == keyword
                     && !Spec.Keywords.ContainsKey(token.Value)
                     && !Spec.Operators.ContainsKey(token.Value)) {
                        token.Type = CustomKeyword;
                    }
                }
            }

            return new TokenPattern(keyword);
        }

        internal void Parse() {
            Macros.Add(
                new MacroDefinition(
                    NewTokenPattern("do"),
                    new ExpressionPattern(typeof(BlockExpression)),
                    new OrPattern(NewTokenPattern("while"), NewTokenPattern("until")),
                    new ExpressionPattern(ParseInfix)
                ));
            Macros.Add(
                new MacroDefinition(
                    NewTokenPattern("until"),
                    new ExpressionPattern(ParseInfix),
                    new ExpressionPattern(typeof(BlockExpression))
                ));
            Macros.Add(
                new MacroDefinition(
                    NewTokenPattern("for"),
                    new ExpressionPattern(ParseAtom),
                    NewTokenPattern("in"),
                    new ExpressionPattern(ParseInfix),
                    new ExpressionPattern(typeof(BlockExpression))
                ));
            Macros.Add(
                new MacroDefinition(
                    NewTokenPattern("unless"),
                    new ExpressionPattern(ParseInfix),
                    new ExpressionPattern(typeof(BlockExpression)),
                    new OptionalPattern(
                        new OptionalPattern(
                            new MultiplePattern(
                                NewTokenPattern("elif"),
                                new ExpressionPattern(ParseInfix),
                                new ExpressionPattern(typeof(BlockExpression))
                            )
                        ),
                        new CascadePattern(
                            NewTokenPattern("else"),
                            new ExpressionPattern(typeof(BlockExpression))
                        )
                    )
                ));
            Macros.Add(
                new MacroDefinition(
                    NewTokenPattern("["),
                    new OptionalPattern(new ExpressionPattern(typeof(Expression))),
                    NewTokenPattern("]")
                ));
            Macros.Add(
                new MacroDefinition(
                    NewTokenPattern("new"),
                    new ExpressionPattern(typeof(TypeName)),
                    new OptionalPattern(
                        NewTokenPattern("("),
                        new ExpressionPattern(typeof(Expression)),
                        NewTokenPattern(")")
                    ),
                    new OptionalPattern(
                        NewTokenPattern("{"),
                        new OptionalPattern(
                            new ExpressionPattern(typeof(Expression)),
                            new OptionalPattern(
                                new MultiplePattern(
                                    NewTokenPattern(","),
                                    new ExpressionPattern(typeof(Expression))
                                )
                            ),
                            new ExpressionPattern(typeof(Expression))
                        ),
                        NewTokenPattern("}")
                    )
                ));
            Macros.Add(
                new MacroDefinition(
                    new ExpressionPattern(typeof(Expression)),
                    NewTokenPattern("match"),
                    new MultiplePattern(
                        NewTokenPattern("|"),
                        new ExpressionPattern(typeof(Expression)),
                        NewTokenPattern("=>"),
                        new ExpressionPattern(typeof(Expression))
                    )
                ));

            while (!MaybeEat(End)) {
                Items.AddRange(ParseCascade());
            }
        }

        internal CSharpSyntaxNode ToCSharp() {
            if (Items.Count == 0) {
                return CompilationUnit();
            }

            var b = new CodeBuilder(OutLang.CSharp);

            if (SourceUnit.ProcessingMode == SourceProcessingMode.Interpret) {
                foreach (Expression e in Items) {
                    if (e is ModuleDefinition) {
                        Unit.Blame(BlameType.ModulesAreNotSupportedInInterpretationMode, e);
                    }
                    else {
                        b.Write(e);
                    }
                    // Semicolon after method or accessor block is not valid
                    if (!(e is FunctionDefinition)) {
                        b.WriteLine(";");
                    }
                }
            }
            else {
                var rootItems = new NodeList<Expression>(this);
                foreach (Expression e in Items) {
                    if (e is ModuleDefinition) {
                        b.Write(e);
                    }
                    else if (e is ClassDefinition) {
                        b.WriteLine("namespace _ {");
                        b.Writer.Indent++;
                        b.Write(e);
                        b.Writer.Indent--;
                        b.Write("}");
                    }
                    else if (e is FunctionDefinition) {
                        b.WriteLine("namespace _ {");
                        b.Writer.Indent++;
                        b.WriteLine("public partial class _RootClass_ {");
                        b.Writer.Indent++;
                        b.Write(e);
                        b.Writer.Indent--;
                        b.WriteLine("}");
                        b.Writer.Indent--;
                        b.Write("}");
                    }
                    else {
                        rootItems.Add(e);
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