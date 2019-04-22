using System.Collections.Generic;
using System.Diagnostics;
using Axion.Core.Specification;

namespace Axion.Core.Processing.Lexical {
    public partial class Lexer {
        /// <summary>
        ///     Index of current character.
        /// </summary>
        private int charIdx;

        /// <summary>
        ///     Current reading character.
        /// </summary>
        private char c => unit.Code[charIdx];

        /// <summary>
        ///     Index of current line.
        /// </summary>
        private int lineIdx;

        /// <summary>
        ///     Index of column in current line.
        /// </summary>
        private int columnIdx;

        /// <summary>
        ///     Line and column position of current character.
        /// </summary>
        private Position Position => (lineIdx, columnIdx);

        /// <summary>
        ///     Returns character next to current.
        /// </summary>
        private char Peek =>
            charIdx + 1 < unit.Code.Length
                ? unit.Code[charIdx + 1]
                : Spec.EndOfCode;

        /// <summary>
        ///     Length of previously processed line.
        ///     Used to return [<see cref="columnIdx" />]
        ///     to previous value when moving backward.
        /// </summary>
        private int prevLineLength;

        /// <summary>
        ///     Checks that current character
        ///     is a newline / end of code character.
        /// </summary>
        private bool AtEndOfLine => c == '\r' || c == '\n' || c == Spec.EndOfCode;

        /// <summary>
        ///     Returns rest of current line,
        ///     starting from current char
        ///     to nearest newline character.
        /// </summary>
        private string GetRestOfLine() {
            string fromCurrentIndex = unit.Code.Substring(charIdx);
            int    indexOfNewline   = fromCurrentIndex.IndexOf('\n');
            if (indexOfNewline == -1) {
                return fromCurrentIndex;
            }

            return fromCurrentIndex.Substring(0, indexOfNewline + 1);
        }

        /// <summary>
        ///     Returns string of specified [<see cref="length" />]
        ///     starting from current character.
        /// </summary>
        private string PeekPiece(int length) {
            if (charIdx + length < unit.Code.Length) {
                return unit.Code.Substring(charIdx, length);
            }

            return unit.Code.Substring(charIdx, unit.Code.Length - charIdx);
        }

        /// <summary>
        ///     Checks if next piece of string
        ///     (from current char) is equal to <paramref name="expected"/>.
        /// </summary>
        private bool NextIs(string expected) {
            return PeekPiece(expected.Length) == expected;
        }

        /// <summary>
        ///     Checks if next piece of string
        ///     (from current char) is equal to <paramref name="expected"/>.
        /// </summary>
        private bool CharIs(IReadOnlyList<char> expected) {
            for (var i = 0; i < expected.Count; i++) {
                if (c == expected[i]) {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        ///     Gets next or previous character from stream,
        ///     depending on [<see cref="byLength" />] value.
        /// </summary>
        private void Move(int byLength = 1) {
            Debug.Assert(byLength != 0);
            if (byLength > 0) {
                for (var _ = 0; _ < byLength && c != Spec.EndOfCode; _++) {
                    if (c == '\n') {
                        lineIdx++;
                        prevLineLength = columnIdx;
                        columnIdx      = 0;
                    }
                    else {
                        columnIdx++;
                    }

                    charIdx++;
                }
            }
            else {
                for (int _ = byLength; _ < 0; _++) {
                    charIdx--;
                    if (c == '\n') {
                        lineIdx--;
                        columnIdx = prevLineLength;
                    }
                    else {
                        columnIdx--;
                    }
                }
            }
        }
    }
}