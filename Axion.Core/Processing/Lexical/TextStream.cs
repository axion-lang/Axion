using System.Diagnostics;
using System.Linq;
using Axion.Core.Specification;

namespace Axion.Core.Processing.Lexical {
    /// <summary>
    ///     Character stream with possibility
    ///     to peek next char, rest of line,
    ///     and moving backwards.
    /// </summary>
    public class TextStream {
        private int    charIdx = -1;
        private int    lineIdx;
        private int    columnIdx;
        public  string Text { get; }

        /// <summary>
        ///     0-based (Line, Column) position of character in source code.
        /// </summary>
        public Location Location => new Location(lineIdx, columnIdx);

        /// <summary>
        ///     Checks that next character is line/source terminator.
        /// </summary>
        public bool AtEndOfLine => Spec.Eols.Contains(Peek()) || PeekIs(Spec.Eoc);

        public string RestOfLine {
            get {
                string textFromCurrent = Text.Substring(charIdx + 1);
                int    i               = textFromCurrent.IndexOf('\n');
                if (i == -1) {
                    return textFromCurrent;
                }

                return textFromCurrent.Substring(0, i + 1);
            }
        }

        /// <summary>
        ///     Current (eaten) character.
        /// </summary>
        public string C => charIdx < 0 ? Spec.Eoc.ToString() : Text[charIdx].ToString();

        public TextStream(string text) {
            if (!text.EndsWith(Spec.Eoc)) {
                text += Spec.Eoc;
            }

            Text = text;
        }

        /// <summary>
        ///     Returns next character from stream,
        ///     not eating it.
        /// </summary>
        public char Peek() {
            if (charIdx + 2 >= 0 && charIdx + 2 < Text.Length) {
                return Text[charIdx + 1];
            }

            return Spec.Eoc;
        }

        /// <summary>
        ///     Returns next N characters from stream,
        ///     not eating them.
        /// </summary>
        public string Peek(int length) {
            if (charIdx + 1 + length >= 0 && charIdx + 1 + length < Text.Length) {
                return Text.Substring(charIdx + 1, length);
            }

            return new string(Spec.Eoc, length);
        }

        /// <summary>
        ///     Compares next substring from stream
        ///     with expected strings.
        /// </summary>
        public bool PeekIs(params string[] expected) {
            // TODO: resolve bottleneck (select peek pieces to element of each unique size?)
            return expected.Any(s => s == Peek(s.Length));
        }

        public bool PeekIs(params char[] expected) {
            return expected.Contains(Peek());
        }

        /// <summary>
        ///     Consumes next substring from stream,
        ///     checking that it's equal to expected.
        /// </summary>
        public string? Eat(params string[] expected) {
            if (expected.Length == 0) {
                Move();
                return C;
            }

            foreach (string value in expected) {
                string nxt = Peek(value.Length);
                if (nxt == value) {
                    Move(nxt.Length);
                    return nxt;
                }
            }

            return null;
        }

        /// <summary>
        ///     Consumes next char from stream,
        ///     checking that it's equal to expected.
        /// </summary>
        public string? Eat(params char[] expected) {
            if (expected.Length == 0) {
                Move();
                return C;
            }
            char nxt = Peek();
            if (expected.Contains(nxt)) {
                Move();
                return nxt.ToString();
            }

            return null;
        }

        private void Move(int by = 1) {
            Debug.Assert(by > 0);
            while (by > 0 && Peek() != Spec.Eoc) {
                if (Peek() == '\n') {
                    lineIdx++;
                    columnIdx = 0;
                }
                else {
                    columnIdx++;
                }

                charIdx++;
                by--;
            }
        }
    }
}