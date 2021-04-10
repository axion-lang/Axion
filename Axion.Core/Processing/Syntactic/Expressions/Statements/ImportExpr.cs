using Axion.Core.Processing.Errors;
using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Processing.Syntactic.Expressions.Atomic;
using Axion.Core.Processing.Syntactic.Expressions.Common;
using Axion.Core.Processing.Syntactic.Expressions.Generic;
using Axion.SourceGenerators;
using Axion.Specification;
using static Axion.Specification.TokenType;

namespace Axion.Core.Processing.Syntactic.Expressions.Statements {
    /// <summary>
    ///     <c>
    ///     import-expr:
    ///         'import' (
    ///             (INDENT import-entry+ OUTDENT)
    ///           | import-entry
    ///         );
    ///     </c>
    /// </summary>
    [SyntaxExpression]
    public partial class ImportExpr : Node {
        [LeafSyntaxNode] Token? kwImport;
        [LeafSyntaxNode] NodeList<Entry>? entries;

        public ImportExpr(Node parent) : base(parent) { }

        public ImportExpr Parse() {
            KwImport = Stream.Eat(KeywordImport);
            if (Stream.PeekIs(Identifier)) {
                Entries.Add(ParseEntry());
            }
            else if (Stream.MaybeEat(Indent)) {
                while (!Stream.MaybeEat(Outdent, TokenType.End)) {
                    Entries.Add(ParseEntry());
                }
            }
            else {
                LanguageReport.To(
                    BlameType.ExpectedImportedModuleName,
                    Stream.Peek
                );
            }
            return this;
        }

        /// <summary>
        ///     <c>
        ///     import-entry:
        ///         name (
        ///             ['.' (
        ///                 import-entry
        ///               | ( '(' import-entry (',' import-entry)* [','] ')' )
        ///             )]
        ///             ['as' name]
        ///             ['except' name [(',' name)* [',']]]
        ///         )
        ///         | (INDENT import-entry+ OUTDENT);
        ///     </c>
        /// </summary>
        Entry ParseEntry(NameExpr? prefixName = null) {
            var rootNameTokens = new NodeList<Token>(this);
            if (prefixName != null) {
                rootNameTokens.AddRange(prefixName.Tokens);
            }
            rootNameTokens.Add(Stream.Eat(Identifier));
            var rootName = new NameExpr(this) {
                Tokens = rootNameTokens
            };
            var subEntries = new NodeList<Entry>(this);
            var exceptions = new NodeList<NameExpr>(this);
            NameExpr? alias = null;
            if (Stream.MaybeEat(Dot)) {
                var hasParens = Stream.PeekIs(OpenParenthesis);
                var es = Multiple.Parse(this, _ => ParseEntry(rootName));
                if (es is not TupleExpr tpl) {
                    return new Entry(
                        this,
                        rootName,
                        subEntries,
                        exceptions
                    ) {
                        Alias = alias
                    };
                }
                if (tpl.Expressions.Count == 1 && hasParens) {
                    LanguageReport.To(BlameType.RedundantParentheses, tpl);
                }
                foreach (Node e in tpl.Expressions) {
                    if (e is Entry name) {
                        subEntries.Add(name);
                    }
                    else {
                        LanguageReport.To(
                            BlameType.ExpectedImportedModuleName,
                            e
                        );
                    }
                }
            }
            else if (Stream.MaybeEat(Indent)) {
                while (!Stream.MaybeEat(Outdent, TokenType.End)) {
                    subEntries.Add(ParseEntry(rootName));
                }
            }
            else {
                if (Stream.MaybeEat("except")) {
                    var hasParens = Stream.PeekIs(OpenParenthesis);
                    var importExceptions = Multiple.Parse<AtomExpr>(this);
                    if (importExceptions is TupleExpr tpl) {
                        if (tpl.Expressions.Count == 1 && hasParens) {
                            LanguageReport.To(BlameType.RedundantParentheses, tpl);
                        }
                        foreach (Node e in tpl.Expressions) {
                            if (e is NameExpr name) {
                                exceptions.Add(name);
                            }
                            else {
                                LanguageReport.To(
                                    BlameType.ExpectedImportedModuleName,
                                    e
                                );
                            }
                        }
                    }
                }
                if (Stream.MaybeEat("as")) {
                    alias = new NameExpr(this).Parse();
                }
            }

            return new Entry(
                this,
                rootName,
                subEntries,
                exceptions
            ) {
                Alias = alias
            };
        }

        public class Entry : Node {
            public NameExpr FullName => Parent is NameExpr pn
                ? new NameExpr(this, pn.ToString())
                : Name;

            public NameExpr Name { get; }

            public NodeList<Entry> Children { get; }

            public NodeList<NameExpr> Exceptions { get; }

            public NameExpr? Alias { get; set; }

            public Entry(
                Node?              parent,
                NameExpr           name,
                NodeList<Entry>    children,
                NodeList<NameExpr> exceptions
            ) : base(parent) {
                Name = name;
                Children = children;
                Exceptions = exceptions;
            }
        }
    }
}
