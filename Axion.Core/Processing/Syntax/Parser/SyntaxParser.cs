using System.Collections.Generic;
using Axion.Core.Processing.Errors;
using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Processing.Syntax.Tree.Expressions;
using Axion.Core.Processing.Syntax.Tree.Statements;
using Axion.Core.Processing.Syntax.Tree.Statements.Definitions;
using Axion.Core.Processing.Syntax.Tree.Statements.Small;

namespace Axion.Core.Processing.Syntax.Parser {
    public partial class SyntaxParser {
        private          bool                      inLoop, inFinally, inFinallyLoop;
        private readonly Stack<FunctionDefinition> functions = new Stack<FunctionDefinition>();
        
        /// <summary>
        ///     Current processing stream.
        /// </summary>
        private readonly TokenStream stream;

        private readonly SourceUnit unit;

        /// <summary>
        ///     Start position of current token in stream.
        /// </summary>
        private Position tokenStart => stream.Token.Span.StartPosition;

        /// <summary>
        ///     End position of current token in stream.
        /// </summary>
        private Position tokenEnd => stream.Token.Span.EndPosition;

        internal SyntaxParser(SourceUnit unit) {
            stream = new TokenStream(this, unit.Tokens);
            this.unit   = unit;
        }

        internal void Process() {
            if (stream.Tokens.Count == 0) {
                return;
            }
            
            var statements = new List<Statement>();
            while (!stream.MaybeEat(TokenType.EndOfCode)) {
                Statement statement = ParseStmt();
                // TODO: automatically add imports to ast.
                statements.Add(statement);
            }

            unit.SyntaxTree.Root = new BlockStatement(statements.ToArray());
            unit.SyntaxTree.MarkPosition(
                stream.Tokens[0],
                stream.Tokens[stream.Tokens.Count - 1]
            );
        }
        
        private Token StartExprOrStmt(TokenType type) {
            stream.Eat(type);
            return stream.Token;
        }

        #region Current function

        private FunctionDefinition currentFunction {
            get {
                if (functions != null
                    && functions.Count > 0) {
                    return functions.Peek();
                }

                return null;
            }
        }

        private FunctionDefinition PopFunction() {
            if (functions != null
                && functions.Count > 0) {
                return functions.Pop();
            }

            return null;
        }

        private void PushFunction(FunctionDefinition function) {
            functions.Push(function);
        }

        #endregion
        
        #region Errors reporting

        internal void BlameInvalidSyntax(TokenType expectedType, SpannedRegion mark) {
            unit.ReportError(
                "Invalid syntax, expected '"
                + expectedType.GetValue()
                + "', got '"
                + stream.Peek.Type.GetValue()
                + "'.",
                mark
            );
        }

        private ErrorExpression Error() {
            return new ErrorExpression(tokenStart, tokenEnd);
        }

        private ExpressionStatement ErrorStmt() {
            return new ExpressionStatement(Error());
        }

        private bool CheckUnexpectedEoc() {
            if (stream.PeekIs(TokenType.EndOfCode)) {
                unit.Blame(BlameType.UnexpectedEndOfCode, stream.Token);
                return true;
            }
            return false;
        }

        #endregion
    }
}