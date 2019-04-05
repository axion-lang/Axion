﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using Axion.Core.Specification;
using ConsoleExtensions;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace Axion.Core.Processing.Errors {
    public class LanguageException : Exception {
        [JsonProperty]
        [NotNull]
        public readonly Blame Blame;

        public override string Message    { get; }
        public override string StackTrace { get; }

        [JsonProperty]
        private readonly string time = DateTime.Now.ToString(CultureInfo.InvariantCulture);

        internal LanguageException([NotNull] Blame blame) {
            Blame      = blame;
            Message    = blame.ToString();
            StackTrace = new StackTrace(2).ToString();
        }

        /// <summary>
        ///     Creates visual representation of occurred error in console.
        /// </summary>
        internal void Print(SourceUnit unit) {
            //--------Error templates--------
            //
            // Error: Invalid operator.
            //
            // |   8 | variable ~~ "string"
            // -----------------^^
            // ...
            //
            // Error: Mismatching parenthesis.

            // |   1 | func("string",
            // ------------^
            // |   2 |      'c',
            // |   3 |      123
            // ...
            //

            ConsoleColor color = Blame.Severity == BlameSeverity.Error
                ? ConsoleColor.Red
                : ConsoleColor.DarkYellow;

            // Write message
            ConsoleUI.WriteLine((Message, color));
            // Append file name if it's exists.
            ConsoleUI.WriteLine("In file '" + unit.SourceFilePath + "'.");

            Console.WriteLine();

            string[] codeLines = unit.Code.Split(Spec.EndOfLines, StringSplitOptions.None);

            var lines = new List<string>();
            // limit rest of code by 5 lines
            for (int i = Blame.Span.StartPosition.Line;
                i < codeLines.Length && lines.Count < 4;
                i++) {
                lines.Add(codeLines[i].TrimEnd('\n', Spec.EndOfCode));
            }

            if (lines.Count > codeLines.Length - Blame.Span.StartPosition.Line) {
                lines.Add("...");
            }

            // first line
            // <line number>| <code line>
            int pointerTailLength =
                ConsoleCodeEditor.LineNumberWidth + Blame.Span.StartPosition.Column;
            int errorTokenLength;
            if (Blame.Span.EndPosition.Line > Blame.Span.StartPosition.Line) {
                errorTokenLength = lines[0].Length - Blame.Span.StartPosition.Column;
            }
            else {
                errorTokenLength = Blame.Span.EndPosition.Column - Blame.Span.StartPosition.Column;
            }

            // upside arrows (^), should be red-colored
            string pointer =
                // tail of pointer
                new string(' ', pointerTailLength)
                +
                // pointer arrows
                new string(
                    '^', // TODO (UI) compute token value length: include tab lengths
                    errorTokenLength
                );

            // Drawing ==========

            // line with error
            ConsoleCodeEditor.PrintLineNumber(Blame.Span.StartPosition.Line + 1);
            ConsoleUI.WriteLine(lines[0]);
            // error pointer
            ConsoleUI.WriteLine((pointer, color));

            // next lines
            for (int lineIndex = Blame.Span.StartPosition.Line + 1;
                lineIndex < lines.Count;
                lineIndex++) {
                ConsoleCodeEditor.PrintLineNumber(lineIndex + 1);
                ConsoleUI.WriteLine(lines[lineIndex]);
            }

            Console.WriteLine();
        }
    }
}