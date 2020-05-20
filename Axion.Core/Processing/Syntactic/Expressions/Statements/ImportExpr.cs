using System.Linq;
using Axion.Core.Processing.Errors;
using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Processing.Syntactic.Expressions.Atomic;
using Axion.Core.Source;
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
                LangException.Report(BlameType.ExpectedImportedModuleName, Stream.Peek);
                return this;
            }
            Members = new ScopeExpr(this).Parse();
            ParseNameScope(Members, "");
            return this;
        }

        private void ParseNameScope(ScopeExpr scope, string namePrefix) {
            foreach (Expr m in scope.Items) {
                if (m is NameExpr importName) {
                    var fullName = importName.ToString();
                    if (fullName == Ast.Source.Module.FullName) {
                        LangException.Report(BlameType.ModuleSelfImport, importName);
                        continue;
                    }
                    if (Ast.Source.Imports.ContainsKey(fullName)) {
                        LangException.Report(BlameType.DuplicatedImport, importName);
                        continue;
                    }
                    Ast.Source.Imports.Add(fullName, Ast.Source.Module.);
                }
                else {
                    LangException.Report(BlameType.ExpectedImportedModuleName, m);
                }
            }
        }
    }
}
