using System;
using System.Collections.Generic;
using System.Diagnostics;
using Axion.Core.Processing.Errors;
using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Specification;

namespace Axion.Core.Processing.Lexical.Lexer {
    public class LexerBase {
        /// <summary>
        ///     Reference to outgoing <see cref="List{T}" /> of tokens.
        /// </summary>
        protected readonly List<Token> Tokens;

        /// <summary>
        ///     All errors and warnings that found during lexical analysis.
        /// </summary>
        protected readonly List<Exception> Blames;

        /// <summary>
        ///     Lexical analysis options enum.
        /// </summary>
        protected SourceProcessingOptions Options;

        /// <summary>
        ///     Current processing stream.
        /// </summary>
        protected CharStream Stream;

        protected LexerBase(
            string                  codeToProcess,
            List<Token>             outTokens,
            List<Exception>         outBlames,
            SourceProcessingOptions processingOptions = SourceProcessingOptions.None
        ) {
            Stream  = new CharStream(codeToProcess);
            Tokens  = outTokens ?? new List<Token>();
            Blames  = outBlames ?? new List<Exception>();
            Options = processingOptions;
        }

        protected LexerBase(
            CharStream              fromStream,
            List<Token>             outTokens,
            List<Exception>         outBlames,
            SourceProcessingOptions processingOptions = SourceProcessingOptions.None
        ) {
            Stream  = new CharStream(fromStream);
            Tokens  = outTokens ?? new List<Token>();
            Blames  = outBlames ?? new List<Exception>();
            Options = processingOptions;
        }

        protected void Blame(BlameType type, Position startPos, Position endPos) {
            Debug.Assert(type != BlameType.None);

            BlameSeverity severity = Spec.Blames[type];
            Blames.Add(
                new LanguageException(new Blame(type, severity, startPos, endPos), Stream.Source)
            );
        }
    }
}