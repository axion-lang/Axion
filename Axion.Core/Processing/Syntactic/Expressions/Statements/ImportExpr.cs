using Axion.Core.Processing.Errors;
using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Processing.Syntactic.Expressions.Atomic;
using Axion.Core.Specification;
using static Axion.Core.Processing.Lexical.Tokens.TokenType;

namespace Axion.Core.Processing.Syntactic.Expressions.Statements {
    /// <summary>
    ///     <c>
    ///         break-expr:
    ///             'import' scope;
    ///     </c>
    ///     The `scope` can contain only `name-expr`-s.
    /// </summary>
    public class ImportExpr : Expr {
        private Token? kwImport;

        public Token? KwImport {
            get => kwImport;
            set => kwImport = BindNullable(value);
        }

        private ScopeExpr? members;

        public ScopeExpr? Members {
            get => members;
            set => members = BindNullable(value);
        }

        public ImportExpr(Node parent) : base(parent) { }

        public ImportExpr Parse() {
            KwImport = Stream.Eat(KeywordImport);
            if (Stream.PeekIs(Spec.NeverExprStartTypes)) {
                LangException.Report(
                    BlameType.ExpectedImportedModuleName,
                    Stream.Peek
                );
                return this;
            }

            Members = new ScopeExpr(this).Parse();
            BindImports(Members, "");
            return this;
        }

        private void BindImports(ScopeExpr scope, string namePrefix) {
            for (var i = 0; i < scope.Items.Count; i++) {
                Expr e = scope.Items[i];
                if (e is NameExpr importName) {
                    var fullName = importName.ToString();
                    if (fullName == Ast.Unit.Module.FullName) {
                        LangException.Report(
                            BlameType.ModuleSelfImport,
                            importName
                        );
                        continue;
                    }

                    if (Ast.Unit.Imports.ContainsKey(fullName)) {
                        LangException.Report(
                            BlameType.DuplicatedImport,
                            importName
                        );
                        continue;
                    }

                    // TODO Ast.Source.Imports.Add(fullName, Ast.Source.Module.Root.Bind(fullName));
                }
                else if (e is ScopeExpr nameScope) {
                    var prefix = scope.Items[i - 1] as NameExpr;
                    if (prefix == null) {
                        continue;
                    }

                    BindImports(nameScope, prefix.ToString());
                }
                else {
                    LangException.Report(
                        BlameType.ExpectedImportedModuleName,
                        e
                    );
                }
            }
        }
    }
}
