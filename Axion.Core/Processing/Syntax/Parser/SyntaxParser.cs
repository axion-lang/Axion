using System;
using System.Collections.Generic;
using System.Diagnostics;
using Axion.Core.Processing.Errors;
using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Processing.Syntax.Tree;
using Axion.Core.Processing.Syntax.Tree.Expressions;
using Axion.Core.Processing.Syntax.Tree.Statements;
using Axion.Core.Processing.Syntax.Tree.Statements.Definitions;
using Axion.Core.Processing.Syntax.Tree.Statements.Small;
using Axion.Core.Specification;

namespace Axion.Core.Processing.Syntax.Parser {
    public partial class SyntaxParser {
        /// <summary>
        ///     Contains all errors that raised during syntax analysis.
        /// </summary>
        private readonly List<Exception> blames;

        /// <summary>
        ///     Current processing stream.
        /// </summary>
        private readonly TokenStream stream;

        /// <summary>
        ///     Start position of current token in stream.
        /// </summary>
        private Position tokenStart => stream.Token.Span.StartPosition;

        /// <summary>
        ///     End position of current token in stream.
        /// </summary>
        private Position tokenEnd => stream.Token.Span.EndPosition;

        /// <summary>
        ///     Outgoing Abstract Syntax Tree.
        /// </summary>
        private readonly Ast ast;

        private bool inLoop, inFinally, inFinallyLoop;

        private readonly Stack<FunctionDefinition> functions = new Stack<FunctionDefinition>();

        internal SyntaxParser(List<Token> tokens, Ast outAst, List<Exception> outBlames) {
            stream = new TokenStream(this, tokens);
            ast    = outAst ?? throw new ArgumentNullException(nameof(outAst));
            blames = outBlames ?? new List<Exception>();
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

            ast.Root = new BlockStatement(statements.ToArray());
        }

        #region Current function

        private FunctionDefinition currentFunction {
            get {
                if (functions != null && functions.Count > 0) {
                    return functions.Peek();
                }
                return null;
            }
        }

        private FunctionDefinition PopFunction() {
            if (functions != null && functions.Count > 0) {
                return functions.Pop();
            }
            return null;
        }

        private void PushFunction(FunctionDefinition function) {
            functions.Push(function);
        }

        #endregion

        private Token StartExprOrStmt(TokenType type) {
            stream.Eat(type);
            return stream.Token;
        }

        #region Errors reporting

        internal void BlameInvalidSyntax(TokenType expectedType, SpannedRegion mark) {
            ReportError(
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

        private bool CheckUnexpectedEOC() {
            if (stream.PeekIs(TokenType.EndOfCode)) {
                Blame(BlameType.UnexpectedEndOfCode, stream.Token);
                return true;
            }
            return false;
        }

        private void ReportError(string message, SpannedRegion mark) {
            blames.Add(
                new LanguageException(
                    new Blame(message, BlameSeverity.Error, mark.Span),
                    ast.Source
                )
            );
        }

        private void Blame(BlameType type, SpannedRegion region) {
            Blame(type, region.Span.StartPosition, region.Span.EndPosition);
        }

        private void Blame(BlameType type, Position start, Position end) {
            Debug.Assert(type != BlameType.None);

            blames.Add(
                new LanguageException(new Blame(type, Spec.Blames[type], start, end), ast.Source)
            );
        }

        #endregion
    }
}