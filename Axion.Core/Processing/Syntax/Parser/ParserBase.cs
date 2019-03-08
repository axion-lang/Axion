using System;
using System.Collections.Generic;
using System.Diagnostics;
using Axion.Core.Processing.Errors;
using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Processing.Syntax.Tree;
using Axion.Core.Processing.Syntax.Tree.Expressions;
using Axion.Core.Processing.Syntax.Tree.Statements.Small;
using Axion.Core.Specification;

namespace Axion.Core.Processing.Syntax.Parser {
    public class ParserBase {
        /// <summary>
        ///     Outgoing Abstract Syntax Tree.
        /// </summary>
        protected readonly Ast Ast;

        /// <summary>
        ///     Current processing stream.
        /// </summary>
        protected readonly TokenStream Stream;

        /// <summary>
        ///     Start position of current token in stream.
        /// </summary>
        protected Position tokenStart => Stream.Token.Span.StartPosition;

        /// <summary>
        ///     End position of current token in stream.
        /// </summary>
        protected Position tokenEnd => Stream.Token.Span.EndPosition;
        
        /// <summary>
        ///     Contains all errors that raised during syntax analysis.
        /// </summary>
        private readonly List<Exception> blames;

        protected ParserBase(List<Token> tokens, Ast outAst, List<Exception> outBlames) {
            Stream = new TokenStream(this, tokens);
            Ast    = outAst ?? throw new ArgumentNullException(nameof(outAst));
            blames = outBlames ?? new List<Exception>();
        }
        
        #region Errors reporting

        internal void BlameInvalidSyntax(TokenType expectedType, SpannedRegion mark) {
            ReportError(
                "Invalid syntax, expected '"
                + expectedType.GetValue()
                + "', got '"
                + Stream.Peek.Type.GetValue()
                + "'.",
                mark
            );
        }

        protected ErrorExpression Error() {
            return new ErrorExpression(tokenStart, tokenEnd);
        }

        protected ExpressionStatement ErrorStmt() {
            return new ExpressionStatement(Error());
        }

        protected bool CheckUnexpectedEoc() {
            if (Stream.PeekIs(TokenType.EndOfCode)) {
                Blame(BlameType.UnexpectedEndOfCode, Stream.Token);
                return true;
            }
            return false;
        }

        protected void ReportError(string message, SpannedRegion mark) {
            blames.Add(
                new LanguageException(
                    new Blame(message, BlameSeverity.Error, mark.Span),
                    Ast.Source
                )
            );
        }

        protected void Blame(BlameType type, SpannedRegion region) {
            Blame(type, region.Span.StartPosition, region.Span.EndPosition);
        }

        protected void Blame(BlameType type, Position start, Position end) {
            Debug.Assert(type != BlameType.None);

            blames.Add(
                new LanguageException(new Blame(type, Spec.Blames[type], start, end), Ast.Source)
            );
        }

        #endregion
    }
}