using System;
using System.Collections.Generic;
using System.Diagnostics;
using Axion.Core.Processing.Errors;
using Axion.Core.Processing.Lexical.Tokens;

namespace Axion.Core.Processing.Lexical {
    public abstract class AbstractLexer {
        /// <summary>
        ///     Reference to outgoing <see cref="LinkedList{T}" /> of tokens.
        /// </summary>
        protected List<Token> Tokens { get; }

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
            List<Token>             outTokens,
            List<Exception>         outErrors,
            List<Exception>         outWarnings,
            SourceProcessingOptions processingOptions = SourceProcessingOptions.None
        ) {
            Stream   = new CharStream(codeToProcess);
            Tokens   = outTokens ?? new List<Token>();
            Errors   = outErrors ?? new List<Exception>();
            Warnings = outWarnings ?? new List<Exception>();
            Options  = processingOptions;
        }

        protected AbstractLexer(
            CharStream              fromStream,
            List<Token>             outTokens,
            List<Exception>         outErrors,
            List<Exception>         outWarnings,
            SourceProcessingOptions processingOptions = SourceProcessingOptions.None
        ) {
            Stream   = new CharStream(fromStream);
            Tokens   = outTokens ?? new List<Token>();
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

        protected void ReportError(
            ErrorType  type,
            (int, int) startPos,
            (int, int) endPos
        ) {
            Debug.Assert(type != ErrorType.None);

            Errors.Add(new SyntaxException(new Error(type, startPos, endPos), Stream.Source));
        }

        protected void ReportWarning(
            WarningType type,
            (int, int)  startPos,
            (int, int)  endPos
        ) {
            Debug.Assert(type != WarningType.None);

            Warnings.Add(new SyntaxException(new Warning(type, startPos, endPos), Stream.Source));
        }
    }
}