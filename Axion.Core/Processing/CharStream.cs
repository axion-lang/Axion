using System.Diagnostics;

namespace Axion.Core.Processing {
    internal class CharStream {
        /// <summary>
        ///     Stream source.
        /// </summary>
        internal readonly string Source;

        /// <summary>
        ///     Index of current processing character in source.
        /// </summary>
        internal int CharIdx { get; private set; }

        /// <summary>
        ///     Current processing character in source.
        /// </summary>
        internal char c => Source[CharIdx];

        /// <summary>
        ///     Index of current processing line.
        /// </summary>
        private int lineIdx;

        /// <summary>
        ///     Index of column in current processing line.
        /// </summary>
        private int columnIdx;

        /// <summary>
        ///     Line and column position of current processing character in source.
        /// </summary>
        internal (int line, int column) Position => (lineIdx, columnIdx);

        /// <summary>
        ///     Length of previous processed line.
        ///     Used to return <see cref="columnIdx"/>
        ///     to previous value when moving backward.
        /// </summary>
        private int prevLineLength;

        /// <summary>
        ///     Initializes a new stream with specified source.
        /// </summary>
        /// <param name="source"></param>
        public CharStream(string source) {
            Source = source;
        }

        /// <summary>
        ///     Copies <see cref="stream"/>
        ///     to current stream.
        /// </summary>
        /// <param name="stream"></param>
        internal CharStream(CharStream stream) {
            Source    = stream.Source;
            CharIdx   = stream.CharIdx;
            lineIdx   = stream.Position.line;
            columnIdx = stream.Position.column;
        }

        /// <summary>
        ///     Checks that current character
        ///     is a newline character.
        /// </summary>
        /// <returns></returns>
        internal bool AtEndOfLine() {
            return c == '\r' || c == Spec.EndOfLine;
        }

        /// <summary>
        ///     Returns rest of current line,
        ///     starting from current char
        ///     to nearest newline character.
        /// </summary>
        /// <returns></returns>
        internal string GetRestOfLine() {
            string fromCurrentIndex = Source.Substring(CharIdx);
            int    indexOfNewline   = fromCurrentIndex.IndexOf(Spec.EndOfLine);
            if (indexOfNewline == -1) {
                return fromCurrentIndex;
            }
            return Source.Substring(CharIdx, indexOfNewline);
        }

        /// <summary>
        ///     Returns character next to current character,
        ///     without moving to it.
        /// </summary>
        /// <returns></returns>
        internal char Peek() {
            if (CharIdx + 1 < Source.Length) {
                return Source[CharIdx + 1];
            }
            return Spec.EndOfStream;
        }

        /// <summary>
        ///     Returns string of specified <see cref="length"/>
        ///     starting from character next to current.
        /// </summary>
        /// <param name="length"></param>
        /// <returns></returns>
        internal string Peek(uint length) {
            // save values
            var savedPosition = Position;
            int savedCharIdx  = CharIdx;

            var value = "";
            for (var i = 0; i < length; i++) {
                Move();
                value += c;
            }

            // restore values
            lineIdx   = savedPosition.line;
            columnIdx = savedPosition.column;
            CharIdx   = savedCharIdx;
            return value;
        }

        /// <summary>
        ///     Gets next (or previous) character from stream,
        ///     depending on <see cref="byLength"/> value.
        /// </summary>
        internal void Move(int byLength = 1) {
            Debug.Assert(byLength != 0);
            if (byLength > 0) {
                for (var _ = 0; _ < byLength && c != Spec.EndOfStream; _++) {
                    if (c == Spec.EndOfLine) {
                        lineIdx++;
                        prevLineLength = columnIdx;
                        columnIdx = 0;
                    }
                    else {
                        columnIdx++;
                    }
                    CharIdx++;
                }
            }
            else {
                for (var _ = byLength; _ < 0; _++) {
                    CharIdx--;
                    if (c == Spec.EndOfLine) {
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