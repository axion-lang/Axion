using System;
using System.Collections.Generic;
using System.Diagnostics;
using Axion.Core.Tokens;

namespace Axion.Core.Processing {
    public abstract class AbstractLexer {
        /// <summary>
        ///     Reference to outgoing <see cref="LinkedList{T}" /> of tokens.
        /// </summary>
        protected LinkedList<Token> Tokens { get; }

        /// <summary>
        ///     All errors that found during lexical analysis.
        /// </summary>
        protected List<Exception> Errors { get; }

        /// <summary>
        ///     All warnings that found during lexical analysis.
        /// </summary>
        protected List<Exception> Warnings { get; }

        /// <summary>
        ///     Lexical analysis options enum.
        /// </summary>
        protected SourceProcessingOptions Options { get; set; }

        /// <summary>
        ///     Current processing stream.
        /// </summary>
        protected CharStream Stream;

        protected AbstractLexer(
            string                  codeToProcess,
            LinkedList<Token>       outTokens,
            List<Exception>         outErrors,
            List<Exception>         outWarnings,
            SourceProcessingOptions processingOptions = SourceProcessingOptions.None
        ) {
            Stream   = new CharStream(codeToProcess);
            Tokens   = outTokens ?? new LinkedList<Token>();
            Errors   = outErrors ?? new List<Exception>();
            Warnings = outWarnings ?? new List<Exception>();
            Options  = processingOptions;
        }

        protected AbstractLexer(
            CharStream              fromStream,
            LinkedList<Token>       outTokens,
            List<Exception>         outErrors,
            List<Exception>         outWarnings,
            SourceProcessingOptions processingOptions = SourceProcessingOptions.None
        ) {
            Stream   = new CharStream(fromStream);
            Tokens   = outTokens ?? new LinkedList<Token>();
            Errors   = outErrors ?? new List<Exception>();
            Warnings = outWarnings ?? new List<Exception>();
            Options  = processingOptions;
        }

        protected abstract void AddPresets(
            List<MultilineCommentToken> unclosedMultilineComments = null,
            List<StringToken>           unclosedStrings           = null,
            string[]                    processingTerminators     = null
        );

        public abstract void Process();

        protected void ReportError(ErrorType occurredErrorType, Token token) {
            if (token == null) {
                throw new ArgumentNullException(nameof(token));
            }
            Debug.Assert(occurredErrorType != ErrorType.None);

            Errors.Add(new SyntaxException(occurredErrorType, Stream.Source, token));
        }

        protected void ReportWarning(WarningType occurredWarningType, Token token) {
            if (token == null) {
                throw new ArgumentNullException(nameof(token));
            }
            Debug.Assert(occurredWarningType != WarningType.None);

            Warnings.Add(new SyntaxException(occurredWarningType, Stream.Source, token));
        }
    }
}