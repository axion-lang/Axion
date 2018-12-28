using System.Linq;
using Axion.Core.Processing.Errors;
using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Processing.Syntax.Tree.Statements;
using Axion.Core.Processing.Syntax.Tree.Statements.Interfaces;
using Axion.Core.Specification;

namespace Axion.Core.Processing.Syntax.Parser {
    public partial class SyntaxParser {
        private Statement ParseModifiersStmt() {
            Token mod = stream.Peek;
            if (Spec.AccessModifiers.Contains(mod.Type)) {
                if (ast.CurrentClass == null
                    /*&& ast.CurrentModule == null*/) {
                    Blame(BlameType.CannotUseAccessModifierOutsideClass, mod);
                }
                stream.NextToken();
                // access region
                if (stream.PeekIs(TokenType.Colon)) {
                    Statement region = ParseSuite();
                    return new AccessRegionStatement(mod, region);
                }
            }
            else if (stream.PeekIs(TokenType.KeywordAsync)) {
                return ParseAsyncStmt();
            }
            return null;
        }

        // async_stmt: 'async' (func_def | with_stmt | for_stmt)
        private Statement ParseAsyncStmt() {
            stream.Eat(TokenType.KeywordAsync);
            Token start = stream.Token;

            Statement result = ParseStmt();
            if (result is IAsyncStatement asyncStmt) {
                asyncStmt.IsAsync = true;
            }
            else {
                Blame(BlameType.AsyncModifierIsInapplicableToThatStatement, start);
            }
            return result;
        }
    }
}