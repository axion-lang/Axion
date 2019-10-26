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
        private int charIdx = -1;
        private int lineIdx;
        private int columnIdx;
        private int prevLineLen;

        public string Text { get; }

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
        public string C {
            get {
                if (charIdx < 0) {
                    return Spec.Eoc;
                }

                return Text[charIdx].ToString();
            }
        }

        public TextStream(string text) {
            if (!text.EndsWith(Spec.Eoc)) {
                text += Spec.Eoc;
            }

            Text = text;
        }

        /// <summary>
        ///     Returns next N characters from stream,
        ///     not eating them.
        /// </summary>
        public string Peek(int length = 1) {
            Debug.Assert(length > 0);
            if (charIdx + 1 + length >= 0 && charIdx + 1 + length < Text.Length) {
                return Text.Substring(charIdx + 1, length);
            }

            return Spec.Eoc.PadRight(length, Spec.Eoc[0]);
        }

        /// <summary>
        ///     Compares next substring from stream
        ///     with expected strings.
        /// </summary>
        public bool PeekIs(params string[] expected) {
            Debug.Assert(expected.Length > 0);
            return expected.Any(s => s == Peek(s.Length));
        }

        /// <summary>
        ///     Consumes next substring from stream,
        ///     checking that it's equal to expected.
        /// </summary>
        public string Eat(params string[] expected) {
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

        private void Move(int by = 1) {
            Debug.Assert(by != 0);
            if (by > 0) {
                while (by > 0 && Peek() != Spec.Eoc) {
                    if (C == "\n") {
                        lineIdx++;
                        prevLineLen = columnIdx;
                        columnIdx   = 0;
                    }
                    else {
                        columnIdx++;
                    }

                    charIdx++;
                    by--;
                }
            }
            else {
                while (by < 0) {
                    charIdx--;
                    if (C == "\n") {
                        lineIdx--;
                        columnIdx = prevLineLen;
                    }
                    else {
                        columnIdx--;
                    }

                    by++;
                }
            }
        }
    }
}