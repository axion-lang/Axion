using Axion.Core.Processing.Errors;
using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Processing.Syntactic.Expressions.Atomic;
using Axion.Core.Processing.Syntactic.Expressions.Common;
using Axion.Core.Processing.Syntactic.Expressions.Generic;
using Axion.Specification;
using Magnolia.Attributes;
using Magnolia.Trees;
using static Axion.Specification.TokenType;

namespace Axion.Core.Processing.Syntactic.Expressions.Statements;

/// <summary>
///     <code>
///     import-expr:
///         'import' (
///             (INDENT import-entry+ OUTDENT)
///           | import-entry
///         );
///     </code>
/// </summary>
[Branch]
public partial class ImportExpr : Node {
    [Leaf] NodeList<ImportEntry, Ast>? entries;
    [Leaf] Token? kwImport;

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
    ///     <code>
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
    ///     </code>
    /// </summary>
    ImportEntry ParseEntry(NameExpr? prefixName = null) {
        var rootNameTokens = new NodeList<Token, Ast>(this);
        if (prefixName != null) {
            rootNameTokens.AddRange(prefixName.Tokens);
        }
        rootNameTokens.Add(Stream.Eat(Identifier));
        var rootName = new NameExpr(this) {
            Tokens = rootNameTokens
        };
        var subEntries = new NodeList<ImportEntry, Ast>(this);
        var exceptions = new NodeList<NameExpr, Ast>(this);
        NameExpr? alias = null;
        if (Stream.MaybeEat(Dot)) {
            var hasParens = Stream.PeekIs(OpenParenthesis);
            var es = Multiple.Parse(this, _ => ParseEntry(rootName));
            if (es is not TupleExpr tpl) {
                return new ImportEntry(
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
            foreach (var e in tpl.Expressions) {
                if (e is ImportEntry name) {
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
                    foreach (var e in tpl.Expressions) {
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

        return new ImportEntry(
            this,
            rootName,
            subEntries,
            exceptions
        ) {
            Alias = alias
        };
    }
}
