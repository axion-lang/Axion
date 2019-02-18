using System;
using System.Collections.Generic;
using System.Diagnostics;
using Axion.Core.Processing.Errors;
using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Specification;

namespace Axion.Core.Processing.Lexical.Lexer {
    public abstract class AbstractLexer {
        /// <summary>
        ///     Reference to outgoing <see cref="List{T}" /> of tokens.
        /// </summary>
        protected readonly List<Token> tokens;

        /// <summary>
        ///     All errors and warnings that found during lexical analysis.
        /// </summary>
        protected readonly List<Exception> blames;

        /// <summary>
        ///     Lexical analysis options enum.
        /// </summary>
        protected SourceProcessingOptions options;

        /// <summary>
        ///     Current processing stream.
        /// </summary>
        protected CharStream Stream;

        protected AbstractLexer(
            string                  codeToProcess,
            List<Token>             outTokens,
            List<Exception>         outBlames,
            SourceProcessingOptions processingOptions = SourceProcessingOptions.None
        ) {
            Stream  = new CharStream(codeToProcess);
            tokens  = outTokens ?? new List<Token>();
            blames  = outBlames ?? new List<Exception>();
            options = processingOptions;
        }

        protected AbstractLexer(
            CharStream              fromStream,
            List<Token>             outTokens,
            List<Exception>         outBlames,
            SourceProcessingOptions processingOptions = SourceProcessingOptions.None
        ) {
            Stream  = new CharStream(fromStream);
            tokens  = outTokens ?? new List<Token>();
            blames  = outBlames ?? new List<Exception>();
            options = processingOptions;
        }

        public abstract void Process();

        protected abstract void AddPresets(
            List<MultilineCommentToken> unclosedMultilineComments = null,
            List<StringToken>           unclosedStrings           = null,
            string[]                    processingTerminators     = null
        );

        protected void Blame(BlameType type, Position startPos, Position endPos) {
            Debug.Assert(type != BlameType.None);

            BlameSeverity severity = Spec.Blames[type];
            blames.Add(
                new LanguageException(new Blame(type, severity, startPos, endPos), Stream.Source)
            );
        }
    }
}